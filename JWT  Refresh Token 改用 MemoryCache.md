# JWT / Refresh Token 改用 MemoryCache 補充需求

請將本專案的 JWT / Refresh Token 機制調整為：

```txt
Access Token：JWT，不儲存在資料庫
Refresh Token：使用 MemoryCache 儲存，不使用資料庫
```

---

# 一、整體原則

1. JWT Access Token 採 Stateless 設計，不需要存入資料庫。
2. Refresh Token 請改用 `IMemoryCache` 儲存。
3. 不需要建立 Refresh Token 資料表。
4. 不需要建立 `RefreshTokenEntity`。
5. 不需要建立 `RefreshTokenRepository`。
6. 不需要透過 Oracle 儲存 Refresh Token。
7. WebAPI 重啟後 MemoryCache 會清空，此時 Refresh Token 失效是可接受行為。
8. MVC 或前端若 Refresh Token 失敗，應重新導向登入流程。

---

# 二、專案結構調整

請移除或不要建立以下檔案：

```txt
B2B.Dao/
  Entities/
    RefreshTokenEntity.cs
  Repositories/
    Interfaces/
      IRefreshTokenRepository.cs
    Implements/
      RefreshTokenRepository.cs
```

`B2BDbContext` 不需要包含：

```csharp
DbSet<RefreshTokenEntity>
```

`OnModelCreating` 不需要設定 `RefreshTokenEntity` Mapping。

---

# 三、請新增 Refresh Token Store 抽象

請建立：

```txt
B2B.Service/
  Interfaces/
    IRefreshTokenStore.cs
```

內容如下：

```csharp
using B2B.Domain.Models;

namespace B2B.Service.Interfaces;

public interface IRefreshTokenStore
{
    Task SaveAsync(
        string refreshToken,
        RefreshTokenModel model,
        TimeSpan expiresIn,
        CancellationToken cancellationToken = default);

    Task<RefreshTokenModel?> GetAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}
```

---

# 四、請新增 MemoryCache 實作

請建立：

```txt
B2B.Service.Impl/
  Stores/
    MemoryRefreshTokenStore.cs
```

需求：

1. 使用 `IMemoryCache`。
2. Refresh Token 不可直接以明文作為 Cache Key。
3. 請將 Refresh Token 做 SHA256 Hash 後，再組成 Cache Key。
4. Cache Key 格式：

```txt
refresh_token:{sha256_hash}
```

5. 儲存時需設定絕對過期時間。
6. 支援 Save / Get / Remove。
7. 不需要寫入資料庫。

實作方向：

```csharp
using System.Security.Cryptography;
using System.Text;
using B2B.Domain.Models;
using B2B.Service.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace B2B.Service.Impl.Stores;

public class MemoryRefreshTokenStore : IRefreshTokenStore
{
    private readonly IMemoryCache _memoryCache;

    public MemoryRefreshTokenStore(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task SaveAsync(
        string refreshToken,
        RefreshTokenModel model,
        TimeSpan expiresIn,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(refreshToken);

        _memoryCache.Set(
            key,
            model,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiresIn
            });

        return Task.CompletedTask;
    }

    public Task<RefreshTokenModel?> GetAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(refreshToken);

        _memoryCache.TryGetValue(key, out RefreshTokenModel? model);

        return Task.FromResult(model);
    }

    public Task RemoveAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(refreshToken);

        _memoryCache.Remove(key);

        return Task.CompletedTask;
    }

    private static string BuildKey(string refreshToken)
    {
        var hash = Sha256(refreshToken);
        return $"refresh_token:{hash}";
    }

    private static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
```

---

# 五、Domain Model 調整

請保留或建立：

```txt
B2B.Domain/
  Models/
    RefreshTokenModel.cs
```

內容需包含：

```csharp
namespace B2B.Domain.Models;

public class RefreshTokenModel
{
    public long UserId { get; set; }

    public string Account { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }
}
```

---

# 六、AuthService 調整

請調整 `AuthService`：

1. Login 成功後：

   * 產生 Access Token。
   * 產生 Refresh Token。
   * 建立 `RefreshTokenModel`。
   * 使用 `IRefreshTokenStore.SaveAsync()` 存入 MemoryCache。
   * 回傳 Access Token 與 Refresh Token。

