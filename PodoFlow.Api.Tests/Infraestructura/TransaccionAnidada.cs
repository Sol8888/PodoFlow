using Microsoft.EntityFrameworkCore.Storage;

namespace PodoFlow.Api.Tests.Infraestructura;

public sealed class TransaccionAnidada : IDbContextTransaction
{
    private readonly IDbContextTransaction _exterior;
    private readonly string _puntoGuardado;
    private bool _terminada;

    public TransaccionAnidada(IDbContextTransaction exterior, string puntoGuardado)
    {
        _exterior = exterior;
        _puntoGuardado = puntoGuardado;
    }

    public Guid TransactionId { get; } = Guid.NewGuid();

    public void Commit()
    {
        _terminada = true;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        _terminada = true;
        return Task.CompletedTask;
    }

    public void Rollback()
    {
        _terminada = true;
        _exterior.RollbackToSavepoint(_puntoGuardado);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _terminada = true;
        await _exterior.RollbackToSavepointAsync(_puntoGuardado, cancellationToken);
    }

    public void Dispose()
    {
        if (_terminada)
            return;

        _terminada = true;

        try
        {
            _exterior.RollbackToSavepoint(_puntoGuardado);
        }
        catch (InvalidOperationException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_terminada)
            return;

        _terminada = true;

        try
        {
            await _exterior.RollbackToSavepointAsync(_puntoGuardado);
        }
        catch (InvalidOperationException)
        {
        }
    }
}
