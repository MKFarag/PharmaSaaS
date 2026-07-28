using FakeItEasy;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Features.Invoices.GetAllReturns;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Abstraction;
using PharmaSaaS.Domain.Entities;
using System.Linq.Expressions;
using Xunit;

namespace PharmaSaaS.Application.Test.Invoices;

public class GetAllReturnInvoicesQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GetAllReturnInvoicesQueryHandler _sut;

    public GetAllReturnInvoicesQueryHandlerTests()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _sut = new GetAllReturnInvoicesQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_Always_ReturnsPaginatedList()
    {
        // Arrange
        var filters = new ReturnInvoiceRequestFilters { PageNumber = 1, PageSize = 10 };
        var query = new GetAllReturnInvoicesQuery(filters);

        var paginatedList = A.Fake<IPaginatedList<ReturnInvoiceResponse>>();
        
        A.CallTo(() => _unitOfWork.ReturnInvoices.FindPaginatedListAsync<ReturnInvoiceResponse>(
            A<Expression<Func<ReturnInvoice, bool>>>._,
            1,
            10,
            nameof(ReturnInvoice.Id),
            A<string[]>._,
            A<CancellationToken>._)).Returns(paginatedList);

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(paginatedList, result.Value);
    }
}
