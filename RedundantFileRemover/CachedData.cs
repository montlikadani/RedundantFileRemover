using System;
using System.Collections.Generic;

namespace RedundantFileRemover {
    public static class CachedData {

        private static readonly Properties.Settings d = Properties.Settings.Default;

        // Main
        public static string FolderPath { get; set; } = d.FolderPath == "" ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) : d.FolderPath;

        public static bool PrintOnlyFoundFiles { get; set; } = d.PrintOnlyFoundFiles;

        public static bool SearchEmptyFolders { get; set; } = d.SearchEmptyFolders;

        public static bool SearchEmptyFiles { get; set; } = d.SearchEmptyFiles;

        public static string PatternFileTypes { get; set; } = d.PatternFileTypes;

        // Settings menu
        public static bool SearchInSubDirectories { get; set; } = d.SearchInSubDirs;

        public static List<string> IgnoredDirectories { get; } = d.IgnoredDirectories ?? new List<string>();

        public static bool ErrorLogging { get; set; } = d.ErrorLogging;

        public static bool AlwaysClearLogs { get; set; } = d.AlwaysClearLogs;

    }
}
