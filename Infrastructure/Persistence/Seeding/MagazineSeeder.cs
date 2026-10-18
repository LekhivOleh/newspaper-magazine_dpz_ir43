using CURSOVA.Domain.Articles.Building;
using CURSOVA.Domain.Authors;
using CURSOVA.Domain.Categories;
using CURSOVA.Domain.Interactive.Polls;
using CURSOVA.Domain.Interactive.Quizzes;
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
        await SeedPollsAsync(db, ct);
        await SeedQuizzesAsync(db, ct);
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
                        .Add(Img("earbuds-app"), "Фірмовий застосунок"), "Деталі", exclusive: true))
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

    private static async Task SeedPollsAsync(MagazineDbContext db, CancellationToken ct)
    {
        if (await db.InteractiveElements.OfType<Poll>().AnyAsync(ct))
            return;

        var laptopArticleId = await db.Articles
            .Where(a => a.Slug == "iak-obraty-noutbuk-dlia-navchannia-u-2026-rotsi")
            .Select(a => (int?)a.Id)
            .FirstOrDefaultAsync(ct);

        db.InteractiveElements.AddRange(
            new Poll("Ноутбук для навчання",
                "Що для вас найважливіше в ноутбуці для навчання?",
                ["Автономність", "Вага та розмір", "Продуктивність", "Ціна"],
                articleId: laptopArticleId),
            new Poll("AI у повсякденні",
                "Як часто ви користуєтеся AI-асистентами?",
                ["Щодня", "Кілька разів на тиждень", "Рідко", "Не користуюся"],
                description: "Опитування редакції TechPulse"),
            new Poll("Покупки року",
                "Які гаджети ви купили цього року?",
                ["Смартфон", "Ноутбук", "Навушники", "Смарт-годинник", "Нічого не купував(-ла)"],
                allowMultiple: true));

        await db.SaveChangesAsync(ct);
    }

    private static async Task SeedQuizzesAsync(MagazineDbContext db, CancellationToken ct)
    {
        if (await db.InteractiveElements.OfType<Quiz>().AnyAsync(ct))
            return;

        var gadget = new Quiz("Який гаджет вам підходить?", ScoringMode.Personality,
            "Чотири питання — і ми підкажемо, з яким пристроєм вам буде найкомфортніше.", Img("quiz-gadget"));
        gadget.Define(ScoringMode.Personality,
        [
            new QuestionSpec("Де ви найчастіше працюєте або навчаєтесь?",
            [
                new OptionSpec("За столом, довгими сесіями", OutcomeKey: "laptop"),
                new OptionSpec("На дивані або в ліжку", OutcomeKey: "tablet"),
                new OptionSpec("Будь-де, на ходу", OutcomeKey: "phone"),
                new OptionSpec("Я весь час у русі — тренування, прогулянки", OutcomeKey: "watch")
            ]),
            new QuestionSpec("Що для вас найважливіше?",
            [
                new OptionSpec("Продуктивність і зручна клавіатура", OutcomeKey: "laptop"),
                new OptionSpec("Великий екран для фільмів і читання", OutcomeKey: "tablet"),
                new OptionSpec("Камера і соцмережі", OutcomeKey: "phone"),
                new OptionSpec("Здоров'я та сповіщення без відволікань", OutcomeKey: "watch")
            ]),
            new QuestionSpec("Ваш ідеальний вихідний?",
            [
                new OptionSpec("Нарешті доробити свій проєкт", OutcomeKey: "laptop"),
                new OptionSpec("Серіал і комікси", OutcomeKey: "tablet"),
                new OptionSpec("Зустрічі з друзями та фото", OutcomeKey: "phone"),
                new OptionSpec("Похід або пробіжка", OutcomeKey: "watch")
            ]),
            new QuestionSpec("Скільки часу на день ви проводите з гаджетом?",
            [
                new OptionSpec("Майже весь робочий день", OutcomeKey: "laptop"),
                new OptionSpec("Кілька годин увечері", OutcomeKey: "tablet"),
                new OptionSpec("Постійно, але потроху", OutcomeKey: "phone"),
                new OptionSpec("Він просто завжди на мені", OutcomeKey: "watch")
            ])
        ],
        [
            new OutcomeSpec("laptop", "Ноутбук", "Вам потрібен повноцінний робочий інструмент: велика клавіатура, потужність і багатозадачність."),
            new OutcomeSpec("tablet", "Планшет", "Ви цінуєте комфорт і великий екран — для читання, серіалів і легкої роботи."),
            new OutcomeSpec("phone", "Смартфон", "Ваш стиль — мобільність. Хороша камера й автономність важать більше за все."),
            new OutcomeSpec("watch", "Смарт-годинник", "Ви в постійному русі, тож вам потрібен помічник, який не відволікає і стежить за здоров'ям.")
        ]);

        var knowledge = new Quiz("Наскільки добре ви знаєте технології?", ScoringMode.CorrectAnswers,
            "П'ять питань про базові речі, які варто знати кожному.", Img("quiz-knowledge"));
        knowledge.Define(ScoringMode.CorrectAnswers,
        [
            new QuestionSpec("Що означає абревіатура CPU?",
            [
                new OptionSpec("Central Processing Unit", IsCorrect: true),
                new OptionSpec("Computer Power Unit"),
                new OptionSpec("Central Program Utility")
            ]),
            new QuestionSpec("У яких одиницях зазвичай вказують тактову частоту процесора?",
            [
                new OptionSpec("Гігабайти"),
                new OptionSpec("Гігагерци", IsCorrect: true),
                new OptionSpec("Вати")
            ]),
            new QuestionSpec("Скільки бітів в одному байті?",
            [
                new OptionSpec("4"),
                new OptionSpec("8", IsCorrect: true),
                new OptionSpec("16")
            ]),
            new QuestionSpec("Що з переліченого є системою контролю версій?",
            [
                new OptionSpec("Git", IsCorrect: true),
                new OptionSpec("Figma"),
                new OptionSpec("Nginx")
            ]),
            new QuestionSpec("Який компонент зберігає дані після вимкнення живлення?",
            [
                new OptionSpec("Оперативна пам'ять (RAM)"),
                new OptionSpec("Кеш процесора"),
                new OptionSpec("SSD-накопичувач", IsCorrect: true)
            ])
        ],
        [
            new OutcomeSpec("beginner", "Початківець", "Є куди рости — почніть із наших гайдів у рубриці «Поради».", 0),
            new OutcomeSpec("confident", "Впевнений користувач", "Ви добре орієнтуєтесь у базових речах. Ще трохи — і будете експертом!", 50),
            new OutcomeSpec("expert", "Технічний експерт", "Чудовий результат! Вас складно здивувати технічними термінами.", 80)
        ]);

        var addiction = new Quiz("Наскільки ви залежні від гаджетів?", ScoringMode.Points,
            "Чесно дайте відповідь на три питання — і дізнайтеся, чи не час для цифрового детоксу.", Img("quiz-detox"));
        addiction.Define(ScoringMode.Points,
        [
            new QuestionSpec("Що ви робите першим після пробудження?",
            [
                new OptionSpec("Снідаю, телефон почекає", Points: 0),
                new OptionSpec("Швидко гляну сповіщення", Points: 1),
                new OptionSpec("Гортаю стрічку ще в ліжку", Points: 2)
            ]),
            new QuestionSpec("Як почуваєтеся, коли забули телефон удома?",
            [
                new OptionSpec("Спокійно — навіть приємно", Points: 0),
                new OptionSpec("Трохи незручно", Points: 1),
                new OptionSpec("Повертаюся по нього", Points: 2)
            ]),
            new QuestionSpec("Скільки годин на день у вас екранного часу?",
            [
                new OptionSpec("Менше 2 годин", Points: 0),
                new OptionSpec("2–5 годин", Points: 1),
                new OptionSpec("Більше 5 годин", Points: 2)
            ])
        ],
        [
            new OutcomeSpec("balanced", "Цифровий баланс", "Ви контролюєте гаджети, а не вони вас. Так тримати!", 0),
            new OutcomeSpec("connected", "Постійно на зв'язку", "Гаджети важливі для вас, але ще не керують днем. Іноді робіть паузи.", 2),
            new OutcomeSpec("detox", "Час для детоксу", "Схоже, екран забирає забагато часу. Спробуйте вихідний без телефону!", 5)
        ]);

        db.InteractiveElements.AddRange(gadget, knowledge, addiction);
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
