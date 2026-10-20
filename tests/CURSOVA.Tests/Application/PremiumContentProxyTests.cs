using CURSOVA.Application.Abstractions.Identity;
using CURSOVA.Application.Articles;
using CURSOVA.Application.Articles.Access;
using CURSOVA.Application.Subscriptions;
using CURSOVA.Domain.Articles;
using NSubstitute;
using static CURSOVA.Tests.TestSupport;

namespace CURSOVA.Tests.Application;

public class PremiumContentProxyTests
{
    private readonly IArticleReader _realReader = Substitute.For<IArticleReader>();
    private readonly ISubscriptionService _subscriptions = Substitute.For<ISubscriptionService>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();

    private Task<ArticleDetailsDto> ReadAsync(ArticleDetailsDto stored, bool hasPremium = false, bool isAdmin = false)
    {
        _realReader.GetAsync(1, Arg.Any<CancellationToken>()).Returns(stored);
        _subscriptions.HasPremiumAccessAsync(Arg.Any<CancellationToken>()).Returns(hasPremium);
        _user.IsAdmin.Returns(isAdmin);
        return new PremiumContentProxy(_realReader, _subscriptions, _user).GetAsync(1);
    }

    [Fact]
    public async Task Free_article_passes_through_without_checking_the_subscription()
    {
        var stored = Article([Text(), Text()]);

        var result = await ReadAsync(stored);

        Assert.Equal(stored.Blocks, result.Blocks);
        Assert.True(result.Access!.IsFull);
        await _subscriptions.DidNotReceive().HasPremiumAccessAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Premium_article_is_cut_to_a_teaser_for_free_readers()
    {
        var result = await ReadAsync(Article([Text("1"), Text("2"), Text("3"), Text("4")], AccessLevel.Premium));

        Assert.Equal(PremiumContentProxy.TeaserBlocks, result.Blocks.Count);
        Assert.False(result.Access!.IsFull);
        Assert.Equal(2, result.Access.HiddenBlocks);
    }

    [Fact]
    public async Task Exclusive_media_is_replaced_with_locked_placeholders()
    {
        var result = await ReadAsync(Article([Text(), ExclusiveImage(), Text()]));

        Assert.Equal(3, result.Blocks.Count);
        Assert.Equal("image", Assert.IsType<LockedBlockDto>(result.Blocks[1]).LockedType);
        Assert.Equal(1, result.Access!.HiddenBlocks);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Subscribers_and_editors_get_the_full_article(bool hasPremium, bool isAdmin)
    {
        var stored = Article([Text(), ExclusiveImage(), Text(), Text()], AccessLevel.Premium);

        var result = await ReadAsync(stored, hasPremium, isAdmin);

        Assert.Equal(stored.Blocks, result.Blocks);
        Assert.True(result.Access!.IsFull);
    }
}
