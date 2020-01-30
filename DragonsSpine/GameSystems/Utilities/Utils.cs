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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using IFormatter = System.Runtime.Serialization.IFormatter;
using BinaryFormatter = System.Runtime.Serialization.Formatters.Binary.BinaryFormatter;
using System.Text;
using System.Security.Cryptography;
using World = DragonsSpine.GameWorld.World;

namespace DragonsSpine
{    

    public class Utils
    {
        public static List<LogType> ConsoleLogTypes = new List<LogType>() { LogType.SystemGo, LogType.Connection, LogType.Login, LogType.Logout,
            LogType.SystemFailure, LogType.SystemFatalError, LogType.SystemWarning, LogType.SystemTesting, LogType.Timeout };

        public static List<LogType> LogsExcludedWhileDebugging = new List<LogType>() { LogType.SpellWarmingFromCreature, LogType.SpellHarmfulFromCreature,
            LogType.SpellBeneficialFromCreature }; // used in debug mode

        /// <summary>
        /// This enumeration does not have a specified order.
        /// </summary>
        public enum LogType
        {
            Adventurer,
            Announcement,
            CoinLogging,
            CombatDamageToCreature,
            CombatDamageToPlayer,
            CombatTesting,
            CommandAllPlayer,
            CommandFailure,
            CommandImmortal,
            Connection,
            CriticalCombatDamageToCreature,
            CriticalCombatDamageToPlayer,
            CSVFormat,
            DatabaseQuery,
            DeathAdventurer,
            DeathCreature,
            DeathUniqueEntity,
            DeathLair,
            DeathPlayer,
            Debug,
            DebugAI,
            DeleteAccount,
            DeletePlayer,
            Disconnect,
            DisplayCreature,
            DisplayPlayer,
            // Documentation, // Note that Documentation is succintly logged (no date/time, only message).
            DocumentationCommands,
            DocumentationQuests,
            DocumentationSpells,
            DocumentationTalents,
            DPSCalcs,
            Exception,
            ExceptionDetail,
            ExperienceLevelGain,
            ExperienceMeleeKill,
            ExperienceSpellAEKill,
            ExperienceSpellCasting,
            ExperienceSpellKill,
            ExperienceTraining,
            GameEvent,
            ItemAttuned,
            ItemFigurineUse,
            JanitorWarning,
            Karma,
            Login,
            Logout,
            // Loot.
            LootAlways,
            LootBelt,
            LootManager,
            LootVeryCommon,
            LootCommon,
            LootUncommon,
            LootRare,
            LootVeryRare,
            LootUltraRare,
            LootBeltRare,
            LootBeltVeryRare,
            LootBeltUltraRare,
            LootScroll,
            LootWand,
            LootWarning,
            LootWarningRarityLevel,
            LootWarningLootType,
            LootWarningLandPlacement,
            LootTable,
            LootTableAbsent,
            LootUnique,
            // Loot end.
            Mark,
            MerchantBuy,
            MerchantSell,
            MerchantTanning,
            NewPlayerCreation,
            NPCWriteToDisplay,
            NPCWriteToDisplaySpells,
            PlayerChat,
            Posterity, // logged for any future reference
            QuestCompletion,
            SkillGainCombat,
            SkillGainNonCombat,
            SkillGainRisk,
            SkillTraining,
            SpellBeneficialFromCreature,
            SpellBeneficialFromPlayer,
            SpellDamageFromMapEffect,
            SpellDamageToCreature,
            SpellDamageToPlayer,
            SpellHarmfulFromCreature,
            SpellHarmfulFromPlayer,
            SpellWarmingFromCreature,
            SpellWarmingFromPlayer,
            SystemFailure,
            SystemFatalError,
            SystemGo,
            SystemRestart,
            SystemTesting,
            SystemWarning,
            Timeout,
            Unknown,
            Yuusha
        }

        public static object lockObject = new object();

        public static int[] ConvertStringToIntArray(string text)
        {
            if (text == "")
                return null;

            try
            {
                string[] list = text.Split(ProtocolYuusha.ASPLIT.ToCharArray());

                int[] intarray = new int[list.Length];

                for (int a = 0; a < intarray.Length; a++)
                {
                    intarray[a] = Convert.ToInt32(list[a]);
                }

                return intarray;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                Utils.Log("Utils.ConvertStringToIntArray failure. text = " + text, LogType.SystemWarning);
                return null;
            }
        }

