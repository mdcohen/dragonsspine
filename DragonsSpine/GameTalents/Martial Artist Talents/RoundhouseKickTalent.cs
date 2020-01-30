using System;

namespace DragonsSpine.Talents
{
    [TalentAttribute("roundhouse", "Roundhouse Kick", "Perform a powerful roundhouse kick to damage and possibly knockdown and stun your opponent.", false, 6, 321000, 16, 30, true, new string[] { },
        Character.ClassType.Martial_Artist)]
    public class RoundhouseKickTalent : ITalentHandler
    {
        public static int DamageMultiplier = 2;

        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Moderately)
            {
                chr.WriteToDisplay("You are too encumbered to perform a Roundhouse Kick.");
                return false;
            }

            Character target = GameSystems.Targeting.TargetAquisition.AcquireTarget(chr, args, 0, 0);

            // safety net
            if (target == null)
            {

                if (args.StartsWith("-") || Int32.TryParse(args, out int id))
                    chr.WriteToDisplay("You do not see your target here.");
                else chr.WriteToDisplay("You do not see a " + args + " here.");
                return false;
            }

            chr.CommandType = CommandTasker.CommandType.RoundhouseKick;
            Combat.DoCombat(chr, target, chr.GetInventoryItem(Globals.eWearLocation.Feet));
            chr.CommandsProcessed.RemoveAll(cmdType => cmdType == CommandTasker.CommandType.RoundhouseKick);

            return true;
        }
    }
}
