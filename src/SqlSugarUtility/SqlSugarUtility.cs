using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

using Yitter.IdGenerator;

namespace SqlSugar;

/// <summary>
/// SqlSugar 数据库访问工具类，提供 SqlSugarClient 的创建与配置、雪花ID生成器设置以及实体特性映射的扩展功能。
/// </summary>
public static class SqlSugarUtility
{
    /// <summary>
    /// 静态构造函数，初始化默认的雪花ID生成器配置（机器码位长6，序列数位长6）。
    /// </summary>
    static SqlSugarUtility()
    {
        SetIdGenerator(new IdGeneratorOptions
        {
            WorkerId = 1,
            WorkerIdBitLength = 6, // 机器码位长，默认值6，取值范围 [1, 19]
            SeqBitLength = 6       // 序列数位长，默认值6，取值范围 [3, 21]（建议不小于4，值越大性能越高、Id位数也更长）
        });
    }

    /// <summary>
    /// 设置雪花ID生成器选项，并替换 SqlSugar 默认的雪花ID生成算法为 YitIdHelper 的实现。
    /// </summary>
    /// <param name="snowIdOpt">雪花ID生成器配置选项，若为 null 则使用默认 WorkerId = 1。</param>
    public static void SetIdGenerator(IdGeneratorOptions snowIdOpt)
    {
        YitIdHelper.SetIdGenerator(snowIdOpt);
        SnowFlakeSingle.WorkId = snowIdOpt?.WorkerId ?? 1;

        // 自定义 SqlSugar 雪花ID算法
        StaticConfig.CustomSnowFlakeFunc = YitIdHelper.NextId;
    }

    /// <summary>
    /// 获取单例模式的 SqlSugar 客户端（SqlSugarScope）。
    /// </summary>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="tableEnumIsString">是否将枚举类型映射为字符串（默认 true）。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例（单例模式）。</returns>
    public static ISqlSugarClient GetSingletonSqlSugarClient(DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) => GetSingletonSqlSugarClient<IgnoreAttribute>(dbType, connectionString, tableEnumIsString, configAction);

    /// <summary>
    /// 获取单例模式的 SqlSugar 客户端（SqlSugarScope），并允许对 MoreSettings 进行额外配置。
    /// </summary>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="moreSettings">对 ConnMoreSettings 的配置委托。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例（单例模式）。</returns>
    public static ISqlSugarClient GetSingletonSqlSugarClient(DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) => GetSingletonSqlSugarClient<IgnoreAttribute>(dbType, connectionString, moreSettings, configAction);

    /// <summary>
    /// 获取单例模式的 SqlSugar 客户端（SqlSugarScope），并支持自定义忽略属性标记类型。
    /// </summary>
    /// <typeparam name="TIgnore">自定义忽略属性的标记类型（需继承 Attribute），默认为 IgnoreAttribute。</typeparam>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="tableEnumIsString">是否将枚举类型映射为字符串（默认 true）。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例（单例模式）。</returns>
    public static ISqlSugarClient GetSingletonSqlSugarClient<TIgnore>(DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) where TIgnore : Attribute => GetSqlSugarClient<TIgnore>(false, dbType, connectionString, tableEnumIsString, configAction);

    /// <summary>
    /// 获取单例模式的 SqlSugar 客户端（SqlSugarScope），支持自定义忽略属性标记类型并允许对 MoreSettings 进行额外配置。
    /// </summary>
    /// <typeparam name="TIgnore">自定义忽略属性的标记类型（需继承 Attribute），默认为 IgnoreAttribute。</typeparam>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="moreSettings">对 ConnMoreSettings 的配置委托。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例（单例模式）。</returns>
    public static ISqlSugarClient GetSingletonSqlSugarClient<TIgnore>(DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) where TIgnore : Attribute => GetSqlSugarClient<TIgnore>(false, dbType, connectionString, moreSettings, configAction);

