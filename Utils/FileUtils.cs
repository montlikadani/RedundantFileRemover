using System;
using System.IO;

namespace RedundantFileRemover.Utils {
    static class FileUtils {

        public static bool IsFileHasAttribute(FileSystemInfo fileSystemInfo, params FileAttributes[] array) {
            foreach (FileAttributes attr in array) {
                try {
                    if (fileSystemInfo.Attributes.HasFlag(attr)) {
                        return true;
                    }
                } catch (Exception ex) {
                    MainWindow.LogException(ex.StackTrace + " " + ex.Message);
                }
            }

            return false;
        }

        public static bool IsFileLocked(string path) {
            try {
                using (File.Open(path, FileMode.Open)) {
                    return false;
                }
            } catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { // Locked file
                return true;
            }
        }
    }
}
