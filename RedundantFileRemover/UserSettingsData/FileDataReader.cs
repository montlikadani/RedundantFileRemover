﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace RedundantFileRemover.UserSettingsData {
    sealed class FileDataReader {

        private readonly FileInfo fi;
        public FileInfo DataFile => fi;

        private static ProgramSettings ps = new();
        public static ProgramSettings ProgramSettings => ps;

        public FileDataReader() {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Redundant File Remover";
            if (!Directory.Exists(appPath)) {
                Directory.CreateDirectory(appPath);
            }

            DeleteTempFolderContent();

            fi = new FileInfo(appPath + @"\userConfigData.yml");
            if (!fi.Exists) {
                fi.Create().Close();
            }

            if (fi.Exists && fi.Length > 0) {
                try {
                    using (StreamReader reader = fi.OpenText()) {
                        ps = new DeserializerBuilder().IgnoreUnmatchedProperties().Build().Deserialize<ProgramSettings>(reader);
                    }
                } catch (Exception) {
                    // Apply default values
                }
            }
        }

        // Remove temporary file
        private void DeleteTempFolderContent() {
            var any = new DirectoryInfo(Path.GetTempPath()).EnumerateDirectories().Where(d => d.Name == "RedundantFileRemover");
            if (any.Any()) {
                any.First().Delete(true);
            }
        }

        public void Save(YamlStream stream) {
            using (TextWriter writer = fi.CreateText()) {
                stream.Save(writer, false);
            }
        }

        public void SaveAllSettings() {
            if (!fi.Exists) {
                fi.Create().Close();
            }

            var mainNode = new YamlMappingNode {
                { "FolderPath", ps.MainWindow.FolderPath },
                { "SearchEmptyFolders", ps.MainWindow.SearchEmptyFolders.ToString().ToLower() },
                { "SearchEmptyFiles", ps.MainWindow.SearchEmptyFiles.ToString().ToLower() },
                { "PatternFileTypes", ps.MainWindow.PatternFileTypes },
                { "AutoScroll", ps.MainWindow.AutoScroll.ToString().ToLower() }
            };

            var idList = new YamlSequenceNode {
                Style = SequenceStyle.Block
            };
            ps.SettingsWindow.IgnoredDirectories.Where(dir => dir != "")
                .Where(d => !d.Contains(ps.MainWindow.FolderPath)).Distinct().ToList().ForEach(id => idList.Add(id));

            var settingsNode = new YamlMappingNode {
                { "SearchInSubDirectories", ps.SettingsWindow.SearchInSubDirectories.ToString().ToLower() },
                { "IgnoredDirectories", idList },
                { "ErrorLogging", ps.SettingsWindow.ErrorLogging.ToString().ToLower() },
                { "AlwaysClearLogs", ps.SettingsWindow.AlwaysClearLogs.ToString().ToLower() },
                { "MoveFileToRecycleBin", ps.SettingsWindow.MoveFileToRecycleBin.ToString().ToLower() }
            };

            var mappingNode = new YamlMappingNode {
                { "MainWindow", mainNode },
                { "SettingsWindow", settingsNode }
            };

            Save(new YamlStream(new YamlDocument(mappingNode)));
        }
    }

    public class ProgramSettings {

        [YamlMember(Alias = "MainWindow", ApplyNamingConventions = false, ScalarStyle = YamlDotNet.Core.ScalarStyle.Any)]
        public MainWindow MainWindow { get; set; } = new MainWindow();

        [YamlMember(Alias = "SettingsWindow", ApplyNamingConventions = false, ScalarStyle = YamlDotNet.Core.ScalarStyle.Any)]
        public SettingsWindow SettingsWindow { get; set; } = new SettingsWindow();

    }

    public class MainWindow {

        public string FolderPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public bool SearchEmptyFolders { get; set; } = true;

        public bool SearchEmptyFiles { get; set; } = false;

        public string PatternFileTypes { get; set; } = ".ini, .txt";

        public bool AutoScroll { get; set; } = false;

    }

    public class SettingsWindow {

        public bool SearchInSubDirectories { get; set; } = true;

        public List<string> IgnoredDirectories { get; set; } = new List<string>();

        public bool ErrorLogging { get; set; } = false;

        public bool AlwaysClearLogs { get; set; } = false;

        public bool MoveFileToRecycleBin { get; set; } = false;

    }
}
