using System;
using System.Collections.Generic;

namespace AmigaOsBuilder
{
    public class ConfigService
    {
        /*
            todo:
            http://aminet.net/package/dev/misc/AmigaOS_guides

            http://aminet.net/package/disk/misc/SD3V13 High-speed disk copier and formatter

            http://aminet.net/package/mus/play/PlaySID

            http://aminet.net/package/dev/gui/classact33

            https://github.com/jens-maus/amissl/releases
            http://aminet.net/package/dev/debug/SegTracker SegTracker 45.1 - A global SegList tracking utility
            (no) http://aminet.net/search?query=enforcer 
            (no) http://aminet.net/package/dev/debug/enforcer
            http://aminet.net/package/dev/debug/MuForce V40 Enforcer,detect illegal RAM accesses
            http://aminet.net/package/dev/debug/Sushi Intercept Enforcer raw serial output
            http://aminet.net/package/util/misc/ReportPlus Multipurpose utility
            http://aminet.net/package/text/font/CharMap Display the whole charset of a given font
            http://aminet.net/package/disk/misc/tsgui Create or write back image files (ADF/HDF)

            http://aminet.net/package/mus/play/AmigaAMP3-68k
            http://aminet.net/package/comm/tcp/dizzytorrent2
            http://aminet.net/package/comm/misc/twittAmiga

            http://www.classicamiga.com/content/view/4585/175/ FileMaster 3.1 Beta 9 incl. key
            http://aminet.net/package/util/dir/DiskMaster DiskMaster2 - 68K & OS4


            SASC
            StormC
            DevPac
            http://aminet.net/package/dev/misc/FlexCat-2.18.lha

        */
        public static Config SysConfig()
        {
            return new Config
            {
                SourceBasePath = "E:\\Amiga\\KrustWB3\\Source\\",
                OutputBasePath = "E:\\Amiga\\KrustWB3\\Output\\System\\",
                Aliases = SysAliases,
                Packages = SysPackages,
                ReverseSync = true,
                UserStartup = true,
            };
        }
        public static Config SysLhaConfig()
        {
            return new Config
            {
                SourceBasePath = "E:\\Amiga\\KrustWB3\\Source\\",
                OutputBasePath = "E:\\Amiga\\KrustWB3\\TempDisk\\KrustWB\\System.lha",
                Aliases = SysAliases,
                Packages = SysPackages,
                ReverseSync = false,
                UserStartup = true,
            };
        }
        public static Config WorkConfig()
        {
            return new Config
            {
                SourceBasePath = "E:\\Amiga\\KrustWB3\\Source\\",
                OutputBasePath = "E:\\Amiga\\KrustWB3\\Output\\Work\\",
                Aliases = WorkAliases,
                Packages = WorkPackages,
                ReverseSync = true,
                UserStartup = false,
            };
        }
        public static Config WorkLhaConfig()
        {
            return new Config
            {
                SourceBasePath = "E:\\Amiga\\KrustWB3\\Source\\",
                OutputBasePath = "E:\\Amiga\\KrustWB3\\TempDisk\\KrustWB\\Work.lha",
                Aliases = WorkAliases,
                Packages = WorkPackages,
                ReverseSync = false,
                UserStartup = false,
            };
        }
        public static Config DevConfig()
        {
            return new Config
            {
                SourceBasePath = "E:\\Amiga\\KrustWB3\\Source\\",
                OutputBasePath = "E:\\Amiga\\KrustWB3\\Output\\Dev\\",
                Aliases = DevAliases,
                Packages = DevPackages,
                ReverseSync = true,
                UserStartup = false,
            };
        }
        public static Config DevLhaConfig()
        {
            return new Config
            {
                SourceBasePath = "E:\\Amiga\\KrustWB3\\Source\\",
                OutputBasePath = "E:\\Amiga\\KrustWB3\\TempDisk\\KrustWB\\Dev.lha",
                Aliases = DevAliases,
                Packages = DevPackages,
                ReverseSync = false,
                UserStartup = false,
            };
        }
        public static Config InstallerConfig()
        {
            return new Config
            {
                SourceBasePath = "E:\\Amiga\\KrustWB3\\Source\\",
                OutputBasePath = "E:\\Amiga\\KrustWB3\\TempDisk\\KrustWB\\i\\",
                Aliases = InstallerAliases,
                Packages = InstallerPackages,
                ReverseSync = true,
                UserStartup = false,
            };
        }

