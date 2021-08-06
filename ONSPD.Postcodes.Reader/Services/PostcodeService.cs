using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ONSPD.Postcodes.Reader.Calculators;
using ONSPD.Postcodes.Reader.Data;
using ONSPD.Postcodes.Reader.Model;
using ONSPD.Postcodes.Reader.Model.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ONSPD.Postcodes.Reader.Services
{
    public class PostcodeService : IPostcodeService
    {
        private readonly IConfiguration _configuration;
        private readonly IDataRepository _dataRepository;
        private readonly ILogger _logger;

        public PostcodeService(
            IConfiguration configuration,
            IDataRepository dataRepository,
            ILogger<PostcodeService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<long> LoadPostcodes()
        {
            var path = _configuration.GetValue<string>("PostcodesFilePath");

            var postcodesDictionary = new Dictionary<string, PostcodeLocation>();
            try
            {
                //var existingPostcodes = await _dataRepository.GetPostcodes();

                var stopwatch = Stopwatch.StartNew();

                using var streamReader = new StreamReader(path,
                    new FileStreamOptions
                    {
                        Mode = FileMode.Open,
                        Access = FileAccess.Read,
                        Share = FileShare.ReadWrite
                    });
                using var reader = new CsvReader(streamReader, CultureInfo.CurrentCulture);

                var hasHeader = reader.Configuration.HasHeaderRecord;
                //reader.Configuration.HasHeaderRecord = false;

                await reader.ReadAsync();
                var header = reader.ReadHeader();

                while (await reader.ReadAsync())
                {
                    var postcode = new PostcodeLocation
                    {
                        Postcode = reader.GetField<string>("pcds"),
                        Latitude = reader.GetField<double>("lat"),
                        Longitude = reader.GetField<double>("long")
                        //date introduced in dointr - yyyymm
                        //date terminated in doterm - yyyymm
                    };

                    //https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
                    //var hashCode = postcode.Postcode.GetHashCode();
                    //var hashCode = postcode.Postcode;
                    var hashCode = CreateMD5Hash(postcode.Postcode);
                    //if (postcodesDictionary.ContainsKey(hashCode))
                    //{
                    //    _logger.LogInformation($"collision for {postcode.Postcode} {postcodesDictionary[hashCode].Postcode}");
                    //}

                    postcodesDictionary.Add(hashCode, postcode);

                    //if (postcodesDictionary.Count >= 10) break;
                }

                stopwatch.Stop();
                _logger.LogInformation($"Postcodes dictionary has {postcodesDictionary.Count} items. Time taken {stopwatch.ElapsedMilliseconds:#,##0}ms ({stopwatch.ElapsedTicks} ticks)");

                stopwatch.Restart();

                //await _dataRepository.UpsertPostcodes(postcodesDictionary.Values);
                await _dataRepository.UpsertPostcodesUsingAdo(postcodesDictionary.Values);

                stopwatch.Stop();
                _logger.LogInformation($"Saved postcodes to database. Time taken {stopwatch.ElapsedMilliseconds:#,##0}ms ({stopwatch.ElapsedTicks} ticks)");
            }
            catch (Exception)
            {
                throw;
            }

            //read pcds
            return postcodesDictionary.Count;
        }

        public async Task<IEnumerable<PostcodeSearchResult>> Search(
            string postcode,
            string filter = "OX2%",
            SearchMethod method = SearchMethod.SqlSpatial)
        {
            IEnumerable<PostcodeSearchResult> results = null;

            var stopwatch = Stopwatch.StartNew();

            //Get postcodes
            //search with Haversine;
            switch (method)
            {
                case SearchMethod.Haversine:
                    //Get all data, then calculate distances and sort
                    var fromLatitude = 52.400997;
                    var fromLongitude = -1.508122;

                    var postcodes = await _dataRepository.GetPostcodes(filter);

                    //stopwatch.Stop();
                    //_logger.LogInformation($"Get postcodes for Haversine took {stopwatch.ElapsedMilliseconds:#,##0}ms ({stopwatch.ElapsedTicks} ticks)");
                    //stopwatch.Restart();

                    var unsortedResults = postcodes
                        .Select(p => new PostcodeSearchResult
                        {
                            Postcode = p.Postcode,
                            Distance = Haversine.Distance(
                                fromLatitude, fromLongitude,
                                p.Latitude, p.Longitude)
                        });

                    //stopwatch.Stop();
                    //_logger.LogInformation($"Distance calculation using Haversine took {stopwatch.ElapsedMilliseconds:#,##0}ms ({stopwatch.ElapsedTicks} ticks)");
                    //stopwatch.Restart();

                    results = unsortedResults.OrderBy(p => p.Distance);

                    stopwatch.Stop();
                    //_logger.LogInformation($"Distance sort for Haversine took {stopwatch.ElapsedMilliseconds:#,##0}ms ({stopwatch.ElapsedTicks} ticks)");
                    _logger.LogInformation($"Distance search and sort for Haversine took {stopwatch.ElapsedMilliseconds:#,##0}ms ({stopwatch.ElapsedTicks} ticks)");
                    break;
                case SearchMethod.SqlSpatial:
                    //Call data method that calls stored proc
                    results = await _dataRepository.PerformDistanceSearch(postcode, filter);

                    stopwatch.Stop();
                    _logger.LogInformation($"Distance search using sql spatial took {stopwatch.ElapsedMilliseconds:#,##0}ms ({stopwatch.ElapsedTicks} ticks)");
                    break;
            }

            return results;
        }

        private string CreateMD5Hash(string input)
        {
            // Step 1, calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            // Step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
