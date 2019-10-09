using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Launcher
{
    internal class Filter
    {
        private readonly List<Regex> _excludeRules = new List<Regex>();
        private readonly List<Regex> _includeRules = new List<Regex>();
        private readonly List<Regex> _entryRules = new List<Regex>();

        private Filter()
        {
        }

        public static Filter Default()
        {
            return new Filter();
        }

        public static Filter FromFile(string file)
        {
            if (!File.Exists(file))
            {
                return new Filter();
            }

            var filter = new Filter();

            var excluding = false;
            var including = false;
            var entry = false;

            var lines = File.ReadAllLines(file);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                {
                    continue;
                }

                if (trimmed.StartsWith("//"))
                {
                    continue;
                }


                if (trimmed == "@exclude_function_patterns")
                {
                    excluding = true;
                    including = false;
                    entry = false;
                    continue;
                }

                if (trimmed == "@include_function_patterns")
                {
                    excluding = false;
                    including = true;
                    entry = false;
                    continue;
                }

                if (trimmed == "@entry_functions")
                {
                    excluding = false;
                    including = false;
                    entry = true;
                    continue;
                }

                if (excluding)
                {
                    var pattern = new Regex(trimmed, RegexOptions.Compiled);
                    filter._excludeRules.Add(pattern);
                }

                if (including)
                {
                    var pattern = new Regex(trimmed, RegexOptions.Compiled);
                    filter._includeRules.Add(pattern);
                }

                if (entry)
                {
                    var pattern = new Regex(trimmed, RegexOptions.Compiled);
                    filter._entryRules.Add(pattern);
                }
            }

            return filter;
        }


        public bool IsEntry(string function)
        {
            if (!_entryRules.Any())
            {
                return true;
            }

            foreach (var rule in _entryRules)
            {
                if (rule.IsMatch(function))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsHidden(string function)
        {
            // 1. No filters at all, default is everything is visible
            // 2. Only include filters, default is everything is hidden
            // 3. Only exclude filters, default in everything is visible
            // 4. Both filters, default is everything is hidden, then include is applied, 
            //    the exclude to exclude again.


            if (!_includeRules.Any() && !_excludeRules.Any())
            {
                // No filtes at all, default is everything is visible
                return false;
            }

            // Only include filters or include and exclude filters: Default is hidden.
            bool hidden = true;
            if (!_includeRules.Any() && _excludeRules.Any())
            {
                // Only exclude filters: default is visible
                hidden = false;
            }

            foreach (var rule in _includeRules)
            {
                
                if (rule.IsMatch(function))
                {
                    hidden = false;
                    break;
                }
            }


            // We don't have any include rules. Default is visible.
            foreach (var rule in _excludeRules)
            {
                if (rule.IsMatch(function))
                {
                    hidden = true;
                    break;
                }
            }


           
            return hidden;
        }
    }
}