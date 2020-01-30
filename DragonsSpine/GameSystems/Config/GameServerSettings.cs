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
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Configuration;


namespace DragonsSpine.Config
{
    /// <summary>
    /// The game server settings class. Note that if there is an xml file then values need to be changed there.
    /// The initialized values here are default.
    /// </summary>
    public class GameServerSettings
    {
        public static bool LoadSettings;

        public bool DebugMode = Convert.ToBoolean(ConfigurationManager.AppSettings["DebugMode"]);
        public bool ProcessEmptyWorld = Convert.ToBoolean(ConfigurationManager.AppSettings["ProcessEmptyWorld"]);

        public DateTime ServerStartTime = DateTime.Now;

        public string SQLConnection = ConfigurationManager.AppSettings["SQLConnection"];
        public string ServerVersion = ConfigurationManager.AppSettings["ServerVersion"];
        public string ClientVersion = ConfigurationManager.AppSettings["ClientVersion"];
        public string ServerName = ConfigurationManager.AppSettings["ServerName"];
        public int ServerPort = Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort"]);
        public string ServerNews = ConfigurationManager.AppSettings["ServerNews"];
        public string DefaultProtocol = ConfigurationManager.AppSettings["DefaultProtocol"]; // default protocol
        public bool ClearStoresOnStartup = Convert.ToBoolean(ConfigurationManager.AppSettings["ClearStores"]);
        public bool RestockStoresOnStartup = Convert.ToBoolean(ConfigurationManager.AppSettings["RestockStores"]);
        public bool RequireMakeRecallReagent = Convert.ToBoolean(ConfigurationManager.AppSettings["RequireMakeRecallReagent"]);
        public bool AllowDisplayCombatDamage = Convert.ToBoolean(ConfigurationManager.AppSettings["DisplayCombatDamage"]);
        public bool UnderworldEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["UnderworldEnabled"]);
        public bool AllowMultipleLoginFromSameIP = Convert.ToBoolean(ConfigurationManager.AppSettings["AllowMultiIP"]);
        public bool SkillLossOverTime = Convert.ToBoolean(ConfigurationManager.AppSettings["SkillLossOverTime"]);
        // Accelerated gains.
        public bool AcceleratedExperienceGain = Convert.ToBoolean(ConfigurationManager.AppSettings["AcceleratedExperienceGain"]);
        public bool AcceleratedSkillGain = Convert.ToBoolean(ConfigurationManager.AppSettings["AcceleratedSkillGain"]);
        public double AcceleratedExperienceGainMultiplier = Convert.ToDouble(ConfigurationManager.AppSettings["AcceleratedExperienceGainMultiplier"]);
        public double AcceleratedSkillGainMultiplier = Convert.ToDouble(ConfigurationManager.AppSettings["AcceleratedSkillGainMultiplier"]);

        public bool NPCSkillGainEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["NPCSkillGain"]);
        public bool DisconnectSamePlayerUponLogin = Convert.ToBoolean(ConfigurationManager.AppSettings["DisconnectSamePlayerUponLogin"]);
        public bool GroupNPCSpawningEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["GroupNPCSpawningEnabled"]);
        public bool NPCTalentsEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["NPCTalentsEnabled"]);
        public bool DetailedCombatLogging = Convert.ToBoolean(ConfigurationManager.AppSettings["DetailedCombatLogging"]);
        public bool DetailedSpellLogging = Convert.ToBoolean(ConfigurationManager.AppSettings["DetailedSpellLogging"]);
        public bool DetailedSkillLogging = Convert.ToBoolean(ConfigurationManager.AppSettings["DetailedSkillLogging"]);
        public bool DetailedNPCDisplayLogging = Convert.ToBoolean(ConfigurationManager.AppSettings["DetailedNPCDisplayLogging"]);
        public int LootPercentageModifier = Convert.ToInt32(ConfigurationManager.AppSettings["LootPercentageModifier"]);
        public bool LotteryEnabled = Convert.ToBoolean(ConfigurationManager.AppSettings["LotteryEnabled"]);

        public bool DetailedLootLogging = Convert.ToBoolean(ConfigurationManager.AppSettings["DetailedLootLogging"]);

        public string ScriptAssemblies = "DragonsSpineScripts.dll";
        public string ScriptCompilationTarget = Utils.GetStartupPath() +"Lib" + Path.DirectorySeparatorChar + "GameScripts.dll";

        /// <summary>
        /// Saves the current settings.
        /// </summary>
        public void Save()
        {
            try
            {
                var dirName = Utils.GetStartupPath() + "Config" + Path.DirectorySeparatorChar;
                Stream stream = File.Create(dirName + "serverconfig.xml");
                var serializer = new XmlSerializer(typeof(GameServerSettings));
                serializer.Serialize(stream, this);
                stream.Close();
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        /// <summary>
        /// Loads settings from a file.
        /// </summary>
        public static GameServerSettings Load()
        {
            try
            {
                if (!LoadSettings)
                    return new GameServerSettings();

                var dirName = Utils.GetStartupPath() + "Config" + Path.DirectorySeparatorChar;

                if (!File.Exists(dirName + "serverconfig.xml"))
                {
                    DragonsSpineMain.Instance.Settings.Save();
                    return new GameServerSettings();
                }

                Stream stream = File.OpenRead(dirName + "serverconfig.xml");
                var serializer = new XmlSerializer(typeof(GameServerSettings));
                var settings = (GameServerSettings)serializer.Deserialize(stream);
                stream.Close();
                return settings;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return new GameServerSettings();
            }
        }
    }
}
