using System;

namespace TmsDataAccess.Sql
{
    public class SqlTableSchema
    {
        public String ColumnName { get; set; }
        public Int32 ColumnOrdinal { get; set; }
        public Int32 ColumnSize { get; set; }
        public Int16 NumericPrecision { get; set; }
        public Int16 NumericScale { get; set; }
//        public Boolean IsUnique { get; set; }
//        public Boolean? IsKey { get; set; }
//        public String BaseServerName { get; set; }
//        public String BaseCatalogName { get; set; }
//        public String BaseColumnName { get; set; }
//        public String BaseSchemaName { get; set; }
//        public String BaseTableName { get; set; }
        public Type DataType { get; set; }
        public Boolean AllowDBNull { get; set; }
//        public Int32 ProviderType { get; set; }
//        public Boolean IsAliased { get; set; }
//        public Boolean IsExpression { get; set; }
        public Boolean IsIdentity { get; set; }
//        public Boolean IsAutoIncrement { get; set; }
//        public Boolean IsRowVersion { get; set; }
//        public Boolean IsHidden { get; set; }
//        public Boolean IsLong { get; set; }
        public Boolean IsReadOnly { get; set; }
//        public Type ProviderSpecificDataType { get; set; }
//        public String DataTypeName { get; set; }
//        public String XmlSchemaCollectionDatabase { get; set; }
//        public String XmlSchemaCollectionOwningSchema { get; set; }
//        public String XmlSchemaCollectionName { get; set; }
//        public String UdtAssemblyQualifiedName { get; set; }
//        public Int32 NonVersionedProviderType { get; set; }
//        public Boolean IsColumnSet { get; set; } 
    }
}