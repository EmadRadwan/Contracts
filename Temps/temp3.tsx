Executed DbCommand (13ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='erp_contracts' AND TABLE_NAME='__EFMigrationsHistory';
info: 09/21/2025 11:44:23.959 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (19ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
CREATE TABLE `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;
info: 09/21/2025 11:44:23.972 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (4ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA='erp_contracts' AND TABLE_NAME='__EFMigrationsHistory';
info: 09/21/2025 11:44:23.987 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (9ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT `MigrationId`, `ProductVersion`
FROM `__EFMigrationsHistory`
ORDER BY `MigrationId`;
info: 09/21/2025 11:44:24.003 RelationalEventId.MigrationsNotApplied[20405] (Microsoft.EntityFrameworkCore.Migrations)
No migrations were applied. The database is already up to date.
    [11:44:24 ERR T7] Failed executing DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (

    SELECT 1
FROM `MIME_TYPE` AS `m`)
fail: 09/21/2025 11:44:24.238 RelationalEventId.CommandError[20102] (Microsoft.EntityFrameworkCore.Database.Command)
Failed executing DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `MIME_TYPE` AS `m`)
[11:44:24 ERR T7] An exception occurred while iterating over the results of a query for context type 'Persistence.DataContext'.
MySqlConnector.MySqlException (0x80004005): Table 'erp_contracts.MIME_TYPE' doesn't exist
at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 894
at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 483
   at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
   at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
   at MySqlConnector.MySqlCommand.ExecuteDbDataReader(CommandBehavior behavior) in /_/src/MySqlConnector/MySqlCommand.cs:line 290
   at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.<>c.<MoveNext>b__21_0(DbContext _, Enumerator enumerator)
   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.<>c__DisplayClass28_0`2.<Execute>b__0(DbContext context, TState state)
at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementation[TState,TResult](Func`3 operation, Func`3 verifySucceeded, TState state)
at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
MySqlConnector.MySqlException (0x80004005): Table 'erp_contracts.MIME_TYPE' doesn't exist
   at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 894
   at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
   at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
   at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 483
at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
at MySqlConnector.MySqlCommand.ExecuteDbDataReader(CommandBehavior behavior) in /_/src/MySqlConnector/MySqlCommand.cs:line 290
at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.<>c.<MoveNext>b__21_0(DbContext _, Enumerator enumerator)
    at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.<>c__DisplayClass28_0`2.<Execute>b__0(DbContext context, TState state)
        at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementation[TState,TResult](Func`3 operation, Func`3 verifySucceeded, TState state)
        at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
        at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
        fail: 09/21/2025 11:44:24.263 CoreEventId.QueryIterationFailed[10100] (Microsoft.EntityFrameworkCore.Query)
        An exception occurred while iterating over the results of a query for context type 'Persistence.DataContext'.
        MySqlConnector.MySqlException (0x80004005): Table 'erp_contracts.MIME_TYPE' doesn't exist
        at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 894
        at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
        at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
        at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 483
        at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
        at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
        at MySqlConnector.MySqlCommand.ExecuteDbDataReader(CommandBehavior behavior) in /_/src/MySqlConnector/MySqlCommand.cs:line 290
        at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
        at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
        at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.<>c.<MoveNext>b__21_0(DbContext _, Enumerator enumerator)
            at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.<>c__DisplayClass28_0`2.<Execute>b__0(DbContext context, TState state)
                at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementation[TState,TResult](Func`3 operation, Func`3 verifySucceeded, TState state)
                at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
                at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
                [11:44:24 ERR T7] An error occured during migration
                MySqlConnector.MySqlException (0x80004005): Table 'erp_contracts.MIME_TYPE' doesn't exist
                at MySqlConnector.Core.ServerSession.ReceiveReplyAsync(IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/ServerSession.cs:line 894
                at MySqlConnector.Core.ResultSet.ReadResultSetHeaderAsync(IOBehavior ioBehavior) in /_/src/MySqlConnector/Core/ResultSet.cs:line 37
                at MySqlConnector.MySqlDataReader.ActivateResultSet(CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 130
                at MySqlConnector.MySqlDataReader.InitAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, IDictionary`2 cachedProcedures, IMySqlCommand command, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlDataReader.cs:line 483
                at MySqlConnector.Core.CommandExecutor.ExecuteReaderAsync(CommandListPosition commandListPosition, ICommandPayloadCreator payloadCreator, CommandBehavior behavior, Activity activity, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/Core/CommandExecutor.cs:line 56
                at MySqlConnector.MySqlCommand.ExecuteReaderAsync(CommandBehavior behavior, IOBehavior ioBehavior, CancellationToken cancellationToken) in /_/src/MySqlConnector/MySqlCommand.cs:line 357
                at MySqlConnector.MySqlCommand.ExecuteDbDataReader(CommandBehavior behavior) in /_/src/MySqlConnector/MySqlCommand.cs:line 290
                at Microsoft.EntityFrameworkCore.Storage.RelationalCommand.ExecuteReader(RelationalCommandParameterObject parameterObject)
                at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.InitializeReader(Enumerator enumerator)
                at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.<>c.<MoveNext>b__21_0(DbContext _, Enumerator enumerator)
                    at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.<>c__DisplayClass28_0`2.<Execute>b__0(DbContext context, TState state)
                        at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteImplementation[TState,TResult](Func`3 operation, Func`3 verifySucceeded, TState state)
                        at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.Execute[TState,TResult](TState state, Func`3 operation, Func`3 verifySucceeded)
                        at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.Enumerator.MoveNext()
                        at System.Linq.Enumerable.TryGetSingle[TSource](IEnumerable`1 source, Boolean& found)
                        at lambda_method896(Closure, QueryContext)
                        at Microsoft.EntityFrameworkCore.Query.Internal.QueryCompiler.Execute[TResult](Expression query)
                        at Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryProvider.Execute[TResult](Expression expression)
                        at Persistence.SeedContracts.SeedData(DataContext context, UserManager`1 userManager, RoleManager`1 roleManager) in /src/Persistence/SeedContracts.cs:line 20
                        at Program.<Main>$(String[] args) in /src/API/Program.cs:line 190
                            [11:44:24 WRN T7] Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'http://*:5100;https://*:8544'.
                            [11:44:24 WRN T7] Overriding address(es) 'http://*:5100, https://*:8544'. Binding to endpoints defined via IConfiguration and/or UseKestrel() instead.