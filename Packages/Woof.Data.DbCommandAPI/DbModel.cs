namespace Woof.Data.DbCommandApi;

/// <summary>
/// Base class for simple SQL data models.
/// </summary>
/// <typeparam name="TParam">A parameter type for the <see cref="DbCommand"/>, database specific.</typeparam>
public abstract class DbModel<TParam> : IDisposable, IAsyncDisposable where TParam : DbParameter, new() {

    /// <summary>
    /// Gets the model's connection.
    /// </summary>
    public DbConnection Connection { get; }

    /// <summary>
    /// Gets or sets the SQL command timeout, in seconds.
    /// </summary>
    public int Timeout { get; set; } = 5;

    /// <summary>
    /// Creates the model with a connection.
    /// </summary>
    /// <param name="connection">Any database connection.</param>
    public DbModel(DbConnection connection) => Connection = connection;

    /// <summary>
    /// Starts a database transaction.
    /// </summary>
    public void BeginTransaction() => Transaction = Connection.BeginTransaction();

    /// <summary>
    /// Starts a database transaction.
    /// </summary>
    public async ValueTask BeginTransactionAsync() => Transaction = await Connection.BeginTransactionAsync();

    /// <summary>
    /// Commits the database transaction started with <see cref="BeginTransaction"/>.
    /// </summary>
    public void Commit() {
        Transaction?.Commit();
        Transaction?.Dispose();
        Transaction = null;
    }

    /// <summary>
    /// Commits the database transaction started with <see cref="BeginTransactionAsync"/>.
    /// </summary>
    public async ValueTask CommitAsync() {
        if (Transaction is not null) {
            await Transaction.CommitAsync();
            await Transaction.DisposeAsync();
            Transaction = null;
        }
    }

    /// <summary>
    /// Rolls back a transaction started with <see cref="BeginTransaction"/>.
    /// </summary>
    public void Rollback() {
        Transaction?.Rollback();
        Transaction?.Dispose();
        Transaction = null;
    }

    /// <summary>
    /// Rolls back a transaction started with <see cref="BeginTransactionAsync"/>.
    /// </summary>
    public async ValueTask RollbackAsync() {
        if (Transaction is not null) {
            await Transaction.RollbackAsync();
            await Transaction.DisposeAsync();
            Transaction = null;
        }
    }

