using Newtonsoft.Json;

namespace Ofl.Challonge
{
    public class TournamentBracketPlayer
    {
        public int Id { get; set; }

        public int Seed { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("portrait_url")]
        public string PortraitUrl { get; set; }

        [JsonProperty("participant_id")]
        public int? ParticipantId { get; set; }
    }
}
