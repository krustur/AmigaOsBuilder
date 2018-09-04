using System;
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
            { @"__systemdrive__",        @"System" },
            { @"__c__",                  @"System\C" },
            { @"__s__",                  @"System\S" },
            { @"__l__",                  @"System\L" },
            { @"__devs__",               @"System\Devs" },
            { @"__prefs__",              @"System\Prefs" },
            { @"__utils__",              @"System\A-Utils" },
            { @"__guides__",             @"System\A-Guides" },
            { @"__system__",             @"System\A-System" },
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
                        Name = "Workbench 3.1 (Clean Install)",
                        Path = "Workbench (clean install)_3.1",
                        Category = "OS",
                        Description = "Workbench 3.1 operation system (clean Install)",
                        Url = ""
                    },
                    #endregion
                    #region KrustWB
                    new Package
                    {
                        Include = true,
                        Name = "Startup-Sequence",
                        Path = "Startup-Sequence",
                        Category = "KrustWB",
                        Description = "KrustWB startup-sequence and user-startup files",
                        Url = ""
                    },
                    new Package
                    {
                        Include = true,
                        Name = "Backdrop",
                        Path = "Backdrop",
                        Category = "KrustWB",
                        Description = "KrustWB .backdrop file. OS setting file that keeps track of \"Leave Out\".",
                        Url = ""
                    },
                    new Package
                    {
                        Include = true,
                        Name = "Env-Archive",
                        Path = "Env-Archive",
                        Category = "KrustWB",
                        Description = "KrustWB system settings files kept in Prefs/Env-Archive",
                        Url = ""
                    },
                    new Package
                    {
                        Include = true,
                        Name = "Monitors",
                        Path = "Monitors",
                        Category = "KrustWB",
                        Description = "KrustWB monitors Devs/Monitors",
                        Url = ""
                    },
                    new Package
                    {
                        Include = true,
                        Name = "A-Directories",
                        Path = "A-Directories",
                        Category = "KrustWB",
                        Description = "A-Directories including icons",
                        Url = ""
                    },
                    #endregion
                    #region System
                    new Package
                    {
                        Include = true,
                        Name = "SetPatch 43.6b",
                        Path = "SetPatch_43.6b",
                        Category = "System",
                        Description = "SetPatch 43.6b",
                        Url = "http://m68k.aminet.net/package/util/boot/SetPatch_43.6b"
                    },
                    #endregion
                    #region Util
                    new Package
                    {
                        Include = true,
                        Name = "Lha 2.15",
                        Path = "Lha_2.15",
                        Category = "Util",
                        Description = "Amiga LhA 2.15",
                        Url = "http://aminet.net/package/util/arc/lha_68k"
                    },
                    new Package
                    {
                        Include = true,
                        Name = "KingCON 1.3",
                        Path = "KingCON_1.3",
                        Category = "Util",
                        Description = "KingCON 1.3",
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
                .WriteTo.Console()
                .WriteTo.File("AmigaOsBuilder_log.txt")
                .CreateLogger();

            var location = configuration["Location"];
            var sourceBasePath = configuration["SourceBasePath"];
            var outputBasePath = configuration["OutputBasePath"];
            var configFile = configuration["ConfigFile"];
            var syncMode = Enum.Parse<SyncMode>(configuration["SyncMode"]);

            BuildIt(location, sourceBasePath, outputBasePath, configFile, syncMode);
            _logger.Information("Press any key!");
            Console.ReadKey();
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

            /*
            var distinctSyncList = syncList
                .GroupBy(x => new {x.FileType, x.SyncType, x.TargetPath, x.SourcePath})
                .Select(x => x.First())
                .ToList();
            //var xxx = from s in distinctSyncList
            //          group s by s.TargetPath into sg
            //    select new
            //    {
            //        TargetPath = sg.Key,
            //        Count = sg.Count(),
            //    };
            //var yyy = distinctSyncList
            //    .GroupBy(x => x.TargetPath)
            //    .Select(x => x.First())
            //    .ToList();
            var zzz = distinctSyncList
                .GroupBy(c => c.TargetPath)
                .Where(grp => grp.Count() > 1)
                .Select(grp => grp.Key)
                .ToList();
            var cnt = 0;
            foreach (var z in zzz)
            {

                //var one = zzz[1];
                var xxx = distinctSyncList
                    .Where(x => x.TargetPath == z)
                    .ToList();
                cnt += xxx.Count;
            }
            */
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

                var fileType = syncListForTarget.First().FileType;
                _logger.Information("{TargetPath} [{FileType}]", targetPath, fileType);

                foreach (var sync in syncListForTarget)
                {
                    _logger.Information(": {SourcePath} [{SyncType}]", sync.SourcePath, sync.SyncType);

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
        }

        private static void Synchronize(IList<Sync> syncList)
        { 
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
                    case FileType.DirectoryRecursive:
                        SynchronizeDirectoryRecursive(sync);
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
                    _logger.Information($@"Copy Source to Target: [{sync.SourcePath}] => [{sync.TargetPath}]");
                    File.Copy(sync.SourcePath, sync.TargetPath, overwrite: true);
                    break;
                case SyncType.TargetToSource:
                    if (File.Exists(sync.TargetPath))
                    {
                        _logger.Information($@"Copy Target to Source: [{sync.SourcePath}] <= [{sync.TargetPath}]");
                        File.Copy(sync.TargetPath, sync.SourcePath, overwrite: true);
                    }
                    else
                    {
                        _logger.Information($@"Copy Target to Source (target is missing!): [{sync.SourcePath}] <= [{sync.TargetPath}]");
                    }

                    break;
                case SyncType.DeleteTarget:
                    if (File.Exists(sync.TargetPath))
                    {
                        _logger.Information($@"Delete: [{sync.TargetPath}]");
                        File.Delete(sync.TargetPath);
                    }
                    //else
                        //{
                        //    _logger.Information($@"Delete (already deleted): [{sync.TargetPath}]");
                        //}
                    break;
                case SyncType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void SynchronizeDirectory(Sync sync)
        {
            switch (sync.SyncType)
            {
                case SyncType.SourceToTarget:
                    if (Directory.Exists(sync.TargetPath))
                    {
                        //_logger.Information($@"Target Directory (already exists): [{sync.TargetPath}]");
                    }
                    else
                    {
                        _logger.Information($@"Create Target Directory: [{sync.TargetPath}]");
                        Directory.CreateDirectory(sync.TargetPath);
                    }

                    break;
                case SyncType.TargetToSource:
                    if (Directory.Exists(sync.SourcePath))
                    {
                        //_logger.Information($@"Source Directory (already exists): [{sync.SourcePath}]");
                    }
                    else
                    {
                        _logger.Information($@"Create Source Directory: [{sync.SourcePath}]");
                        Directory.CreateDirectory(sync.SourcePath);
                    }

                    break;
                case SyncType.DeleteTarget:
                    if (Directory.Exists(sync.TargetPath))
                    {
                        _logger.Information($@"Delete: [{sync.TargetPath}]");
                        Directory.Delete(sync.TargetPath, recursive: true);
                    }
                    //else
                    //{
                    //    _logger.Information($@"Delete (already deleted): [{sync.TargetPath}]");
                    //}

                    break;
                case SyncType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void SynchronizeDirectoryRecursive(Sync sync)
        {
            switch (sync.SyncType)
            {
                   
                case SyncType.TargetToSource:
                    var recurseEntries = Directory.GetFileSystemEntries(sync.TargetPath, "*", SearchOption.AllDirectories);
                    //if (Directory.Exists(sync.SourcePath))
                    //{
                    //    //_logger.Information($@"Source Directory (already exists): [{sync.SourcePath}]");
                    //}
                    //else
                    //{
                    //    _logger.Information($@"Create Source Directory: [{sync.SourcePath}]");
                    //    Directory.CreateDirectory(sync.SourcePath);
                    //}

                    break;
                case SyncType.SourceToTarget:
                case SyncType.DeleteTarget:
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

    public class Sync
    {
        public SyncType SyncType { get; set; }
        public FileType FileType { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }

        public override string ToString()
        {
            //_logger.Information($"{packageEntry} => {fileOutputPath}");
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
        DirectoryRecursive
    }

    public class Config
    {
        public IList<Package> Packages { get; set; }

    }
    public class Package
    {
        public bool Include { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
    }
}
