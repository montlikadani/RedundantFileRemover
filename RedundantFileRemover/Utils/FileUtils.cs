using RedundantFileRemover.UserSettingsData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RedundantFileRemover {
    static class FileUtils {

        private static readonly List<FileSystemInfo> accessibleFiles = new();

        public static bool IsFileHasAttribute(FileSystemInfo fileSystemInfo, params FileAttributes[] array) {
            foreach (FileAttributes attr in array) {
                if (fileSystemInfo.Attributes.HasFlag(attr)) {
                    return true;
                }
            }

            return false;
        }

        public static bool IsFileLocked(string path) {
            try {
                using (File.Open(path, FileMode.Open)) {
                    return false;
                }
            } catch (IOException) { // Locked file
                return true;
            }
        }

        public static async Task<List<FileSystemInfo>> GetFiles(DirectoryInfo directoryInfo, string searchPattern, bool isDirectory) {
            accessibleFiles.Clear();

            if (!isDirectory) {
                searchPattern = searchPattern.Replace(',', '|').Replace(" ", "");
            }

            await Task.Run(() => {
                if (!isDirectory && searchPattern.Contains("|")) {
                    foreach (string sp in searchPattern.Split('|')) {
                        CollectAccessibleFiles(directoryInfo, sp, isDirectory);
                    }
                } else {
                    CollectAccessibleFiles(directoryInfo, searchPattern, isDirectory);
                }
            });

            return accessibleFiles;
        }

        private static void CollectAccessibleFiles(DirectoryInfo directoryInfo, string searchPattern, bool isDirectory) {
            try {
                if (RedundantFileRemover.TerminateRequested) {
                    return;
                }

                var directories = directoryInfo.EnumerateDirectories()
                    .Where(d => !d.FullName.Replace(" ", "").Contains("RedundantFileRemover"))
                    .Where(d => !IsFileHasAttribute(d, FileAttributes.System)).ToArray();

                RedundantFileRemover.ChangeProgressBar(accessibleFiles.Count);

                if (isDirectory) {
                    for (int i = 0; i < directories.Length; i++) {
                        try {
                            var dirInfo = directories.ElementAt(i);

                            if (Directory.Exists(dirInfo.FullName) && !Directory.EnumerateFileSystemEntries(dirInfo.FullName).Any()
                                && !IsFileHasAttribute(dirInfo, FileAttributes.Hidden, FileAttributes.System)) {
                                accessibleFiles.Add(dirInfo);
                            }

                            if (FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories) {
                                CollectAccessibleFiles(dirInfo, searchPattern, isDirectory);
                            }
                        } catch (Exception ex) {
                            RedundantFileRemover.LogException(ex.Message + " " + ex.StackTrace);
                        }
                    }
                } else {
                    for (int i = 0; i < directories.Length; i++) {
                        try {
                            if (!Directory.Exists(directories.ElementAt(i).FullName)) {
                                continue;
                            }

                            foreach (FileInfo fileInfo in directories[i].GetFiles('*' + searchPattern).Where(file => file.Name.ToLower().EndsWith(searchPattern))) {
                                try {
                                    if (File.Exists(fileInfo.FullName) && !IsFileHasAttribute(fileInfo, FileAttributes.Hidden, FileAttributes.System)
                                        && !IsFileLocked(fileInfo.FullName) && fileInfo.Length <= 0) {
                                        accessibleFiles.Add(fileInfo);
                                    }
                                } catch (Exception exe) {
                                    RedundantFileRemover.LogException(exe.Message + " " + exe.StackTrace);
                                }
                            }

                            if (FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories) {
                                CollectAccessibleFiles(directories[i], searchPattern, isDirectory);
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
    }
}