        private static readonly IDictionary<string, string> SysAliases = new Dictionary<string, string>
        {
            // Amiga OS folders
            { @"__c__",                  @"C" },
            { @"__devs__",               @"Devs" },
            { @"__l__",                  @"L" },
            { @"__locale__",             @"Locale" },
            { @"__libs__",               @"Libs" },
            { @"__prefs__",              @"Prefs" },
            { @"__s__",                  @"S" },
            { @"__storage__",            @"Storage" },
            { @"__system__",             @"System" },
            { @"__utilities__",          @"Utilities" },
            { @"__wbstartup__",          @"WBStartup" },

            // KrustWB folders
            { @"__aprograms__",          @"A-Programs" },
            { @"__asystem__",            @"A-System" },
            { @"__aguides__",            @"A-Guides" },
        };
        private static readonly IDictionary<string, string> WorkAliases = new Dictionary<string, string>
        {
            // KrustWB folders
            { @"__aguides__",            @"A-Guides" },
            { @"__awhdgames__",          @"A-WHDGames" },
            { @"__ademos__",             @"A-Demos" },
            { @"__amods__",              @"A-Mods" },
        };
        private static readonly IDictionary<string, string> DevAliases = new Dictionary<string, string>
        {
            // KrustWB folders
            { @"__aguides__",            @"A-Guides" },
            { @"__sdk__",                @"SDK" },
        };
        private static readonly IDictionary<string, string> InstallerAliases = new Dictionary<string, string>
        {
        };

