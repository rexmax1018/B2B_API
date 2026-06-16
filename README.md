# B2B_API

.NET 10 WebAPI 多層式架構範例，包含 Oracle 19c + EF Core、JWT、Refresh Token Rotation、全域例外處理、Transaction Log、NLog 與 Swagger。

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
- `B2B.Service.Impl`：JWT、Refresh Token、登入流程與使用者服務實作。
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

設定位置：`B2B.WebApi/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "User Id=B2B_USER;Password=B2B_PASSWORD;Data Source=localhost:1521/ORCLPDB1;"
  }
}
```

目前預設 `DataAccess:UseFakeRepositories` 為 `true`，方便未連 Oracle 時直接測試 API。正式接 Oracle 時請改為：

```json
{
  "DataAccess": {
    "UseFakeRepositories": false
  }
}
```

## JWT 設定

```json
{
  "Jwt": {
    "Issuer": "B2B_API",
    "Audience": "B2B_API_CLIENT",
    "SecretKey": "PLEASE_CHANGE_THIS_SECRET_KEY_TO_AT_LEAST_32_CHARS",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 7
  }
}
```

正式環境請改用 Secret Manager、環境變數或安全機密管理服務保存 `SecretKey`。

## Transaction Log

```json
{
  "TransactionLog": {
    "Enabled": true,
    "IncludeRequestBody": true,
    "IncludeResponseBody": true,
    "MaxBodyLogLength": 10000
  }
}
```

Middleware 會記錄 TraceId、HTTP method、path、query string、status code、request/response body、client IP、UserAgent、耗時與時間戳。client IP 會優先取 `X-Forwarded-For` 第一個 IP，其次取 `X-Real-IP`，最後回退到連線的 remote IP。敏感欄位如 `password`、`accessToken`、`refreshToken`、`token`、`authorization` 會遮罩。

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
GET /api/health
```

Login：

```http
POST /api/auth/login
Content-Type: application/json

{
  "account": "admin",
  "password": "123456"
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

## 正式環境建議

- 將 `DataAccess:UseFakeRepositories` 改為 `false` 並建立正式 Oracle schema。
- 將測試密碼驗證替換為 BCrypt、Argon2 或 PBKDF2。
- 將 `Jwt:SecretKey` 移至安全機密管理。
- 依環境調整 NLog 保留天數、封存策略與集中式 log 收集。
- 規劃 EF Core migration 或 DBA-controlled DDL 流程。
