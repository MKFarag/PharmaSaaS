using FakeItEasy;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Features.Invoices.GetReturnById;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;
using System.Linq.Expressions;
using Xunit;

namespace PharmaSaaS.Application.Test.Invoices;

public class GetReturnInvoiceByIdQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GetReturnInvoiceByIdQueryHandler _sut;

    public GetReturnInvoiceByIdQueryHandlerTests()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _sut = new GetReturnInvoiceByIdQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenReturnInvoiceNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetReturnInvoiceByIdQuery(1);

        A.CallTo(() => _unitOfWork.ReturnInvoices.FindAsync(A<Expression<Func<ReturnInvoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(Task.FromResult<ReturnInvoice?>(null));

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenReturnInvoiceFound_ReturnsMappedResponse()
    {
        // Arrange
        var query = new GetReturnInvoiceByIdQuery(1);
        var returnInvoice = ReturnInvoice.Create(1, 1, 50m);

        A.CallTo(() => _unitOfWork.ReturnInvoices.FindAsync(A<Expression<Func<ReturnInvoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(returnInvoice);

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
    }
}
