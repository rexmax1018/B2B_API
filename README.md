# B2B_API

.NET 10 WebAPI 多層式架構範例，包含 Oracle 19c + EF Core、JWT Access Token、MemoryCache Refresh Token、全域例外處理、Transaction Log、NLog 與 Swagger。

## 專案架構

```txt
B2B_API.sln
B2B.Dao/
B2B.Domain/
B2B.Service/
B2B.Service.Impl/
B2B.WebApi/
B2B.WebApi.Model/
```

## 子專案職責

- `B2B.Domain`：Service 與 Dao 之間共用的 Domain Model。
- `B2B.Dao`：EF Core DbContext、Oracle Entity Mapping、Repository。
- `B2B.Service`：Service interface。
- `B2B.Service.Impl`：JWT Access Token、MemoryCache Refresh Token、登入流程與使用者服務實作。
- `B2B.WebApi.Model`：對外 Request / Response DTO 與統一 API 回應格式。
- `B2B.WebApi`：Controller、Middleware、DI、Swagger、NLog、設定檔與 API 入口。

## 還原與建置

```powershell
dotnet restore B2B_API.sln
dotnet build B2B_API.sln
```

## 執行 WebApi

```powershell
dotnet run --project B2B.WebApi/B2B.WebApi.csproj
```

啟動後可開啟 Swagger：

```txt
https://localhost:<port>/swagger
```

## Oracle Connection String

設定 key：`ConnectionStrings:DefaultConnection`

請透過 Secret Manager、環境變數或正式機密管理服務注入，不要將含密碼的 connection string commit 到 repository。

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<oracle-connection-string>" --project B2B.WebApi/B2B.WebApi.csproj
```

正式接 Oracle 時請設定：

```json
{
  "DataAccess": {
    "UseFakeRepositories": false
  }
}
```

非 Development 環境若啟用 `DataAccess:UseFakeRepositories`，應用程式會在啟動時中止。

## JWT 設定

Access Token 使用 JWT，採 stateless 設計，不儲存在資料庫。`Jwt:Issuer`、`Jwt:Audience`、token 期限可放在一般設定檔；`Jwt:SecretKey` 必須透過安全設定來源注入。

```powershell
dotnet user-secrets set "Jwt:SecretKey" "<strong-random-secret>" --project B2B.WebApi/B2B.WebApi.csproj
```

若 `Jwt:SecretKey` 為空白或仍為 placeholder，應用程式會在啟動時中止。

## Refresh Token Store

Refresh Token 使用 `IMemoryCache` 儲存，不寫入 Oracle，也不需要 Refresh Token 資料表或 Repository。Cache key 會使用 Refresh Token 的 SHA256 hash 組成，不直接使用明文 token。

```json
{
  "RefreshTokenStore": {
    "Provider": "Memory"
  }
}
```

WebAPI 重啟後 MemoryCache 會清空，原本核發的 Refresh Token 會失效，使用者需要重新登入。MemoryCache 適合開發環境與單機部署；若正式環境有多台 WebAPI，應改用 Redis / Distributed Cache 或資料庫儲存 Refresh Token，確保多節點之間狀態一致。

## Transaction Log

```json
{
  "TransactionLog": {
    "Enabled": true,
    "IncludeRequestBody": false,
    "IncludeResponseBody": false,
    "TrustForwardedHeaders": false,
    "MaxBodyLogLength": 10000
  }
}
```

Middleware 會記錄 TraceId、HTTP method、path、query string、status code、client IP、UserAgent、耗時與時間戳。request/response body 預設不記錄；非 Development 環境若開啟 body logging，應用程式會在啟動時中止。敏感欄位如 `password`、`accessToken`、`refreshToken`、`token`、`authorization` 會遮罩。client IP 預設使用連線來源；只有在明確設定 `TrustForwardedHeaders` 時才會讀取 `X-Forwarded-For` / `X-Real-IP`。

## NLog Log 位置

```txt
B2B.WebApi/logs/
  app/app.log
  error/error.log
  transaction/transaction.log
```

一般 app/error log 單檔上限為 10 MB，依天輪替並保留最多 7 天。Transaction log 單檔上限為 10 MB，依小時輪替並保留最多 7 天。

## API 測試範例

Health：

```http
GET /Health
```

Login：

```http
POST /api/auth/login
Content-Type: application/json

{
  "account": "admin",
  "password": "<password>"
}
```

Refresh Token：

```http
POST /api/auth/refresh-token
Content-Type: application/json

{
  "refreshToken": "<login response refreshToken>"
}
```

Logout：

```http
POST /api/auth/logout
Content-Type: application/json

{
  "refreshToken": "<login response refreshToken>"
}
```

## 正式環境建議

- 將 `DataAccess:UseFakeRepositories` 改為 `false` 並建立正式 Oracle schema。
- 使用 PBKDF2、BCrypt 或 Argon2 儲存密碼雜湊，不保存明文密碼。
- 將 `Jwt:SecretKey` 移至安全機密管理。
- 多台 WebAPI 部署時，將 Refresh Token Store 從 MemoryCache 改為 Redis / Distributed Cache 或資料庫。
- 依環境調整 NLog 保留天數、封存策略與集中式 log 收集。
- 規劃 EF Core migration 或 DBA-controlled DDL 流程。
