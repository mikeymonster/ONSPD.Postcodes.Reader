using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using Microsoft.SqlServer.Server;
using ONSPD.Postcodes.Reader.Model;

namespace ONSPD.Postcodes.Reader.Data
{
    public static class TableValuedParameterExtensions
    {
        //class DapperExtensions
        public static SqlMapper.ICustomQueryParameter AsTableValuedParameter<T>(
            this IEnumerable<T> enumerable,
            string typeName,
            IEnumerable<string> orderedColumnNames = null)
        {
            return enumerable.AsDataTable(orderedColumnNames).AsTableValuedParameter(typeName);
        }

        public static IEnumerable<SqlDataRecord> ToEnumerableSqlDataRecords(this IEnumerable<PostcodeLocation> postcodeLocations)
        {
            var dataRecord = new SqlDataRecord(
                new SqlMetaData("Postcode", SqlDbType.VarChar, 10),
                new SqlMetaData("Latitude", SqlDbType.Decimal, 9, 6),
                new SqlMetaData("Longitude", SqlDbType.Decimal, 9, 6));

            foreach (var postcodeLocation in postcodeLocations)
            {
                dataRecord.SetSqlString(0, postcodeLocation.Postcode);
                dataRecord.SetSqlDecimal(1, Convert.ToDecimal(postcodeLocation.Latitude));
                dataRecord.SetSqlDecimal(2, Convert.ToDecimal(postcodeLocation.Longitude));
                yield return dataRecord;
            }
        }

        //public static SqlMapper.ICustomQueryParameter AsTableValuedParameterYielded<T>(
        //    this IEnumerable<T> enumerable,
        //    string typeName,
        //    IEnumerable<string> orderedColumnNames = null)
        //{
        //    var dataTable = enumerable.AsDataTable(orderedColumnNames);
        //    foreach (var item in dataTable.Rows)
        //    {
        //        yield return (new List<DataRow> { item as DataRow }).AsTableValuedParameter(typeName);
        //    }
        //}

        //class EnumerableExtensions

        public static DataTable AsDataTable<T>(
            this IEnumerable<T> data,
            IEnumerable<string> orderedColumnNames = null)
        {
            var dataTable = new DataTable();
            if (typeof(T).IsValueType)
            {
                dataTable.Columns.Add("NONAME", typeof(T));
                foreach (T obj in data)
                {
                    dataTable.Rows.Add(obj);
                }
            }
            else
            {
                var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var readableProperties = properties.Where(w => w.CanRead);//.ToArray();
                var columnNames = (orderedColumnNames ?? readableProperties.Select(s => s.Name));//.ToArray();
                foreach (string name in columnNames)
                {
                    dataTable.Columns.Add(name, readableProperties.Single(s => s.Name.Equals(name)).PropertyType);
                }

                foreach (T obj in data)
                {
                    dataTable.Rows.Add(
                        columnNames.Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj))
                            .ToArray());
                }
            }

            return dataTable;
        }

        //public static IEnumerable<DataRow> AsDataRowCollection<T>(
        //    this IEnumerable<T> data,
        //    IEnumerable<string> orderedColumnNames = null)
        //{
        //    var dataTable = new DataTable();
        //    if (typeof(T).IsValueType)
        //    {
        //        dataTable.Columns.Add("NONAME", typeof(T));
        //        foreach (T obj in data)
        //        {
        //            dataTable.Rows.Add(obj);
        //        }
        //    }
        //    else
        //    {
        //        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //        var readableProperties = properties.Where(w => w.CanRead);//.ToArray();
        //        var columnNames = (orderedColumnNames ?? readableProperties.Select(s => s.Name));//.ToArray();
        //        foreach (string name in columnNames)
        //        {
        //            dataTable.Columns.Add(name, readableProperties.Single(s => s.Name.Equals(name)).PropertyType);
        //        }

        //        foreach (T obj in data)
        //        {
        //            //dataTable.Rows.Add(
        //            yield return
        //                columnNames.Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj))
        //                    ;
        //        }
        //    }

        //return dataTable;
        //}

        //private static IEnumerable<SqlDataRecord> SendRows(Dictionary<string, int> RowData)
        //public static IEnumerable<SqlDataRecord> GetDataRows<T>(
        //    this IEnumerable<T> data,
        //    string typeName,
        //    IEnumerable<string> orderedColumnNames = null)
        //{
        //    //var _TvpSchema = new SqlMetaData[] {
        //    //    new SqlMetaData("ID", SqlDbType.NVarChar, 4000),
        //    //    new SqlMetaData("SortOrderNumber", SqlDbType.Int)
        //    //};
        //    var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    var readableProperties = properties.Where(w => w.CanRead);//.ToArray();
        //    var columnNames = (orderedColumnNames ?? readableProperties.Select(s => s.Name)).ToArray();

        //    //var _TvpSchema = new SqlMetaData[columnNames.Length];
        //    var tvpSchema = new List<SqlMetaData>();

        //    foreach (string name in columnNames)
        //    {
        //        var t = readableProperties.Single(s => s.Name.Equals(name)).PropertyType;
        //        var y = t.ConvertToSqlDbType();

        //        tvpSchema.Add(new SqlMetaData(
        //            name,
        //            readableProperties
        //            .Single(s => s.Name.Equals(name))
        //            .PropertyType.ConvertToSqlDbType()));
        //    }

        //    var dataRecord = new SqlDataRecord(tvpSchema.ToArray());

        //    //var _FileReader = null;

        //    // read a row, send a row
        //    foreach (T obj in data)
        //    //foreach (KeyValuePair<string, int> _CurrentRow in RowData)
        //    {
        //        // You shouldn't need to call "_DataRecord = new SqlDataRecord" as
        //        // SQL Server already received the row when "yield return" was called.
        //        // Unlike BCP and BULK INSERT, you have the option here to create an
        //        // object, do manipulation(s) / validation(s) on the object, then pass
        //        // the object to the DB or discard via "continue" if invalid.
        //        dataRecord.SetValues(
        //            columnNames
        //            .Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj))
        //            .ToArray());

        //        yield return dataRecord;
        //    }
        //}

        public static SqlDbType ConvertToSqlDbType(this Type giveType)
        {
            //TODO: move type map to static class variable
            var typeMap = new Dictionary<Type, SqlDbType>
            {
                [typeof(string)] = SqlDbType.NVarChar,
                [typeof(char[])] = SqlDbType.NVarChar,
                [typeof(int)] = SqlDbType.Int,
                [typeof(Int32)] = SqlDbType.Int,
                [typeof(Int16)] = SqlDbType.SmallInt,
                [typeof(Int64)] = SqlDbType.BigInt,
                [typeof(Byte[])] = SqlDbType.VarBinary,
                [typeof(Boolean)] = SqlDbType.Bit,
                [typeof(DateTime)] = SqlDbType.DateTime2,
                [typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset,
                [typeof(Decimal)] = SqlDbType.Decimal,
                [typeof(Double)] = SqlDbType.Float,
                [typeof(Decimal)] = SqlDbType.Money,
                [typeof(Byte)] = SqlDbType.TinyInt,
                [typeof(TimeSpan)] = SqlDbType.Time
            };

            return typeMap[(giveType)];
        }
    }
}
