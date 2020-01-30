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
    [CommandAttribute("eat", "Eat an edible item.", (int)Globals.eImpLevel.USER, 1, new string[] { "eat", "eat <item>" }, Globals.ePlayerState.PLAYING)]
    public class EatCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                return true;
            }

            if (!String.IsNullOrEmpty(args))
            {
                var food = chr.GetHeldItem(args);

                if (food != null)
                {
                    if (food.itemType == Globals.eItemType.Edible)
                    {
                        Food.EatFood(food, chr);
                        return true;
                    }

                    chr.WriteToDisplay("The " + food.name + " is not edible.");
                }
                else
                {
                    chr.WriteToDisplay("You are not holding that.");
                }
            }
            else
            {
                if (chr.RightHand != null && chr.RightHand.itemType == Globals.eItemType.Edible)
                {
                    Food.EatFood(chr.RightHand, chr);
                    chr.UnequipRightHand(chr.RightHand);
                }
                else if (chr.LeftHand != null && chr.LeftHand.itemType == Globals.eItemType.Edible)
                {
                    Food.EatFood(chr.LeftHand, chr);
                    chr.UnequipLeftHand(chr.LeftHand);
                }
                else
                {
                    chr.WriteToDisplay("You are not holding something to eat.");
                }
            }

            return true;
        }
    }
}
