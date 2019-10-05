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
                if (line.Trim().StartsWith("//"))
                {
                    continue;
                }

                if (line == "@exclude_function_patterns")
                {
                    excluding = true;
                    including = false;
                    continue;
                }

                if (line == "@include_function_patterns")
                {
                    excluding = false;
                    including = true;
                    continue;
                }

                if (excluding)
                {
                    filter._excludeRules.Add(line.Trim());
                }

                if (including)
                {
                    filter._includeRules.Add(line.Trim());
                }
            }

            return filter;
        }


        public bool IsHidden(string function)
        {
            if (!_includeRules.Any() && !_excludeRules.Any())
            {
                return false;
            }


            foreach (var rule in _includeRules)
            {
                if (Regex.IsMatch(function, rule))
                {
                    // Inclusive list has precedence over exclude list
                    return false;
                }
            }

            if (!_includeRules.Any())
            {
                // We have include rules. Default is hidden.
                return true;
            }


            // We don't have any include rules. Default is visible.
            foreach (var rule in _excludeRules)
            {
                if (Regex.IsMatch(function, rule))
                {
                    return true;
                }
            }

           
            return false;
        }
    }
}