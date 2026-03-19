# XYPortal.PasswordBook 项目开发说明

## 项目位置
`D:\Repositories\XYPortal\XYPortal.PasswordBook`

## 业务需求

| 需求 | 说明 |
|------|------|
| 权限控制 | 仅拥有 `PASSWORDBOOKUSER` 权限的用户才能使用 PasswordBook 功能 |
| 密码本 | 用户可创建 1 个或多个 PasswordBook |
| 密码格式 | 每个 PasswordBook 有统一的密码格式要求 |
| 用户名 | 密码分为"有用户名"和"无用户名"两类 |
| 密码类型 | 密码分为"纯数字密码"和"一般密码" |
| 弱等级 | 一般密码需分弱等级 |
| 软删除 | PasswordBook 支持软删除和硬删除 |
| 密码历史 | 每个密码实体包含多个密码值，只有一个是有效的 |

## 项目结构

```
src/
├── XYPortal.PasswordBook.Domain/
│   ├── AggregateRoots/
│   │   └── PasswordBook.cs              # 聚合根
│   ├── Entities/
│   │   ├── PasswordEntry.cs             # 密码条目实体
│   │   └── PasswordHistory.cs           # 密码历史实体
│   ├── ValueObjects/
│   │   └── PasswordFormatRequirement.cs # 密码格式要求值对象
│   └── Services/
│       └── PasswordBookManager.cs       # DomainService
├── XYPortal.PasswordBook.Domain.Shared/
│   └── Enums/
│       ├── PasswordType.cs              # 密码类型枚举
│       └── PasswordWeakLevel.cs         # 弱等级枚举
├── XYPortal.PasswordBook.EntityFrameworkCore/
│   └── EntityFrameworkCore/
│       ├── IPasswordBookDbContext.cs
│       ├── PasswordBookDbContext.cs
│       └── PasswordBookDbContextModelCreatingExtensions.cs
└── XYPortal.PasswordBook.Application/
    └── PasswordBooks/
        ├── IPasswordBookAppService.cs    # 应用服务接口
        ├── PasswordBookAppService.cs     # 应用服务实现
        └── Dtos.cs                      # DTOs
```

## 代码详情

### 1. 枚举 (Domain.Shared/Enums)

**PasswordType.cs**
```csharp
public enum PasswordType
{
    NumericOnly = 0,  // 纯数字密码
    General = 1       // 一般密码
}
```

**PasswordWeakLevel.cs**
```csharp
public enum PasswordWeakLevel
{
    VeryWeak = 0,  // 非常弱
    Weak = 1,      // 弱
    Medium = 2,    // 中等
    Strong = 3,    // 强
    VeryStrong = 4 // 非常强
}
```

### 2. 值对象 (Domain/ValueObjects)

**PasswordFormatRequirement.cs**
- 属性：MinLength, MaxLength, RequireUppercase, RequireLowercase, RequireDigit, RequireSpecialChar, SpecialChars, AllowedTypes
- 方法：Validate(password) 返回 (bool IsValid, string? ErrorMessage)
- 静态工厂：DefaultNumeric, DefaultGeneral

### 3. 实体 (Domain/Entities)

**PasswordEntry.cs**
- 所属密码本ID、标题、是否有用户名、用户名、密码类型、弱等级、当前密码、备注
- 创建时间、最后修改时间、是否已删除
- 包含 PasswordHistory 集合
- 方法：UpdatePassword, UpdateInfo, SoftDelete, Restore

**PasswordHistory.cs**
- 密码条目ID、密码值、是否当前有效、创建时间
- 方法：MarkAsInvalid

### 4. 聚合根 (Domain/AggregateRoots)

**PasswordBook.cs**
- 所有者ID、名称、描述、密码格式JSON
- 创建时间、最后修改时间、是否已删除、删除时间
- 包含 PasswordEntry 集合
- 方法：
  - AddPasswordEntry - 添加密码条目
  - UpdatePasswordEntry - 更新密码条目信息
  - UpdatePasswordValue - 更新密码值
  - RemovePasswordEntry - 软删除密码条目
  - RestorePasswordEntry - 恢复密码条目
  - HardDeletePasswordEntry - 硬删除密码条目
  - UpdateInfo - 更新密码本信息
  - UpdatePasswordFormat - 更新密码格式要求
  - SoftDelete - 软删除
  - Restore - 恢复
  - HardDelete - 硬删除

### 5. DomainService (Domain/Services)

**PasswordBookManager.cs**
- CreateAsync - 创建密码本
- GetListByOwnerAsync - 获取用户密码本列表
- GetByIdAsync - 获取密码本详情
- SoftDeleteAsync - 软删除
- RestoreAsync - 恢复
- HardDeleteAsync - 硬删除
- EvaluatePasswordStrength - 评估密码强度
- HasAccessPermissionAsync - 检查访问权限

