using CURSOVA.Application.Articles;
using CURSOVA.Application.Categories;
using CURSOVA.Application.Common;
using CURSOVA.Application.Interactive;
using CURSOVA.Application.Issues;

namespace CURSOVA.Models;

public record PagerViewModel(int Page, int TotalPages, Func<int, string> PageUrl);

public record HomeViewModel(IssueSummaryDto? FeaturedIssue, ArticleSummaryDto? Featured, IReadOnlyList<ArticleSummaryDto> Articles, PagerViewModel Pager);

public record CategoryPageViewModel(CategoryDto Category, PagedResult<ArticleSummaryDto> Articles, PagerViewModel Pager);

public record SearchViewModel(string? Query, PagedResult<ArticleSummaryDto>? Results, PagerViewModel? Pager);

public record ArticlePageViewModel(ArticleDetailsDto Article, IReadOnlyList<ArticleSummaryDto> Related, PollDto? Poll);
