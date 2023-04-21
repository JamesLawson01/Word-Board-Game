using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WordBoardGame
{
    public class Ngram
    {
        [JsonPropertyName("timeseries")]
        public List<float> Timeseries { get; set; }
    }
}