2. Refresh Token 時：

   * 使用 `IRefreshTokenStore.GetAsync()` 查詢 Refresh Token。
   * 如果查不到，回傳未授權錯誤。
   * 如果已過期或已撤銷，回傳未授權錯誤。
   * 採用 Refresh Token Rotation：

     * 移除舊 Refresh Token。
     * 產生新的 Access Token。
     * 產生新的 Refresh Token。
     * 儲存新的 Refresh Token。
     * 回傳新的 Access Token 與 Refresh Token。

3. Logout 時：

   * 使用 `IRefreshTokenStore.RemoveAsync()` 移除 Refresh Token。

4. 不要注入 `IRefreshTokenRepository`。

5. 不要操作 `RefreshTokenEntity`。

6. 不要呼叫 Refresh Token 相關資料庫邏輯。

---

# 七、DI 註冊調整

請在 DI 加入：

```csharp
services.AddMemoryCache();
services.AddScoped<IRefreshTokenStore, MemoryRefreshTokenStore>();
```

請移除或不要加入：

```csharp
services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
```

若原本有 `RefreshTokenRepository` 相關註冊，請一併移除。

---

# 八、appsettings.json 調整

請保留 JWT 設定：

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

請新增 Refresh Token Store 設定：

```json
{
  "RefreshTokenStore": {
    "Provider": "Memory"
  }
}
```

---

# 九、Controller 調整

`AuthController` 請提供：

```http
POST /api/auth/login
POST /api/auth/refresh-token
POST /api/auth/logout
```

## Login Request

```json
{
  "account": "admin",
  "password": "123456"
}
```

## Refresh Token Request

```json
{
  "refreshToken": "..."
}
```

## Logout Request

```json
{
  "refreshToken": "..."
}
```

---

# 十、錯誤處理

Refresh Token 失敗時請回傳明確錯誤代碼。

例如：

```json
{
  "success": false,
  "data": null,
  "message": "登入狀態已失效，請重新登入",
  "error": {
    "code": "INVALID_REFRESH_TOKEN",
    "message": "登入狀態已失效，請重新登入"
  }
}
```

常見錯誤代碼：

```txt
INVALID_REFRESH_TOKEN
REFRESH_TOKEN_EXPIRED
REFRESH_TOKEN_REVOKED
```

---

# 十一、Swagger 測試

Swagger 需可測試：

1. Login 取得 Access Token 與 Refresh Token。
2. Refresh Token 換發新的 Token。
3. Logout 移除 Refresh Token。
4. 使用 JWT Authorize 測試需要授權的 API。

---

# 十二、README 補充

README 請說明：

1. Access Token 使用 JWT，不儲存在資料庫。
2. Refresh Token 使用 MemoryCache。
3. WebAPI 重啟後 MemoryCache 會清空。
4. WebAPI 重啟後，原本的 Refresh Token 會失效，使用者需要重新登入。
5. MemoryCache 適合開發環境、單機環境。
6. 若正式環境有多台 WebAPI，應改用 Redis / Distributed Cache 或資料庫儲存 Refresh Token。

---

# 十三、驗收條件

完成後請確認：

1. `dotnet build` 成功。
2. `B2B.WebApi` 可以啟動。
3. Swagger 可以開啟。
4. `/api/auth/login` 可以取得 Access Token 與 Refresh Token。
5. `/api/auth/refresh-token` 可以使用 Refresh Token 換發新的 Access Token 與 Refresh Token。
6. Refresh Token Rotation 正常：

   * 舊 Refresh Token 使用後失效。
   * 新 Refresh Token 可正常使用。
7. `/api/auth/logout` 可以讓 Refresh Token 失效。
8. 不存在 `RefreshTokenEntity`。
9. 不存在 `RefreshTokenRepository`。
10. `B2BDbContext` 不包含 `RefreshTokenEntity`。
11. Refresh Token 沒有寫入 Oracle。
12. Refresh Token Cache Key 使用 SHA256 Hash，不使用明文 Token。
13. WebAPI 重啟後，舊 Refresh Token 失效是可接受行為，README 有清楚說明。
