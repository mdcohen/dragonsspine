#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Heal_Servant, "healservant", "Heal Servant", "Channel your own lifeforce into one of your pets to heal them.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Single, 8, 12, 55000, "0233", false, false, true, false, false, Character.ClassType.Sorcerer)]
    public class HealServantSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            // fail to heal a target that is not a pet
            if (!(target is NPC) || !caster.Pets.Contains(target as NPC))
            {
                caster.WriteToDisplay("The target of your " + ReferenceSpell.Name + " spell is not connected to you.");
                return false;
            }

            // perhaps we should allow the caster to kill themselves by channeling lifeforce?
            if (caster.HitsFull < 12)
            {
                caster.WriteToDisplay("You are too weak to channel lifeforce to your servant.");
                return false;
            }

            int healAmount = target.HitsMax - target.Hits; //caster.HitsFull / 2;

            if(healAmount <= 0)
            {
                caster.WriteToDisplay("Your servant is not damaged.");
                return false;
            }

            int lifeforceChannelled = healAmount;

            // halve the heal amount if diseased
            if (target.EffectsList.ContainsKey(Effect.EffectTypes.Contagion))
                healAmount = healAmount / 2;

            // 0 means the caster survived the lifeforce channel
            if (Combat.DoSpellDamage(caster, caster, null, lifeforceChannelled, "lifeforce drain") == 0)
            {
                target.Hits += healAmount;

                if (target.Hits > target.HitsMax) { target.Hits = target.HitsMax; }

                ReferenceSpell.SendGenericCastMessage(caster, target, true);

                caster.WriteToDisplay("You channel your lifeforce to heal your servant.");
                target.WriteToDisplay("You have been healed.");
            }

            return true;
        }
    }
}
