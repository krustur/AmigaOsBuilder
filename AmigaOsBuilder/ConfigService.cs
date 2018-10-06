using System.Collections.Generic;

namespace AmigaOsBuilder
{
    public class ConfigService
    {
        // @formatter:off

        private static readonly Config TestConfig = new Config
        {
            Packages =
                new List<Package>
                {
                    #region OS
                    new Package
                    {
                        Include = true,
                        Path = "Workbench (clean install)_3.1",
                        Category = "OS",
                        Description = "Workbench 3.1 operation system (clean Install)",
                        //Source = ""
                    },
                    new Package
                    {
                        Include = true,
                        Path = "WorkbenchMultiView",
                        Category = "OS",
                        Description = "Copy from Amiga OS Utilities folder. Also replaces AmigaGuide.",
                    },
                    new Package
                    {
                        Include = true,
                        Path = "WorkbenchBackups",
                        Category = "OS",
                        Description = "Backups of Workbench files that will be replaced by other packages (e.g. c:info to c:info_original)",
                    },
                    #endregion
                    #region KrustWB
                    new Package
                    {
                        Include = true,
                        //Name = "Startup-Sequence",
                        Path = "Startup-Sequence",
                        Category = "KrustWB",
                        Description = "KrustWB startup-sequence and user-startup files",
                        //Source = ""
                    },
                    new Package
                    {
                        Include = true,
                        //Name = "Backdrop",
                        Path = "Backdrop",
                        Category = "KrustWB",
                        Description = "KrustWB .backdrop file. OS setting file that keeps track of \"Leave Out\".",
                        //Source = ""
                    },
                    new Package
                    {
                        Include = true,
                        //Name = "Env-Archive",
                        Path = "Env-Archive",
                        Category = "KrustWB",
                        Description = "KrustWB system settings files kept in Prefs/Env-Archive",
                        //Source = ""
                    },
                    new Package
                    {
                        Include = true,
                        //Name = "Monitors",
                        Path = "Monitors",
                        Category = "KrustWB",
                        Description = "KrustWB monitors Devs/Monitors",
                        //Source = ""
                    },
                    new Package
                    {
                        Include = true,
                        //Name = "A-Directories",
                        Path = "A-Directories",
                        Category = "KrustWB",
                        Description = "A-Directories including icons",
                        //Source = ""
                    },
                    new Package
                    {
                        Include = true,
                        Path = "KrustWBInstall",
                        Category = "KrustWB",
                        Description = "Scripts to pack and install KrustWB",
                        //Source = ""
                    },
                    #endregion
                    #region A-System
                    new Package
                    {
                        Include = true,
                        Path = "SetPatch_43.6b",
                        Category = "System",
                        Description = "Makes ROM patches in system software",
                        Source = "http://m68k.aminet.net/package/util/boot/SetPatch_43.6b"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "NoClick_1.1",
                        Category = "System",
                        Description = "Disables the clicking of the floppy drives.",
                        Source = "http://aminet.net/package/disk/misc/NoClick"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Installer_44.10",
                        Category = "System",
                        Description = "Installer software",
                        //Source = ""
                    },
                    new Package
                    {
                        Include = false,
                        Path = "InstallerNG_1.5 pre",
                        Category = "System",
                        Description = "Installer software",
                        Source = "http://aminet.net/package/util/sys/InstallerNG"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "CardPatch_1.2",
                        Category = "System",
                        Description = "When a PC Card is plugged in the PCMCIA slot and cnet.device is not run then Amiga system slows."
                                      + " CardPatch patches this \"slow bug\" and other bugs in card.resource. The CardResetCard() function"
                                      + " is patched and each \"new\" card is reseted after it is inserted in the PCMCIA slot.",
                        Source = "http://aminet.net/package/util/boot/CardPatch"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "CardReset_3.0",
                        Category = "System",
                        Description = "CardReset forces a high level on pin 58 of the Amiga PCMCIA slot (reset signal)",
                        Source = "http://aminet.net/package/util/boot/CardReset"
                    },
                    new Package
                    {
                        Include = false,
                        Path = "Borderblank",
                        Category = "System",
                        Description = "This simply blanks the border",
                        Source = "http://aminet.net/package/util/boot/bordblnk"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Borderblank_FromClassicWb",
                        Category = "System",
                        Description = "This simply blanks the border",
                    },
                    new Package
                    {
                        Include = true,
                        Path = "LoadModule_45.15",
                        Category = "System",
                        Description = "LoadModule installs \"resident modules\" in a reset-proof way.",
                        Source = "http://aminet.net/package/util/boot/LoadModule"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "SCSI_43.45p",
                        Category = "System",
                        Description = "Patched scsi.device to enable use of 128 GB or bigger IDE devices",
                        Source = "http://aminet.net/package/driver/media/SCSI4345p"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Fat95_3.18",
                        Category = "System",
                        Description = "a DOS handler to mount and use Win95/98 volumes just as if they were AMIGA volumes.",
                        Source = "http://aminet.net/package/disk/misc/fat95"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Cfd_1.33",
                        Category = "System",
                        Description = "Read and write files from CompactFlash cards",
                        Source = "http://aminet.net/package/driver/media/CFD133"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "AmigaOS ROM Update from OS3.9 BB2",
                        Category = "System",
                        Description = "44.57 AmigaOS ROM Update from OS3.9 BB2",
                    },
                    new Package
                    {
                        Include = true,
                        Path = "AssignWedge_1.5",
                        Category = "System",
                        Description = "Add Mount and Assign options to Amiga requester",
                        Source = "http://aminet.net/package/util/wb/AssignWedge",
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Roadshow demo_1.13",
                        Category = "System",
                        Description = "Amiga TCP/IP stack (demo version)",
                    },
                    new Package
                    {
                        Include = true,
                        Path = "3c589_1.5",
                        Category = "System",
                        Description = "SANA-II network driver for 3Com Etherlink III PC Cards (PCMCIA cards)",
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Cnetdevice_1.9",
                        Category = "System",
                        Description = "PCMCIA (aka PC Card) ethernet card SANA2 driver for Amiga 600 and Amiga 1200 computers",
                    },
                    new Package
                    {
                        Include = false,
                        Path = "scrsh",
                        Category = "System",
                        Description = "Opens Shell in a screen",
                        Source = "http://aminet.net/package/util/cli/ksc_scrsh",
                    },
                    new Package
                    {
                        Include = true,
                        Path = "ShellScreen_1.6",
                        Category = "System",
                        Description = "Opens Shell in a screen",
                        Source = "http://aminet.net/package/util/shell/ShellScr",
                    },
                    new Package
                    {
                        Include = false,
                        Path = "ViNCEd_3.73",
                        Category = "System",
                        Description = "Full screen shell editor",
                        Source = "http://aminet.net/package/util/shell/ShellScr",
                    },
                    new Package
                    {
                        Include = true,
                        Path = "KingCON_1.3",
                        Category = "System",
                        Description = @"A console-handler that optionally replaces the standard console devices. Adds some useful features, such as Filename-completion",
                        Source = "http://aminet.net/package/util/shell/KingCON_1.3"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "FreeWheel_2.2.2",
                        Category = "System",
                        Description = @"A tool to fine-tune your mouse",
                        Source = "http://m68k.aminet.net/package/util/mouse/FreeWheel"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "ToolsDaemon_2.1a",
                        Category = "System",
                        Description = @"ToolsDaemon allows you to run programs simply by selecting a menu item from the menu strip of Workbench",
                        Source = "http://aminet.net/package/util/boot/ToolsDaemon21a"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Info_39.18b",
                        Category = "System",
                        Description = @"ToolsDaemon allows you to run programs simply by selecting a menu item from the menu strip of Workbench",
                        Source = "http://aminet.net/package/util/sys/info"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Dr_2.0",
                        Category = "System",
                        Description = @"Dir replacement",
                        Source = "http://paulkienitz.net/amiga/"
                    },
                    new Package
                    {
                        Include = false,
                        Path = "EvenMore_0.91",
                        Category = "System",
                        Description = @"Text viewer",
                        Source = "http://www.evenmore.co.uk/"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "MuchMore_4.6",
                        Category = "System",
                        Description = @"Text viewer",
                        Source = "http://aminet.net/package/text/show/muchmore46"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "TextView_1.25",
                        Category = "System",
                        Description = @"Text viewer",
                        Source = "http://aminet.net/package/text/show/TextView125"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "PatchRAM_1.11",
                        Category = "System",
                        Description = @"Patches the RAM disk to show the real size occupied",
                        Source = "http://aminet.net/package/util/sys/PatchRAM"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "MUI_3.9-2015R1",
                        Category = "Library",
                        Description = @"Magical User Interface",
                        Source = "https://muidev.de/downloads"
                    },
                    #endregion
                    #region Libraries
                    new Package
                    {
                        Include = false,
                        Path = "ReqTools_38.1210",
                        Category = "Library",
                        Description = @"ReqTools library",
                        Source = "http://aminet.net/package/util/boot/ToolsDaemon21a"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "ReqTools_39.3",
                        Category = "Library",
                        Description = @"ReqTools library",
                        Source = "http://aminet.net/package/util/libs/ReqToolsLib"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "MakeIcon_1.5",
                        Category = "Library",
                        Description = @"Create Icons",
                        Source = "http://aminet.net/package/util/cli/MakeIcon1_5"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "JanoEditor_1.01d",
                        Category = "Library",
                        Description = @"Text editor",
                        Source = "http://aminet.net/package/text/edit/JanoEditor"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "guigfxnofpu_20.0",
                        Category = "Library",
                        Description = @"Application layer for pixel graphics (no fpu version)",
                        Source = "http://aminet.net/package/dev/misc/guigfxlib_nofpu"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "MCCGuigfx_19.2",
                        Category = "Library",
                        Description = @"Guigfx Custom Class for Magic User Interface",
                        Source = "http://aminet.net/package/dev/mui/MCC_Guigfx"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "MCCTextEditor_15.50",
                        Category = "Library",
                        Description = @"TextEditor Custom Class for Magic User Interface",
                        Source = "http://aminet.net/package/dev/mui/MCC_TextEditor-15.50"
                    },
                    new Package
                    {





                        Include = false,
                        //Include = true, // NOPE!!!!







                        Path = "renderlib_40.8",
                        Category = "Library",
                        Description = @"shared library that serves an image processing kernel (re-implementation of render.library in ANSI C)",
                        Source = "http://aminet.net/package/dev/misc/renderlib"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "renderlib_31",
                        Category = "Library",
                        Description = @"shared library that serves an image processing kernel (re-implementation of render.library in ANSI C)",
                        Source = "http://aminet.net/package/dev/misc/renderlib31"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "TransADF_4.0.46",
                        Category = "Library",
                        Description = @"Makes Compressed ADFs",
                        Source = "http://aminet.net/package/disk/misc/TransADF"
                    },
                    new Package
                    {
                        Include = false,
                        Path = "MCP_1.48",
                        Category = "Library",
                        Description = @"Master Control Program. Collect all usual patches for AmigaOS at a time when the development of AmigaOS seemed to have stopped",
                        Source = "http://mcp.a1k.org/indexe.html"
                    },
                    #endregion
                    #region A-Programs
                    new Package
                    {
                        Include = true,
                        Path = "Lha_2.15",
                        Category = "Program",
                        Description = "Lha command line (un)archiving",
                        Source = "http://aminet.net/package/util/arc/lha_68k"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Sha256_1.1",
                        Category = "Program",
                        Description = @"A command line utility to calculate the SHA-256 hashes of a list of files",
                        Source = "http://aminet.net/package/util/cli/sha256"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "spatch_6.51 rel 4",
                        Category = "Program",
                        Description = @"Clone of SAS Binary File Patcher",
                        Source = "http://aminet.net/package/dev/misc/spatch"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "SysInfo_4.0",
                        Category = "Program",
                        Description = @"Util for getting information about the system, like OS and library versions, hardware revisions and stuff",
                        Source = "https://sysinfo.d0.se/"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "SnoopDos_3.8",
                        Category = "Program",
                        Description = @"System and application monitor",
                        Source = "http://aminet.net/package/util/moni/SnoopDos"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "IconZ_1.1",
                        Category = "Program",
                        Description = @"Sorts Workbench icons",
                        Source = "http://aminet.net/package/util/wb/IconZ"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "SortIconsOld_1.0",
                        Category = "Program",
                        Description = @"Sorts Workbench icons",
                        Source = "http://aminet.net/package/util/wb/SortIconsOld"
                    },              
                    new Package
                    {
                        Include = true,
                        Path = "WHDLoad usr_18.3",
                        Category = "Programs",
                        Description = @"WHDLoad",
                        Source = "http://whdload.de/"
                    },              
                    new Package
                    {
                        Include = true,
                        Path = "iGame_1.5",
                        Category = "Programs",
                        Description = @"Frontend to launching WHDLoad games",
                        Source = "http://aminet.net/package/util/misc/iGame"
                    },
                    #endregion
                    #region A-Games     
                    new Package
                    {
                        Include = false,
                        Path = "WHDGames",
                        Category = "Games",
                        Description = @"WHDGames",
                        Source = "http://aminet.net/package/util/moni/SnoopDos"
                    },
                    #endregion
                    #region Dev
                    new Package
                    {
                        Include = true,
                        Path = "NDK_3.9",
                        Category = "Dev",
                        Description = @"Amiga OS 3.9 NDK (Native Development Kit)",
                        Source = "http://www.haage-partner.de/download/AmigaOS/NDK39.lha"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "vbcc-bin-amigaos68k_0.906",
                        Category = "Dev",
                        Description = @"Highly optimizing portable and retargetable ISO C compiler. It supports ISO C according to ISO/IEC 9899:1989 and a subset of the new standard ISO/IEC 9899:1999 (C99).",
                        Source = "http://sun.hasenbraten.de/vbcc/"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "vbcc-target-m68k-amigaos_0.906",
                        Category = "Dev",
                        Description = @"VBCC Compiler target AmigaOS 2.x/3.x M680x0.",
                        Source = "http://sun.hasenbraten.de/vbcc/"
                    },
                    new Package
                    {
                        Include = false,
                        Path = "AsmOne_1.25",
                        Category = "Dev",
                        Description = @"(seems like 1.29 actually) 68k/PPC Assembler Development Environment for the Amiga",
                        Source = "From My old Amiga"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "AsmOne_1.29",
                        Category = "Dev",
                        Description = @"68k/PPC Assembler Development Environment for the Amiga",
                        Source = "From old floppies"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "AsmOne_1.48",
                        Category = "Dev",
                        Description = @"68k/PPC Assembler Development Environment for the Amiga",
                        Source = "http://aminet.net/package/dev/asm/ASM-One"
                    },
                    #endregion
                }
        };

        // @formatter:on

        public static Config GetConfig(string location, string configFileName)
        {
            var config = TestConfig;
            
            //var configFilePath = Path.Combine(location, configFileName);
            
            //// Serialize
            //var configString = JsonConvert.SerializeObject(config, Formatting.Indented);
            //File.WriteAllText(configFilePath, configString, Encoding.UTF8);

            //// Deserialize
            //var configString = File.ReadAllText(configFilePath);
            //config = JsonConvert.DeserializeObject<Config>(configString);

            return config;
        }
    }
}