namespace Ofl.Challonge
{
    public class TournamentBracketResponse
    {
        public TournamentBracketRequest Request { get; set; }

        public TournamentPageDetails PageContent { get; set; }

        public TournamentBracket Bracket { get; set; }

        public TournamentTargetingKeyValues TargetingKeyValues { get; set; }

        public TournamentMetaList MetaList { get; set; }
    }
}
