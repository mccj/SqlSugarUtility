using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace System.Linq;

/// <summary>
/// 为 <see cref="ISugarQueryable{T}"/> 与内存集合提供分页扩展，返回统一的 <see cref="SqlSugarPagedList{TEntity}"/>。
/// </summary>
public static class SqlSugarPagedExtensions
{
    /// <summary>
    /// 对数据库查询分页，并将结果投影为指定类型。
    /// </summary>
    /// <typeparam name="TEntity">查询实体类型。</typeparam>
    /// <typeparam name="TResult">投影结果类型。</typeparam>
    /// <param name="query">SqlSugar 可查询对象。</param>
    /// <param name="pageIndex">当前页码，从 1 开始。</param>
    /// <param name="pageSize">每页记录数。</param>
    /// <param name="expression">Select 投影表达式。</param>
    /// <returns>包含投影结果的分页对象。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> 为 <see langword="null"/> 时抛出。</exception>
    public static SqlSugarPagedList<TResult> ToPagedList<TEntity, TResult>(this ISugarQueryable<TEntity> query, int pageIndex, int pageSize,
        Expression<Func<TEntity, TResult>> expression) where TEntity : class, new()
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        var total = 0;
        var items = query.ToPageList(pageIndex, pageSize, ref total, expression);
        return CreateSqlSugarPagedList(items.ToArray(), total, pageIndex, pageSize);
    }

    /// <summary>
    /// 对数据库查询分页，返回完整实体列表。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    /// <param name="query">SqlSugar 可查询对象。</param>
    /// <param name="pageIndex">当前页码，从 1 开始。</param>
    /// <param name="pageSize">每页记录数。</param>
    /// <returns>包含实体数据的分页对象。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> 为 <see langword="null"/> 时抛出。</exception>
    public static SqlSugarPagedList<TEntity> ToPagedList<TEntity>(this ISugarQueryable<TEntity> query, int pageIndex, int pageSize) where TEntity : class, new()
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        var total = 0;
        var items = query.ToPageList(pageIndex, pageSize, ref total);
        return CreateSqlSugarPagedList(items.ToArray(), total, pageIndex, pageSize);
    }

    /// <summary>
    /// 异步对数据库查询分页，并将结果投影为指定类型。
    /// </summary>
    /// <typeparam name="TEntity">查询实体类型。</typeparam>
    /// <typeparam name="TResult">投影结果类型。</typeparam>
    /// <param name="query">SqlSugar 可查询对象。</param>
    /// <param name="pageIndex">当前页码，从 1 开始。</param>
    /// <param name="pageSize">每页记录数。</param>
    /// <param name="expression">Select 投影表达式。</param>
    /// <returns>包含投影结果的分页对象。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> 为 <see langword="null"/> 时抛出。</exception>
    public static async Task<SqlSugarPagedList<TResult>> ToPagedListAsync<TEntity, TResult>(this ISugarQueryable<TEntity> query, int pageIndex, int pageSize,
        Expression<Func<TEntity, TResult>> expression) where TEntity : class, new()
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, total, expression).ConfigureAwait(false);
        return CreateSqlSugarPagedList(items.ToArray(), total, pageIndex, pageSize);
    }

    /// <summary>
    /// 异步对数据库查询分页，返回完整实体列表。
    /// </summary>
    /// <typeparam name="TEntity">实体类型。</typeparam>
    /// <param name="query">SqlSugar 可查询对象。</param>
    /// <param name="pageIndex">当前页码，从 1 开始。</param>
    /// <param name="pageSize">每页记录数。</param>
    /// <returns>包含实体数据的分页对象。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="query"/> 为 <see langword="null"/> 时抛出。</exception>
    public static async Task<SqlSugarPagedList<TEntity>> ToPagedListAsync<TEntity>(this ISugarQueryable<TEntity> query, int pageIndex, int pageSize) where TEntity : class, new()
    {
        if (query == null) throw new ArgumentNullException(nameof(query));
        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, total).ConfigureAwait(false);
        return CreateSqlSugarPagedList(items.ToArray(), total, pageIndex, pageSize);
    }

    /// <summary>
    /// 对已在内存中的集合进行分页（不发起数据库查询）。
    /// </summary>
    /// <typeparam name="TEntity">元素类型。</typeparam>
    /// <param name="list">内存中的数据集合。</param>
    /// <param name="pageIndex">当前页码，从 1 开始。</param>
    /// <param name="pageSize">每页记录数。</param>
    /// <returns>分页结果；总条数取自 <paramref name="list"/> 的元素数量。</returns>
    public static SqlSugarPagedList<TEntity> ToPagedList<TEntity>(this IEnumerable<TEntity> list, int pageIndex, int pageSize)
    {
        var total = list.Count();
        var items = list.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        return CreateSqlSugarPagedList(items.ToArray(), total, pageIndex, pageSize);
    }

    /// <summary>
    /// 根据分页参数与数据构建 <see cref="SqlSugarPagedList{TEntity}"/> 实例。
    /// </summary>
    /// <typeparam name="TEntity">元素类型。</typeparam>
    /// <param name="items">当前页数据数组。</param>
    /// <param name="total">总记录数。</param>
    /// <param name="pageIndex">当前页码，从 1 开始。</param>
    /// <param name="pageSize">每页记录数。</param>
    /// <returns>填充了分页元信息的只读结果对象。</returns>
    private static SqlSugarPagedList<TEntity> CreateSqlSugarPagedList<TEntity>(TEntity[] items, int total, int pageIndex, int pageSize)
    {
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0;
        return new SqlSugarPagedList<TEntity>
        {
            Page = pageIndex,
            PageSize = pageSize,
            Items = new ReadOnlyCollection<TEntity>(items),
            Total = total,
            TotalPages = totalPages,
            HasNextPage = pageIndex < totalPages,
            HasPrevPage = pageIndex - 1 > 0
        };
    }
}