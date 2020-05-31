using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ofl.Common;
using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Ofl.Challonge
{
    public class ChallongeClient : CommonBase, IChallongeClient
    {
        #region Constructor

        public ChallongeClient(
            HttpClient httpClient,
            IChallongeTournamentBracketPageHtmlParser challongeTournamentBracketPageHtmlParser,
            IOptions<ChallongeClientConfiguration> challongeResponseStorageConfigurationOptions,
            ICommonServices commonServices
        ) : base(commonServices)
        {
            // Validate parameters.
            _httpClient = httpClient
                ?? throw new ArgumentNullException(nameof(httpClient));
            _parser = challongeTournamentBracketPageHtmlParser
                ?? throw new ArgumentNullException(nameof(challongeTournamentBracketPageHtmlParser));
            _challongeClientConfigurationOptions = challongeResponseStorageConfigurationOptions
                ?? throw new ArgumentNullException(nameof(challongeResponseStorageConfigurationOptions));
        }

        #endregion

        #region Static, read-only state.

        internal static readonly string HttpClientName = nameof(ChallongeClient);

        #endregion

        #region Instance, read-only state.

        private readonly IChallongeTournamentBracketPageHtmlParser _parser;

        private readonly HttpClient _httpClient;

        private readonly IOptions<ChallongeClientConfiguration> 
            _challongeClientConfigurationOptions;

        #endregion

        #region Helpers

        internal static void ConfigureHttpClient(IServiceProvider serviceProvider, HttpClient client)
        {
            // Validate parameters.
            var sp = serviceProvider ?? throw new ArgumentNullException(nameof(ArgumentNullException));
            if (client == null) throw new ArgumentNullException(nameof(client));

            // Get the configuration.
            IOptions<ChallongeClientConfiguration> options = sp
                .GetRequiredService<IOptions<ChallongeClientConfiguration>>();

            // Get the config.
            ChallongeClientConfiguration config = options.Value;

            // If there is no user agent specified, do nothing.
            if (string.IsNullOrWhiteSpace(config.UserAgent)) return;

            // Set the user agent.
            client.DefaultRequestHeaders.Add("User-Agent", config.UserAgent);
        }

        #endregion

        #region IChallongeClient implementation.

        public async Task<TournamentBracketResponse> GetTournamentBracketAsync(
            TournamentBracketRequest request,
            TournamentBracketContent content,
            CancellationToken cancellationToken)
        {
            // Validate parameters.
            // Note: The content parameter can be null.
            if (request == null) throw new ArgumentNullException(nameof(request));

            // If the url is null, throw.
            if (request.Url == null)
                throw new InvalidOperationException(
                    $"The {nameof(request.Url)} property on the {nameof(request)} parameter cannot be null.");

            // The disposables.
            using var disposables = new CompositeDisposable();

            // The page content.
            TournamentBracketPage page;

            // Get the content stream.
            (Stream rawStream, bool fromStorage) = await GetContextStreamAsync(
                request, content, disposables, cancellationToken
            ).ConfigureAwait(false);

            // Add the raw stream to the disposables.
            disposables.Add(rawStream);

            // If there is no stream, copy it.
            if (!(rawStream is MemoryStream stream))
            {
                // Create a new one.
                stream = new MemoryStream();

                // Copy.
                await rawStream.CopyToAsync(stream, 4096, cancellationToken)
                    .ConfigureAwait(false);
            }

            // Add the stream to the disposables.
            disposables.Add(stream);

            // Set the position of the stream back.
            stream.Position = 0;

            // Create a reader.
            using TextReader reader = new StreamReader(stream);

            // Parse.
            page = await _parser.ParseTournamentBracketPageAsync(reader, cancellationToken)
                .ConfigureAwait(false);

            // Everything was successful at this point, reset the stream and persist.
            stream.Position = 0;

            // Persist if this wasn't from storage to begin with.
            if (!fromStorage)
                await PersistContentAsync(request, stream, cancellationToken)
                    .ConfigureAwait(false);

            // Return.
            return new TournamentBracketResponse {
                Request = request,
                PageContent = page.PageContent,
                Bracket = page.Bracket,
                TargetingKeyValues = page.TargetingKeyValues,
                MetaList = page.MetaList
            };
        }

        #endregion

        #region Helpers

        private Task PersistContentAsync(
            TournamentBracketRequest request,
            Stream stream,
            CancellationToken cancellationToken
        )
        {
            // Validate parameters.
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            // Get the container reference.
            CloudBlobContainer container = GetStorageContainerReference();

            // Get the blob filename.
            string file = GetBlobFilename(request);

            // Create the block blob.
            CloudBlockBlob blob = container.GetBlockBlobReference(file);

            // Upload the stream to the blob.
            return blob.UploadFromStreamAsync(stream, cancellationToken);
        }

        private async Task<(Stream stream, bool fromStorage)> GetContextStreamAsync(
            TournamentBracketRequest request,
            TournamentBracketContent content,
            CompositeDisposable disposables,
            CancellationToken cancellationToken
        )
        {
            // Validate parameters.
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (disposables == null) throw new ArgumentNullException(nameof(disposables));

            // The algorithm is this:
            // If there is content, use that.
            // If there is not content, then try to get from challonge.
            // If there is no content, try to get from persistence
            // If it can't be retrieved, throw.

            // Check the request for content.
            if (content != null
                && content.MediaType.Type == "text"
                && content.MediaType.SubTypeWithoutSuffix == "html"
            )
            {
                // Return a streamreader.
                return (content.Content, false);
            }

            // Try and get it from challonge.
            Stream stream = await GetStreamFromChallongeAsync(
                request, disposables, cancellationToken
            )
            .ConfigureAwait(false);

            // If not null, return.
            if (stream != null) return (stream, false);

            // Get the reader from storage.
            stream = await GetStreamFromStorageAsync(
                request, disposables, cancellationToken
            ).ConfigureAwait(false);

            // If there is no stream at this point, throw.
            if (stream == null)
                throw new InvalidOperationException(
                    $"Could not retrieve content for URL \"{request.Url}\" from user-sumbmitted content, Challonge, or storage.");

            // Return the reader.
            return (stream, true);
        }

        private async Task<Stream> GetStreamFromChallongeAsync(
            TournamentBracketRequest request,
            CompositeDisposable disposables,
            CancellationToken cancellationToken
        )
        {
            // Validate parameters.
            if (request == null) throw new ArgumentNullException(nameof(request));

            // Try to get from challonge.
            HttpResponseMessage response = await _httpClient
                .GetAsync(request.Url, cancellationToken)
                .ConfigureAwait(false);

            // If there is not a X-Challonge-Cache-ID header, return
            // null.
            if (!response.Headers.TryGetValues("X-Challonge-Cache-ID", out var _))
                // Return null.
                return null;

            // Add to the disposables.
            disposables.Add(response);

            // Get the stream.
            Stream stream = await response.Content
                .ReadAsStreamAsync()
                .ConfigureAwait(false);

            // Return.
            return stream;
        }

        private async Task<Stream> GetStreamFromStorageAsync(
            TournamentBracketRequest request,
            CompositeDisposable disposables,
            CancellationToken cancellationToken
        )
        {
            // Validate parameters.
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (disposables == null) throw new ArgumentNullException(nameof(disposables));

            // Get the container reference.
            CloudBlobContainer container = GetStorageContainerReference();

            // Get the blob filename.
            string file = GetBlobFilename(request);

            // Create the block blob.
            CloudBlockBlob blob = container.GetBlockBlobReference(file);

            // Does it exist?  If not, return null.
            if (!await blob.ExistsAsync(cancellationToken)
                .ConfigureAwait(false))
                return null;

            // The stream to be a target.
            var ms = new MemoryStream();

            // Add to the disposables.
            disposables.Add(ms);

            // Download.
            await blob.DownloadToStreamAsync(ms, cancellationToken)
                .ConfigureAwait(false);

            // Reset the stream.
            ms.Position = 0;

            // Return the stream.
            return ms;
        }

        private static string GetBlobFilename(TournamentBracketRequest request)
        {
            // Validate parameters.
            if (request == null) throw new ArgumentNullException(nameof(request));

            // The filename will be based on the type.
            // Fir single stage tournaments, it will be.
            //
            // t/<host>?/<id>.html
            //
            // If there is no host, we will use "(root)" as the host
            // ID is the query string.
            // Look at the hostname of the URL to get the host.
            //
            // TODO: Handle event brackets.
            string host = "(root)";

            // Is it not challonge in it's entirety?
            if (string.Compare(request.Url.Host, "challonge.com", StringComparison.OrdinalIgnoreCase) != 0)
            {
                // Split the host.
                ReadOnlySpan<string> parts = request.Url.Host.Split('.');

                // If there are not three parts, throw.
                if (parts.Length != 3)
                    throw new InvalidOperationException($"The URL { request.Url } is not a valid Challonge URL.");

                // Set the host with the first part.
                host = parts[0].ToLowerInvariant();
            }

            // Get the path.  Lowercase, and set the extension.
            string path = Path.ChangeExtension(request.Url.AbsolutePath.ToLowerInvariant(), ".html");

            // If there are separators in the path, throw.
            // There is one at the beginning.
            if (path.IndexOf('/') >= 1)
                throw new InvalidOperationException($"The URL { request.Url } is not a valid Challonge URL.");

            // Put it all together.
            return $"t/{host}{path}";
        }

        private CloudBlobContainer GetStorageContainerReference()
        {
            // Get the configuration.
            ChallongeClientConfiguration config = _challongeClientConfigurationOptions
                .Value;

            // Get the storage account.
            CloudStorageAccount account = CloudStorageAccount.Parse(CommonServices.ShellServices
                .ConfigurationRoot.GetConnectionString(config.Storage.ConnectionString));

            // Get the client.
            CloudBlobClient client = account.CreateCloudBlobClient();

            // Get the container and return.
            return client.GetContainerReference(config.Storage.Container);
        }

        #endregion
    }
}
