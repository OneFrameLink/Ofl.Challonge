using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ofl.Challonge
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChallonge(
            this IServiceCollection serviceCollection,
            IConfiguration challongeResponseStorageConfiguration
        )
        {
            // Validate parameters.
            if (serviceCollection == null) throw new ArgumentNullException(nameof(serviceCollection));
            if (challongeResponseStorageConfiguration == null) 
                throw new ArgumentNullException(nameof(challongeResponseStorageConfiguration));

            // For ease-of-use.
            var sc = serviceCollection;

            // Register.
            sc = sc.AddTransient<IChallongeTournamentBracketPageHtmlParser, ChallongeTournamentBracketPageHtmlParser>();

            // Add the configuration.
            sc = sc.Configure<ChallongeClientConfiguration>(
                challongeResponseStorageConfiguration.Bind);

            // Add the configuration for the http client.
            sc.AddHttpClient<IChallongeClient, ChallongeClient>()
                .ConfigureHttpClient(ChallongeClient.ConfigureHttpClient);

            // Return the service collection.
            return sc;
        }
    }
}