### 6. EntityFrameworkCore 配置

**PasswordBookDbContextModelCreatingExtensions.cs**
- 配置 PasswordBook、PasswordEntry、PasswordHistory 实体映射
- 表名前缀、索引、字段属性

### 7. 权限定义 (Application.Contracts/Permissions)

**PasswordBookPermissionDefinitionProvider.cs**
```csharp
public static class PasswordBookPermissions
{
    public const string GroupName = "PasswordBook";
    public const string PassWordBookUser = "PasswordBook.User";
    public const string Manage = "PasswordBook.Manage";
    public const string Create = "PasswordBook.Manage.Create";
    public const string Update = "PasswordBook.Manage.Update";
    public const string Delete = "PasswordBook.Manage.Delete";
}
```

### 8. Application 层 (Application)

**IPasswordBookAppService.cs** - 应用服务接口
- GetListByOwnerAsync - 获取用户密码本列表
- GetWithEntriesAsync - 获取密码本详情（含条目）
- AddPasswordEntryAsync - 添加密码条目
- UpdatePasswordAsync - 更新密码
- DeletePasswordEntryAsync - 删除密码条目（软删除）
- RestorePasswordEntryAsync - 恢复密码条目
- EvaluatePasswordStrengthAsync - 评估密码强度
- DeleteAsync - 软删除密码本
- RestoreAsync - 恢复密码本
- HardDeleteAsync - 硬删除密码本
- CreateAsync - 创建密码本
- UpdateAsync - 更新密码本

**Dtos.cs** - 数据传输对象
- PasswordBookDto - 密码本 DTO
- CreateUpdatePasswordBookDto - 创建/更新密码本 DTO
- PasswordEntryDto - 密码条目 DTO
- CreatePasswordEntryDto - 创建密码条目 DTO
- UpdatePasswordDto - 更新密码 DTO

**PasswordBookAppService.cs** - 应用服务实现
- 实现 IPasswordBookAppService 接口
- 权限检查：所有方法都检查 PASSWORDBOOKUSER 权限
- 所有权检查：验证用户是否有权访问指定的密码本

## 技术栈

- 框架：AspNet Boilerplate (ABP) 10.1.0
- .NET 10.0
- 数据库：待配置
- ORM：Entity Framework Core

## 编译状态

✅ 编译成功（0 错误，0 警告）

## 更新记录

### 2026-03-19
- 完成 Domain 层（聚合根、实体、值对象、DomainService）
- 完成 EntityFrameworkCore 配置
- 完成 Application 层（应用服务、DTOs）
- 完成权限定义
- 编译验证通过

### 2026-03-19 (后续)
- 修复 Web 项目警告：移除 GenerateEmbeddedFilesManifest 属性及不存在的 EmbeddedResource 定义
  - 文件：`src/XYPortal.PasswordBook.Web/XYPortal.PasswordBook.Web.csproj`
  - 原因：项目中设置了 GenerateEmbeddedFilesManifest=true，但实际的 CSS/JS 文件不存在
- 修复注释乱码：所有中文注释改为英文
- 业务逻辑修改：`AllowedTypes`（数组）改为 `AllowedType`（单数）
  - 每个 PasswordBook 只能有一种密码类型，不能同时支持多种
  - 修改文件：
    - `CreateUpdatePasswordBookDto.cs`: `PasswordType AllowedType`
    - `PasswordFormatRequirement.cs`: `PasswordType AllowedType` 属性
    - `PasswordBook.cs`: 序列化/反序列化及验证逻辑
    - `PasswordBookAppService.cs`: 创建和更新逻辑
- 引入 XYPortal.RandomStringProvider 模块
  - 添加 6 个项目引用
  - Module 添加 DependsOn 依赖
- 添加 PasswordCharacterType 枚举遮蔽 RandomCategory
  - 文件：`XYPortal.PasswordBook.Domain.Shared/Enums/PasswordCharacterType.cs`
  - 定义密码字符类型：小写字母、大写字母、阿拉伯数字、英文标点
- 添加生成随机密码功能
  - 在 Application.Contracts 添加 DTO：GenerateRandomPasswordDto, GenerateRandomPasswordResultDto
  - 在 IPasswordBookAppService 添加 GenerateRandomPasswordAsync 接口
  - 在 PasswordBookAppService 实现 GenerateRandomPasswordAsync 方法
  - 在 HttpApi 添加 PasswordBookController
- 添加 Web 页面
  - 恢复 GenerateEmbeddedFilesManifest 属性到 Web.csproj
  - 创建 Index.cshtml、Index.cshtml.cs、index.js、index.css
  - 更新 PasswordBookDto 添加 AllowedType, MinLength, MaxLength 属性
