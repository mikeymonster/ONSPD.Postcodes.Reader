using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ONSPD.Postcodes.Reader.Data;
using ONSPD.Postcodes.Reader.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ONSPD.Postcodes.Reader.Services
{
    public class PostcodeReaderService : IPostcodeReaderService
    {
        private readonly IConfiguration _configuration;
        private readonly IDataRepository _dataRepository;
        private readonly ILogger _logger;

        public PostcodeReaderService(
            IConfiguration configuration,
            IDataRepository dataRepository,
            ILogger<PostcodeReaderService> logger)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _dataRepository = dataRepository ?? throw new ArgumentNullException(nameof(dataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<long> LoadPostcodes()
        {
            var path = _configuration.GetValue<string>("PostcodesFilePath");

            var count = 0L;
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
                    };

                    //https://andrewlock.net/why-is-string-gethashcode-different-each-time-i-run-my-program-in-net-core/
                    //var hashCode = postcode.Postcode.GetHashCode();
                    var hashCode = CreateMD5Hash(postcode.Postcode);
                    if (postcodesDictionary.ContainsKey(hashCode))
                    {
                        _logger.LogInformation($"collision for {postcode.Postcode} {postcodesDictionary[hashCode].Postcode}");
                    }

                    postcodesDictionary.Add(hashCode, postcode);
                    //await _dataRepository.AddPostcode(postcode);

                    //_logger.LogInformation($"{postcodesDictionary.Count} - {postcode.Postcode} - {postcode.Latitude}, {postcode.Longitude}");

                    //if (postcodesDictionary.Count >= 5) break;
                }

                stopwatch.Stop();
                _logger.LogInformation($"Postcodes dictionary has {postcodesDictionary.Count} items. Time taken {stopwatch.ElapsedMilliseconds:#,##0}ms ({stopwatch.ElapsedTicks} ticks)");

                stopwatch.Restart();

                await _dataRepository.UpsertPostcodes(postcodesDictionary.Values);

                stopwatch.Stop();
                _logger.LogInformation($"Saved postcodes to database. Time taken {stopwatch.ElapsedMilliseconds:#,##0}ms ({stopwatch.ElapsedTicks} ticks)");

            }
            catch (Exception ex)
            {
                throw;
            }

            //read pcds
            return postcodesDictionary.Count;
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
