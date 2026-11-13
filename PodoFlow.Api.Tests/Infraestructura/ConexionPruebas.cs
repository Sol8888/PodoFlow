using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace PodoFlow.Api.Tests.Infraestructura;

public class ConexionPruebas : SqlServerConnection
{
    private int _contador;

    public ConexionPruebas(RelationalConnectionDependencies dependencias)
        : base(dependencias)
    {
    }

    public override IDbContextTransaction BeginTransaction()
        => BeginTransaction(IsolationLevel.Unspecified);

    public override Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => BeginTransactionAsync(IsolationLevel.Unspecified, cancellationToken);

    public override IDbContextTransaction BeginTransaction(IsolationLevel isolationLevel)
    {
        var exterior = CurrentTransaction;

        if (exterior == null)
            return base.BeginTransaction(isolationLevel);

        var nombre = SiguientePunto();
        exterior.CreateSavepoint(nombre);

        return new TransaccionAnidada(exterior, nombre);
    }

    public override async Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        var exterior = CurrentTransaction;

        if (exterior == null)
            return await base.BeginTransactionAsync(isolationLevel, cancellationToken);

        var nombre = SiguientePunto();
        await exterior.CreateSavepointAsync(nombre, cancellationToken);

        return new TransaccionAnidada(exterior, nombre);
    }

    private string SiguientePunto()
        => "prueba_" + Interlocked.Increment(ref _contador);
}
