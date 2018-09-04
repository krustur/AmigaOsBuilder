# AmigaOsBuilder

## About
Used by myself to compose my version of Amiga OS.

Built using as a .NET Core Command Line tool, so it might work on Apple and Linux.

Currently most stuff is configured by code, but should be moved to external json files. I might build some ui to make the json files easier to manage in the future.

## How to use it
This is a quick example of how to compose a basic Workbench 3.1 with one extra package (I'll use Lha) and some reverse content (explained below)

### Preparation
1. Make some work folder (e.g. C:\MyAmigaOs)
2. Make two sub folders: Source and Output
4. Change application settings to match your created folders.
    - Location and ConfigName is currently not in use
5. Change application settings 'SyncMode' to 'Synchronize'
    - Other modes will be removed

### Vanilla Workbench 3.1
1. Make a sub folder in Source, named 'Workbench_3.1'
2. Make one more sub folder in this called content
3. Make one more sub folder in this called __systemdrive__
3. You should probably have a folder with the full path 'C:\MyAmigaOs\Source\Workbench_3.1\content'
4. Make a clean install of Workbench 3.1 somewhere, and copy the contents of this folder into the folder above
    -You have now created a simple base package for Workbench 3.1

### First build of MyAmigaOs
    -(this is the part that should probably be located in a json file outside of the code. However I found it easier to have it in code for now, as its easier to spot build errors in VS)

### First test
1. To test this, configure an Amiga emulator to use the Outpu\System folder as a hard drive
2. When you start your emulator it should load your vanilla install of Workbench 3.1

### todo in readme
debug logging off
priority
content_reverse