        public static string ConvertNumberToString(int number)
        {
            switch (number)
            {
                case 1:
                    return "first";
                case 2:
                    return "second";
                case 3:
                    return "third";
                case 4:
                    return "fourth";
                case 5:
                    return "fifth";
                case 6:
                    return "sixth";
                case 7:
                    return "seventh";
                case 8:
                    return "eigth";
                case 9:
                    return "ninth";
                case 10:
                    return "tenth";
                case 11:
                    return "eleventh";
                default:
                    return "";
            }
        }

        public static string ConvertIntArrayToString(int[] array)
        {
            if (array == null)
            {
                return "";
            }

            try
            {
                string list = "";

                for (int a = 0; a < array.Length; a++)
                {
                    list += Convert.ToString(array[a]) + ProtocolYuusha.ASPLIT;
                }

                if (list.Length > 0)
                {
                    list = list.Substring(0, list.Length - ProtocolYuusha.ASPLIT.Length);
                }

                return list;
            }
            catch (Exception e)
            {
                LogException(e);
                return "";
            }
        }

        public static string ConvertListToString(List<string> generic)
        {
            try
            {
                string list = "";
                for (int a = 0; a < generic.Count; a++)
                {
                    list += Convert.ToString(generic[a]) + ProtocolYuusha.ASPLIT;
                }

                if (list.Length > 0)
                {
                    list = list.Substring(0, list.Length - ProtocolYuusha.ASPLIT.Length);
                }
                return list;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return "";
            }
        }

        /*  Return a hexadecimal encoded SHA-256 hash of a string.
         * */
        public static string GetSHA(string pw)
        {
            SHA256 s256 = new SHA256Managed();

            string shaouts = "";
            byte[] shain;
            byte[] shaout;

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            shain = enc.GetBytes(pw);
            shaout = s256.ComputeHash(shain);

            for (int i = 0; i < shaout.Length; i++)                               // Convert to a hexadecimal string.
            {
                shaouts += shaout[i].ToString("X2");
            }

            return shaouts;
        }

        public static string ConvertListToString(List<short> generic)
        {
            try
            {
                string list = "";
                for (int a = 0; a < generic.Count; a++)
                {
                    list += Convert.ToString(generic[a]) + ProtocolYuusha.ASPLIT;
                }

                if (list.Length > 0)
                {
                    list = list.Substring(0, list.Length - ProtocolYuusha.ASPLIT.Length);
                }
                return list;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return "";
            }
        }

        public static string ConvertListToString(List<int> generic)
        {
            try
            {
                string list = "";
                for (int a = 0; a < generic.Count; a++)
                {
                    list += Convert.ToString(generic[a]) + ProtocolYuusha.ASPLIT;
                }

                if (list.Length > 0)
                {
                    list = list.Substring(0, list.Length - ProtocolYuusha.ASPLIT.Length);
                }
                return list;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return "";
            }
        }

        public static string ConvertListToString(Array enumeration)
        {
            try
            {
                string list = "";
                for (int a = 0; a < enumeration.Length; a++)
                {
                    list += Utils.FormatEnumString(enumeration.GetValue(a).ToString()) + ProtocolYuusha.ASPLIT;
                }

                if (list.Length > 0)
                {
                    list = list.Substring(0, list.Length - ProtocolYuusha.ASPLIT.Length);
                }
                return list;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return "";
            }
        }

        public static string FormatEnumString(string enumString)
        {
            enumString = enumString.Replace("__", "'");
            enumString = enumString.Replace("_", " ");
            return enumString;
        }

        public static string ParseEmote(string emoteString)
        {
            if (emoteString.Contains("{") && emoteString.Contains("}"))
                return emoteString.Substring(emoteString.IndexOf("{") + 1, (emoteString.IndexOf("}") - emoteString.IndexOf("{")) - 1);
            
            return "";
        }

