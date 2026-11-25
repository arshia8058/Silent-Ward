using UnityEngine;

namespace SilentWard.Audio
{
    /// <summary>Procedurally generated placeholder audio. Every clip is
    /// synthesized in code at runtime, so the project needs no binary audio
    /// assets: a sad piano loop, a hospital ambience loop and one-shot SFX.
    /// Real recorded audio can later replace these clips without changing
    /// any call site (everything plays through AudioManager).</summary>
    public static class ProceduralAudio
    {
        private const int Rate = 44100;
        private static readonly System.Random Rng = new System.Random(20261125);

        private static AudioClip music, ambient, click, switchClick, powerOn,
            pickup, door, elevatorDing, speakerStatic, step;

        public static AudioClip Music => music ??= RenderMusic();
        public static AudioClip Ambient => ambient ??= RenderAmbient();
        public static AudioClip Click => click ??= RenderClick();
        public static AudioClip SwitchClick => switchClick ??= RenderSwitch();
        public static AudioClip PowerOn => powerOn ??= RenderPowerOn();
        public static AudioClip Pickup => pickup ??= RenderPickup();
        public static AudioClip Door => door ??= RenderDoor();
        public static AudioClip ElevatorDing => elevatorDing ??= RenderDing();
        public static AudioClip SpeakerStatic => speakerStatic ??= RenderStatic();
        public static AudioClip Step => step ??= RenderStep();

        private static float Noise() => (float)(Rng.NextDouble() * 2.0 - 1.0);
        private static float Midi(int m) => 440f * Mathf.Pow(2f, (m - 69) / 12f);