        private static readonly List<Package> SysPackages = new List<Package>
        {
            #region Workbench
            new Package(false, "TestReverseSync_1.0")
            {
                Category = "Test",
                Description = "Test content reverse",
                //Source = ""
            },
            new Package(false, "Workbench (clean install)_3.1")
            {
                Category = "OS",
                Description = "Workbench 3.1 operation system (clean Install)",
                //Source = ""
            },
            new Package(true, "Workbench (clean install)_3.1.4")
            {
                Category = "OS",
                Description = "Workbench 3.1.4 operation system (clean Install) [FIX LHA CONTENT FILE]",
                //Source = ""
            },
            new Package(true, "Workbench (extra files)_3.1.4")
            {
                Category = "OS",
                Description = "Workbench 3.1.4 operation system (extra files) i.e. c:Reboot",
            },
            new Package(true, "Workbench (glowicons)_3.1.4")
            {
                Category = "OS",
                Description = "Workbench 3.1.4 Glowicons [FIX LHA CONTENT FILE]",
                //Source = ""
            },
            new Package(true, "Workbench (intuition-v45-library)_3.1.4")
            {
                Category = "OS",
                Description = "Workbench 3.1.4 operation system v-45 intuition.library. Will allow you to drag windows partially off the screen [FIX LHA CONTENT FILE]",
                //Source = ""
            },
            new Package(true, "WorkbenchMultiView")
            {
                Category = "OS",
                Description = "Copy from Amiga OS Utilities folder. Also replaces AmigaGuide.",
            },
            new Package(true, "WorkbenchBackups")
            {
                Category = "OS",
                Description = "Backups of Workbench files that will be replaced by other packages (e.g. c:info to c:info_original)",
            },
            #endregion

            #region KrustWB
            new Package(true, "KrustWBStartupSequence")
            {
                Category = "KrustWB",
                Description = "KrustWB startup-sequence and user-startup files",
                //Source = ""
            },
            new Package(true, "KrustWBMaxMemSequence")
            {
                Category = "KrustWB",
                Description = "MaxMem-Sequence to boot to CLI with max memory",
                //Source = ""
            },
            new Package(true, "KrustWBBackdropSys")
            {
                Category = "KrustWB",
                Description = "KrustWB .backdrop file for System: drive (OS setting file that keeps track of \"Leave Out\").",
                //Source = ""
            },
            new Package(true, "Env-Archive")
            {
                Category = "KrustWB",
                Description = "KrustWB system settings files kept in Prefs/Env-Archive",
                //Source = ""
            },
            new Package(true, "Monitors")
            {
                Category = "KrustWB",
                Description = "KrustWB monitors Devs/Monitors",
                //Source = ""
            },
            new Package(true, "A-DirectoriesSys")
            {
                Category = "KrustWB",
                Description = "A-Directories including icons (System: drive)",
                //Source = ""
            },
            new Package(false, "KrustWBInstall")
            {
                Category = "KrustWB",
                Description = "Scripts to pack and install KrustWB",
                //Source = ""
            },
            new Package(true, "KrustWBRAMDiskInfo_0.1")
            {
                Category = "KrustWB",
                Description = "Disk.info handling for RAM: drive - to support snapshot via script",
                //Source = ""
            },
            new Package(true, "KrustWBShellStartup")
            {
                Category = "KrustWB",
                Description = "Shell-Startup script",
                //Source = ""
            },
            
            #endregion

            #region ROMS
            new Package(true, "AmigaOS ROM_46.143")
            {
                Category = "ROMS",
                Description = "Amiga OS ROM 46.143 (3.1.4) [FIX LHA CONTENT FILE]",
                //Source = ""
            },
            #endregion

            #region A-System
            new Package(false, "SetPatch_43.6b")
            {
                Category = "System",
                Description = "Makes ROM patches in system software",
                Source = "http://m68k.aminet.net/package/util/boot/SetPatch_43.6b"
            },
            new Package(true, "NoClick_1.1")
            {
                Category = "System",
                Description = "Disables the clicking of the floppy drives.",
                Source = "http://aminet.net/package/disk/misc/NoClick"
            },
            new Package(false, "Installer_44.10")
            {
                Category = "System",
                Description = "Installer software",
                //Source = ""
            },
            new Package(false, "InstallerNG_1.5 pre")
            {
                Category = "System",
                Description = "Installer software",
                Source = "http://aminet.net/package/util/sys/InstallerNG"
            },
            new Package(true, "CardPatch_1.2")
            {
                Category = "System",
                Description = "When a PC Card is plugged in the PCMCIA slot and cnet.device is not run then Amiga system slows."
                                + " CardPatch patches this \"slow bug\" and other bugs in card.resource. The CardResetCard() function"
                                + " is patched and each \"new\" card is reseted after it is inserted in the PCMCIA slot.",
                Source = "http://aminet.net/package/util/boot/CardPatch"
            },
            new Package(true, "CardReset_3.0")
            {
                Category = "System",
                Description = "CardReset forces a high level on pin 58 of the Amiga PCMCIA slot (reset signal)",
                Source = "http://aminet.net/package/util/boot/CardReset"
            },
            new Package(false, "Borderblank")
            {
                Category = "System",
                Description = "This simply blanks the border",
                Source = "http://aminet.net/package/util/boot/bordblnk"
            },
            new Package(true, "Borderblank_FromClassicWb")
            {
                Category = "System",
                Description = "This simply blanks the border",
            },
            new Package(true, "LoadModule_45.15")
            {
                Category = "System",
                Description = "LoadModule installs \"resident modules\" in a reset-proof way.",
                Source = "http://aminet.net/package/util/boot/LoadModule"
            },
            new Package(false, "SCSI_43.45p")
            {
                Category = "System",
                Description = "Patched scsi.device to enable use of 128 GB or bigger IDE devices",
                Source = "http://aminet.net/package/driver/media/SCSI4345p"
            },
            new Package(true, "Fat95_3.18")
            {
                Category = "System",
                Description = "a DOS handler to mount and use Win95/98 volumes just as if they were AMIGA volumes.",
                Source = "http://aminet.net/package/disk/misc/fat95"
            },
            new Package(true, "Cfd_1.33")
            {
                Category = "System",
                Description = "Read and write files from CompactFlash cards",
                Source = "http://aminet.net/package/driver/media/CFD133"
            },
            new Package(false, "AmigaOS ROM Update from OS3.9 BB2")
            {
                Category = "System",
                Description = "44.57 AmigaOS ROM Update from OS3.9 BB2",
            },
            new Package(true, "AssignWedge_1.5")
            {
                Category = "System",
                Description = "Add Mount and Assign options to Amiga requester",
                Source = "http://aminet.net/package/util/wb/AssignWedge",
            },
            new Package(true, "Roadshow demo_1.13")
            {
                Category = "System",
                Description = "Amiga TCP/IP stack (demo version)",
            },
            new Package(true, "3c589_1.5")
            {
                Category = "System",
                Description = "SANA-II network driver for 3Com Etherlink III PC Cards (PCMCIA cards)",
            },
            new Package(true, "Cnetdevice_1.9")
            {
                Category = "System",
                Description = "PCMCIA (aka PC Card) ethernet card SANA2 driver for Amiga 600 and Amiga 1200 computers",
            },
            new Package(false, "scrsh")
            {
                Category = "System",
                Description = "Opens Shell in a screen",
                Source = "http://aminet.net/package/util/cli/ksc_scrsh",
            },
            new Package(true, "ShellScreen_1.6")
            {
                Category = "System",
                Description = "Opens Shell in a screen",
                Source = "http://aminet.net/package/util/shell/ShellScr",
            },
            new Package(false, "ViNCEd_3.73")
            {
                Category = "System",
                Description = "Full screen shell editor",
                Source = "http://aminet.net/package/util/shell/ShellScr",
            },
            new Package(true, "KingCON_1.3")
            {
                Category = "System",
                Description = @"A console-handler that optionally replaces the standard console devices. Adds some useful features, such as Filename-completion",
                Source = "http://aminet.net/package/util/shell/KingCON_1.3"
            },
            new Package(true, "MX1000_1.0")
            {
                Category = "System",
                Description = @"AmigaKit DB9 optical mouse driver (MX1000.driver and MX1000Test)",
                Source = "https://mega.nz/#!RttR3BiS!7FiIN5l-t5N6-NP66d79h9qcaOh1f9qQ46wwZtiEJVY"
            },
            new Package(true, "NewMouse_1.2")
            {
                Category = "System",
                Description = @"Serial mouse driver, wheel support",
                Source = "http://aminet.net/package/driver/input/NewMouse12"
            },
            new Package(true, "FreeWheel_2.2.2")
            {
                Category = "System",
                Description = @"A tool to fine-tune your mouse",
                Source = "http://m68k.aminet.net/package/util/mouse/FreeWheel"
            },
            new Package(true, "JoyPortTest_0.1")
            {
                Category = "System",
                Description = @"JoyPortTest  is an utility to test and check your joystick, CD32 joypad and mouse connected on amiga port 0 and 1.  ",
                Source = "http://aminet.net/package/driver/input/JoyPortTest"
            },
            new Package(false, "ToolsDaemon_2.1a")
            {
                Category = "System",
                Description = @"ToolsDaemon allows you to run programs simply by selecting a menu item from the menu strip of Workbench",
                Source = "http://aminet.net/package/util/boot/ToolsDaemon21a"
            },
            new Package(false, "ToolsDaemon_2.2")
            {
                Category = "System",
                Description = @"These patches fix ToolsDaemon 2.1a, written by Nico François, to take advantage of V45 (OS 3.9) Workbench API",
                Source = "http://aminet.net/package/util/boot/ToolsDaemon22"
            },
            new Package(true, "ToolsMenu_1.6")
            {
                Category = "System",
                Description = @"Add tools to the Workbench Tools menu",
                Source = "http://aminet.net/package/util/cdity/ToolsMenu"
            },
            new Package(false, "Info_39.18b")
            {
                Category = "System",
                Description = @"Info is a replacement for the original AmigaDOS 'info' command.",
                Source = "http://aminet.net/package/util/sys/info"
            },
            new Package(true, "Dr_2.0")
            {
                Category = "System",
                Description = @"Dir replacement",
                Source = "http://paulkienitz.net/amiga/"
            },
            new Package(false, "EvenMore_0.91")
            {
                Category = "System",
                Description = @"Text viewer",
                Source = "http://www.evenmore.co.uk/"
            },
            new Package(true, "MuchMore_4.6")
            {
                Category = "System",
                Description = @"Text viewer",
                Source = "http://aminet.net/package/text/show/muchmore46"
            },
            new Package(true, "TextView_1.25")
            {
                Category = "System",
                Description = @"Text viewer",
                Source = "http://aminet.net/package/text/show/TextView125"
            },
            new Package(false, "PatchRAM_1.11")
            {
                Category = "System",
                Description = @"Patches the RAM disk to show the real size occupied",
                Source = "http://aminet.net/package/util/sys/PatchRAM"
            },
            new Package(true, "MUI_3.9-2015R1")
            {
                Category = "System",
                Description = @"Magical User Interface",
                Source = "https://muidev.de/downloads"
            },
            new Package(true, "BlizKick_1.24")
            {
                Category = "System",
                Description = @"BlizKick is used to rekick any Kickstart ROM image with Blizzard turbo boards having MAPROM feature (jumper).",
                Source = "http://aminet.net/package/util/boot/BlizKick"
            },
            new Package(true, "MakeIcon_1.5")
            {
                Category = "System",
                Description = @"Create Icons",
                Source = "http://aminet.net/package/util/cli/MakeIcon1_5"
            },
            new Package(true, "JanoEditor_1.01d")
            {
                Category = "System",
                Description = @"Text editor",
                Source = "http://aminet.net/package/text/edit/JanoEditor"
            },
            new Package(false, "MCP_1.48")
            {
                Category = "System",
                Description = @"Master Control Program. Collect all usual patches for AmigaOS at a time when the development of AmigaOS seemed to have stopped",
                Source = "http://mcp.a1k.org/indexe.html"
            },
            new Package(true, "TransADF_4.0.46")
            {
                Category = "System",
                Description = @"Makes Compressed ADFs",
                Source = "http://aminet.net/package/disk/misc/TransADF"
            },
            new Package(true, "DirSSCompare_1.4")
            {
                Category = "System",
                Description = @"Another program to compare two dirs for search differences",
                Source = "http://aminet.net/package/util/dir/DirSSCompare"
            },            
            new Package(true, "CompareDirs_1.1")
            {
                Category = "System",
                Description = @"CompareDirs allows you to compare the directory stucture of two directories",
                Source = "http://aminet.net/package/util/cli/CompareDirs"
            },
            new Package(true, "ALeXcompare_2.0")
            {
                Category = "System",
                Description = @"cmp is a program, which compares files in two directories  (or trees)",
                Source = "http://aminet.net/package/util/cli/ALeXcompare"
            },            
            new Package(true, "IconGrid_1.0")
            {
                Category = "System",
                Description = @"IconGrid is a Shell tool to align Workbench icons on a virtual grid",
                Source = "http://aminet.net/package/util/cli/IconGrid"
            },
            new Package(false, "Huge_1.1")
            {
                Category = "System",
                Description = @"icons, sprites, bobs Editor anno 1990 (from 1990!)",
                Source = "http://aminet.net/package/gfx/edit/Huge"
            },
            new Package(true, "AmiFTP_1.843")
            {
                Category = "System",
                Description = @"Easy to use GUI FTP client for OS 2.0+",
                Source = "http://aminet.net/package/comm/tcp/AmiFTP"
            },
            new Package(true, "Iconian_2.98u")
            {
                Category = "System",
                Description = @"OS3.0 icon editor, NewIcon support",
                Source = "http://aminet.net/package/gfx/edit/Iconian2_98u"
            },
            new Package(true, "ProcessIcon_1.19")
            {
                Category = "System",
                Description = @"V1.19 CLI tool to manipulate icon data",
                Source = "http://aminet.net/package/util/wb/ProcessIcon"
            },
            new Package(true, "IconSnap_0.3")
            {
                Category = "System",
                Description = @"Snap Workbench icons to a virtual grid",
                Source = "http://aminet.net/package/util/wb/IconSnap"
            },
            new Package(false, "AFnews_1.03")
            {
                Category = "System",
                Description = @"Amiga Future Everywhere 68k",
                Source = "http://aminet.net/package/comm/news/AFnews-68k"
            },
            new Package(true, "SimpleFind3_1.2")
            {
                Category = "System",
                Description = @"V1.2 of the renewed File Finder",
                Source = "http://aminet.net/package/util/dir/simplefind3"
            },
            new Package(true, "rewincy_0.8")
            {
                Category = "System",
                Description = @"Cycle through windows/screens Alt-Tab",
                Source = "http://aminet.net/package/util/cdity/rewincy.lha"
            },
            new Package(true, "DeliTracker_2.32")
            {
                Category = "System",
                Description = @"Final Amiga Version, now Freeware",
                Source = "http://aminet.net/package/mus/play/DeliTracker232"
            },
            new Package(true, "GetMouseInput_1.4")
            {
                Category = "System",
                Description = @"V1.4 - read mouse button state",
                Source = "http://aminet.net/package/util/boot/GetMouseInput"
            },
            new Package(true, "BoardsLib_3.42")
            {
                Category = "System",
                Description = @"provides detailed info about expansions",
                Source = "http://aminet.net/package/util/libs/BoardsLib"
            },
            new Package(true, "WhichAmiga_1.3.3")
            {
                Category = "System",
                Description = @"ShowConfig kind of tool. V1.3.3",
                Source = "http://aminet.net/package/util/moni/WhichAmiga"
            },
            new Package(true, "Redit_2.0")
            {
                Category = "System",
                Description = @"Text editor",
                Source = "http://aminet.net/package/text/edit/Redit"
            },
            #endregion

            #region Libraries
            new Package(false, "ReqTools_38.1210")
            {
                Category = "Library",
                Description = @"ReqTools library",
                Source = "http://aminet.net/package/util/boot/ToolsDaemon21a"
            },
            new Package(true, "ReqTools_39.3")
            {
                Category = "Library",
                Description = @"ReqTools library",
                Source = "http://aminet.net/package/util/libs/ReqToolsLib"
            },
            new Package(true, "guigfxnofpu_20.0")
            {
                Category = "Library",
                Description = @"Application layer for pixel graphics (no fpu version)",
                Source = "http://aminet.net/package/dev/misc/guigfxlib_nofpu"
            },
            new Package(true, "MCCGuigfx_19.2")
            {
                Category = "Library",
                Description = @"Guigfx Custom Class for Magic User Interface",
                Source = "http://aminet.net/package/dev/mui/MCC_Guigfx"
            },
            new Package(true, "MCCTextEditor_15.50")
            {
                Category = "Library",
                Description = @"TextEditor Custom Class for Magic User Interface",
                Source = "http://aminet.net/package/dev/mui/MCC_TextEditor-15.50"
            },
            new Package(true, "MCCTextEditor_15.50")
            {
                Category = "Library",
                Description = @"TextEditor Custom Class for Magic User Interface",
                Source = "http://aminet.net/package/dev/mui/MCC_TextEditor-15.50"
            },
            new Package(true, "MCC_NList_0.124")
            {
                Category = "Library",
                Description = @"NList custom classes for MUI",
                Source = "http://aminet.net/package/dev/mui/MCC_NList-0.124"
            },
            new Package(false, "renderlib_40.8")
            {
                Category = "Library",
                Description = @"shared library that serves an image processing kernel (re-implementation of render.library in ANSI C)",
                Source = "http://aminet.net/package/dev/misc/renderlib"
            },
            new Package(true, "renderlib_31")
            {
                Category = "Library",
                Description = @"shared library that serves an image processing kernel (re-implementation of render.library in ANSI C)",
                Source = "http://aminet.net/package/dev/misc/renderlib31"
            },
            new Package(true, "MMULib_46.16")
            {
                Category = "Library",
                Description = "Library to ctrl the MC68K MMUs",
                //Source = ""
            },
            new Package(true, "WBStart_2.2")
            {
                Category = "Library",
                Description = "Emulate program starting from WB (V2.2)",
                Source = "http://aminet.net/package/util/libs/WBStart"
            },
            #endregion

            #region A-Programs
            new Package(true, "Lha_2.15")
            {
                Category = "Program",
                Description = "Lha command line (un)archiving",
                Source = "http://aminet.net/package/util/arc/lha_68k"
            },
            new Package(true, "Sha256_1.1")
            {
                Category = "Program",
                Description = @"A command line utility to calculate the SHA-256 hashes of a list of files",
                Source = "http://aminet.net/package/util/cli/sha256"
            },
            new Package(true, "spatch_6.51 rel 4")
            {
                Category = "Program",
                Description = @"Clone of SAS Binary File Patcher",
                Source = "http://aminet.net/package/dev/misc/spatch"
            },
            new Package(true, "SysInfo_4.0")
            {
                Category = "Program",
                Description = @"Util for getting information about the system, like OS and library versions, hardware revisions and stuff",
                Source = "https://sysinfo.d0.se/"
            },
            new Package(true, "SnoopDos_3.8")
            {
                Category = "Program",
                Description = @"System and application monitor",
                Source = "http://aminet.net/package/util/moni/SnoopDos"
            },
            new Package(true, "IconZ_1.1")
            {
                Category = "Program",
                Description = @"Sorts Workbench icons",
                Source = "http://aminet.net/package/util/wb/IconZ"
            },
            new Package(true, "SortIconsOld_1.0")
            {
                Category = "Program",
                Description = @"Sorts Workbench icons",
                Source = "http://aminet.net/package/util/wb/SortIconsOld"
            },
            new Package(true, "WHDLoad usr_18.3")
            {
                Category = "Programs",
                Description = @"WHDLoad",
                Source = "http://whdload.de/"
            },
            new Package(true, "iGame_1.5")
            {
                Category = "Programs",
                Description = @"Frontend to launching WHDLoad games",
                Source = "http://aminet.net/package/util/misc/iGame"
            },
            new Package(true, "DirOpus_4.16")
            {
                Category = "Programs",
                Description = @"Legendary File Manager for Amiga Computers",
                Source = "http://aminet.net/package/util/dopus/DOpus416JRbin"
            },
            #endregion
        };

