-- Run explicitly with sqlcmd against the existing MovieReviews database.
-- Adds reference vocabulary; preserves custom entries, descriptions, and order.
-- Safe to rerun. This is intentionally not a startup seed or a schema migration.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @Sections TABLE (Id int IDENTITY, Name nvarchar(200), Icon nvarchar(20));
INSERT @Sections (Name, Icon) VALUES
(N'Overall impression', N'✦'),
(N'Acting / performance', N'🎭'),
(N'Camera / visuals', N'🎥'),
(N'Audio', N'🔊'),
(N'Movement / physical performance', N'↔'),
(N'Flow', N'〰'),
(N'Overall quality words', N'★'),
(N'Enthusiasm / attitude', N'💬'),
(N'Ready-to-use review phrases', N'✍');

DECLARE @Entries TABLE (Id int IDENTITY, SectionName nvarchar(200), Text nvarchar(200), Description nvarchar(max));
INSERT @Entries (SectionName, Text, Description) VALUES
(N'Overall impression', N'natural', N'طبیعی؛ feels believable rather than staged.'),
(N'Overall impression', N'staged', N'ساختگی / صحنه‌سازی‌شده؛ clearly feels performed.'),
(N'Overall impression', N'authentic', N'باورپذیر و اصیل؛ feels genuine.'),
(N'Overall impression', N'forced', N'زورکی / تصنعی؛ does not feel spontaneous.'),
(N'Overall impression', N'engaging', N'جذاب؛ holds your attention.'),
(N'Overall impression', N'memorable', N'به‌یادماندنی؛ stays with you afterward.'),
(N'Overall impression', N'repetitive', N'تکراری؛ repeats the same ideas or actions.'),
(N'Overall impression', N'smooth', N'روان؛ everything flows without awkward transitions.'),
(N'Overall impression', N'awkward', N'معذب‌کننده / ناجور؛ feels uncomfortable or unnatural.'),
(N'Overall impression', N'cozy', N'صمیمی و راحت؛ a relaxed, comfortable atmosphere.'),
(N'Overall impression', N'passionate', N'پرشور؛ shows strong feeling and enthusiasm.'),
(N'Overall impression', N'immersive', N'غرق‌کننده؛ draws you into the scene.'),
(N'Overall impression', N'formulaic', N'کلیشه‌ای و قالبی؛ follows a predictable formula.'),
(N'Overall impression', N'contrived', N'بیش‌ازحد ساختگی؛ events seem arranged just to force an outcome.'),
(N'Overall impression', N'understated', N'ظریف و کم‌اغراق؛ achieves its effect without showiness.'),
(N'Overall impression', N'flat', N'بی‌روح؛ lacks energy, expression, or impact.'),
(N'Overall impression', N'atmospheric', N'فضاساز؛ creates a strong mood.'),
(N'Acting / performance', N'convincing', N'باورپذیر؛ makes the role or reaction feel real.'),
(N'Acting / performance', N'natural', N'طبیعی؛ delivery does not feel rehearsed or artificial.'),
(N'Acting / performance', N'expressive', N'پر‌احساس و گویا؛ clearly communicates feelings through face, voice, or movement.'),
(N'Acting / performance', N'playful', N'شوخ و بازیگوش؛ lighthearted rather than serious.'),
(N'Acting / performance', N'confident', N'بااعتمادبه‌نفس؛ looks assured and comfortable.'),
(N'Acting / performance', N'energetic', N'پرانرژی؛ lively and active.'),
(N'Acting / performance', N'enthusiastic', N'مشتاق؛ shows interest and excitement.'),
(N'Acting / performance', N'awkward', N'ناجور / معذب؛ delivery or interaction feels uncomfortable.'),
(N'Acting / performance', N'stiff', N'خشک؛ lacks relaxed movement or expression.'),
(N'Acting / performance', N'forced', N'تصنعی / زورکی؛ reactions seem pushed rather than natural.'),
(N'Acting / performance', N'overacted', N'اغراق‌آمیز بازی‌شده؛ reactions are bigger than the scene needs.'),
(N'Acting / performance', N'understated', N'کم‌اغراق؛ uses subtle reactions.'),
(N'Acting / performance', N'good chemistry', N'هماهنگی و کشش خوب؛ the performers connect naturally.'),
(N'Acting / performance', N'weak chemistry', N'ارتباط ضعیف؛ the interaction lacks a convincing connection.'),
(N'Acting / performance', N'wooden', N'بسیار خشک و بی‌احساس؛ little variation in expression or delivery.'),
(N'Acting / performance', N'believable reactions', N'واکنش‌های باورپذیر؛ responses fit what is happening.'),
(N'Acting / performance', N'forced dialogue', N'دیالوگ تصنعی؛ the conversation sounds unnatural.'),
(N'Acting / performance', N'poor delivery', N'بیان ضعیف؛ lines are spoken without the right timing or feeling.'),
(N'Acting / performance', N'subtle expressions', N'حالت‌های ظریف چهره؛ small expressions convey the feeling.'),
(N'Acting / performance', N'effortless chemistry', N'هماهنگی بی‌تکلف؛ the connection feels easy and natural.'),
(N'Camera / visuals', N'good framing', N'کادربندی خوب؛ subjects are placed effectively within the frame.'),
(N'Camera / visuals', N'clean shots', N'نماهای تمیز؛ clear images without distracting clutter.'),
(N'Camera / visuals', N'good angles', N'زاویه‌های خوب؛ camera positions show the action clearly.'),
(N'Camera / visuals', N'great close-ups', N'نماهای نزدیک عالی؛ close shots capture useful detail and expression.'),
(N'Camera / visuals', N'good composition', N'ترکیب‌بندی خوب؛ the visual elements are arranged well.'),
(N'Camera / visuals', N'well-lit', N'نورپردازی خوب؛ subjects and details are easy to see.'),
(N'Camera / visuals', N'dark / poorly lit', N'تاریک / نورپردازی ضعیف؛ important detail is hard to see.'),
(N'Camera / visuals', N'sharp', N'واضح؛ details are in focus.'),
(N'Camera / visuals', N'blurry', N'تار؛ details are out of focus or unclear.'),
(N'Camera / visuals', N'stable', N'باثبات؛ the camera stays steady.'),
(N'Camera / visuals', N'shaky', N'لرزان؛ unwanted camera movement distracts.'),
(N'Camera / visuals', N'immersive', N'غرق‌کننده؛ camera work helps you feel present.'),
(N'Camera / visuals', N'visually appealing', N'از نظر بصری جذاب؛ pleasing to look at.'),
(N'Camera / visuals', N'well-shot', N'خوب فیلم‌برداری‌شده؛ effective camera work overall.'),
(N'Camera / visuals', N'cinematic', N'سینمایی؛ visually crafted like a film.'),
(N'Camera / visuals', N'obstructed view', N'دید مسدود؛ something blocks an important part of the frame.'),
(N'Camera / visuals', N'awkward framing', N'کادربندی نامناسب؛ the frame cuts off or hides useful detail.'),
(N'Camera / visuals', N'distracting cuts', N'برش‌های مزاحم؛ editing interrupts attention or continuity.'),
(N'Camera / visuals', N'overexposed', N'بیش‌ازحد روشن؛ bright areas lose detail.'),
(N'Camera / visuals', N'washed-out colors', N'رنگ‌های بی‌جان؛ colors look pale and lack depth.'),
(N'Audio', N'clear', N'واضح؛ voices and sounds are easy to hear.'),
(N'Audio', N'clean', N'تمیز؛ little unwanted noise or distortion.'),
(N'Audio', N'immersive', N'غرق‌کننده؛ sound creates a sense of presence.'),
(N'Audio', N'balanced', N'متعادل؛ voices, music, and other sounds have suitable levels.'),
(N'Audio', N'muffled', N'خفه؛ sound lacks clarity, as if something covers the microphone.'),
(N'Audio', N'distant', N'دور؛ sound seems recorded from too far away.'),
(N'Audio', N'loud', N'بلند؛ high volume, not necessarily high energy or enthusiasm.'),
(N'Audio', N'quiet', N'آرام / کم‌صدا؛ low volume, not necessarily a lack of enthusiasm.'),
(N'Audio', N'natural-sounding', N'طبیعی به گوش می‌رسد؛ sound does not feel artificial.'),
(N'Audio', N'distracting', N'حواس‌پرت‌کن؛ sound draws attention away from the scene.'),
(N'Audio', N'background noise', N'صدای پس‌زمینه؛ unwanted ambient sound.'),
(N'Audio', N'clipped audio', N'صدای دچار اعوجاج؛ peaks distort because the recording level is too high.'),
(N'Audio', N'out-of-sync audio', N'صدای ناهماهنگ با تصویر؛ sound and visible action do not line up.'),
(N'Audio', N'overpowering music', N'موسیقی بیش‌ازحد بلند؛ music overwhelms voices or other sound.'),
(N'Movement / physical performance', N'smooth', N'روان؛ movement has no awkward breaks.'),
(N'Movement / physical performance', N'fluid', N'سیال؛ movements connect naturally.'),
(N'Movement / physical performance', N'natural', N'طبیعی؛ movement feels unforced.'),
(N'Movement / physical performance', N'rhythmic', N'ریتمیک؛ follows a consistent rhythm.'),
(N'Movement / physical performance', N'energetic', N'پرانرژی؛ lively movement.'),
(N'Movement / physical performance', N'controlled', N'کنترل‌شده؛ movement is deliberate and precise.'),
(N'Movement / physical performance', N'expressive', N'گویا؛ movement conveys feeling.'),
(N'Movement / physical performance', N'responsive', N'واکنش‌پذیر؛ responds naturally to the other performer.'),
(N'Movement / physical performance', N'synchronized', N'هماهنگ؛ movements line up well.'),
(N'Movement / physical performance', N'awkward', N'ناهماهنگ / ناجور؛ movement looks uncomfortable.'),
(N'Movement / physical performance', N'stiff', N'خشک؛ little ease or flexibility.'),
(N'Movement / physical performance', N'repetitive', N'تکراری؛ little variation in movement.'),
(N'Movement / physical performance', N'mechanical', N'ماشینی؛ repetitive and lacking natural expression.'),
(N'Movement / physical performance', N'hesitant', N'مردد؛ pauses or holds back before acting.'),
(N'Movement / physical performance', N'well-coordinated', N'خوب هماهنگ‌شده؛ timing and movement work well together.'),
(N'Flow', N'good flow', N'روند روان؛ the scene moves naturally from one part to the next.'),
(N'Flow', N'smooth progression', N'پیشروی روان؛ transitions feel natural.'),
(N'Flow', N'gradual build-up', N'اوج‌گیری تدریجی؛ interest or intensity develops step by step.'),
(N'Flow', N'good variation', N'تنوع خوب؛ enough change to keep it interesting.'),
(N'Flow', N'well-paced', N'خوش‌ریتم؛ neither too slow nor too rushed.'),
(N'Flow', N'uneven', N'نامتوازن؛ some parts work better or move faster than others.'),
(N'Flow', N'repetitive', N'تکراری؛ the structure repeats without enough development.'),
(N'Flow', N'dragged', N'کش‌دار بود؛ more naturally write: The scene dragged / It felt drawn-out.'),
(N'Flow', N'rushed', N'شتاب‌زده؛ moves too quickly to develop naturally.'),
(N'Flow', N'drawn-out', N'کش‌دار؛ lasts longer than it needs to.'),
(N'Flow', N'abrupt transition', N'تغییر ناگهانی؛ moves between parts without enough connection.'),
(N'Flow', N'loses momentum', N'از ریتم می‌افتد؛ energy or interest drops as it goes on.'),
(N'Flow', N'well-earned payoff', N'نتیجه‌گیری رضایت‌بخش؛ the ending rewards the build-up.'),
(N'Flow', N'anticlimactic', N'پایان کم‌اثر؛ the outcome feels weaker than the build-up promised.'),
(N'Flow', N'predictable', N'قابل‌پیش‌بینی؛ easy to guess what happens next.'),
(N'Overall quality words', N'excellent', N'عالی؛ very high quality.'),
(N'Overall quality words', N'great', N'خیلی خوب؛ strongly positive.'),
(N'Overall quality words', N'very good', N'بسیار خوب؛ clearly above average.'),
(N'Overall quality words', N'enjoyable', N'لذت‌بخش؛ pleasant to watch.'),
(N'Overall quality words', N'impressive', N'چشمگیر؛ stands out through quality or skill.'),
(N'Overall quality words', N'memorable', N'به‌یادماندنی؛ leaves a lasting impression.'),
(N'Overall quality words', N'satisfying', N'رضایت‌بخش؛ meets expectations well.'),
(N'Overall quality words', N'engaging', N'جذاب؛ holds attention.'),
(N'Overall quality words', N'polished', N'پرداخت‌شده و حرفه‌ای؛ carefully finished.'),
(N'Overall quality words', N'authentic', N'اصیل و باورپذیر؛ feels genuine.'),
(N'Overall quality words', N'decent', N'قابل‌قبول؛ reasonably good without being exceptional.'),
(N'Overall quality words', N'average', N'متوسط؛ around the usual standard.'),
(N'Overall quality words', N'okay', N'بد نیست / قابل‌قبول؛ mildly positive or neutral.'),
(N'Overall quality words', N'ordinary', N'معمولی؛ lacks anything unusual.'),
(N'Overall quality words', N'standard', N'معمول / مطابق انتظار؛ typical for its kind.'),
(N'Overall quality words', N'nothing special', N'چیز خاصی ندارد؛ acceptable but unremarkable.'),
(N'Overall quality words', N'weak', N'ضعیف؛ lacks impact or quality.'),
(N'Overall quality words', N'bland', N'بی‌مزه / بی‌روح؛ little character or interest.'),
(N'Overall quality words', N'repetitive', N'تکراری؛ too little variety.'),
(N'Overall quality words', N'awkward', N'ناجور؛ uncomfortable or poorly coordinated.'),
(N'Overall quality words', N'forced', N'تصنعی / زورکی؛ lacks naturalness.'),
(N'Overall quality words', N'boring', N'خسته‌کننده؛ fails to hold attention.'),
(N'Overall quality words', N'underwhelming', N'پایین‌تر از انتظار؛ less impressive than hoped.'),
(N'Overall quality words', N'poorly executed', N'ضعیف اجراشده؛ the idea may work, but the execution does not.'),
(N'Overall quality words', N'standout', N'برجسته؛ noticeably better or more memorable than the rest.'),
(N'Overall quality words', N'hit-or-miss', N'یک‌درمیان خوب و بد؛ quality is inconsistent.'),
(N'Overall quality words', N'forgettable', N'فراموش‌شدنی؛ leaves little lasting impression.'),
(N'Overall quality words', N'rough around the edges', N'هنوز جای پرداخت دارد؛ promising but not fully polished.'),
(N'Enthusiasm / attitude', N'reluctantly / with reluctance', N'با اکراه؛ seems hesitant or unwilling. Example: The delivery comes across as reluctant. Describes an impression, not proof of someone''s feelings.'),
(N'Enthusiasm / attitude', N'half-heartedly', N'بی‌اشتیاق / بدون دل‌ودماغ؛ little effort or enthusiasm. Example: The lines are delivered half-heartedly.'),
(N'Enthusiasm / attitude', N'going through the motions', N'فقط رفع تکلیف کردن؛ follows the actions without visible enthusiasm. Example: The performance feels like going through the motions.'),
(N'Enthusiasm / attitude', N'perfunctory', N'سرسری و از سر رفع تکلیف؛ done with minimal care or interest. Example: The reactions feel perfunctory.'),
(N'Enthusiasm / attitude', N'unenthusiastic', N'بی‌اشتیاق؛ shows little excitement. Example: The delivery seems unenthusiastic. Does not necessarily mean unwilling.'),
(N'Enthusiasm / attitude', N'lackluster', N'کم‌فروغ / بی‌رمق؛ lacks energy or impact. Example: A lackluster performance.'),
(N'Enthusiasm / attitude', N'disengaged', N'بی‌توجه / بی‌درگیرشدن؛ seems mentally distant. Example: The performer comes across as disengaged.'),
(N'Enthusiasm / attitude', N'detached', N'از نظر احساسی فاصله‌گرفته؛ little visible emotional involvement. Example: The delivery feels detached.'),
(N'Enthusiasm / attitude', N'apathetic', N'بی‌تفاوت؛ shows little interest or feeling. Stronger than merely quiet or reserved.'),
(N'Enthusiasm / attitude', N'reserved', N'خوددار / کم‌ابراز؛ restrained in expression, without necessarily lacking interest.'),
(N'Enthusiasm / attitude', N'wholeheartedly', N'از صمیم دل / با تمام وجود؛ with full commitment and enthusiasm.'),
(N'Enthusiasm / attitude', N'committed performance', N'اجرای با تعهد و جدیت؛ the performer appears fully invested in the role.'),
(N'Ready-to-use review phrases', N'The performance feels half-hearted.', N'اجرا بی‌اشتیاق به نظر می‌رسد؛ a direct way to describe low enthusiasm.'),
(N'Ready-to-use review phrases', N'The delivery comes across as reluctant.', N'نحوهٔ بیان با اکراه به نظر می‌رسد؛ focuses on the impression conveyed.'),
(N'Ready-to-use review phrases', N'They seem to be going through the motions.', N'به نظر می‌رسد فقط دارند رفع تکلیف می‌کنند.'),
(N'Ready-to-use review phrases', N'There is little visible enthusiasm.', N'اشتیاق چندانی در اجرا دیده نمی‌شود؛ describes what is visible.'),
(N'Ready-to-use review phrases', N'Natural chemistry, but uneven pacing.', N'هماهنگی طبیعی، اما ریتم نامتوازن؛ concise mixed verdict.'),
(N'Ready-to-use review phrases', N'A strong start, but it loses momentum.', N'شروع قوی، اما در ادامه از ریتم می‌افتد.'),
(N'Ready-to-use review phrases', N'Good framing, let down by poor lighting.', N'کادربندی خوب که نورپردازی ضعیف به آن لطمه زده است.'),
(N'Ready-to-use review phrases', N'The reactions feel genuine rather than exaggerated.', N'واکنش‌ها طبیعی و باورپذیرند، نه اغراق‌آمیز.'),
(N'Ready-to-use review phrases', N'Well-paced, with a satisfying build-up.', N'خوش‌ریتم، با اوج‌گیری رضایت‌بخش.'),
(N'Ready-to-use review phrases', N'Visually polished, but emotionally flat.', N'از نظر بصری پرداخت‌شده، اما از نظر احساسی بی‌روح.');

