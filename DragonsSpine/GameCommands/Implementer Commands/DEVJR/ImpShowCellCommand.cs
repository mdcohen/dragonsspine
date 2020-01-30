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
    [CommandAttribute("impshowcell", "Display cell variables and contents.", (int)Globals.eImpLevel.DEVJR, new string[] { },
        0, new string[] { "There are currently no arguments for this command." }, Globals.ePlayerState.PLAYING)]
    public class ImpShowCellCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.WriteToDisplay("X: " + chr.X + " Y: " + chr.Y + " Z: " + chr.Z);
            chr.WriteToDisplay("Cell Graphic: " + chr.CurrentCell.CellGraphic);
            chr.WriteToDisplay("Display Graphic: " + chr.CurrentCell.DisplayGraphic);
            chr.WriteToDisplay("Characters: " + chr.CurrentCell.Characters.Count);
            foreach (Character ch in chr.CurrentCell.Characters.Values)
                chr.WriteToDisplay(ch.GetLogString());
            chr.WriteToDisplay("AreaEffects: " + chr.CurrentCell.AreaEffects.Count);
            foreach (Effect.EffectTypes effectType in chr.CurrentCell.AreaEffects.Keys)
                chr.WriteToDisplay(effectType.ToString());
            chr.WriteToDisplay("Items: " + chr.CurrentCell.Items.Count);

            if (chr.CurrentCell.cellLock.CellLockString != "")
            {
                chr.WriteToDisplay("CellLock:");
                chr.WriteToDisplay(chr.CurrentCell.cellLock.CellLockString);
            }
            else chr.WriteToDisplay("CellLock: None");

            return true;
        }
    }
}
