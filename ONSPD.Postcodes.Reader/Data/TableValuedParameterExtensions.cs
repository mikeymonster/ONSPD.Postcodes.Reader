using Dapper;
using ONSPD.Postcodes.Reader.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;

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
            //Dapper.SqlMapper.AsTableValuedParameter
            var try1 = enumerable.AsDataTable(orderedColumnNames).AsTableValuedParameter(typeName);
            //var try2 = enumerable.AsTableValuedParameter(typeName, orderedColumnNames);
            return try1;
        }

        //class EnumerableExtensions
        public static DataTable AsDataTable<T>(this IEnumerable<T> enumerable, IEnumerable<string> orderedColumnNames = null)
        {
            var dataTable = new DataTable();
            if (typeof(T).IsValueType)
            {
                dataTable.Columns.Add("NONAME", typeof(T));
                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(obj);
                }
            }
            else
            {
                PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] readableProperties = properties.Where(w => w.CanRead).ToArray();
                var columnNames = (orderedColumnNames ?? readableProperties.Select(s => s.Name)).ToArray();
                foreach (string name in columnNames)
                {
                    dataTable.Columns.Add(name, readableProperties.Single(s => s.Name.Equals(name)).PropertyType);
                }

                foreach (T obj in enumerable)
                {
                    dataTable.Rows.Add(
                        columnNames.Select(s => readableProperties.Single(s2 => s2.Name.Equals(s)).GetValue(obj))
                            .ToArray());
                }
            }
            return dataTable;
        }
    }
}
