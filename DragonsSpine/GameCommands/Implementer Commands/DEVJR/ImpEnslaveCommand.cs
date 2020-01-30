using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impenslave", "Forces an NPC to be your pet.", (int)Globals.eImpLevel.DEVJR, new string[] { "impens" },
        0, new string[] { "impwarm <spell command>" }, Globals.ePlayerState.PLAYING)]
    public class ImpEnslaveCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            Character target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, args, true, true);

            if (target == null)
            {
                chr.WriteToDisplay("You do not see " + args + " here.");
                return true;
            }

            if(!(target is NPC))
            {
                chr.WriteToDisplay("Sorry. You cannot enslave players. I know, it sucks.");
                return true;
            }

            target.canCommand = true;
            target.special += " enslaved";
            if(target.PetOwner != null)
                target.PetOwner.Pets.Remove(target as NPC);
            chr.Pets.Add(target as NPC);
            target.PetOwner = chr;

            chr.WriteToDisplay(target.GetNameForActionResult() + " has been enslaved. Use '" + target.Name + ", begone' to free " + Character.PRONOUN_2[(int)target.gender].ToLower() + ".");

            return true;
        }
    }
}
