using System.Globalization;
using System.Text.Json.Serialization;

namespace InvertirOnlineApp.Models
{
    public class TwelveDataResponse
    {
        [JsonPropertyName("meta")]
        public MetaData? Meta { get; set; }

        [JsonPropertyName("values")]
        public List<PriceValue> Values { get; set; }
            = new();

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
    }

    public class MetaData
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = "";

        [JsonPropertyName("interval")]
        public string Interval { get; set; } = "";

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "";

        [JsonPropertyName("exchange")]
        public string Exchange { get; set; } = "";
    }

    public class PriceValue
    {
        [JsonPropertyName("datetime")]
        public DateTime DateTime { get; set; }

        [JsonPropertyName("open")]
        public string OpenRaw { get; set; } = "";

        [JsonPropertyName("high")]
        public string HighRaw { get; set; } = "";

        [JsonPropertyName("low")]
        public string LowRaw { get; set; } = "";

        [JsonPropertyName("close")]
        public string CloseRaw { get; set; } = "";

        [JsonPropertyName("volume")]
        public string VolumeRaw { get; set; } = "";

        // =============================================
        // PARSED VALUES
        // =============================================

        [JsonIgnore]
        public decimal Open =>
            decimal.Parse(
                OpenRaw,
                CultureInfo.InvariantCulture);

        [JsonIgnore]
        public decimal High =>
            decimal.Parse(
                HighRaw,
                CultureInfo.InvariantCulture);

        [JsonIgnore]
        public decimal Low =>
            decimal.Parse(
                LowRaw,
                CultureInfo.InvariantCulture);

        [JsonIgnore]
        public decimal Close =>
            decimal.Parse(
                CloseRaw,
                CultureInfo.InvariantCulture);

        [JsonIgnore]
        public long Volume =>
            long.Parse(
                VolumeRaw,
                CultureInfo.InvariantCulture);
    }
}