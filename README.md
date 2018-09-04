# AmigaOsBuilder

## About
Used by myself to compose my version of Amiga OS.

Built as a .NET Core Command Line tool, so it should work on Apple and Linux.

Currently most stuff is configured by code, but should be moved to external json files. I might build some ui to make the json files easier to manage in the future.

*Note: Coding- and Amiga OS- skills required!*

## Why ??
If you follow the steps in 'How to use it' below, you'll end up with a simple 'MyAmigaOs' that consists of 4 'packages':
1. Workbench 3.1 (clean install)
1. Lha 2.15
1. Backdrop-Settings
1. Env-Archive

Imagine when you have alot more packages, and you want to remove one of them. With this tool it'll be as easy as change a boolean from true to false. If the packages ain't separated it'll be a nightmare to remove them, especially if they've spread their files all over your hard drives.

_*Warning: When you run the tool all files that aren't part of your package will be deleted! This tool is supposed to build your 'System'-drive only - keep your work files somewhere else! :)*_

## How to use it
This is a quick example of how to compose a basic Workbench 3.1 with one extra Util package (Lha) and some reverse content package for Workbench Env-Archive and .backdrop file.

### Preparation
1. Make some work folder (e.g. C:\MyAmigaOs)
1. Make two sub folders: Source and Output
1. Change application settings to match your created folders.
    - Location and ConfigName is currently not in use
1. Change application settings 'SyncMode' to 'Synchronize'
    - Other modes will be removed

### Vanilla Workbench 3.1
1. Make a sub folder in Source, named 'Workbench\_3.1'
1. Make one more sub folder in this called 'content''
1. Make one more sub folder in this called '\_\_systemdrive\_\_'
1. You should have a folder with the full path 'C:\MyAmigaOs\Source\Workbench\_3.1\content\\_\_systemdrive\_\_'
1. Make a clean install of Workbench 3.1 somewhere, and copy the contents of this folder into the folder above
    -You have now created a simple base package for Workbench 3.1

### First build of MyAmigaOs
1. Open the code for editing
1. Locate `private static readonly Config TestConfig = new Config { ... }` block
1. Remove all the current Packages
1. Add your own package, like this
    -This part should probably be located in a json file outside of the code. However I found it easier to have it in code for now, as its easier to spot build errors in VS
    ```csharp
    new Package
    {
        Include = true,
        Path = "Workbench\_3.1",
        Category = "OS",
        Description = "Workbench 3.1 operation system (clean Install)",
    },
    ```
1. To ensure you're ony receiving useful logging, ensure that `.MinimumLevel.Debug()` when initializing Serilog is commented out/removed
1. Run this code (in Visual Studio or from command line)
1. This will cause all of the files from the 'C:\MyAmigaOs\Source\Workbench_3.1\content\\_\_systemdrive\_\_'  folder to be copied into the 'C:\MyAmigaOs\Output\System' folder
    -Note that the \_\_systemdrive\_\_ has been changed to System. There's a bunch of aliases that you can use that will map to specific output folders. For the full list, check out `private static readonly IDictionary<string, string> AliasToOutputMap = new Dictionary<string, string> { ... }` in the code.

### First test
1. To test this, configure an Amiga emulator to use the Output\\System folder as a hard drive
1. When you start your emulator it should load your vanilla install of Workbench 3.1

### Let's add Lha
1. Make a sub folder in Source, named 'Lha\_2.15'
1. Download 'lha_68k.lha' from http://aminet.net/package/util/arc/lha_68k
1. Place it your 'Lha\_2.15' folder and unpack the archive into an folder called 'unarchived'
    -This is not necessary, but I think it's good practice to keep the downloaded archive and all of the unarchived files within the Source package folder
1. Make one more sub folder in your 'Lha\_2.15' folder called 'content'
1. Make one more sub folder in this called \_\_c\_\_
1. You should have a folder with the full path 'C:\MyAmigaOs\Source\Lha\_3.1\content\\_\_c\_\_'
1. Copy the desired lha executable from your 'unarchived' folder into the newly created '\_\_c\_\_' folder. Rename the file to 'lha'
1. Add a Lha package to the code, like this
    ```csharp
    new Package
    {
        Include = true,
        Path = "Lha\_2.15",
        Category = "Util",
        Description = "Lha command line (un)archiving",
		Url = "http://aminet.net/package/util/arc/lha_68k",
    },
    ```
1. Run the code again
1. The log will tell you that the Lha executable is copied to your MyAmigaOs's C-folder!
1. Ensure that it work by opening a Shell in your Workbench and run lha

### Let's add Backdrop-Settings
1. The tool can also handle 'reverse content' wich means that files changed inside your OS (emulator) will be synced back to your package. This is used for system- and program settings.
1. Make a sub folder in Source, named 'Backdrop-Settings'
1. Make a sub folder to this called 'content_reverse'
    -The reverse part indicates that the content will Sync both ways
1. Make a sub folder to this called '\_\_systemdrive\_\_'
1. In your workbench, create a '.backdrop' file by selecting 'Leave out' for Shell
1. Copy the '.backdrop' file from the 'Output\System' folder to the newly created '\_\_systemdrive\_\_' folder
1. Everything is already up to date (because you copied the correct file already)
1. But, as you've created this 'reverse content' package now, whenever you select 'Leave out' for more applications of folders, you can simple run the tool to sync those settings 'back' to MyAmigaOs.

### And Env-Archive
1. Make a sub folder in Source, named 'Env-Archive'
1. Make a sub folder to this called 'content_reverse'
1. Make a sub folder to this called '\_\_prefs\_\_'
1. Make a sub folder to this called 'Env-Archive'
1. Inside this folder, create an empty file called 'content_reverse_all' without extensions
1. Now when you run the tool, all files from 'Prefs\\Env-Archive' will be synced back to MyAmigaOs. Try it out by changing some system settings like Screen Resoultion or Input keyboard language.
