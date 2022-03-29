# RedundantFileRemover [![Github All Releases](https://img.shields.io/github/downloads/montlikadani/RedundantFileRemover/total.svg)](https://github.com/montlikadani/RedundantFileRemover/releases)
Does your system is filled with empty directory or files which is unneeded? With this you can remove it.

_This is the first C# project I created in December 2020 in my boredom._

![Main window](img/MainWindow.png?raw=true "Main window")

# Features
Currently it is only available on Windows (10-8). Not sure if it works on lowest windows versions.

- Ability to search in the directory where the user wants (this includes sub-directories if the option is enabled)
- The search operation can ignore the files/directories with specified attributes (like windows, system or hidden files).
- Option to remove all found empty files and folders
  - (Optional) You can select whenever to remove files from disk or move files into recycle bin.
- Path exceptions when the program does not search for empty files in the specified directories
- Can search for all known file types or the user can specify which of the file should search
- Ability to display errors that occur during a search operation.
- Pop-up context menu on right click when the user want to open or remove the found file manually
- Different settings window for options

More coming in future releases...

# Installation
- Download the latest release from [Releases](https://github.com/montlikadani/RedundantFileRemover/releases) tab
- Unzip the downloaded file
- Run `RedundantFileRemover.exe`

# Contributing
Any changes are welcome in [Pull Request](https://docs.github.com/en/free-pro-team@latest/github/collaborating-with-issues-and-pull-requests/about-pull-requests)

**Requirements**
- .NET framework 4.8
- WPF
- IDE, I use Visual Studio version 17
- C# 10.0 (latest)