    /// <summary>
    /// 获取作用域（Scope）模式的 SqlSugar 客户端（SqlSugarClient，每次调用创建新实例）。
    /// </summary>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="tableEnumIsString">是否将枚举类型映射为字符串（默认 true）。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例（作用域模式）。</returns>
    public static ISqlSugarClient GetScopeSqlSugarClient(DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) => GetScopeSqlSugarClient<IgnoreAttribute>(dbType, connectionString, tableEnumIsString, configAction);

    /// <summary>
    /// 获取作用域（Scope）模式的 SqlSugar 客户端（SqlSugarClient），并允许对 MoreSettings 进行额外配置。
    /// </summary>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="moreSettings">对 ConnMoreSettings 的配置委托。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例（作用域模式）。</returns>
    public static ISqlSugarClient GetScopeSqlSugarClient(DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) => GetScopeSqlSugarClient<IgnoreAttribute>(dbType, connectionString, moreSettings, configAction);

    /// <summary>
    /// 获取作用域（Scope）模式的 SqlSugar 客户端（SqlSugarClient），支持自定义忽略属性标记类型。
    /// </summary>
    /// <typeparam name="TIgnore">自定义忽略属性的标记类型（需继承 Attribute），默认为 IgnoreAttribute。</typeparam>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="tableEnumIsString">是否将枚举类型映射为字符串（默认 true）。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例（作用域模式）。</returns>
    public static ISqlSugarClient GetScopeSqlSugarClient<TIgnore>(DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) where TIgnore : Attribute => GetSqlSugarClient<TIgnore>(true, dbType, connectionString, tableEnumIsString, configAction);

    /// <summary>
    /// 获取作用域（Scope）模式的 SqlSugar 客户端（SqlSugarClient），支持自定义忽略属性标记类型并允许对 MoreSettings 进行额外配置。
    /// </summary>
    /// <typeparam name="TIgnore">自定义忽略属性的标记类型（需继承 Attribute），默认为 IgnoreAttribute。</typeparam>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="moreSettings">对 ConnMoreSettings 的配置委托。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例（作用域模式）。</returns>
    public static ISqlSugarClient GetScopeSqlSugarClient<TIgnore>(DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) where TIgnore : Attribute => GetSqlSugarClient<TIgnore>(true, dbType, connectionString, moreSettings, configAction);

    /// <summary>
    /// 获取 SqlSugar 客户端，根据 scope 参数决定返回单例（SqlSugarScope）或非单例（SqlSugarClient）实例。
    /// </summary>
    /// <param name="scope">true 表示作用域模式（每次新建 SqlSugarClient），false 表示单例模式（SqlSugarScope）。</param>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="tableEnumIsString">是否将枚举类型映射为字符串（默认 true）。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例。</returns>
    public static ISqlSugarClient GetSqlSugarClient(bool scope, DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) => GetSqlSugarClient<IgnoreAttribute>(scope, dbType, connectionString, tableEnumIsString, configAction);

    /// <summary>
    /// 获取 SqlSugar 客户端（内部核心创建方法），支持自定义忽略属性标记类型，并自动设置默认的 MoreSettings。
    /// </summary>
    /// <typeparam name="TIgnore">自定义忽略属性的标记类型（需继承 Attribute），默认为 IgnoreAttribute。</typeparam>
    /// <param name="scope">true 表示作用域模式（每次新建 SqlSugarClient），false 表示单例模式（SqlSugarScope）。</param>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="tableEnumIsString">是否将枚举类型映射为字符串（默认 true）。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例。</returns>
    public static ISqlSugarClient GetSqlSugarClient<TIgnore>(bool scope, DbType dbType, string connectionString, bool tableEnumIsString = true, Action<SqlSugarClient>? configAction = null) where TIgnore : Attribute
    {
        var _moreSettings = new ConnMoreSettings
        {
            SqlServerCodeFirstNvarchar = dbType == DbType.SqlServer, // SqlServer 使用 nvarchar
            TableEnumIsString = tableEnumIsString                    // 枚举以字符串方式存储
        };
        return GetSqlSugarClient<TIgnore>(scope, dbType, connectionString, _moreSettings, configAction);
    }

