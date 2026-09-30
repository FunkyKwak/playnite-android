using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Playnite.SDK;

namespace AndroidLibrary
{
    public class InputFile
    {
        public List<InputFileLine> Lines { get; set; }

        public InputFile()
        {
            Lines = new List<InputFileLine>();
        }
        public InputFile(string filePath, ILogger logger) : this()
        {
            foreach (string line in File.ReadLines(filePath))
            {
                InputFileLine newLine = new InputFileLine(line, logger);
                if (!string.IsNullOrWhiteSpace(newLine.GamePackageName) && !string.IsNullOrWhiteSpace(newLine.GameName))
                    Lines.Add(newLine);
            }
        }

    }

    public class InputFileLine
    {
        public string GamePackageName { get; set; }
        public string GameName { get; set; }
        public DateTime? LastTimePlayed { get; set; }

        public InputFileLine(string line, ILogger logger)
        {
            logger.Debug($"-------------line '{line}'");

            string[] values = line.Split(',');

            GamePackageName = values[0];
            GameName = values[1];


            if (double.TryParse(values[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double timestamp))
            {
                if (timestamp != 0)
                {
                    LastTimePlayed = DateTimeOffset
                        .FromUnixTimeMilliseconds((long)(timestamp * 1000))
                        .LocalDateTime;
                }
            }
            else if (DateTime.TryParse(values[2], out DateTime dt))
                LastTimePlayed = dt;

            logger.Debug($"Date '{values[2]}' parsée en '{LastTimePlayed}'");

            logger.Info($"Jeu Android trouvé : {GameName} ({GamePackageName})");
        }
    }
}