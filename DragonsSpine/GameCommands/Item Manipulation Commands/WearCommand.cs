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
    [CommandAttribute("wear", "Wear an item.", (int)Globals.eImpLevel.USER, 1, new string[] { "wear <item>" }, Globals.ePlayerState.PLAYING)]
    public class WearCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Wear what?");
                return true;
            }

            bool canWear = true;

            Item tmpItem = chr.FindHeldItem(args);

            if (tmpItem == null)
            {
                chr.WriteToDisplay("You are not holding a " + args + ".");
                return true;
            }

            if (tmpItem.name == "ring")
            {
                chr.WriteToDisplay("Use \"put ring on # <left | right>\".");
                return true;
            }

            if (tmpItem.wearLocation == Globals.eWearLocation.None)
            {
                chr.WriteToDisplay("You cannot wear " + tmpItem.shortDesc + ".");
                return true;
            }

            if (tmpItem.IsAttunedToOther(chr))
            {
                chr.WriteToDisplay("The " + tmpItem.name + GameSystems.Text.TextManager.SOULDBOUND_TO_ANOTHER_AFFIX);
                return true;
            }

            string[] wearLocs = Enum.GetNames(typeof(Globals.eWearLocation));
            for (int a = 0; a < wearLocs.Length; a++)
            {
                if (wearLocs[a] == tmpItem.wearLocation.ToString())
                {
                    if (Globals.Max_Wearable.Length >= a + 1)
                    {
                        if (Globals.Max_Wearable[a] > 1)
                        {
                            chr.WriteToDisplay("Use \"put " + tmpItem.name + " on <left | right> " + tmpItem.wearLocation.ToString().ToLower() + "\".");
                            return true;
                        }
                    }
                }
            }

            foreach (Item item in chr.wearing)
            {
                if (item.wearLocation == tmpItem.wearLocation)
                {
                    canWear = false;
                    break;
                }
            }

            if (canWear)
            {
                if (chr.WearItem(tmpItem))
                {
                    // Check here for auto nocking wristbow.
                    if(tmpItem.baseType == Globals.eItemBaseType.Bow && tmpItem.returning && tmpItem.spell == (int)DragonsSpine.Spells.GameSpell.GameSpellID.Venom)
                    {
                        tmpItem.venom = tmpItem.spellPower;
                        tmpItem.charges -= 1;
                    }

                    if (tmpItem == chr.RightHand)
                        chr.UnequipRightHand(tmpItem);
                    else if (tmpItem == chr.LeftHand)
                        chr.UnequipLeftHand(tmpItem);
                }
            }
            else
                chr.WriteToDisplay("You are already wearing something there.");

            return true;
        }
    }
}
