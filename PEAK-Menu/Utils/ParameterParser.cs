using System;
using System.Collections.Generic;
using System.Linq;

namespace PEAK_Menu.Utils
{
    /// <summary>
    /// Centralized parameter parsing utility for consistent command parameter handling
    /// </summary>
    public static class ParameterParser
    {
        public struct ParsedParameters
        {
            public string PlayerName { get; set; }
            public float? NumericValue { get; set; }
            public bool? BooleanValue { get; set; }
            public string Action { get; set; }
            public string[] RemainingParameters { get; set; }
        }

        /// <summary>
        /// Parse parameters expecting: [action] [player] [value]
        /// </summary>
        public static ParsedParameters ParsePlayerAndValue(string[] parameters, int startIndex = 1)
        {
            var result = new ParsedParameters
            {
                RemainingParameters = new string[0]
            };
            
            if (parameters.Length <= startIndex)
                return result;
                
            // Check if last parameter is a numeric value
            if (parameters.Length > startIndex + 1 && 
                TryParseNumeric(parameters[parameters.Length - 1], out float numValue))
            {
                result.NumericValue = numValue;
                result.PlayerName = string.Join(" ", parameters.Skip(startIndex).Take(parameters.Length - startIndex - 1));
            }
            // Check if last parameter is a boolean value
            else if (parameters.Length > startIndex + 1 && 
                    TryParseBoolean(parameters[parameters.Length - 1], out bool boolValue))
            {
                result.BooleanValue = boolValue;
                result.PlayerName = string.Join(" ", parameters.Skip(startIndex).Take(parameters.Length - startIndex - 1));
            }
            else
            {
                result.PlayerName = string.Join(" ", parameters.Skip(startIndex));
            }
            
            // Clean quotes from player name
            result.PlayerName = CleanPlayerName(result.PlayerName);
            
            // Store action if available
            if (parameters.Length > 0)
            {
                result.Action = parameters[0];
            }
            
            return result;
        }

