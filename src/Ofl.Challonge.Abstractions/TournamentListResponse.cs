using System.Collections.Generic;

namespace Ofl.Challonge
{
    public class TournamentListResponse
    {
        public TournamentListRequest Request { get; set; }

        public int Total { get; set; }

        public bool TotalIsApproximate { get; set; }

        public int Offset { get; set; }

        public IReadOnlyCollection<TournamentListItem> Tournaments { get; set; }
    }
}
