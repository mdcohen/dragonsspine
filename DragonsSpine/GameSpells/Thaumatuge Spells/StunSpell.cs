using System;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Stun, "stun", "Stun", "Target becomes stunned, unable to move or perform any action.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single_or_Group, 4, 4, 100, "0229", false, true, false, false, false, Character.ClassType.Thaumaturge)]
    public class StunSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            try
            {
                Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

                if (target == null) { return false; }

                if (target.Stunned > 1)
                {
                    caster.WriteToDisplay(target.GetNameForActionResult() + " is already stunned.");
                    return true;
                }

                ReferenceSpell.SendGenericCastMessage(caster, target, true);

                int modifier = caster.Level - target.Level;

                if (!Combat.DND_CheckSavingThrow(target, Combat.SavingThrow.ParalyzationPoisonDeath, modifier - target.StunResistance))
                //if (Combat.DoSpellDamage(caster, target, null, 0, "stun") == 1)
                {
                    target.WriteToDisplay("You are stunned!");

                    if (target.preppedSpell != null)
                    {
                        target.preppedSpell = null;
                        target.WriteToDisplay("Your spell has been lost.");
                    }

                    //stun duration is random rounds from 1 to magic skill level divided by 2
                    short stunAmount = (short)(Rules.Dice.Next(1, Skills.GetSkillLevel(caster.magic) / 2) + 1);

                    target.Stunned = stunAmount;

                    short stunCount = 1;

                    if (target.Group != null)
                    {
                        foreach (NPC npc in target.Group.GroupNPCList)
                        {
                            if (npc != target)
                            {
                                target.Stunned = stunAmount;

                                stunCount++;
                            }
                        }
                    }

                    if (stunCount == 1)
                        target.SendToAllInSight(target.GetNameForActionResult() + " is stunned.");
                    else target.SendToAllInSight(stunCount + " " + GameSystems.Text.TextManager.Multinames(target.Name) + " are stunned.");
                }
                else ReferenceSpell.SendGenericResistMessages(caster, target);
                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }
    }
}
