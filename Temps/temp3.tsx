info: 09/21/2025 13:53:11.860 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `PARTY_TAX_AUTH_INFO` AS `p`)
info: 09/21/2025 13:53:11.864 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `ACCTG_TRANS` AS `a`)
info: 09/21/2025 13:53:11.869 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `ACCTG_TRANS_ENTRY` AS `a`)
info: 09/21/2025 13:53:11.872 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `PARTY_GL_ACCOUNT` AS `p`)
info: 09/21/2025 13:53:11.879 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (4ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `PARTY_CLASSIFICATION_TYPE` AS `p`)
info: 09/21/2025 13:53:11.882 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `PARTY_CLASSIFICATION_GROUP` AS `p`)
info: 09/21/2025 13:53:11.886 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `QUALITY_STANDARD` AS `q`)
info: 09/21/2025 13:53:11.889 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `TRANSACTION_TYPE_ACCOUNT_RULES` AS `t`)
info: 09/21/2025 13:53:11.996 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (25ms) [Parameters=[@__normalizedName_0='?' (Size = 256)], CommandType='Text', CommandTimeout='30']
SELECT `a`.`Id`, `a`.`ConcurrencyStamp`, `a`.`Name`, `a`.`NormalizedName`, `a`.`PercentageAllowed`
FROM `AspNetRoles` AS `a`
WHERE `a`.`NormalizedName` = @__normalizedName_0
LIMIT 1
info: 09/21/2025 13:53:12.039 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (1ms) [Parameters=[@__normalizedName_0='?' (Size = 256)], CommandType='Text', CommandTimeout='30']
SELECT `a`.`Id`, `a`.`ConcurrencyStamp`, `a`.`Name`, `a`.`NormalizedName`, `a`.`PercentageAllowed`
FROM `AspNetRoles` AS `a`
WHERE `a`.`NormalizedName` = @__normalizedName_0
LIMIT 1
info: 09/21/2025 13:53:12.041 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (0ms) [Parameters=[@__normalizedName_0='?' (Size = 256)], CommandType='Text', CommandTimeout='30']
SELECT `a`.`Id`, `a`.`ConcurrencyStamp`, `a`.`Name`, `a`.`NormalizedName`, `a`.`PercentageAllowed`
FROM `AspNetRoles` AS `a`
WHERE `a`.`NormalizedName` = @__normalizedName_0
LIMIT 1
info: 09/21/2025 13:53:12.052 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `USER_LOGIN` AS `u`)
info: 09/21/2025 13:53:12.058 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT EXISTS (
    SELECT 1
FROM `AspNetUsers` AS `a`)
[13:53:12 WRN T10] Overriding HTTP_PORTS '8080' and HTTPS_PORTS ''. Binding to values defined by URLS instead 'http://*:5100;https://*:8544'.
    [13:53:12 WRN T10] Overriding address(es) 'http://*:5100, https://*:8544'. Binding to endpoints defined via IConfiguration and/or UseKestrel() instead.
    [13:53:57 ERR T3] An exception occurred while iterating over the results of a query for context type 'Persistence.DataContext'.
    System.InvalidOperationException: The configured execution strategy 'MySqlRetryingExecutionStrategy' does not support user-initiated transactions. Use the execution strategy returned by 'DbContext.Database.CreateExecutionStrategy()' to execute all the operations in the transaction as a retriable unit.
    at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.OnFirstExecution()
at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
System.InvalidOperationException: The configured execution strategy 'MySqlRetryingExecutionStrategy' does not support user-initiated transactions. Use the execution strategy returned by 'DbContext.Database.CreateExecutionStrategy()' to execute all the operations in the transaction as a retriable unit.
   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.OnFirstExecution()
   at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
   at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
fail: 09/21/2025 13:53:57.388 CoreEventId.QueryIterationFailed[10100] (Microsoft.EntityFrameworkCore.Query)
An exception occurred while iterating over the results of a query for context type 'Persistence.DataContext'.
    System.InvalidOperationException: The configured execution strategy 'MySqlRetryingExecutionStrategy' does not support user-initiated transactions. Use the execution strategy returned by 'DbContext.Database.CreateExecutionStrategy()' to execute all the operations in the transaction as a retriable unit.
    at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.OnFirstExecution()
at Microsoft.EntityFrameworkCore.Storage.ExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
Result`1[Application.Projects.ProjectDto]
info: 09/21/2025 13:53:58.472 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (5ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
SELECT COUNT(*)
FROM `WORK_EFFORT` AS `w`
LEFT JOIN `STATUS_ITEM` AS `s` ON `w`.`CURRENT_STATUS_ID` = `s`.`STATUS_ID`
WHERE `w`.`WORK_EFFORT_TYPE_ID` = 'PROJECT'
info: 09/21/2025 13:53:58.545 RelationalEventId.CommandExecuted[20101] (Microsoft.EntityFrameworkCore.Database.Command)
Executed DbCommand (1ms) [Parameters=[@__TypedProperty_1='?' (DbType = Int32), @__TypedProperty_0='?' (DbType = Int32)], CommandType='Text', CommandTimeout='30']
SELECT `w`.`WORK_EFFORT_ID` AS `WorkEffortId`, `w`.`PROJECT_NAME` AS `ProjectName`, `w`.`CURRENT_STATUS_ID` AS `CurrentStatusId`, `s`.`DESCRIPTION` AS `CurrentStatusDescription`, `w`.`ESTIMATED_START_DATE` AS `EstimatedStartDate`, `w`.`ESTIMATED_COMPLETION_DATE` AS `EstimatedCompletionDate`
FROM `WORK_EFFORT` AS `w`
LEFT JOIN `STATUS_ITEM` AS `s` ON `w`.`CURRENT_STATUS_ID` = `s`.`STATUS_ID`
WHERE `w`.`WORK_EFFORT_TYPE_ID` = 'PROJECT'
ORDER BY `w`.`WORK_EFFORT_ID`
LIMIT @__TypedProperty_1 OFFSET @__TypedProperty_0