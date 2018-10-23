using System;
using System.Collections.Generic;

namespace AmigaOsBuilder
{
    public class AliasService
    {
        private readonly IDictionary<string, string> _aliasToOutputMap;
    
        public AliasService(IDictionary<string, string> aliasToOutputMap)
        {
            _aliasToOutputMap = aliasToOutputMap;
        }

        public string TargetAliasToOutputPath(string aliasedPath)
        {
            foreach (var mapKey in _aliasToOutputMap.Keys)
            {
                aliasedPath = aliasedPath.Replace(mapKey, _aliasToOutputMap[mapKey]);
            }

            if (aliasedPath.Contains("__"))
            {
                throw new Exception($"Couldn't map target alias {aliasedPath}!");
            }

            return aliasedPath;          
        }

        public IEnumerable<string> GetAliases()
        {
            return _aliasToOutputMap.Keys;
        }

        public string GetAliasPath(string alias)
        {
            return _aliasToOutputMap[alias];
        }
    }
}