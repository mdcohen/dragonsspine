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
    [CommandAttribute("quaff", "Grab a balm from your sack, open it and drink it. The empty bottle is destroyed.", (int)Globals.eImpLevel.USER, new string[] { "qua", "quaf" },
        0, new string[] { "There are no arguments for the quaff command." }, Globals.ePlayerState.PLAYING)]
    public class QuaffCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            Bottle bottle;

            try
            {
                if (args == null || args == "")
                {
                    if (chr.RightHand != null && chr.RightHand.itemID == Item.ID_BALM)
                    {
                        bottle = (Bottle)chr.RightHand;

                        if (!bottle.IsEmpty())
                        {
                            Bottle.OpenBottle(bottle, chr);
                            Bottle.DrinkBottle(bottle, chr);
                            chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.BreakingGlass));
                            chr.UnequipRightHand(chr.RightHand);
                            return true;
                        }
                    }

                    if (chr.LeftHand != null && chr.LeftHand.itemID == Item.ID_BALM)
                    {
                        bottle = (Bottle)chr.LeftHand;

                        if (!bottle.IsEmpty())
                        {
                            Bottle.OpenBottle(bottle, chr);
                            Bottle.DrinkBottle(bottle, chr);
                            chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.BreakingGlass));
                            chr.UnequipLeftHand(chr.LeftHand);
                            return true;
                        }
                    }

                    int balmCount = 0;
                    bool drankBalm = false;
                    foreach (Item item in chr.sackList)
                    {
                        if (item.itemID == Item.ID_BALM)
                        {
                            bottle = (Bottle)item;

                            if (!bottle.IsEmpty())
                            {
                                drankBalm = true;
                                Bottle.OpenBottle(bottle, chr);
                                Bottle.DrinkBottle(bottle, chr);
                                chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.BreakingGlass));
                                chr.RemoveFromSack(item);
                                return true;
                            }
                            else balmCount++;
                        }
                    }

                    if(drankBalm)
                    {
                        if (balmCount == 1) chr.WriteToDisplay("You have only 1 balm remaining.");
                        return true;
                    }

                    chr.WriteToDisplay("You don't have any balms to quaff. Good luck.");
                }
                // TODO: else if supplied a worldItemID
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return true;
        }
    }
}
