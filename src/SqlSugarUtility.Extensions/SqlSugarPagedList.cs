namespace SqlSugar;

/// <summary>
/// 分页查询结果封装，包含当前页数据及分页元信息。
/// </summary>
/// <typeparam name="TEntity">单条记录的类型。</typeparam>
/// <remarks>
/// 通常通过 <see cref="SqlSugarPagedExtensions.ToPagedListAsync{TEntity}(ISugarQueryable{TEntity}, int, int)"/> 获取。
/// </remarks>
public class SqlSugarPagedList<TEntity>
{
    /// <summary>
    /// 当前页码，从 1 开始。
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页记录数。
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 满足查询条件的总记录数。
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 根据 <see cref="Total"/> 与 <see cref="PageSize"/> 计算得到的总页数。
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 当前页的数据集合（只读）。
    /// </summary>
    public System.Collections.ObjectModel.ReadOnlyCollection<TEntity>? Items { get; set; }

    /// <summary>
    /// 是否存在上一页（即 <see cref="Page"/> &gt; 1）。
    /// </summary>
    public bool HasPrevPage { get; set; }

    /// <summary>
    /// 是否存在下一页（即 <see cref="Page"/> &lt; <see cref="TotalPages"/>）。
    /// </summary>
    public bool HasNextPage { get; set; }
}
