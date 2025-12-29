namespace Ace_Admin.Dto
{
    public class InstrumentCsv
    {
        public string exchange { get; set; }
        public string token { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public string isin { get; set; }
        public string instrument_type { get; set; }
        public decimal tick_size { get; set; }
    }
}
