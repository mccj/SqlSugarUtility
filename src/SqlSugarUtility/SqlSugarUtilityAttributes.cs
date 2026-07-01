namespace SqlSugar;

/// <summary>
/// 用于标记实体属性在数据库映射中应被忽略。
/// </summary>
/// <remarks>
/// 当属性标记了 <see cref="IgnoreAttribute"/> 且未在 <see cref="SugarColumn"/> 中显式设置
/// <c>IsIgnore = false</c> 时，该属性不会映射为数据表列。
/// 可通过 <c>GetSingletonSqlSugarClient&lt;TIgnore&gt;</c> 的泛型参数替换为自定义忽略特性。
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public sealed class IgnoreAttribute : Attribute { }

/// <summary>
/// 标记枚举类型属性应以字符串形式存储于数据库。
/// 与 ConnMoreSettings.TableEnumIsString 类似，但此特性可单独控制某个属性。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class EnumColumnAttribute : Attribute { }

/// <summary>
/// 标记属性为数据库自增列（通常用于 int 主键）。
/// 若属性同时标记了 [Key] 且为 int 类型，也会自动设为自增，但使用此特性可显式指定。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class IdentityAttribute : Attribute { }

/// <summary>
/// 标记属性在插入操作时忽略（即不参与 INSERT 语句）。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OnlyIgnoreInsertAttribute : Attribute { }

/// <summary>
/// 标记属性在更新操作时忽略（即不参与 UPDATE 语句）。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class OnlyIgnoreUpdateAttribute : Attribute { }

/// <summary>
/// 标识该字段为乐观锁版本控制字段（用于并发控制）。
/// 通常配合 SqlSugar 的版本验证功能使用。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class EnableUpdateVersionValidationAttribute : Attribute { }

/// <summary>
/// 标记属性为树形结构中的主键字段（如递归查询时的关键标识）。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class TreeKeyAttribute : Attribute { }

/// <summary>
/// 标记属性应存储为 JSON 格式（数据库对应列通常为 nvarchar 或 text）。
/// 当使用 SqlSugar 的 Json 类型功能时，可自动序列化/反序列化。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class JsonAttribute : Attribute { }

/// <summary>
/// 标记属性需要进行转码处理（如存储 Emoji 或特殊字符）。
/// 具体转码逻辑需结合 SqlSugar 的全局配置或自定义处理。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class TranscodingAttribute : Attribute { }

/// <summary>
/// 指定属性对应的数据库参数类型（SqlParameter 的 DbType）。
/// 可用于需要精确控制参数类型的场景，如枚举转换、自定义类型映射等。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SqlParameterDbTypeAttribute : Attribute
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dbType">自定义的 DbType 类型（通常为实现 SqlSugar 的 IDbType 接口的类型）。</param>
    public SqlParameterDbTypeAttribute(Type dbType)
    {
        DbType = dbType;
    }

    /// <summary>
    /// 数据库参数类型
    /// </summary>
    public Type DbType { get; }
}