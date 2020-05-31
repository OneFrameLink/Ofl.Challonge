using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ofl.Challonge
{
    public class TournamentBracketTournament
    {
        public int Id { get; set; }

        [JsonProperty("tournament_id")]
        public int? TournamentId { get; set; }

        public string State { get; set; }

        [JsonProperty("tournament_type")]
        public string TournamentType { get; set; }

        [JsonProperty("quick_advance")]
        public bool QuickAdvance { get; set; }

        [JsonProperty("hide_seeds")]
        public bool HideSeeds { get; set; }

        [JsonProperty("hide_identifiers")]
        public bool HideIdentifiers { get; set; }

        public bool Animated { get; set; }

        [JsonProperty("accept_attachements")]
        public bool AcceptAttachments { get; set; }

        [JsonProperty("participant_count_to_advance")]
        public int ParticipantCountToAdvance { get; set; }

        [JsonProperty("owner_ids")]
        public IReadOnlyCollection<int> OwnerIds { get; set; }

        [JsonProperty("admin_ids")]
        public IReadOnlyCollection<int> AdminIds { get; set; }

        [JsonProperty("participants_swappable")]
        public bool ParticipantsSwappable { get; set; }

        [JsonProperty("progress_meter")]
        public int ProgressMeter { get; set; }

        [JsonProperty("group_stage_progress_meter")]
        public int GroupStageProgressMeter { get; set; }

        [JsonProperty("grand_finals_modifier")]
        public string GrandFinalsModifier { get; set; }

        [JsonProperty("predict_the_losers_bracket")]
        public bool? PredictTheLosersBracket { get; set; }
    }
}
