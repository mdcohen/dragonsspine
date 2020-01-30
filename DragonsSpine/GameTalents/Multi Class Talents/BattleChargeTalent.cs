using System;
using eItemBaseType = DragonsSpine.Globals.eItemBaseType;

namespace DragonsSpine.Talents
{
    /// <summary>
    /// The battle charge talent is an activated talent.
    /// </summary>
    [TalentAttribute("charge", "Battle Charge", "Charge a nearby opponent with weapons drawn.", false, 5, 155000, 13, 5, true, new string[] { },
        Character.ClassType.Berserker, Character.ClassType.Fighter, Character.ClassType.Knight, Character.ClassType.Ranger, Character.ClassType.Ravager)]
    public class BattleChargeTalent : ITalentHandler
    {
        public static int DamageMultiplier = 3;

        private static eItemBaseType[] AllowedItemBaseTypes = new eItemBaseType[]
        {
            eItemBaseType.Flail,
            eItemBaseType.Halberd,
            eItemBaseType.Mace,
            eItemBaseType.Rapier,
            eItemBaseType.Staff,
            eItemBaseType.Sword,
            eItemBaseType.Threestaff,
            eItemBaseType.TwoHanded,
            eItemBaseType.Shield
        };

        public static bool MeetsRequirements(Character chr, Character target)
        {
            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Moderately)
            {
                chr.WriteToDisplay("You are too encumbered to charge into battle.");
                return false;
            }

            Item weapon = chr.RightHand;

            if (weapon == null)
                return false;

            if (weapon.itemType != Globals.eItemType.Weapon)
                return false;

            if (Array.IndexOf(AllowedItemBaseTypes, weapon.baseType) == -1)
                return false;

            if (weapon.IsAttunedToOther(chr)) // check if a weapon is attuned
                return false;

            if (!weapon.AlignmentCheck(chr)) // check if a weapon has an alignment
                return false;

            return true;
        }

        // args: left/right targetID
        public bool OnPerform(Character chr, string args)
        {
            if(args == null || args == "")
            {
                chr.WriteToDisplay("Who do you want to Battle Charge?");
                return false;
            }

            if (!MeetsRequirements(chr, null))
                return false;            

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = GameSystems.Targeting.TargetAquisition.AcquireTarget(chr, args, GameWorld.Cell.DEFAULT_VISIBLE_DISTANCE, 0);

            // safety net
            if (target == null)
                return false;

            if (target.CurrentCell != chr.CurrentCell)
            {
                if (!PathTest.SuccessfulPathTest(PathTest.RESERVED_NAME_JUMPKICKCOMMAND, chr.CurrentCell, target.CurrentCell))
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.PATH_IS_BLOCKED);
                    return false;
                }
            }

            if (GameWorld.Cell.GetCellDistance(chr.X, chr.Y, target.X, target.Y) < 1)
            {
                chr.WriteToDisplay("Your target is too close for you to charge into battle.");
                return false;
            }

            chr.WriteToDisplay("You charge into battle!");
            target.WriteToDisplay(chr.GetNameForActionResult() + " charges you!");

            chr.CommandType = CommandTasker.CommandType.BattleCharge;
            chr.CurrentCell = target.CurrentCell;
            Combat.DoCombat(chr, target, chr.RightHand);
            // TODO: emit battle charge sound

            // Battle charge with a shield does an automatic bash.
            if (chr.HasTalent(GameTalent.TALENTS.Bash) && chr.LeftHand != null && chr.LeftHand.baseType == eItemBaseType.Shield)
            {
                GameTalent gtShieldBash = GameTalent.GameTalentDictionary[GameTalent.TALENTS.Bash.ToString().ToLower()];

                if (gtShieldBash.MeetsPerformanceCost(chr) && gtShieldBash.Handler.OnPerform(chr, args))
                {
                    gtShieldBash.SuccessfulPerformance(chr);
                }
            }

            return true;
        }        
    }
}
