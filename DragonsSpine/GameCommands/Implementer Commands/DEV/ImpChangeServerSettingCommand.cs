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
    [CommandAttribute("impchangeserver", "Change a server setting that is located in the GameServerSettings class. It is for the currently loaded server instance.", (int)Globals.eImpLevel.DEV, new string[] { "impchange", "impsettings" },
        0, new string[] { "impchange <server setting> <value>", "impchange (this will display a list of settings and their values)" }, Globals.ePlayerState.CONFERENCE)]
    public class ImpChangeServerSettingCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            //string[] columns = DAL.DBPlayer.GetPlayerTableColumnNames(id);
            //string fieldName = setfArgs[1];
            //string fieldValue_old = Convert.ToString(DAL.DBPlayer.GetPlayerField(id, fieldName, null)); // subtract one because the /listf display added 1
            //string fieldValue_new = setfArgs[2];

            //System.Reflection.PropertyInfo[] propertyInfo = typeof(DragonsSpine.Config).GetProperties(); // get property info array
            System.Reflection.FieldInfo[] fieldInfo = typeof(DragonsSpine.Config.GameServerSettings).GetFields(); // get field info array

            if (args == "" || args == null)
            {
                chr.WriteToDisplay("Displaying " + fieldInfo.Length + " fields for GameServerSettings...");
                chr.WriteToDisplay("Name (Type): [Value]");
                foreach (System.Reflection.FieldInfo field in fieldInfo)
                {                        
                    chr.WriteToDisplay(field.Name + " (" + field.FieldType + "): " + field.GetValue(field.GetType()).ToString());
                }
            }

            //        if (field.Name.ToLower() == fieldName.ToLower())
            //        {
            //            field.SetValue(pcOnline, Convert.ChangeType(setfArgs[2], Convert.GetTypeCode(field.GetType())), null);
            //            //foundProperty = true;
            //            ch.WriteLine("Found property: " + field.Name, Protocol.TextType.System);
            //            break;
            //        }
            //    }
            //}

            return true;
        }
    }
}
