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
namespace DragonsSpine.Commands
{
    [CommandAttribute("impreloaditems", "Reload the item catalog. Use after adding new rows to CatalogItem.", (int)Globals.eImpLevel.DEVJR, new string[] { },
        0, new string[] { "There are no arguments for the impreloaditems command" }, Globals.ePlayerState.CONFERENCE)]
    public class ImpReloadItemsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            //if(PC.PCInGameWorld.Count > 1)
            //{
            //    chr.WriteToDisplay("There are players in the game world. Command failed.");
            //    return false;
            //}

            DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Locked;
            chr.WriteToDisplay("Game world has been locked.", ProtocolYuusha.TextType.System);

            chr.WriteToDisplay("Reloading item catalog...", ProtocolYuusha.TextType.System);
            chr.WriteToDisplay("Current count of Items: " + Item.ItemDictionary.Count);

            Item.ItemDictionary.Clear();

            DAL.DBItem.LoadItems();

            chr.WriteToDisplay("Item catalog reloaded.", ProtocolYuusha.TextType.System);
            chr.WriteToDisplay("Current count of Items: " + Item.ItemDictionary.Count);

            DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Running;
            chr.WriteToDisplay("Game world has been unlocked.", ProtocolYuusha.TextType.System);

            return true;
        }
    }
}
