using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

using Yitter.IdGenerator;

namespace SqlSugar;

public static class SqlSugarUtility
{
    static SqlSugarUtility()
    {
        SetIdGenerator(new IdGeneratorOptions
        {
            WorkerId = 1,
            WorkerIdBitLength = 6, // 机器码位长 默认值6，取值范围 [1, 19]
            SeqBitLength = 6 // 序列数位长 默认值6，取值范围 [3, 21]（建议不小于4，值越大性能越高、Id位数也更长）
        });
    }
    public static void SetIdGenerator(IdGeneratorOptions snowIdOpt)
    {
        YitIdHelper.SetIdGenerator(snowIdOpt);
        SnowFlakeSingle.WorkId = snowIdOpt?.WorkerId ?? 1;

        // 自定义 SqlSugar 雪花ID算法
        StaticConfig.CustomSnowFlakeFunc = YitIdHelper.NextId;
    }
    /// <summary>
    /// 单例模式
    /// </summary>
    /// <param name="dbType"></param>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static ISqlSugarClient GetSingletonSqlSugarClient(DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) => GetSingletonSqlSugarClient<IgnoreAttribute>(dbType, connectionString, tableEnumIsString, configAction);
    public static ISqlSugarClient GetSingletonSqlSugarClient(DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) => GetSingletonSqlSugarClient<IgnoreAttribute>(dbType, connectionString, moreSettings, configAction);

    public static ISqlSugarClient GetSingletonSqlSugarClient<IgnoreT>(DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) where IgnoreT : Attribute => GetSqlSugarClient<IgnoreT>(false, dbType, connectionString, tableEnumIsString, configAction);
    public static ISqlSugarClient GetSingletonSqlSugarClient<IgnoreT>(DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) where IgnoreT : Attribute => GetSqlSugarClient<IgnoreT>(false, dbType, connectionString, moreSettings, configAction);
    /// <summary>
    /// Scope 模式
    /// </summary>
    /// <param name="dbType"></param>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static ISqlSugarClient GetScopeSqlSugarClient(DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) => GetScopeSqlSugarClient<IgnoreAttribute>(dbType, connectionString, tableEnumIsString, configAction);
    public static ISqlSugarClient GetScopeSqlSugarClient(DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) => GetScopeSqlSugarClient<IgnoreAttribute>(dbType, connectionString, moreSettings, configAction);

    public static ISqlSugarClient GetScopeSqlSugarClient<IgnoreT>(DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) where IgnoreT : Attribute => GetSqlSugarClient<IgnoreT>(true, dbType, connectionString, tableEnumIsString, configAction);
    public static ISqlSugarClient GetScopeSqlSugarClient<IgnoreT>(DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) where IgnoreT : Attribute => GetSqlSugarClient<IgnoreT>(true, dbType, connectionString, moreSettings, configAction);

    public static ISqlSugarClient GetSqlSugarClient(bool scope, DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) => GetSqlSugarClient<IgnoreAttribute>(scope, dbType, connectionString, tableEnumIsString, configAction);

