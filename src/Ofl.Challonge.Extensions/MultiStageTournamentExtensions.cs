using System;
using System.Linq;

namespace Ofl.Challonge
{
    public static class MultiStageTournamentExtensions
    {
        public static bool IsMultiStageTournament(this TournamentBracketResponse response)
        {
            // Validate parameters.
            if (response == null) throw new ArgumentNullException(nameof(response));

            // Call with the bracket.
            return response.Bracket.IsMultiStageTournament();
        }

        public static bool IsMultiStageTournament(this TournamentBracketPage page)
        {
            // Validate parameters.
            if (page == null) throw new ArgumentNullException(nameof(page));

            // Call with the bracket.
            return page.Bracket.IsMultiStageTournament();
        }

        public static bool IsMultiStageTournament(this TournamentBracket root)
        {
            // Validate parameters.
            if (root == null) throw new ArgumentNullException(nameof(root));

            // If there are multiple levels.
            if (root.Groups.Count > 0) return true;


            // Are any of the matches group stage matches?
            return root
                .GetTournamentBrackets()
                .SelectMany(b => b.GetTournamentBracketMatches())
                .Any(m => m.IsGroupMatch);
        }
    }
}
