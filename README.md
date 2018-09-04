# AmigaOsBuilder

## About
Used by myself to compose my version of Amiga OS.

Built using as a .NET Core Command Line tool, so it might work on Apple and Linux.

Currently most stuff is configured by code, but should be moved to external json files. I might build some ui to make the json files easier to manage in the future.

## How to use it
1. Make some work folder (e.g. C:\MyAmigaOs)
2. Make two sub folders: Source and Output
3. For easy testing, configure an Amiga emulator to use the Output folder as a hard drive
    - If you start your emulator now it should say something like "No disk present in device DH0"
4. Change application settings to match your created folders.
  - Location and ConfigName is not in use
5. 