        private static readonly List<Package> WorkPackages = new List<Package>
        {
            #region KrustWB   

            new Package(true, "A-DirectoriesWork")
            {
                Category = "KrustWB",
                Description = "A-Directories including icons (Work: drive)",
                //Source = ""
            },
            new Package(true, "KrustWBBackdropWork")
            {
                Category = "KrustWB",
                Description = "KrustWB .backdrop file for Work: drive (OS setting file that keeps track of \"Leave Out\").",
                //Source = ""
            },
            
            #endregion

            #region A-Games     

            new Package(true, "WHDGames")
            {
                Category = "Games",
                Description = @"WHDGames",
                Source = "http://aminet.net/package/util/moni/SnoopDos"
            },

            #endregion

            #region A-Demos
            new Package(true, "ADemos")
            {
                Category = "Demos",
                Description = @"Demos",
            },
            #endregion

            #region A-Mods
            new Package(true, "AMods")
            {
                Category = "Mods",
                Description = @"Mods",
            },
            #endregion
        };

        private static readonly List<Package> DevPackages = new List<Package>
        { 
            #region KrustWB            
            //new Package(true, "A-DirectoriesDev")
            //{
            //    Category = "KrustWB",
            //    Description = "A-Directories including icons (Dev: drive)",
            //    //Source = ""
            //},
            new Package(true, "KrustWBBackdropDev")
            {
                Category = "KrustWB",
                Description = "KrustWB .backdrop file for Dev: drive (OS setting file that keeps track of \"Leave Out\").",
                //Source = ""
            },
            #endregion

            #region Dev
            new Package(true, "NDK_3.9")
            {
                Category = "Dev",
                Description = @"Amiga OS 3.9 NDK (Native Development Kit)",
                Source = "http://www.haage-partner.de/download/AmigaOS/NDK39.lha"
            },
            new Package(true, "vbcc-bin-amigaos68k_0.906")
            {
                Category = "Dev",
                Description = @"Highly optimizing portable and retargetable ISO C compiler. It supports ISO C according to ISO/IEC 9899:1989 and a subset of the new standard ISO/IEC 9899:1999 (C99).",
                Source = "http://sun.hasenbraten.de/vbcc/"
            },
            new Package(false, "vbcc-bin-amigaos68k-installer_0.906")
            {
    
                Category = "Dev",
                Description = @"Installer for: Highly optimizing portable and retargetable ISO C compiler. It supports ISO C according to ISO/IEC 9899:1989 and a subset of the new standard ISO/IEC 9899:1999 (C99).",
                Source = "http://sun.hasenbraten.de/vbcc/"
            },
            new Package(true, "vasmm68k_mot_os3_1.8d")
            {
                Category = "Dev",
                Description = @"vasm is a portable and retargetable assembler to create linkable objects in various formats or absolute code. Motorola/Freescale 68k syntax (Devpac compatible)",
                Source = "http://sun.hasenbraten.de/vasm"
            },
            new Package(true, "vlink_AmigaM68k_0.16a")
            {
                Category = "Dev",
                Description = @"vlink is a portable linker, written in ANSI-C, that can read and write a wide range of object- and executable file formats. ",
                Source = "http://sun.hasenbraten.de/vlink/"
            },
            new Package(true, "vbcc-target-m68k-amigaos_0.906")
            {
                Category = "Dev",
                Description = @"VBCC Compiler target AmigaOS 2.x/3.x M680x0.",
                Source = "http://sun.hasenbraten.de/vbcc/"
            },
            new Package(false, "vbcc-target-m68k-amigaos-installer_0.906")
            {
                Category = "Dev",
                Description = @"Installer for: VBCC Compiler target AmigaOS 2.x/3.x M680x0.",
                Source = "http://sun.hasenbraten.de/vbcc/"
            },
            new Package(true, "vbcc-PosixLib_2.6")
            {
                Category = "Dev",
                Description = @"This linker library contains the most important POSIX functions and some other useful UNIX functions, which are not in the standard C-library.",
                Source = "http://aminet.net/package/dev/c/vbcc_PosixLib"
            },
            new Package(false, "AsmOne_1.25")
            {
                Category = "Dev",
                Description = @"(seems like 1.29 actually) 68k/PPC Assembler Development Environment for the Amiga",
                Source = "From My old Amiga"
            },
            new Package(true, "AsmOne_1.29")
            {
                Category = "Dev",
                Description = @"68k/PPC Assembler Development Environment for the Amiga",
                Source = "From old floppies"
            },
            new Package(true, "AsmOne_1.48")
            {
                Category = "Dev",
                Description = @"68k/PPC Assembler Development Environment for the Amiga",
                Source = "http://aminet.net/package/dev/asm/ASM-One"
            },
            new Package(false, "AutoDocReader_1.6.5")
            {
                Category = "Dev",
                Description = @"View autodocs, C headers and text files (requires cybergraphics.library)",
                Source = "http://aminet.net/package/dev/misc/AutoDocReader_1v65", 
                
            },
            #endregion

            #region WS
            new Package(true, "KrustWBDevWS")
            {
                Category = "KrustWB",
                Description = "Dev Workspace folder content_reverse_all",
                //Source = ""
            },
            #endregion

        };

