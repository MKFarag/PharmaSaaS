namespace PharmaSaaS.Infrastructure.Messaging;

public sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));

        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler for {request.GetType().Name}");

        return await ((dynamic)handler).Handle((dynamic)request, cancellationToken);
    }

    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());

        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler for {request.GetType().Name}");

        await ((dynamic)handler).Handle((dynamic)request, cancellationToken);
    }
}