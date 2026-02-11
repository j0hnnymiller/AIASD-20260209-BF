using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PostHubAPI.Exceptions;

namespace PostHubAPI.Extensions;

public static class QueryableExtensions
{
    public static async Task<T> GetOrThrowAsync<T>(
        this IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        string? errorMessage = null) where T : class
    {
        var entity = await query.FirstOrDefaultAsync(predicate);
        if (entity == null)
        {
            throw new NotFoundException(
                errorMessage ?? $"{typeof(T).Name} not found!");
        }
        return entity;
    }
}
