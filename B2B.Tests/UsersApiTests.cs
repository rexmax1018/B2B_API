using B2B.Domain;
using B2B.Dao.Repositories.Interfaces;
using B2B.Service.Impl.Services;
using B2B.Service.Interfaces;
using B2B.WebApi.Controllers;
using B2B.WebApi.Model.Common;
using B2B.WebApi.Model.User;
using Microsoft.AspNetCore.Mvc;

namespace B2B.Tests;

/// <summary>
/// 驗證 UserService 與 UsersController 的移植契約。
/// </summary>
public sealed class UsersApiTests
{
    [Fact]
    public async Task UserService_DelegatesUserLookupToDaoContract()
    {
        var expected = new UserDomain
        {
            UserId = 7,
            Account = "legacy-user",
            DisplayName = "Legacy User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var service = new UserService(new StubUserRepository(expected));

        var byAccount = await service.GetByAccountAsync("legacy-user", CancellationToken.None);
        var byId = await service.GetByIdAsync(7, CancellationToken.None);

        Assert.Same(expected, byAccount);
        Assert.Same(expected, byId);
    }

    [Fact]
    public async Task GetByAccount_DelegatesToServiceAndMapsSafeResponse()
    {
        var controller = new UsersController(new StubUserService
        {
            User = new UserDomain
            {
                UserId = 1,
                Account = "admin",
                DisplayName = "系統管理員",
                PasswordHash = "must-not-be-returned",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        });

        var action = await controller.GetByAccount("admin", CancellationToken.None);

        var result = Assert.IsType<OkObjectResult>(action.Result);
        var payload = Assert.IsType<ApiResponse<UserResponse>>(result.Value);
        Assert.True(payload.Success);
        Assert.Equal(1, payload.Data?.UserId);
        Assert.Equal("admin", payload.Data?.Account);
        Assert.DoesNotContain("PasswordHash", payload.Data?.GetType().GetProperties().Select(x => x.Name) ?? []);
    }

    [Fact]
    public async Task GetById_WhenServiceReturnsNull_ReturnsNotFound()
    {
        var controller = new UsersController(new StubUserService());

        var action = await controller.GetById(999, CancellationToken.None);

        var result = Assert.IsType<NotFoundObjectResult>(action.Result);
        var payload = Assert.IsType<ApiResponse<UserResponse>>(result.Value);
        Assert.False(payload.Success);
        Assert.Equal("USER_NOT_FOUND", payload.Error?.Code);
    }

    private sealed class StubUserService : IUserService
    {
        public UserDomain? User { get; init; }

        public Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken) =>
            Task.FromResult(User);

        public Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken) =>
            Task.FromResult(User);
    }

    private sealed class StubUserRepository(UserDomain? user) : IUserRepository
    {
        public Task<UserDomain?> GetByAccountAsync(string account, CancellationToken cancellationToken) =>
            Task.FromResult(user);

        public Task<UserDomain?> GetByIdAsync(long userId, CancellationToken cancellationToken) =>
            Task.FromResult(user);
    }
}
