using System.Data;
using System.Linq.Expressions;

namespace System.Linq;

public static class SimpleClientExpression
{
    #region SimpleClient
    public static int Count<T>(this SimpleClient<T> client) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().Count();
    }
    public static Task<int> CountAsync<T>(this SimpleClient<T> client) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().CountAsync();
    }
    public static ISugarQueryable<T> GetQueryable<T>(this SimpleClient<T> client, Expression<Func<T, bool>> whereExpression) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().Where(whereExpression);
    }
    public static T[] GetArray<T>(this SimpleClient<T> client) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().ToArray();
    }
    public static T[] GetArray<T>(this SimpleClient<T> client, Expression<Func<T, bool>> whereExpression) where T : class, new()
    {
        return client.GetQueryable(whereExpression).ToArray();
    }
    public static Task<T[]> GetArrayAsync<T>(this SimpleClient<T> client) where T : class, new()
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        return client.AsQueryable().ToArrayAsync();
    }
    public static Task<T[]> GetArrayAsync<T>(this SimpleClient<T> client, Expression<Func<T, bool>> whereExpression) where T : class, new()
    {
        return client.GetQueryable(whereExpression).ToArrayAsync();
    }
    #endregion

    #region ISugarQueryable
    public static void ForEachByPage<T>(this ISugarQueryable<T> client, Action<T> action, int pageIndex, int pageSize, out int totalNumber, int singleMaxReads = 300, CancellationTokenSource? cancellationTokenSource = null)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        client.ForEachByPage(action, pageIndex, pageSize, ref totalNumber, singleMaxReads, cancellationTokenSource);
    }
    public static string ToJsonPage<T>(this ISugarQueryable<T> client, int pageIndex, int pageSize, out int totalNumber)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        return client.ToJsonPage(pageIndex, pageSize, ref totalNumber);
    }
    public static DataTable ToDataTablePage<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        return client.ToDataTablePage(pageNumber, pageSize, ref totalNumber);
    }
    public static DataTable ToDataTablePage<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber, out int totalPage)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        totalPage = 0;
        return client.ToDataTablePage(pageNumber, pageSize, ref totalNumber, ref totalPage);
    }
    public static IReadOnlyCollection<T> ToOffsetPage<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        return client.ToOffsetPage(pageNumber, pageSize, ref totalNumber);
    }
    public static IReadOnlyCollection<T> ToPageList<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        return client.ToPageList(pageNumber, pageSize, ref totalNumber);
    }
    public static IReadOnlyCollection<T> ToPageList<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber, out int totalPage)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        totalNumber = 0;
        totalPage = 0;
        return client.ToPageList(pageNumber, pageSize, ref totalNumber, ref totalPage);
    }
    public static T[] ToPageArray<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber)
    {
        return client.ToPageList(pageNumber, pageSize, out totalNumber).ToArray();
    }
    public static T[] ToPageArray<T>(this ISugarQueryable<T> client, int pageNumber, int pageSize, out int totalNumber, out int totalPage)
    {
        return client.ToPageList(pageNumber, pageSize, out totalNumber, out totalPage).ToArray();
    }
    #endregion
}

