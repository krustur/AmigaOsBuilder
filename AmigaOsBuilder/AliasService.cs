using System;
using System.Collections.Generic;
using System.IO;
using Serilog.Core;

namespace AmigaOsBuilder
{
    class AliasService
    {
        // @formatter:off
        private static readonly IDictionary<string, string> AliasToOutputMap = new Dictionary<string, string>
        {
            // System drive
            { @"__systemdrive__",        @"System" },

            // Amiga OS folders
            { @"__c__",                  @"System\C" },
            { @"__devs__",               @"System\Devs" },
            { @"__l__",                  @"System\L" },
            { @"__locale__",             @"System\Locale" },
            { @"__libs__",               @"System\Libs" },
            { @"__prefs__",              @"System\Prefs" },
            { @"__s__",                  @"System\S" },
            { @"__storage__",            @"System\Storage" },
            { @"__system__",             @"System\System" },
            { @"__utilities__",          @"System\Utilities" },
            { @"__wbstartup__",          @"System\WBStartup" },

            // KrustWB folders
            { @"__agames__",             @"System\A-Games" },
            { @"__aguides__",            @"System\A-Guides" },
            { @"__aprograms__",          @"System\A-Programs" },
            { @"__asystem__",            @"System\A-System" },
            { @"__awhdpackages__",       @"System\A-WHDPackages" },
        };
        // @formatter:on

        public static void CreateOutputAliasDirectories(Logger logger, string outputBasePath)
        {
            logger.Information("Creating output alias directories ...");
            Directory.CreateDirectory(outputBasePath);
            foreach (var map in AliasService.AliasToOutputMap)
            {
                var outputAliasPath = Path.Combine(outputBasePath, map.Value);
                logger.Information($@"[{map.Key}] = [{outputAliasPath}]");
                Directory.CreateDirectory(outputAliasPath);
            }

            logger.Information("Create output alias directories done!");
        }

        public static string TargetAliasToOutputPath(string targetAlias)
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
    }
}