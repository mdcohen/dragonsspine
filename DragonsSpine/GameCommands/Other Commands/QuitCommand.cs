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
    [CommandAttribute("quit", "Exit the game.", (int)Globals.eImpLevel.USER, new string[] { "exit" },
        2, new string[] { "There are no arguments for the quit command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class QuitCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.PCState == Globals.ePlayerState.CONFERENCE)
            {
                if (!chr.IsInvisible) // send exit message if character is not invisible
                {
                    (chr as PC).SendToAllInConferenceRoom(Conference.GetStaffTitle(chr as PC) + chr.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                }

                Utils.Log(chr.GetLogString(), Utils.LogType.Logout); // log the logout

                if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                    chr.WriteLine(ProtocolYuusha.LOGOUT);

                (chr as PC).RemoveFromConf();
                chr.RemoveFromServer();
                return true;
            }
            else if (chr.PCState != Globals.ePlayerState.PLAYING) // likely at one of the menus, currently 12/21/2013 no route here
            {
                Utils.Log(chr.GetLogString(), Utils.LogType.Logout);
                (chr as PC).RemoveFromMenu();
                chr.RemoveFromServer();
                return true;
            }

            if (chr.CommandWeight > 3)
            {
                return true;
            }

            chr.CommandType = CommandTasker.CommandType.Quit;

            if (chr.IsDead && chr is PC)
            {
                Rules.DeadRest(chr as PC);
                return true;
            }

            #region Cannot quit in a no recall zone
            if (chr.CurrentCell.IsNoRecall && (chr as PC).ImpLevel < Globals.eImpLevel.GM)
            {
                chr.WriteToDisplay("You cannot quit here.");
                return true;
            }
            #endregion

            #region Cannot quit next to an altar or counter
            if (GameWorld.Map.IsNextToCounter(chr) && (chr as PC).ImpLevel < Globals.eImpLevel.GM)
            {
                chr.WriteToDisplay("You cannot quit in front of a counter or an altar.");
                return true;
            }
            #endregion

            #region Cannot quit in front of lockers
            if (chr.CurrentCell.IsLocker && (chr as PC).ImpLevel < Globals.eImpLevel.GM)
            {
                chr.WriteToDisplay("You cannot quit in front of your locker.");
                return true;
            }
            #endregion

            #region Cannot quit on a teleport
            if (chr.CurrentCell.IsTeleport && (chr as PC).ImpLevel < Globals.eImpLevel.GM)
            {
                chr.WriteToDisplay("You cannot quit on a teleport.");
                return true;
            }
            #endregion

            if (chr.FindHeldItem("corpse") != null) // this works as long as long as corpses keep the same name
            {
                chr.WriteToDisplay("You cannot quit while holding a corpse.");
                return true;
            }

            if (chr.DamageRound <= DragonsSpineMain.GameRound - 3 ||
                DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Locked ||
                (chr as PC).ImpLevel > Globals.eImpLevel.USER)
            {
                chr.SendToAllInSight(chr.Name + " has left the world.");
                if (chr.protocol == "old-kesmai") { chr.Write(GameWorld.Map.KP_ENHANCER_DISABLE); }
                chr.RemoveFromWorld();
                chr.PCState = Globals.ePlayerState.CONFERENCE;
                (chr as PC).AddToConf();
                Conference.Header(chr as PC, true);
                PC pc = (PC)chr;
                System.Threading.Thread saveThread = new System.Threading.Thread(pc.Save);
                saveThread.Start();
                return true;
            }

            chr.WriteToDisplay("You must wait three rounds after taking damage to quit.");

            return true;
        }
    }
}
