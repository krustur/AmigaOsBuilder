﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;

namespace AmigaOsBuilder
{
    //TODO: Warn for existing Sources that is missing in Config

    class Program
    {
        public enum SyncMode
        {
            Unknown = 0,
            Forward,
            Reverse,
            Synchronize
        }
        // @formatter:off
        private static readonly IDictionary<string, string> AliasToOutputMap = new Dictionary<string, string>
        {
            // System drive
            { @"__systemdrive__",        @"System" },

            // Amiga OS folders
            { @"__c__",                  @"System\C" },
            { @"__devs__",               @"System\Devs" },
            { @"__l__",                  @"System\L" },
            { @"__libs__",               @"System\Libs" },
            { @"__prefs__",              @"System\Prefs" },
            { @"__s__",                  @"System\S" },
            { @"__system__",             @"System\System" },

            // KrustWB folders
            { @"__autils__",             @"System\A-Utils" },
            { @"__aguides__",            @"System\A-Guides" },
            { @"__asystem__",            @"System\A-System" },
        };
        // @formatter:on

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
                        //Url = ""
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
                        //Url = ""
                    },
                    new Package
                    {
                        Include = true,
                        //Name = "Backdrop",
                        Path = "Backdrop",
                        Category = "KrustWB",
                        Description = "KrustWB .backdrop file. OS setting file that keeps track of \"Leave Out\".",
                        //Url = ""
                    },
                    new Package
                    {
                        Include = true,
                        //Name = "Env-Archive",
                        Path = "Env-Archive",
                        Category = "KrustWB",
                        Description = "KrustWB system settings files kept in Prefs/Env-Archive",
                        //Url = ""
                    },
                    new Package
                    {
                        Include = true,
                        //Name = "Monitors",
                        Path = "Monitors",
                        Category = "KrustWB",
                        Description = "KrustWB monitors Devs/Monitors",
                        //Url = ""
                    },
                    new Package
                    {
                        Include = true,
                        //Name = "A-Directories",
                        Path = "A-Directories",
                        Category = "KrustWB",
                        Description = "A-Directories including icons",
                        //Url = ""
                    },
                    #endregion
                    #region A-System
                    new Package
                    {
                        Include = true,
                        Path = "SetPatch_43.6b",
                        Category = "System",
                        Description = "Makes ROM patches in system software",
                        Url = "http://m68k.aminet.net/package/util/boot/SetPatch_43.6b"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "NoClick_1.1",
                        Category = "System",
                        Description = "Disables the clicking of the floppy drives.",
                        Url = "http://aminet.net/package/disk/misc/NoClick"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "Installer_44.10",
                        Category = "System",
                        Description = "Installer software",
                        //Url = ""
                    },
                    new Package
                    {
                        Include = false,
                        Path = "InstallerNG_1.5 pre",
                        Category = "System",
                        Description = "Installer software",
                        Url = "http://aminet.net/package/util/sys/InstallerNG"
                    },
                    #endregion
                    #region A-Util
                    new Package
                    {
                        Include = true,
                        Path = "Lha_2.15",
                        Category = "Util",
                        Description = "Lha command line (un)archiving",
                        Url = "http://aminet.net/package/util/arc/lha_68k"
                    },
                    new Package
                    {
                        Include = true,
                        Path = "KingCON_1.3",
                        Category = "Util",
                        Description = @"A console-handler that optionally replaces the standard console devices. Adds some useful features, such as Filename-completion",
                        Url = "http://aminet.net/package/util/shell/KingCON_1.3"
                    },
                    #endregion
                }
        };

        private static Logger _logger;
        // @formatter:on

        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddCommandLine(args)
                .Build();

            _logger = new LoggerConfiguration()
                //.Destructure.ByTransforming<Sync>(
                    //r => new { r.SyncType, r.SourcePath, r.TargetPath, r.FileType})
                //.MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("AmigaOsBuilder_log.txt")
                .CreateLogger();

            var location = configuration["Location"];
            var sourceBasePath = configuration["SourceBasePath"];
            var outputBasePath = configuration["OutputBasePath"];
            var configFile = configuration["ConfigFile"];
            var syncMode = Enum.Parse<SyncMode>(configuration["SyncMode"]);

            BuildIt(location, sourceBasePath, outputBasePath, configFile, syncMode);
            _logger.Information("Press enter!");
            Console.ReadLine();
        }

        private static void BuildIt(string location, string sourceBasePath, string outputBasePath, string configFileName, SyncMode syncMode)
        {
            var config = GetConfig(location, configFileName);

            CreateOutputAliasDirectories(outputBasePath);

            var syncList = BuildSyncList(sourceBasePath, outputBasePath, syncMode, config);

            if (syncMode != SyncMode.Synchronize)
            {
                Synchronize(syncList);
            }
            else
            {
                SynchronizeV2(syncList);
            }
        }

        private static void CreateOutputAliasDirectories(string outputBasePath)
        {
            _logger.Information("Creating output alias directories ...");
            Directory.CreateDirectory(outputBasePath);
            foreach (var map in AliasToOutputMap)
            {
                var outputAliasPath = Path.Combine(outputBasePath, map.Value);
                _logger.Information($@"[{map.Key}] = [{outputAliasPath}]");
                Directory.CreateDirectory(outputAliasPath);
            }

            _logger.Information("Create output alias directories done!");
        }

        private static List<Sync> BuildSyncList(string sourceBasePath, string outputBasePath, SyncMode syncMode, Config config)
        {
            _logger.Information("Building sync list ...");
            _logger.Information($"SyncMode  [{syncMode}]");

            var syncList = new List<Sync>();

            switch (syncMode)
            {
                case SyncMode.Forward:
                {
                    AddContentToSyncList(sourceBasePath, outputBasePath, config, "content", SyncType.SourceToTarget, syncList);
                    AddContentToSyncList(sourceBasePath, outputBasePath, config, "content_reverse", SyncType.SourceToTarget, syncList);
                    AddDeleteToSyncList(outputBasePath, syncList);
                    break;
                }
                case SyncMode.Reverse:
                {
                    AddContentToSyncList(sourceBasePath, outputBasePath, config, "content_reverse", SyncType.TargetToSource, syncList);
                    break;
                }
                case SyncMode.Synchronize:
                {
                    AddContentToSyncList(sourceBasePath, outputBasePath, config, "content", SyncType.SourceToTarget, syncList);
                    //AddContentToSyncList(sourceBasePath, outputBasePath, config, "content_reverse", SyncType.SourceToTarget, syncList);
                    AddDeleteToSyncList(outputBasePath, syncList);
                    AddContentToSyncList(sourceBasePath, outputBasePath, config, "content_reverse", SyncType.TargetToSource, syncList);
                    break;
                }
            }

            _logger.Information("Building sync list done!");

            return syncList;
        }

        private static void AddContentToSyncList(string sourceBasePath, string outputBasePath, Config config,
            string contentFolderName, SyncType syncType, List<Sync> syncList)
        {
            foreach (var package in config.Packages)
            {
                if (package.Include == false)
                {
                    continue;
                }

                var packageBasePath = Path.Combine(sourceBasePath, package.Path, contentFolderName);
                if (Directory.Exists(packageBasePath) == false)
                {
                    continue;
                }

                var packageTargets = Directory.GetDirectories(packageBasePath);
                foreach (var sourcePath in packageTargets)
                {
                    //var sourcePath = packageTarget.FullName;
                    var dirInfo = new DirectoryInfo(sourcePath);
                    var targetAlias = dirInfo.Name;

                    //#Write-Output -Verbose $sourcePath
                    //#Write-Output -Verbose $targetAlias
                    var packageOutputPath = TargetAliasToOutputPath(targetAlias);
                    packageOutputPath = Path.Combine(outputBasePath, packageOutputPath);
                    //#Write-Output -Verbose $outputPath
                    //#Write-Output -Verbose "$sourcePath => $outputPath"

                    var packageEntries = Directory.GetFileSystemEntries(sourcePath, "*", SearchOption.AllDirectories);
                    foreach (var packageEntry in packageEntries)
                    {
                        var packageEntryFileName = Path.GetFileName(packageEntry);
                        if (ShouldContentReverseAll(packageEntryFileName))
                        {
                            var contentReversePackageEntryPath = Path.GetDirectoryName(packageEntry);
                            var contentReversePackageSubPath = RemoveRoot(sourcePath, contentReversePackageEntryPath);
                            var contentReverseFileOutputPath = Path.Combine(packageOutputPath, contentReversePackageSubPath);
                            if (Directory.Exists(contentReverseFileOutputPath))
                            {
                                var innerContentReversePackageEntries = Directory.GetFileSystemEntries(contentReverseFileOutputPath, "*", SearchOption.AllDirectories);
                                foreach (var innerContentReversePackageEntry in innerContentReversePackageEntries)
                                {
                                    var innerContentReversePackageSubPath = RemoveRoot(packageOutputPath, innerContentReversePackageEntry);
                                    var innerContentReverseSourcePath = Path.Combine(sourcePath, innerContentReversePackageSubPath);

                                    var innerSync = new Sync
                                    {
                                        SourcePath = innerContentReverseSourcePath,
                                        TargetPath = innerContentReversePackageEntry,
                                        SyncType = syncType,
                                        FileType = GetFileType(syncType, innerContentReversePackageEntry)
                                    };

                                    syncList.Add(innerSync);
                                }
                            }
                        }
                        else
                        {
                            var packageSubPath = RemoveRoot(sourcePath, packageEntry);
                            var fileOutputPath = Path.Combine(packageOutputPath, packageSubPath);
                            //_logger.Information($"{packageEntry} => {fileOutputPath}");
                            var sync = new Sync
                            {
                                SourcePath = packageEntry,
                                TargetPath = fileOutputPath,
                                SyncType = syncType,
                                FileType = GetFileType(syncType, packageEntry)
                            };

                            syncList.Add(sync);
                        }
                    }
                }
            }
        }

        private static bool ShouldContentReverseAll(string packageEntryFileName)
        {
            if (packageEntryFileName.ToLowerInvariant() == "content_reverse_all")
            {
                return true;
            }

            return false;
        }

        private static void AddDeleteToSyncList(string outputBasePath, List<Sync> syncList)
        {
            var outputEntries = Directory.GetFileSystemEntries(outputBasePath, "*", SearchOption.AllDirectories);
            foreach (var outputEntry in outputEntries)
            {
                //if (syncList.Any(x => outputEntry.ToLowerInvariant() == x.TargetPath.ToLowerInvariant()) == false)
                if (syncList.Any(x => x.TargetPath.ToLowerInvariant().StartsWith(outputEntry.ToLowerInvariant())) == false)
                {
                    syncList.Add(new Sync
                    {
                        SyncType = SyncType.DeleteTarget,
                        FileType = GetFileType(SyncType.DeleteTarget, outputEntry),
                        TargetPath = outputEntry
                    });
                }
            }
        }

        private static void SynchronizeV2(IList<Sync> syncList)
        {
            _logger.Information("Synchronizing ...");
           
            var targetPaths = syncList
                .Select(x => x.TargetPath)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // Source Paths check if pure paranoia! Shouldn't be needed.
            var sourcePaths = syncList
                .Select(x => x.SourcePath)
                .Distinct()
                .ToList();

            foreach (var targetPath in targetPaths)
            {
                var syncListForTarget = syncList
                    .Where(x => x.TargetPath == targetPath)
                    .ToList();
                if (syncListForTarget.Count(x => x.FileType == FileType.Directory) > 0 &&
                    syncListForTarget.Count(x => x.FileType == FileType.File) > 0)
                {
                    throw new Exception("syncListForTarget contains both FileType.Directory and FileType.File!");
                }

                var syncCount = syncListForTarget.Count;
                var syncSkipList = syncListForTarget.GetRange(0, syncCount - 1);
                foreach (var sync in syncSkipList)
                {
                    _logger.Debug("Sync (skipped) {Sync}", sync);
                }

                var actualSync = syncListForTarget.Last();
                _logger.Debug("Sync {Sync})", actualSync);
                switch (actualSync.FileType)
                {
                    case FileType.File:
                        SynchronizeFile(actualSync);
                        break;
                    case FileType.Directory:
                        SynchronizeDirectory(actualSync);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var syncListForTargetSourcePaths = syncListForTarget
                    .Select(x => x.SourcePath)
                    .ToList();
                sourcePaths.RemoveAll(x => syncListForTargetSourcePaths.Contains(x));
            }

            // Source Paths check if pure paranoia! Shouldn't be needed.
            if (sourcePaths.Count > 0)
            {
                throw new Exception("SourcePaths was not synchronized!!");
            }

            _logger.Information("Synchronizing done!");
        }

        private static void Synchronize(IList<Sync> syncList)
        { 
            _logger.Information("Synchronizing ...");

            foreach (var sync in syncList)
            {
                switch (sync.FileType)
                {
                    case FileType.File:
                        SynchronizeFile(sync);
                        break;
                    case FileType.Directory:
                        SynchronizeDirectory(sync);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _logger.Information("Synchronizing done!");
        }

        private static void SynchronizeFile(Sync sync)
        {
            switch (sync.SyncType)
            {
                case SyncType.SourceToTarget:
                {
                    var fileDiff = GetFileDiff(sync);
                    if (fileDiff == FileDiff.Equal)
                    {
                        _logger.Debug(@"Copy Source to Target (files are equal!): [{SourcePath}] => [{TargetPath}]", sync.SourcePath, sync.TargetPath);
                    }
                    else
                    {
                        if (fileDiff == FileDiff.DiffTargetMissing || fileDiff == FileDiff.DiffSourceNewer)
                        {
                            _logger.Information(@"Copy Source to Target: [{SourcePath}] => [{TargetPath}] [{FileDiff}]", sync.SourcePath, sync.TargetPath, fileDiff);
                        }
                        else
                        {
                            _logger.Warning(@"Copy Source to Target: [{SourcePath}] => [{TargetPath}] [{FileDiff}]", sync.SourcePath, sync.TargetPath, fileDiff);
                        }

                        File.Copy(sync.SourcePath, sync.TargetPath, overwrite: true);
                    }

                    break;
                }
                case SyncType.TargetToSource:
                {
                    var fileDiff = GetFileDiff(sync);
                    {
                        switch (fileDiff)
                        {
                            case FileDiff.Equal:
                            {
                                _logger.Debug(@"Copy Target to Source (files are equal!): [{SourcePath}] <= [{TargetPath}]", sync.SourcePath, sync.TargetPath);
                                break;
                            }
                            case FileDiff.DiffSourceMissing:
                            case FileDiff.DiffTargetNewer:
                            {
                                _logger.Information(@"Copy Target to Source: [{SourcePath}] <= [{TargetPath}] [{FileDiff}]", sync.SourcePath, sync.TargetPath, fileDiff);
                                File.Copy(sync.TargetPath, sync.SourcePath, overwrite: true);
                                break;
                            }
                            case FileDiff.DiffTargetMissing:
                            {
                                _logger.Information(@"Copy Target to Source (invserse): [{SourcePath}] <= [{TargetPath}] [{FileDiff}]", sync.SourcePath, sync.TargetPath, fileDiff);
                                File.Copy(sync.SourcePath, sync.TargetPath, overwrite: true);
                                break;
                            }
                            case FileDiff.DiffContent:
                            case FileDiff.DiffSourceNewer:
                            default:
                            {
                                _logger.Warning(@"Copy Target to Source: [{SourcePath}] <= [{TargetPath}] [{FileDiff}]", sync.SourcePath, sync.TargetPath, fileDiff);
                                File.Copy(sync.TargetPath, sync.SourcePath, overwrite: true);
                                break;
                            }
                        }

                    }
                    break;
                }
                case SyncType.DeleteTarget:
                {
                    if (File.Exists(sync.TargetPath))
                    {
                        _logger.Information(@"Delete Target: [{TargetPath}]", sync.TargetPath);
                        File.Delete(sync.TargetPath);
                    }
                    else
                    {
                        _logger.Debug(@"Delete Target (nothing to delete!): [{TargetPath}]", sync.TargetPath);
                    }
                    break;
                }
                case SyncType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static FileDiff GetFileDiff(Sync sync)
        {
            var sourceInfo = new FileInfo(sync.SourcePath);
            var targetInfo = new FileInfo(sync.TargetPath);

            if (sourceInfo.Exists == true && targetInfo.Exists == false)
            {
                return FileDiff.DiffTargetMissing;
            }

            if (sourceInfo.Exists == false && targetInfo.Exists == true)
            {
                return FileDiff.DiffSourceMissing;
            }

            if (FileComparer.FilesContentsAreEqual(sourceInfo, targetInfo))
            {
                return FileDiff.Equal;
            }

            if (sourceInfo.LastWriteTimeUtc < targetInfo.LastWriteTimeUtc)
            {
                return FileDiff.DiffTargetNewer;
            }

            if (targetInfo.LastWriteTimeUtc < sourceInfo.LastWriteTimeUtc)
            {
                return FileDiff.DiffSourceNewer;
            }

            return FileDiff.DiffContent;
        }

        private static void SynchronizeDirectory(Sync sync)
        {
            switch (sync.SyncType)
            {
                case SyncType.SourceToTarget:
                    if (Directory.Exists(sync.TargetPath))
                    {
                        _logger.Debug(@"Create Target Directory (already exists): [{TargetPath}]", sync.TargetPath);
                    }
                    else
                    {
                        _logger.Information(@"Create Target Directory: [{TargetPath}]", sync.TargetPath);
                        Directory.CreateDirectory(sync.TargetPath);
                    }

                    break;
                case SyncType.TargetToSource:
                    if (Directory.Exists(sync.SourcePath))
                    {
                        _logger.Debug(@"Create Source Directory (already exists): [{SourcePath}]", sync.SourcePath);
                    }
                    else
                    {
                        _logger.Information(@"Create Source Directory: [{SourcePath}]", sync.SourcePath);
                        Directory.CreateDirectory(sync.SourcePath);
                    }

                    if (Directory.Exists(sync.TargetPath) == false)
                    { 
                        _logger.Information(@"Create Target Directory: [{TargetPath}]", sync.TargetPath);
                        Directory.CreateDirectory(sync.TargetPath);
                    }

                    break;
                case SyncType.DeleteTarget:
                    if (Directory.Exists(sync.TargetPath))
                    {
                        _logger.Information(@"Delete Target Directory: [{TargetPath}]", sync.TargetPath);
                        Directory.Delete(sync.TargetPath, recursive: true);
                    }
                    else
                    {
                        _logger.Debug(@"Delete Target Directory (already deleted): [{TargetPath}]", sync.TargetPath);
                    }

                    break;
                case SyncType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static FileType GetFileType(SyncType syncType, string packageEntry)
        {
            var packageEntryFileInfo = new FileInfo(packageEntry);
            var fileType = (packageEntryFileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory ? FileType.Directory : FileType.File;

            return fileType;
        }

        private static string TargetAliasToOutputPath(string targetAlias)
        {
            foreach (var mapKey in AliasToOutputMap.Keys)
            {
                targetAlias = targetAlias.Replace(mapKey, AliasToOutputMap[mapKey]);
            }

            if (targetAlias.Contains("__"))
            {
                throw new Exception($"Couldn't map target alias {targetAlias}!");
            }

            return targetAlias;          
        }

        private static Config GetConfig(string location, string configFileName)
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

        public static string RemoveRoot(string root, string target)
        {
            ////format the paths first
            //var p = String.Join("\\", (from t in target.Split(("\\").ToCharArray()) where t != "" select t).ToArray());
            //var r = String.Join("\\", (from t in root.Split(("\\").ToCharArray()) where t != "" select t).ToArray());
            //if (p.StartsWith(r))
            //{
            //    return p.Remove(0, r.Length);
            //}
            //return target;
            if (target.ToLowerInvariant().StartsWith(root.ToLowerInvariant()))
            {
                target = target.Substring(root.Length);
                if (target.StartsWith('\\'))
                {
                    target = target.Substring(1);
                }
            }

            return target;
        }
    }

    internal enum FileDiff
    {
        Unknown = 0,
        Equal,
        DiffTargetMissing,
        DiffSourceMissing,
        DiffTargetNewer,
        DiffSourceNewer,
        DiffContent,
    }

    public class Sync
    {
        public SyncType SyncType { get; set; }
        public FileType FileType { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }

        public override string ToString()
        {
            switch (SyncType)
            {
                case SyncType.SourceToTarget:
                    {
                        return $@"{SyncType} ({FileType}): {SourcePath} => {TargetPath}";
                    }
                case SyncType.TargetToSource:
                    {
                        return $@"{SyncType} ({FileType}): {SourcePath} <= {TargetPath}";
                    }
                case SyncType.DeleteTarget:
                    {
                        return $@"{SyncType} ({FileType}): {TargetPath}";
                    }
            }
            return $"i am error (unknown SyncType)";
        }
    }

    public enum SyncType
    {
        Unknown = 0,
        SourceToTarget,
        TargetToSource,
        DeleteTarget
    }

    public enum FileType
    {
        Unknown = 0,
        File,
        Directory,
    }

    public class Config
    {
        public IList<Package> Packages { get; set; }

    }
    public class Package
    {
        public bool Include { get; set; }
        public string Path { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }
}
