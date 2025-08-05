using System.Collections.Generic;
using Zorro.Core.CLI;

namespace PEAK_Menu.Utils.CLI
{
    [TypeParser(typeof(string))]
    public class StringCLIParser : CLITypeParser
    {
        public override object Parse(string str)
        {
            // Remove quotes if present
            if (str.StartsWith("\"") && str.EndsWith("\""))
            {
                return str.Substring(1, str.Length - 2);
            }
            return str;
        }

        public override List<ParameterAutocomplete> FindAutocomplete(string parameterText)
        {
            // For strings, we can provide common suggestions or context-based ones
            return new List<ParameterAutocomplete>();
        }
    }
}