using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;


namespace AndroidLibrary
{
    public class AndroidLibrary : LibraryPlugin
    {
        private readonly ILogger logger;

        public override Guid Id => Guid.Parse("7e6d2f35-8b8e-4f4d-a9e4-2c8f6b1d73a1");

        public override string Name => "Android";

        public override string LibraryIcon => "";



        public AndroidLibrarySettings Settings;
        public override ISettings GetSettings(bool firstRunSettings)
        {
            return new AndroidLibrarySettings(this);
        }

        public override UserControl GetSettingsView(bool firstRunSettings)
        {
            return new AndroidLibrarySettingsView();
        }

        

        public AndroidLibrary(IPlayniteAPI api) : base(api)
        {
            Properties = new LibraryPluginProperties
            {
                HasSettings = true
            };
            Settings = (AndroidLibrarySettings)GetSettings(false);

            logger = LogManager.GetLogger();
        }



        public override IEnumerable<GameMetadata> GetGames(LibraryGetGamesArgs args)
        {
            var filePath = Settings.InstalledAppsFilePath;

            logger.Info($"Lecture de la bibliothèque Android : {filePath}");

            if (!File.Exists(filePath))
            {
                logger.Warn($"Fichier introuvable : {filePath}");
                yield break;
            }

            foreach (var line in File.ReadLines(filePath))
            {
                string[] values = line.Split(',');
                string gamePackageName = values[0];
                string gameName = values[1];

                if (string.IsNullOrWhiteSpace(gamePackageName))
                    continue;
                if (string.IsNullOrWhiteSpace(gameName))
                    continue;

                logger.Info($"Jeu Android trouvé : {gameName} ({gamePackageName})");

                yield return new GameMetadata
                {
                    GameId = gamePackageName,
                    //Name = gameName,

                    Platforms = new HashSet<MetadataProperty>
                    {
                        new MetadataNameProperty("Android")
                    },
                    Source = new MetadataNameProperty("Android")
                };
            }
        }
    }
}