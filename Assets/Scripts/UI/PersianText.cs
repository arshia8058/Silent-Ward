using System.Collections.Generic;

namespace SilentWard.UI
{
    /// <summary>Persian/Arabic text shaping for TextMeshPro, which has no
    /// complex-text layout: converts a logical-order string into shaped
    /// presentation forms in visual (reversed) order, so TMP can render it
    /// left-to-right and it still reads correctly. Handles the Persian
    /// alphabet, ZWNJ (breaks joining), the lam-alef ligature, and keeps
    /// embedded Latin/digit runs in their natural order.
    /// All codepoints are written as escapes on purpose — do not replace
    /// them with literal presentation-form glyphs.</summary>
    public static class PersianText
    {
        private struct Forms
        {
            public char Iso, Fin, Ini, Med;
            public bool Dual; // connects forward to the next letter
        }

        private static Forms F(char iso, char fin, char ini, char med, bool dual) =>
            new Forms { Iso = iso, Fin = fin, Ini = ini, Med = med, Dual = dual };

        private static Forms R(char iso, char fin) => F(iso, fin, '\0', '\0', false);

        // Key: base letter. Values: isolated, final, initial, medial forms.
        private static readonly Dictionary<char, Forms> Map = new Dictionary<char, Forms>
        {
            { 'آ', R('ﺁ', 'ﺂ') },
            { 'أ', R('ﺃ', 'ﺄ') },
            { 'ؤ', R('ﺅ', 'ﺆ') },
            { 'إ', R('ﺇ', 'ﺈ') },
            { 'ئ', F('ﺉ', 'ﺊ', 'ﺋ', 'ﺌ', true) },
            { 'ا', R('ﺍ', 'ﺎ') },
            { 'ب', F('ﺏ', 'ﺐ', 'ﺑ', 'ﺒ', true) },
            { 'پ', F('ﭖ', 'ﭗ', 'ﭘ', 'ﭙ', true) },
            { 'ت', F('ﺕ', 'ﺖ', 'ﺗ', 'ﺘ', true) },
            { 'ث', F('ﺙ', 'ﺚ', 'ﺛ', 'ﺜ', true) },
            { 'ج', F('ﺝ', 'ﺞ', 'ﺟ', 'ﺠ', true) },
            { 'چ', F('ﭺ', 'ﭻ', 'ﭼ', 'ﭽ', true) },
            { 'ح', F('ﺡ', 'ﺢ', 'ﺣ', 'ﺤ', true) },
            { 'خ', F('ﺥ', 'ﺦ', 'ﺧ', 'ﺨ', true) },
            { 'د', R('ﺩ', 'ﺪ') },
            { 'ذ', R('ﺫ', 'ﺬ') },
            { 'ر', R('ﺭ', 'ﺮ') },
            { 'ز', R('ﺯ', 'ﺰ') },
            { 'ژ', R('ﮊ', 'ﮋ') },
            { 'س', F('ﺱ', 'ﺲ', 'ﺳ', 'ﺴ', true) },
            { 'ش', F('ﺵ', 'ﺶ', 'ﺷ', 'ﺸ', true) },
            { 'ص', F('ﺹ', 'ﺺ', 'ﺻ', 'ﺼ', true) },
            { 'ض', F('ﺽ', 'ﺾ', 'ﺿ', 'ﻀ', true) },
            { 'ط', F('ﻁ', 'ﻂ', 'ﻃ', 'ﻄ', true) },
            { 'ظ', F('ﻅ', 'ﻆ', 'ﻇ', 'ﻈ', true) },
            { 'ع', F('ﻉ', 'ﻊ', 'ﻋ', 'ﻌ', true) },
            { 'غ', F('ﻍ', 'ﻎ', 'ﻏ', 'ﻐ', true) },
            { 'ف', F('ﻑ', 'ﻒ', 'ﻓ', 'ﻔ', true) },
            { 'ق', F('ﻕ', 'ﻖ', 'ﻗ', 'ﻘ', true) },
            { 'ک', F('ﮎ', 'ﮏ', 'ﮐ', 'ﮑ', true) },
            { 'گ', F('ﮒ', 'ﮓ', 'ﮔ', 'ﮕ', true) },
            { 'ل', F('ﻝ', 'ﻞ', 'ﻟ', 'ﻠ', true) },
            { 'م', F('ﻡ', 'ﻢ', 'ﻣ', 'ﻤ', true) },
            { 'ن', F('ﻥ', 'ﻦ', 'ﻧ', 'ﻨ', true) },
            { 'ه', F('ﻩ', 'ﻪ', 'ﻫ', 'ﻬ', true) },
            { 'و', R('ﻭ', 'ﻮ') },
            { 'ى', R('ﻯ', 'ﻰ') },
            { 'ي', F('ﻱ', 'ﻲ', 'ﻳ', 'ﻴ', true) },
            { 'ی', F('ﯼ', 'ﯽ', 'ﯾ', 'ﯿ', true) },
            { 'ة', R('ﺓ', 'ﺔ') },
        };

