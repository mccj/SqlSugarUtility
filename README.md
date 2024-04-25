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
                if (string.IsNullOrWhiteSpace(connectionString)) throw new Exception("�����ַ������ܿ�");

                _client = SqlSugarUtility.GetSingletonSqlSugarClient<IgnoreAttribute>(dbType, connectionString, false, db =>
                {
                    //(A)ȫ����Ч���õ㣬һ��AOP�ͳ��������������������� ��������������Ч
                    //����SQL�¼�������ɾ��
                    db.Aop.OnLogExecuting = (sql, pars) =>
                    {
#if DEBUG
                        //Console.WriteLine(sql);//���sql,�鿴ִ��sql ������Ӱ��

                        //5.0.8.2 ��ȡ�޲����� SQL  ��������Ӱ�죬�ر���SQL������ģ�����ʹ��
                        var sql2 = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, sql, pars);
                        Console.WriteLine(sql2);//���sql,�鿴ִ��sql ������Ӱ��
                        System.Diagnostics.Debug.WriteLine(sql2);
#endif
                    };
                    db.Aop.OnError = (ex) =>
                    {
                        Console.WriteLine(ex.Message);//���sql,�鿴ִ��sql ������Ӱ��
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