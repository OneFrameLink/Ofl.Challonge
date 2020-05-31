using System;
using System.Collections.Generic;
using System.Linq;
using Ofl.Collections.Generic;

namespace Ofl.Challonge
{
    public static class TournamentBracketMatchExtensions
    {
        public static IEnumerable<TournamentBracketPlayer> GetDistinctTournamentBracketPlayers(
            this IEnumerable<TournamentBracketMatch> matches)
        {
            // Validate parameters.
            if (matches == null) throw new ArgumentNullException(nameof(matches));

            // Query and return.
            return matches
                // Flatten players.
                .SelectMany(m => new[] {m.Player1, m.Player2})
                // Omit null.
                .Where(p => p != null)
                // Distinct.
                .Distinct(EqualityComparerExtensions.CreateFromProperty<TournamentBracketPlayer, int>(p => p.Id));
        }
    }
}
