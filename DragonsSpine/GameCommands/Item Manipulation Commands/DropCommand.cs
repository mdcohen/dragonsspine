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
    [CommandAttribute("drop", "Drop an item on the ground.", (int)Globals.eImpLevel.USER, new string[] { "dr" }, 1, new string[] { "drop <item>." }, Globals.ePlayerState.PLAYING)]
    public class DropCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            Item coins = null;

            if (args == null || args == "")
            {
                chr.WriteToDisplay("Drop what?");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            int ItemLoc = chr.WhichHand(args.ToLower());

            if (sArgs.Length < 2)
            {
                // if the item is in the player's hand
                if (ItemLoc == (int)Globals.eWearOrientation.Right || (sArgs[0] == "right" && chr.RightHand != null)) // Trap "Drop Right"
                {
                    if(!chr.RightHand.special.Contains(Item.EXTRAPLANAR))
                        chr.CurrentCell.Add(chr.RightHand);

                    chr.UnequipRightHand(chr.RightHand);
                }
                else if (ItemLoc == (int)Globals.eWearOrientation.Left || (sArgs[0] == "left" && chr.LeftHand != null)) // Trap "Drop Left"
                {
                    if (!chr.LeftHand.special.Contains(Item.EXTRAPLANAR))
                        chr.CurrentCell.Add(chr.LeftHand);
                    chr.UnequipLeftHand(chr.LeftHand);
                }
                else
                {
                    chr.WriteToDisplay("You are not carrying that.");
                }
            }
            else if (sArgs.Length == 2)
            {
                // drop # <item>
                if (sArgs[1] == "coins" || sArgs[1] == "coin")
                {
                    if (Convert.ToInt32(sArgs[0]) < 1)
                    {
                        return CommandTasker.ParseCommand(chr, "drop", "coins");
                    }

                    if (chr.RightHand != null && chr.RightHand.itemType == Globals.eItemType.Coin)
                    {
                        if (chr.RightHand.coinValue == Convert.ToInt32(sArgs[0]))
                        {
                            return CommandTasker.ParseCommand(chr, "drop", "coins");
                        }
                        else if (chr.RightHand.coinValue > Convert.ToInt32(sArgs[0]))
                        {
                            coins = Item.CopyItemFromDictionary(Item.ID_COINS);
                            chr.RightHand.coinValue -= Convert.ToInt32(sArgs[0]);
                            coins.coinValue = Convert.ToInt32(sArgs[0]);
                            chr.CurrentCell.Add(coins);
                            return true;
                        }
                        else
                        {
                            chr.WriteToDisplay("You are not holding that many coins.");
                            return true;
                        }
                    }
                    else if (chr.LeftHand != null && chr.LeftHand.itemType == Globals.eItemType.Coin)
                    {
                        if (chr.LeftHand.coinValue == Convert.ToInt32(sArgs[0]))
                        {
                            return CommandTasker.ParseCommand(chr, "drop", "coins");
                        }
                        else if (chr.LeftHand.coinValue > Convert.ToInt32(sArgs[0]))
                        {
                            coins = Item.CopyItemFromDictionary(Item.ID_COINS);
                            chr.LeftHand.coinValue -= Convert.ToInt32(sArgs[0]);
                            coins.coinValue = Convert.ToInt32(sArgs[0]);
                            chr.CurrentCell.Add(coins);
                            return true;
                        }
                        else
                        {
                            chr.WriteToDisplay("You are not holding that many coins.");
                            return true;
                        }
                    }
                    else
                    {
                        chr.WriteToDisplay("You are not holding any coins.");
                        return true;
                    }
                }
                else
                {
                    chr.WriteToDisplay("You are not carrying that many " + sArgs[1] + ".");
                    return true;
                }
            }
            else if (sArgs.Length > 2)
            {
                return CommandTasker.ParseCommand(chr, "put", args);
            }

            return true;
        }
    }
}
