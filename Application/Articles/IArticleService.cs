using CURSOVA.Application.Common;

namespace CURSOVA.Application.Articles;

public interface IArticleService
{
    Task<PagedResult<ArticleSummaryDto>> SearchAsync(ArticleFilter filter, CancellationToken ct = default);
    Task<ArticleDetailsDto> GetAsync(int id, CancellationToken ct = default);
    Task<ArticleDetailsDto> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<ArticleDetailsDto> CreateAsync(SaveArticleRequest request, CancellationToken ct = default);
    Task<ArticleDetailsDto> UpdateAsync(int id, SaveArticleRequest request, CancellationToken ct = default);
    Task<ArticleDetailsDto> ReplaceContentAsync(int id, SaveContentRequest request, CancellationToken ct = default);
    Task<ArticleDetailsDto> PublishAsync(int id, CancellationToken ct = default);
    Task<ArticleDetailsDto> ArchiveAsync(int id, CancellationToken ct = default);
    Task DeleteAsync(int id, CancellationToken ct = default);
}
