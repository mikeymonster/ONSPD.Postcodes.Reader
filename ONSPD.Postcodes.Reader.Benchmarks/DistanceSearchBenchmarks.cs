using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ONSPD.Postcodes.Reader.Data;
using ONSPD.Postcodes.Reader.Model.Enums;
using ONSPD.Postcodes.Reader.Services;

namespace ONSPD.Postcodes.Reader.Benchmarks
{
    [MinColumn]
    [MaxColumn]
    public class DistanceSearchBenchmarks
    {
        //TODO: Move to config file
        private string _connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=Postcodes;Trusted_Connection=True;";

        private readonly IDataRepository _dataRepository;
        private readonly IPostcodeService _postcodeService;

        public DistanceSearchBenchmarks()
        {
            var configuration = Substitute.For<IConfiguration>();
            var logger = Substitute.For<ILogger<PostcodeService>>();

            _dataRepository = new DataRepository(_connectionString);
            _postcodeService = new PostcodeService(
                configuration,
_dataRepository,
logger);
        }

        [Benchmark(Description = "Search - SQL Spatial")]
        public void SearchWithSqlSpatialBenchmark() =>
            _postcodeService.Search("CV1 2WT", "OX%").GetAwaiter().GetResult();

        [Benchmark(Description = "Search - Haversine")]
        public void SearchWithHaversineBenchmark() =>
            _postcodeService.Search("CV1 2WT", "O%", SearchMethod.Haversine).GetAwaiter().GetResult();
    }
}
