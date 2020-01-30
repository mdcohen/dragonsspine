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
    [CommandAttribute("ust", "Display a list of everyone online.", (int)Globals.eImpLevel.USER, new string[]{ "who", "user", "users" }, 0,
        new string[] { "" }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class WhoCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            string nameOfChar = "";
            string nameOfMap = "";
            string nameOfRoom = "";
            string nameOfClass = "";
            //string levelInfo = "";
            bool isAnonymous = false;
            string anonymous = "";
            bool isVisible = true;
            string visibility = "";
            string zPlane = "Unknown";

            var usersList = new System.Collections.Generic.List<string>();

            try
            {
                foreach (PC pc in Character.MenuList)
                {
                    #region Menu
                    isAnonymous = pc.IsAnonymous;
                    isVisible = !pc.IsInvisible;
                    //make sure all players are visible to impLevel 5+
                    if (chr is PC && (chr as PC).ImpLevel >= Globals.eImpLevel.GM)
                    {
                        isAnonymous = false;
                        isVisible = true;
                        // flag an impLevel that is anonymous or invisible
                        anonymous = "";
                        if (pc.IsAnonymous) { anonymous = "[ANON]"; }
                        else if (pc.ImpLevel > Globals.eImpLevel.USER) { anonymous = Conference.GetStaffTitle(pc); }
                        else { anonymous = ""; }
                        if (pc.IsInvisible) { visibility = "[INVIS]"; }
                        else { visibility = ""; }
                    }

                    if (pc.IsPC && isVisible)
                    {
                        nameOfChar = Conference.GetStaffTitle(pc) + pc.Name;
                        if (pc.afk) { nameOfChar = nameOfChar + "(AFK)"; }

                        nameOfRoom = Conference.GetUserLocation(pc);

                        if (pc.Name == "Nobody")
                        {
                            nameOfChar = "Nobody";
                            nameOfRoom = "CharGen";
                        }

                        if (isAnonymous)
                            nameOfClass = "ANONYMOUS";
                        else
                        {
                            //levelInfo = "[" + pc.Level.ToString() + "]";
                            //nameOfClass = levelInfo.PadRight(5) + pc.classFullName;
                            nameOfClass = pc.classFullName;
                        }
                        //count++;
                        //ch.WriteLine(count + " " + nameOfChar + spaces.Substring(0, spaces.Length - nameOfChar.Length) + nameOfRoom + spaces.Substring(0, spaces.Length - nameOfRoom.Length) + nameOfClass + " " + anonymous + visibility, Protocol.TextType.Listing);
                        //chr.WriteLine(count + " " + nameOfChar.PadRight(25, ' ') + nameOfRoom.PadRight(20, ' ') + nameOfClass.PadRight(20, ' ') + anonymous + visibility, Protocol.TextType.Listing);
                        usersList.Add(nameOfChar.PadRight(25, ' ') + nameOfRoom.PadRight(20, ' ') + nameOfClass.PadRight(20, ' ') + anonymous + visibility);
                    } 
                    #endregion
                }

                foreach (PC pc in Character.ConfList)
                {
                    #region Conference
                    isAnonymous = pc.IsAnonymous;
                    isVisible = !pc.IsInvisible;
                    //make sure all players are visible to impLevel 5+
                    if (chr is PC && (chr as PC).ImpLevel >= Globals.eImpLevel.GM)
                    {
                        isAnonymous = false;
                        isVisible = true;
                        //flag an impLevel that is anonymous or invisible
                        anonymous = "";

                        if (pc.IsAnonymous)
                        {
                            anonymous = "[ANON]";
                        }
                        else if (pc.ImpLevel > Globals.eImpLevel.USER)
                        {
                            anonymous = Conference.GetStaffTitle(pc);
                        }

                        if (pc.IsInvisible) { visibility = "[INVIS]"; }
                        else { visibility = ""; }
                    }

                    if (pc is PC && isVisible)
                    {
                        nameOfChar = Conference.GetStaffTitle(pc) + pc.Name;

                        if (pc.afk) { nameOfChar = nameOfChar + "(AFK)"; }

                        nameOfRoom = Conference.GetUserLocation(pc);

                        if (pc.Name == "Nobody")
                        {
                            nameOfChar = "Nobody";
                            nameOfRoom = "CharGen";
                        }

                        nameOfClass = isAnonymous ? "ANONYMOUS" : pc.classFullName;
                        zPlane = isAnonymous ? "ANON" : pc.Map.GetZName(pc.Z);
                        
                        usersList.Add(nameOfChar.PadRight(25, ' ') + nameOfRoom.PadRight(20, ' ') + nameOfClass.PadRight(20, ' ') + anonymous + visibility);
                    } 
                    #endregion
                }

                foreach (Adventurer adv in Character.AdventurersInGameWorldList)
                {
                    #region Adventurers
                    nameOfChar = adv.Name;
                    nameOfMap = adv.Map.Name;
                    //levelInfo = "[" + adv.Level.ToString() + "]";
                    //nameOfClass = levelInfo.PadRight(5) + adv.classFullName;
                    nameOfClass = adv.classFullName;
                    //count++;

                    //string final = count + " " + nameOfChar.PadRight(25) + nameOfMap.PadRight(20) + nameOfClass.PadRight(20);
                    string final = nameOfChar.PadRight(25) + nameOfMap.PadRight(20) + nameOfClass.PadRight(20);

                    if (adv.IsDead && !isAnonymous)
                        final += "(dead)";
                    if (chr is PC && (chr as PC).ImpLevel >= Globals.eImpLevel.GM)
                        final += "[ADV]";

                    zPlane = isAnonymous ? "ANON" : adv.Map.GetZName(adv.Z);

                    //chr.WriteLine(final, Protocol.TextType.Listing);
                    usersList.Add(final); 
                    #endregion
                }

                // get everyone in the game
                foreach (PC pc in Character.PCInGameWorld)
                {
                    #region Players in Game
                    isAnonymous = pc.IsAnonymous;
                    isVisible = !pc.IsInvisible;
                    // make sure all players are visible to impLevel 5+
                    if (chr is PC && (chr as PC).ImpLevel >= Globals.eImpLevel.GM)
                    {
                        isAnonymous = false;
                        isVisible = true;
                        anonymous = "";
                        // flag an impLevel that is anonymous or invisible
                        if (pc.IsAnonymous) { anonymous = "[ANON]"; }
                        else if (pc.ImpLevel > Globals.eImpLevel.USER) { anonymous = Conference.GetStaffTitle(pc); }
                        else { anonymous = ""; }
                        if (pc.IsInvisible) { visibility = "[INVIS]"; }
                        else { visibility = ""; }

                        zPlane = isAnonymous ? "ANON" : pc.Map.GetZName(pc.Z);
                    }

                    if (pc is PC && isVisible)
                    {
                        nameOfChar = Conference.GetStaffTitle(pc) + ((pc.MyClone != null) ? pc.MyClone.Name : pc.Name);

                        if (pc.afk) { nameOfChar = nameOfChar + "(AFK)"; }
                        nameOfMap = isAnonymous ? "ANONYMOUS" : pc.Map.Name;
                        nameOfClass = isAnonymous ? "ANONYMOUS" : pc.classFullName;
                        zPlane = isAnonymous ? "ANON" : pc.Map.GetZName(pc.Z);
                        string final = nameOfChar.PadRight(25) + nameOfMap.PadRight(20) + nameOfClass.PadRight(20);
                        if (pc.IsDead && !isAnonymous)
                            final += "(dead)";
                        final += " " + anonymous + visibility;
                        usersList.Add(final);
                    } 
                    #endregion
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            usersList.Sort(); // sort alphabetically

            chr.WriteLine("", ProtocolYuusha.TextType.Listing);
            chr.WriteLine("Name                     Location            Profession", ProtocolYuusha.TextType.Listing);

            int count = 1;

            foreach (string line in usersList)
            {
                chr.WriteLine(line, ProtocolYuusha.TextType.Listing);
                count++;
            }

            if (count > 1)
                chr.WriteLine("There are " + usersList.Count + " adventurers in " + DragonsSpineMain.Instance.Settings.ServerName + ".", ProtocolYuusha.TextType.Listing);
            else
                chr.WriteLine("There is 1 adventurer in " + DragonsSpineMain.Instance.Settings.ServerName + ".", ProtocolYuusha.TextType.Listing);
            chr.WriteLine("", ProtocolYuusha.TextType.Listing);

            return true;
        }
    }
}
