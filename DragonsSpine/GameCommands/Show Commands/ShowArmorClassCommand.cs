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

namespace DragonsSpine.Commands
{
    [CommandAttribute("showac", "Display current armor class information.", (int)Globals.eImpLevel.USER, new string[] { "show ac", "show armorclass", "show armor class" },
        0, new string[] { "There are no arguments for the show armor class command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowArmorClassCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            Character character = chr;

            if (chr is PC && (chr as PC).ImpLevel >= Globals.eImpLevel.DEVJR && args != null && args.Length > 0 && args.ToLower() != "ac" && args.ToLower() != "armorclass" && args.ToLower() != "armor class")
            {
                character = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, args, true, true);

                if (character == null)
                {
                    chr.WriteToDisplay("Unable to find character: " + args);
                    return true;
                }
            }

            chr.WriteToDisplay("Armor Class Statistics");
            chr.WriteToDisplay("Base Armor Class           : " + character.baseArmorClass + " (Armor Class is NOT Armor Rating. Most humans/humanoids have base AC 10.)");
            chr.WriteToDisplay("Armor Rating               : " + Combat.AC_GetArmorClassRating(character));
            chr.WriteToDisplay("Shield Rating vs. Ranged   : " + Combat.AC_GetShieldingArmorClass(character, true));
            chr.WriteToDisplay("Shield Rating vs. Melee    : " + Combat.AC_GetShieldingArmorClass(character, false));
            chr.WriteToDisplay("Unarmed Armor Class Adj.   : " + Combat.AC_GetUnarmedArmorClassBonus(null, character));
            chr.WriteToDisplay("Dexterity Armor Class Adj. : " + Combat.AC_GetDexterityArmorClassBonus(null, character));
            chr.WriteToDisplay("Right Hand Armor Class     : " + Combat.AC_GetRightHandArmorClass(null, character));
            chr.WriteToDisplay("Left Hand Armor Class      : " + Combat.AC_GetLeftHandArmorClass(null, character));
            chr.WriteToDisplay("Some armor class information is not displayed here. Unarmed, dexterity, right and left hand AC is only applied against targets you are aware of.");

            return true;
        }
    }
}
