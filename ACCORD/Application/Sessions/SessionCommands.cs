using ACCORD.Application.Common;

namespace ACCORD.Application.Sessions;

public interface ISessionGateway
{
    Task<OperationResult> CreateGuestAsync(CancellationToken cancellationToken);
    Task<OperationResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken);
}

public sealed record CreateGuestSessionCommand;
public sealed record RefreshGuestSessionCommand(string? RefreshToken);

public sealed class CreateGuestSessionCommandHandler(ISessionGateway gateway) : ICommandHandler<CreateGuestSessionCommand>
{
    public Task<OperationResult> HandleAsync(CreateGuestSessionCommand command, CancellationToken cancellationToken) =>
        gateway.CreateGuestAsync(cancellationToken);
}

public sealed class RefreshGuestSessionCommandHandler(ISessionGateway gateway) : ICommandHandler<RefreshGuestSessionCommand>
{
    public Task<OperationResult> HandleAsync(RefreshGuestSessionCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.RefreshToken) || command.RefreshToken.Length > 4096)
            return Task.FromResult(OperationResult.Error(400, "Phiên khách không hợp lệ."));
        return gateway.RefreshAsync(command.RefreshToken, cancellationToken);
    }
}