    /// <summary>
    /// 获取 SqlSugar 客户端（内部核心创建方法），支持自定义忽略属性标记类型和通过委托配置 MoreSettings。
    /// </summary>
    /// <typeparam name="TIgnore">自定义忽略属性的标记类型（需继承 Attribute），默认为 IgnoreAttribute。</typeparam>
    /// <param name="scope">true 表示作用域模式（每次新建 SqlSugarClient），false 表示单例模式（SqlSugarScope）。</param>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="moreSettings">对 ConnMoreSettings 的配置委托。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例。</returns>
    public static ISqlSugarClient GetSqlSugarClient<TIgnore>(bool scope, DbType dbType, string connectionString, Action<ConnMoreSettings> moreSettings, Action<SqlSugarClient>? configAction = null) where TIgnore : Attribute
    {
        var _moreSettings = new ConnMoreSettings
        {
            SqlServerCodeFirstNvarchar = dbType == DbType.SqlServer
        };
        moreSettings?.Invoke(_moreSettings);
        return GetSqlSugarClient<TIgnore>(scope, dbType, connectionString, _moreSettings, configAction);
    }
    /// <summary>
    /// 获取 SqlSugar 客户端（内部核心创建方法），支持自定义忽略属性标记类型和直接传入 ConnMoreSettings 对象。
    /// 此方法完成了 ConnectionConfig 的构建、实体特性的解析（如 Description、DisplayName、DefaultValue 等）以及 AOP 的默认配置。
    /// </summary>
    /// <typeparam name="TIgnore">自定义忽略属性的标记类型（需继承 Attribute），默认为 IgnoreAttribute。</typeparam>
    /// <param name="scope">true 表示作用域模式（每次新建 SqlSugarClient），false 表示单例模式（SqlSugarScope）。</param>
    /// <param name="dbType">数据库类型。</param>
    /// <param name="connectionString">数据库连接字符串。</param>
    /// <param name="moreSettings">已配置的 ConnMoreSettings 对象。</param>
    /// <param name="configAction">可选的客户端配置委托，用于设置 AOP 等。</param>
    /// <returns>ISqlSugarClient 实例。</returns>
    public static ISqlSugarClient GetSqlSugarClient<TIgnore>(bool scope, DbType dbType, string connectionString, ConnMoreSettings moreSettings, Action<SqlSugarClient>? configAction = null) where TIgnore : Attribute
    {
        var connectionConfig = new ConnectionConfig
        {
            ConnectionString = connectionString,                  // 连接字符串
            DbType = dbType,                                      // 数据库类型
            IsAutoCloseConnection = true,                         // 自动关闭连接（无需手动 Close）
            ConfigureExternalServices = new ConfigureExternalServices
            {
                // 实体属性映射配置：从特性中读取列信息
                EntityService = (property, column) =>
                {
                    var sugarColumn = property.GetCustomAttribute<SugarColumn>();

                    // 忽略属性（如果标记了 TIgnore 或 SugarColumn.IsIgnore == true）
                    if (sugarColumn?.IsIgnore != false && (property.GetCustomAttribute<TIgnore>() != null))
                    {
                        column.IsIgnore = true;
                    }

                    // 列描述：优先使用 SugarColumn.ColumnDescription，其次使用 DescriptionAttribute
                    var description = sugarColumn?.ColumnDescription ?? property.GetCustomAttribute<DescriptionAttribute>(true)?.Description;
                    if (!string.IsNullOrWhiteSpace(description)) column.ColumnDescription = description;

                    // 字段名称：优先使用 SugarColumn.ColumnName，其次使用 DisplayNameAttribute
                    var columnName = sugarColumn?.ColumnName ?? property.GetCustomAttribute<DisplayNameAttribute>(true)?.DisplayName;
                    if (!string.IsNullOrWhiteSpace(columnName)) column.DbColumnName = columnName;
                    //p.DbColumnName = UtilMethods.ToUnderLine(p.DbColumnName);//ToUnderLine驼峰转下划线方法

                    // 默认值：优先使用 SugarColumn.DefaultValue，其次使用 DefaultValueAttribute
                    var defaultValue = sugarColumn?.DefaultValue ?? property.GetCustomAttribute<DefaultValueAttribute>(true)?.Value?.ToString();
                    if (!string.IsNullOrWhiteSpace(defaultValue)) column.DefaultValue = defaultValue;//?.ToSqlValue();

                    // 字段长度：优先使用 SugarColumn.Length，其次使用 StringLengthAttribute
                    var stringLength = sugarColumn?.Length ?? property.GetCustomAttribute<StringLengthAttribute>(true)?.MaximumLength;
                    if (stringLength.HasValue) column.Length = stringLength.Value;

                    // 可空类型处理：如果属性是 Nullable<T> 或 string?（通过 IsNullable 检测），则标记为可空
                    if (sugarColumn?.IsNullable != false && property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        column.IsNullable = true;
                    }
                    else if (sugarColumn?.IsNullable != false && property.PropertyType == typeof(string) && IsNullable(property))
                    {//高版C#写法 支持string?和string  
                        column.IsNullable = true;
                    }

                    // 货币数字类型默认精度：decimal 或 Nullable<decimal> 默认 DecimalDigits=4, Length=18
                    if (sugarColumn?.DecimalDigits == null && (property.PropertyType == typeof(decimal) || (property.PropertyType.IsGenericType && property.PropertyType.GenericTypeArguments.FirstOrDefault() == typeof(decimal))))
                    {
                        column.DecimalDigits = 4;
                        column.Length = 18;
                    }

                    // 枚举字符串存储：如果标记了 EnumColumnAttribute 且属性为枚举或可空枚举，则映射为 varchar，长度取枚举名称最大长度+2
                    if (property.GetCustomAttributes<EnumColumnAttribute>(true).Any() && /*moreSettings.TableEnumIsString == true && */(property.PropertyType.IsEnum || (property.PropertyType.IsGenericType && property.PropertyType.GenericTypeArguments.FirstOrDefault().IsEnum)))
                    {
                        column.DataType = "varchar";
                        //column.SqlParameterDbType = typeof(SqlSugar.DbConvert.EnumToStringConvert);
                        column.Length = Enum.GetNames(property.PropertyType.IsGenericType ? property.PropertyType.GenericTypeArguments.FirstOrDefault() : property.PropertyType).Max(f => f.Length) + 2;
                    }

                    // 默认主键处理：属性名为 "id" 或标记了 KeyAttribute，则设为主键、不可空；若类型为 int 则设为自增
                    if (sugarColumn?.IsPrimaryKey == null && (column.PropertyName?.ToUpperInvariant() == "ID") || (property.GetCustomAttribute<KeyAttribute>(true) != null))
                    {
                        column.IsNullable = false;
                        column.IsPrimarykey = true;
                        if (column.PropertyInfo.PropertyType == typeof(int)) //是id并且是int的是自增
                        {
                            column.IsIdentity = true;
                        }
                    }

                    // 自增长属性：标记了 IdentityAttribute 则设为自增
                    if (sugarColumn?.IsIdentity != false && (property.GetCustomAttribute<IdentityAttribute>() != null))
                    {
                        column.IsIdentity = true;
                    }

                    // 仅插入忽略（插入时忽略该字段）
                    if (sugarColumn?.IsOnlyIgnoreInsert != false && (property.GetCustomAttribute<OnlyIgnoreInsertAttribute>() != null))
                    {
                        column.IsOnlyIgnoreInsert = true;
                    }

                    // 仅更新忽略（更新时忽略该字段）
                    if (sugarColumn?.IsOnlyIgnoreUpdate != false && (property.GetCustomAttribute<OnlyIgnoreUpdateAttribute>() != null))
                    {
                        column.IsOnlyIgnoreUpdate = true;
                    }

                    // 乐观锁版本字段
                    if (sugarColumn?.IsEnableUpdateVersionValidation != false && (property.GetCustomAttribute<EnableUpdateVersionValidationAttribute>() != null))
                    {
                        column.IsEnableUpdateVersionValidation = true;
                    }

                    // 树形结构主键
                    if (sugarColumn?.IsTreeKey != false && (property.GetCustomAttribute<TreeKeyAttribute>() != null))
                    {
                        column.IsTreeKey = true;
                    }

                    // JSON 类型字段
                    if (sugarColumn?.IsJson != false && (property.GetCustomAttribute<JsonAttribute>() != null))
                    {
                        column.IsJson = true;
                        column.IsNullable = true;
                    }

                    // 转码字段（如处理 Emoji 等）
                    if (sugarColumn?.IsTranscoding != false && (property.GetCustomAttribute<TranscodingAttribute>() != null))
                    {
                        column.IsTranscoding = true;
                    }

                    // 自定义 SqlParameter DbType
                    var sqlParameterDbType = sugarColumn?.SqlParameterDbType as Type ?? property.GetCustomAttribute<SqlParameterDbTypeAttribute>(true)?.DbType;
                    if (sqlParameterDbType != null) column.SqlParameterDbType = sqlParameterDbType;
                },

                // 实体表名映射配置：从特性中读取表名和描述
                EntityNameService = (x, p) =>
                {
                    //p.IsDisabledUpdateAll = true;//禁止更新+删除
                    p.IsDisabledDelete = true; // 禁止删除列（全局设置）

                    // 表描述：优先使用 DescriptionAttribute
                    var description = x.GetCustomAttribute<DescriptionAttribute>(true)?.Description;
                    if (description != null) p.TableDescription = description;

                    // 表名：优先使用 TableAttribute.Name
                    var tableName = x.GetCustomAttribute<TableAttribute>(true)?.Name;
                    if (tableName != null) p.DbTableName = tableName;

                    //p.DbTableName = UtilMethods.ToUnderLine(p.DbTableName);//ToUnderLine驼峰转下划线方法
                }
            },
            MoreSettings = moreSettings
        };

        // 默认的客户端配置：包含调试日志 AOP（仅在 DEBUG 模式下输出 SQL）
        var defaultConfigAction = (SqlSugarClient db) =>
        {
            // 全局 AOP 配置
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
#if DEBUG
                // 输出带参数值的 SQL（方便调试，注意性能影响）
                var sql2 = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, sql, pars);
                Console.WriteLine(sql2);
                System.Diagnostics.Debug.WriteLine(sql2);
#endif
            };
            db.Aop.OnError = (ex) =>
            {
                // 异常日志可在此处理
            };
            db.Aop.OnDiffLogEvent = (diff) =>
            {
                // 差异日志（用于审计等）
            };
        };

        // 根据 scope 参数返回对应实例
        ISqlSugarClient client = scope ?
            new SqlSugarClient(connectionConfig, configAction ?? defaultConfigAction) :
            new SqlSugarScope(connectionConfig, configAction ?? defaultConfigAction);
        return client;
    }

    /// <summary>
    /// 判断属性是否为可空类型（支持 .NET 6+ 的 Nullable 上下文和 .NET Standard 的传统方式）。
    /// </summary>
    /// <param name="property">要检查的 PropertyInfo。</param>
    /// <returns>如果属性可空则返回 true，否则 false。</returns>
    private static bool IsNullable(PropertyInfo property)
    {
#if NET6_0_OR_GREATER
        // .NET 6+ 使用 NullabilityInfoContext 判断
        return new NullabilityInfoContext().Create(property).WriteState is NullabilityState.Nullable;
#else
        // 低版本 .NET 通过 NullableAttribute 和 NullableContextAttribute 判断
        // 参考：https://github.com/dotnet/roslyn/blob/main/docs/features/nullable-metadata.md

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

        // 未找到相关特性
        return false;
#endif
    }
}