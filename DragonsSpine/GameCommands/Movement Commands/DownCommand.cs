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
    [CommandAttribute("down", "Move down stairs.", (int)Globals.eImpLevel.USER, new string[] { "d" }, 2, new string[] { "There are no arguments for the down command." },
        Globals.ePlayerState.PLAYING)]
    public class DownCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr is NPC || (chr is PC && (chr as PC).ImpLevel > Globals.eImpLevel.USER) || chr.CommandWeight == 2)
            {
                #region Transfer to new Map on Down
                if (chr.CurrentCell.CellGraphic.Equals("ZZ") || chr.CurrentCell.CellGraphic.Equals("XX"))
                {
                    Segue segue = chr.CurrentCell.Segue;// Segue.GetSegue1(chr.LandID, chr.MapID, chr.X, chr.Y);
                    if (segue != null)
                    {
                        chr.CurrentCell = Cell.GetCell(chr.FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z);
                    }
                    return true;
                }
                #endregion

                #region Transfer Player to Random Map
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

                if (chr.CurrentCell.IsStairsDown)
                {
                    chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MoveDownStairs));
                    Map.MoveCharacter(chr, "down", args);
                }
                else
                {
                    chr.WriteToDisplay("You don't see any stairs leading down here.");
                }
            }
            return true;
        }
    }
}
