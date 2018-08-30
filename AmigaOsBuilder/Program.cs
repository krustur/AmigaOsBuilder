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
        private static IDictionary<string, string> _aliasToOutputMap = new Dictionary<string, string>
        {
            { @"__systemdrive__",        @"System" },
            { @"__c__",                  @"System\C" },
            { @"__s__",                  @"System\S" },
            { @"__devs__",               @"System\Devs" },
            { @"__utils__",              @"System\A-Utils" },
            { @"__guides__",             @"System\A-Guides" },
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

            foreach (var package in config.Packages)
            {
                var packageBasePath = Path.Combine(sourceBasePath, package.Path, "content");
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
                        Console.WriteLine($"{packageEntry} => {fileOutputPath}");
                        var packageEntryFileInfo = new FileInfo(packageEntry);
                        if ((packageEntryFileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            // Add dir sync
                        }
                        else
                        {
                            // Add file sync
                        }
                    }
                }
            }

            Console.WriteLine("Building sync list done!");
            Console.WriteLine();

            Console.WriteLine();
            Console.WriteLine("Synchronizing in progress ...");
            Console.WriteLine("Synchronizing done!");
            Console.WriteLine();




        }

        private static string TargetAliasToOutputPath(string targetAlias)
        {
            foreach (var mapKey in _aliasToOutputMap.Keys)
            {
                targetAlias = targetAlias.Replace(mapKey, _aliasToOutputMap[mapKey]);
            }

            if (targetAlias.Contains("__"))
            {
                throw new Exception($"Couldn't map target alias {targetAlias}!");
            }

            return targetAlias;          
        }

        private static Config GetConfig()
        {
            var config = new Config
            {
                Packages =
                    new List<Package>
                    {
                        new Package
                        {
                            Include = true,
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
