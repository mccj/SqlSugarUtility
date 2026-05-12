using System.Data;
using System.Linq.Expressions;

namespace System.Linq;

/// <summary>
/// 为 SqlSugar 的 SimpleClient 和 ISugarQueryable 提供一组扩展方法，简化常用数据访问操作（如计数、数组转换、分页遍历等）。
/// </summary>
public static class SimpleClientExpression
{
    #region SimpleClient

    /// <summary>
    /// 获取实体对应的表中满足默认条件的数据行数。
    /// </summary>
    /// <typeparam name="T">实体类型，必须为 class 且具有无参构造函数。</typeparam>
    /// <param name="client">SimpleClient 实例。</param>
    /// <returns>表中数据的总行数。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static int Count<T>(this SimpleClient<T> client) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().Count();
    }

    /// <summary>
    /// 异步获取实体对应的表中满足默认条件的数据行数。
    /// </summary>
    /// <typeparam name="T">实体类型，必须为 class 且具有无参构造函数。</typeparam>
    /// <param name="client">SimpleClient 实例。</param>
    /// <returns>表示异步操作的任务，任务结果包含表中数据的总行数。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static Task<int> CountAsync<T>(this SimpleClient<T> client) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().CountAsync();
    }

    /// <summary>
    /// 根据条件表达式获取可查询对象（ISugarQueryable），可用于后续的筛选、排序等操作。
    /// </summary>
    /// <typeparam name="T">实体类型，必须为 class 且具有无参构造函数。</typeparam>
    /// <param name="client">SimpleClient 实例。</param>
    /// <param name="whereExpression">筛选条件表达式。</param>
    /// <returns>包含指定筛选条件的可查询对象。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static ISugarQueryable<T> GetQueryable<T>(this SimpleClient<T> client, Expression<Func<T, bool>> whereExpression) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().Where(whereExpression);
    }

    /// <summary>
    /// 获取表中所有数据并转换为数组。
    /// </summary>
    /// <typeparam name="T">实体类型，必须为 class 且具有无参构造函数。</typeparam>
    /// <param name="client">SimpleClient 实例。</param>
    /// <returns>包含表中所有数据的数组。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static T[] GetArray<T>(this SimpleClient<T> client) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().ToArray();
    }

    /// <summary>
    /// 根据条件获取表中满足条件的数据并转换为数组。
    /// </summary>
    /// <typeparam name="T">实体类型，必须为 class 且具有无参构造函数。</typeparam>
    /// <param name="client">SimpleClient 实例。</param>
    /// <param name="whereExpression">筛选条件表达式。</param>
    /// <returns>包含满足条件数据的数组。</returns>
    public static T[] GetArray<T>(this SimpleClient<T> client, Expression<Func<T, bool>> whereExpression) where T : class, new()
    {
        return client.GetQueryable(whereExpression).ToArray();
    }

    /// <summary>
    /// 异步获取表中所有数据并转换为数组。
    /// </summary>
    /// <typeparam name="T">实体类型，必须为 class 且具有无参构造函数。</typeparam>
    /// <param name="client">SimpleClient 实例。</param>
    /// <returns>表示异步操作的任务，任务结果包含表中所有数据的数组。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static Task<T[]> GetArrayAsync<T>(this SimpleClient<T> client) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().ToArrayAsync();
    }

    /// <summary>
    /// 异步根据条件获取表中满足条件的数据并转换为数组。
    /// </summary>
    /// <typeparam name="T">实体类型，必须为 class 且具有无参构造函数。</typeparam>
    /// <param name="client">SimpleClient 实例。</param>
    /// <param name="whereExpression">筛选条件表达式。</param>
    /// <returns>表示异步操作的任务，任务结果包含满足条件数据的数组。</returns>
    public static Task<T[]> GetArrayAsync<T>(this SimpleClient<T> client, Expression<Func<T, bool>> whereExpression) where T : class, new()
    {
        return client.GetQueryable(whereExpression).ToArrayAsync();
    }

    #endregion

    #region ISugarQueryable

    /// <summary>
    /// 对查询结果进行分页遍历，并在每一页上执行指定的操作。此方法自动处理分页逻辑，并返回总记录数。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="client">ISugarQueryable 实例。</param>
    /// <param name="action">对每一页中的每个元素执行的操作。</param>
    /// <param name="pageIndex">页码（从1开始）。</param>
    /// <param name="pageSize">每页大小。</param>
    /// <param name="totalNumber">输出参数，表示满足条件的数据总行数。</param>
    /// <param name="singleMaxReads">每次内部读取的最大记录数，用于控制内存使用（默认为300）。</param>
    /// <param name="cancellationTokenSource">可选的取消令牌源，用于取消遍历操作。</param>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static void ForEachByPage<T>(this ISugarQueryable<T> client, Action<T> action, int pageIndex, int pageSize, out int totalNumber, int singleMaxReads = 300, CancellationTokenSource? cancellationTokenSource = null)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        client.ForEachByPage(action, pageIndex, pageSize, ref totalNumber, singleMaxReads, cancellationTokenSource);
    }

    /// <summary>
    /// 将查询结果分页并序列化为 JSON 字符串，同时返回总记录数。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="client">ISugarQueryable 实例。</param>
    /// <param name="pageIndex">页码（从1开始）。</param>
    /// <param name="pageSize">每页大小。</param>
    /// <param name="totalNumber">输出参数，表示满足条件的数据总行数。</param>
    /// <returns>当前页数据的 JSON 字符串。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static string ToJsonPage<T>(this ISugarQueryable<T> client, int pageIndex, int pageSize, out int totalNumber)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        return client.ToJsonPage(pageIndex, pageSize, ref totalNumber);
    }

    /// <summary>
    /// 将查询结果分页并转换为 DataTable，同时返回总记录数。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="client">ISugarQueryable 实例。</param>
    /// <param name="pageNumber">页码（从1开始）。</param>
    /// <param name="pageSize">每页大小。</param>
    /// <param name="totalNumber">输出参数，表示满足条件的数据总行数。</param>
    /// <returns>包含当前页数据的 DataTable。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static DataTable ToDataTablePage<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        return client.ToDataTablePage(pageNumber, pageSize, ref totalNumber);
    }

    /// <summary>
    /// 将查询结果分页并转换为 DataTable，同时返回总记录数和总页数。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="client">ISugarQueryable 实例。</param>
    /// <param name="pageNumber">页码（从1开始）。</param>
    /// <param name="pageSize">每页大小。</param>
    /// <param name="totalNumber">输出参数，表示满足条件的数据总行数。</param>
    /// <param name="totalPage">输出参数，表示总页数。</param>
    /// <returns>包含当前页数据的 DataTable。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static DataTable ToDataTablePage<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber, out int totalPage)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        totalPage = 0;
        return client.ToDataTablePage(pageNumber, pageSize, ref totalNumber, ref totalPage);
    }

    /// <summary>
    /// 将查询结果分页（基于偏移量）并返回只读集合，同时返回总记录数。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="client">ISugarQueryable 实例。</param>
    /// <param name="pageNumber">页码（从1开始）。</param>
    /// <param name="pageSize">每页大小。</param>
    /// <param name="totalNumber">输出参数，表示满足条件的数据总行数。</param>
    /// <returns>包含当前页数据的只读集合。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static IReadOnlyCollection<T> ToOffsetPage<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        return client.ToOffsetPage(pageNumber, pageSize, ref totalNumber);
    }

    /// <summary>
    /// 将查询结果分页并返回只读集合，同时返回总记录数。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="client">ISugarQueryable 实例。</param>
    /// <param name="pageNumber">页码（从1开始）。</param>
    /// <param name="pageSize">每页大小。</param>
    /// <param name="totalNumber">输出参数，表示满足条件的数据总行数。</param>
    /// <returns>包含当前页数据的只读集合。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static IReadOnlyCollection<T> ToPageList<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        return client.ToPageList(pageNumber, pageSize, ref totalNumber);
    }

    /// <summary>
    /// 将查询结果分页并返回只读集合，同时返回总记录数和总页数。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="client">ISugarQueryable 实例。</param>
    /// <param name="pageNumber">页码（从1开始）。</param>
    /// <param name="pageSize">每页大小。</param>
    /// <param name="totalNumber">输出参数，表示满足条件的数据总行数。</param>
    /// <param name="totalPage">输出参数，表示总页数。</param>
    /// <returns>包含当前页数据的只读集合。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="client"/> 为 null 时抛出。</exception>
    public static IReadOnlyCollection<T> ToPageList<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber, out int totalPage)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        totalPage = 0;
        return client.ToPageList(pageNumber, pageSize, ref totalNumber, ref totalPage);
    }

    /// <summary>
    /// 将查询结果分页并返回数组，同时返回总记录数。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="client">ISugarQueryable 实例。</param>
    /// <param name="pageNumber">页码（从1开始）。</param>
    /// <param name="pageSize">每页大小。</param>
    /// <param name="totalNumber">输出参数，表示满足条件的数据总行数。</param>
    /// <returns>包含当前页数据的数组。</returns>
    public static T[] ToPageArray<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber)
    {
        return client.ToPageList(pageNumber, pageSize, out totalNumber).ToArray();
    }

    /// <summary>
    /// 将查询结果分页并返回数组，同时返回总记录数和总页数。
    /// </summary>
    /// <typeparam name="T">实体类型。</typeparam>
    /// <param name="client">ISugarQueryable 实例。</param>
    /// <param name="pageNumber">页码（从1开始）。</param>
    /// <param name="pageSize">每页大小。</param>
    /// <param name="totalNumber">输出参数，表示满足条件的数据总行数。</param>
    /// <param name="totalPage">输出参数，表示总页数。</param>
    /// <returns>包含当前页数据的数组。</returns>
    public static T[] ToPageArray<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber, out int totalPage)
    {
        return client.ToPageList(pageNumber, pageSize, out totalNumber, out totalPage).ToArray();
    }

    #endregion
}