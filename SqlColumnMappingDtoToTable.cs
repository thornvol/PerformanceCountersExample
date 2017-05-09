using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace TmsDataAccess.Sql
{
    public interface ISqlColumnMappingDtoToTable
    {
        string ConnectionString { get; }
        string TableName { get; }
        Type ObjectType { get; }
        IEnumerable<SqlTableSchema> TableSchema { get; set; }
        IEnumerable<SqlBulkCopyColumnMapping> Mappings { get; }
    }

    public sealed class SqlColumnMappingDtoToTable : ISqlColumnMappingDtoToTable
    {
        public string ConnectionString { get; private set;  }
        public string TableName { get; private set; }
        public Type ObjectType { get; private set; }
        public IEnumerable<SqlTableSchema> TableSchema { get; set; }
        public IEnumerable<SqlBulkCopyColumnMapping> Mappings { get; private set; }

        private SqlColumnMappingDtoToTable(string tableName, string connectionString, Type objectType)
        {
            this.TableName = tableName;
            this.ConnectionString = connectionString;
            this.ObjectType = objectType; 

            this.Init();
        }
        private void Init()
        {
         //   System.Diagnostics.Debugger.Launch();
            this.TableSchema = SqlHelper.GetSqlTableSchema(this.TableName, this.ConnectionString);

            var method = typeof(SqlHelper).GetMethod("GetBulkCopyColumnMapping", new Type[] { typeof(IEnumerable<SqlTableSchema>) });
            var generic = method.MakeGenericMethod(this.ObjectType);
            this.Mappings = generic.Invoke(null, new object[] { this.TableSchema }) as IEnumerable<SqlBulkCopyColumnMapping>;
        }

        public static SqlColumnMappingDtoToTable GetMapping<T>(string tableName, string connectionString)
        {
            return GetMapping(tableName, connectionString, typeof (T)); 
        }

        public static SqlColumnMappingDtoToTable GetMapping(string tableName, string connectionString, Type objectType)
        {
            return new SqlColumnMappingDtoToTable(tableName, connectionString, objectType);
        }
    }
}