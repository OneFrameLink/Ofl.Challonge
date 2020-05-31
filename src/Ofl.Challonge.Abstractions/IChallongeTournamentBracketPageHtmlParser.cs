using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ofl.Challonge
{
    public interface IChallongeTournamentBracketPageHtmlParser
    {
        Task<TournamentBracketPage> ParseTournamentBracketPageAsync(TextReader reader, CancellationToken cancellationToken);
    }
}
