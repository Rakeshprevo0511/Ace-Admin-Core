using System.Globalization;
using System.Net.Http.Headers;
using Ace_Admin.Dto;
using Ace_Admin.Models;
using CsvHelper;

namespace Ace_Admin.Services
{
    public class Upservecies
    {
        private readonly PracticeDbContext _context;
        private readonly IConfiguration _config;
        private readonly HttpClient _http;

        public Upservecies(PracticeDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            _http = new HttpClient();
        }

        public async Task<int> ImportInstrumentsAsync()
        {
            var accessToken = _config["Upstox:AccessToken"]; // store this in appsettings.json
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var url = "https://api.upstox.com/v2/instruments";
            var response = await _http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch instrument list");

            var csvData = await response.Content.ReadAsStringAsync();

            using var reader = new StringReader(csvData);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var records = csv.GetRecords<InstrumentCsv>().ToList();

            // Clear existing data
            _context.Instruments.RemoveRange(_context.Instruments);
            await _context.SaveChangesAsync();

            // Convert & insert
            var instruments = records.Select(r => new Instrument
            {
                Exchange = r.exchange,
                Token = r.token,
                Symbol = r.symbol,
                Name = r.name,
                Isin = r.isin,
                InstrumentType = r.instrument_type,
                TickSize = r.tick_size
            });

            await _context.Instruments.AddRangeAsync(instruments);
            return await _context.SaveChangesAsync();
        }

    }
}
