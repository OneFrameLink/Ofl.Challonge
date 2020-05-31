using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Newtonsoft.Json;
using Ofl.Text.RegularExpressions;
using Ofl.Html;
using AngleSharp.Html.Dom;

namespace Ofl.Challonge
{
    public class ChallongeTournamentBracketPageHtmlParser : IChallongeTournamentBracketPageHtmlParser
    {
        #region Implementation of IChallongeTournamentBracketHtmlParser

        public async Task<TournamentBracketPage> ParseTournamentBracketPageAsync(TextReader reader, 
            CancellationToken cancellationToken)
        {
            // Validate parameters.
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            // Get the html document.
            using IHtmlDocument doc = await reader.ToHtmlDocumentAsync(cancellationToken).ConfigureAwait(false);

            // Call the overload.
            return ParseTournamentBracketPage(doc);
        }

        private static readonly Regex BracketJsonRegex = new Regex(@";\s*window\s*\.\s*_initialStoreState\s*\[\s*['""]TournamentStore['""]\s*]\s*=\s*(?<json>{.*?})\s*;",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        private static readonly Regex TargetingKeyValuesJsonRegex = new Regex(@";\s*gon\s*\.\s*targetingKeyValues\s*\=\s*(?<json>{.*?})\s*;",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

        private static TournamentBracketPage ParseTournamentBracketPage(IHtmlDocument htmlDocument)
        {
            // Validate parameters.
            if (htmlDocument == null) throw new ArgumentNullException(nameof(htmlDocument));

            // Extracts the JSON from the page.
            string ExtractJson(Regex regex) => (
                from e in htmlDocument.QuerySelectorAll("script")
                let m = regex.Match(e.InnerHtml)
                where m.Success
                select m.GetGroupValue("json")
            ).Single();

            // Targeting key values.
            string targetingKeyValuesJson = ExtractJson(TargetingKeyValuesJsonRegex);

            // Bracket JSON.
            string bracketJson = ExtractJson(BracketJsonRegex);

            // Create a serializer.
            var serializer = new JsonSerializer();

            // Create the readers.
            using var targetingKeyValuesStringReader = new StringReader(targetingKeyValuesJson);
            using var targetingKeyValuesJsonReader = new JsonTextReader(targetingKeyValuesStringReader);
            using var bracketStringReader = new StringReader(bracketJson);
            using var bracketJsonReader = new JsonTextReader(bracketStringReader);

            // Deserialize.
            return new TournamentBracketPage
            {
                PageContent = ParsePageDetails(htmlDocument),
                Bracket = serializer.Deserialize<TournamentBracket>(bracketJsonReader),
                TargetingKeyValues = serializer.Deserialize<TournamentTargetingKeyValues>(targetingKeyValuesJsonReader),
                MetaList = ParseMetaList(htmlDocument)
            };
        }

        private static TournamentMetaList ParseMetaList(IHtmlDocument document)
        {
            // Validate parameters.
            if (document == null) throw new ArgumentNullException(nameof(document));

            // Get the game and start date.
            string game = document.QuerySelector(".meta-list a")?.Text().Trim();
            string unparsedStartTime = document.QuerySelector(".meta-list #start-time")?.Text().Trim();

            // The parsed date time.
            DateTimeOffset? startTime = null;

            // Try and parse the start time.
            if (DateTimeOffset.TryParseExact(unparsedStartTime, "MMMM d, yyyy 'at' h:mm tt z",
                CultureInfo.GetCultureInfo("en-US"),
                DateTimeStyles.AllowWhiteSpaces, out DateTimeOffset tempStartTime))
                // Assign.
                startTime = tempStartTime;

            // Return.
            return new TournamentMetaList {
                Game = game,
                StartTime = startTime
            };
        }

        private static TournamentPageDetails ParsePageDetails(IHtmlDocument document)
        {
            // Validate parameters.
            if (document == null) throw new ArgumentNullException(nameof(document));

            // Get the title and URL.
            string title = document.QuerySelector("*[data-tournament-name]").Attributes["data-tournament-name"].Value;
            string url = document.QuerySelector("meta[property='og:url']").GetAttribute("content");

            // Return.
            return new TournamentPageDetails {
                Name = title,
                Url = url
            };
        }

        #endregion
    }
}
