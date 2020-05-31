using System;
using System.Collections.Generic;
using System.Linq;
using Ofl.Linq;

namespace Ofl.Challonge
{
    public static class TournamentBracketExtensions
    {
        public static IEnumerable<TournamentBracket> GetTournamentBrackets(this TournamentBracket bracket)
        {
            // Validate parameters.
            if (bracket == null) throw new ArgumentNullException(nameof(bracket));

            // The implementation.
            IEnumerable<TournamentBracket> Implementation() {
                // The stack of brackets.
                var stack = new Stack<TournamentBracket>(EnumerableExtensions.From(bracket));

                // While there are items on the stack.
                while (stack.Count > 0)
                {
                    // Pop the item.
                    TournamentBracket popped = stack.Pop();

                    // Yield.
                    yield return popped;

                    // Add the groups to the stack.
                    foreach (TournamentBracket group in popped.Groups)
                        // Add.
                        stack.Push(group);
                }
            }

            // Return the implementation.
            return Implementation();
        }

        public static IEnumerable<TournamentBracketMatch> GetTournamentBracketMatches(this TournamentBracket bracket)
        {
            // Validate parameters.
            if (bracket == null) throw new ArgumentNullException(nameof(bracket));

            // Get the third place match, as well as the matches by round.
            return EnumerableExtensions.From(bracket.ThirdPlaceMatch)
                .Concat(bracket.MatchesByRound.SelectMany(r => r.Value))
                .Where(m => m != null);
        }

        private static string GetSingleStageType(this TournamentBracket bracket)
        {
            // Validate parameters.
            if (bracket == null) throw new ArgumentNullException(nameof(bracket));

            // Return distinct stage types.
            return bracket
                .Rounds
                .Select(r => r.StageType)
                .Distinct()
                .Single();
        }

        public static bool IsGroupStage(this TournamentBracket bracket) =>
            bracket.GetSingleStageType() == StageType.GroupStage;

        public static bool IsTournament(this TournamentBracket bracket) =>
            bracket.GetSingleStageType() == StageType.Tournament;
    }
}
