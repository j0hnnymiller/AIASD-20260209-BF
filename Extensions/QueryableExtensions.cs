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
        T? entity = await query.FirstOrDefaultAsync(predicate);
        if (entity != null)
        {
            return entity;
        }

        throw new NotFoundException(errorMessage);
    }
}
