using RedundantFileRemover.UserSettingsData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RedundantFileRemover {
    static class FileUtils {

        private static readonly List<FileSystemInfo> accessibleFiles = new List<FileSystemInfo>();

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

        public static async Task<List<FileSystemInfo>> GetFiles(DirectoryInfo directoryInfo, string searchPattern, bool isDir) {
            accessibleFiles.Clear();

            await Task.Run(() => {
                if (!isDir && searchPattern.Contains("|")) {
                    foreach (string sp in searchPattern.Split('|')) {
                        CollectAccessibleFiles(directoryInfo, sp, isDir);
                    }
                } else {
                    CollectAccessibleFiles(directoryInfo, searchPattern, isDir);
                }
            });

            return accessibleFiles;
        }

        private static void CollectAccessibleFiles(DirectoryInfo directoryInfo, string sp, bool isDir) {
            try {
                var directories = directoryInfo.EnumerateDirectories()
                    .Where(d => !d.FullName.Replace(" ", "").Contains("RedundantFileRemover"))
                    .Where(d => !IsFileHasAttribute(d, FileAttributes.System)).ToArray();

                RedundantFileRemover.ChangeProgressBar(accessibleFiles.Count);

                if (isDir) {
                    for (int i = 0; i < directories.Length; i++) {
                        try {
                            var dirInfo = directories.ElementAt(i);

                            if (Directory.Exists(dirInfo.FullName) && !Directory.EnumerateFileSystemEntries(dirInfo.FullName).Any()
                                && !IsFileHasAttribute(dirInfo, FileAttributes.Hidden, FileAttributes.System)
                                        && !IsFileLocked(dirInfo.FullName)) {
                                accessibleFiles.Add(dirInfo);
                            }

                            if (FileDataReader.ProgramSettings.SettingsWindow.SearchInSubDirectories) {
                                CollectAccessibleFiles(dirInfo, sp, isDir);
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

                            foreach (FileInfo fileInfo in directories[i].GetFiles('*' + sp).Where(file => file.Name.ToLower().EndsWith(sp))) {
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
    }
}
