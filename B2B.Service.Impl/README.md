# B2B.Service.Impl

`B2B.Service.Impl` 是服務層實作專案，負責服務憑證登入、使用者查詢、JWT 產生、Refresh Token 輪替與撤銷等商業流程。AuthService 不會讀取 User 資料；UserService 則供 Web API 查詢使用者。

## 專案定位

```mermaid
flowchart LR
    WebApi["B2B.WebApi"]
    Service["B2B.Service"]
    Impl["B2B.Service.Impl"]
    Dao["B2B.Dao"]
    Domain["B2B.Domain"]

    WebApi --> Service
    Service --> Impl
    Impl --> Dao
    Impl --> Domain
```

WebApi 只呼叫介面，實作由 Autofac Module 註冊。

## 主要內容

| 路徑 | 說明 |
| --- | --- |
| `Services/` | `AuthService`、`EntryCredentialValidator`、`UserService`、`TokenService` |
| `Stores/` | `MemoryRefreshTokenStore` |
| `Mappings/` | Service 身分與 JWT Claims 轉換 |
| `Modules/` | Autofac Module |

## 是否使用 Module

有。此專案使用 Autofac Module。

| Module | 位置 | 用途 |
| --- | --- | --- |
| `B2BServiceModule` | `Modules/B2BServiceModule.cs` | 註冊 Auth、Entry 憑證、User、Token 與 Refresh Token Store 服務 |

## Module 使用方式

`B2BServiceModule` 由 `B2BWebApiModule` 引入：

```csharp
builder.RegisterModule<B2BServiceModule>();
```

註冊內容：

| 介面 | 實作 | 生命週期 |
| --- | --- | --- |
| `IAuthService` | `AuthService` | `InstancePerLifetimeScope` |
| `IEntryCredentialValidator` | `EntryCredentialValidator` | `SingleInstance` |
| `IUserService` | `UserService` | `InstancePerLifetimeScope` |
| `ITokenService` | `TokenService` | `InstancePerLifetimeScope` |
| `IRefreshTokenStore` | `MemoryRefreshTokenStore` | `InstancePerLifetimeScope` |

## 登入流程

```mermaid
sequenceDiagram
    participant WebApi as AuthController
    participant Auth as AuthService
    participant Credential as IEntryCredentialValidator
    participant Token as TokenService
    participant Store as IRefreshTokenStore

    WebApi->>Auth: LoginAsync(encryptedCredential)
    Auth->>Credential: IsValid(encryptedCredential)
    Credential-->>Auth: 比對 Entry.ini 的 AES-GCM 密文
    Auth->>Token: 產生 Access Token / Refresh Token
    Auth->>Store: 儲存 Refresh Token
    Auth-->>WebApi: LoginResultDomain
```

## Refresh Token 流程

```mermaid
sequenceDiagram
    participant WebApi as AuthController
    participant Auth as AuthService
    participant Store as IRefreshTokenStore
    participant Token as TokenService

    WebApi->>Auth: RefreshTokenAsync(refreshToken)
    Auth->>Store: 驗證並消耗舊 Refresh Token
    Auth->>Token: 產生新 Token
    Auth->>Store: 儲存新 Refresh Token
    Auth-->>WebApi: LoginResultDomain
```

Refresh Token 採輪替機制。舊 Token 使用後會失效，再產生新的 Access Token 與 Refresh Token。

## Options 需求

`TokenService` 需要 `JwtOptions`，由 WebApi 的 `AddB2BOptions` 綁定：

```json
{
  "Jwt": {
    "Issuer": "B2B.WebApi",
    "Audience": "B2B.Client",
    "SecretKey": "請填入至少 32 字元的安全密鑰",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 7
  }
}
```

## 使用方式

Controller 或其他服務只依賴介面：

```csharp
public sealed class MyController(IAuthService authService)
{
    public Task<LoginResultDomain> LoginAsync(string encryptedCredential)
    {
        return authService.LoginAsync(encryptedCredential, CancellationToken.None);
    }
}
```

## 維護注意事項

- 商業流程應放在此專案，不要放到 Controller 或 Repository。
- 新增服務實作時，需先在 `B2B.Service` 定義介面，再於 `B2BServiceModule` 註冊。
- Refresh Token 目前使用 Memory Store，服務重啟後資料會清空。
- 若未來多機部署，`IRefreshTokenStore` 應改為 Redis、Distributed Cache 或資料庫實作。
- `EntryCredentialValidator` 只接受與 `Entry.ini` 相同的 AES-GCM 密文；非 Development 環境不得使用版控中的公開範例。
