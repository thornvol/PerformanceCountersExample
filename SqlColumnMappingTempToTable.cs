using System.Collections.Generic;
using System.Data.SqlClient;

namespace TmsDataAccess.Sql
{
    public interface ISqlColumnMappingTempToTableParams
    {
        int Ordinal { get; set;  }
        string TableName { get; set; }
        string ConnectionString { get; set; }
        string TempTableName { get; set; }
        bool IsHeaderTable { get; set; }
        string IdentityColumnName { get; set; }
        string ParentIdentityColumnName { get; set; }
        bool DoNotUseTempDb { get; set; }
    }

    public sealed class SqlColumnMappingTempToTableParams : ISqlColumnMappingTempToTableParams
    {
        public int Ordinal { get; set; }
        public string TableName { get; set; }
        public string ConnectionString { get; set; }
        public string TempTableName { get; set; }
        public bool IsHeaderTable { get; set; }
        public string IdentityColumnName { get; set; }
        public string ParentIdentityColumnName { get; set; }
        public bool DoNotUseTempDb { get; set; }
    }

    public interface ISqlColumnMappingTempToTable
    {
        int Ordinal { get;  }
        bool IsValidMapping { get;  }
        bool IsHeaderTable { get; }
        ISqlColumnMappingTempToTableParams Params { get; }
        IEnumerable<SqlTableSchema> TableSchema { get;  }
        IEnumerable<SqlTableSchema> TempTableSchema { get;  }
        IEnumerable<SqlBulkCopyColumnMapping> Mappings { get; }
    }

    public sealed class SqlColumnMappingTempToTable : ISqlColumnMappingTempToTable
    {
        public int Ordinal { get; private set; }
        public bool IsValidMapping { get; private set; }
        public bool IsHeaderTable { get; private set; }
        public ISqlColumnMappingTempToTableParams Params { get; private set; }
        public IEnumerable<SqlTableSchema> TableSchema { get; private set; }
        public IEnumerable<SqlTableSchema> TempTableSchema { get; private set; }
        public IEnumerable<SqlBulkCopyColumnMapping> Mappings { get; private set; }


        private SqlColumnMappingTempToTable(ISqlColumnMappingTempToTableParams sqlColumnMappingTempToTableParams)
        {
            this.Ordinal = -1; // Will be set by Init(); 
            this.Params = sqlColumnMappingTempToTableParams;
            this.Init();
        }
        private void Init()
        {
            //   System.Diagnostics.Debugger.Launch();
            if(SqlHelper.DoesTableExist(this.Params.TableName, this.Params.ConnectionString))
                this.TableSchema = SqlHelper.GetSqlTableSchema(this.Params.TableName, this.Params.ConnectionString);
            if (SqlHelper.DoesTableExist(this.Params.TempTableName, this.Params.ConnectionString, (this.Params.DoNotUseTempDb)?string.Empty:"tempdb"))
                this.TempTableSchema = SqlHelper.GetSqlTableSchema(this.Params.TempTableName, this.Params.ConnectionString, (this.Params.DoNotUseTempDb) ? string.Empty : "tempdb");
            if (this.TableSchema != null && this.TempTableSchema != null)
            {
                this.Mappings = SqlHelper.GetBulkCopyColumnMapping(this.TempTableSchema, this.TableSchema);
                this.IsValidMapping = true;
                this.Ordinal = this.Params.Ordinal;
                this.IsHeaderTable = this.Params.IsHeaderTable; 
            }
        }

        public static SqlColumnMappingTempToTable CreateFrom(ISqlColumnMappingTempToTableParams sqlColumnMappingTempToTableParams)
        {
            return new SqlColumnMappingTempToTable(sqlColumnMappingTempToTableParams);
        }
    }
}