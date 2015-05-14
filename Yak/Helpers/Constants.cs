using System;
using System.IO;
using System.Reflection;

namespace Yak.Helpers
{
    /// <summary>
    /// Constants of the project
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Directory of covers images
        /// </summary>
        public static readonly string CoverMoviesDirectory = Path.GetTempPath() + "Yak\\Covers\\";

        /// <summary>
        /// Directory of poster images
        /// </summary>
        public static readonly string PosterMovieDirectory = Path.GetTempPath() + "Yak\\Posters\\";

        /// <summary>
        /// Directory of background images 
        /// </summary>
        public static readonly string BackgroundMovieDirectory = Path.GetTempPath() + "Yak\\Backgrounds\\";

        /// <summary>
        /// Directory of directors images
        /// </summary>
        public static readonly string DirectorMovieDirectory = Path.GetTempPath() + "Yak\\Directors\\";

        /// <summary>
        /// Directory of actors images
        /// </summary>
        public static readonly string ActorMovieDirectory = Path.GetTempPath() + "Yak\\Actors\\";

        /// <summary>
        /// Directory of torrents
        /// </summary>
        public static readonly string TorrentDirectory = Path.GetTempPath() + "Yak\\Torrents\\";

        /// <summary>
        /// Directory of downloaded movies
        /// </summary>
        public static readonly string MovieDownloads = Path.GetTempPath() + "Yak\\Downloads\\";

        /// <summary>
        /// YtsApiEndpoint Endpoint to YTS
        /// </summary>
        public const string YtsApiEndpoint = "http://yts.to/api/v2";

        /// <summary>
        /// Client ID for TMDb
        /// </summary>
        public const string TmDbClientId = "52db02421219a8b6b4a8eed1df0b8bd8";

        /// <summary>
        /// Background image size for movie, retrieved from TMDb
        /// </summary>
        public const string BackgroundImageSizeTmDb = "original";

        /// <summary>
        /// Generic path to youtube video
        /// </summary>
        public const string YoutubePath = "http://www.youtube.com/watch?v=";

        /// <summary>
        /// We want at least 5 rows to be able to scroll inside the main window
        /// </summary>
        public const int NumberOfRowsPerPage = 5;

        /// <summary>
        /// In percentage, the minimum of buffering before we can actually start playing the movie
        /// </summary>
        public const double MinimumBufferingBeforeMoviePlaying = 2.0;

        /// <summary>
        /// The maximum number of movies per page to load from the API
        /// </summary>
        public const int MaxMoviesPerPage = 20;

        /// <summary>
        /// Extension of image file
        /// </summary>
        public const string ImageFileExtension = ".jpg";

        /// <summary>
        /// Extension of video file
        /// </summary>
        public const string VideoFileExtension = ".mp4";

        /// <summary>
        /// Extension of torrent file
        /// </summary>
        public const string TorrentFileExtension = ".torrent";

        /// <summary>
        /// ConnectionErrorPropertyName
        /// </summary>
        public const string ConnectionErrorPropertyName = "ConnectionError";
        
        /// <summary>
        /// Used to manage file extensions
        /// </summary>
        public enum FileType
        {
            BackgroundImage = 0,
            PosterImage = 1,
            CoverImage = 2,
            DirectorImage = 3,
            ActorImage = 4,
            TorrentFile = 5
        }

        /// <summary>
        /// Get the current directory of the Vlc libraries
        /// </summary>
        /// <returns></returns>
        public static DirectoryInfo GetVlcLibDirectory()
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            if (currentDirectory == null)
            {
                throw new Exception();
            }

            return new DirectoryInfo(Path.Combine(currentDirectory, @"lib\"));
        }

        public enum YoutubeStreamingQuality
        {
            Low = 0,
            Medium = 1,
            High = 2
        }

        /// <summary>
        /// Used to manage media types (movie, trailer)
        /// </summary>
        public enum MediaType
        {
            Movie = 0,
            Trailer = 1
        }
    }
}
