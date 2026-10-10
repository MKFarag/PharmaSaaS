using PharmaSaaS.Application.Features.Roles.GetAllPermissions;

namespace PharmaSaaS.Application.Test.Roles;

public class GetAllPermissionsQueryHandlerTest
{
    private readonly GetAllPermissionsQueryHandler _handler = new();

    [Fact]
    public async Task Handle_ReturnsAllSystemPermissionsAsDto()
    {
        var result = await _handler.Handle(new GetAllPermissionsQuery(), CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotEmpty(result);

        var userModify = Assert.Single(result, p => p.Permission == Permissions.UpdateUser);
        Assert.Equal("User", userModify.Resource);
        Assert.Equal("Modify", userModify.Action);
        Assert.Equal(Permissions.UpdateUser, userModify.Permission);

        var invoiceCreate = Assert.Single(result, p => p.Permission == Permissions.CreateInvoice);
        Assert.Equal("Invoice", invoiceCreate.Resource);
        Assert.Equal("Create", invoiceCreate.Action);
        Assert.Equal(Permissions.CreateInvoice, invoiceCreate.Permission);
    }
}
