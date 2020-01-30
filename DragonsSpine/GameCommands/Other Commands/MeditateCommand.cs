using System.Collections.Generic;

namespace DragonsSpine.Commands
{
    [CommandAttribute("meditate", "Place yourself into deep meditation.", (int)Globals.eImpLevel.USER, new string[] { "med" },
        3, new string[] { "There are no arguments for the meditate command." }, Globals.ePlayerState.PLAYING)]
    public class MeditateCommand : ICommandHandler
    {
        public static List<Effect.EffectTypes> NoMedOrRestEffects = new List<Effect.EffectTypes>()
        {
            Effect.EffectTypes.Fire,
            Effect.EffectTypes.Fire_Storm,
            Effect.EffectTypes.Dragon__s_Breath_Fire,
            Effect.EffectTypes.Dragon__s_Breath_Ice,
            Effect.EffectTypes.Dragon__s_Breath_Acid,
            Effect.EffectTypes.Dragon__s_Breath_Poison,
            Effect.EffectTypes.Dragon__s_Breath_Storm,
            Effect.EffectTypes.Dragon__s_Breath_Wind,
            Effect.EffectTypes.Ice,
        };

        public bool OnCommand(Character chr, string args)
        {
            // Command weight.
            if (chr.CommandsProcessed.Count > 0 || chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Meditation requires your utmost concentration and cannot be combined with other actions.");
                return true;
            }

            // Ensnared
            if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Ensnare))
            {
                chr.WriteToDisplay("You are ensnared and thus unable to focus on meditation.");
                return false;
            }

            foreach(Effect.EffectTypes effectType in chr.CurrentCell.AreaEffects.Keys)
            {
                if(NoMedOrRestEffects.Contains(effectType))
                {
                    chr.WriteToDisplay("You find it difficult to meditate in the " + Utils.FormatEnumString(effectType.ToString()) + ".");
                    return false;
                }
            }

            if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Contagion))
            {
                chr.WriteToDisplay("You are diseased and thus unable to focus on meditation.");
                return false;
            }

            if(chr.Poisoned > 0 )
            {
                chr.WriteToDisplay("You are poisoned and thus unable to focus on meditation.");
                return false;
            }

            if (chr.DamageRound > DragonsSpineMain.GameRound - 3)
            {
                chr.WriteToDisplay("You suffered damage recently and are unable to focus on meditation.");
                return false;
            }

            System.Collections.Generic.List<string> DisallowedMeditationCells = new System.Collections.Generic.List<string>()
            {
                GameWorld.Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL, GameWorld.Cell.GRAPHIC_OPEN_DOOR_VERTICAL
            };

            if(GameWorld.Map.IsNextToCounter(chr) ||
                DisallowedMeditationCells.Contains(chr.CurrentCell.DisplayGraphic) ||
                chr.CurrentCell.IsLair || chr.CurrentCell.IsLocker || chr.CurrentCell.IsOrnicLocker || chr.CurrentCell.IsMapPortal)
            {
                chr.WriteToDisplay("You find it difficult to meditate here.");
                return false;
            }

            if (new CommandTasker(chr)["rest"])
            {
                chr.CommandType = CommandTasker.CommandType.Meditate;
                chr.IsResting = true;
                chr.IsMeditating = true;

                // TODO who should know how to meditate? Make it an ability?
                chr.WriteToDisplay("You close your eyes, steady your breathing, and place yourself into a meditative trance.");
            }

            return true;
        }
    }
}
