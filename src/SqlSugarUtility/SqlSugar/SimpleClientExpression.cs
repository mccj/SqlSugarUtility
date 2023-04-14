using SqlSugar;
using System.Linq.Expressions;

namespace System.Linq
{
    public static class SimpleClientExpression
    {
        public static int Count<T>(this SimpleClient<T> client) where T : class, new()
        {
            return client.AsQueryable().Count();
        }
        public static Task<int> CountAsync<T>(this SimpleClient<T> client) where T : class, new()
        {
            return client.AsQueryable().CountAsync();
        }
        public static ISugarQueryable<T> GetQueryable<T>(this SimpleClient<T> client, Expression<Func<T, bool>> whereExpression) where T : class, new()
        {
            return client.AsQueryable().Where(whereExpression);
        }
        public static T[] GetArray<T>(this SimpleClient<T> client) where T : class, new()
        {
            return client.AsQueryable().ToArray();
        }
        public static T[] GetArray<T>(this SimpleClient<T> client, Expression<Func<T, bool>> whereExpression) where T : class, new()
        {
            return client.GetQueryable(whereExpression).ToArray();
        }
        public static Task<T[]> GetArrayAsync<T>(this SimpleClient<T> client) where T : class, new()
        {
            return client.AsQueryable().ToArrayAsync();
        }
        public static Task<T[]> GetArrayAsync<T>(this SimpleClient<T> client, Expression<Func<T, bool>> whereExpression) where T : class, new()
        {
            return client.GetQueryable(whereExpression).ToArrayAsync();
        }
    }
}

