using System;
using System.Configuration;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impappconfig", "Update server app configuration file.", (int)Globals.eImpLevel.DEV, new string[] { },
        0, new string[] { "impappconfig [server|client] <text>" }, Globals.ePlayerState.CONFERENCE)]
    public class ImpUpdateAppConfigCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("You must specifiy which key to update in the app configuration file. impappconfig [key] <value>");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length < 1)
            {
                chr.WriteToDisplay("Invalid number of arguments.");
                return true;
            }

            if (sArgs.Length == 1)
            {
                string result = ReadSetting(sArgs[0]);

                chr.WriteToDisplay("AppConfig Setting [" + sArgs[0] + "] is '" + result + "'");
            }

            return true;
        }

        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings.Count == 0 | settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException exc)
            {
                Utils.LogException(exc);
            }
        }

        public static string DisplaySettings(Character chr)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
            }
            catch (ConfigurationErrorsException exc)
            {
                Utils.LogException(exc);
            }
            return string.Empty;
        }

        public static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                var result = appSettings[key] ?? string.Empty;
                return result;
            }
            catch (ConfigurationErrorsException exc)
            {
                Utils.LogException(exc);
            }
            return string.Empty;
        }
    }
}
