using System;
using System.IO;

namespace WPFVideoPlayer
{
    /// <summary>
    /// Represents a video item in the playlist or history
    /// </summary>
    public class VideoItem
    {
        /// <summary>
        /// Full path to the video file
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Display name for the video (filename without extension)
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Creates a new VideoItem from a file path
        /// </summary>
        public VideoItem(string filePath)
        {
            FilePath = filePath;
            DisplayName = Path.GetFileNameWithoutExtension(filePath);
        }

        /// <summary>
        /// Returns the display name for this video item
        /// </summary>
        public override string ToString()
        {
            return DisplayName;
        }
    }
}