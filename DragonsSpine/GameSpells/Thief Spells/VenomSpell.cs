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
    [SpellAttribute(GameSpell.GameSpellID.Venom, "venom", "Venom", "Coat a piercing weapon with a caustic venom.",
        Globals.eSpellType.Necromancy, Globals.eSpellTargetType.Self, 22, 13, 10000, "", false, true, false, false, false, Character.ClassType.Thief)]
    public class VenomSpell : ISpellHandler
    {
        public static double VenomDamageMultiplier = 1.7;

        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            // Right hand, piercing weapon. Includes wristbow gauntlets.
            if (caster.RightHand != null && caster.RightHand.IsPiercingWeapon())
            {
                // Check if item should have charges of venom.
                if (caster.RightHand.spell == (int)GameSpell.GameSpellID.Venom)
                {
                    Item item = Item.CopyItemFromDictionary(caster.RightHand.itemID);
                    // Get the amount of charges the venom spell item had.
                    if (item.charges >= Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        caster.WriteToDisplay("The " + caster.RightHand.name + " has unlimited charges of " + ReferenceSpell.Name + ".");
                        return false;
                    }

                    if (item.charges > caster.RightHand.charges)
                    {
                        caster.WriteToDisplay("You have added a charge of " + ReferenceSpell.Name + " to your " + caster.RightHand.name + ".");
                        caster.RightHand.charges += 1;
                        if (ReferenceSpell.IsClassSpell(caster.BaseProfession))
                            Skills.GiveSkillExp(caster, Skills.GetSkillLevel(caster.magic) * ReferenceSpell.ManaCost, Globals.eSkillType.Magic);
                        return true;
                    }
                }

                if (caster.RightHand.venom > 0)
                {
                    caster.WriteToDisplay("The " + caster.RightHand.name + " is already envenomed.");
                    return false;
                }

                //if(caster.RightHand.baseType == Globals.eItemBaseType.Bow && !caster.RightHand.IsNocked)
                //{
                //    caster.WriteToDisplay("The " + caster.RightHand.name + " must be nocked before applying venom.");
                //    return false;
                //}

                caster.WriteToDisplay("The " + caster.RightHand.name + " drips with a caustic venom.");
                caster.RightHand.venom = (int)(Skills.GetSkillLevel(caster.magic) * VenomDamageMultiplier);
            }
            // Left hand, piercing weapon. Includes wristbow gauntlets.
            else if (caster.LeftHand != null && caster.LeftHand.IsPiercingWeapon())
            {
                // Check if item should have charges of venom.
                if (caster.LeftHand.spell == (int)GameSpell.GameSpellID.Venom)
                {
                    Item item = Item.CopyItemFromDictionary(caster.LeftHand.itemID);
                    // Get the amount of charges the venom spell item had.
                    if (item.charges >= Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        caster.WriteToDisplay("The " + caster.LeftHand.name + " has unlimited charges of " + ReferenceSpell.Name + ".");
                        return false;
                    }

                    if (item.charges > caster.LeftHand.charges)
                    {
                        caster.WriteToDisplay("You have added a charge of " + ReferenceSpell.Name + " to your " + caster.RightHand.name + ".");
                        caster.LeftHand.charges += 1;
                        if (ReferenceSpell.IsClassSpell(caster.BaseProfession))
                            Skills.GiveSkillExp(caster, Skills.GetSkillLevel(caster.magic) * ReferenceSpell.ManaCost, Globals.eSkillType.Magic);
                        return true;
                    }
                }

                if (caster.LeftHand.venom > 0)
                {
                    caster.WriteToDisplay("The " + caster.LeftHand.name + " is already envenomed.");
                    return false;
                }

                //if (caster.LeftHand.baseType == Globals.eItemBaseType.Bow && !caster.LeftHand.IsNocked)
                //{
                //    caster.WriteToDisplay("The " + caster.LeftHand.name + " must be nocked before applying venom.");
                //    return false;
                //}

                // Check if wristbow auto nocking gauntlets.

                caster.WriteToDisplay("The " + caster.LeftHand.name + " drips with a caustic venom.");
                caster.LeftHand.venom = (int)(Skills.GetSkillLevel(caster.magic) * VenomDamageMultiplier);
            }
            else
            {
                caster.WriteToDisplay(GameSystems.Text.TextManager.YOUR_SPELL_FAILS);
                caster.WriteToDisplay("You can only cast venom on piercing weapons.");
                return false;
            }

            if (ReferenceSpell.IsClassSpell(caster.BaseProfession))
                Skills.GiveSkillExp(caster, Skills.GetSkillLevel(caster.magic) * ReferenceSpell.ManaCost, Globals.eSkillType.Magic);

            return true;
        }
    }
}