DECLARE @SectionOrder int = COALESCE((SELECT MAX(SortOrder) FROM GuideSections), 0);
INSERT GuideSections (Name, Icon, SortOrder)
SELECT s.Name, s.Icon, @SectionOrder + ROW_NUMBER() OVER (ORDER BY s.Id)
FROM @Sections s
WHERE NOT EXISTS (SELECT 1 FROM GuideSections existing WITH (UPDLOCK, HOLDLOCK) WHERE existing.Name = s.Name);
DECLARE @AddedSections int = @@ROWCOUNT;

INSERT GuideItems (GuideSectionId, Text, Description, SortOrder)
SELECT s.Id, e.Text, e.Description,
    COALESCE((SELECT MAX(i.SortOrder) FROM GuideItems i WHERE i.GuideSectionId = s.Id), 0)
    + ROW_NUMBER() OVER (PARTITION BY s.Id ORDER BY e.Id)
FROM @Entries e JOIN GuideSections s ON s.Name = e.SectionName
WHERE NOT EXISTS (
    SELECT 1 FROM GuideItems existing WITH (UPDLOCK, HOLDLOCK)
    WHERE existing.GuideSectionId = s.Id AND LOWER(LTRIM(RTRIM(existing.Text))) = LOWER(e.Text));
DECLARE @AddedItems int = @@ROWCOUNT;

UPDATE i SET Description = e.Description
FROM GuideItems i JOIN GuideSections s ON s.Id = i.GuideSectionId
JOIN @Entries e ON e.SectionName = s.Name AND LOWER(e.Text) = LOWER(LTRIM(RTRIM(i.Text)))
WHERE NULLIF(LTRIM(RTRIM(i.Description)), N'') IS NULL;
DECLARE @ExplainedItems int = @@ROWCOUNT;

COMMIT;
SELECT @AddedSections AS AddedSections, @AddedItems AS AddedItems, @ExplainedItems AS ExplainedExistingItems;
