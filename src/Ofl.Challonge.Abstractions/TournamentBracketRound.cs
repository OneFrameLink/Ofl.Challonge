using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ofl.Challonge
{
    public class TournamentBracketRound
    {
        public int? Id { get; set; }

        [JsonProperty("stage_type")]
        public string StageType { get; set; }

        [JsonProperty("stage_id")]
        public int StageId { get; set; }

        public int Number { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        [JsonProperty("best_of")]
        public int BestOf { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("updated_at")]
        public string UpdatedAt { get; set; }

        [JsonProperty("group_index")]
        public string GroupIndex { get; set; }

        [JsonProperty("title_lines")]
        public IReadOnlyCollection<string> TitleLines { get; set; }

        public string Href { get; set; }
    }
}
