using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace RedundantFileRemover.UserSettingsData {
    public sealed class FileDataReader {

        private readonly FileInfo fi;
        public FileInfo DataFile => fi;

        private static ProgramSettings ps = new();
        public static ProgramSettings ProgramSettings => ps;

        public FileDataReader() {
            fi = new FileInfo(Directory.GetCurrentDirectory() + @"\userConfigData.yml");

            if (!fi.Exists) {
                fi.Create().Close();
            } else if (fi.Length != 0) {
                using (StreamReader reader = fi.OpenText()) {
                    ps = new DeserializerBuilder().Build().Deserialize<ProgramSettings>(reader);
                }
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
                { "SearchEmptyFolders", ps.MainWindow.SearchEmptyFolders == true ? "true" : "false" },
                { "SearchEmptyFiles", ps.MainWindow.SearchEmptyFiles == true ? "true" : "false" },
                { "PatternFileTypes", ps.MainWindow.PatternFileTypes },
                { "AutoScroll", ps.MainWindow.AutoScroll == true ? "true" : "false" },
                { "LookThroughAllFileType", ps.MainWindow.LookThroughAllFileType == true ? "true" : "false" }
            };

            var idList = new YamlSequenceNode {
                Style = SequenceStyle.Block
            };
            foreach (string id in ps.SettingsWindow.IgnoredDirectories.Distinct()) {
                idList.Add(id);
            } 

            var fileAttrs = new YamlSequenceNode {
                Style = SequenceStyle.Block
            };
            foreach (FileAttributes fa in ps.SettingsWindow.IgnoredFileAttributes) {
                fileAttrs.Add(Enum.GetName(typeof(FileAttributes), fa));
            }

            var directoryAttrs = new YamlSequenceNode {
                Style = SequenceStyle.Block
            };
            foreach (FileAttributes fa in ps.SettingsWindow.IgnoredDirectoryAttributes) {
                directoryAttrs.Add(Enum.GetName(typeof(FileAttributes), fa));
            }

            var settingsNode = new YamlMappingNode {
                { "SearchInSubDirectories", ps.SettingsWindow.SearchInSubDirectories == true ? "true" : "false" },
                { "IgnoredDirectories", idList },
                { "ErrorLogging", ps.SettingsWindow.ErrorLogging == true ? "true" : "false" },
                { "AlwaysClearLogs", ps.SettingsWindow.AlwaysClearLogs == true ? "true" : "false" },
                { "MoveFileToRecycleBin", ps.SettingsWindow.MoveFileToRecycleBin == true ? "true" : "false" },
                { "IgnoredFileAttributes", fileAttrs },
                { "IgnoredDirectoryAttributes", directoryAttrs }
            };

            Save(new YamlStream(new YamlDocument(new YamlMappingNode {
                { "MainWindow", mainNode },
                { "SettingsWindow", settingsNode }
            })));
        }
    }

    public class ProgramSettings {

        [YamlMember(Alias = "MainWindow", ApplyNamingConventions = false, ScalarStyle = YamlDotNet.Core.ScalarStyle.Any)]
        public MainWindow MainWindow { get; protected set; } = new MainWindow();

        [YamlMember(Alias = "SettingsWindow", ApplyNamingConventions = false, ScalarStyle = YamlDotNet.Core.ScalarStyle.Any)]
        public SettingsWindow SettingsWindow { get; protected set; } = new SettingsWindow();

    }

    public class MainWindow {

        public string FolderPath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public bool SearchEmptyFolders { get; set; } = true;

        public bool SearchEmptyFiles { get; set; } = false;

        public string PatternFileTypes { get; set; } = ".ini, .txt";

        public bool AutoScroll { get; set; } = false;

        public bool LookThroughAllFileType { get; set; } = false;

    }

    public class SettingsWindow {

        public bool SearchInSubDirectories { get; set; } = true;

        public List<string> IgnoredDirectories { get; protected set; } = new List<string>();

        public bool ErrorLogging { get; set; } = false;

        public bool AlwaysClearLogs { get; set; } = false;

        public bool MoveFileToRecycleBin { get; set; } = false;

        public FileAttributes[] IgnoredFileAttributes { get; set; } = { FileAttributes.Hidden, FileAttributes.System };

        public FileAttributes[] IgnoredDirectoryAttributes { get; set; } = { FileAttributes.System };

    }
}
