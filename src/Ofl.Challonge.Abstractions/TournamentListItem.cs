using System;

namespace Ofl.Challonge
{
    public class TournamentListItem
    {
        public string Name { get; set; }

        public Uri Url { get; set; }

        public string Game { get; set; }

        public string Type { get; set; }

        public int Participants { get; set; }

        public DateTime CreatedOn { get; set; }

        public int Progress { get; set; }
    }
}
