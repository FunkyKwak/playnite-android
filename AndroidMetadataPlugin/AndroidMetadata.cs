using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidMetadata
{
    public class AndroidMetadata : MetadataPlugin
    {
        private readonly ILogger logger = LogManager.GetLogger();

        public AndroidMetadata(IPlayniteAPI api) : base(api)
        {
        }

        public override Guid Id => Guid.Parse("3139a44e-8c88-422b-86b3-73c7f36cf0f2");

        public override string Name => "Android";

        public override List<MetadataField> SupportedFields =>
            new List<MetadataField>
            {
                MetadataField.Name,
                MetadataField.Description,
                MetadataField.Icon,
                MetadataField.CoverImage,
                MetadataField.BackgroundImage,
                MetadataField.Developers,
                MetadataField.Genres,
                MetadataField.Tags,
                MetadataField.Links
            };

        public override OnDemandMetadataProvider GetMetadataProvider(MetadataRequestOptions options)
        {
            logger.Info($"GetMetadataProvider appelé pour GameId = {options.GameData.GameId}");
            return new AndroidMetadataProvider(logger, options.GameData);
        }
    }

    public class AndroidMetadataProvider : OnDemandMetadataProvider
    {
        private readonly ILogger logger;
        private readonly Game game;
        private readonly GooglePlayClient googlePlayClient;

        private GooglePlayGame? googlePlayGame;
        private bool loaded;


        public AndroidMetadataProvider(ILogger logger, Game game)
        {
            this.logger = logger;
            this.game = game;

            logger.Info(
                $"Récupération Google Play pour GameId = {game.GameId}");

            var client = new GooglePlayClient();

            googlePlayGame = Task.Run(async () =>
                await client.GetGameAsync(
                    game.GameId,
                    System.Threading.CancellationToken.None))
                .GetAwaiter()
                .GetResult();

            if (googlePlayGame == null)
            {
                logger.Error(
                    $"Impossible de récupérer les métadonnées Google Play pour {game.GameId}");
            }
            else
            {
                logger.Info(
                    $"Google Play récupéré avec succès pour {game.GameId}");

                logger.Info(
                    $"Google Play - Name = {googlePlayGame.Name}");

                logger.Info(
                    $"Google Play - Description = {googlePlayGame.Description}");

                logger.Info(
                    $"Google Play - IconUrl = {googlePlayGame.IconUrl}");

                logger.Info(
                    $"Google Play - Developer = {googlePlayGame.Developer}");

                logger.Info(
                    $"Google Play - DeveloperUrl = {googlePlayGame.DeveloperUrl}");

                logger.Info(
                    $"Google Play - Category = {googlePlayGame.Category}");

                logger.Info(
                    $"Google Play - AgeRating = {googlePlayGame.AgeRating}");

                logger.Info(
                    $"Google Play - Rating = {googlePlayGame.Rating}");

                logger.Info(
                    $"Google Play - RatingCount = {googlePlayGame.RatingCount}");

                logger.Info(
                    $"Google Play - Price = {googlePlayGame.Price}");

                logger.Info(
                    $"Google Play - Currency = {googlePlayGame.Currency}");
            }
        }

        public override List<MetadataField> AvailableFields =>
            new List<MetadataField>
            {
                MetadataField.Name,
                MetadataField.Description,
                MetadataField.CoverImage,
                MetadataField.Developers,
                MetadataField.Genres,
                MetadataField.AgeRating,
                MetadataField.CommunityScore
            };


        public override string GetName(GetMetadataFieldArgs args)
        {
            if (googlePlayGame == null)
                return null;

            return googlePlayGame.Name;
        }

        public override string GetDescription(GetMetadataFieldArgs args)
        {
            if (googlePlayGame == null)
                return null;

            return googlePlayGame.Description;
        }

        public override MetadataFile GetCoverImage(GetMetadataFieldArgs args)
        {
            if (googlePlayGame == null ||
                string.IsNullOrWhiteSpace(googlePlayGame.IconUrl))
            {
                return null;
            }

            return new MetadataFile(
                googlePlayGame.IconUrl);
        }

        public override IEnumerable<MetadataProperty> GetDevelopers(
            GetMetadataFieldArgs args)
        {
            if (googlePlayGame == null ||
                string.IsNullOrWhiteSpace(googlePlayGame.Developer))
            {
                return new List<MetadataProperty>();
            }

            return new List<MetadataProperty>
            {
                new MetadataNameProperty(
                    googlePlayGame.Developer)
            };
        }

        public override IEnumerable<MetadataProperty> GetGenres(
            GetMetadataFieldArgs args)
        {
            if (googlePlayGame == null ||
                string.IsNullOrWhiteSpace(googlePlayGame.Category))
            {
                return new List<MetadataProperty>();
            }

            return new List<MetadataProperty>
            {
                new MetadataNameProperty(
                    googlePlayGame.Category)
            };
        }

        public override int? GetCommunityScore(
            GetMetadataFieldArgs args)
        {
            if (googlePlayGame == null ||
                !googlePlayGame.Rating.HasValue)
            {
                return null;
            }

            return (int)(googlePlayGame.Rating.Value * 20.0);
        }
    }
}
