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
        private static readonly StringBuilder ReadmeBuilder = new StringBuilder(32727);

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

                var sourceBasePath = configuration["SourceBasePath"];                
                var outputBasePath = configuration["OutputBasePath"];

                var sourceBasePath2 = configuration["SourceBasePath2"];
                var outputBasePath2 = configuration["OutputBasePath2"];

                var configFile = configuration["ConfigFile"];

                BuildIt(sourceBasePath, outputBasePath, configFile);
                BuildIt(sourceBasePath2, outputBasePath2, configFile);
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

        private static void BuildIt(string sourceBasePath, string outputBasePath, string configFileName)
        {
            using (var outputFileHandler = FileHandlerFactory.Create(_logger, outputBasePath))
            {
                var config = ConfigService.GetConfig(configFileName);

                outputFileHandler.CreateBasePaths();

                var syncList = BuildSyncList(sourceBasePath, outputFileHandler, config);

                SynchronizeV2(syncList, outputFileHandler);

                SynchronizeTextFile(UserStartupBuilder.ToString(), outputFileHandler, "__s__", "user-startup");
                SynchronizeTextFile(ReadmeBuilder.ToString(), outputFileHandler, "__systemdrive__", "readme_krustwb3.txt");
            }
        }

        private static List<Sync> BuildSyncList(string sourceBasePath, IFileHandler outputFileHandler, Config config)
        {
            _logger.Information("Building sync list ...");

            var syncList = new List<Sync>();

            AddContentToSyncList(sourceBasePath, outputFileHandler, config, "content", SyncType.SourceToTarget, syncList, appendToReadme: true);
            AddDeleteToSyncList(outputFileHandler, syncList);
            AddContentToSyncList(sourceBasePath, outputFileHandler, config, "content_reverse", SyncType.TargetToSource, syncList, appendToReadme: false);

            ProgressBar.ClearProgressBar();

            _logger.Information("Building sync list done!");

            return syncList;
        }

        private static void AddContentToSyncList(string sourceBasePath, IFileHandler outputFileHandler, Config config,
            string contentFolderName, SyncType syncType, List<Sync> syncList, bool appendToReadme)
        {
            int packageCnt = 0;
            foreach (var package in config.Packages)
            {
                ProgressBar.DrawProgressBar("AddContentToSyncList ", packageCnt++, config.Packages.Count);
                if (package.Include == false)
                {
                    continue;
                }

                if (appendToReadme)
                {
                    ReadmeBuilder.AppendLine($"{package.Path}");
                    ReadmeBuilder.AppendLine($"{new string('=', package.Path.Length)}");
                    ReadmeBuilder.AppendLine($"Category: [{package.Category}]");
                    ReadmeBuilder.AppendLine($"Source {package.Source}");
                    ReadmeBuilder.AppendLine($"{package.Description}");
                    ReadmeBuilder.AppendLine($"");
                }

                var packageBasePath = _pathService.Combine(sourceBasePath, package.Path);
                if (Directory.Exists(packageBasePath) == false)
                {
                    throw new Exception($"Package [{package.Path}] base path [{packageBasePath}] is missing, check your configuration.");
                }



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


                    var packageTargets = contentFileHandler.DirectoryGetDirectories("");
                    foreach (var sourcePath in packageTargets)
                    {
                        var dirInfo = new DirectoryInfo(sourcePath);
                        var targetAlias = dirInfo.Name;

                        var packageOutputPath = AliasService.TargetAliasToOutputPath(targetAlias);
                        //packageOutputPath = _pathService.Combine(outputBasePath, packageOutputPath);

                        var packageEntries = contentFileHandler.DirectoryGetFileSystemEntriesRecursive(sourcePath);
                        foreach (var packageEntry in packageEntries)
                        {
                            var packageEntryFileName = _pathService.GetFileName(packageEntry);
                            if (ShouldContentReverseAll(packageEntryFileName))
                            {
                                var contentReversePackageEntryPath = _pathService.GetDirectoryName(packageEntry);
                                var contentReversePackageSubPath = RemoveRoot(sourcePath, contentReversePackageEntryPath);
                                var contentReverseFileOutputPath = _pathService.Combine(packageOutputPath, contentReversePackageSubPath);
                                if (outputFileHandler.DirectoryExists(contentReverseFileOutputPath))
                                {
                                    var innerContentReversePackageEntries = outputFileHandler.DirectoryGetFileSystemEntriesRecursive(contentReverseFileOutputPath);
                                    foreach (var innerContentReversePackageEntry in innerContentReversePackageEntries)
                                    {
                                        //var innerContentReversePackageSubPath = fileHandler.GetSubPath(innerContentReversePackageEntry);
                                        //var innerContentReversePackageSubPath = innerContentReversePackageEntry;
                                        var innerContentReversePackageSubPath = RemoveRoot(packageOutputPath, innerContentReversePackageEntry);
                                        var innerContentReverseSourcePath = _pathService.Combine(sourcePath, innerContentReversePackageSubPath);

                                        var innerSync = new Sync
                                        {
                                            PackageContentBasePath = packageContentBasePath,
                                            SourcePath = innerContentReverseSourcePath,
                                            TargetPath = innerContentReversePackageEntry,
                                            //TargetPath = innerContentReversePackageSubPath,
                                            SyncType = syncType,
                                            FileType = outputFileHandler.GetFileType(innerContentReversePackageEntry)
                                        };

                                        syncList.Add(innerSync);
                                    }
                                }
                            }
                            else
                            {
                                var packageSubPath = RemoveRoot(sourcePath, packageEntry);
                                var fileOutputPath = _pathService.Combine(packageOutputPath, packageSubPath);
                                //_logger.Information($"{packageEntry} => {fileOutputPath}");
                                var sync = new Sync
                                {
                                    PackageContentBasePath = packageContentBasePath,
                                    SourcePath = packageEntry,
                                    //TargetPath = fileOutputPath,
                                    TargetPath = fileOutputPath,
                                    SyncType = syncType,
                                    FileType = contentFileHandler.GetFileType(packageEntry)
                                };

                                syncList.Add(sync);
                            }
                        }
                    }

                    var packageFiles = contentFileHandler.DirectoryGetFiles("");
                    foreach (var packageFile in packageFiles)
                    {
                        var packageFileName = _pathService.GetFileName(packageFile)
                            .ToLowerInvariant();
                        switch (packageFileName)
                        {
                            case "user-startup":
                            {
                                var userStartupContent = contentFileHandler.FileReadAllText(packageFile);
                                UserStartupBuilder.Append(userStartupContent);
                                break;
                            }
                            default:
                            {
                                throw new Exception($"Package file [{packageFileName}] is not supported!");
                            }
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
            _logger.Information("Synchronizing ...");
           
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

        private static void SynchronizeTextFile(string content, IFileHandler outputFileHandler, string outputSubPath, string fileName)
        {
            var outputPath = _pathService.Combine(AliasService.TargetAliasToOutputPath(outputSubPath), fileName);

            var oldContent = outputFileHandler.FileExists(outputPath) ? outputFileHandler.FileReadAllText(outputPath) : string.Empty;

            content = content.Replace("\r\n", "\n");
            //if (content != oldContent)
            if (content.Equals(oldContent, StringComparison.Ordinal) == false)
            {
                _logger.Information(@"Updating text file: [{TargetPath}]", outputPath);
                _logger.Information("<<<< Begin content >>>>");
                _logger.Information(content);
                _logger.Information("<<<< End content >>>>");
                outputFileHandler.FileWriteAllText(outputPath, content);
            }
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
                                outputFileHandler.FileCopyBack(sync.TargetPath, sync.SourcePath, overwrite: true);
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
                                outputFileHandler.FileCopyBack(sync.TargetPath, sync.SourcePath, overwrite: true);
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