    /// <summary>
    /// Executes a stored procedure with optional parameters.
    /// Returns whatever database engine returns for non-query mode.
    /// </summary>
    /// <param name="procedure">Stored procedure name.</param>
    /// <param name="parameters">An object whose properties with names will be used to set command parameters. Use <see cref="DataTable"/> for collections.</param>
    /// <returns>Number of affected rows or other integer the specific database engine returns for non-query mode.</returns>
    public int Exec(string procedure, object? parameters = null) {
        if (Connection.State != ConnectionState.Open) Connection.Open();
        using var cmd = Connection.CreateCommand();
        if (Transaction is not null) cmd.Transaction = Transaction;
        cmd.CommandTimeout = Timeout;
        cmd.CommandText = procedure;
        cmd.CommandType = CommandType.StoredProcedure;
        if (parameters is not null) cmd.SetParameters<TParam>(parameters);
        return cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Executes a stored procedure with optional parameters.
    /// Returns whatever database engine returns for non-query mode.
    /// </summary>
    /// <param name="procedure">Stored procedure name.</param>
    /// <param name="parameters">An object whose properties with names will be used to set command parameters. Use <see cref="DataTable"/> for collections.</param>
    /// <returns>A <see cref="ValueTask"/> returning a number of affected rows or other integer the specific database engine returns for non-query mode.</returns>
    public async ValueTask<int> ExecAsync(string procedure, object? parameters = null) {
        if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();
        await using var cmd = Connection.CreateCommand();
        if (Transaction is not null) cmd.Transaction = Transaction;
        cmd.CommandTimeout = Timeout;
        cmd.CommandText = procedure;
        cmd.CommandType = CommandType.StoredProcedure;
        if (parameters is not null) cmd.SetParameters<TParam>(parameters);
        return await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Executes a stored procedure with optional parameters to fetch data.
    /// </summary>
    /// <typeparam name="TElement">Returned element type.</typeparam>
    /// <param name="procedure">Stored procedure name.</param>
    /// <param name="parameters">An object whose properties with names will be used to set command parameters. Use <see cref="DataTable"/> for collections.</param>
    /// <returns>A collection of elements.</returns>
    public IEnumerable<TElement> Query<TElement>(string procedure, object? parameters = null) where TElement : new() {
        if (Connection.State != ConnectionState.Open) Connection.Open();
        using var cmd = Connection.CreateCommand();
        if (Transaction is not null) cmd.Transaction = Transaction;
        cmd.CommandTimeout = Timeout;
        cmd.CommandText = procedure;
        cmd.CommandType = CommandType.StoredProcedure;
        if (parameters is not null) cmd.SetParameters<TParam>(parameters);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            if (reader.ReadToPropertiesOfNew<TElement>() is TElement item)
                yield return item;
    }

    /// <summary>
    /// Executes a stored procedure with some optional parameters to fetch data.
    /// </summary>
    /// <typeparam name="TElement">Returned element type.</typeparam>
    /// <param name="procedure">Stored procedure name.</param>
    /// <param name="parameters">An object whose properties with names will be used to set command parameters. Use <see cref="DataTable"/> for collections.</param>
    /// <returns>An asynchronous stream of elements.</returns>
    public async IAsyncEnumerable<TElement> QueryAsync<TElement>(string procedure, object? parameters = null) where TElement : new() {
        if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();
        await using var cmd = Connection.CreateCommand();
        if (Transaction is not null) cmd.Transaction = Transaction;
        cmd.CommandTimeout = Timeout;
        cmd.CommandText = procedure;
        cmd.CommandType = CommandType.StoredProcedure;
        if (parameters is not null) cmd.SetParameters<TParam>(parameters);
        await using var reader = cmd.ExecuteReader();
        while (await reader.ReadAsync())
            if (reader.ReadToPropertiesOfNew<TElement>() is TElement item)
                yield return item;
    }

    /// <summary>
    /// Executes a stored procedure with some parameters to fetch data. The rows will be fetched as arrays of objects.
    /// </summary>
    /// <param name="procedure">Stored procedure name.</param>
    /// <param name="parameters">An object whose properties with names will be used to set command parameters. Use <see cref="DataTable"/> for collections.</param>
    /// <returns>A collection of rows.</returns>
    public IEnumerable<object[]> QueryRaw(string procedure, object? parameters = null) {
        if (Connection.State != ConnectionState.Open) Connection.Open();
        using var cmd = Connection.CreateCommand();
        if (Transaction is not null) cmd.Transaction = Transaction;
        cmd.CommandTimeout = Timeout;
        cmd.CommandText = procedure;
        cmd.CommandType = CommandType.StoredProcedure;
        if (parameters is not null) cmd.SetParameters<TParam>(parameters);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);
            yield return values;
        }
    }

    /// <summary>
    /// Executes a stored procedure with some optional parameters to fetch data. The rows will be fetched as arrays of objects.
    /// </summary>
    /// <param name="procedure">Stored procedure name.</param>
    /// <param name="parameters">An object whose properties with names will be used to set command parameters. Use <see cref="DataTable"/> for collections.</param>
    /// <returns>An asynchronous stream of rows.</returns>
    public async IAsyncEnumerable<object[]> QueryRawAsync(string procedure, object? parameters = null) {
        if (Connection.State != ConnectionState.Open) await Connection.OpenAsync();
        await using var cmd = Connection.CreateCommand();
        if (Transaction is not null) cmd.Transaction = Transaction;
        cmd.CommandTimeout = Timeout;
        cmd.CommandText = procedure;
        cmd.CommandType = CommandType.StoredProcedure;
        if (parameters is not null) cmd.SetParameters<TParam>(parameters);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) {
            var values = new object[reader.FieldCount];
            reader.GetValues(values);
            yield return values;
        }
    }

    /// <summary>
    /// Disposes the connection (and transaction if applicable).
    /// </summary>
    public void Dispose() {
        Transaction?.Dispose();
        Connection.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the connection (and transaction if applicable).
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> completed when everything is disposed.</returns>
    public async ValueTask DisposeAsync() {
        if (Transaction is not null) await Transaction.DisposeAsync();
        if (Connection is not null) await Connection.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Local common transaction reference.
    /// </summary>
    private DbTransaction? Transaction;

}