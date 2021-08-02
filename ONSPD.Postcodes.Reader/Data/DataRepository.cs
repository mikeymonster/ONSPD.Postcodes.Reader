using ONSPD.Postcodes.Reader.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.IdentityModel.Protocols;
using System.Data;

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

        public async Task UpsertPostcodes(IEnumerable<PostcodeLocation> postcodes)
        {
            //https://blog.schroederspace.com/tumbleweed-technology/bulk-upsert-with-dapper-and-sql-server

            //https://stackoverflow.com/questions/19957132/pass-dictionarystring-int-to-stored-procedure-t-sql/25815939#25815939
            //https://stackoverflow.com/questions/25770180/how-can-i-insert-10-million-records-in-the-shortest-time-possible/25773471#25773471

            //Testing
            //https://nsubstitute.github.io/help/nsubstitute-analysers/
            //https://mikhail.io/2016/02/unit-testing-dapper-repositories/

            using var connection = new SqlConnection(_connectionString);
            connection.Execute("Postcode_Upsert",
                new
                {
                    data = postcodes.AsTableValuedParameter("dbo.PostcodeDataType")
                },
                commandType: CommandType.StoredProcedure);
        }
    }
}
