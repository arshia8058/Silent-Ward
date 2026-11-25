# ساخت صحنه‌های فصل ۱ — «سکوتِ سفید» (Chapter 1: بیداری)

**وضعیت (۲۴ نوامبر ۲۰۲۶، ساعت ۱۱:۰۷ تهران):** کد فصل ۱ کامل نوشته شده
(پایین) و در کامیت محلی `c6b3815` ثبت شده. صحنه `Boot.unity` هم ساخته شده
(فعلاً فقط یک دوربین — بدون مدیران و بدون لودر) و در Build Settings ثبت است؛
صحنه‌های `MainMenu` و `Chapter01` هنوز فایل `.unity` ندارند و فقط داخل
Unity Editor ساخته می‌شوند. نتیجه صادقانه: **اولین بیلد Cloud Build بعد از
پوش، فقط یک تست کامپایل است** (صحنه خالی با دوربین) — فصل ۱ قابل بازی نیست
تا وقتی صحنه‌های منو و فصل ۱ در ادیتور ساخته شوند. فایل‌های `.meta` هم هنوز
نیستند و در اولین بازشدن پروژه در Unity ساخته می‌شوند.

## ۱) کد آماده فصل ۱ (در مخزن، کامیت بعدی)

| فایل | کار |
|---|---|
| `Assets/Scripts/Puzzle/PuzzleBase.cs` | پایه همه معماها (حل‌شدن، تلاش اشتباه) |
| `Assets/Scripts/Puzzle/GeneratorPuzzle.cs` | معمای ژنراتور فصل ۱: ترتیب کلیدها **۲-۱-۳** (از یادداشت پرستار) + `SwitchInteractable` برای هر کلید |
| `Assets/Scripts/Dialogue/Chapter01Dialogue.cs` | همه دیالوگ‌های فصل ۱ (بیداری، مچ‌بند، فرم پذیرش با انتخاب پاسخ، یادداشت سارا، صدای لیلا، آسانسور، پایان فصل) |
| `Assets/Scripts/Chapter01/Chapter01Controller.cs` | جریان فصل: ۷ مرحله، هدف هر مرحله، ذخیره چک‌پوینت، توهم لیلا (کم‌شدن تمرکز)، بازشدن آسانسور + `LeilaTrigger` و `ElevatorInteractable` |
| `Assets/Scripts/Core/DocumentPickup.cs` | خواندن سند/برداشتن آیتم (`IInteractable`) + `CheckpointTrigger` (ذخیره خودکار) |
| `Assets/Scripts/UI/MainMenuController.cs` | منوی اصلی: بازی جدید / ادامه / تغییر زبان |
| `Assets/Scripts/UI/HUDController.cs` | نوار سلامت و تمرکز، هدف جاری، راهنمای تعامل، پنل دیالوگ با دکمه‌های انتخاب، دکمه‌های لمسی (حرکت/دویدن/پرش/خم‌شدن/تعامل) |
| `Assets/Resources/Localization/fa.json` و `en.json` | ۸ کلید قبلی + ۳۶ کلید جدید فصل ۱ (دیالوگ‌ها، هدف‌ها، راهنما) — جمعاً ۴۴ کلید در هر زبان |

## ۲) صحنه‌ها — داخل Unity Editor بسازید (به همین ترتیب)

### Boot (صحنه اول در Build Settings)
- GameObject خالی `Managers` با: `GameManager` ،`LocalizationManager` ،`AudioManager` ،`DialogueSystem` ،`InventorySystem` (هر ۵ `DontDestroyOnLoad` دارند)
- اسکریپت شروع: `GameManager` صحنه `MainMenu` را لود می‌کند (یا Boot خودش بعد از ۱ ثانیه `MainMenu` را لود کند).
- دوربین: ندارد (Boot فقط مدیران را نگه می‌دارد).

### MainMenu
- Canvas + سه دکمه با `MainMenuController`: بازی جدید، ادامه، زبان.
- ادامه (Continue) وقتی ذخیره‌ای نیست غیرفعال می‌شود (خود اسکریپت انجام می‌دهد).

### Chapter01 (بخش بستری بیمارستان مهرگان)
- زمین/دیوارها (Tilemap یا Sprite)، نور کم + چراغ اضطراری، تخت کیان (نقطه شروع بازیکن).
- `Player` با تگ **Player**: `Rigidbody2D` + `CapsuleCollider2D` + `PlayerController` + `PlayerHealth`؛ لایه تعامل (Interact Layer) روی اشیای قابل‌تعامل.
- دوربین با `CameraFollow` (هدف: Player، مرزهای بخش).
- `Chapter01Controller` با ارجاع‌ها: Player ،PlayerHealth ،DialogueSystem ،GeneratorPuzzle ،elevatorDoor ،elevatorTrigger.
- `HUD` (Canvas): `HUDController` + نوار سلامت/تمرکز + متن هدف + متن راهنما + پنل دیالوگ + دکمه‌های لمسی.
- ژنراتور: `GeneratorPuzzle` + سه کلید (`SwitchInteractable` با شماره ۱، ۲، ۳) روی لایه تعامل.
- اسناد (`DocumentPickup`): مچ‌بند (`wristband`، آیتم `wristband`)، فرم پذیرش (`admission_form`)، یادداشت پرستار (`nurse_note`) — هر سه روی لایه تعامل.
- `LeilaTrigger` (Trigger2D کنار بلندگو) و `ElevatorInteractable` (در آسانسور، اول غیرفعال).
- `CheckpointTrigger` در ورودی بخش و کنار ژنراتور.

## ۳) آیتم‌های فصل ۱ (در InventorySystem → Item Catalog)

| id | دسته | توضیح |
|---|---|---|
| `wristband` | Key | مچ‌بند کیان — بخش بستری، تخت ۷ |
| `admission_form` | Document | فرم پذیرش با امضای خودش |
| `nurse_note` | Document | یادداشت سارا: ترتیب کلیدها ۲-۱-۳ |

## ۴) ترتیب کار تا اولین بیلد

1. پروژه را در Unity Hub (نسخه **2022.3.62 LTS**) باز کنید — `.meta`‌ها و صحنه‌ها ساخته می‌شوند.
2. صحنه‌های منو و فصل ۱ را طبق §۲ بسازید و در **Build Settings** به ترتیب Boot ،MainMenu ،Chapter01 اضافه کنید (فعلاً فقط Boot ثبت است).
3. کامیت + پوش به گیت‌هاب (مخزن `Silent-Ward`) — کامیت محلی `c6b3815` (کد فصل ۱ + Boot) هنوز پوش نشده است.
4. در Unity Dashboard → Build Automation: مخزن را وصل کنید، هدف **Android**، نسخه **2022.3.62 LTS**، و اولین بیلد را بزنید. توجه: تا وقتی صحنه‌های منو و فصل ۱ ساخته نشده‌اند، بیلد فقط تست کامپایل است و چیز قابل بازی نشان نمی‌دهد.

## ۵) چیزی که هنوز نیست (عمداً برای بعد)

- فصل‌های ۲ تا ۵، مبارزه/سلاح، دشمنان فصل ۱ (فصل ۱ دشمن ندارد — فقط توهم)،
  موسیقی و صداهای محیط، آرت نهایی صحنه‌ها (فعلاً جای‌نگهدار ساده).
- `Chapter01Controller.OnChapterComplete` فعلاً فقط رویداد می‌فرستد؛
  لود فصل ۲ وقتی ساخته شد به آن وصل می‌شود.