        public static bool ContainsArabic(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
                if (c >= '؀' && c <= 'ۿ') return true;
            return false;
        }

        private static bool IsAlef(char c) =>
            c == 'ا' || c == 'آ' || c == 'أ' || c == 'إ';

        private static char Ligature(char alef, bool joined)
        {
            switch (alef)
            {
                case 'آ': return joined ? 'ﻶ' : 'ﻵ';
                case 'أ': return joined ? 'ﻸ' : 'ﻷ';
                case 'إ': return joined ? 'ﻺ' : 'ﻹ';
                default: return joined ? 'ﻼ' : 'ﻻ';
            }
        }

        public static string Fix(string s)
        {
            if (string.IsNullOrEmpty(s) || !ContainsArabic(s)) return s;

            var shaped = new List<char>(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                // Lam-alef ligature (consumes the alef).
                if (c == 'ل' && i + 1 < s.Length && IsAlef(s[i + 1]))
                {
                    bool ligJoined = i > 0 && Map.TryGetValue(s[i - 1], out Forms lf) && lf.Dual;
                    shaped.Add(Ligature(s[i + 1], ligJoined));
                    i++;
                    continue;
                }

                if (Map.TryGetValue(c, out Forms f))
                {
                    bool prevJoins = i > 0
                                     && Map.TryGetValue(s[i - 1], out Forms pf) && pf.Dual;
                    bool nextJoins = false;
                    if (f.Dual && i + 1 < s.Length)
                    {
                        char n = s[i + 1];
                        if (n == '‌') nextJoins = false;      // ZWNJ blocks the join
                        else if (Map.ContainsKey(n)) nextJoins = true;
                    }
                    char glyph;
                    if (prevJoins && nextJoins) glyph = f.Med != '\0' ? f.Med : f.Fin;
                    else if (prevJoins) glyph = f.Fin;
                    else if (nextJoins) glyph = f.Ini != '\0' ? f.Ini : f.Iso;
                    else glyph = f.Iso;
                    shaped.Add(glyph);
                    continue;
                }

                shaped.Add(c); // spaces, digits, punctuation, ZWNJ, Latin
            }

            // Visual order: reverse everything, then restore LTR runs
            // (Latin words, numbers, codes like 2-1-3) to natural order.
            shaped.Reverse();
            var arr = shaped.ToArray();
            int j = 0;
            while (j < arr.Length)
            {
                if (IsLtrRunChar(arr[j]))
                {
                    int k = j;
                    bool hasAlnum = false;
                    while (k < arr.Length && IsLtrRunChar(arr[k]))
                    {
                        if (IsAlnum(arr[k])) hasAlnum = true;
                        k++;
                    }
                    if (hasAlnum) System.Array.Reverse(arr, j, k - j);
                    j = k;
                }
                else j++;
            }
            return new string(arr);
        }

        private static bool IsAlnum(char c) =>
            (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
            (c >= '0' && c <= '9') ||
            (c >= '٠' && c <= '٩') || (c >= '۰' && c <= '۹');

        private static bool IsLtrRunChar(char c) =>
            IsAlnum(c) || c == '.' || c == ':' || c == '/' || c == '%' || c == '-' || c == '+';
    }
}
