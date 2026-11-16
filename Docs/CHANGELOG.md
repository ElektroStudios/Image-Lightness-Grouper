# Image Lightness Grouper Change Log 📋

## v1.1 🔄
- Added command-line support, through the new console application `Image Lightness Grouper.CLI.exe`:
  - `"Image Lightness Grouper.CLI.exe" <directory> [options]` groups the images, with the options `/recursive`, `/step:<n>`, `/alpha:<n>` and `/parallel:<n>`.
  - Every image and the final summary are written to the console, and Ctrl+C cancels the operation.
  - Documented exit codes, so it can be used in scripts.
  - `/?` (or no arguments) shows the help, which also explains how the program works.
- The main window accepts a directory as its command-line argument, and selects it on startup. An invalid argument is reported in its console.
- Added the Help > Command-Line Reference menu item.
- The shared logic now lives in the new `Image Lightness Grouper.Core.dll` library, used by both applications.
- The final summary of the main window and of the command line share the same format.

## v1.0 🔄
Initial Release.
