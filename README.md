# AmigaOsBuilder

## About
Used by myself to compose my version of Amiga OS.

Built using as a .NET Core Command Line tool, so it might work on Apple and Linux.

Currently most stuff is configured by code, but should be moved to external json files. I might build some ui to make the json files easier to manage in the future.

*Coding skills required!*

## How to use it
This is a quick example of how to compose a basic Workbench 3.1 with one extra package (I'll use Lha) and some reverse content (explained below).

### Preparation
1. Make some work folder (e.g. C:\MyAmigaOs)
1. Make two sub folders: Source and Output
1. Change application settings to match your created folders.
    - Location and ConfigName is currently not in use
1. Change application settings 'SyncMode' to 'Synchronize'
    - Other modes will be removed

### Vanilla Workbench 3.1
1. Make a sub folder in Source, named 'Workbench\_3.1'
1. Make one more sub folder in this called content
1. Make one more sub folder in this called \_\_systemdrive\_\_
1. You should probably have a folder with the full path 'C:\MyAmigaOs\Source\Workbench_3.1\content\\_\_systemdrive\_\_'
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
        Path = "Workbench_3.1",
        Category = "OS",
        Description = "Workbench 3.1 operation system (clean Install)",
    },
    ```
1. To ensure you're ony receiving useful logging, ensure that `.MinimumLevel.Debug()` when initializing Serilog is commented out/removed
1. Run this code (in Visual Studio or from command line)
1. This will cause all of the files from the 'C:\MyAmigaOs\Source\Workbench_3.1\content\__systemdrive__'  folder to be copied into the 'C:\MyAmigaOs\Output\System' folder
    -Note that the __systemdrive__ has been changed to System. There's a bunch of aliases that you can use that will map to specific output folders. For the full list, check out `private static readonly IDictionary<string, string> AliasToOutputMap = new Dictionary<string, string> { ... }` in the code.

### Output folder aliases


### First test
1. To test this, configure an Amiga emulator to use the Outpu\System folder as a hard drive
1. When you start your emulator it should load your vanilla install of Workbench 3.1

### todo in readme
debug logging off
priority
content_reverse