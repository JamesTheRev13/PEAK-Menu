using PEAK_Menu.Utils;

namespace PEAK_Menu.Commands
{
    public class CustomizationCommand : BaseCommand
    {
        public override string Name => "customize";
        public override string Description => "Character customization commands (supports rainbow effect!)";
        
        public override string DetailedHelp =>
@"=== CUSTOMIZE Command Help ===
Character customization commands with rainbow effects!

Usage: customize <option> [value]

Options:
  skin <index>      - Set skin color by index (0-20)
  eyes <index>      - Set eyes by index
  mouth <index>     - Set mouth by index
  accessory <index> - Set accessory by index
  outfit <index>    - Set outfit by index
  hat <index>       - Set hat by index
  randomize         - Randomize all appearance

Rainbow Effects:
  rainbow           - Toggle rainbow skin effect
  rainbow on        - Enable rainbow effect
  rainbow off       - Disable rainbow effect
  rainbow speed <n> - Set rainbow speed (0.1-10.0)

Examples:
  customize rainbow on
  customize rainbow speed 2.5
  customize skin 3
  customize randomize";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                LogError("Missing customization option");
                LogInfo("Use 'help customize' for available options");
                return;
            }

            var parsed = ParameterParser.ParseSubCommand(parameters);
            
            switch (parsed.Action)
            {
                case "randomize":
                    HandleRandomizeCommand();
                    break;

                case "rainbow":
                    HandleRainbowCommand(parsed);
                    break;
                    
                case "skin":
                    HandleSkinCommand(parsed);
                    break;

                case "eyes":
                    HandleEyesCommand(parsed);
                    break;

                case "mouth":
                    HandleMouthCommand(parsed);
                    break;

                case "accessory":
                    HandleAccessoryCommand(parsed);
                    break;

                case "outfit":
                    HandleOutfitCommand(parsed);
                    break;

                case "hat":
                    HandleHatCommand(parsed);
                    break;

                default:
                    LogError($"Unknown customization option: {parsed.Action}");
                    LogInfo("Use 'help customize' for available options");
                    break;
            }
        }

        private void HandleRandomizeCommand()
        {
            if (Character.localCharacter?.refs?.customization != null)
            {
                Character.localCharacter.refs.customization.RandomizeCosmetics();
                LogInfo("Character appearance randomized");
            }
            else
            {
                LogError("No character customization found");
            }
        }

        private void HandleRainbowCommand(ParameterParser.ParsedParameters parsed)
        {
            var rainbowManager = Plugin.Instance?._menuManager?.GetRainbowManager();
            if (rainbowManager == null)
            {
                LogError("Rainbow manager not available");
                return;
            }

            if (parsed.RemainingParameters.Length == 0)
            {
                // Toggle rainbow mode
                rainbowManager.ToggleRainbow();
                LogInfo($"Rainbow skin effect {(rainbowManager.IsRainbowEnabled ? "enabled" : "disabled")}");
                return;
            }

            var action = parsed.RemainingParameters[0].ToLower();
            switch (action)
            {
                case "on":
                case "enable":
                case "start":
                    rainbowManager.EnableRainbow();
                    LogInfo("Rainbow skin effect enabled!");
                    break;

                case "off":
                case "disable":
                case "stop":
                    rainbowManager.DisableRainbow();
                    LogInfo("Rainbow skin effect disabled");
                    break;

                case "speed":
                    HandleRainbowSpeed(parsed.RemainingParameters, rainbowManager);
                    break;

                default:
                    LogError($"Unknown rainbow action: {action}");
                    LogInfo("Use 'help customize' for rainbow options");
                    break;
            }
        }

        private void HandleRainbowSpeed(string[] parameters, RainbowManager rainbowManager)
        {
            if (parameters.Length < 2)
            {
                LogError("Usage: customize rainbow speed <value>");
                LogInfo("Valid range: 0.1-10.0");
                return;
            }

            if (!float.TryParse(parameters[1], out float speed))
            {
                LogError("Please provide a valid speed value");
                return;
            }

            if (!ParameterParser.ValidateNumericRange(speed, 0.1f, 10.0f, out string error))
            {
                LogError($"Invalid rainbow speed: {error}");
                return;
            }

            rainbowManager.SetRainbowSpeed(speed);
            LogInfo($"Rainbow speed set to {speed:F1}x");
            if (speed >= 4.0f)
                LogInfo("RAINBOW OVERDRIVE!");
        }

        private void HandleSkinCommand(ParameterParser.ParsedParameters parsed)
        {
            if (!parsed.NumericValue.HasValue)
            {
                LogError("Usage: customize skin <index>");
                LogInfo("Valid range: 0-20");
                return;
            }

            if (!ParameterParser.ValidateIntegerRange(parsed.NumericValue, 0, 20, out string error))
            {
                LogError($"Invalid skin index: {error}");
                return;
            }

            int skinIndex = (int)parsed.NumericValue.Value;

            // Disable rainbow when manually setting skin
            var rainbowMgr = Plugin.Instance?._menuManager?.GetRainbowManager();
            if (rainbowMgr?.IsRainbowEnabled == true)
            {
                rainbowMgr.DisableRainbow();
                LogInfo("Rainbow disabled (manual skin change)");
            }
            
            CharacterCustomization.SetCharacterSkinColor(skinIndex);
            LogInfo($"Skin color set to index {skinIndex}");
        }

        private void HandleEyesCommand(ParameterParser.ParsedParameters parsed)
        {
            if (!parsed.NumericValue.HasValue)
            {
                LogError("Usage: customize eyes <index>");
                return;
            }

            if (!ParameterParser.ValidateIntegerRange(parsed.NumericValue, 0, 50, out string error))
            {
                LogError($"Invalid eye index: {error}");
                return;
            }

            int eyeIndex = (int)parsed.NumericValue.Value;
            CharacterCustomization.SetCharacterEyes(eyeIndex);
            LogInfo($"Eyes set to index {eyeIndex}");
        }

        private void HandleMouthCommand(ParameterParser.ParsedParameters parsed)
        {
            if (!parsed.NumericValue.HasValue)
            {
                LogError("Usage: customize mouth <index>");
                return;
            }

            if (!ParameterParser.ValidateIntegerRange(parsed.NumericValue, 0, 50, out string error))
            {
                LogError($"Invalid mouth index: {error}");
                return;
            }

            int mouthIndex = (int)parsed.NumericValue.Value;
            CharacterCustomization.SetCharacterMouth(mouthIndex);
            LogInfo($"Mouth set to index {mouthIndex}");
        }

        private void HandleAccessoryCommand(ParameterParser.ParsedParameters parsed)
        {
            if (!parsed.NumericValue.HasValue)
            {
                LogError("Usage: customize accessory <index>");
                return;
            }

            if (!ParameterParser.ValidateIntegerRange(parsed.NumericValue, 0, 50, out string error))
            {
                LogError($"Invalid accessory index: {error}");
                return;
            }

            int accessoryIndex = (int)parsed.NumericValue.Value;
            CharacterCustomization.SetCharacterAccessory(accessoryIndex);
            LogInfo($"Accessory set to index {accessoryIndex}");
        }

        private void HandleOutfitCommand(ParameterParser.ParsedParameters parsed)
        {
            if (!parsed.NumericValue.HasValue)
            {
                LogError("Usage: customize outfit <index>");
                return;
            }

            if (!ParameterParser.ValidateIntegerRange(parsed.NumericValue, 0, 50, out string error))
            {
                LogError($"Invalid outfit index: {error}");
                return;
            }

            int outfitIndex = (int)parsed.NumericValue.Value;
            CharacterCustomization.SetCharacterOutfit(outfitIndex);
            LogInfo($"Outfit set to index {outfitIndex}");
        }

        private void HandleHatCommand(ParameterParser.ParsedParameters parsed)
        {
            if (!parsed.NumericValue.HasValue)
            {
                LogError("Usage: customize hat <index>");
                return;
            }

            if (!ParameterParser.ValidateIntegerRange(parsed.NumericValue, 0, 50, out string error))
            {
                LogError($"Invalid hat index: {error}");
                return;
            }

            int hatIndex = (int)parsed.NumericValue.Value;
            CharacterCustomization.SetCharacterHat(hatIndex);
            LogInfo($"Hat set to index {hatIndex}");
        }

        public override bool CanExecute()
        {
            return Character.localCharacter != null && !Character.localCharacter.data.dead;
        }
    }
}