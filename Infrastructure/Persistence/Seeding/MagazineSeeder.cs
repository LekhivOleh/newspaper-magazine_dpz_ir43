using CURSOVA.Domain.Articles.Building;
using CURSOVA.Domain.Authors;
using CURSOVA.Domain.Categories;
using CURSOVA.Domain.Issues;
using Microsoft.EntityFrameworkCore;

namespace CURSOVA.Infrastructure.Persistence.Seeding;

public static class MagazineSeeder
{
    public static async Task InitializeAsync(MagazineDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);
        await SeedCatalogAsync(db, ct);
        await SeedIssuesAsync(db, ct);
    }

    private static async Task SeedCatalogAsync(MagazineDbContext db, CancellationToken ct)
    {
        if (await db.Categories.AnyAsync(ct))
            return;

        var reviews = new Category("Огляди", "Тестуємо гаджети, ноутбуки та аксесуари.", 1);
        var analytics = new Category("Аналітика", "Тренди ринку та технологій без хайпу.", 2);
        var ai = new Category("Штучний інтелект", "Нейромережі, моделі та їхній вплив на життя.", 3);
        var interviews = new Category("Інтерв'ю", "Розмови з людьми, які творять технології.", 4);
        var tips = new Category("Поради", "Практичні гайди на кожен день.", 5);
        var gaming = new Category("Геймінг", "Ігри, залізо та індустрія.", 6);
        db.Categories.AddRange(reviews, analytics, ai, interviews, tips, gaming);

        var olena = new Author("Олена Коваль", "Головна редакторка", "Пише про технології понад десять років.", Img("olena", 200, 200));
        var andrii = new Author("Андрій Мельник", "Оглядач гаджетів", "Тестує все, що має батарею.", Img("andrii", 200, 200));
        var iryna = new Author("Ірина Бойко", "AI-аналітикиня", "Досліджує машинне навчання та етику AI.", Img("iryna", 200, 200));
        db.Authors.AddRange(olena, andrii, iryna);

        await db.SaveChangesAsync(ct);

        db.Articles.AddRange(
            Article("Як обрати ноутбук для навчання у 2026 році",
                    "Процесор, екран, автономність і вага: розбираємо, на що звертати увагу студенту і за що не варто переплачувати.",
                    tips, andrii, "laptop")
                .WithContent(c => c
                    .AddText("Перше правило — визначитися зі сценаріями. Для конспектів, браузера та офісних програм не потрібна топова відеокарта, а от для дизайну чи програмування варто подумати про більший обсяг оперативної пам'яті.", "Почніть із задач")
                    .AddImage(Img("laptop-desk"), "Робоче місце студента")
                    .AddText("16 ГБ оперативної пам'яті сьогодні — комфортний мінімум. SSD обов'язковий, а від 512 ГБ ви не будете постійно чистити диск.", "Пам'ять і накопичувач")
                    .AddText("Автономність понад 10 годин означає, що зарядку можна залишати вдома. Вага до 1,5 кг відчутно полегшує щоденні поїздки.", "Автономність і вага"))
                .Build(),

            Article("Локальні AI-моделі: чи варто запускати нейромережу вдома",
                    "Відкриті мовні моделі стали достатньо компактними для домашнього ПК. Пояснюємо, що для цього потрібно і які є компроміси.",
                    ai, iryna, "ai-local")
                .Premium()
                .WithContent(c => c
                    .AddText("Ще кілька років тому запуск мовної моделі вимагав серверної стійки. Сьогодні квантизовані моделі працюють на звичайній відеокарті або навіть на ноутбуці.", "Що змінилося")
                    .AddGallery(g => g
                        .Add(Img("gpu"), "Відеокарта — головний ресурс")
                        .Add(Img("terminal"), "Запуск моделі з терміналу")
                        .Add(Img("server-room"), "Для порівняння: серверна інфраструктура"), "Від хмари до домашнього ПК")
                    .AddText("Головні переваги — приватність і робота без інтернету. Недоліки — менша якість порівняно з найбільшими хмарними моделями та навантаження на залізо.", "Плюси та мінуси"))
                .Build(),

            Article("Ринок смарт-годинників: що буде далі",
                    "Продажі стабілізувалися, а виробники шукають нові причини оновлюватися. Аналізуємо головні тренди.",
                    analytics, olena, "smartwatch")
                .WithContent(c => c
                    .AddText("Після кількох років швидкого зростання ринок смарт-годинників увійшов у фазу зрілості. Користувачі оновлюють пристрої рідше, тож виробники роблять ставку на здоров'я та автономність.", "Фаза зрілості")
                    .AddImage(Img("watch-wrist"), "Здоров'я — головний аргумент продажів")
                    .AddText("Наступна хвиля — неінвазивні сенсори та інтеграція з медичними сервісами. Але регуляторні вимоги суттєво уповільнюють їхню появу.", "Що далі"))
                .Build(),

            Article("Інтерв'ю: як інді-студія з Києва робить ігри для всього світу",
                    "Команда з восьми людей розповіла про роботу під час війни, перший реліз у Steam і про те, чому вони не женуться за трендами.",
                    interviews, olena, "gamedev")
                .WithContent(c => c
                    .AddText("— Ми починали вчотирьох у звичайній квартирі. Перший прототип зробили за два місяці, і він був жахливий, але в нього хотілося грати.", "Як усе почалося")
                    .AddImage(Img("studio"), "Робочий простір студії")
                    .AddText("— Найважче — планувати, коли не знаєш, чи буде завтра світло. Ми навчилися працювати асинхронно і тримати все в хмарі.", "Робота в умовах війни"))
                .Build(),

            Article("5 кроків, щоб захистити свої акаунти вже сьогодні",
                    "Двофакторна автентифікація, менеджер паролів і ще три прості звички, які зупиняють більшість атак.",
                    tips, andrii, "security")
                .WithContent(c => c
                    .AddText("Увімкніть двофакторну автентифікацію всюди, де це можливо. Застосунок-автентифікатор надійніший за SMS.", "1. Двофакторна автентифікація")
                    .AddText("Менеджер паролів дозволяє мати унікальний складний пароль для кожного сервісу і не запам'ятовувати жодного з них.", "2. Менеджер паролів")
                    .AddText("Оновлюйте систему та застосунки, перевіряйте адресу сайту перед введенням пароля, а для важливих акаунтів використовуйте апаратні ключі.", "3–5. Прості звички"))
                .Build(),

            Article("Огляд бездротових навушників середнього класу",
                    "Шумозаглушення, кодеки та зручність: порівняли кілька моделей до 5000 гривень і з'ясували, чи потрібно переплачувати за флагман.",
                    reviews, andrii, "headphones")
                .WithContent(c => c
                    .AddImage(Img("headphones-table"), "Учасники тесту")
                    .AddText("Активне шумозаглушення у середньому класі вже давно не компроміс. Різниця з флагманами відчувається хіба що в літаку.", "Шумозаглушення")
                    .AddGallery(g => g
                        .Add(Img("earbuds-case"), "Кейси для заряджання")
                        .Add(Img("earbuds-ear"), "Посадка у вусі")
                        .Add(Img("earbuds-app"), "Фірмовий застосунок"), "Деталі"))
                .Build(),

            Article("Механічні клавіатури: гайд для початківців",
                    "Які бувають перемикачі, що таке hot-swap і чому всі навколо збирають власні клавіатури.",
                    gaming, andrii, "keyboard")
                .WithContent(c => c
                    .AddText("Лінійні перемикачі натискаються плавно, тактильні дають відчутний «горбик», а клікові ще й гучно клацають. Почніть із тестера перемикачів — він коштує недорого.", "Типи перемикачів")
                    .AddImage(Img("keyboard-close"), "Тестер перемикачів")
                    .AddText("Hot-swap плата дозволяє міняти перемикачі без паяльника. Для першої клавіатури це найкращий вибір.", "Hot-swap"))
                .Build(),

            Article("Квантові комп'ютери простими словами",
                    "Кубіти, суперпозиція і чому квантовий комп'ютер не замінить ваш ноутбук. Пояснюємо без формул.",
                    analytics, iryna, "quantum")
                .Premium()
                .WithContent(c => c
                    .AddText("Звичайний біт — це 0 або 1. Кубіт може перебувати у суперпозиції обох станів, і саме це дає змогу розв'язувати певні задачі значно швидше.", "Кубіт проти біта")
                    .AddImage(Img("quantum-lab"), "Квантовий процесор потребує наднизьких температур")
                    .AddText("Квантові комп'ютери сильні в моделюванні молекул і криптографії, але для повсякденних задач класичні процесори залишаться ефективнішими.", "Де це знадобиться"))
                .Build());

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedIssuesAsync(MagazineDbContext db, CancellationToken ct)
    {
        if (await db.Issues.AnyAsync(ct))
            return;

        var articles = await db.Articles.ToDictionaryAsync(a => a.Slug, a => a.Id, ct);
        int Id(string slug) => articles[slug];

        var issue = new Issue(
            "Журнал місяця: Жовтень 2026",
            "Головні теми місяця: як обрати техніку для навчання, куди рухається штучний інтелект і що чекає на ринок гаджетів.",
            Img("issue-october"));

        var main = issue.AddSection(issue.Root, "Головне");
        issue.AddArticle(main, Id("iak-obraty-noutbuk-dlia-navchannia-u-2026-rotsi"), "Must-read для кожного студента перед новим семестром.");
        issue.AddArticle(main, Id("5-krokiv-shchob-zakhystyty-svoi-akaunty-vzhe-sohodni"));

        var future = issue.AddSection(issue.Root, "AI та майбутнє");
        issue.AddArticle(future, Id("lokalni-ai-modeli-chy-varto-zapuskaty-neiromerezhu-vdoma"), "Ексклюзив для передплатників.");
        var deepDive = issue.AddSection(future, "Глибоке занурення");
        issue.AddArticle(deepDive, Id("kvantovi-kompiutery-prostymy-slovamy"));
        issue.AddArticle(deepDive, Id("rynok-smart-hodynnykiv-shcho-bude-dali"));

        var people = issue.AddSection(issue.Root, "Люди та залізо");
        issue.AddArticle(people, Id("interviu-iak-indi-studiia-z-kyieva-robyt-ihry-dlia-vsoho-svitu"));
        issue.AddArticle(people, Id("mekhanichni-klaviatury-haid-dlia-pochatkivtsiv"));

        issue.Publish();
        issue.Feature();

        db.Issues.Add(issue);
        await db.SaveChangesAsync(ct);
    }

    private static ArticleBuilder Article(string title, string lead, Category category, Author author, string coverSeed) =>
        ArticleBuilder.Create(title)
            .WithLead(lead)
            .InCategory(category.Id)
            .By(author.Id)
            .WithCover(Img(coverSeed))
            .Published();

    private static string Img(string seed, int width = 1200, int height = 700) =>
        $"https://picsum.photos/seed/techpulse-{seed}/{width}/{height}";
}
