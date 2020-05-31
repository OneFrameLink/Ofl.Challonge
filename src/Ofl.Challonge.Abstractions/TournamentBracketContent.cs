using Microsoft.Net.Http.Headers;
using System;
using System.IO;

namespace Ofl.Challonge
{
    public class TournamentBracketContent : IDisposable
    {
        #region Constructor

        public TournamentBracketContent(
            MediaTypeHeaderValue mediaType,
            Stream content
        )
        {
            // Validate parameters.
            MediaType = mediaType ?? throw new ArgumentNullException(nameof(mediaType));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        #endregion

        #region Instance, read-only state

        public MediaTypeHeaderValue MediaType { get; }

        public Stream Content { get; }

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            // Call the overload, remove
            // from finalization.
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            // Dispose of unmanaged resources.

            // If not disposing, get out.
            if (!disposing) return;

            // Dispose of disposables.
            using (Content) { }
        }

        ~TournamentBracketContent() => Dispose(false);

        #endregion
    }
}
