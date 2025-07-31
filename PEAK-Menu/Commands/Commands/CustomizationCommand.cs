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

            var option = parameters[0].ToLower();
            
            switch (option)
            {
                case "randomize":
                    if (Character.localCharacter?.refs?.customization != null)
                    {
                        Character.localCharacter.refs.customization.RandomizeCosmetics();
                        LogInfo("Character appearance randomized");
                    }
                    else
                    {
                        LogError("No character customization found");
                    }
                    break;

                case "rainbow":
                    var rainbowManager = Plugin.Instance?._menuManager?.GetRainbowManager();
                    if (rainbowManager != null)
                    {
                        if (parameters.Length > 1)
                        {
                            var action = parameters[1].ToLower();
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
                                    if (parameters.Length > 2 && float.TryParse(parameters[2], out float speed))
                                    {
                                        rainbowManager.SetRainbowSpeed(speed);
                                        LogInfo($"Rainbow speed set to {speed:F1}x");
                                        if (speed >= 4.0f)
                                            LogInfo("RAINBOW OVERDRIVE!");
                                    }
                                    else
                                    {
                                        LogError("Please provide a valid speed value");
                                    }
                                    break;
                                default:
                                    LogError($"Unknown rainbow action: {action}");
                                    LogInfo("Use 'help customize' for rainbow options");
                                    break;
                            }
                        }
                        else
                        {
                            // Toggle rainbow mode
                            rainbowManager.ToggleRainbow();
                            LogInfo($"Rainbow skin effect {(rainbowManager.IsRainbowEnabled ? "enabled" : "disabled")}");
                        }
                    }
                    else
                    {
                        LogError("Rainbow manager not available");
                    }
                    break;
                    
                case "skin":
                    if (parameters.Length > 1 && int.TryParse(parameters[1], out int skinIndex))
                    {
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
                    else
                    {
                        LogError("Please provide a valid skin index");
                    }
                    break;

                case "eyes":
                    if (parameters.Length > 1 && int.TryParse(parameters[1], out int eyeIndex))
                    {
                        CharacterCustomization.SetCharacterEyes(eyeIndex);
                        LogInfo($"Eyes set to index {eyeIndex}");
                    }
                    else
                    {
                        LogError("Please provide a valid eye index");
                    }
                    break;

                case "mouth":
                    if (parameters.Length > 1 && int.TryParse(parameters[1], out int mouthIndex))
                    {
                        CharacterCustomization.SetCharacterMouth(mouthIndex);
                        LogInfo($"Mouth set to index {mouthIndex}");
                    }
                    else
                    {
                        LogError("Please provide a valid mouth index");
                    }
                    break;

                case "accessory":
                    if (parameters.Length > 1 && int.TryParse(parameters[1], out int accessoryIndex))
                    {
                        CharacterCustomization.SetCharacterAccessory(accessoryIndex);
                        LogInfo($"Accessory set to index {accessoryIndex}");
                    }
                    else
                    {
                        LogError("Please provide a valid accessory index");
                    }
                    break;

                case "outfit":
                    if (parameters.Length > 1 && int.TryParse(parameters[1], out int outfitIndex))
                    {
                        CharacterCustomization.SetCharacterOutfit(outfitIndex);
                        LogInfo($"Outfit set to index {outfitIndex}");
                    }
                    else
                    {
                        LogError("Please provide a valid outfit index");
                    }
                    break;

                case "hat":
                    if (parameters.Length > 1 && int.TryParse(parameters[1], out int hatIndex))
                    {
                        CharacterCustomization.SetCharacterHat(hatIndex);
                        LogInfo($"Hat set to index {hatIndex}");
                    }
                    else
                    {
                        LogError("Please provide a valid hat index");
                    }
                    break;

                default:
                    LogError($"Unknown customization option: {option}");
                    LogInfo("Use 'help customize' for available options");
                    break;
            }
        }

        public override bool CanExecute()
        {
            return Character.localCharacter != null && !Character.localCharacter.data.dead;
        }
    }
}