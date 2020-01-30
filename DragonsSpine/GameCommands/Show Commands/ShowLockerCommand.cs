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
using System.Collections;

namespace DragonsSpine.Commands
{
    [CommandAttribute("showlocker", "Display a list of locker items.", (int)Globals.eImpLevel.USER, new string[] { "show locker" },
        0, new string[] { "There are no arguments for the show locker command." }, Globals.ePlayerState.PLAYING)]
    public class ShowLockerCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CurrentCell.IsLocker)
            {
                if (chr.lockerList.Count > 0)
                {
                    int z = 0, i = 0;
                    string dispMsg = "";
                    double itemcount = 0;
                    bool moreThanOne = false;
                    ArrayList templist = new ArrayList();
                    Item[] itemList = new Item[chr.lockerList.Count];
                    chr.lockerList.CopyTo(itemList);
                    foreach (Item item in itemList)
                    {
                        templist.Add(item);
                    }
                    z = templist.Count - 1;
                    dispMsg = "In your locker you see ";
                    while (z >= 0)
                    {

                        Item item = (Item)templist[z];

                        itemcount = 0;
                        for (i = templist.Count - 1; i > -1; i--)
                        {
                            Item tmpitem = (Item)templist[i];
                            if (tmpitem.name == item.name && tmpitem.name == "coins")
                            {
                                templist.RemoveAt(i);
                                itemcount = itemcount + (double)item.coinValue;
                                z = templist.Count;

                            }
                            else if (tmpitem.name == item.name)
                            {
                                templist.RemoveAt(i);
                                z = templist.Count;
                                itemcount += 1;
                            }

                        }
                        if (itemcount > 0)
                        {
                            if (moreThanOne)
                            {
                                if (z == 0)
                                {
                                    dispMsg += " and ";
                                }
                                else
                                {
                                    dispMsg += ", ";
                                }
                            }
                            dispMsg += GameSystems.Text.TextManager.ConvertNumberToString(itemcount) + Item.GetLookShortDesc(item, itemcount);

                        }
                        moreThanOne = true;
                        z--;
                    }
                    dispMsg += ".";
                    chr.WriteToDisplay(dispMsg);
                }
                else
                { chr.WriteToDisplay("Your locker is empty."); }
            }
            else
            {
                chr.WriteToDisplay("Your locker is not here.");
            }

            return true;
        }
    }
}
