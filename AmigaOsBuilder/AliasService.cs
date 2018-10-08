using System;
using System.Collections;
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
            { @"__workdrive__",          @"Work" },

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
            { @"__aprograms__",          @"System\A-Programs" },
            { @"__asystem__",            @"System\A-System" },
            { @"__adev__",               @"System\A-Dev" },
            { @"__aguides__",            @"Work\A-Guides" },
            { @"__awhdgames__",          @"Work\A-WHDGames" },

            { @"__sdk__",                @"Work\SDK" },
        };
        // @formatter:on

        public static string TargetAliasToOutputPath(string aliasedPath)
        {
            foreach (var mapKey in AliasToOutputMap.Keys)
            {
                aliasedPath = aliasedPath.Replace(mapKey, AliasToOutputMap[mapKey]);
            }

            if (aliasedPath.Contains("__"))
            {
                throw new Exception($"Couldn't map target alias {aliasedPath}!");
            }

            return aliasedPath;          
        }

        public static IEnumerable<string> GetAliases()
        {
            return AliasToOutputMap.Keys;
        }

        public static string GetAliasPath(string alias)
        {
            return AliasToOutputMap[alias];
        }
    }
}