        private static AudioClip Make(string name, float[] data)
        {
            var clip = AudioClip.Create(name, data.Length, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static void Normalize(float[] buf, float peak)
        {
            float max = 0f;
            for (int i = 0; i < buf.Length; i++) max = Mathf.Max(max, Mathf.Abs(buf[i]));
            if (max < 0.0001f) return;
            float scale = peak / max;
            for (int i = 0; i < buf.Length; i++) buf[i] *= scale;
        }

        /// <summary>One piano-ish note: harmonic stack with a fast attack and
        /// an exponential ring. Writes wrap around the buffer end so the
        /// music loop has no seam.</summary>
        private static void AddNote(float[] buf, int midi, float startSec, float ringSec, float gain, float decay)
        {
            float f = Midi(midi);
            int start = (int)(startSec * Rate);
            int len = (int)(ringSec * Rate);
            for (int i = 0; i < len; i++)
            {
                float t = i / (float)Rate;
                float env = (1f - Mathf.Exp(-t * 45f)) * Mathf.Exp(-t * decay);
                if (env < 0.001f) break;
                float s = 0f;
                for (int h = 1; h <= 5; h++)
                {
                    float hf = f * h * (1f + 0.0003f * h * h);
                    s += Mathf.Sin(2f * Mathf.PI * hf * t) * Mathf.Pow(0.52f, h - 1) * Mathf.Exp(-t * decay * h * 0.7f);
                }
                int idx = (start + i) % buf.Length;
                buf[idx] += s * env * gain;
            }
        }

        private static AudioClip RenderMusic()
        {
            const float bar = 4.5f;
            const int bars = 8;
            var buf = new float[(int)(bar * bars * Rate)];
            // D minor: Dm, Bb, F, A — twice.
            int[][] chords =
            {
                new[] { 38, 45, 53 }, new[] { 34, 41, 50 },
                new[] { 41, 45, 48 }, new[] { 33, 40, 49 },
                new[] { 38, 45, 53 }, new[] { 34, 41, 50 },
                new[] { 41, 45, 48 }, new[] { 33, 40, 49 },
            };
            // Sparse sad melody: (midi, offset in bar, gain) per bar.
            float[][][] melody =
            {
                new[] { new[] { 65f, 0.2f, 0.70f }, new[] { 69f, 1.6f, 0.60f }, new[] { 74f, 3.1f, 0.75f } },
                new[] { new[] { 72f, 0.4f, 0.60f }, new[] { 74f, 2.0f, 0.65f }, new[] { 70f, 3.4f, 0.50f } },
                new[] { new[] { 69f, 0.2f, 0.60f }, new[] { 72f, 1.7f, 0.70f }, new[] { 69f, 3.2f, 0.50f } },
                new[] { new[] { 64f, 0.4f, 0.60f }, new[] { 61f, 2.1f, 0.65f }, new[] { 62f, 3.5f, 0.55f } },
                new[] { new[] { 74f, 0.2f, 0.70f }, new[] { 72f, 1.6f, 0.55f }, new[] { 69f, 3.0f, 0.60f } },
                new[] { new[] { 70f, 0.4f, 0.55f }, new[] { 69f, 2.0f, 0.60f }, new[] { 65f, 3.4f, 0.50f } },
                new[] { new[] { 72f, 0.2f, 0.60f }, new[] { 74f, 1.8f, 0.65f }, new[] { 77f, 3.2f, 0.60f } },
                new[] { new[] { 69f, 0.4f, 0.60f }, new[] { 64f, 2.2f, 0.55f }, new[] { 62f, 3.6f, 0.60f } },
            };
            for (int b = 0; b < bars; b++)
            {
                float t0 = b * bar;
                foreach (int tone in chords[b]) AddNote(buf, tone, t0, bar, 0.15f, 0.85f);
                AddNote(buf, chords[b][0] - 12, t0, bar, 0.11f, 0.8f);
                foreach (var n in melody[b])
                    AddNote(buf, (int)n[0], t0 + n[1], 3.4f, 0.30f * n[2] + 0.12f, 1.05f);
            }
            // Cheap space: two soft echo taps.
            var wet = (float[])buf.Clone();
            int tap1 = (int)(0.27f * Rate), tap2 = (int)(0.54f * Rate);
            for (int i = 0; i < buf.Length; i++)
            {
                float e = buf[(i - tap1 + buf.Length) % buf.Length] * 0.20f
                        + buf[(i - tap2 + buf.Length) % buf.Length] * 0.12f;
                wet[i] += e;
            }
            Normalize(wet, 0.55f);
            return Make("ProceduralPiano", wet);
        }

        private static AudioClip RenderAmbient()
        {
            const float seconds = 30f;
            var buf = new float[(int)(seconds * Rate)];
            float lp = 0f;
            float[] dripAt = { 4.3f, 13.7f, 22.9f };
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                lp += (Noise() - lp) * 0.035f; // low rumble bed
                float lfo = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 0.07f * t);
                float s = lp * 2.6f * lfo;
                s += Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.012f;   // mains hum
                s += Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.006f;
                buf[i] = s;
            }
            foreach (float dt in dripAt) // distant water drips
            {
                AddDrip(buf, dt, 0.16f);
                AddDrip(buf, dt + 0.33f, 0.07f);
            }
            Normalize(buf, 0.42f);
            return Make("ProceduralAmbience", buf);
        }

        private static void AddDrip(float[] buf, float startSec, float gain)
        {
            int start = (int)(startSec * Rate);
            int len = (int)(0.09f * Rate);
            for (int i = 0; i < len && start + i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                float f = Mathf.Lerp(1500f, 620f, t / 0.09f);
                buf[start + i] += Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-t * 55f) * gain;
            }
        }