        private static readonly List<Package> InstallerPackages = new List<Package>
        { 
            #region Installer
            new Package(true, "KrustWBInstall_3.0")
            {
                Category = "KrustWB",
                Description = "Scripts to pack and install KrustWB",
                //Source = ""
            },
            #endregion

        };

        //public static Config GetSysConfig(string configFileName)
        //{
            //throw new NotImplementedException();
            //var config = SysConfig;
            //          "ConfigFile": "E:\\Amiga\\KrustWB3\\config.json",
            //"SourceBasePath": "E:\\Amiga\\KrustWB3\\Source\\",
            //"OutputBasePath": "E:\\Amiga\\KrustWB3\\Output\\",
            //"SourceBasePath2": "E:\\Amiga\\KrustWB3\\Source\\",
            //"OutputBasePath2": "E:\\Amiga\\KrustWB3\\Output.lha"


            //var configFilePath = Path.Combine(location, configFileName);

            //// Serialize
            //var configString = JsonConvert.SerializeObject(config, Formatting.Indented);
            //File.WriteAllText(configFileName, configString, Encoding.UTF8);

            //// Deserialize
            //var configString = File.ReadAllText(configFileName);
            //config = JsonConvert.DeserializeObject<Config>(configString);

            //return config;
        //}
    }
}