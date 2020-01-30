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
using DragonsSpine.GameWorld;

namespace DragonsSpine.Commands
{
    [CommandAttribute("up", "Move up stairs.", (int)Globals.eImpLevel.USER, new string[] { "u" }, 2, new string[] { "There are no arguments for the up command." },
        Globals.ePlayerState.PLAYING)]
    public class UpCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr is NPC || (chr is PC && (chr as PC).ImpLevel > Globals.eImpLevel.USER) || chr.CommandWeight == 2)
            {
                #region Transfer to new map on Up
                if (chr.CurrentCell.CellGraphic.Equals("ZZ") || chr.CurrentCell.CellGraphic.Equals("XX"))
                {
                    Segue segue = chr.CurrentCell.Segue;// Segue.GetSegue1(chr.CurrentCell.LandID, chr.CurrentCell.MapID, chr.CurrentCell.X, chr.CurrentCell.Y);
                    if (segue != null)
                    {
                        chr.CurrentCell = Cell.GetCell(chr.FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                    }
                    return true;
                }
                #endregion

                #region Transfer to Random Map Up
                if (chr.CurrentCell.CellGraphic.Equals("RD") || chr.CurrentCell.CellGraphic.Equals("RU"))
                {

                    Segue segue = chr.CurrentCell.Segue;// Segue.GetSegue1(chr.CurrentCell.LandID, chr.CurrentCell.MapID, chr.CurrentCell.X, chr.CurrentCell.Y);
                    if (segue != null)
                    {
                        chr.CurrentCell = Cell.GetCell(chr.FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                    }
                    return true;
                }
                #endregion

                if (chr.CurrentCell.IsStairsUp)
                {
                    chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MoveUpStairs));
                    Map.MoveCharacter(chr, "up", args);
                }
                else
                {
                    chr.WriteToDisplay("You don't see any stairs leading up here.");
                }
            }

            return true;
        }
    }
}
