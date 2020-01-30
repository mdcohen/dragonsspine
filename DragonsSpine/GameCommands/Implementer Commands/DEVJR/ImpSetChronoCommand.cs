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
using World = DragonsSpine.GameWorld.World;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impsetchrono", "Cast any spell.", (int)Globals.eImpLevel.DEVJR, new string[] { "impsettime" },
        0, new string[] { "impsetchrono [time | moon] [times: morning, afternoon, evening, night] [moons: new, waxing_crescent, waning_crescent, full]" }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ImpSetChronoCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Format: impset [time | moon] [times: morning, afternoon, evening, night] [moons: new, waxing_crescent, waning_crescent, full]");
                return true;
            }

            try
            {
                string[] sArgs = args.Split(" ".ToCharArray());
                string cycleList = "";

                switch (sArgs[0].ToLower())
                {
                    case "time":
                        if (sArgs.Length >= 2)
                        {
                            foreach (World.DailyCycle dailyCycle in Enum.GetValues(typeof(World.DailyCycle)))
                            {
                                cycleList += dailyCycle.ToString() + ", ";
                                if (sArgs[1].ToLower() == dailyCycle.ToString().ToLower())
                                {
                                    chr.WriteToDisplay("Game time set from " + World.CurrentDailyCycle + " to " + sArgs[1].ToUpper() + ".");
                                    World.CurrentDailyCycle = dailyCycle;
                                    return true;
                                }
                            }
                            cycleList = cycleList.Substring(0, cycleList.Length - 2);
                            cycleList += ".";
                            chr.WriteToDisplay("Invalid time argument. Choices are " + cycleList);
                        }
                        break;
                    case "moon":
                        if (sArgs.Length >= 2)
                        {
                            foreach (World.LunarCycle lunarCycle in Enum.GetValues(typeof(World.LunarCycle)))
                            {
                                cycleList += lunarCycle.ToString() + ", ";
                                if (sArgs[1].ToLower() == lunarCycle.ToString().ToLower())
                                {
                                    chr.WriteToDisplay("Moon phase set from " + World.CurrentLunarCycle + " to " + sArgs[1].ToUpper() + ".");
                                    World.CurrentLunarCycle = lunarCycle;
                                    return true;
                                }
                            }
                            cycleList = cycleList.Substring(0, cycleList.Length - 2);
                            cycleList += ".";
                            chr.WriteToDisplay("Invalid moon phase argument. Choices are " + cycleList);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                chr.WriteToDisplay("Format: impset [time | moon] [times: morning, afternoon, evening, night] [moons: new, waxing_crescent, waning_crescent, full]");
                return true;
            }

            return false;
        }
    }
}
