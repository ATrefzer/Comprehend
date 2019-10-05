using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Launcher
{
    internal class Filter
    {
        private readonly List<string> _excludeRules = new List<string>();
        private readonly List<string> _includeRules = new List<string>();

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

            var excluding = true;
            var including = false;

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
                    continue;
                }

                if (trimmed == "@include_function_patterns")
                {
                    excluding = false;
                    including = true;
                    continue;
                }

                if (excluding)
                {
                    filter._excludeRules.Add(trimmed);
                }

                if (including)
                {
                    filter._includeRules.Add(trimmed);
                }
            }

            return filter;
        }


        public bool IsHidden(string function)
        {
            // 1. No filtes at all, default is everything is visible
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
                if (Regex.IsMatch(function, rule))
                {
                    hidden = false;
                    break;
                }
            }


            // We don't have any include rules. Default is visible.
            foreach (var rule in _excludeRules)
            {
                if (Regex.IsMatch(function, rule))
                {
                    hidden = true;
                    break;
                }
            }

           
            return hidden;
        }
    }
}