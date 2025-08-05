using System.Collections.Generic;
using System.Linq;
using Zorro.Core.CLI;

namespace PEAK_Menu.Utils.CLI
{
    [TypeParser(typeof(Player))]
    public class PlayerCLIParser : CLITypeParser
    {
        public override object Parse(string str)
        {
            var player = Character.AllCharacters.FirstOrDefault(c => c.characterName.Equals(str, System.StringComparison.OrdinalIgnoreCase)) ?? Character.AllCharacters.FirstOrDefault(c => c.characterName.ToLower().Contains(str.ToLower()));
            return player;
        }

        public override List<ParameterAutocomplete> FindAutocomplete(string parameterText)
        {
            var suggestions = new List<ParameterAutocomplete>();
            
            try
            {
                foreach (var character in Character.AllCharacters)
                {
                    var name = character.characterName;
                    if (string.IsNullOrEmpty(parameterText) || 
                        name.ToLower().Contains(parameterText.ToLower()))
                    {
                        suggestions.Add(new ParameterAutocomplete($"\"{name}\""));
                    }
                }
            }
            catch (System.Exception ex)
            {
                Plugin.Log?.LogWarning($"Error getting player autocomplete: {ex.Message}");
            }
            
            return suggestions;
        }
    }
}