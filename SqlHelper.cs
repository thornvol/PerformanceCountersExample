using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Dapper;
using TmsDataAccess.Common;

namespace TmsDataAccess.Sql
{
    public static class SqlHelper
    {
        public static bool DoesTableExist(string tableNameToCheck, string connectionString, string overrideDbName = "")
        {
            bool rtn;
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                if (!string.IsNullOrWhiteSpace(overrideDbName))
                    conn.ChangeDatabase(overrideDbName);

                var result = conn.Query<dynamic>(string.Format("select object_id('{0}') as ObjectId", tableNameToCheck), null).FirstOrDefault();
                //rtn = (conn.GetSchema("Tables", new[] { null, null, tableNameToCheck }).Rows.Count == 1);
                rtn = (result.ObjectId != null);
            }
            return (rtn);
        }
        public static IEnumerable<SqlTableSchema> GetSqlTableSchema(string tableName, string connectionString, string overrideDbName = "")
        {
            var dtTemp = GetSchemaTable(tableName, connectionString, overrideDbName);
            var rtn = dtTemp.AsPoco<SqlTableSchema>();
            return rtn;
        }
        public static DataTable GetSchemaTable(string tableName, string connectionString, string overrideDbName = "")
        {
            DataTable dtSchema;
            using (var conn = new SqlConnection(connectionString))
            {

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = string.Format("select top 0 * from dbo.[{0}] with (nolock)", tableName);
                    conn.Open();
                    if (!string.IsNullOrWhiteSpace(overrideDbName))
                        conn.ChangeDatabase(overrideDbName);


                    using (var sdrTemp = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        dtSchema = sdrTemp.GetSchemaTable();
                    }
                }
            }
            return (dtSchema);
        }
        public static string GetTableScript(string sourceTableName,
                    string connectionString
                   , string asNewTableName = ""
                   , string asFileGroup = ""
                   , string primaryKeys = ""
                   , string appendColumns = ""
                   , bool forceDateTimeOverSmallDateTime = false)
        {
            var sbTableScript = new StringBuilder();
            var dtSchema = GetSchemaTable(sourceTableName, connectionString);
            var primaryKeysArray = dtSchema.AsEnumerable()
                .Where(s => primaryKeys.ToLower().IndexOf(s["ColumnName"].ToString().ToLower(), StringComparison.Ordinal) >= 0)
                .Select(s => s["ColumnName"].ToString());

            //dtSchema.Dump();
            var tableScriptQuery = from dr in dtSchema.AsEnumerable().OrderBy(s => s["ColumnOrdinal"])
                                   let columnName = dr["ColumnName"].ToString()
                                   let dataType = dr["DataTypeName"].ToString()
                                   let columnSize = Convert.ToInt32(dr["ColumnSize"])
                                   let numericPrecision = Convert.ToInt32(dr["NumericPrecision"])
                                   let numericScale = Convert.ToInt32(dr["NumericScale"])
                                   select GetTableScriptGetDataSizeScript(columnName, dataType, columnSize, numericPrecision, numericScale, forceDateTimeOverSmallDateTime);
            sbTableScript.Append("create table ");
            sbTableScript.AppendFormat("dbo.[{0}]", (asNewTableName != "") ? asNewTableName : sourceTableName);
            sbTableScript.AppendLine("(");
            sbTableScript.AppendLine(string.Join(",\n", tableScriptQuery.ToArray()));
            if (appendColumns != "")
                sbTableScript.Append("," + appendColumns);

            if (primaryKeysArray.Any() && asNewTableName != "")
                sbTableScript.AppendFormat("CONSTRAINT [PK_{0}] PRIMARY KEY ({1}) WITH (IGNORE_DUP_KEY = ON) \n", asNewTableName
                                        , string.Join(",", primaryKeysArray));
            sbTableScript.AppendLine(")");
            if (!string.IsNullOrEmpty(asFileGroup))
                sbTableScript.AppendFormat(" ON [{0}]", asFileGroup);
            return (sbTableScript.ToString());
        }

        public static void CloneTable(ICloneTableParams cloneTableParams)
        {

            using (var conn = new SqlConnection(cloneTableParams.TargetConnStr))
            {
                conn.Open();
                var tableScript = (DoesTableExist(cloneTableParams.TargetTable, cloneTableParams.TargetConnStr) || (cloneTableParams.ForceRebuild && DoesTableExist(cloneTableParams.TargetTable, cloneTableParams.TargetConnStr))) ? string.Format("drop table dbo.[{0}]; \n", cloneTableParams.TargetTable) : string.Empty;
                tableScript += GetTableScript(cloneTableParams.SourceTable,
                    cloneTableParams.SourceConnStr,
                    cloneTableParams.TargetTable,
                    cloneTableParams.AsFileGroup,
                    cloneTableParams.PrimaryKeys,
                    cloneTableParams.AppendColumns,
                    cloneTableParams.ForceDateTimeOverSmallDateTime);
                using (var cmd = new SqlCommand(tableScript, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        public static void DropTableView(IEnumerable<string> pTableOrViewNames, string connectionString)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (var tableOrView in pTableOrViewNames)
                {

                    var dt = conn.GetSchema("Tables", new[] { null, null, tableOrView });
                    if (dt.Rows.Count > 0)
                    {
                        using (var cmd =
                            new SqlCommand(
                                string.Format("drop {1} dbo.[{0}]; \n"
                                              , tableOrView
                                              ,
                                              ((dt.Rows[0]["Table_Type"].ToString().ToUpper() != "VIEW")
                                                   ? "TABLE"
                                                   : "VIEW")
                                    )
                                , conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get list of columns for bulk insert that matched to the object's properties 
        /// </summary>
        /// <typeparam name="T">your DTO object type</typeparam>
        /// <param name="schema">table schema</param>
        /// <returns></returns>
        public static IEnumerable<SqlBulkCopyColumnMapping> GetBulkCopyColumnMapping<T>(IEnumerable<SqlTableSchema> schema)
        {
            var sourceSchema = typeof(T).GetProperties().Where(x => x.CanRead).Select(x => new { ColumnName = x.Name, DataType = x.PropertyType }).ToArray();
            var targetSchema = schema.AsEnumerable().Select(s => new { s.ColumnName, DataType = s.DataType }).ToArray();
            var queryBulkCopyColumnMapping = from tarCol in targetSchema
                                             join srcCol in sourceSchema
                                             on tarCol.ColumnName.ToLower() equals srcCol.ColumnName.ToLower()
                                             select new SqlBulkCopyColumnMapping(
                                                         srcCol.ColumnName, tarCol.ColumnName);

            return queryBulkCopyColumnMapping.ToArray();
        }

        public static void AddRange(this SqlBulkCopyColumnMappingCollection columnMappings,
                                    IEnumerable<SqlBulkCopyColumnMapping> mappings)
        {
            foreach (var mapping in mappings)
            {
                columnMappings.Add(mapping);
            }
        }



        private static string GetTableScriptGetDataSizeScript(string columnName, string dataType, int columnSize = 0, int numericPrecision = 0, int numericScale = 0, bool forceDateTimeOverSmallDateTime = false)
        {
            var dataSize = string.Empty;
            switch (dataType.ToLowerInvariant())
            {
                case "nvarchar":
                case "nchar":
                case "varchar":
                case "char":
                    dataSize = string.Format("({0})", getColumnSize(dataType, columnSize));
                    break;
                case "numeric":
                case "decimal":
                    dataSize = string.Format("({0},{1})", numericPrecision, numericScale);
                    break;
                case "smalldatetime":
                    dataType = ((forceDateTimeOverSmallDateTime && dataType == "smalldatetime") ? "datetime" : dataType);
                    break;
            }
            var rtn = string.Format("[{0}] {1}{2}", columnName, dataType, dataSize);
            return rtn;
        }

        private static string getColumnSize(string dataType, int columnSize)
        {
            var rtnColumnSize = string.Empty;

            switch (dataType.ToLowerInvariant())
            {
                case "nvarchar":
                case "nchar":
                    rtnColumnSize = columnSize > 4000 ? "max" : columnSize.ToString();
                    break;
                case "varchar":
                case "char":
                    rtnColumnSize = columnSize > 8000 ? "max" : columnSize.ToString();
                    break;
            }

            return rtnColumnSize;
        }
        public static bool AddColumn(string table, string connStr, string columnName, string dataType)
        {
            const string template =
@"if object_id('{0}') is not null begin
    if col_length('{0}','{1}') is null begin
        alter table [{0}] add [{1}] {2};
        exec ('create index [{1}] on [{0}] ([{1}])')
    end 
end";
            using (var conn = new SqlConnection(connStr))
            {
                var sqlString = string.Format(template, table, columnName, dataType);
                var cmd = conn.CreateCommand();
                cmd.CommandText = sqlString;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;
        }

        public static IEnumerable<SqlBulkCopyColumnMapping> GetBulkCopyColumnMapping(IEnumerable<SqlTableSchema> source, IEnumerable<SqlTableSchema> target)
        {
            var queryBulkCopyColumnMapping = from tarCol in target
                                             join srcCol in source
                                             on tarCol.ColumnName.ToLower() equals srcCol.ColumnName.ToLower()
                                             where !tarCol.IsIdentity && !tarCol.IsReadOnly
                                             select new SqlBulkCopyColumnMapping(
                                                         srcCol.ColumnName, tarCol.ColumnName);

            return queryBulkCopyColumnMapping.ToArray();
        }

       

        public static string GetInsertStatement(this IEnumerable<SqlBulkCopyColumnMapping> columnMappings,
            string fullyQualifiedSourceTableName,
            string fullyQualifiedTargetTableName,
            string insertOutputStatement = "",
            string whereStatement = "",
            string orderbyStatement = "")
        {
            var temp = columnMappings.Select(m => new { TarCol = m.DestinationColumn, SrcCol = m.SourceColumn }).ToArray();
            var rtn = string.Format("insert into {0} ({1}) {4} select {2} from {3} {5} {6} ",
                    fullyQualifiedTargetTableName,
                    string.Join(",", temp.Select(t => t.TarCol)),
                    string.Join(",", temp.Select(t => t.SrcCol)),
                    fullyQualifiedSourceTableName,
                    insertOutputStatement,
                    whereStatement,
                    orderbyStatement);
            return rtn;
        }
    }
}