    //public static ISqlSugarClient GetSqlSugarClient<IgnoreT>(bool scope, DbType dbType, string connectionString, bool tableEnumIsString = true, DbType? databaseModel = null, Action<SqlSugarClient>? configAction = null) where IgnoreT : Attribute
    public static ISqlSugarClient GetSqlSugarClient<IgnoreT>(bool scope, DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) where IgnoreT : Attribute
    {
        var _moreSettings = new ConnMoreSettings
        {
            SqlServerCodeFirstNvarchar = dbType == DbType.SqlServer,
            TableEnumIsString = tableEnumIsString//枚举以字符串的方式存储
        };
        return GetSqlSugarClient<IgnoreT>(scope, dbType, connectionString, _moreSettings, configAction);
    }
    public static ISqlSugarClient GetSqlSugarClient<IgnoreT>(bool scope, DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) where IgnoreT : Attribute
    {
        var _moreSettings = new ConnMoreSettings
        {
            SqlServerCodeFirstNvarchar = dbType == DbType.SqlServer
        };
        moreSettings?.Invoke(_moreSettings);
        return GetSqlSugarClient<IgnoreT>(scope, dbType, connectionString, _moreSettings, configAction);
    }
    public static ISqlSugarClient GetSqlSugarClient<IgnoreT>(bool scope, DbType dbType, string connectionString, ConnMoreSettings moreSettings, Action<SqlSugarClient>? configAction = null) where IgnoreT : Attribute
    {
        var connectionConfig = new ConnectionConfig
        {
            ConnectionString = connectionString,//连接符字串
            DbType = dbType, //数据库类型
            IsAutoCloseConnection = true,//不设成true要手动close
            ConfigureExternalServices = new ConfigureExternalServices
            {
                //注意:  这儿AOP设置不能少
                EntityService = (property, column) =>
                {
                    var sugarColumn = property.GetCustomAttribute<SugarColumn>();
                    var description = sugarColumn?.ColumnDescription ?? property.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName ?? property.GetCustomAttribute<DescriptionAttribute>(true)?.Description;

                    if (!string.IsNullOrWhiteSpace(description)) column.ColumnDescription = description;

                    var defaultValue = sugarColumn?.DefaultValue ?? property.GetCustomAttribute<DefaultValueAttribute>(true)?.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(defaultValue)) column.DefaultValue = defaultValue?.ToSqlValue();

                    var stringLength = sugarColumn?.Length ?? property.GetCustomAttribute<StringLengthAttribute>(true)?.MaximumLength;
                    if (stringLength.HasValue) column.Length = stringLength.Value;

                    //p.DbColumnName = UtilMethods.ToUnderLine(p.DbColumnName);//ToUnderLine驼峰转下划线方法

                    // int?  decimal?这种 isnullable=true 不支持string
                    if (sugarColumn?.IsNullable != false && property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        column.IsNullable = true;
                    }
                    else if (sugarColumn?.IsNullable != false && property.PropertyType == typeof(string) && IsNullable(property))
                    {//高版C#写法 支持string?和string  
                        column.IsNullable = true;
                    }
                    //忽略属性
                    if (sugarColumn?.IsIgnore != false && (property.GetCustomAttribute<IgnoreT>() != null))
                    {
                        column.IsIgnore = true;
                    }

                    if (sugarColumn?.DecimalDigits == null && (property.PropertyType == typeof(decimal) || (property.PropertyType.IsGenericType && property.PropertyType.GenericTypeArguments.FirstOrDefault() == typeof(decimal))))
                    {
                        column.DecimalDigits = 4;
                        column.Length = 18;
                    }

                    // 枚举
                    if (moreSettings.TableEnumIsString == true && (property.PropertyType.IsEnum || (property.PropertyType.IsGenericType && property.PropertyType.GenericTypeArguments.FirstOrDefault().IsEnum)))
                    {
                        column.DataType = "varchar";
                        //column.SqlParameterDbType = typeof(EnumToStringConvert);
                        column.Length = Enum.GetNames(property.PropertyType.IsGenericType ? property.PropertyType.GenericTypeArguments.FirstOrDefault() : property.PropertyType).Max(f => f.Length) + 2;
                    }

                    if (sugarColumn?.IsPrimaryKey == null && column.PropertyName.ToLower() == "id") //是id的设为主键
                    {
                        column.IsNullable = false;
                        column.IsPrimarykey = true;
                        if (column.PropertyInfo.PropertyType == typeof(int)) //是id并且是int的是自增
                        {
                            column.IsIdentity = true;
                        }
                    }
                    else if (sugarColumn?.IsPrimaryKey == null && property.GetCustomAttribute<KeyAttribute>(true) != null)
                    {
                        column.IsNullable = false;
                        column.IsPrimarykey = true;
                        if (column.PropertyInfo.PropertyType == typeof(int)) //是id并且是int的是自增
                        {
                            column.IsIdentity = true;
                        }
                    }
                },
                EntityNameService = (x, p) => //处理表名
                {
                    //p.IsDisabledUpdateAll = true;//禁止更新+删除
                    p.IsDisabledDelete = true;//禁止删除列

                    var description = x.GetCustomAttribute<DescriptionAttribute>(true)?.Description;
                    if (description != null) p.TableDescription = description;

                    var tableName = x.GetCustomAttribute<TableAttribute>(true)?.Name;
                    if (tableName != null) p.DbTableName = tableName;

                    //p.DbTableName = UtilMethods.ToUnderLine(p.DbTableName);//ToUnderLine驼峰转下划线方法
                }
            },
            MoreSettings = moreSettings
        };

        var defaultConfigAction = (SqlSugarClient db) =>
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
             };
             db.Aop.OnDiffLogEvent = (diff) =>
             {
             };
         };

        ISqlSugarClient client = scope ?
            new SqlSugarClient(connectionConfig, configAction ?? defaultConfigAction) :
            new SqlSugarScope(connectionConfig, configAction ?? defaultConfigAction);
        return client;
    }

    private static bool IsNullable(PropertyInfo property)
    {
#if NET6_0_OR_GREATER
        return new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable;
#else
        //https://github.com/dotnet/roslyn/blob/main/docs/features/nullable-metadata.md

        var nullable = property.CustomAttributes
            .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                {
                    return (byte)args[0].Value == 2;
                }
            }
            else if (attributeArgument.ArgumentType == typeof(byte))
            {
                return (byte)attributeArgument.Value == 2;
            }
        }

        var context = property.ReflectedType.CustomAttributes
            .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
        if (context != null &&
            context.ConstructorArguments.Count == 1 &&
            context.ConstructorArguments[0].ArgumentType == typeof(byte))
        {
            return (byte)context.ConstructorArguments[0].Value == 2;
        }

        // Couldn't find a suitable attribute
        return false;
#endif
    }
}
public class IgnoreAttribute : Attribute { }