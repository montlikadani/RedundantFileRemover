using RedundantFileRemover.UserSettingsData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedundantFileRemover {
    static class FileUtils {

        private static readonly List<FileSystemInfo> accessibleFiles = new List<FileSystemInfo>();

        public static bool IsFileNotHidden(FileSystemInfo fileSystemInfo) {
            return fileSystemInfo.Attributes != FileAttributes.Hidden && fileSystemInfo.Attributes != FileAttributes.System;
        }

        public static List<FileSystemInfo> GetFiles(DirectoryInfo directoryInfo, string searchPattern, bool isDir) {
            accessibleFiles.Clear();

            if (!isDir && searchPattern.Contains("|")) {
                foreach (string sp in searchPattern.Split('|')) {
                    CollectAccessibleFiles(directoryInfo, sp, isDir);
                }

                return accessibleFiles;
            }

            CollectAccessibleFiles(directoryInfo, searchPattern, isDir);
            return accessibleFiles;
        }

        private static void CollectAccessibleFiles(DirectoryInfo directoryInfo, string sp, bool isDir) {
            try {
                var directories = directoryInfo.EnumerateDirectories()
                    .Where(d => !d.FullName.Replace(" ", "").Contains("RedundantFileRemover"))
                    .Where(d => IsNotSpecialFolder(d.Name))
                    .Where(d => IsFileNotHidden(d)).ToArray();

                if (isDir) {
                    for (int i = 0; i < directories.Length; i++) {
                        if (directories[i].Exists) {
                            try {
                                var dirInfo = directories.ElementAt(i);
                                accessibleFiles.Add(dirInfo);

                                if (FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories) {
                                    CollectAccessibleFiles(dirInfo, sp, isDir);
                                }
                            } catch (Exception ex) {
                                RedundantFileRemover.LogException(ex.Message + " " + ex.StackTrace);
                            }
                        }
                    }
                } else {
                    for (int i = 0; i < directories.Length; i++) {
                        try {
                            if (!directories.ElementAt(i).Exists) {
                                continue;
                            }

                            foreach (FileInfo fileInfo in directories[i].GetFiles('*' + sp)) {
                                try {
                                    if (fileInfo.Exists && IsFileNotHidden(fileInfo)) {
                                        accessibleFiles.Add(fileInfo);
                                    }
                                } catch (Exception exe) {
                                    RedundantFileRemover.LogException(exe.Message + " " + exe.StackTrace);
                                }
                            }

                            if (FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories) {
                                CollectAccessibleFiles(directories[i], sp, isDir);
                            }
                        } catch (Exception ex) {
                            RedundantFileRemover.LogException(ex.Message + " " + ex.StackTrace);
                        }
                    }
                }
            } catch (Exception ex) {
                RedundantFileRemover.LogException(ex.StackTrace + " " + ex.Message);
            }
        }

        public static bool IsNotSpecialFolder(string dirName) {
            dirName = dirName.Replace(" ", "");

            return !dirName.Contains(Environment.SpecialFolder.System.ToString())
                && !dirName.Contains(Environment.SpecialFolder.Windows.ToString())
                && !dirName.Equals("PerfLogs");
        }
    }
}
