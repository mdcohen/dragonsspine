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
namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Cage_Soul, "cagesoul", "Cage Soul", "Suck a soul out of a living entity and store it in a piece of vengeful tsavorite.",
        Globals.eSpellType.Abjuration, Globals.eSpellTargetType.Single, 63, 13, 1200000, "0272", false, false, true, false, false, Character.ClassType.Sorcerer)]
    public class CageSoulSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell { get; set; }

        public bool OnCast(Character caster, string args)
        {
            var target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);

            if (target == null) { return false; }

            ReferenceSpell.SendGenericCastMessage(caster, target, true);

            // Cannot capture a player's soul.
            if(target.IsPC)
            {
                caster.WriteToDisplay("You cannot cage souls of players...yet.");
                return true;
            }

            // Does not work on animals.
            if (target.animal)
            {
                caster.WriteToDisplay(ReferenceSpell.Name + " does not work on animals.");
                return true;
            }

            // Does not work on figurines or summoned beings.
            if(target.special.Contains("figurine") || ((target is NPC) && (target as NPC).IsSummoned))
            {
                caster.WriteToDisplay(ReferenceSpell.Name + " does not work on beings from other planes of existence.");
                return true;
            }

            if(target.PetOwner != null)
            {
                caster.WriteToDisplay(ReferenceSpell.Name + " does not work on pets.");
                return true;
            }

            // Does not work on lair creatures.
            if(target is NPC && (target as NPC).lairCritter)
            {
                caster.WriteToDisplay(ReferenceSpell.Name + " does not work on lair creatures.");
                return true;
            }

            // Undead have no souls.
            if (target.IsUndead)
            {
                caster.WriteToDisplay("The undead do not have souls.");
                return true;
            }

            // Images have no soul.
            if (target.IsImage)
            {
                caster.WriteToDisplay("The projected image does not have a soul.");
                return true;
            }

            // Verify target is not too powerful to cage.
            if (target.Level > caster.Level - 2)
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " is too powerful for you to cage " +
                                      Character.POSSESSIVE[(int)target.gender].ToLower() + " soul.");
                return true;
            }

            // Target must be at or below 50% health.
            if(target.Hits > (target.HitsMax / 2))
            {
                caster.WriteToDisplay(target.GetNameForActionResult() + " must be weakened further before caging " + Character.POSSESSIVE[(int)target.gender].ToLower() + " soul.");
                return true;
            }

            var hand = (int)Globals.eWearOrientation.None;
            var tsavorite = caster.GetHeldItem(Item.ID_DAZZLING_TSAVORITE, out hand); // unequips the item

            // No tsavorite found.
            if(tsavorite == null)
            {
                caster.WriteToDisplay("You are not holding the required material component.");
                return true;
            }

            // Check value of tsavorite.
            if (tsavorite.coinValue / target.Level < 1000)
            {
                caster.WriteToDisplay("Your " + tsavorite.name + " is too weak to contain " + target.GetNameForActionResult(true) + ".");
                caster.WriteToDisplay("The " + tsavorite.name + " explodes!");
                Combat.DoDamage(caster, target, target.Level * Rules.RollD(1, 4), false);
                return true;
            }

            // Create the soul gem.
            var soulGem = new SoulGem(Item.ItemDictionary[tsavorite.itemID]);

            soulGem.Soul = target;
            soulGem.coinValue = Rules.RollD(2, 100);
            soulGem.fragile = true;
            soulGem.longDesc = "a chunk of vengeful green tsavorite";
            soulGem.name = "soulgem";

            caster.WriteToDisplay("You cage " + target.GetNameForActionResult(true) + @"\'s soul.");

            // Message sent to all in sight.
            caster.SendToAllInSight(caster.GetNameForActionResult() + " cages " + target.GetNameForActionResult(true) + @"\'s soul.");
            
            Skills.GiveSkillExp(caster, target, Globals.eSkillType.Magic);

            // The physical body dies.
            Rules.DoDeath(target, caster);

            // Put the Soul Gem in the caster's hand.
            if (hand == (int)Globals.eWearOrientation.Right)
                caster.EquipRightHand(soulGem);
            else if (hand == (int)Globals.eWearOrientation.Left) caster.EquipLeftHand(soulGem);

            return true;
        }
    }
}
