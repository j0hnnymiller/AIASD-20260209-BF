using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PostHubAPI.Exceptions;

namespace PostHubAPI.Extensions;

/// <summary>
/// Extension methods for IQueryable to reduce boilerplate code
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Retrieves an entity matching the predicate or throws NotFoundException
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The queryable source</param>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="errorMessage">Optional custom error message</param>
    /// <returns>The found entity</returns>
    /// <exception cref="NotFoundException">Thrown when entity is not found</exception>
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

    /// <summary>
    /// Retrieves an entity matching the predicate with specific resource type info or throws NotFoundException
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <param name="query">The queryable source</param>
    /// <param name="predicate">The filter predicate</param>
    /// <param name="id">The entity ID for better error messaging</param>
    /// <returns>The found entity</returns>
    /// <exception cref="NotFoundException">Thrown when entity is not found with ID information</exception>
    public static async Task<T> GetOrThrowAsync<T>(
        this IQueryable<T> query,
        Expression<Func<T, bool>> predicate,
        int id) where T : class
    {
        var entity = await query.FirstOrDefaultAsync(predicate);
        if (entity == null)
        {
            throw new NotFoundException(typeof(T).Name, id);
        }
        return entity;
    }
}
