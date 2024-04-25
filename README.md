# SqlSugarUtility

[![Build status](https://ci.appveyor.com/api/projects/status/qiondtrhluxh4p4n?svg=true)](https://ci.appveyor.com/project/mccj/sqlsugarutility)
[![MyGet](https://img.shields.io/myget/mccj/vpre/SqlSugarUtility.svg)](https://myget.org/feed/mccj/package/nuget/SqlSugarUtility)
[![NuGet](https://buildstats.info/nuget/SqlSugarUtility?includePreReleases=false)](https://www.nuget.org/packages/SqlSugarUtility)
[![MIT License](https://img.shields.io/badge/license-MIT-orange.svg)](https://github.com/mccj/AutoModeCodeGenerator.Analyzers/blob/master/LICENSE)

```C#
using SqlSugar;

public static class SqlSugarHelper
{
    private static ISqlSugarClient? _client = null;
    public static ISqlSugarClient Db
    {
        get
        {
            if (_client == null)
            {

                var dbType = DbType.SqlServer;
                //var connectionString = "Data Source=192.168.x.x;Initial Catalog=xxx;User ID=sa;PWD=xxx";
                if (string.IsNullOrWhiteSpace(connectionString)) throw new Exception("连接字符串不能空");

                _client = SqlSugarUtility.GetSingletonSqlSugarClient<IgnoreAttribute>(dbType, connectionString, false, db =>
                {
                    //(A)全局生效配置点，一般AOP和程序启动的配置扔这里面 ，所有上下文生效
                    //调试SQL事件，可以删掉
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
#if DEBUG
                        //Console.WriteLine(sql);//输出sql,查看执行sql 性能无影响

                        //5.0.8.2 获取无参数化 SQL  对性能有影响，特别大的SQL参数多的，调试使用
                        var sql2 = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, sql, pars);
                        Console.WriteLine(sql2);//输出sql,查看执行sql 性能无影响
                        System.Diagnostics.Debug.WriteLine(sql2);
#endif
                    };
                    db.Aop.OnError = (ex) =>
                    {
                        Console.WriteLine(ex.Message);//输出sql,查看执行sql 性能无影响
                        System.Diagnostics.Debug.Fail(ex.Message);
                    };
                    db.Aop.OnDiffLogEvent = (diff) =>
                    {

                    };
                });

            }
            return _client;
        }
    }
}
public class IgnoreAttribute : Attribute { }
```