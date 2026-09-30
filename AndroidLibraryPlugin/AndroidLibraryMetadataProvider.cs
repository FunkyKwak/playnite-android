using AndroidCommon;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Threading;

namespace AndroidLibrary
{
    public class AndroidLibraryMetadataProvider : LibraryMetadataProvider
    {
        private readonly ILogger logger;
        private readonly GooglePlayClient googlePlayClient;

        public AndroidLibraryMetadataProvider(ILogger logger)
        {
            this.logger = logger;
            this.googlePlayClient = new GooglePlayClient();
        }

        public override GameMetadata GetMetadata(Game game)
        {
            if (game == null)
            {
                return null;
            }

            string packageName = game.GameId;

            if (string.IsNullOrWhiteSpace(packageName))
            {
                logger.Warn(
                    "Impossible de récupérer les métadonnées Android : GameId est vide.");

                return null;
            }

            logger.Info(
                $"Récupération des métadonnées Google Play pour {packageName}");

            try
            {
                AndroidGameMetadata androidGame =
                    googlePlayClient
                        .GetGameAsync(
                            packageName,
                            CancellationToken.None)
                        .GetAwaiter()
                        .GetResult();

                if (androidGame == null)
                {
                    logger.Warn(
                        $"Aucune métadonnée Google Play trouvée pour {packageName}.");

                    return null;
                }

                return AndroidGameMetadataMapper.ToGameMetadata(
                    androidGame);
            }
            catch (Exception ex)
            {
                logger.Error(
                    ex,
                    $"Erreur lors de la récupération des métadonnées Google Play pour {packageName}.");

                return null;
            }
        }
    }
}
