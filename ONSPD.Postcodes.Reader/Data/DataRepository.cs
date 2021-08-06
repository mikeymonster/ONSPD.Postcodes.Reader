using ONSPD.Postcodes.Reader.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.Common;

namespace ONSPD.Postcodes.Reader.Data
{
    public class DataRepository : IDataRepository
    {
        private readonly string _connectionString;

        public DataRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task AddPostcode(PostcodeLocation postcode)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<PostcodeLocation>> GetPostcodes()
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();
            var postcodes = await connection.QueryAsync<PostcodeLocation>("SELECT * FROM Postcode");
            return postcodes;
        }

        public async Task<IEnumerable<PostcodeLocation>> GetPostcodes(string filter)
        {
            //var parameters = new DynamicParameters(new { Filter = filter });

            using var connection = new SqlConnection(_connectionString);
            connection.Open();
                       
            var postcodes = await connection
                .QueryAsync<PostcodeLocation>(
                "SELECT * FROM Postcode " +
                "WHERE Postcode LIKE @Filter " +
                "  AND [Location] IS NOT NULL",
                //parameters
                new { Filter = filter }
                );
            return postcodes;
        }

        public async Task<IEnumerable<PostcodeSearchResult>> PerformDistanceSearch(string postcode, string filter)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@fromPostcode", postcode, DbType.String, ParameterDirection.Input);
            parameters.Add("@postcodeDestinationSelector", filter, DbType.String, ParameterDirection.Input);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            var results = await connection.QueryAsync<PostcodeSearchResult>(
                "dbo.Postcode_Distance_Search",
                parameters,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120);

            return results;
        }

        public async Task UpsertPostcodes(IEnumerable<PostcodeLocation> postcodes)
        {
            //https://blog.schroederspace.com/tumbleweed-technology/bulk-upsert-with-dapper-and-sql-server

            //https://stackoverflow.com/questions/19957132/pass-dictionarystring-int-to-stored-procedure-t-sql/25815939#25815939
            //https://stackoverflow.com/questions/25770180/how-can-i-insert-10-million-records-in-the-shortest-time-possible/25773471#25773471

            //TODO: Try with ADO - will it stream properly?
            //https://www.sommarskog.se/arrays-in-sql-2008.html
            //https://www.mssqltips.com/sqlservertip/2338/streaming-rows-of-sql-server-data-to-a-table-valued-parameter-using-a-sqldatareader/

            //https://gist.github.com/taylorkj/9012616
            //https://github.com/DapperLib/Dapper/blob/61e965eed900355e0dbd27771d6469248d798293/Dapper.Tests/Tests.Parameters.cs

            //https://rikgarner.co.uk/2018/03/using-table-valued-parameters-in-dapper/

            //Testing
            //https://nsubstitute.github.io/help/nsubstitute-analysers/
            //https://mikhail.io/2016/02/unit-testing-dapper-repositories/

            using var connection = new SqlConnection(_connectionString);
            await connection.ExecuteAsync("Postcode_Upsert",
                new
                {
                    data = postcodes.AsTableValuedParameter("dbo.PostcodeDataType")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 120);
        }

        public async Task UpsertPostcodesUsingAdo(IEnumerable<PostcodeLocation> postcodes)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("Postcode_Upsert", connection)
            {
                CommandType = CommandType.StoredProcedure,
                CommandTimeout = 120
            };

            var tvParam = new SqlParameter()
            {
                ParameterName = "@data",
                TypeName = "dbo.PostcodeDataType",
                SqlDbType = SqlDbType.Structured,
                Value = postcodes.ToEnumerableSqlDataRecords()
            };                  

            command.Parameters.Add(tvParam);
            
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
