using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace AndroidCommon
{
    public static class AndroidGameMetadataMapper
    {
        public static GameMetadata ToGameMetadata(
            AndroidGameMetadata androidGame)
        {
            if (androidGame == null)
            {
                return null;
            }

            var metadata = new GameMetadata
            {
                Name = androidGame.Name,
                Description = androidGame.Description,
                CommunityScore = ConvertRatingToPlayniteScore(
                    androidGame.Rating)
            };

            if (!string.IsNullOrWhiteSpace(androidGame.IconUrl))
            {
                metadata.CoverImage =
                    new MetadataFile(androidGame.IconUrl);
            }

            if (!string.IsNullOrWhiteSpace(androidGame.Developer))
            {
                metadata.Developers =
                    new HashSet<MetadataProperty>
                    {
                        new MetadataNameProperty(
                            androidGame.Developer)
                    };
            }

            if (!string.IsNullOrWhiteSpace(androidGame.AgeRating))
            {
                metadata.AgeRatings =
                    new HashSet<MetadataProperty>
                    {
                        new MetadataNameProperty(
                            androidGame.AgeRating)
                    };
            }

            return metadata;
        }

        private static int? ConvertRatingToPlayniteScore(
            double? rating)
        {
            if (!rating.HasValue)
            {
                return null;
            }

            // Google Play : 0 à 5
            // Playnite : 0 à 100
            return (int)Math.Round(
                rating.Value * 20,
                MidpointRounding.AwayFromZero);
        }
    }
}
