namespace PEAK_Menu.Commands
{
    public class CustomizationCommand : BaseCommand
    {
        public override string Name => "customize";
        public override string Description => "Character customization commands";

        public override void Execute(string[] parameters)
        {
            if (parameters.Length == 0)
            {
                LogInfo("Usage: customize <option> [value]");
                LogInfo("Options: skin, eyes, mouth, accessory, outfit, hat, randomize");
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
                    
                case "skin":
                    if (parameters.Length > 1 && int.TryParse(parameters[1], out int skinIndex))
                    {
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
                    LogInfo("Valid options: skin, eyes, mouth, accessory, outfit, hat, randomize");
                    break;
            }
        }

        public override bool CanExecute()
        {
            return Character.localCharacter != null && !Character.localCharacter.data.dead;
        }
    }
}