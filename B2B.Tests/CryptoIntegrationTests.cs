using System.Security.Cryptography;
using System.Text;
using B2B.CryptoLib;
using B2B.Dao.Extensions;
using B2B.Dao.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace B2B.Tests;

/// <summary>
/// 驗證 B2B API 的 CryptoLib 啟動設定與 EF Core 欄位加密 mapping。
/// </summary>
public sealed class CryptoIntegrationTests : IClassFixture<CryptoTestFixture>
{
    private readonly CryptoTestFixture fixture;

    public CryptoIntegrationTests(CryptoTestFixture fixture)
    {
        this.fixture = fixture;
    }

    /// <summary>
    /// Crypto 停用時不會建立或讀取金鑰目錄。
    /// </summary>
    [Fact]
    public void InitializeB2BCrypto_WhenDisabled_DoesNotCreateKeyDirectory()
    {
        var keyPath = Path.Combine(
            fixture.ContentRootPath,
            "b2b-api-crypto-disabled-" + Guid.NewGuid().ToString("N"));

        fixture.CreateConfiguration(false, keyPath).InitializeB2BCrypto(fixture.ContentRootPath);

        Assert.False(Directory.Exists(keyPath));
    }

    /// <summary>
    /// Crypto 啟用時必須設定金鑰根目錄。
    /// </summary>
    [Fact]
    public void InitializeB2BCrypto_WhenKeyManagerBasePathIsMissing_FailsFast()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            fixture.CreateConfiguration(true, " ").InitializeB2BCrypto(fixture.ContentRootPath));

        Assert.Contains("Crypto:KeyManagerBasePath", exception.Message);
    }

    /// <summary>
    /// Crypto 啟用時必須設定目前金鑰名稱。
    /// </summary>
    [Fact]
    public void InitializeB2BCrypto_WhenActiveUnifiedNameIsMissing_FailsFast()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            fixture.CreateConfiguration(true, fixture.RelativeKeyPath, " ")
                .InitializeB2BCrypto(fixture.ContentRootPath));

        Assert.Contains("Crypto:ActiveUnifiedName", exception.Message);
    }

    /// <summary>
    /// 相對金鑰路徑以宿主內容根目錄為基準解析，並可完成加密。
    /// </summary>
    [Fact]
    public void InitializeB2BCrypto_WithRelativeKeyPath_UsesContentRootPath()
    {
        fixture.CreateConfiguration(true, fixture.RelativeKeyPath)
            .InitializeB2BCrypto(fixture.ContentRootPath);

        var encrypted = Crypto.Encrypt("relative-path-secret");

        Assert.NotNull(encrypted);
        Assert.True(Crypto.IsValidEncryptedFormat(encrypted));
        Assert.Equal(fixture.ActiveUnifiedName, Crypto.GetUnifiedName(encrypted));
    }

    /// <summary>
    /// 絕對金鑰路徑不會再與宿主內容根目錄拼接。
    /// </summary>
    [Fact]
    public void InitializeB2BCrypto_WithAbsoluteKeyPath_PreservesAbsolutePath()
    {
        var unrelatedContentRoot = Path.Combine(
            fixture.ContentRootPath,
            "unrelated-content-root-" + Guid.NewGuid().ToString("N"));

        fixture.CreateConfiguration(true, fixture.Root)
            .InitializeB2BCrypto(unrelatedContentRoot);

        var encrypted = Crypto.Encrypt("absolute-path-secret");

        Assert.NotNull(encrypted);
        Assert.Equal(fixture.ActiveUnifiedName, Crypto.GetUnifiedName(encrypted));
    }

    /// <summary>
    /// 加密 mapping 會掛上 EF Core ValueConverter，且不改變 required 設定鏈。
    /// </summary>
    [Fact]
    public void HasB2BEncryption_AddsValueConverterToTestOnlyModel()
    {
        using var context = CreateContext();

        var property = context.Model
            .FindEntityType(typeof(EncryptionTestEntity))!
            .FindProperty(nameof(EncryptionTestEntity.Secret))!;

        Assert.NotNull(property.GetValueConverter());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// ValueConverter 使用 CryptoLib 格式完成隨機化密文 round-trip，並保留 NULL。
    /// </summary>
    [Fact]
    public void HasB2BEncryption_ConverterRoundTripsRandomizedCiphertextAndNull()
    {
        fixture.CreateConfiguration(true, fixture.RelativeKeyPath)
            .InitializeB2BCrypto(fixture.ContentRootPath);

        using var context = CreateContext();
        var property = context.Model
            .FindEntityType(typeof(EncryptionTestEntity))!
            .FindProperty(nameof(EncryptionTestEntity.Secret))!;
        var converter = property.GetValueConverter();
        Assert.NotNull(converter);
        const string plainText = "converter-round-trip-secret";

        var firstCipherText = Assert.IsType<string>(converter.ConvertToProvider(plainText));
        var secondCipherText = Assert.IsType<string>(converter.ConvertToProvider(plainText));
        var restored = converter.ConvertFromProvider(firstCipherText);

        Assert.NotEqual(plainText, firstCipherText);
        Assert.NotEqual(firstCipherText, secondCipherText);
        Assert.True(Crypto.IsValidEncryptedFormat(firstCipherText));
        Assert.Equal(fixture.ActiveUnifiedName, Crypto.GetUnifiedName(firstCipherText));
        Assert.Equal(plainText, restored);
        Assert.Null(converter.ConvertToProvider(null));
        Assert.Null(converter.ConvertFromProvider(null));
        Assert.Throws<ArgumentException>(() =>
        {
            _ = converter.ConvertFromProvider("existing-plaintext");
        });
    }

    private EncryptionTestContext CreateContext()
    {
        return new EncryptionTestContext(
            new DbContextOptionsBuilder<EncryptionTestContext>()
                .UseOracle("User Id=test;Password=test;Data Source=test")
                .Options);
    }

    private sealed class EncryptionTestContext(
        DbContextOptions<EncryptionTestContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EncryptionTestEntity>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Secret)
                    .HasColumnName("TEST_SECRET")
                    .HasB2BEncryption()
                    .IsRequired();
                entity.Property(x => x.OptionalSecret)
                    .HasColumnName("TEST_OPTIONAL_SECRET")
                    .HasB2BEncryption();
            });
        }
    }

    private sealed class EncryptionTestEntity
    {
        public int Id { get; set; }

        public string Secret { get; set; } = string.Empty;

        public string OptionalSecret { get; set; } = string.Empty;
    }
}

