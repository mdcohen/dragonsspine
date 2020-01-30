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
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Make_Recall, "makerecall", "Make Recall", "Enchant a gold ring with recall magic.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Self, 6, 4, 600, "", false, true, false, false, false, Character.ClassType.Thief)]
    public class MakeRecallSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            if (caster.RightHand != null && caster.RightHand.itemID == Item.ID_GOLDRING)
            {
                caster.UnequipRightHand(caster.RightHand);
                caster.EquipRightHand(Item.CopyItemFromDictionary(Item.ID_RECALLRING));
            }
            else if (caster.LeftHand != null && caster.LeftHand.itemID == Item.ID_GOLDRING)
            {
                caster.UnequipLeftHand(caster.LeftHand);
                caster.EquipLeftHand(Item.CopyItemFromDictionary(Item.ID_RECALLRING));
            }
            else
            {
                if (ConfigurationManager.AppSettings["RequireMakeRecallReagent"].ToLower() == "false")
                {
                    Item recallRing = Item.CopyItemFromDictionary(Item.ID_RECALLRING);
                    recallRing.coinValue = 0; // to avoid exploitation
                    caster.EquipEitherHandOrDrop(recallRing);
                }
                else
                {
                    if (caster.RightHand != null)
                    {
                        caster.WriteToDisplay("Your " + caster.RightHand.name + " explodes!");
                        caster.RightHand = null;
                        Combat.DoSpellDamage(null, caster, null, Rules.Dice.Next(1, 20), "concussion");
                    }
                    else if (caster.LeftHand != null)
                    {
                        caster.WriteToDisplay("Your " + caster.LeftHand.name + " explodes!");
                        caster.LeftHand = null;
                        Combat.DoSpellDamage(null, caster, null, Rules.Dice.Next(1, 20), "concussion");
                    }
                    else
                    {
                        //GenericFailMessage(caster, "");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
