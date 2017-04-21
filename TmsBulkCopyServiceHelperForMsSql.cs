using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using TmsDataCore.Contracts.Modules.TmsBulkCopy;

namespace TmsDataService.Modules.TmsBulkCopy
{
    public static class TmsBulkCopyServiceHelperForMsSql
    {
        public static async Task<ITmsBulkCopyActionResult> ExecuteBulkCopyFromMsSqlToMsSql(this ITmsBulkCopyAction action)
        {
            var columnMappings = action.GetColumnMappings();

            var rtn = new TmsBulkCopyActionResult();

            using (var srcConn = action.Source.MyDbConnection as SqlConnection)
            {
                var srcQuery = action.Source.GetQueryStatement();
                System.Diagnostics.Debug.WriteLine("Action {0}, SourceQuery: {1}", action.Ordinal, srcQuery); 
                using(var reader = (SqlDataReader) (await srcConn.ExecuteReaderAsync(srcQuery)))
                {
                    var tarConn = action.Target.MyDbConnection as SqlConnection;
                    if (tarConn == null)
                        throw new ApplicationException("Cannot cast Target.MyDbConnection as SqlConnection");
                    if (tarConn.State == ConnectionState.Closed)
                        tarConn.Open();

                    var transaction = GetCurrentSqlTransactionFromTmsBulkCopyAction(action, tarConn);
                    #region SqlBulkCopy
                    using (var sqlBulkCopy = new SqlBulkCopy(tarConn, SqlBulkCopyOptions.Default, transaction))
                    {
                        try
                        {
                            foreach (var columnMapping in columnMappings)
                                sqlBulkCopy.ColumnMappings.Add(columnMapping.Key, columnMapping.Value);

                            sqlBulkCopy.DestinationTableName = action.Target.TableName;
                            await sqlBulkCopy.WriteToServerAsync(reader);

                            rtn.Success = true;
                        }
                        catch(Exception ex)
                        {
                            #region Exception Block
                            System.Diagnostics.Debug.WriteLine("Error found, Start Rollback {0}", action.Ordinal);
                            try
                            {
                                transaction.Rollback();
                            }
                            catch (Exception exRollback)
                            {
                                NewRelic.Api.Agent.NewRelic.NoticeError(exRollback, new Dictionary<string, string>
                                                                                {
                                                                                    { "ExecuteBulkCopy", "Rollback" },
                                                                                    { "ActionSrcServer" , action.Source.ServerName },
                                                                                    { "ActionSrcDbnameOrLib" , action.Source.DbNameOrLibrary },
                                                                                    { "ActionSrcQuery" , srcQuery },
                                                                                    { "ActionTarDbnameOrLib", action.Target.ServerName},
                                                                                    { "ActionTarDbnameOrLib", action.Target.DbNameOrLibrary},
                                                                                    { "ActionTarTable", action.Target.TableName}
                                                                                });
                            }
                            rtn.Success = false;
                            rtn.Exception = ex;
                            #endregion
                        }
                        finally
                        {
                            if (!action.Parent.UseSameTransaction || (action.Parent.UseSameTransaction && action.IsLastAction()))
                            {
                                transaction.Commit();
                                transaction.Dispose();
                                if (tarConn.State != ConnectionState.Closed)
                                {
                                    tarConn.Close();
                                    tarConn.Dispose();
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
            return rtn;
        }
        private static SqlTransaction GetCurrentSqlTransactionFromTmsBulkCopyAction(ITmsBulkCopyAction action, SqlConnection sqlConn)
        {
            SqlTransaction transaction;
            if (action.TargetTransaction == null)
            {
                transaction = sqlConn.BeginTransaction();
                action.TargetTransaction = transaction;
                System.Diagnostics.Debug.WriteLine("Setup New transaction for Ordinal {0}", action.Ordinal);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Reused transaction for Ordinal {0}", action.Ordinal);
                transaction = action.TargetTransaction as SqlTransaction;
            }
            if (transaction == null)
                throw new ApplicationException("Cannot cast action.TargetTransaction as SqlTransaction");
            return transaction;
        }
    }
}