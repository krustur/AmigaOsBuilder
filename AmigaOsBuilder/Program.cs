using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace AmigaOsBuilder
{
    class Program
    {
        // @formatter:off
        private static readonly IDictionary<string, string> AliasToOutputMap = new Dictionary<string, string>
        {
            { @"__systemdrive__",        @"System" },
            { @"__c__",                  @"System\C" },
            { @"__s__",                  @"System\S" },
            { @"__devs__",               @"System\Devs" },
            { @"__utils__",              @"System\A-Utils" },
            { @"__guides__",             @"System\A-Guides" },
        };
        // @formatter:on

        // @formatter:off
        private static readonly Config TestConfig = new Config
        {
            Packages =
                new List<Package>
                {
                    new Package
                    {
                        Include = false,
                        Name = "Workbench 3.1 (Clean Install)",
                        Path = "Workbench (clean install)_3.1",
                        Category = "OS",
                        Description = "Workbench 3.1 operation system (clean Install)"
                    },
                    new Package
                    {
                        Include = true,
                        Name = "Lha 2.15",
                        Path = "Lha_2.15",
                        Category = "Util",
                        Description = "Amiga LhA 2.15"
                    },
                }
        };
        // @formatter:on

        static void Main(string[] args)
        {
            var location = @"E:\Amiga\KrustWB3";
            var sourceBasePath = @"E:\Amiga\KrustWB3\Source";
            var outputBasePath = @"E:\Amiga\KrustWB3\Output";
            var configFile = @"config.json";

            BuildIt(location, sourceBasePath, outputBasePath, configFile);
        }

        private static void BuildIt(string location, string sourceBasePath, string outputBasePath, string configFileName)
        {
            var config = GetConfig();

            Console.WriteLine();
            Console.WriteLine("Building sync list progress ...");

            var syncList = new List<Sync>();

            AddContentToSyncList(sourceBasePath, outputBasePath, config, "content", SyncType.SourceToTarget, syncList);
            AddContentToSyncList(sourceBasePath, outputBasePath, config, "content_reverse", SyncType.TargetToSource, syncList);

            var outputEntries = Directory.GetFileSystemEntries(outputBasePath, "*", SearchOption.AllDirectories);
            foreach (var outputEntry in outputEntries)
            {
                if (syncList.Any(x => outputEntry.ToLowerInvariant() == x.TargetPath.ToLowerInvariant()) == false)
                {
                    syncList.Add(new Sync
                    {
                        SyncType = SyncType.DeleteTarget,
                        FileType = GetFileType(outputEntry),
                        TargetPath = outputEntry
                    });
                }
                
            }

            Console.WriteLine("Building sync list done!");
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Synchronizing in progress ...");
            Console.WriteLine("Synchronizing done!");
            Console.WriteLine();
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
                        //var filePath = Path.Combine(sourcePath, packageFileSystemEntry);
                        var packageSubPath = RemoveRoot(sourcePath, packageEntry);
                        var fileOutputPath = Path.Combine(packageOutputPath, packageSubPath);
                        //Console.WriteLine($"{packageEntry} => {fileOutputPath}");
                        var sync = new Sync
                        {
                            SourcePath = packageEntry,
                            TargetPath = fileOutputPath,
                            SyncType = syncType,
                            FileType = GetFileType(packageEntry)
                        };
                        
                        syncList.Add(sync);
                    }
                }
            }
        }

        private static FileType GetFileType(string packageEntry)
        {
            var packageEntryFileInfo = new FileInfo(packageEntry);
            var fileType = (packageEntryFileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory
                ? FileType.Directory
                : FileType.File;

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

        private static Config GetConfig()
        {
            var config = TestConfig;

            //var configFilePath = Path.Combine(location, configFileName);
            //// Deserialize
            //var configString = File.ReadAllText(configFilePath);
            //arne = JsonConvert.DeserializeObject<Config>(configString);
            //// Serialize
            //var configString = JsonConvert.SerializeObject(arne);
            //<IList<Package>>();

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
            //Console.WriteLine($"{packageEntry} => {fileOutputPath}");
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
        Directory,
        File
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
    }
}