        public static void Log(string message, LogType logType)
        {
#if DEBUG
            if (LogsExcludedWhileDebugging.Contains(logType))
                return;
#endif

            switch (logType)
            {
                case LogType.CombatDamageToCreature:
                case LogType.CombatDamageToPlayer:
                case LogType.CombatTesting:
                    if (!DragonsSpineMain.Instance.Settings.DetailedCombatLogging)
                        return;
                    break;
                case LogType.SpellBeneficialFromCreature:
                case LogType.SpellBeneficialFromPlayer:
                case LogType.SpellDamageFromMapEffect:
                case LogType.SpellDamageToCreature:
                case LogType.SpellDamageToPlayer:
                case LogType.SpellHarmfulFromCreature:
                case LogType.SpellHarmfulFromPlayer:
                case LogType.SpellWarmingFromCreature:
                case LogType.SpellWarmingFromPlayer:
                case LogType.NPCWriteToDisplaySpells:
                    if (!DragonsSpineMain.Instance.Settings.DetailedSpellLogging)
                        return;
                    break;
                case LogType.SkillGainCombat:
                case LogType.SkillGainNonCombat:
                case LogType.SkillGainRisk:
                case LogType.SkillTraining:
                    if (!DragonsSpineMain.Instance.Settings.DetailedSkillLogging)
                        return;
                    break;
                default:
                    break;
            }

            // If detailed loot logging is disabled, only log loot errors.
            if (!DragonsSpineMain.Instance.Settings.DetailedLootLogging && logType.ToString().StartsWith("Loot") && !logType.ToString().StartsWith("LootWarning"))
                return;

            lock (lockObject)
            {
                var date = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" +
                              DateTime.Now.Year.ToString().Substring(2);
                var directory = Utils.GetStartupPath() + Path.DirectorySeparatorChar + "Logs" +
                                   Path.DirectorySeparatorChar + date;

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                directory += Path.DirectorySeparatorChar;

                try
                {
                    var fileName = "DS_" + date + "_" + logType.ToString();
                    var logExtension = ".log";
                    var file = new FileStream(directory + fileName + logExtension,FileMode.Append, FileAccess.Write);
                    var count = 1;

                    // Log files should not exceed 1500 bytes.
                    while(file.Length > 1500000)
                    {
                        file = new FileStream(directory + fileName + count.ToString() + logExtension, FileMode.Append, FileAccess.Write);
                        count++;
                    }

                    var rw = new StreamWriter(file);
                    if (logType != LogType.CSVFormat)
                        rw.WriteLine(logType.ToString().Contains("Documentation") ? message : DateTime.Now.ToString() + ": {" + logType.ToString() + "} " + message);
                    else rw.WriteLine(message);
                    rw.Close();
                    file.Close();
                }
                catch (System.IO.IOException)
                {
                }
                catch (Exception)
                {
                }

                #region LogTypes displayed on the console.
                if (ConsoleLogTypes.Contains(logType))
                {
                    Console.WriteLine(DateTime.Now.ToString() + ": {" + logType.ToString() + "} " + message);
                    Console.Write("> ");
                }
                #endregion
            }
        }

        public static string GetStartupPath()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string exeName = Path.GetFileNameWithoutExtension(executingAssembly.Location);
            return (Path.GetDirectoryName(executingAssembly.Location) + @"\");
        }

        public static void LogException(Exception e)
        {
            lock (lockObject)
            {
                var date = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Year.ToString().Substring(2);
                var directory = String.Concat(new object[] { GetStartupPath(), Path.DirectorySeparatorChar, "Logs", Path.DirectorySeparatorChar, date });

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                // but if that wouldn't have been done then it would have been done another way. but that's not the way. the way we would do it if we don't do it then we do it this
                // way for sure. so, no worries 

                var file = new FileStream((directory + Path.DirectorySeparatorChar) + "DS_" + date + "_Exception.log", FileMode.Append, FileAccess.Write);

                var rw = new StreamWriter(file);

                rw.WriteLine(DateTime.Now.ToString() + ": EXCEPTION");
                rw.WriteLine("TargetSite: " + e.TargetSite.ToString());
                rw.WriteLine("Message: " + e.Message.ToString());
                rw.WriteLine("StackTrace: " + e.StackTrace.ToString());
                rw.WriteLine("");
                rw.Close();
                file.Close();
            }
        }

        public static bool SetFieldValue(object obj, String fieldName, object val) {
            var type = obj.GetType();
            var field = type.GetField(fieldName);
            if (field == null) { return false; }
            field.SetValue(obj, Convert.ChangeType(val, field.FieldType));
			return true;
		}

        public static int GetCpuUsage()
        {
            var cpuCounter = new System.Diagnostics.PerformanceCounter("Processor", "% Processor Time", "_Total", Environment.MachineName);
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(1000);
            return (int)cpuCounter.NextValue();
        }

        public static TimeSpan RoundsToTimeSpan(int rounds)
        {
            var secondsPerRound = new TimeSpan(0,0, Convert.ToInt32(DragonsSpineMain.MasterRoundInterval / 1000));
            return new TimeSpan(0, 0, rounds * secondsPerRound.Seconds);
        }

        public static int TimeSpanToRounds(TimeSpan span)
        {
            return Convert.ToInt32(span.TotalSeconds / (DragonsSpineMain.MasterRoundInterval / 1000));
        }
        
        public static void Shuffle(ref List<GameWorld.Cell> list)
        {
            Random rng = new Random();

            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                GameWorld.Cell value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}