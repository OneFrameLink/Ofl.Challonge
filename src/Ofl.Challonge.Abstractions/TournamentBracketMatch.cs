using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ofl.Challonge
{
    public class TournamentBracketMatch
    {
        [JsonConverter(typeof(NullableStructDefaultValueJsonConverter<int>))]
        // TODO: Change to null, accept null.
        public int Id { get; set; }

        [JsonProperty("tournament_id")]
        public int TournamentId { get; set; }

        public int Identifier { get; set; }

        public int Round { get; set; }

        public string State { get; set; }

        [JsonProperty("underway_at")]
        public DateTimeOffset? UnderwayAt { get; set; }

        public TournamentBracketPlayer Player1 { get; set; }

        public TournamentBracketPlayer Player2 { get; set; }

        [JsonProperty("player1_prereq_identifier")]
        public int? Player1PrerequisiteIdentifier { get; set; }

        [JsonProperty("player2_prereq_identifier")]
        public int? Player2PrerequisiteIdentifier { get; set; }

        [JsonProperty("player1_is_prereq_match_loser")]
        public bool Player1IsPrerequisiteMatchLoser { get; set; }

        [JsonProperty("player2_is_prereq_match_loser")]
        public bool Player2IsPrerequisiteMatchLoser { get; set; }

        [JsonProperty("player1_placeholder_text")]
        public string Player1PlaceholderText { get; set; }

        [JsonProperty("player2_placeholder_text")]
        public string Player2PlaceholderText { get; set; }

        [JsonProperty("winner_id")]
        public int? WinnerId { get; set; }

        [JsonProperty("loser_id")]
        public int? LoserId { get; set; }

        public IReadOnlyList<int> Scores { get; set; }

        public IReadOnlyList<IReadOnlyList<int>> Games { get; set; }

        [JsonProperty("editable_by_user_ids")]
        public IReadOnlyCollection<int> EditableByUserIds { get; set; }

        [JsonProperty("has_attachment")]
        public bool HasAttachment { get; set; }

        [JsonProperty("is_group_match")]
        public bool IsGroupMatch { get; set; }

        // NOTE: Is null, not sure what the value should be.
        //public object Forfeited { get; set; }

        // ReSharper disable InconsistentNaming
        [JsonProperty("md5")]
        public string MD5 { get; set; }
        // ReSharper restore InconsistentNaming
    }
}
