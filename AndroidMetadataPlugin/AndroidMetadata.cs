using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;

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

        public override List<MetadataField> SupportedFields => new List<MetadataField>();

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

        public AndroidMetadataProvider(ILogger logger, Game game)
        {
            this.logger = logger;
            this.game = game;
        }

        public override List<MetadataField> AvailableFields =>
            new List<MetadataField>
            {
                MetadataField.Name
            };


        public override string GetName(GetMetadataFieldArgs args)
        {
            logger.Info($"GetName appelé pour GameId = {game.GameId}");

            return game.GameId;
        }
    }
}