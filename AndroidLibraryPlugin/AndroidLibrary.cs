using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;
using AndroidCommon;
using System.Linq;

namespace AndroidLibrary
{
    public class AndroidLibrary : LibraryPlugin
    {
        private readonly ILogger logger;
        private readonly IPlayniteAPI playniteApi;

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
            this.playniteApi = api;
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

            InputFile inputFile = new InputFile(filePath, logger);

            logger.Info($"Start reading file '{filePath}'");
            logger.Debug($"{inputFile.Lines.Count} lines");
            foreach (InputFileLine line in inputFile.Lines)
            {
                logger.Info($"Search matching game");
                Game matchingGame = playniteApi.Database.Games.SingleOrDefault(i => i.Source?.Name == "Android" && i.GameId == line.GamePackageName);

                if (matchingGame != null)
                {
                    logger.Info($"Jeu Android existant : {matchingGame.Name}");
                    if (!matchingGame.IsInstalled)
                    {
                        matchingGame.IsInstalled = true;
                    }
                    matchingGame.LastActivity = line.LastTimePlayed;
                }
                else
                {
                    logger.Info($"Nouveau jeu Android installé : {line.GameName}");
                    yield return new GameMetadata
                    {
                        GameId = line.GamePackageName,
                        Platforms = new HashSet<MetadataProperty>
                        {
                            new MetadataNameProperty("Android")
                        },
                        Source = new MetadataNameProperty("Android"),
                        IsInstalled = true
                    };
                }
            }

            logger.Info($"Mark games as uninstalled");
            foreach (Game game in playniteApi.Database.Games.Where(i => i.Source.Name == "Android"))
            {
                if (!inputFile.Lines.Exists(i => i.GamePackageName == game.GameId))
                {
                    logger.Info($"Jeu Android désinstallé : {game.Name}");
                    game.IsInstalled = false;
                }
            }
        }

        public override LibraryMetadataProvider GetMetadataDownloader()
        {
            return new AndroidLibraryMetadataProvider(logger);
        }
    }
}