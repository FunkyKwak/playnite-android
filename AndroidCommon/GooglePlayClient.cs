using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AndroidCommon
{
    internal class GooglePlayJsonLd
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string OperatingSystem { get; set; }
        public string ApplicationCategory { get; set; }
        public string Image { get; set; }
        public string ContentRating { get; set; }

        public GooglePlayAuthor Author { get; set; }
        public GooglePlayAggregateRating AggregateRating { get; set; }
        public GooglePlayOffer[] Offers { get; set; }
    }

    internal class GooglePlayAuthor
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    internal class GooglePlayAggregateRating
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string RatingValue { get; set; }
        public string RatingCount { get; set; }
    }

    internal class GooglePlayOffer
    {
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string Price { get; set; }
        public string PriceCurrency { get; set; }
        public string Availability { get; set; }
    }

    public class GooglePlayClient
    {
        private readonly HttpClient httpClient;

        public GooglePlayClient()
        {
            httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " +
                "AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/140.0.0.0 Safari/537.36");
        }

        public async Task<AndroidGameMetadata> GetGameAsync(
            string packageName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(packageName))
                return null;

            string url =
                "https://play.google.com/store/apps/details?id=" +
                Uri.EscapeDataString(packageName) +
                "&hl=fr&gl=FR";

            try
            {
                using (var response = await httpClient.GetAsync(
                    url,
                    cancellationToken))
                {
                    if (!response.IsSuccessStatusCode)
                        return null;

                    string html = await response.Content.ReadAsStringAsync();

                    return ParseGame(html, packageName);
                }
            }
            catch
            {
                return null;
            }
        }

        private AndroidGameMetadata ParseGame(
            string html,
            string packageName)
        {
            if (string.IsNullOrWhiteSpace(html))
                return null;

            // Google Play ajoute parfois des attributs à la balise,
            // par exemple :
            //
            // <script type="application/ld+json" nonce="...">
            //
            // On recherche donc simplement le début de la balise.
            const string scriptMarker = "<script type=\"application/ld+json\"";
            const string scriptEnd = "</script>";

            int searchPosition = 0;

            while (searchPosition < html.Length)
            {
                int scriptStart = html.IndexOf(
                    scriptMarker,
                    searchPosition,
                    StringComparison.OrdinalIgnoreCase);

                if (scriptStart < 0)
                    break;

                // On cherche le '>' qui termine la balise <script ...>
                int contentStart = html.IndexOf('>',scriptStart);
                if (contentStart < 0)
                    break;
                contentStart++;

                int contentEnd = html.IndexOf(scriptEnd, contentStart, StringComparison.OrdinalIgnoreCase);
                if (contentEnd < 0)
                    break;
                string json = html.Substring( contentStart,contentEnd - contentStart).Trim();

                searchPosition = contentEnd + scriptEnd.Length;

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                GooglePlayJsonLd data = null;

                try
                {
                    data =JsonConvert.DeserializeObject<GooglePlayJsonLd>(json);
                }
                catch
                {
                    // Ce JSON-LD n'est pas celui que l'on cherche.
                    continue;
                }

                if (data == null)
                    continue;

                if (!string.Equals(
                    data.Type,
                    "SoftwareApplication",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var game = new AndroidGameMetadata
                {
                    PackageName = packageName,
                    Name = data.Name,
                    Description = data.Description,
                    IconUrl = data.Image,

                    Developer = data.Author != null
                        ? data.Author.Name
                        : null,

                    DeveloperUrl = data.Author != null
                        ? data.Author.Url
                        : null,

                    Category = data.ApplicationCategory,
                    AgeRating = data.ContentRating
                };

                if (data.AggregateRating != null)
                {
                    double rating;

                    if (double.TryParse(
                        data.AggregateRating.RatingValue,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out rating))
                    {
                        game.Rating = rating;
                    }

                    long ratingCount;

                    if (long.TryParse(
                        data.AggregateRating.RatingCount,
                        NumberStyles.Any,
                        CultureInfo.InvariantCulture,
                        out ratingCount))
                    {
                        game.RatingCount = ratingCount;
                    }
                }

                if (data.Offers != null &&
                    data.Offers.Length > 0 &&
                    data.Offers[0] != null)
                {
                    game.Price = data.Offers[0].Price;
                    game.Currency = data.Offers[0].PriceCurrency;
                }

                return game;
            }

            return null;
        }
    }
}