namespace DragonsSpine.Talents
{
    [TalentAttribute("bash", "Shield Bash", "Bash an opponent with a shield to damage and possibly stun them.", false, 1, 5000, 9, 10, true, new string[] { "bash <target>"}, Character.ClassType.Fighter )]
    public class ShieldBashTalent : ITalentHandler
    {
        private const int MOVE_AND_BASH_LEVEL = 9;  // Skill level 9 = Proficient

        public static bool MeetsRequirements(Character chr, Character target)
        {
            // if first command is movement and bash skill level is less than 12 (Expert) then fail a move and bash attempt
            if (chr.CommandsProcessed.Contains(CommandTasker.CommandType.Movement) && Skills.GetSkillLevel(chr.bash) < MOVE_AND_BASH_LEVEL)
            {
                chr.WriteToDisplay("You are not talented enough yet to move and bash simultaneously.");
                return false;
            }

            if (chr.LeftHand == null)
            {
                chr.WriteToDisplay("You cannot bash if you do not have a shield equipped in your left hand.");
                return false;
            }

            if (chr.LeftHand.baseType != Globals.eItemBaseType.Shield)
            {
                chr.WriteToDisplay("You cannot bash with " + chr.LeftHand.shortDesc + ".");
                return false;
            }
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Bash what?");
                return false;
            }

            if (!MeetsRequirements(chr, null))
                return false;

            Item shield = chr.LeftHand;

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = GameSystems.Targeting.TargetAquisition.AcquireTarget(chr, args, 0, 0);

            if (target == null)
            {
                chr.WriteToDisplay("You don't see a " + sArgs[0] + " here.");
                return false;
            }

            if (target is Merchant)
            {
                if ((target as Merchant).trainerType > Merchant.TrainerType.None)
                {
                    chr.WriteToDisplay("You cannot bash a trainer...yet.");
                    return false;
                }
                else
                {
                    chr.WriteToDisplay("You cannot bash a merchant...yet.");
                    return false;
                }
            }

            if (shield.IsAttunedToOther(chr)) // check if a weapon is attuned
            {
                chr.CurrentCell.Add(shield);
                chr.WriteToDisplay("The " + shield.name + " leaps from your hand!");
                chr.UnequipLeftHand(shield);
                return false;
            }

            if (!shield.AlignmentCheck(chr)) // check if a weapon has an alignment
            {
                chr.CurrentCell.Add(shield);
                chr.WriteToDisplay("The " + shield.name + " singes your hand and falls to the ground!");
                chr.UnequipLeftHand(shield);
                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                return false;
            }

            chr.CommandType = CommandTasker.CommandType.Shield_Bash;
            Combat.DoCombat(chr, target, shield);

            return true;
        }
    }
}
