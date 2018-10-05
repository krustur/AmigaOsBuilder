﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace AmigaOsBuilder
{
    //TODO: Warn for existing Sources that is missing in Config

    class Program
    {
        private static Logger _logger;
        private static readonly StringBuilder UserStartupBuilder = new StringBuilder(32727);
        private static readonly StringBuilder ReadmeBuilder = new StringBuilder(32727);

        static void Main(string[] args)
        {
            try
            {
                //LhaTest.RunTest();
                //return;

                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddCommandLine(args)
                    .Build();

                _logger = new LoggerConfiguration()
                    //.MinimumLevel.Debug()
                    .WriteTo.Console()
                    .WriteTo.File("AmigaOsBuilder_log.txt")
                    .CreateLogger();

                var location = configuration["Location"];
                var sourceBasePath = configuration["SourceBasePath"];
                
                var outputBasePath = configuration["OutputBasePath"];
                var outputHandler = new FolderOutputHandler(_logger, outputBasePath);

                var configFile = configuration["ConfigFile"];

                BuildIt(location, sourceBasePath, outputHandler, configFile);
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

        private static void BuildIt(string location, string sourceBasePath, IOutputHandler outputHandler, string configFileName)
        {
            var config = ConfigService.GetConfig(location, configFileName);

            //AliasService.CreateOutputAliasDirectories(_logger, outputBasePath);
            outputHandler.CreateBasePaths();

            var syncList = BuildSyncList(sourceBasePath, outputHandler, config);

            SynchronizeV2(syncList, outputHandler);

            SynchronizeTextFile(UserStartupBuilder.ToString(), outputHandler, "__s__", "user-startup");
            SynchronizeTextFile(ReadmeBuilder.ToString(), outputHandler, "__systemdrive__", "readme_krustwb3.txt");

        }

        private static List<Sync> BuildSyncList(string sourceBasePath, IOutputHandler outputHandler, Config config)
        {
            _logger.Information("Building sync list ...");

            var syncList = new List<Sync>();

            AddContentToSyncList(sourceBasePath, outputHandler, config, "content", SyncType.SourceToTarget, syncList, appendToReadme: true);
            AddDeleteToSyncList(outputHandler, syncList);
            AddContentToSyncList(sourceBasePath, outputHandler, config, "content_reverse", SyncType.TargetToSource, syncList, appendToReadme: false);

            ProgressBar.ClearProgressBar();

            _logger.Information("Building sync list done!");

            return syncList;
        }

        private static void AddContentToSyncList(string sourceBasePath, IOutputHandler outputHandler, Config config,
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

                var packageBasePath = Path.Combine(sourceBasePath, package.Path);
                if (Directory.Exists(packageBasePath) == false)
                {
                    throw new Exception($"Package [{package.Path}] base path [{packageBasePath}] is missing, check your configuration.");
                }

                var packageContentBasePath = Path.Combine(sourceBasePath, package.Path, contentFolderName);
                if (Directory.Exists(packageContentBasePath) == false)
                {
                    continue;
                }

                var packageTargets = Directory.GetDirectories(packageContentBasePath);
                foreach (var sourcePath in packageTargets)
                {
                    var dirInfo = new DirectoryInfo(sourcePath);
                    var targetAlias = dirInfo.Name;

                    var packageOutputPath_XXX = AliasService.TargetAliasToOutputPath(targetAlias);
                    //packageOutputPath = Path.Combine(outputBasePath, packageOutputPath);

                    var packageEntries = Directory.GetFileSystemEntries(sourcePath, "*", SearchOption.AllDirectories);
                    foreach (var packageEntry in packageEntries)
                    {
                        var packageEntryFileName = Path.GetFileName(packageEntry);
                        if (ShouldContentReverseAll(packageEntryFileName))
                        {
                            var contentReversePackageEntryPath = Path.GetDirectoryName(packageEntry);
                            var contentReversePackageSubPath = RemoveRoot(sourcePath, contentReversePackageEntryPath);
                            var contentReverseFileOutputPath = Path.Combine(packageOutputPath_XXX, contentReversePackageSubPath);
                            if (outputHandler.DirectoryExists(contentReverseFileOutputPath))
                            {
                                var innerContentReversePackageEntries = outputHandler.DirectoryGetFileSystemEntries(contentReverseFileOutputPath);
                                foreach (var innerContentReversePackageEntry in innerContentReversePackageEntries)
                                {
                                    //var innerContentReversePackageSubPath = outputHandler.GetSubPath(innerContentReversePackageEntry);
                                    //var innerContentReversePackageSubPath = innerContentReversePackageEntry;
                                    var innerContentReversePackageSubPath = RemoveRoot(packageOutputPath_XXX, innerContentReversePackageEntry);
                                    var innerContentReverseSourcePath = Path.Combine(sourcePath, innerContentReversePackageSubPath);

                                    var innerSync = new Sync
                                    {
                                        SourcePath = innerContentReverseSourcePath,
                                        TargetPath = innerContentReversePackageEntry,
                                        //TargetPath = innerContentReversePackageSubPath,
                                        SyncType = syncType,
                                        FileType = outputHandler.GetFileType(innerContentReversePackageEntry)
                                    };

                                    syncList.Add(innerSync);
                                }
                            }
                        }
                        else
                        {
                            var packageSubPath = RemoveRoot(sourcePath, packageEntry);
                            var fileOutputPath = Path.Combine(packageOutputPath_XXX, packageSubPath);
                            //_logger.Information($"{packageEntry} => {fileOutputPath}");
                            var sync = new Sync
                            {
                                SourcePath = packageEntry,
                                //TargetPath = fileOutputPath,
                                TargetPath = fileOutputPath,
                                SyncType = syncType,
                                FileType = GetFileType(syncType, packageEntry)
                            };

                            syncList.Add(sync);
                        }
                    }
                }
                var packageFiles = Directory.GetFiles(packageContentBasePath);
                foreach (var packageFile in packageFiles)
                {
                    var packageFileName = Path.GetFileName(packageFile).ToLowerInvariant();
                    switch (packageFileName)
                    {
                        case "user-startup":
                        {
                            var userStartupContent = File.ReadAllText(packageFile);
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

        private static bool ShouldContentReverseAll(string packageEntryFileName)
        {
            if (packageEntryFileName.ToLowerInvariant() == "content_reverse_all")
            {
                return true;
            }

            return false;
        }

        private static void AddDeleteToSyncList(IOutputHandler outputHandler, List<Sync> syncList)
        {
            var outputEntries = outputHandler.DirectoryGetFileSystemEntries("");
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
                        SyncType = SyncType.DeleteTarget,
                        FileType = outputHandler.GetFileType(outputEntry),
                        TargetPath = outputEntry
                    });
                    syncListKeys.Add(outputEntryLower);
                }
            }
        }

        private static void SynchronizeV2(IList<Sync> syncList, IOutputHandler outputHandler)
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
                switch (actualSync.FileType)
                {
                    case FileType.File:
                        SynchronizeFile(actualSync, outputHandler);
                        break;
                    case FileType.Directory:
                        SynchronizeDirectory(actualSync, outputHandler);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
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

        private static void SynchronizeTextFile(string content, IOutputHandler outputHandler, string outputSubPath, string fileName)
        {
            var outputPath = Path.Combine(AliasService.TargetAliasToOutputPath(outputSubPath), fileName);

            var oldContent = outputHandler.FileExists(outputPath) ? outputHandler.FileReadAllText(outputPath) : string.Empty;

            content = content.Replace("\r\n", "\n");
            //if (content != oldContent)
            if (content.Equals(oldContent, StringComparison.Ordinal) == false)
            {
                _logger.Information(@"Updating text file: [{TargetPath}]", outputPath);
                _logger.Information("<<<< Begin content >>>>");
                _logger.Information(content);
                _logger.Information("<<<< End content >>>>");
                outputHandler.FileWriteAllText(outputPath, content);
            }
        }

        private static void SynchronizeFile(Sync sync, IOutputHandler outputHandler)
        {
            switch (sync.SyncType)
            {
                case SyncType.SourceToTarget:
                {
                    var fileDiff = GetFileDiff(sync, outputHandler);
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

                        outputHandler.FileCopy(sync.SourcePath, sync.TargetPath, overwrite: true);
                    }

                    break;
                }
                case SyncType.TargetToSource:
                {
                    var fileDiff = GetFileDiff(sync, outputHandler);
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
                                outputHandler.FileCopyBack(sync.TargetPath, sync.SourcePath, overwrite: true);
                                break;
                            }
                            case FileDiff.DiffTargetMissing:
                            {
                                _logger.Information(@"{SyncLogType}: [{SourcePath}] [{FileDiff}]", SyncLogType.CopyToSource.GetDescription(), sync.SourcePath, fileDiff);
                                _logger.Debug(@"{SyncLogType}: (invserse) [{TargetPath}] [{FileDiff}]", SyncLogType.CopyFromTarget.GetDescription(), sync.TargetPath, fileDiff);
                                outputHandler.FileCopy(sync.SourcePath, sync.TargetPath, overwrite: true);
                                break;
                            }
                            case FileDiff.DiffContent:
                            case FileDiff.DiffSourceNewer:
                            default:
                            {
                                _logger.Warning(@"{SyncLogType}: [{SourcePath}] [{FileDiff}]", SyncLogType.CopyToSource.GetDescription(), sync.SourcePath, fileDiff);
                                _logger.Debug(@"{SyncLogType}: [{TargetPath}] [{FileDiff}]", SyncLogType.CopyFromTarget.GetDescription(), sync.TargetPath, fileDiff);
                                outputHandler.FileCopyBack(sync.TargetPath, sync.SourcePath, overwrite: true);
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
                        _logger.Information(@"{SyncLogType}: [{TargetPath}]", SyncLogType.DeleteTarget.GetDescription(), sync.TargetPath);
                        outputHandler.FileDelete(sync.TargetPath);
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

        private static FileDiff GetFileDiff(Sync sync, IOutputHandler outputHandler)
        {
            var sourceInfo = new FileInfo(sync.SourcePath);
            var targetInfo = outputHandler.GetFileInfo(sync.TargetPath);

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

        private static void SynchronizeDirectory(Sync sync, IOutputHandler outputHandler)
        {
            switch (sync.SyncType)
            {
                case SyncType.SourceToTarget:
                    if (outputHandler.DirectoryExists(sync.TargetPath))
                    {
                        _logger.Debug(@"{SyncLogType}: (already exists) [{TargetPath}]", SyncLogType.CreateTargetDirectory.GetDescription(), sync.TargetPath);
                    }
                    else
                    {
                        _logger.Information(@"{SyncLogType}: [{TargetPath}]", SyncLogType.CreateTargetDirectory.GetDescription(), sync.TargetPath);
                        outputHandler.DirectoryCreateDirectory(sync.TargetPath);
                    }

                    break;
                case SyncType.TargetToSource:
                    if (Directory.Exists(sync.SourcePath))
                    {
                        _logger.Debug(@"{SyncLogType}: (already exists) [{SourcePath}]", SyncLogType.CreateSourceDirectory.GetDescription(), sync.SourcePath);
                    }
                    else
                    {
                        _logger.Information(@"{SyncLogType}: [{SourcePath}]", SyncLogType.CreateSourceDirectory.GetDescription(), sync.SourcePath);
                        Directory.CreateDirectory(sync.SourcePath);
                    }

                    if (outputHandler.DirectoryExists(sync.TargetPath) == false)
                    { 
                        _logger.Information(@"{SyncLogType}: [{TargetPath}]", SyncLogType.CreateTargetDirectory.GetDescription(), sync.TargetPath);
                        outputHandler.DirectoryCreateDirectory(sync.TargetPath);
                    }

                    break;
                case SyncType.DeleteTarget:
                    if (outputHandler.DirectoryExists(sync.TargetPath))
                    {
                        _logger.Information(@"{SyncLogType}: [{TargetPath}]", SyncLogType.DeleteTargetDirectory.GetDescription(), sync.TargetPath);
                        outputHandler.DirectoryDelete(sync.TargetPath, recursive: true);
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

        public static FileType GetFileType(SyncType syncType, string path)
        {
            var packageEntryFileInfo = new FileInfo(path);
            var fileType = (packageEntryFileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory ? FileType.Directory : FileType.File;

            return fileType;
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

