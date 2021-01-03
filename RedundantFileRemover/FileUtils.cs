using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RedundantFileRemover {
    static class FileUtils {

        private static readonly List<FileSystemInfo> accessibleFiles = new List<FileSystemInfo>();

        public static bool IsFileHasDefaultAttribute(FileSystemInfo fileSystemInfo) {
            return fileSystemInfo.Attributes == FileAttributes.Directory || fileSystemInfo.Attributes == FileAttributes.Archive;
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

        // TODO make this async
        private static void CollectAccessibleFiles(DirectoryInfo directoryInfo, string sp, bool isDir) {
            if (isDir) {
                DirectoryInfo[] dirs = directoryInfo.GetDirectories();
                for (int i = 0; i < dirs.Length; i++) {
                    if (dirs[i].Exists && IsNotSpecialFolder(dirs[i].Name)) {
                        try {
                            var dirInfo = dirs.ElementAt(i);
                            accessibleFiles.Add(dirInfo);
                            CollectAccessibleFiles(dirInfo, sp, isDir);
                        } catch (Exception) {
                        }
                    }
                }
            } else {
                var directories = directoryInfo.GetDirectories();
                for (int i = 0; i < directories.Length; i++) {
                    try {
                        if (!directories.ElementAt(i).Exists) {
                            continue;
                        }

                        if (IsNotSpecialFolder(directories[i].Name)) {
                            foreach (FileInfo fileInfo in directories[i].GetFiles('*' + sp)) {
                                try {
                                    if (fileInfo.Exists) {
                                        accessibleFiles.Add(fileInfo);
                                    }
                                } catch (Exception) {
                                }
                            }
                        }

                        CollectAccessibleFiles(directories[i], sp, isDir);
                    } catch (Exception) {
                    }
                }
            }
        }

        public static bool IsNotSpecialFolder(string dirName) {
            dirName = dirName.Replace(" ", "");

            return !dirName.Contains(Environment.SpecialFolder.System.ToString())
                && !dirName.Contains(Environment.SpecialFolder.ProgramFiles.ToString())
                && !dirName.Contains(Environment.SpecialFolder.ProgramFilesX86.ToString())
                && !dirName.Contains(Environment.SpecialFolder.Windows.ToString());
        }
    }
}