/// <summary>
/// 建立與 B2B.CryptoLib v2 key-manager contract 相容的測試金鑰組。
/// </summary>
public sealed class CryptoTestFixture : IDisposable
{
    public CryptoTestFixture()
    {
        Root = Path.Combine(
            Path.GetTempPath(),
            "B2B_API_CryptoTests",
            Guid.NewGuid().ToString("N"));
        ContentRootPath = Directory.GetParent(Root)!.FullName;
        ActiveUnifiedName = "b2b-test-key";
        RelativeKeyPath = Path.GetFileName(Root);

        var currentPath = Path.Combine(Root, "current");
        Directory.CreateDirectory(currentPath);

        using var rsa = RSA.Create(2048);
        var aesKey = RandomNumberGenerator.GetBytes(32);
        var aesIv = RandomNumberGenerator.GetBytes(16);
        var aesMaterial = Convert.ToBase64String(aesKey) + ":" + Convert.ToBase64String(aesIv);
        var encryptedAesMaterial = rsa.Encrypt(
            Encoding.UTF8.GetBytes(aesMaterial),
            RSAEncryptionPadding.OaepSHA1);

        File.WriteAllBytes(
            Path.Combine(currentPath, ActiveUnifiedName + ".aes"),
            encryptedAesMaterial);
        File.WriteAllText(
            Path.Combine(currentPath, ActiveUnifiedName + ".pub"),
            rsa.ExportSubjectPublicKeyInfoPem(),
            Encoding.UTF8);
        File.WriteAllText(
            Path.Combine(currentPath, ActiveUnifiedName + ".priv"),
            rsa.ExportPkcs8PrivateKeyPem(),
            Encoding.UTF8);
    }

    public string Root { get; }

    public string ContentRootPath { get; }

    public string RelativeKeyPath { get; }

    public string ActiveUnifiedName { get; }

    public IConfiguration CreateConfiguration(
        bool enabled,
        string keyManagerBasePath,
        string? activeUnifiedName = null)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Crypto:Enabled"] = enabled.ToString(),
                ["Crypto:KeyManagerBasePath"] = keyManagerBasePath,
                ["Crypto:ActiveUnifiedName"] = activeUnifiedName ?? ActiveUnifiedName
            })
            .Build();
    }

    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }
}
