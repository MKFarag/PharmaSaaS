using FakeItEasy;
using PharmaSaaS.Application.Contracts.Invoices;
using PharmaSaaS.Application.Features.Invoices.GetById;
using PharmaSaaS.Application.Interfaces.Infrastructure;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Domain.Errors;
using System.Linq.Expressions;
using Xunit;

namespace PharmaSaaS.Application.Test.Invoices;

public class GetInvoiceByIdQueryHandlerTests
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly GetInvoiceByIdQueryHandler _sut;

    public GetInvoiceByIdQueryHandlerTests()
    {
        _unitOfWork = A.Fake<IUnitOfWork>();
        _sut = new GetInvoiceByIdQueryHandler(_unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetInvoiceByIdQuery(1);

        A.CallTo(() => _unitOfWork.Invoices.FindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(Task.FromResult<Invoice?>(null));

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(InvoiceErrors.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_WhenInvoiceFound_ReturnsMappedResponse()
    {
        // Arrange
        var query = new GetInvoiceByIdQuery(1);
        var invoice = Invoice.Create(1, null, 100m, 0m, 1);

        A.CallTo(() => _unitOfWork.Invoices.FindAsync(A<Expression<Func<Invoice, bool>>>._, A<string[]>._, A<CancellationToken>._))
            .Returns(invoice);

        // Act
        var result = await _sut.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        // Mapster is used in the handler to map Invoice -> InvoiceDetailResponse
        // We just ensure it succeeds and returns a non-null value
        Assert.NotNull(result.Value);
    }
}
