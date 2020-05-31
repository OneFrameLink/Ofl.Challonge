using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using Ofl.Linq;
using Ofl.Text.RegularExpressions;

namespace Ofl.Challonge
{
    public static class TournamentListPageElementExtensions
    {
        public static IReadOnlyCollection<TournamentListItem> ToTournamentListItems(this IElement table)
        {
            // Validate parameters.
            if (table == null) throw new ArgumentNullException(nameof(table));

            // Get the single row in the thead.
            IElement header = table.QuerySelector("thead > tr");

            // Map the headers to the indexes.
            IReadOnlyDictionary<string, int> map = header.
                QuerySelectorAll("th").
                Select((c, i) => new KeyValuePair<string, int>(c.TextContent.Trim(), i)).
                ToReadOnlyDictionary(StringComparer.OrdinalIgnoreCase);

            // Parse the rows in the table.
            IEnumerable<IElement> rows = table.QuerySelectorAll("tbody > tr");

            // Materialize the rows.
            return rows.Select(row => row.ToTournamentListItem(map)).ToReadOnlyCollection();
        }

        private static class TournamentListItemTableColumnHeader
        {
            internal const string Name = "Name";
            internal const string Game = "Game";
            internal const string Type = "Type";
            internal const string Participants = "Participants";
            internal const string CreatedOn = "Created On";
            internal const string Progress = "Progress";
        }

        private static readonly Regex ProgressWidthRegex = new Regex(@"^\s*?width\s*?:\s*?(?<width>[0-9]+)\s*?%\s*?$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private static TournamentListItem ToTournamentListItem(this IElement row, IReadOnlyDictionary<string, int> columnOrdinalMap)
        {
            // Validate parameters.
            if (row == null) throw new ArgumentNullException(nameof(row));
            if (columnOrdinalMap == null) throw new ArgumentNullException(nameof(columnOrdinalMap));

            // Map to an array.
            IElement[] elements = row.QuerySelectorAll("td").ToArray();

            // Map.
            IReadOnlyDictionary<string, IElement> values = columnOrdinalMap
                .Select(p => new KeyValuePair<string, IElement>(p.Key, elements[columnOrdinalMap[p.Key]]))
                .ToReadOnlyDictionary(p => p.Key, p => p.Value);

            // Get the values.

            // Create the item.
            var tournamentListItem = new TournamentListItem {
                Name = values[TournamentListItemTableColumnHeader.Name].GetText(),
                Url = new Uri(values[TournamentListItemTableColumnHeader.Name].QuerySelector("a").Attributes["href"].Value),
                Game = values[TournamentListItemTableColumnHeader.Game].GetText(),
                Type = values[TournamentListItemTableColumnHeader.Type].GetText(),
                Participants = int.Parse(values[TournamentListItemTableColumnHeader.Participants].GetText()),
                CreatedOn = DateTime.ParseExact(values[TournamentListItemTableColumnHeader.CreatedOn].GetText(), "MM-dd-yy",
                    CultureInfo.InvariantCulture),
                Progress =
                    int.Parse(
                        ProgressWidthRegex.Match(
                            values[TournamentListItemTableColumnHeader.Progress].QuerySelector(".progress-bar").Attributes["style"].Value)
                            .GetGroupValue("width"),
                        CultureInfo.InvariantCulture)
            };

            // Fix instance.
            tournamentListItem.Game = tournamentListItem.Game == "–" ? null : tournamentListItem.Game;

            // Return the tournament list item.
            return tournamentListItem;
        }

        private static string GetText(this IElement element)
        {
            // Validate parameters.
            if (element == null) throw new ArgumentNullException(nameof(element));

            // Get the text.
            return element.Text()?.Trim();
        }

        public static int GetLastPage(this IElement element)
        {
            // Validate parameters.
            if (element == null) throw new ArgumentNullException(nameof(element));

            // Look for the last page.
            IElement last = element.QuerySelector(".last");

            // If there's something, extract the page and return.
            if (last != null) return last.GetUrlPage().Value;

            // Get all the pages, return max.
            return element.GetPageElements().
                // Map to a page.
                Select(e => e.GetUrlPage()).
                // Where not null.
                Where(p => p != null).
                // Select the value.
                Select(p => p.Value).
                // Max.
                Max();
        }

        public static int? GetActivePage(this IElement element)
        {
            // Validate parameters.
            if (element == null) throw new ArgumentNullException(nameof(element));

            // Get the page and active.
            IElement activePage = element.QuerySelector(".page.active");

            // Return the page.
            return activePage.GetUrlPage();
        }

        private static IEnumerable<IElement> GetPageElements(this IElement element)
        {
            // Validate parameters.
            if (element == null) throw new ArgumentNullException(nameof(element));

            // Map.
            return element.QuerySelectorAll(".page");
        }

        private static readonly Regex PageRegex = new Regex(@"\?page=(?<page>[0-9]+)$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        private static int? GetUrlPage(this IElement element)
        {
            // Validate parameters.
            if (element == null) throw new ArgumentNullException(nameof(element));

            // Get the href.
            string href = element.QuerySelector("a").Attributes["href"].Value;

            // If the href is just the root, return 1.
            if (href == "/") return 1;

            // Get the groups.
            // TODO: Move to NameValueCollection parsing the query string.
            Group group = PageRegex.Match(href).Groups["page"];

            // If not successful, return null.
            if (!group.Success) return null;

            // Parse an integer.
            return int.Parse(group.Value);
        }
    }
}
