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
using System.IO;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impshowlogs", "Display information about server logs.", (int)Globals.eImpLevel.DEVJR, new string[] { "impshowlog" },
        0, new string[] { "impshowlog dir", "impshowlog <filename>" }, Globals.ePlayerState.CONFERENCE)]
    public class ImpShowLogsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("Usage: impshowlog dir, impshowlog dir <date>, impshowlog <LogType>, impshowlog <LogType> <date>");
                args = "dir";
            }

            try
            {
                var date = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" +
                                  DateTime.Now.Year.ToString().Substring(2);

                var directory = Utils.GetStartupPath() + Path.DirectorySeparatorChar + "Logs" +
                                       Path.DirectorySeparatorChar + date;

                string[] sArgs = args.Split(" ".ToCharArray());

                switch (sArgs[0].ToLower())
                {
                    case "directory":
                    case "dir":
                        if (sArgs.Length > 1)
                        {
                            DateTime newDate;
                            if (DateTime.TryParse(sArgs[1], out newDate))
                            {
                                date = newDate.Month.ToString() + "_" + newDate.Day.ToString() + "_" + newDate.Year.ToString().Substring(2);
                                directory = Utils.GetStartupPath() + Path.DirectorySeparatorChar + "Logs" +
                                       Path.DirectorySeparatorChar + date;
                            }
                        }

                        var filesList = Directory.GetFiles(directory);

                        chr.WriteToDisplay("Logs directory for date: " + date);

                        foreach (string fileLine in filesList)
                            chr.WriteToDisplay(fileLine.Substring(fileLine.IndexOf("DS_"), fileLine.Length - fileLine.IndexOf("DS_")));

                        break;
                    default:
                        Utils.LogType logType;
                        if (Enum.TryParse<Utils.LogType>(sArgs[0], true, out logType))
                        {
                            if (sArgs.Length >= 2)
                            {
                                DateTime chosenDate;
                                if (DateTime.TryParse(sArgs[1], out chosenDate))
                                {
                                    date = chosenDate.Month.ToString() + "_" + chosenDate.Day.ToString() + "_" + chosenDate.Year.ToString().Substring(2);
                                    directory = Utils.GetStartupPath() + Path.DirectorySeparatorChar + "Logs" +
                                       Path.DirectorySeparatorChar + date;
                                }
                            }

                            string fileName = "DS_" + date + "_" + logType.ToString() + ".log";

                            if (!File.Exists(directory + Path.DirectorySeparatorChar + fileName))
                            {
                                chr.WriteToDisplay("Log file does not exist.");
                                return true;
                            }

                            chr.WriteToDisplay("File: " + fileName);

                            using (var sr = new StreamReader(directory + Path.DirectorySeparatorChar + fileName))
                            {
                                string[] lines = sr.ReadToEnd().Split("\n".ToCharArray());
                                foreach (string line in lines)
                                    chr.WriteToDisplay(line);
                            }
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return true;
        }
    }
}