        /// <summary>
        /// Parse parameters for commands with subcommands: [command] [subcommand] [parameters...]
        /// </summary>
        public static ParsedParameters ParseSubCommand(string[] parameters)
        {
            var result = new ParsedParameters
            {
                RemainingParameters = new string[0]
            };
            
            if (parameters.Length == 0)
                return result;
                
            result.Action = parameters[0].ToLower();
            
            if (parameters.Length > 1)
            {
                result.RemainingParameters = parameters.Skip(1).ToArray();
                
                // Try to parse common patterns
                if (result.RemainingParameters.Length > 0)
                {
                    var lastParam = result.RemainingParameters[result.RemainingParameters.Length - 1];
                    
                    // Check for numeric value at end
                    if (TryParseNumeric(lastParam, out float numValue))
                    {
                        result.NumericValue = numValue;
                        if (result.RemainingParameters.Length > 1)
                        {
                            result.PlayerName = string.Join(" ", result.RemainingParameters.Take(result.RemainingParameters.Length - 1));
                            result.PlayerName = CleanPlayerName(result.PlayerName);
                        }
                    }
                    // Check for boolean value at end
                    else if (TryParseBoolean(lastParam, out bool boolValue))
                    {
                        result.BooleanValue = boolValue;
                        if (result.RemainingParameters.Length > 1)
                        {
                            result.PlayerName = string.Join(" ", result.RemainingParameters.Take(result.RemainingParameters.Length - 1));
                            result.PlayerName = CleanPlayerName(result.PlayerName);
                        }
                    }
                    else
                    {
                        // No special value, treat as player name
                        result.PlayerName = string.Join(" ", result.RemainingParameters);
                        result.PlayerName = CleanPlayerName(result.PlayerName);
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// Simple parameter parsing for single values
        /// </summary>
        public static ParsedParameters ParseSingleValue(string[] parameters, int valueIndex = 0)
        {
            var result = new ParsedParameters();
            
            if (parameters.Length <= valueIndex)
                return result;
                
            var value = parameters[valueIndex];
            
            if (TryParseNumeric(value, out float numValue))
            {
                result.NumericValue = numValue;
            }
            else if (TryParseBoolean(value, out bool boolValue))
            {
                result.BooleanValue = boolValue;
            }
            
            result.Action = value;
            result.RemainingParameters = parameters.Skip(valueIndex + 1).ToArray();
            
            return result;
        }

        /// <summary>
        /// Parse coordinates from parameters (x, y, z)
        /// </summary>
        public static bool TryParseCoordinates(string[] parameters, int startIndex, out UnityEngine.Vector3 coordinates)
        {
            coordinates = UnityEngine.Vector3.zero;
            
            if (parameters.Length < startIndex + 3)
                return false;
                
            if (float.TryParse(parameters[startIndex], out float x) &&
                float.TryParse(parameters[startIndex + 1], out float y) &&
                float.TryParse(parameters[startIndex + 2], out float z))
            {
                coordinates = new UnityEngine.Vector3(x, y, z);
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Parse item name and optional quantity
        /// </summary>
        public static ParsedParameters ParseItemAndQuantity(string[] parameters, int startIndex = 0)
        {
            var result = new ParsedParameters();
            
            if (parameters.Length <= startIndex)
                return result;
                
            // Check if last parameter is a quantity
            if (parameters.Length > startIndex + 1 && 
                TryParseNumeric(parameters[parameters.Length - 1], out float quantity))
            {
                result.NumericValue = quantity;
                result.PlayerName = string.Join(" ", parameters.Skip(startIndex).Take(parameters.Length - startIndex - 1));
            }
            else
            {
                result.PlayerName = string.Join(" ", parameters.Skip(startIndex));
            }
            
            result.PlayerName = CleanPlayerName(result.PlayerName);
            return result;
        }

        /// <summary>
        /// Validate numeric value is within range
        /// </summary>
        public static bool ValidateNumericRange(float? value, float min, float max, out string errorMessage)
        {
            errorMessage = null;
            
            if (!value.HasValue)
            {
                errorMessage = "No numeric value provided";
                return false;
            }
            
            if (value.Value < min || value.Value > max)
            {
                errorMessage = $"Value {value.Value} is outside valid range ({min}-{max})";
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Validate integer range
        /// </summary>
        public static bool ValidateIntegerRange(float? value, int min, int max, out string errorMessage)
        {
            errorMessage = null;
            
            if (!value.HasValue)
            {
                errorMessage = "No numeric value provided";
                return false;
            }
            
            int intValue = (int)value.Value;
            
            if (intValue < min || intValue > max)
            {
                errorMessage = $"Value {intValue} is outside valid range ({min}-{max})";
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Get target players from player name (supports "all" keyword)
        /// </summary>
        public static List<Character> GetTargetPlayers(string playerName, out string errorMessage)
        {
            errorMessage = null;
            var results = new List<Character>();
            
            if (string.IsNullOrWhiteSpace(playerName))
            {
                errorMessage = "No player name provided";
                return results;
            }
            
            if (playerName.ToLower() == "all")
            {
                results.AddRange(Character.AllCharacters.Where(c => c != null));
                return results;
            }
            
            var character = FindPlayerByName(playerName);
            if (character != null)
            {
                results.Add(character);
            }
            else
            {
                errorMessage = $"Player '{playerName}' not found";
            }
            
            return results;
        }

        private static bool TryParseNumeric(string value, out float result)
        {
            result = 0f;
            
            if (string.IsNullOrWhiteSpace(value))
                return false;
                
            return float.TryParse(value, out result);
        }

        private static bool TryParseBoolean(string value, out bool result)
        {
            result = false;
            
            if (string.IsNullOrWhiteSpace(value))
                return false;
                
            var lower = value.ToLower().Trim();
            
            if (lower == "on" || lower == "true" || lower == "1" || lower == "enable" || lower == "enabled")
            {
                result = true;
                return true;
            }
            
            if (lower == "off" || lower == "false" || lower == "0" || lower == "disable" || lower == "disabled")
            {
                result = false;
                return true;
            }
            
            return false;
        }

        private static string CleanPlayerName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return string.Empty;
                
            playerName = playerName.Trim();
            
            // Remove surrounding quotes
            if ((playerName.StartsWith("\"") && playerName.EndsWith("\"")) ||
                (playerName.StartsWith("'") && playerName.EndsWith("'")))
            {
                playerName = playerName.Substring(1, playerName.Length - 2);
            }
            
            return playerName.Trim();
        }

        private static Character FindPlayerByName(string playerName)
        {
            if (string.IsNullOrWhiteSpace(playerName))
                return null;

            var allCharacters = Character.AllCharacters;
            
            // Try exact match first (case-insensitive)
            var exactMatch = allCharacters.FirstOrDefault(c => 
                string.Equals(c.characterName, playerName, StringComparison.OrdinalIgnoreCase));
            
            if (exactMatch != null)
                return exactMatch;

            // Try partial match (contains)
            var partialMatch = allCharacters.FirstOrDefault(c => 
                c.characterName.ToLower().Contains(playerName.ToLower()));
            
            return partialMatch;
        }
    }
}