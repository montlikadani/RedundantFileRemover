# RedundantFileRemover
Does your system is filled with empty directory or files which is unneeded? With this you can remove it.

_This is the first C# project I created in December 2020 in my boredom._

![Main window](img/MainWindow.png?raw=true "Main window")

# Features
Currently it is only available on Windows.

- Ability to search in the root directory including sub-directories (optional)
  - During the search operation the program ignores the system files (like windows, system or hidden files).
- Option to remove all found empty files and folders
- Path exceptions when the program does not search for empty files in the specified directories
- Can search for all known file types, and the user can specify which of the file should search
- Ability to display errors that occur during a search operation.

More coming in future releases...

# Contributing
Any changes are welcome in [Pull Request](https://docs.github.com/en/free-pro-team@latest/github/collaborating-with-issues-and-pull-requests/about-pull-requests)

**Requirements**
- .NET framework 4.8
- Windows forms
