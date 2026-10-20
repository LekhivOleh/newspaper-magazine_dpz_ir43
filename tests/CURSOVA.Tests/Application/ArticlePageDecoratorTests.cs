using CURSOVA.Application.Advertising;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Articles.Presentation;
using NSubstitute;
using static CURSOVA.Tests.TestSupport;

namespace CURSOVA.Tests.Application;

public class ArticlePageDecoratorTests
{
    private static readonly PartnerDto Partner = new(7, "Гаджетарня", null, null, "https://example.com", true);
    private static readonly ReaderContext FreeReader = new(HasPremium: false);
    private static readonly ReaderContext PremiumReader = new(HasPremium: true);

    private readonly IAdvertisingService _ads = Substitute.For<IAdvertisingService>();

    public ArticlePageDecoratorTests()
    {
        _ads.PickAsync(Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(call => Enumerable.Range(1, call.ArgAt<int>(1)).Select(Ad).ToList());
        _ads.FindActivePartnerAsync(Partner.Id, Arg.Any<CancellationToken>()).Returns(Partner);
    }

    private static AdDto Ad(int id) => new(id, $"Реклама {id}", null, null, "https://example.com", "Перейти", null, null, null, true, 0, 0);

    private IArticlePageComposer FullChain() =>
        new SponsoredContentDecorator(new AdSlotsDecorator(new PlainArticleComposer(), _ads), _ads);

    private static string Shape(ArticlePage page) => string.Join(" ", page.Segments.Select(s => s switch
    {
        ContentSegment => "text",
        AdSegment => "ad",
        SponsorDisclosureSegment => "disclosure",
        PartnerPromoSegment => "promo",
        _ => "?"
    }));

    private static ArticleDetailsDto Blocks(int count, int? sponsor = null) =>
        Article(Enumerable.Range(1, count).Select(i => (BlockDto)Text(i.ToString())).ToList(), sponsorPartnerId: sponsor);

    [Fact]
    public async Task Plain_component_renders_only_the_article()
    {
        var page = await new PlainArticleComposer().ComposeAsync(Blocks(3), FreeReader);

        Assert.Equal("text text text", Shape(page));
    }

    [Fact]
    public async Task Ad_slots_go_after_every_two_blocks_up_to_the_limit()
    {
        var page = await new AdSlotsDecorator(new PlainArticleComposer(), _ads).ComposeAsync(Blocks(7), FreeReader);

        Assert.Equal("text text ad text text ad text text text", Shape(page));
    }

    [Fact]
    public async Task Short_article_still_gets_one_ad_at_the_end()
    {
        var page = await new AdSlotsDecorator(new PlainArticleComposer(), _ads).ComposeAsync(Blocks(1), FreeReader);

        Assert.Equal("text ad", Shape(page));
    }

    [Fact]
    public async Task Decorators_stack_disclosure_ads_and_partner_promo()
    {
        var page = await FullChain().ComposeAsync(Blocks(4, sponsor: Partner.Id), FreeReader);

        Assert.Equal("disclosure text text ad text text ad promo", Shape(page));
    }

    [Fact]
    public async Task Premium_reader_gets_no_ads_but_still_sees_the_sponsor_disclosure()
    {
        var page = await FullChain().ComposeAsync(Blocks(4, sponsor: Partner.Id), PremiumReader);

        Assert.Equal("disclosure text text text text", Shape(page));
        await _ads.DidNotReceive().PickAsync(Arg.Any<int?>(), Arg.Any<int>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Inactive_sponsor_adds_nothing()
    {
        _ads.FindActivePartnerAsync(Partner.Id, Arg.Any<CancellationToken>()).Returns((PartnerDto?)null);

        var page = await new SponsoredContentDecorator(new PlainArticleComposer(), _ads).ComposeAsync(Blocks(2, sponsor: Partner.Id), FreeReader);

        Assert.Equal("text text", Shape(page));
    }
}
