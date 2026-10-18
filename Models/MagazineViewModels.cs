using CURSOVA.Application.Articles;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Comments;
using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Application.Issues;
using CURSOVA.Application.Profiles;
using CURSOVA.Application.Sharing;

namespace CURSOVA.Models;

public record PagerViewModel(int Page, int TotalPages, Func<int, string> PageUrl);

public record HomeViewModel(IssueSummaryDto? FeaturedIssue, ArticleSummaryDto? Featured, IReadOnlyList<ArticleSummaryDto> Articles, PagerViewModel Pager);

public record CategoryPageViewModel(CategoryDto Category, PagedResult<ArticleSummaryDto> Articles, PagerViewModel Pager);

public record SearchViewModel(string? Query, PagedResult<ArticleSummaryDto>? Results, PagerViewModel? Pager);

public record ArticlePageViewModel(ArticleDetailsDto Article, IReadOnlyList<ArticleSummaryDto> Related, PollDto? Poll, IReadOnlyList<CommentDto> Comments, ShareBarViewModel Share, bool IsSaved,
    IReadOnlyList<CollectionChoiceDto> Collections);

public record InteractiveHubViewModel(IReadOnlyList<QuizSummaryDto> Quizzes, IReadOnlyList<PollDto> Polls);

public record ShareBarViewModel(IReadOnlyList<ShareLinkDto> Links, int TotalShares, string PageUrl);

public record SavedPageViewModel(string DisplayName, PagedResult<SavedArticleDto> Saved, PagerViewModel Pager);
