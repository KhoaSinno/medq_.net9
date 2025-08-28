namespace Medq.Api.Contracts.Common;

public sealed record PagingQuery(int Page = 1, int PageSize = 0, string? Sort = null);
