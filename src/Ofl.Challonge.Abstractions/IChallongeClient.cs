using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ofl.Challonge
{
    public interface IChallongeClient
    {
        Task<TournamentBracketResponse> GetTournamentBracketAsync(
            TournamentBracketRequest request, 
            TournamentBracketContent content,
            CancellationToken cancellationToken
        );
    }
}
