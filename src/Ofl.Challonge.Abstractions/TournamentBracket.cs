using System.Collections.Generic;
using Newtonsoft.Json;

namespace Ofl.Challonge
{
    public class TournamentBracket
    {
        [JsonProperty("requested_plotter")]
        public string RequestedPlotter { get; set; }

        public TournamentBracketTournament Tournament { get; set; }

        public IReadOnlyCollection<TournamentBracketRound> Rounds { get; set; }

        [JsonProperty("third_place_match")]
        public TournamentBracketMatch ThirdPlaceMatch { get; set; }

        [JsonProperty("matches_by_round")]
        public IReadOnlyDictionary<int, IReadOnlyCollection<TournamentBracketMatch>> MatchesByRound { get; set; }

        public IReadOnlyCollection<TournamentBracket> Groups { get; set; }

        public string Name { get; set; }

        [JsonProperty("scorecard_html")]
        public string ScorecardHtml { get; set; }
    }
}
