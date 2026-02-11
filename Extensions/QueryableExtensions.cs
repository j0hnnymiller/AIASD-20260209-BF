using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PostHubAPI.Exceptions;

namespace PostHubAPI.Extensions;

public static class QueryableExtensions
{
    public static async Task<T> GetOrThrowAsync<T>(
        this IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        string errorMessage) where T : class
    {
        return await query.FirstOrDefaultAsync(predicate) ?? throw new NotFoundException(errorMessage);
    }
}
