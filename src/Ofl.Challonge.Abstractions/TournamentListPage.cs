using System.Collections.Generic;

namespace Ofl.Challonge
{
    public class TournamentListPage
    {
        public int Page { get; set; }

        public int Pages { get; set; }

        public IReadOnlyCollection<TournamentListItem> Tournaments { get; set; }
    }
}
