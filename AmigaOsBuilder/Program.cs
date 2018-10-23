using System;
using System.Collections.Generic;
using System.IO;
//using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace AmigaOsBuilder
{
    //TODO: Warn for existing Sources that is missing in Config

    public class Program
    {
        private static Logger _logger;
        private static IPathService _pathService;
        private static readonly StringBuilder UserStartupBuilder = new StringBuilder(32727);

        static void Main(string[] args)
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddCommandLine(args)
                    .Build();

                _logger = new LoggerConfiguration()
                    //.MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File("AmigaOsBuilder_log.txt")
                    .CreateLogger();

                _pathService = new PathService();

                BuildIt(new List<Config>
                {
                    ConfigService.SysConfig(),
                    //ConfigService.WorkConfig(),
                    ConfigService.DevConfig()
                });
                BuildIt(new List<Config>
                {
                    ConfigService.SysLhaConfig(),
                    //ConfigService.WorkLhaConfig(),
                    ConfigService.DevLhaConfig(),
                });
            }
            catch (Exception e)
            {
                Console.WriteLine("");
                Console.WriteLine(e);
            }

            Console.WriteLine("");
            Console.WriteLine("Press enter!");
            Console.ReadLine();
        }

        private static void BuildIt(IList<Config> configs)
        {
            foreach (var config in configs)
            {
                _logger.Information("");
                _logger.Information("Building from {SourcePath} to {TargetPath} ...", config.SourceBasePath, config.OutputBasePath);
                using (var outputFileHandler = FileHandlerFactory.Create(_logger, config.OutputBasePath))
                {
                    AliasService aliasService = new AliasService(config.Aliases);
                    outputFileHandler.CreateBasePaths(aliasService);

                    var syncList = BuildSyncList(outputFileHandler, config, aliasService);
                    if (config.UserStartup)
                    {
                        BuildUserStartup(configs, syncList);
                    }
                    SynchronizeV2(syncList, outputFileHandler);
                }
                _logger.Information("Build done");
            }
        }

        private static IList<Sync> BuildSyncList(IFileHandler outputFileHandler, Config config, AliasService aliasService)
        {
            _logger.Information("Building sync list ...");

            var syncList = new List<Sync>();

            var packages = GetIncludedPackages(config);

            AddContentToSyncList(config.SourceBasePath, outputFileHandler, packages, "content", SyncType.SourceToTarget, syncList, aliasService: aliasService);
            AddDeleteToSyncList(outputFileHandler, syncList);
            if (config.ReverseSync)
            {
                AddContentToSyncList(config.SourceBasePath, outputFileHandler, packages, "content_reverse", SyncType.TargetToSource, syncList, aliasService: aliasService);
            }


            BuildReadme(packages, syncList);

            ProgressBar.ClearProgressBar();

            _logger.Information("Building sync list done!");

            return syncList;
        }

        private static IList<Package> GetIncludedPackages(Config config)
        {
            var packages = config.Packages
                .Where(x => x.Include == true)
                .ToList();

            foreach (var package in packages)
            {                
                var packageBasePath = _pathService.Combine(config.SourceBasePath, package.Path);
                if (Directory.Exists(packageBasePath) == false)
                {
                    throw new Exception($"Package [{package.Path}] base path [{packageBasePath}] is missing, check your configuration.");
                }
            }

            return packages;
        }

        private static void AddContentToSyncList(string sourceBasePath, IFileHandler outputFileHandler, IList<Package> packages,
            string contentFolderName, SyncType syncType, List<Sync> syncList, AliasService aliasService)
        {
            int packageCnt = 0;
            foreach (var package in packages)
            {
                ProgressBar.DrawProgressBar($"AddContentToSyncList [{contentFolderName}]", packageCnt++, packages.Count);
                
                var packageContentFolderBasePath = _pathService.Combine(sourceBasePath, package.Path, contentFolderName);
                var packageContentLhaBasePath = _pathService.Combine(sourceBasePath, package.Path, contentFolderName+".lha");
                if (Directory.Exists(packageContentFolderBasePath) && File.Exists(packageContentLhaBasePath))
                {
                    throw new Exception($"Package [{package.Path}] has both [content] folder and [content.lha].");
                }

                string packageContentBasePath;
                if (File.Exists(packageContentLhaBasePath))
                {
                    packageContentBasePath = packageContentLhaBasePath;
                }
                else if (Directory.Exists(packageContentFolderBasePath))
                {
                    packageContentBasePath = packageContentFolderBasePath;
                }
                else
                {
                    continue;
                }

                using (var contentFileHandler = FileHandlerFactory.Create(_logger, packageContentBasePath))
                {
                    var packageOutputPath = "";
                    var sourcePath = "";
                    var packageEntries = contentFileHandler.DirectoryGetFileSystemEntriesRecursive(sourcePath);
                    foreach (var packageEntry in packageEntries)
                    {
                        var outputPath = aliasService.TargetAliasToOutputPath(packageEntry);

                        var packageEntryFileName = _pathService.GetFileName(outputPath);
                        if (ShouldContentReverseAll(packageEntryFileName))
                        {
                            var contentReversePackageEntryPath = _pathService.GetDirectoryName(outputPath);
                            var contentReversePackageSubPath = RemoveRoot(sourcePath, contentReversePackageEntryPath);
                            var contentReverseFileOutputPath = _pathService.Combine(packageOutputPath, contentReversePackageSubPath);
                            if (outputFileHandler.DirectoryExists(contentReverseFileOutputPath))
                            {
                                var innerContentReversePackageEntries = outputFileHandler.DirectoryGetFileSystemEntriesRecursive(contentReverseFileOutputPath);
                                foreach (var innerContentReversePackageEntry in innerContentReversePackageEntries)
                                {
                                    var innerContentReversePackageSubPath = RemoveRoot(packageOutputPath, innerContentReversePackageEntry);
                                    var innerContentReverseSourcePath = _pathService.Combine(sourcePath, innerContentReversePackageSubPath);

                                    var innerSync = new Sync
                                    {
                                        PackageContentBasePath = packageContentBasePath,
                                        SourcePath = innerContentReverseSourcePath,
                                        TargetPath = innerContentReversePackageEntry,
                                        SyncType = syncType,
                                        FileType = outputFileHandler.GetFileType(innerContentReversePackageEntry)
                                    };

                                    syncList.Add(innerSync);
                                }
                            }
                        }
                        else
                        {
                            var packageSubPath = RemoveRoot(sourcePath, outputPath);
                            var fileOutputPath = _pathService.Combine(packageOutputPath, packageSubPath);
                            var sync = new Sync
                            {
                                PackageContentBasePath = packageContentBasePath,
                                SourcePath = packageEntry,
                                TargetPath = fileOutputPath,
                                SyncType = syncType,
                                FileType = contentFileHandler.GetFileType(packageEntry)
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

        private static void AddDeleteToSyncList(IFileHandler outputFileHandler, List<Sync> syncList)
        {
            var outputEntries = outputFileHandler.DirectoryGetFileSystemEntriesRecursive("");
            var syncListKeys = syncList
                .Select(x => x.TargetPath.ToLowerInvariant())
                .Distinct()
                .ToList();
            var packageCnt = 0;
            foreach (var outputEntry in outputEntries)
            {
                ProgressBar.DrawProgressBar("AddDeleteToSyncList  ", packageCnt++, outputEntries.Count);
                var outputEntryLower = outputEntry.ToLowerInvariant();
                //if (syncList.Any(x => x.TargetPath.ToLowerInvariant().StartsWith(outputEntryLower)) == false)
                if (syncListKeys.Any(x => x.StartsWith(outputEntryLower)) == false)
                {
                    syncList.Add(new Sync
                    {
                        PackageContentBasePath = null,//outputFileHandler.OutputBasePath,
                        SyncType = SyncType.DeleteTarget,
                        FileType = outputFileHandler.GetFileType(outputEntry),
                        TargetPath = outputEntry
                    });
                    syncListKeys.Add(outputEntryLower);
                }
            }
        }

        private static void SynchronizeV2(IList<Sync> syncList, IFileHandler outputFileHandler)
        {
            _logger.Information("Synchronizing ...", outputFileHandler.OutputBasePath);
           
            var targetPaths = syncList
                .Select(x => x.TargetPath.ToLowerInvariant())
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            // Source Paths check is pure paranoia! Shouldn't be needed.
            var sourcePaths = syncList
                .Select(x => x.SourcePath?.ToLowerInvariant())
                .Distinct()
                .ToList();

            var packageCnt = 0;
            foreach (var targetPath in targetPaths)
            {
                ProgressBar.DrawProgressBar("SynchronizeV2", packageCnt++, targetPaths.Count);

                var syncListForTarget = syncList
                    .Where(x => x.TargetPath.ToLowerInvariant() == targetPath)
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
                using (var contentFileHandler = FileHandlerFactory.Create(_logger, actualSync.PackageContentBasePath))
                {
                    switch (actualSync.FileType)
                    {
                        case FileType.File:
                            SynchronizeFile(actualSync, contentFileHandler, outputFileHandler);
                            break;
                        case FileType.Directory:
                            SynchronizeDirectory(actualSync, contentFileHandler, outputFileHandler);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                var syncListForTargetSourcePaths = syncListForTarget
                    .Select(x => x.SourcePath?.ToLowerInvariant())
                    .ToList();
                sourcePaths.RemoveAll(x => syncListForTargetSourcePaths.Contains(x));
            }

            ProgressBar.ClearProgressBar();

            // Source Paths check is pure paranoia! Shouldn't be needed.
            if (sourcePaths.Count > 0)
            {
                throw new Exception("SourcePaths was not synchronized!!");
            }


            _logger.Information("Synchronizing done!");
        }

        private static void BuildReadme(IList<Package> packages, IList<Sync> syncList)
        {
            _logger.Information("Building Readme ...");

            int packageCnt = 0;
            var builder = new StringBuilder(32727);
            var outputPath = "KrustWB3.readme.txt";

            foreach (var package in packages)
            {
                ProgressBar.DrawProgressBar("BuildReadme ", packageCnt++, packages.Count);                

                builder.AppendLine($"{package.Path}");
                builder.AppendLine($"{new string('=', package.Path.Length)}");
                builder.AppendLine($"Category: [{package.Category}]");
                builder.AppendLine($"Source {package.Source}");
                builder.AppendLine($"{package.Description}");
                builder.AppendLine($"");
            }

            syncList.Add(new Sync
            {
                PackageContentBasePath = "KrustWB3.readme.txt",
                SyncType = SyncType.SourceToTarget,
                FileType = FileType.File,
                SourcePath = outputPath,
                TargetPath = outputPath
            });

            
            FileHandlerFactory.AddCustomFileHandler(outputPath, new InmemoryFileHandler(outputPath, builder.ToString(), 0x00));
            ProgressBar.ClearProgressBar();
            _logger.Information("Build Readme done!");
        }

        private static void BuildUserStartup(IList<Config> configs, IList<Sync> syncList)
        {
            _logger.Information("Building user-startup ...");

            var configCnt = 0;
            var builder = new StringBuilder(32727);
            var path = "user-startup";
            var innerPath = "s\\user-startup";

            foreach (var config in configs)
            {
                configCnt++;
                var packageCnt = 0;
                var packages = GetIncludedPackages(config);
                foreach (var package in packages)
                {
                    ProgressBar.DrawProgressBar($"Build user-startup {configCnt}", packageCnt++, packages.Count);

                    var packagFolderBasePath = _pathService.Combine(config.SourceBasePath, package.Path, "");
                    using (var fileHandler = FileHandlerFactory.Create(_logger, packagFolderBasePath))
                    {
                        if (fileHandler.FileExists(path))
                        {
                            var userstartup = fileHandler.FileReadAllText(path);
                            if (userstartup.Contains(Environment.NewLine))
                            {
                                _logger.Warning($"user-starttup for {package.Path} has incorrect NewLine! Correct NewLine is '\\n'");
                                userstartup = userstartup.Replace(Environment.NewLine, "\n");
                            }
                            builder.Append(userstartup);                    
                        }
                    }
                }
            }
            syncList.Add(new Sync
            {
                PackageContentBasePath = path,
                SyncType = SyncType.SourceToTarget,
                FileType = FileType.File,
                SourcePath = innerPath,
                TargetPath = innerPath
            });
            var str = builder.ToString();
            FileHandlerFactory.AddCustomFileHandler(path, new InmemoryFileHandler(innerPath, str, 0x40)); // 0x40 = S

            ProgressBar.ClearProgressBar();
            _logger.Information("Build user-startup done!");
        }   

        private static void SynchronizeFile(Sync sync, IFileHandler contentFileHandler, IFileHandler outputFileHandler)
        {
            switch (sync.SyncType)
            {
                case SyncType.SourceToTarget:
                {
                    var fileDiff = GetFileDiff(sync, contentFileHandler, outputFileHandler);
                    if (fileDiff == FileDiff.Equal)
                    {
                        _logger.Debug(@"{SyncLogType}: [{TargetPath}]", SyncLogType.CopyToTarget.GetDescription(), sync.TargetPath);
                        _logger.Debug(@"{SyncLogType} : (files are equal!) [{SourcePath}]", SyncLogType.CopyFromSource.GetDescription(), sync.SourcePath, sync.TargetPath);
                    }
                    else
                    {
                        if (fileDiff == FileDiff.DiffTargetMissing || fileDiff == FileDiff.DiffSourceNewer)
                        {
                            _logger.Information(@"{SyncLogType}: [{TargetPath}] [{FileDiff}]", SyncLogType.CopyToTarget.GetDescription(), sync.TargetPath, fileDiff);
                            _logger.Debug(@"{SyncLogType}: [{SourcePath}] [{FileDiff}]", SyncLogType.CopyFromSource.GetDescription(), sync.SourcePath, fileDiff);
                        }
                        else
                        {
                            _logger.Warning(@"{SyncLogType}: [{TargetPath}] [{FileDiff}]", SyncLogType.CopyToTarget.GetDescription(), sync.TargetPath, fileDiff);
                            _logger.Debug(@"{SyncLogType}: [{SourcePath}] [{FileDiff}]", SyncLogType.CopyFromSource.GetDescription(), sync.SourcePath, fileDiff);
                        }

                        outputFileHandler.FileCopy(contentFileHandler, sync.SourcePath, sync.TargetPath);
                    }

                    break;
                }
                case SyncType.TargetToSource:
                {
                    var fileDiff = GetFileDiff(sync, contentFileHandler, outputFileHandler);
                    {
                        switch (fileDiff)
                        {
                            case FileDiff.Equal:
                            {
                                _logger.Debug(@"{SyncLogType}: [{SourcePath}]", SyncLogType.CopyToSource.GetDescription(), sync.SourcePath);
                                _logger.Debug(@"{SyncLogType}: (files are equal!) [{TargetPath}]", SyncLogType.CopyFromTarget.GetDescription(), sync.TargetPath);
                                break;
                            }
                            case FileDiff.DiffSourceMissing:
                            case FileDiff.DiffTargetNewer:
                            {
                                _logger.Information(@"{SyncLogType}: [{SourcePath}] [{FileDiff}]", SyncLogType.CopyToSource.GetDescription(), sync.SourcePath, fileDiff);
                                _logger.Debug(@"{SyncLogType}: [{TargetPath}] [{FileDiff}]", SyncLogType.CopyFromTarget.GetDescription(), sync.TargetPath, fileDiff);
                                contentFileHandler.FileCopyBack(sync.SourcePath, outputFileHandler, sync.TargetPath);
                                break;
                            }
                            case FileDiff.DiffTargetMissing:
                            {
                                _logger.Information(@"{SyncLogType}: [{SourcePath}] [{FileDiff}]", SyncLogType.CopyToSource.GetDescription(), sync.SourcePath, fileDiff);
                                _logger.Debug(@"{SyncLogType}: (invserse) [{TargetPath}] [{FileDiff}]", SyncLogType.CopyFromTarget.GetDescription(), sync.TargetPath, fileDiff);
                                outputFileHandler.FileCopy(contentFileHandler, sync.SourcePath, sync.TargetPath);
                                break;
                            }
                            case FileDiff.DiffContent:
                            case FileDiff.DiffSourceNewer:
                            default:
                            {
                                _logger.Warning(@"{SyncLogType}: [{SourcePath}] [{FileDiff}]", SyncLogType.CopyToSource.GetDescription(), sync.SourcePath, fileDiff);
                                _logger.Debug(@"{SyncLogType}: [{TargetPath}] [{FileDiff}]", SyncLogType.CopyFromTarget.GetDescription(), sync.TargetPath, fileDiff);
                                outputFileHandler.FileCopyBack(sync.SourcePath, outputFileHandler, sync.TargetPath);
                                //outputFileHandler.FileCopy(contentFileHandler, sync.SourcePath, sync.TargetPath);
                                break;
                            }
                        }

                    }
                    break;
                }
                case SyncType.DeleteTarget:
                {
                    if (outputFileHandler.FileExists(sync.TargetPath))
                    {
                        _logger.Information(@"{SyncLogType}: [{TargetPath}]", SyncLogType.DeleteTarget.GetDescription(), sync.TargetPath);
                        outputFileHandler.FileDelete(sync.TargetPath);
                    }
                    else
                    {
                        _logger.Debug(@"{SyncLogType}: (nothing to delete!) [{TargetPath}]", SyncLogType.DeleteTarget.GetDescription(), sync.TargetPath);
                    }
                    break;
                }
                case SyncType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static FileDiff GetFileDiff(Sync sync, IFileHandler contentFileHandler, IFileHandler outputFileHandler)
        {
            var sourceInfo = contentFileHandler.GetFileInfo(sync.SourcePath);
            var targetInfo = outputFileHandler.GetFileInfo(sync.TargetPath);

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

            if (sourceInfo.LastWriteTime < targetInfo.LastWriteTime)
            {
                return FileDiff.DiffTargetNewer;
            }

            if (targetInfo.LastWriteTime < sourceInfo.LastWriteTime)
            {
                return FileDiff.DiffSourceNewer;
            }

            return FileDiff.DiffContent;
        }

        private static void SynchronizeDirectory(Sync sync, IFileHandler contentFileHandler, IFileHandler outputFileHandler)
        {
            switch (sync.SyncType)
            {
                case SyncType.SourceToTarget:
                    if (outputFileHandler.DirectoryExists(sync.TargetPath))
                    {
                        _logger.Debug(@"{SyncLogType}: (already exists) [{TargetPath}]", SyncLogType.CreateTargetDirectory.GetDescription(), sync.TargetPath);
                    }
                    else
                    {
                        _logger.Information(@"{SyncLogType}: [{TargetPath}]", SyncLogType.CreateTargetDirectory.GetDescription(), sync.TargetPath);
                        outputFileHandler.DirectoryCreateDirectory(sync.TargetPath);
                    }

                    break;
                case SyncType.TargetToSource:
                    if (contentFileHandler.DirectoryExists(sync.SourcePath))
                    {
                        _logger.Debug(@"{SyncLogType}: (already exists) [{SourcePath}]", SyncLogType.CreateSourceDirectory.GetDescription(), sync.SourcePath);
                    }
                    else
                    {
                        _logger.Information(@"{SyncLogType}: [{SourcePath}]", SyncLogType.CreateSourceDirectory.GetDescription(), sync.SourcePath);
                        contentFileHandler.DirectoryCreateDirectory(sync.SourcePath);
                    }

                    if (outputFileHandler.DirectoryExists(sync.TargetPath) == false)
                    { 
                        _logger.Information(@"{SyncLogType}: [{TargetPath}]", SyncLogType.CreateTargetDirectory.GetDescription(), sync.TargetPath);
                        outputFileHandler.DirectoryCreateDirectory(sync.TargetPath);
                    }

                    break;
                case SyncType.DeleteTarget:
                    if (outputFileHandler.DirectoryExists(sync.TargetPath))
                    {
                        _logger.Information(@"{SyncLogType}: [{TargetPath}]", SyncLogType.DeleteTargetDirectory.GetDescription(), sync.TargetPath);
                        outputFileHandler.DirectoryDelete(sync.TargetPath, recursive: true);
                    }
                    else
                    {
                        _logger.Debug(@"{SyncLogType}: (already deleted) [{TargetPath}]", SyncLogType.DeleteTargetDirectory.GetDescription(), sync.TargetPath);
                    }

                    break;
                case SyncType.Unknown:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string RemoveRoot(string root, string target)
        {
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
}

