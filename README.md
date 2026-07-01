# SqlSugarUtility

[![Build status](https://github.com/mccj/SqlSugarUtility/actions/workflows/build.yml/badge.svg)](https://github.com/mccj/SqlSugarUtility/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/SqlSugarUtility.svg)](https://www.nuget.org/packages/SqlSugarUtility)
[![NuGet Extensions](https://img.shields.io/nuget/v/SqlSugarUtility.Extensions.svg?label=SqlSugarUtility.Extensions)](https://www.nuget.org/packages/SqlSugarUtility.Extensions)
[![MIT License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)

基于 [SqlSugar](https://www.donet5.com/Home/Doc) 的实用工具库，简化客户端创建、实体特性映射、雪花 ID 配置，以及常用查询扩展。

**目标框架：** `net48` · `netstandard2.1`

---

## 包说明

| NuGet 包 | 说明 |
|----------|------|
| [SqlSugarUtility](https://www.nuget.org/packages/SqlSugarUtility) | 核心库：客户端工厂、Yitter 雪花 ID、基于 .NET 标准特性的实体列映射 |
| [SqlSugarUtility.Extensions](https://www.nuget.org/packages/SqlSugarUtility.Extensions) | 扩展库：分页结果封装、`SimpleClient` / `ISugarQueryable` 便捷方法（依赖核心包） |

安装时需 **两类** NuGet 包，职责不同：

| 类型 | 是否自动带入 | 说明 |
|------|--------------|------|
| **SqlSugar**（ORM 本体） | 否，须自行安装 | 本库以 `PrivateAssets` 引用，版本由你的项目控制 |
| **SqlSugarUtility** / **Extensions** | Extensions 会自动引用核心包 | 推荐只装 `SqlSugarUtility.Extensions` |

按目标框架选择 SqlSugar 包：

| 目标框架 | SqlSugar 包 |
|----------|-------------|
| .NET Core / .NET 5+ / `netstandard2.1` | [SqlSugarCore](https://www.nuget.org/packages/SqlSugarCore) |
| .NET Framework 4.8 | [SqlSugar](https://www.nuget.org/packages/SqlSugar) |

```bash
# .NET / netstandard2.1 项目（推荐组合）
dotnet add package SqlSugarCore
dotnet add package SqlSugarUtility.Extensions

# 仅核心功能（不含分页扩展）
dotnet add package SqlSugarCore
dotnet add package SqlSugarUtility

# .NET Framework 4.8 项目
dotnet add package SqlSugar
dotnet add package SqlSugarUtility.Extensions
```

---

## 快速开始

### 1. 封装 SqlSugarHelper（推荐）

将客户端创建、连接校验与 AOP 集中在一处，业务代码通过 `SqlSugarHelper.Db` 访问即可。
`GetSingletonSqlSugarClient` 底层使用 `SqlSugarScope`，适合 Web / 后台等全局共享场景。

```csharp
using SqlSugar;

public static class SqlSugarHelper
{
    private static ISqlSugarClient? _client;

    public static ISqlSugarClient DbClient
    {
        get
        {
            if (_client == null)
            {
                var dbType = DbType.SqlServer;
                const string connectionString = "Server=.;Database=Demo;Trusted_Connection=True;";
                // var connectionString = "Data Source=192.168.x.x;Initial Catalog=xxx;User ID=sa;PWD=xxx";
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new InvalidOperationException("连接字符串不能为空");

                // 泛型参数可指定“忽略列”标记类型，默认 IgnoreAttribute（即 [Ignore]）
                _client = SqlSugarUtility.GetSingletonSqlSugarClient<IgnoreAttribute>(
                    dbType,
                    connectionString,
                    tableEnumIsString: false, // false：枚举存数值；true（默认）：存字符串
                    configAction: db =>
                    {
                        // (A) 全局生效配置点，AOP 与程序启动配置放这里，所有上下文生效
                        // 调试 SQL 事件，生产环境可删掉或留空
                        db.Aop.OnLogExecuting = (sql, pars) =>
                        {
#if DEBUG
                            // Console.WriteLine(sql); // 输出原始 SQL，对性能无影响

                            // 5.0.8.2+ 获取无参数化 SQL（参数多时略有性能影响，仅调试使用）
                            var sql2 = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, sql, pars);
                            Console.WriteLine(sql2);
                            System.Diagnostics.Debug.WriteLine(sql2);
#endif
                        };
                        db.Aop.OnError = (ex) =>
                        {
                            Console.WriteLine(ex.Message);
                            System.Diagnostics.Debug.Fail(ex.Message);
                        };
                        db.Aop.OnDiffLogEvent = (diff) =>
                        {
                            // 差异日志（用于审计、数据变更追踪等）
                        };
                    });
            }
            return _client;
        }
    }
}
```

> 库已内置 `[Ignore]` 特性（`SqlSugar.IgnoreAttribute`），实体上标注即可忽略列映射，无需再自定义 `IgnoreAttribute` 类。
>
> 若需自定义忽略标记类型，将泛型参数改为你的特性即可：`GetSingletonSqlSugarClient<NotMappedAttribute>(...)`。

### 2. 快速创建（脚本或临时调用）

不需要封装 Helper 时，可直接创建客户端（未传 `configAction` 时，库在 DEBUG 下会默认输出 SQL）：

```csharp
using SqlSugar;

var db = SqlSugarUtility.GetSingletonSqlSugarClient(
    DbType.SqlServer,
    "Server=.;Database=Demo;Trusted_Connection=True;",
    tableEnumIsString: true,
    configAction: db =>
    {
        db.Aop.OnLogExecuting = (sql, pars) =>
        {
#if DEBUG
            var sql2 = UtilMethods.GetSqlString(db.CurrentConnectionConfig.DbType, sql, pars);
            System.Diagnostics.Debug.WriteLine(sql2);
#endif
        };
        db.Aop.OnError = (ex) => System.Diagnostics.Debug.WriteLine(ex.Message);
    });
```

线程安全单例也可用 `Lazy<T>`（适合无需自定义 AOP 的场景）：

```csharp
public static class SqlSugarHelper
{
    private static readonly Lazy<ISqlSugarClient> _client = new(() =>
        SqlSugarUtility.GetSingletonSqlSugarClient(
            DbType.SqlServer,
            connectionString: "Server=.;Database=Demo;Trusted_Connection=True;"));

    public static ISqlSugarClient Db => _client.Value;
}
```

### 3. 定义实体并建表

```csharp
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SqlSugar;

[Table("sys_user")]
[Description("系统用户")]
public class User
{
    public int Id { get; set; }              // 名为 Id 的 int 字段自动识别为主键、自增

    [DisplayName("user_name")]
    [StringLength(64)]
    public string UserName { get; set; } = "";

    [Description("昵称")]
    public string? NickName { get; set; }  // string? 自动识别为可空列

    [EnumColumn]
    public UserStatus Status { get; set; }   // 枚举按字符串存储

    [Json]
    public UserProfile? Profile { get; set; } // JSON 列

    [Ignore]
    public string? TempField { get; set; }   // 不映射到数据库
}

public enum UserStatus { Active, Disabled }

// Code First 建表
SqlSugarHelper.Db.CodeFirst.InitTables<User>();
```

---

## 客户端工厂

`SqlSugarUtility` 提供两类客户端创建方式：

| 方法 | 底层类型 | 适用场景 |
|------|----------|----------|
| `GetSingletonSqlSugarClient` | `SqlSugarScope` | Web API、后台服务等全局共享连接（**推荐**） |
| `GetScopeSqlSugarClient` | `SqlSugarClient` | 每次调用创建新实例，适合短生命周期或特殊隔离场景 |

所有重载均支持：

- `tableEnumIsString`：全局将枚举存为字符串（默认 `true`）
- `Action<ConnMoreSettings> moreSettings`：细粒度连接配置
- `Action<SqlSugarClient> configAction`：AOP、日志等自定义配置
- 泛型 `TIgnore`：自定义“忽略列”标记类型（默认 `IgnoreAttribute`）

```csharp
// 自定义 MoreSettings
var db = SqlSugarUtility.GetSingletonSqlSugarClient(
    DbType.MySql,
    connectionString,
    moreSettings: s =>
    {
        s.TableEnumIsString = false;
        s.IsWithNoLockQuery = true;
    });

// 使用自定义忽略特性
public sealed class NotMappedAttribute : Attribute { }

var db2 = SqlSugarUtility.GetSingletonSqlSugarClient<NotMappedAttribute>(
    DbType.SqlServer,
    connectionString);
```

**默认行为：**

- `IsAutoCloseConnection = true`（自动关闭连接）
- SQL Server 下 `SqlServerCodeFirstNvarchar = true`
- DEBUG 模式下自动输出完整 SQL 到控制台 / 调试窗口
- 全局禁止删列（`IsDisabledDelete = true`）

---

## 雪花 ID（Yitter）

库在静态构造函数中默认初始化 Yitter 雪花 ID，并替换 SqlSugar 内置算法：

```csharp
SqlSugarUtility.SetIdGenerator(new IdGeneratorOptions
{
    WorkerId = 1,
    WorkerIdBitLength = 6,
    SeqBitLength = 6
});
```

实体主键使用 `long` 并配置 `SnowFlakeSingle` 即可生成 ID（与 SqlSugar 官方用法一致）。

---

## 实体特性映射

除 SqlSugar 原生 `[SugarColumn]` 外，以下 .NET / 自定义特性会在 **Code First** 时自动映射为列配置（`SugarColumn` 显式设置优先）：

### 标准 .NET 特性

| 特性 | 映射效果 |
|------|----------|
| `[Table("name")]` | 表名 |
| `[Description("...")]` | 表 / 列描述 |
| `[DisplayName("col")]` | 列名 |
| `[DefaultValue(...)]` | 默认值 |
| `[StringLength(n)]` | 字符串长度 |
| `[Key]` | 主键；`int` 类型同时设为自增 |
| `Nullable<T>` / `string?` | 可空列 |

### SqlSugarUtility 自定义特性

| 特性 | 映射效果 |
|------|----------|
| `[Ignore]` | 忽略该属性，不生成列 |
| `[EnumColumn]` | 该枚举列按 `varchar` 存字符串 |
| `[Identity]` | 自增列 |
| `[OnlyIgnoreInsert]` | 插入时忽略 |
| `[OnlyIgnoreUpdate]` | 更新时忽略 |
| `[EnableUpdateVersionValidation]` | 乐观锁版本字段 |
| `[TreeKey]` | 树形结构主键 |
| `[Json]` | JSON 列（可空） |
| `[Transcoding]` | 转码列（如 Emoji） |
| `[SqlParameterDbType(typeof(...))]` | 自定义参数 DbType |

### 自动推断规则

- 属性名为 `Id`（不区分大小写）→ 主键；`int` 类型 → 自增
- `decimal` / `decimal?` → `Length = 18`，`DecimalDigits = 4`
- `[EnumColumn]` 的枚举 → 列长 = 最长枚举名 + 2

---

## 扩展方法（SqlSugarUtility.Extensions）

引用扩展包后，以下方法在 `System.Linq` 命名空间下可用（与 `SqlSugar` 一起 `using` 即可）。

### 分页 `ToPagedList`

返回统一的 `SqlSugarPagedList<T>`，包含总条数、总页数、上下页标记：

```csharp
using SqlSugar;

// 同步分页
var page = await db.Queryable<Order>()
    .Where(o => o.Status == OrderStatus.Paid)
    .OrderBy(o => o.CreateTime, OrderByType.Desc)
    .ToPagedListAsync(pageIndex: 1, pageSize: 20);

Console.WriteLine($"共 {page.Total} 条，第 {page.Page}/{page.TotalPages} 页");
foreach (var order in page.Items!)
{
    // ...
}

// 分页 + 投影
var dtoPage = db.Queryable<User>()
    .ToPagedList(1, 10, u => new UserDto { Id = u.Id, Name = u.UserName });

// 内存集合分页（不访问数据库）
var memPage = list.ToPagedList(1, 10);
```

`SqlSugarPagedList<T>` 属性：

| 属性 | 说明 |
|------|------|
| `Page` | 当前页码（从 1 开始） |
| `PageSize` | 每页条数 |
| `Total` | 总记录数 |
| `TotalPages` | 总页数 |
| `Items` | 当前页数据（只读集合） |
| `HasPrevPage` / `HasNextPage` | 是否有上 / 下一页 |

### SimpleClient 便捷方法

```csharp
var client = db.GetSimpleClient<User>();

int count = client.Count();
int countAsync = await client.CountAsync();

User[] all = client.GetArray();
User[] active = client.GetArray(u => u.Status == UserStatus.Active);
User[] allAsync = await client.GetArrayAsync();
```

### ISugarQueryable 分页辅助

对 SqlSugar 原生 `ref int` 分页 API 的 `out int` 包装，以及数组化结果：

```csharp
var query = db.Queryable<Order>().Where(o => o.Amount > 0);

var rows = query.ToPageList(1, 20, out int total);
var arr = query.ToPageArray(1, 20, out total);
var json = query.ToJsonPage(1, 20, out total);
var table = query.ToDataTablePage(1, 20, out total);

// 大批量逐页处理
query.ForEachByPage(item => Process(item), 1, 500, out total);
```

---

## 与 ASP.NET Core 集成示例

```csharp
// Program.cs
builder.Services.AddSingleton<ISqlSugarClient>(_ =>
    SqlSugarUtility.GetSingletonSqlSugarClient(
        DbType.SqlServer,
        builder.Configuration.GetConnectionString("Default")!));

// 业务代码
public class UserService(ISqlSugarClient db)
{
    public async Task<SqlSugarPagedList<User>> GetUsersAsync(int page, int size) =>
        await db.Queryable<User>()
            .OrderBy(u => u.Id)
            .ToPagedListAsync(page, size);
}
```

---

## 本地构建

```bash
git clone https://github.com/mccj/SqlSugarUtility.git
cd SqlSugarUtility
dotnet build src/SqlSugarUtility.Extensions/SqlSugarUtility.Extensions.csproj -c Release
```

构建产物包含 `.nupkg` 与符号包 `.snupkg`。

---

## 相关链接

- [SqlSugar 官方文档](https://www.donet5.com/Home/Doc)
- [Yitter.IdGenerator](https://github.com/yitter/idgenerator)
- [问题反馈](https://github.com/mccj/SqlSugarUtility/issues)

## 许可证

[MIT](LICENSE)