        private static AudioClip RenderClick()
        {
            var buf = new float[(int)(0.07f * Rate)];
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                buf[i] = (Mathf.Sin(2f * Mathf.PI * 1350f * t) * 0.6f + Noise() * 0.25f)
                         * Mathf.Exp(-t * 70f);
            }
            return Make("ProceduralClick", buf);
        }

        private static AudioClip RenderSwitch()
        {
            var buf = new float[(int)(0.14f * Rate)];
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                float tick = 0f;
                if (t < 0.025f) tick += Noise() * Mathf.Exp(-t * 160f);
                float t2 = t - 0.06f;
                if (t2 > 0f && t2 < 0.03f) tick += Noise() * Mathf.Exp(-t2 * 140f) * 0.9f;
                buf[i] = tick * 0.8f + Mathf.Sin(2f * Mathf.PI * 290f * t) * Mathf.Exp(-t * 60f) * 0.25f;
            }
            return Make("ProceduralSwitch", buf);
        }

        private static AudioClip RenderPowerOn()
        {
            var buf = new float[(int)(1.4f * Rate)];
            float lp = 0f;
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                float s = 0f;
                if (t < 0.03f) s += Noise() * Mathf.Exp(-t * 150f) * 0.7f; // relay clack
                float ramp = Mathf.Clamp01((t - 0.05f) / 0.8f);
                float f = Mathf.Lerp(44f, 50f, ramp);
                s += (Mathf.Sin(2f * Mathf.PI * f * t) + 0.6f * Mathf.Sin(2f * Mathf.PI * 2f * f * t))
                     * 0.16f * ramp;
                lp += (Noise() - lp) * 0.01f;
                s += lp * 0.4f * ramp;
                buf[i] = s * (1f - Mathf.Clamp01((t - 1.25f) / 0.15f) * 0.35f);
            }
            Normalize(buf, 0.6f);
            return Make("ProceduralPowerOn", buf);
        }

        private static AudioClip RenderPickup()
        {
            var buf = new float[(int)(0.3f * Rate)];
            float lp = 0f;
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                float cut = Mathf.Lerp(0.02f, 0.22f, t / 0.3f); // paper swish sweep
                lp += (Noise() - lp) * cut;
                float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 0.3f));
                buf[i] = lp * 2.2f * env * env;
            }
            Normalize(buf, 0.5f);
            return Make("ProceduralPickup", buf);
        }

        private static AudioClip RenderDoor()
        {
            var buf = new float[(int)(0.95f * Rate)];
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                float s = 0f;
                if (t < 0.72f) // slow creak
                {
                    float f = Mathf.Lerp(135f, 82f, t / 0.72f) + Mathf.Sin(t * 31f) * 4f;
                    float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(t / 0.72f));
                    s += (Mathf.Sin(2f * Mathf.PI * f * t)
                          + 0.5f * Mathf.Sin(4f * Mathf.PI * f * t)
                          + 0.22f * Mathf.Sin(6f * Mathf.PI * f * t)) * 0.16f * env;
                }
                float tt = t - 0.74f; // closing thud
                if (tt > 0f) s += Mathf.Sin(2f * Mathf.PI * 64f * tt) * Mathf.Exp(-tt * 26f) * 0.55f;
                buf[i] = s;
            }
            Normalize(buf, 0.6f);
            return Make("ProceduralDoor", buf);
        }

        private static AudioClip RenderDing()
        {
            var buf = new float[(int)(1.1f * Rate)];
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                buf[i] = (Mathf.Sin(2f * Mathf.PI * 988f * t)
                          + 0.4f * Mathf.Sin(2f * Mathf.PI * 1976f * t))
                         * Mathf.Exp(-t * 3.4f) * 0.5f;
            }
            return Make("ProceduralElevatorDing", buf);
        }

        private static AudioClip RenderStatic()
        {
            var buf = new float[(int)(0.9f * Rate)];
            float gate = 0f;
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                if (i % 220 == 0) gate = Rng.NextDouble() > 0.35 ? 1f : 0.12f; // crackle blocks
                float env = (1f - t / 0.9f) * 0.8f + 0.2f;
                buf[i] = Noise() * gate * env * 0.34f
                         + Mathf.Sin(2f * Mathf.PI * 3050f * t) * 0.03f * env;
            }
            return Make("ProceduralSpeakerStatic", buf);
        }

        private static AudioClip RenderStep()
        {
            var buf = new float[(int)(0.09f * Rate)];
            float lp = 0f;
            for (int i = 0; i < buf.Length; i++)
            {
                float t = i / (float)Rate;
                lp += (Noise() - lp) * 0.25f;
                float env = Mathf.Exp(-t * 42f);
                buf[i] = lp * 1.6f * env + Mathf.Sin(2f * Mathf.PI * 74f * t) * env * 0.35f;
            }
            return Make("ProceduralStep", buf);
        }
    }
}
