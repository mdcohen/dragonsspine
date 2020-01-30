using System;
using System.Windows.Forms;
using DragonsSpine.GameWorld;
using GameSpell = DragonsSpine.Spells.GameSpell;
using GameTalent = DragonsSpine.Talents.GameTalent;

namespace DragonsSpine
{
    public static class ProtocolYuusha
    {
        #region Protocol
        public enum TextType
        {
            PlayerChat, Enter, Exit, Header, Status, System, Help, Private, Listing, Error,
            Friend, Page, Attuned, CreatureChat, Death, SpellCast, SpellWarm, CombatHit, CombatMiss, SpellHit, SpellMiss, DiceRoll
        }

        public enum PromptStates
        {
            Normal, Stunned, Blind, Feared, Resting, Meditating
        }

        public static string USPLIT = "?";
        public static string ASPLIT = " "; // attribute delimiter
        public static string ISPLIT = "^"; // item delimiter
        public static string VSPLIT = "~"; // variable delimiter (if multiple items in proto line)
        public static string TEXT_RETURN = (char)27 + "UU" + (char)27;

        public static int MAX_SCORES = 200;

        #region Commands
        public static string PING = (char)27 + "88" + (char)27;
        public static string DELETE_CHARACTER = (char)27 + "89" + (char)27;
        public static string CHARGEN_RECEIVE = (char)27 + "90" + (char)27;
        public static string GET_SCORES = (char)27 + "91" + (char)27;
        public static string GOTO_GAME = (char)27 + "92" + (char)27;
        public static string GOTO_CHARGEN = (char)27 + "93" + (char)27;
        public static string GOTO_MENU = (char)27 + "94" + (char)27;
        public static string GOTO_CONFERENCE = (char)27 + "95" + (char)27;
        public static string LOGOUT = (char)27 + "96" + (char)27;
        public static string SWITCH_CHARACTER = (char)27 + "97" + (char)27;
        public static string SET_PROTOCOL = (char)27 + "98" + (char)27;
        public static string SET_CLIENT = (char)27 + "99" + (char)27;        
        #endregion

        #region Version Information
        public static string VERSION_SERVER = (char)27 + "V0" + (char)27;
        public static string VERSION_SERVER_END = (char)27 + "V1" + (char)27;
        public static string VERSION_CLIENT = (char)27 + "V2" + (char)27;
        public static string VERSION_CLIENT_END = (char)27 + "V3" + (char)27;
        public static string VERSION_MASTERROUNDINTERVAL = (char)27 + "V4" + (char)27;
        public static string VERSION_MASTERROUNDINTERVAL_END = (char)27 + "V5" + (char)27;
        #endregion

        public static string ACCOUNT_INFO = (char)27 + "A0" + (char)27;
        public static string ACCOUNT_INFO_END = (char)27 + "A1" + (char)27;

        #region Character Information
        public static string CHARACTER_LIST = (char)27 + "C0" + (char)27;
        public static string CHARACTER_LIST_END = (char)27 + "C1" + (char)27;
        public static string CHARACTER_STATS = (char)27 + "C2" + (char)27;
        public static string CHARACTER_STATS_END = (char)27 + "C3" + (char)27;
        public static string CHARACTER_RIGHTHAND = (char)27 + "C4" + (char)27;
        public static string CHARACTER_RIGHTHAND_END = (char)27 + "C5" + (char)27;
        public static string CHARACTER_LEFTHAND = (char)27 + "C6" + (char)27;
        public static string CHARACTER_LEFTHAND_END = (char)27 + "C7" + (char)27;
        public static string CHARACTER_INVENTORY = (char)27 + "C8" + (char)27;
        public static string CHARACTER_INVENTORY_END = (char)27 + "C9" + (char)27;
        public static string CHARACTER_SACK = (char)27 + "CA" + (char)27;
        public static string CHARACTER_SACK_END = (char)27 + "CB" + (char)27;
        public static string CHARACTER_BELT = (char)27 + "CC" + (char)27;
        public static string CHARACTER_BELT_END = (char)27 + "CD" + (char)27;
        public static string CHARACTER_RINGS = (char)27 + "CE" + (char)27;
        public static string CHARACTER_RINGS_END = (char)27 + "CF" + (char)27;
        public static string CHARACTER_LOCKER = (char)27 + "CG" + (char)27;
        public static string CHARACTER_LOCKER_END = (char)27 + "CH" + (char)27;
        public static string CHARACTER_SPELLS = (char)27 + "CI" + (char)27;
        public static string CHARACTER_SPELLS_END = (char)27 + "CJ" + (char)27;
        public static string CHARACTER_EFFECTS = (char)27 + "CK" + (char)27;
        public static string CHARACTER_EFFECTS_END = (char)27 + "CL" + (char)27;
        public static string CHARACTER_CURRENT = (char)27 + "CM" + (char)27;
        public static string CHARACTER_CURRENT_END = (char)27 + "CN" + (char)27;
        public static string CHARACTER_SKILLS = (char)27 + "CO" + (char)27;
        public static string CHARACTER_SKILLS_END = (char)27 + "CP" + (char)27;
        public static string CHARACTER_LIST_SPLIT = (char)27 + "CZ" + (char)27;
        public static string CHARACTER_HITS_UPDATE = (char)27 + "C00" + (char)27;
        public static string CHARACTER_HITS_UPDATE_END = (char)27 + "C01" + (char)27;
        public static string CHARACTER_STAMINA_UPDATE = (char)27 + "C02" + (char)27;
        public static string CHARACTER_STAMINA_UPDATE_END = (char)27 + "C03" + (char)27;
        public static string CHARACTER_MANA_UPDATE = (char)27 + "C04" + (char)27;
        public static string CHARACTER_MANA_UPDATE_END = (char)27 + "C05" + (char)27;
        public static string CHARACTER_EXPERIENCE = (char)27 + "C06" + (char)27;
        public static string CHARACTER_EXPERIENCE_END = (char)27 + "C07" + (char)27;
        public static string CHARACTER_MACROS = (char)27 + "C08" + (char)27;
        public static string CHARACTER_MACROS_END = (char)27 + "C09" + (char)27;
        public static string CHARACTER_PROMPT = (char)27 + "C10" + (char)27;
        public static string CHARACTER_PROMPT_END = (char)27 + "C11" + (char)27;
        public static string CHARACTER_POUCH = (char)27 + "C12" + (char)27;
        public static string CHARACTER_POUCH_END = (char)27 + "C13" + (char)27;
        public static string CHARACTER_TALENTS = (char)27 + "C14" + (char)27;
        public static string CHARACTER_TALENTS_END = (char)27 + "C15" + (char)27;
        public static string CHARACTER_SPELLCAST = (char)27 + "C16" + (char)27;
        public static string CHARACTER_SPELLCAST_END = (char)27 + "C17" + (char)27;
        public static string CHARACTER_MAIL = (char)27 + "C18" + (char)27;
        public static string CHARACTER_MAIL_END = (char)27 + "C19" + (char)27;
        public static string CHARACTER_RESISTS = (char)27 + "C20" + (char)27;
        public static string CHARACTER_RESISTS_END = (char)27 + "C21" + (char)27;
        public static string CHARACTER_PROTECTIONS = (char)27 + "C22" + (char)27;
        public static string CHARACTER_PROTECTIONS_END = (char)27 + "C23" + (char)27;
        public static string CHARACTER_WORNEFFECTS = (char)27 + "C24" + (char)27;
        public static string CHARACTER_WORNEFFECTS_END = (char)27 + "C25" + (char)27;
        public static string CHARACTER_COMBATINFO = (char)27 + "C26" + (char)27;
        public static string CHARACTER_COMBATINFO_END = (char)27 + "C27" + (char)27;
        public static string CHARACTER_SAVINGTHROWS = (char)27 + "C28" + (char)27;
        public static string CHARACTER_SAVINGTHROWS_END = (char)27 + "C29" + (char)27;
        public static string CHARACTER_SERVERSETTINGS = (char)27 + "C30" + (char)27;
        public static string CHARACTER_SERVERSETTINGS_END = (char)27 + "C31" + (char)27;
        public static string CHARACTER_TALENT_USE = (char)27 + "C32" + (char)27;
        public static string CHARACTER_TALENT_USE_END = (char)27 + "C33" + (char)27;
        public static string CHARACTER_SKILLRISK = (char)27 + "C34" + (char)27;
        public static string CHARACTER_SKILLRISK_END = (char)27 + "C35" + (char)27;
        public static string CHARACTER_SKILLEXPCHANGE = (char)27 + "C36" + (char)27;
        public static string CHARACTER_SKILLEXPCHANGE_END = (char)27 + "C37" + (char)27;
        #endregion

        #region Requests
        public static string REQUEST_CHARACTER_INVENTORY = (char)27 + "R8" + (char)27;
        public static string REQUEST_CHARACTER_SACK = (char)27 + "RA" + (char)27;
        public static string REQUEST_CHARACTER_POUCH = (char)27 + "RB" + (char)27;
        public static string REQUEST_CHARACTER_BELT = (char)27 + "RC" + (char)27;
        public static string REQUEST_CELLITEMS = (char)27 + "RD" + (char)27;
        public static string REQUEST_CHARACTER_RINGS = (char)27 + "RE" + (char)27;
        public static string REQUEST_CHARACTER_STATS = (char)27 + "RF" + (char)27;
        public static string REQUEST_CHARACTER_LOCKER = (char)27 + "RG" + (char)27;
        public static string REQUEST_CHARACTER_SPELLS = (char)27 + "RI" + (char)27;
        public static string REQUEST_CHARACTER_EFFECTS = (char)27 + "RK" + (char)27;
        public static string REQUEST_CHARACTER_SKILLS = (char)27 + "RO" + (char)27;
        public static string REQUEST_CHARACTER_TALENTS = (char)27 + "RP" + (char)27;
        public static string REQUEST_CHARACTER_WORNEFFECTS = (char)27 + "RQ" + (char)27;
        public static string REQUEST_CHARACTER_RESISTS = (char)27 + "RR" + (char)27;
        public static string REQUEST_CHARACTER_PROTECTIONS = (char)27 + "RS" + (char)27;
        public static string REQUEST_CHARACTER_COMBATINFO = (char)27 + "RT" + (char)27; // armor class variables and THAC0
        public static string REQUEST_CHARACTER_SAVINGTHROWS = (char)27 + "RU" + (char)27;
        #endregion

        #region Main Menu, News, Detect Protocol/Client
        public static string MENU_MAIN = (char)27 + "M0" + (char)27;
        public static string NEWS = (char)27 + "M1" + (char)27;
        public static string NEWS_END = (char)27 + "M2" + (char)27;
        public static string DETECT_PROTOCOL = (char)27 + "M3" + (char)27;
        public static string DETECT_CLIENT = (char)27 + "M4" + (char)27;
        public static string MESSAGEBOX = (char)27 + "M5" + (char)27;
        public static string MESSAGEBOX_END = (char)27 + "M6" + (char)27;
        #endregion

        #region Text
        public static string TEXT_PLAYERCHAT = (char)27 + "T00" + (char)27;
        public static string TEXT_PLAYERCHAT_END = (char)27 + "T01" + (char)27;
        public static string TEXT_HEADER = (char)27 + "T02" + (char)27;
        public static string TEXT_HEADER_END = (char)27 + "T03" + (char)27;
        public static string TEXT_STATUS = (char)27 + "T04" + (char)27;
        public static string TEXT_STATUS_END = (char)27 + "T05" + (char)27;
        public static string TEXT_PRIVATE = (char)27 + "T06" + (char)27;
        public static string TEXT_PRIVATE_END = (char)27 + "T07" + (char)27;
        public static string TEXT_ENTER = (char)27 + "T08" + (char)27;
        public static string TEXT_ENTER_END = (char)27 + "T09" + (char)27;
        public static string TEXT_EXIT = (char)27 + "T10" + (char)27;
        public static string TEXT_EXIT_END = (char)27 + "T11" + (char)27;
        public static string TEXT_SYSTEM = (char)27 + "T12" + (char)27;
        public static string TEXT_SYSTEM_END = (char)27 + "T13" + (char)27;
        public static string TEXT_HELP = (char)27 + "T14" + (char)27;
        public static string TEXT_HELP_END = (char)27 + "T15" + (char)27;
        public static string TEXT_LISTING = (char)27 + "T16" + (char)27;
        public static string TEXT_LISTING_END = (char)27 + "T17" + (char)27;
        public static string TEXT_ERROR = (char)27 + "T18" + (char)27;
        public static string TEXT_ERROR_END = (char)27 + "T19" + (char)27;
        public static string TEXT_FRIEND = (char)27 + "T20" + (char)27;
        public static string TEXT_FRIEND_END = (char)27 + "T21" + (char)27;
        public static string TEXT_PAGE = (char)27 + "T22" + (char)27;
        public static string TEXT_PAGE_END = (char)27 + "T23" + (char)27;
        public static string TEXT_CREATURECHAT = (char)27 + "T24" + (char)27;
        public static string TEXT_CREATURECHAT_END = (char)27 + "T25" + (char)27;
        public static string TEXT_DEFAULT = (char)27 + "T26" + (char)27;
        public static string TEXT_DEFAULT_END = (char)27 + "T27" + (char)27;
        public static string TEXT_DICEROLL = (char)27 + "T28" + (char)27;
        public static string TEXT_DICEROLL_END = (char)27 + "T29" + (char)27;
        
        #endregion

        public static string IMP_CHARACTERFIELDS = (char)27 + "I0" + (char)27;
        public static string IMP_CHARACTERFIELDS_END = (char)27 + "I1" + (char)27;

        #region Sound
        public static string SOUND = (char)27 + "S0" + (char)27;
        public static string SOUND_END = (char)27 + "S1" + (char)27;
        public static string SOUND_FROM_CLIENT = (char)27 + "S2" + (char)27; 
        #endregion

        #region CharGen
        public static string CHARGEN_ENTER = (char)27 + "CG0" + (char)27;
        public static string CHARGEN_ROLLER_RESULTS = (char)27 + "CG1" + (char)27;
        public static string CHARGEN_ROLLER_RESULTS_END = (char)27 + "CG2" + (char)27;
        public static string CHARGEN_ERROR = (char)27 + "CG3" + (char)27;
        public static string CHARGEN_INVALIDNAME = (char)27 + "CG4" + (char)27;
        public static string CHARGEN_ACCEPTED = (char)27 + "CG5" + (char)27; 
        #endregion

        #region Conference Room
        public static string CONF_ENTER = (char)27 + "F0" + (char)27;
        public static string CONF_INFO = (char)27 + "F1" + (char)27;
        public static string CONF_INFO_END = (char)27 + "F2" + (char)27;
        #endregion

        #region Game Information
        public static string GAME_CELL = (char)27 + "G0" + (char)27;
        public static string GAME_CELL_END = (char)27 + "G1" + (char)27;
        public static string GAME_CELL_INFO = (char)27 + "G2" + (char)27;
        public static string GAME_CELL_INFO_END = (char)27 + "G3" + (char)27;
        public static string GAME_CELL_CRITTERS = (char)27 + "G4" + (char)27;
        public static string GAME_CELL_CRITTERS_END = (char)27 + "G5" + (char)27;
        public static string GAME_CELL_ITEMS = (char)27 + "G6" + (char)27;
        public static string GAME_CELL_ITEMS_END = (char)27 + "G7" + (char)27;
        public static string GAME_CRITTER_INFO = (char)27 + "G8" + (char)27;
        public static string GAME_CRITTER_INFO_END = (char)27 + "G9" + (char)27;
        public static string GAME_CRITTER_INVENTORY = (char)27 + "GA" + (char)27;
        public static string GAME_CRITTER_INVENTORY_END = (char)27 + "GB" + (char)27;
        public static string GAME_CELL_EFFECTS = (char)27 + "GC" + (char)27;
        public static string GAME_CELL_EFFECTS_END = (char)27 + "GD" + (char)27;
        public static string GAME_WORLD_INFO = (char)27 + "GE" + (char)27;
        public static string GAME_WORLD_INFO_END = (char)27 + "GF" + (char)27;
        public static string GAME_EXIT = (char)27 + "GG" + (char)27;
        public static string GAME_TEXT = (char)27 + "GH" + (char)27;
        public static string GAME_TEXT_END = (char)27 + "GI" + (char)27;
        public static string GAME_NEW_ROUND = (char)27 + "GJ" + (char)27;
        public static string GAME_END_ROUND = (char)27 + "GK" + (char)27;
        public static string GAME_ROUND_DELAY = (char)27 + "GL" + (char)27;
        public static string GAME_ENTER = (char)27 + "GM" + (char)27;
        public static string GAME_POINTER_UPDATE = (char)27 + "GN" + (char)27;
        public static string GAME_CHARACTER_DEATH = (char)27 + "GO" + (char)27;
        public static string GAME_CHARACTER_DEATH_END = (char)27 + "GP" + (char)27;
        #endregion

        #region World Information
        public static string WORLD_SPELLS = (char)27 + "W0" + (char)27;
        public static string WORLD_SPELLS_END = (char)27 + "W1" + (char)27;
        public static string WORLD_LANDS = (char)27 + "W2" + (char)27;
        public static string WORLD_LANDS_END = (char)27 + "W3" + (char)27;
        public static string WORLD_MAPS = (char)27 + "W4" + (char)27;
        public static string WORLD_MAPS_END = (char)27 + "W5" + (char)27;
        public static string WORLD_SCORES = (char)27 + "W6" + (char)27;
        public static string WORLD_SCORES_END = (char)27 + "W7" + (char)27;
        public static string WORLD_USERS = (char)27 + "W8" + (char)27;
        public static string WORLD_USERS_END = (char)27 + " W9" + (char)27;
        public static string WORLD_INFORMATION = (char)27 + "WA" + (char)27;
        public static string WORLD_CHARGEN_INFO = (char)27 + "WB" + (char)27;
        public static string WORLD_CHARGEN_INFO_END = (char)27 + "WC" + (char)27;
        public static string WORLD_CELL_INFO = (char)27 + "WD" + (char)27;
        public static string WORLD_CELL_INFO_END = (char)27 + "WE" + (char)27;
        public static string WORLD_ITEMS = (char)27 + "WF" + (char)27;
        public static string WORLD_ITEMS_END = (char)27 + "WG" + (char)27;
        public static string WORLD_TALENTS = (char)27 + "WH" + (char)27;
        public static string WORLD_TALENTS_END = (char)27 + "WI" + (char)27;
        public static string WORLD_QUESTS = (char)27 + "WJ" + (char)27;
        public static string WORLD_QUESTS_END = (char)27 + "WK" + (char)27;
        #endregion
        #endregion

        public static string GetTextProtocolString(TextType textType, bool startString)
        {
            try
            {
                System.Reflection.FieldInfo[] fieldInfo = typeof(ProtocolYuusha).GetFields();

                foreach (System.Reflection.FieldInfo info in fieldInfo)
                {
                    if (info.Name.StartsWith("TEXT_"))
                    {
                        if (startString)
                        {
                            if (info.Name == ("TEXT_" + textType.ToString().ToUpper()))
                            {
                                return (string)info.GetValue(null);
                            }
                        }
                        else
                        {
                            if (info.Name == ("TEXT_" + textType.ToString().ToUpper() + "_END"))
                            {
                                return (string)info.GetValue(null);
                            }
                        }
                    }
                }
                Utils.Log("Protocol.GetTextProtocolString(" + textType.ToString() + ", " + Convert.ToString(startString) + ") failed to find a suitable value.", Utils.LogType.SystemFailure);
                return "";
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return "";
            }
        }

        public static void UpdateUserLists() // called when one of the character array lists is changed, or someone switches characters
        {
            //TODO: this should probably be in a new thread
            int a;
            PC ch;
            for (a = 0; a < Character.PCInGameWorld.Count; a++)
            {
                ch = Character.PCInGameWorld[a] as PC;
                if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                {
                    SendUserList(ch);
                }
            }
            for (a = 0; a < Character.ConfList.Count; a++)
            {
                ch = Character.ConfList[a] as PC;
                if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                {
                    SendUserList(ch);
                }
            }
            for (a = 0; a < Character.MenuList.Count; a++)
            {
                ch = Character.MenuList[a] as PC;
                if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                {
                    SendUserList(ch);
                }
            }
            for (a = 0; a < Character.CharGenList.Count; a++)
            {
                ch = Character.CharGenList[a] as PC;
                if (ch.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                {
                    SendUserList(ch);
                }
            }
        }

        public static void SendAccountInfo(PC ch)
        {
            string accountInfo = ch.Account.accountName + ProtocolYuusha.VSPLIT +
                ch.Account.accountID + ProtocolYuusha.VSPLIT +
                ch.lifetimeMarks + ProtocolYuusha.VSPLIT +
                ch.currentMarks + ProtocolYuusha.VSPLIT +
                ch.Account.ipAddress;

            ch.WriteLine(ProtocolYuusha.ACCOUNT_INFO + accountInfo + ProtocolYuusha.ACCOUNT_INFO_END);
                
        }

        public static void SendCharacterList(PC ch)
        {
            DAL.DBPlayer.p_sendCharacterList(ch.Account.accountID, ch);
        }

        public static void SendUserList(PC ch)
        {
            string playerList = "";

            // get everyone at the menu
            foreach (PC user in Character.MenuList)
            {
                playerList += FormatUserInfo(ch, user);
            }
            // get everyone in conference rooms
            foreach (PC user in Character.ConfList)
            {
                playerList += FormatUserInfo(ch, user);
            }
            // get everyone in the game
            foreach (PC user in Character.PCInGameWorld)
            {
                playerList += FormatUserInfo(ch, user);
            }
            if (playerList.Length > ISPLIT.Length)
            {
                // remove the last ISPLIT
                playerList = playerList.Substring(0, playerList.Length - ISPLIT.Length);
                // send the complete protocol line
                ch.WriteLine(WORLD_USERS + playerList + WORLD_USERS_END);
            }
        }

        #region Formatting
        public static string FormatUserInfo(PC ch, PC user)
        {
            // format: id, title, name, class, level, location, afk

            string userInformation = "";

            if (user.IsAnonymous && !user.IsInvisible)
            {
                if (ch.ImpLevel >= Globals.eImpLevel.GM)
                {
                    userInformation = user.UniqueID + VSPLIT +
                        (int)user.ImpLevel + VSPLIT +
                        user.Name + VSPLIT +
                        user.classFullName + VSPLIT +
                        user.Level + VSPLIT +
                        Conference.GetUserLocation(user) + VSPLIT +
                        user.LandID + VSPLIT +
                        user.MapID + VSPLIT +
                        user.IsDead + VSPLIT +
                        user.IsAnonymous + VSPLIT +
                        user.IsInvisible + VSPLIT +
                        user.afk + VSPLIT +
                        user.receivePages + VSPLIT +
                        user.receiveTells + VSPLIT +
                        user.showStaffTitle;
                    userInformation += ISPLIT;
                }
                else
                {
                    userInformation = user.UniqueID + VSPLIT +
                        (int)user.ImpLevel + VSPLIT +
                        user.Name + VSPLIT +
                        VSPLIT + // do not send class info
                        "0" + VSPLIT; // do not send level info
                    // switch to determine whether to send location information or not
                    switch (user.PCState)
                    {
                        case Globals.ePlayerState.CONFERENCE:
                            userInformation += Conference.GetUserLocation(user) + VSPLIT; // send room location
                            break;
                        default:
                            userInformation += VSPLIT; // do not send location info
                            break;
                    }
                    userInformation +=
                        user.LandID + VSPLIT +
                        user.MapID + VSPLIT +
                        user.IsDead + VSPLIT +
                        user.IsAnonymous + VSPLIT +
                        user.IsInvisible + VSPLIT +
                        user.afk + VSPLIT +
                        user.receivePages + VSPLIT +
                        user.receiveTells + VSPLIT +
                        user.showStaffTitle;
                    userInformation += ISPLIT;
                }

            }
            else if (user.IsInvisible)
            {
                // only send invisible player's information if the viewer's impLevel is >= the user's
                if (ch.ImpLevel >= user.ImpLevel)
                {
                    userInformation = user.UniqueID + VSPLIT +
                        (int)user.ImpLevel + VSPLIT +
                        user.Name + VSPLIT +
                        user.classFullName + VSPLIT +
                        user.Level + VSPLIT +
                        Conference.GetUserLocation(user) + VSPLIT +
                        user.LandID + VSPLIT +
                        user.MapID + VSPLIT +
                        user.IsDead + VSPLIT +
                        user.IsAnonymous + VSPLIT +
                        user.IsInvisible + VSPLIT +
                        user.afk + VSPLIT +
                        user.receivePages + VSPLIT +
                        user.receiveTells + VSPLIT +
                        user.showStaffTitle;

                    userInformation += ISPLIT;
                }
            }
            else // send all available user information
            {
                userInformation = user.UniqueID + VSPLIT +
                    (int)user.ImpLevel + VSPLIT +
                    user.Name + VSPLIT +
                    user.classFullName + VSPLIT +
                    user.Level + VSPLIT +
                    Conference.GetUserLocation(user) + VSPLIT +
                    user.LandID + VSPLIT +
                    user.MapID + VSPLIT +
                    user.IsDead + VSPLIT +
                    user.IsAnonymous + VSPLIT +
                    user.IsInvisible + VSPLIT +
                    user.afk + VSPLIT +
                    user.receivePages + VSPLIT +
                    user.receiveTells + VSPLIT +
                    user.showStaffTitle;

                userInformation += ISPLIT;
            }
            return userInformation;
        }

        public static string FormatInventoryItemInfo(Item item)
        {
            return "" + item.itemID + VSPLIT +
                        item.UniqueID + VSPLIT +
                        //(int)item.itemType + VSPLIT +
                        //(int)item.baseType + VSPLIT +
                        //(int)item.skillType + VSPLIT +
                        item.name + VSPLIT +
                        item.visualKey + VSPLIT +
                        (int)item.wearLocation + VSPLIT +
                        (int)item.wearOrientation;// + VSPLIT + // this
        }

        public static string FormatItemInfo(Item item)
        {
            return "" + item.itemID + VSPLIT +
                        item.UniqueID + VSPLIT +
                        item.name + VSPLIT +
                        item.visualKey;// + VSPLIT +
                        //(int)item.wearLocation + VSPLIT +
                        //(int)item.wearOrientation + VSPLIT +
                        //item.identifiedName + VSPLIT +
                        //(int)item.baseType + VSPLIT +
                        //item.IsNocked;
        }

        public static string FormatLandInfo(Land land)
        {
            return "" + land.LandID + VSPLIT +
                        land.Name + VSPLIT +
                        land.ShortDesc + VSPLIT +
                        land.LongDesc;
        }

        public static string FormatMapInfo(Map map)
        {
            return "" + map.LandID + VSPLIT +
                        map.MapID + VSPLIT +
                        map.Name + VSPLIT +
                        map.ShortDesc + VSPLIT +
                        map.LongDesc + VSPLIT +
                        map.SuggestedMaximumLevel + VSPLIT +
                        map.SuggestedMinimumLevel + VSPLIT +
                        map.IsPVPEnabled + VSPLIT +
                        map.ExperienceModifier + VSPLIT +
                        map.Difficulty + VSPLIT +
                        (int)map.Climate + VSPLIT;
        }

        public static void SendCellItemsInfo(Cell cell, Character ch)
        {
            if (cell == null || ch == null) return;

            string cellItemsInfo = ProtocolYuusha.GAME_CELL_ITEMS;
            string cellCoords = cell.X + VSPLIT + cell.Y + VSPLIT + cell.Z;
            cellItemsInfo += cellCoords + ISPLIT;

            for (int a = 0; a < cell.Items.Count; a++)
            {
                Item item = cell.Items[a];
                cellItemsInfo += FormatCellItem(item) + ISPLIT;
            }
            if (cellItemsInfo.Length > ProtocolYuusha.GAME_CELL_ITEMS.Length)
                cellItemsInfo = cellItemsInfo.Substring(0, cellItemsInfo.Length - ISPLIT.Length);

            cellItemsInfo += ProtocolYuusha.GAME_CELL_ITEMS_END;
            ch.Write(cellItemsInfo);
        }

        public static string FormatCellInfo(Cell cell, Character ch)
        {
            string info = ProtocolYuusha.GAME_CELL_INFO;
            info += cell.LandID + ISPLIT +
                        cell.MapID + ISPLIT +
                        cell.X + ISPLIT +
                        cell.Y + ISPLIT +
                        cell.Z + ISPLIT +
                        cell.CellGraphic + ISPLIT +
                        cell.DisplayGraphic + ISPLIT +
                        cell.IsLocker + ISPLIT +
                        cell.IsMapPortal + ISPLIT +
                        (cell.Items.Count > 0 ? Convert.ToString(true) : Convert.ToString(false)) + GAME_CELL_INFO_END;

            string critters = ProtocolYuusha.GAME_CELL_CRITTERS;

            foreach (Character crit in cell.Characters.Values)
            {
                if (crit.UniqueID != ch.UniqueID || crit.IsImage)
                {
                    if (Rules.DetectHidden(crit, ch) && Rules.DetectInvisible(crit, ch))
                    {
                        // Used for acquiring targets.
                        if (!ch.seenList.Contains(crit)) ch.seenList.Add(crit);

                        if (cell != ch.CurrentCell)
                        {
                            if (crit.Group == null || crit.IsPC || ((crit is NPC) && crit.Group != null && crit.Group.GroupLeaderID == crit.UniqueID))
                            {
                                critters += FormatCellCritter(crit, ch) + USPLIT;
                            }
                        }
                        else
                        {
                            critters += FormatCellCritter(crit, ch) + USPLIT;
                        }
                    }
                }
            }

            if (critters.Length > ProtocolYuusha.GAME_CELL_CRITTERS.Length)
            {
                critters = critters.Substring(0, critters.Length - USPLIT.Length);
            }

            critters += ProtocolYuusha.GAME_CELL_CRITTERS_END;

            //string items = Protocol.GAME_CELL_ITEMS;
            //for (a = 0; a < cell.Items.Count; a++)
            //{
            //    Item item = (Item)cell.Items[a];
            //    items += FormatCellItem(item) + ISPLIT;
            //}
            //if (items.Length > Protocol.GAME_CELL_ITEMS.Length)
            //    items = items.Substring(0, items.Length - ISPLIT.Length);

            //items += Protocol.GAME_CELL_ITEMS_END;

            string effects = ProtocolYuusha.GAME_CELL_EFFECTS;

            foreach (Effect effect in cell.AreaEffects.Values)
                effects += FormatEffectInfo(effect) + ISPLIT;

            if (effects.Length > ProtocolYuusha.GAME_CELL_EFFECTS.Length)
                effects = effects.Substring(0, effects.Length - ISPLIT.Length);

            effects += ProtocolYuusha.GAME_CELL_EFFECTS_END;

            //return info + critters + items + effects;
            return info + critters + effects;
        }

        public static string FormatCellCritter(Character target, Character ch)
        {
            string info = GAME_CRITTER_INFO;

            Globals.eAlignment detectedAlignment = target.Alignment;
            Character.ClassType detectedClassType = target.BaseProfession;
            string detectedVisualKey = target.visualKey;

            if (!Rules.DetectThief(target, ch))
            {
                detectedAlignment = ch.Alignment;
                detectedClassType = Character.ClassType.Fighter;

                if (target.species == Globals.eSpecies.Human)
                    detectedVisualKey = ch.visualKey.Replace("thief", "fighter");
            }

            if (target.IsPC)
            {
                info += target.UniqueID + VSPLIT +
                    target.Name + VSPLIT;
            }
            else
            {
                if (target.Group == null || target.CurrentCell == ch.CurrentCell)
                {
                    info += target.UniqueID + VSPLIT +
                        target.Name + VSPLIT;
                }
                else
                {
                    info += target.UniqueID + VSPLIT +
                        target.Group.GroupNPCList.Count.ToString() + " " + GameSystems.Text.TextManager.Multinames(target.Name) + VSPLIT;
                }
            }

            //if (target is NPC)
            //{
            //    info += (target as NPC).shortDesc + VSPLIT +
            //    (target as NPC).longDesc + VSPLIT;
            //}
            //else
            //{
            //    info += "" + VSPLIT + "" + VSPLIT;
            //}

            info += detectedVisualKey + VSPLIT +
            //(int)detectedClassType + VSPLIT +
            (int)detectedAlignment + VSPLIT;
            //target.Level + VSPLIT +
            //(int)target.gender + VSPLIT +
            //target.race + VSPLIT +
            //target.Hits + VSPLIT +
            //target.HitsMax + VSPLIT;

            if (target.RightHand != null)
            {
                info += target.RightHand.itemID + VSPLIT + target.RightHand.name + VSPLIT + target.RightHand.visualKey + VSPLIT;
            }
            else
            {
                info += VSPLIT + VSPLIT + VSPLIT;
            }
            if (target.LeftHand != null)
            {
                info += target.LeftHand.itemID + VSPLIT + target.LeftHand.name + VSPLIT + target.LeftHand.visualKey + VSPLIT;
            }
            else
            {
                info += VSPLIT + VSPLIT + VSPLIT;
            }
            info += target.GetVisibleArmorName() + VSPLIT;
            info += (double)target.Hits / target.HitsFull * 100 + VSPLIT;
            info += (double)target.Stamina / target.StaminaFull * 100;
            if(target.ManaFull > 0)
                info += VSPLIT + (double)target.Mana / target.ManaFull * 100;
            info += ProtocolYuusha.GAME_CRITTER_INFO_END;

            //string inventory = ProtocolYuusha.GAME_CRITTER_INVENTORY;
            //foreach (Item item in target.wearing)
            //{
            //    if (item != null)
            //    {
            //        if ((target is NPC) && ((target as NPC).tanningResult == null || (target as NPC).tanningResult.ContainsKey(item.itemID)))
            //        {
            //            inventory += FormatCellItem(item) + ISPLIT;
            //        }
            //    }
            //}
            //if (inventory.Length > ProtocolYuusha.GAME_CRITTER_INVENTORY.Length)
            //{
            //    inventory = inventory.Substring(0, inventory.Length - ISPLIT.Length);
            //}
            //inventory += ProtocolYuusha.GAME_CRITTER_INVENTORY_END;
            return info;// + inventory;
        }

        public static string FormatCellItem(Item item)
        {
            return "" + item.itemID + VSPLIT +
                item.UniqueID + VSPLIT +
                item.name + VSPLIT +
                item.visualKey;
        }

        public static string FormatSpellInfo(GameSpell spell)
        {
            string classTypes = "";

            for (int a = 0; a < spell.ClassTypes.Length; a++)
            {
                classTypes += (int)spell.ClassTypes[a] + " ";
            }
            classTypes = classTypes.Substring(0, classTypes.Length - 1);

            return "" + spell.ID + VSPLIT +
                        spell.Command + VSPLIT +
                        spell.Name + VSPLIT +
                        spell.Description + VSPLIT +
                        spell.RequiredLevel + VSPLIT +
                        spell.ManaCost + VSPLIT +
                        (int)spell.SpellType + VSPLIT +
                        (int)spell.TargetType + VSPLIT +
                        classTypes + VSPLIT +
                        spell.TrainingPrice + VSPLIT +
                        spell.IsBeneficial + VSPLIT +
                        spell.SoundFile;
        }

        public static string FormatQuestInfo(GameQuest quest)
        {
            return "";
            //return "" + quest.QuestID + VSPLIT +
            //            quest.Name + VSPLIT +
            //            quest.Description + VSPLIT +
            //            quest.CompletedDescription + VSPLIT +
            //            quest.MasterQuestID + VSPLIT +
            //            quest.RewardExperience + VSPLIT +
            //            quest.RewardFlags + VSPLIT +
            //            rewardItems + VSPLIT +
            //            classTypes + VSPLIT +
            //            spell.TrainingPrice + VSPLIT +
            //            spell.IsBeneficial + VSPLIT +
            //            spell.SoundFile;
        }

        public static string FormatTalentInfo(GameTalent talent)
        {
            //string classTypes = "";

            //for (int a = 0; a < talent..ClassTypes.Length; a++)
            //{
            //    classTypes += (int)spell.ClassTypes[a] + " ";
            //}
            //classTypes = classTypes.Substring(0, classTypes.Length - 1);

            return "" +
                        talent.Command + VSPLIT +
                        talent.Name + VSPLIT +
                        talent.Description + VSPLIT +
                        talent.MinimumLevel + VSPLIT +
                        talent.PerformanceCost + VSPLIT +
                        talent.PurchasePrice + VSPLIT +
                        talent.IsPassive + VSPLIT +
                        talent.IsAvailableAtMentor + VSPLIT +
                        talent.SoundFile + VSPLIT +
                        Utils.TimeSpanToRounds(talent.DownTime);
        }

        public static string FormatEffectInfo(Effect effect)
        {
            string effectString = effect.EffectType + VSPLIT +
                effect.Power + VSPLIT +
                effect.Duration + VSPLIT;

            if (effect.Caster != null)
                effectString += effect.Caster.Name;

            return effectString;
        }

        public static string FormatScoreInfo(PC score)
        {
            return "" + score.UniqueID + VSPLIT +
                        score.Name + VSPLIT +
                        (int)score.BaseProfession + VSPLIT +
                        score.classFullName + VSPLIT +
                        score.Level + VSPLIT +
                        score.Experience + VSPLIT +
                        score.Kills + VSPLIT +
                        score.RoundsPlayed + VSPLIT +
                        score.lastOnline + VSPLIT +
                        score.IsAnonymous + VSPLIT +
                        (int)score.ImpLevel + VSPLIT +
                        score.LandID;
        }

        public static string FormatCharGenInfo(Character newbie)
        {
            string startingEquipment = "";
            string startingSpells = "";
            string startingSkills = "";

            try
            {
                #region Starting Equipment
                if (newbie.RightHand != null)
                {
                    startingEquipment += newbie.RightHand.notes + " (Right Hand)" + VSPLIT;
                }

                if (newbie.LeftHand != null)
                {
                    startingEquipment += newbie.LeftHand.notes + " (Left Hand)" + VSPLIT;
                }

                foreach (Item item in newbie.wearing)
                {
                    startingEquipment += item.notes + " (Worn)" + VSPLIT;
                }

                foreach (Item item in newbie.beltList)
                {
                    startingEquipment += item.notes + " (Belt)" + VSPLIT;
                }

                foreach (Item item in newbie.sackList)
                {
                    startingEquipment += item.notes + " (Sack)" + VSPLIT;
                }

                if(startingEquipment.Length > VSPLIT.Length)
                    startingEquipment = startingEquipment.Substring(0, startingEquipment.Length - VSPLIT.Length);


                #endregion

                #region Starting Spells
                if (newbie.spellDictionary.Count > 0)
                {
                    foreach (int spellID in newbie.spellDictionary.Keys)
                    {
                        startingSpells += spellID + VSPLIT;
                    }
                    startingSpells = startingSpells.Substring(0, startingSpells.Length - VSPLIT.Length);
                }
                #endregion

                #region Starting Skills
                foreach (Globals.eSkillType skillType in Enum.GetValues(typeof(Globals.eSkillType)))
                {
                    if (skillType != Globals.eSkillType.None)
                    {
                        startingSkills += (int)skillType + " " + newbie.GetSkillExperience(skillType) + VSPLIT;
                    }
                }

                startingSkills = startingSkills.Substring(0, startingSkills.Length - VSPLIT.Length);
                #endregion

                return "" + newbie.race + ISPLIT +
                            (int)newbie.BaseProfession + ISPLIT +
                            (int)newbie.Alignment + ISPLIT +
                            startingEquipment + ISPLIT +
                            startingSpells + ISPLIT +
                            startingSkills;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return "";
            }
        }

        public static string FormatCharGenRollerResults(Character ch)
        {
            return ch.Strength + VSPLIT +
                ch.Dexterity + VSPLIT +
                ch.Intelligence + VSPLIT +
                ch.Wisdom + VSPLIT +
                ch.Constitution + VSPLIT +
                ch.Charisma + VSPLIT +
                ch.strengthAdd + VSPLIT +
                ch.dexterityAdd + VSPLIT +
                ch.HitsMax + VSPLIT +
                ch.StaminaMax + VSPLIT +
                ch.ManaMax;
        } 
        #endregion

        #region Check Command
        public static bool CheckAllCommand(Character ch, string command, string args)
        {
            #region Ping
            if (command == PING)
            {
                ch.WriteLine(PING);
                return true;
            }

            if(command == GAME_EXIT)
            {
                if(ch is PC pc)
                {
                    pc.RemoveFromWorld();
                    pc.RemoveFromConf();
                    pc.RemoveFromLogin();
                    pc.RemoveFromCharGen();
                    pc.RemoveFromServer();
                }
                return true;
            }
            #endregion

            return false;
        }

        public static bool CheckLoginCommand(Character ch, string command, string args)
        {
            if (command == SET_CLIENT)
            {
                ch.usingClient = true;
                // make Character a new instatiation of YuushaPC
                // set SSL streams
                return true;
            }

            return false;
        }

        public static bool CheckMenuCommand(PC ch, string command, string args)
        {
            if (CheckAllCommand(ch, command, args))
                return true;

            #region Set Protocol
            if (command == SET_PROTOCOL)
            {
                ch.protocol = DragonsSpineMain.Instance.Settings.DefaultProtocol;
                Utils.Log(ch.GetLogString() + " SET_PROTOCOL.", Utils.LogType.Yuusha);
                Menu.PrintMainMenu(ch);
                return true;
            }
            #endregion

            if(command == VERSION_CLIENT)
            {
                Utils.Log(ch.GetLogString() + " VERSION_CLIENT " + args, Utils.LogType.Yuusha);
            }

            #region Return False If Protocol Not Set
            else if (ch.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                return false;
            }
            #endregion

            #region Set Client
            else if (command == SET_CLIENT)
            {
                ch.usingClient = true;
                return true;
            }
            #endregion

            #region Switch Character
            else if (command == SWITCH_CHARACTER)
            {
                if (PC.SwitchCharacter(ch, Convert.ToInt32(args)))
                {
                    ProtocolYuusha.SendCurrentCharacterID(ch);
                }
                return true;
            }
            #endregion

            #region Delete Character
            else if (command == DELETE_CHARACTER)
            {
                int id = Convert.ToInt32(args);

                if (id == ch.UniqueID)
                {
                    ProtocolYuusha.sendMessageBox(ch, "You cannot delete your active character.", "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return true;
                }

                string[] playerlist = DAL.DBPlayer.GetCharacterList("Name", ch.Account.accountID);

                bool accountMatch = false;

                foreach (string name in playerlist)
                {
                    if (PC.GetName(id) == name)
                    {
                        accountMatch = true;
                        break;
                    }
                }

                if (accountMatch)
                {
                    ProtocolYuusha.sendMessageBox(ch, "You have deleted your character \"" + PC.GetName(id) + "\".", "Character Delete Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DAL.DBPlayer.DeletePlayerFromDatabase(id);
                }
                else if (ch.ImpLevel == Globals.eImpLevel.DEV)
                {
                    ProtocolYuusha.sendMessageBox(ch, "You have deleted the character \"" + PC.GetName(id) + "\" belonging to account \"" + PC.GetPC(id).Account.accountName + "\".", "Character Delete Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DAL.DBPlayer.DeletePlayerFromDatabase(id);
                }
                return true;
            }
            #endregion

            #region Go To Conference
            else if (command == GOTO_CONFERENCE)
            {
                ch.PCState = Globals.ePlayerState.CONFERENCE;
                ch.RemoveFromMenu();
                ch.AddToConf();
                //ch.PCState = Character.State.CONFERENCE;
                Conference.Header(ch as PC, true);
            } 
            #endregion

            #region Go To Game
            else if (command == GOTO_GAME)
            {
                ch.RemoveFromMenu(); // remove the character from the menu list
                ch.PCState = Globals.ePlayerState.PLAYING; // change character state to playing
                ch.AddToWorld(); // add the character to the world list
                ProtocolYuusha.ShowMap(ch); // removes delay in showing map upon entering game
                return true;
            }
            #endregion

            #region Go To CharGen
            else if (command == GOTO_CHARGEN)
            {
                if (DAL.DBPlayer.GetCharactersCount(ch.Account.accountID) >= Character.MAX_CHARS) // currently verified by the client -Eb 5/17/06
                {
                    ProtocolYuusha.sendMessageBox(ch, "You have reached the maximum amount of characters allowed.  Try deleting an existing character.");
                    return true;
                }
                PC newchar = new PC();
                newchar.Account.accountName = ch.Account.accountName;
                newchar.Account.accountID = ch.Account.accountID;
                newchar.protocol = ch.protocol;
                newchar.usingClient = ch.usingClient;
                newchar.echo = ch.echo;
                newchar.confRoom = ch.confRoom;
                ch.RemoveFromMenu(); // remove the character from the menu list
                PC.LoadCharacter(ch, newchar); // load the new character
                ch.AddToCharGen(); // add the character to the character generation list
                //ch.Write(ProtocolYuusha.CHARGEN_ENTER);
                ch.WriteLine(CharGen.NewChar());
                ch.Write(CharGen.PickGender());
                ch.PCState = Globals.ePlayerState.PICKGENDER;
                return true;
            }
            #endregion

            #region Get Scores
            else if (command == GET_SCORES)
            {
                ProtocolYuusha.sendWorldScores(ch);
                return true;
            }
            #endregion

            #region Logout
            else if (command == LOGOUT)
            {
                Utils.Log(ch.GetLogString(), Utils.LogType.Logout);
                ch.RemoveFromMenu();
                ch.RemoveFromServer();
                return true;
            }
            #endregion

            return false;
        }

        public static bool CheckChatRoomCommand(PC ch, string command, string args)
        {
            if (ch.protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol) { return false; }

            if (CheckAllCommand(ch, command, args))
                return true;

            #region Logout
            if (command == LOGOUT)
            {
                if (!ch.IsInvisible) // send message if character is not invisible
                {
                    ch.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch as PC) + ch.Name + " has left the world.", ProtocolYuusha.TextType.Exit);
                }
                Utils.Log(ch.GetLogString(), Utils.LogType.Logout);
                ch.RemoveFromConf();
                ch.RemoveFromServer();
                return true;
            }
            #endregion

            #region Go To CharGen
            else if (command == GOTO_CHARGEN)
            {
                if (DAL.DBPlayer.GetCharactersCount(ch.Account.accountID) >= Character.MAX_CHARS) // currently verified by the client -Eb 5/17/06
                {
                    ProtocolYuusha.sendMessageBox(ch, "You have reached the maximum amount of characters allowed.  Try deleting an existing character.");
                    return true;
                }
                if (!ch.IsInvisible) // send message if character is not invisible
                {
                    ch.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch as PC) + ch.Name + " has left for the character generator.", ProtocolYuusha.TextType.Exit);
                }
                Utils.Log(ch.GetLogString(), Utils.LogType.Logout);
                PC newchar = new PC();
                newchar.Account.accountName = ch.Account.accountName;
                newchar.Account.accountID = ch.Account.accountID;
                newchar.protocol = ch.protocol;
                newchar.usingClient = ch.usingClient;
                newchar.echo = ch.echo;
                newchar.confRoom = ch.confRoom;
                ch.RemoveFromConf(); // remove the character from the menu list
                PC.LoadCharacter(ch, newchar); // load the new character
                ch.AddToCharGen(); // add the character to the character generation list
                //ch.Write(CHARGEN_ENTER);
                ch.PCState = Globals.ePlayerState.PICKGENDER;
            }
            #endregion

            #region Delete Character
            else if (command == DELETE_CHARACTER)
            {
                int id = Convert.ToInt32(args);

                if (id == ch.UniqueID)
                {
                    ProtocolYuusha.sendMessageBox(ch, "You cannot delete your active character.", "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return true;
                }

                string[] playerlist = DAL.DBPlayer.GetCharacterList("Name", ch.Account.accountID);

                bool accountMatch = false;

                foreach (string name in playerlist)
                {
                    if (PC.GetName(id) == name)
                    {
                        accountMatch = true;
                        break;
                    }
                }

                if (accountMatch)
                {
                    ProtocolYuusha.sendMessageBox(ch, "You have deleted your character \"" + PC.GetName(id) + "\".", "Character Delete Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DAL.DBPlayer.DeletePlayerFromDatabase(id);
                }
                else if (ch.ImpLevel == Globals.eImpLevel.DEV)
                {
                    ProtocolYuusha.sendMessageBox(ch, "You have deleted the character \"" + PC.GetName(id) + "\" belonging to account \"" + PC.GetPC(id).Account.accountName + "\".", "Character Delete Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DAL.DBPlayer.DeletePlayerFromDatabase(id);
                }
                return true;
            }
            #endregion

            #region Switch Character
            else if (command == SWITCH_CHARACTER)
            {
                if (PC.SwitchCharacter(ch, Convert.ToInt32(args)))
                {
                    ProtocolYuusha.SendCurrentCharacterID(ch);
                }
                return true;
            }
            #endregion

            #region Go To Game
            else if (command == GOTO_GAME)
            {
                ch.RemoveFromConf();
                ch.PCState = Globals.ePlayerState.PLAYING;
                ch.AddToWorld();
                ProtocolYuusha.ShowMap(ch); // removes delay in displaying game screen

                if (ch.IsAnonymous && !ch.IsInvisible)
                {
                    ch.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch as PC) + ch.Name + " has left for the lands.", ProtocolYuusha.TextType.Exit);
                }
                else if (!ch.IsAnonymous && !ch.IsInvisible)
                {
                    ch.SendToAllInConferenceRoom(Conference.GetStaffTitle(ch as PC) + ch.Name + " has left for " + World.GetFacetByID(ch.FacetID).GetLandByID(ch.LandID).GetMapByID(ch.MapID).ShortDesc + " (" + World.GetFacetByID(ch.FacetID)+").", ProtocolYuusha.TextType.Exit);
                }
                return true;
            }
            #endregion

            #region Go To Menu
            else if (command == GOTO_MENU)
            {
                ch.PCState = Globals.ePlayerState.MAINMENU;
                Menu.PrintMainMenu(ch);
            }
            #endregion

            #region Get Scores
            else if (command == GET_SCORES)
            {
                ProtocolYuusha.sendWorldScores(ch);
                return true;
            }
            #endregion

            return false;
        }

        public static bool CheckCharGenCommand(PC ch, string command, string args)
        {
            if (CheckAllCommand(ch, command, args))
                return true;

            if (command == SET_CLIENT)
            {
                ch.usingClient = true;
                return true;
            }

            if (command == VERSION_CLIENT)
            {
                Utils.Log(ch.GetLogString() + " VERSION_CLIENT " + args, Utils.LogType.Yuusha);
            }

            if (command == SET_PROTOCOL)
            {
                ch.protocol = DragonsSpineMain.Instance.Settings.DefaultProtocol;
                return true;
            }

            // Step 1: gender, race, class type and name verification
            // Step 2: roll stats or go back to step 1

            #region CharGen Receive (receive gender, homeland (race), class type and name from client)
            if (command == CHARGEN_RECEIVE)
            {
                string[] cArgs = args.Split(VSPLIT.ToCharArray()); // gender, homeland, classType, name
                bool genderOK = false;
                foreach (Globals.eGender gender in Enum.GetValues(typeof(Globals.eGender)))
                {
                    if (cArgs[0] == gender.ToString())
                    {
                        ch.gender = gender;
                        genderOK = true;
                    }
                }
                if (!genderOK)
                {
                    ch.Write(ProtocolYuusha.CHARGEN_ERROR);
                    return true;
                }

                bool homelandOK = false;
                foreach (Globals.eHomeland homeland in Enum.GetValues(typeof(Globals.eHomeland)))
                {
                    if (cArgs[1] == homeland.ToString())
                    {
                        ch.race = homeland.ToString();
                        homelandOK = true;
                    }
                }
                if (!homelandOK)
                {
                    ch.Write(ProtocolYuusha.CHARGEN_ERROR);
                    return true;
                }

                bool classTypeOK = false;
                foreach (Character.ClassType classType in Enum.GetValues(typeof(Character.ClassType)))
                {
                    if (cArgs[2] == Utils.FormatEnumString(classType.ToString()))
                    {
                        ch.BaseProfession = classType;
                        ch.classFullName = Utils.FormatEnumString(classType.ToString());
                        classTypeOK = true;
                    }
                }
                if (!classTypeOK)
                {
                    ch.Write(ProtocolYuusha.CHARGEN_ERROR);
                    return true;
                }

                if (CharGen.CharacterNameDenied(ch, cArgs[3]))
                {
                    ch.Write(ProtocolYuusha.CHARGEN_INVALIDNAME);
                    return true;
                }

                ch.Name = cArgs[3].Substring(0, 1).ToUpper() + cArgs[3].Substring(1, cArgs[3].Length - 1);
                ch.Write(ProtocolYuusha.CHARGEN_ACCEPTED);
                ch.Write(CharGen.RollStats(ch));
                ch.PCState = Globals.ePlayerState.ROLLSTATS;
                return true;
            }
            #endregion

            #region Send Back To Step One
            else if (command == GOTO_CHARGEN)
            {
                ch.PCState = Globals.ePlayerState.PICKGENDER;
                return true;
            }
            #endregion

            #region Go To Menu
            else if (command == GOTO_MENU)
            {
                int lastPlayed = Account.GetLastPlayed(ch.Account.accountID);

                if (lastPlayed > 0) // new account with no character cannot leave chargen
                {
                    PC pc1 = PC.GetPC(lastPlayed);
                    (ch as PC).RemoveFromCharGen();
                    ch.UniqueID = lastPlayed;
                    pc1.UniqueID = lastPlayed;
                    PC.LoadCharacter(ch, pc1); // fill in our ch with all the properties of lastplayed char.
                    (ch as PC).AddToMenu(); // add the character to the menu list
                    Utils.Log(ch.GetLogString(), Utils.LogType.Login);
                    (ch as PC).lastOnline = DateTime.UtcNow; // set last online
                    PC.SaveField(ch.UniqueID, "lastOnline", (ch as PC).lastOnline, null); // save last online
                    ch.PCState = Globals.ePlayerState.MAINMENU; // set state to menu
                    Menu.PrintMainMenu(ch as PC); // print main menu
                }
                return true;
            }
            #endregion

            #region Go To Conference
            else if (command == GOTO_CONFERENCE)
            {
                int lastPlayed = Account.GetLastPlayed(ch.Account.accountID);

                if (lastPlayed > 0) // new account with no character cannot leave chargen
                {
                    PC pc1 = PC.GetPC(lastPlayed);
                    ch.RemoveFromCharGen();
                    ch.UniqueID = lastPlayed;
                    pc1.UniqueID = lastPlayed;
                    PC.LoadCharacter(ch, pc1); // fill in our ch with all the properties of lastplayed char.
                    ch.AddToConf(); // add the character to the menu list
                    Utils.Log(ch.GetLogString(), Utils.LogType.Login);
                    ch.lastOnline = DateTime.UtcNow; // set last online
                    PC.SaveField(ch.UniqueID, "lastOnline", (ch as PC).lastOnline, null); // save last online
                    ch.PCState = Globals.ePlayerState.CONFERENCE; // set state to menu
                    Conference.Header(ch as PC, true);
                }
                return true;
            }
            #endregion

            #region Logout
            else if (command == LOGOUT)
            {
                Utils.Log("Account (" + ch.Account.accountName + ") logged out from chargen.", Utils.LogType.Logout);
                ch.RemoveFromCharGen();
                ch.RemoveFromServer();
                return true;
            }
            #endregion

            return false;
        }

        public static bool CheckGameCommand(Character ch, string command, string args)
        {
            if (CheckAllCommand(ch, command, args))
                return true;

            if (command == REQUEST_CHARACTER_BELT)
            {
                SendCharacterBelt(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_EFFECTS)
            {
                SendCharacterEffects(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_WORNEFFECTS)
            {
                SendCharacterWornEffects(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_RESISTS)
            {
                SendCharacterResists(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_PROTECTIONS)
            {
                SendCharacterProtections(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_COMBATINFO)
            {
                SendCharacterCombatInfo(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_SAVINGTHROWS)
            {
                SendCharacterSavingThrows(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_INVENTORY)
            {
                SendCharacterInventory(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_LOCKER)
            {
                SendCharacterLocker(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_RINGS)
            {
                SendCharacterRings(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_STATS)
            {
                SendCharacterStats(ch as PC, ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_SACK)
            {
                SendCharacterSack(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_POUCH)
            {
                SendCharacterPouch(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_SKILLS)
            {
                SendCharacterSkills(ch);
                return true;
            }
            else if (command == REQUEST_CHARACTER_SPELLS)
            {
                SendCharacterSpells(ch as PC, ch);
                return true;
            }
            else if(command == REQUEST_CHARACTER_TALENTS)
            {
                SendCharacterTalents(ch as PC, ch);
                return true;
            }
            else if(command == REQUEST_CELLITEMS)
            {
                // should add a check in here to make sure distance is less than 2 cells away...
                // need coordinates from arguments
                string[] coords = args.Split(VSPLIT.ToCharArray());
                Cell cell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, Convert.ToInt32(coords[0]), Convert.ToInt32(coords[1]), Convert.ToInt32(coords[2]));
                if (cell == null)
                {
                    Utils.Log("Cell NULL for REQUEST_CELLITEMS with args: " + args, Utils.LogType.SystemWarning);
                    return true;
                }

                SendCellItemsInfo(cell, ch);
                return true;
            }

            #region Logout
            else if (command == LOGOUT)
            {
                Utils.Log(ch.GetLogString(), Utils.LogType.Logout);
                ch.RemoveFromWorld();
                ch.RemoveFromServer();
                return true;
            } 
            #endregion

            return false;
        }
        #endregion

        #region Character Updates
        public static void SendPlayerHitsUpdate(PC pc)
        {
            pc.Write(CHARACTER_HITS_UPDATE + pc.Hits + VSPLIT + pc.HitsMax + VSPLIT + pc.HitsAdjustment + VSPLIT + pc.HitsDoctored + CHARACTER_HITS_UPDATE_END);
        }

        public static void SendPlayerStaminaUpdate(PC pc)
        {
            pc.Write(CHARACTER_STAMINA_UPDATE + pc.Stamina + VSPLIT + pc.StaminaMax + VSPLIT + pc.StaminaAdjustment + CHARACTER_STAMINA_UPDATE_END);
        }

        public static void SendPlayerManaUpdate(PC pc)
        {
            pc.Write(CHARACTER_MANA_UPDATE + pc.Mana + VSPLIT + pc.ManaMax + VSPLIT + pc.ManaAdjustment + CHARACTER_MANA_UPDATE_END);
        }

        public static void SendPlayerExperienceUpdate(PC pc, long expChange)
        {
            pc.Write(CHARACTER_EXPERIENCE + expChange + VSPLIT + pc.Experience + CHARACTER_EXPERIENCE_END);
        }

        /// <summary>
        /// This method needs work. Updates should be event based, only sent when there is a change. Not every round.
        /// 3/14/2017 Eb
        /// </summary>
        /// <param name="pc"></param>
        /// <param name="currentCharacter"></param>
        public static void SendCharacterUpdate(PC pc, Character currentCharacter)
        {
            if ((currentCharacter as PC).protocol != DragonsSpineMain.Instance.Settings.DefaultProtocol)
                return;

            SendCharacterStats(pc, currentCharacter);
            //SendCharacterSkills(currentCharacter);
            SendCharacterRightHand(pc, currentCharacter);
            SendCharacterLeftHand(pc, currentCharacter);
            //SendCharacterInventory(currentCharacter);
            //SendCharacterSack(currentCharacter);
            //SendCharacterBelt(currentCharacter);
            //SendCharacterRings(currentCharacter);
            //SendCharacterLocker(currentCharacter);
            //if(currentCharacter.IsSpellWarmingProfession)
            //    SendCharacterSpells(currentCharacter);
            //SendCharacterEffects(currentCharacter);
            return;
        }

        public static void SendCharacterStats(PC pc, Character currentCharacter)
        {
            string message = ProtocolYuusha.CHARACTER_STATS;
            try
            {
                message += pc.UniqueID + VSPLIT +
                    pc.Name + VSPLIT +
                    (int)pc.gender + VSPLIT +
                    pc.race + VSPLIT +
                    (int)pc.BaseProfession + VSPLIT +
                    pc.classFullName + VSPLIT +
                    (int)pc.Alignment + VSPLIT +
                    (int)pc.ImpLevel + VSPLIT +
                    pc.IsImmortal + VSPLIT +
                    pc.showStaffTitle + VSPLIT + //
                    pc.receivePages + VSPLIT + //
                    pc.receiveTells + VSPLIT + //
                    Utils.ConvertIntArrayToString(pc.friendsList) + VSPLIT +
                    Utils.ConvertIntArrayToString(pc.ignoreList) + VSPLIT +
                    pc.friendNotify + VSPLIT + // move to a settings update
                    pc.filterProfanity + VSPLIT + // move to a settings update
                    pc.IsAncestor + VSPLIT +
                    pc.IsAnonymous + VSPLIT + // move to a settings update
                    pc.LandID + VSPLIT +
                    pc.MapID + VSPLIT +
                    pc.X + VSPLIT +
                    pc.Y + VSPLIT +
                    pc.Stunned + VSPLIT +
                    pc.floating + VSPLIT + // unnecessary
                    pc.IsDead + VSPLIT +  // unnecessary
                    pc.IsHidden + VSPLIT +  // unnecessary
                    pc.IsInvisible + VSPLIT +
                    pc.HasNightVision + VSPLIT + // unnecessary
                    pc.HasFeatherFall + VSPLIT + // unnecessary
                    pc.CanBreatheWater + VSPLIT + // unnecessary
                    pc.IsBlind + VSPLIT +
                    pc.Poisoned + VSPLIT +
                    (int)pc.fighterSpecialization + VSPLIT +
                    pc.Level + VSPLIT +
                    pc.Experience + VSPLIT +
                    pc.Hits + VSPLIT +
                    pc.HitsMax + VSPLIT +
                    pc.Stamina + VSPLIT +
                    pc.StaminaMax + VSPLIT +
                    pc.Mana + VSPLIT +
                    pc.ManaMax + VSPLIT +
                    pc.Age + VSPLIT +
                    pc.RoundsPlayed + VSPLIT +
                    pc.Kills + VSPLIT +
                    pc.Deaths + VSPLIT +
                    pc.bankGold + VSPLIT +
                    pc.Strength + VSPLIT +
                    pc.Dexterity + VSPLIT +
                    pc.Intelligence + VSPLIT +
                    pc.Wisdom + VSPLIT +
                    pc.Constitution + VSPLIT +
                    pc.Charisma + VSPLIT +
                    pc.strengthAdd + VSPLIT +
                    pc.dexterityAdd + VSPLIT +
                    pc.encumbrance + VSPLIT +
                    pc.birthday + VSPLIT +
                    pc.lastOnline + VSPLIT +
                    pc.currentKarma + VSPLIT +
                    pc.currentMarks + VSPLIT +
                    pc.pvpNumKills + VSPLIT +
                    pc.pvpNumDeaths + VSPLIT +
                    Utils.ConvertListToString(pc.PlayersKilled) + VSPLIT +
                    Utils.ConvertListToString(pc.FlaggedUniqueIDs) + VSPLIT +
                    pc.HasKnightRing + VSPLIT +
                    pc.dirPointer + VSPLIT +
                    pc.HitsAdjustment + VSPLIT +
                    pc.StaminaAdjustment + VSPLIT +
                    pc.ManaAdjustment + VSPLIT +
                    pc.visualKey + VSPLIT +
                    pc.HitsDoctored + VSPLIT +
                    pc.lastOnline.ToString() + VSPLIT +
                    pc.Map.Name + VSPLIT +
                    pc.Map.ZPlanes[pc.Z].name;
                message += ProtocolYuusha.CHARACTER_STATS_END;
                currentCharacter.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterCombatInfo(Character chr)
        {
            // needs updating
            string message = ProtocolYuusha.CHARACTER_COMBATINFO;
            message += "Armor Rating" + ISPLIT + Combat.AC_GetArmorClassRating(chr) + VSPLIT +
                "Shielding (melee)" + ISPLIT + Combat.AC_GetShieldingArmorClass(chr, false) + VSPLIT +
                "Shielding (ranged)" + ISPLIT + Combat.AC_GetShieldingArmorClass(chr, true) + VSPLIT;
            message += CHARACTER_COMBATINFO_END;
            chr.Write(message);
        }

        public static void SendCharacterSavingThrows(Character chr)
        {
            // needs updating
            string message = ProtocolYuusha.CHARACTER_COMBATINFO;
            message += "Armor Rating" + ISPLIT + Combat.AC_GetArmorClassRating(chr) + VSPLIT +
                "Shielding (melee)" + ISPLIT + Combat.AC_GetShieldingArmorClass(chr, false) + VSPLIT +
                "Shielding (ranged)" + ISPLIT + Combat.AC_GetShieldingArmorClass(chr, true) + VSPLIT;
            message += CHARACTER_COMBATINFO_END;
            chr.Write(message);
        }

        public static void SendCharacterResists(Character chr)
        {
            string message = ProtocolYuusha.CHARACTER_RESISTS;
            message += "Acid" + ISPLIT + chr.AcidResistance + VSPLIT +
                "Blind" + ISPLIT + chr.BlindResistance + VSPLIT +
                "Cold" + ISPLIT + chr.ColdResistance + VSPLIT +
                "Death" + ISPLIT + chr.DeathResistance + VSPLIT +
                "Fear" + ISPLIT + chr.FearResistance + VSPLIT +
                "Fire" + ISPLIT + chr.FireResistance + VSPLIT +
                "Lightning " + ISPLIT + chr.LightningResistance + VSPLIT +
                "Poison" + ISPLIT + chr.PoisonResistance + VSPLIT +
                "Stun" + ISPLIT + chr.StunResistance;
            message += CHARACTER_RESISTS_END;
            chr.Write(message);
        }

        public static void SendCharacterProtections(Character chr)
        {
            string message = ProtocolYuusha.CHARACTER_PROTECTIONS;
            message += "Acid" + ISPLIT + chr.AcidProtection + VSPLIT +
                "Cold" + ISPLIT + chr.ColdProtection + VSPLIT +
                "Death" + ISPLIT + chr.DeathProtection + VSPLIT +
                "Fire" + ISPLIT + chr.FireProtection + VSPLIT +
                "Lightning" + ISPLIT + chr.LightningProtection;
            message += CHARACTER_PROTECTIONS_END;
            chr.Write(message);
        }

        // TODO: SendCharacterTraining
        public static void SendCharacterSkills(Character chr)
        {
            Array skillTypes = Enum.GetValues(typeof(Globals.eSkillType));

            string message = ProtocolYuusha.CHARACTER_SKILLS;

            for (int a = 1; a < skillTypes.Length; a++)
            {
                Globals.eSkillType skillType = (Globals.eSkillType)skillTypes.GetValue(a);
                long exp = chr.GetSkillExperience(skillType);
                int skillLevel = Skills.GetSkillLevel(exp);

                message += skillType.ToString() + ISPLIT + skillLevel + ISPLIT +
                    Skills.GetSkillTitle(skillType, chr.BaseProfession, exp, chr.gender) + VSPLIT;
            }

            message = message.Substring(0, message.Length - VSPLIT.Length);
            message += ProtocolYuusha.CHARACTER_SKILLS_END;
            chr.Write(message);
        }

        public static void SendCharacterRightHand(PC pc, Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_RIGHTHAND;
                if (currentCharacter.RightHand != null)
                {
                    message += FormatItemInfo(currentCharacter.RightHand);
                }
                currentCharacter.Write(message + CHARACTER_RIGHTHAND_END);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterLeftHand(PC pc, Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_LEFTHAND;
                if (pc.LeftHand != null)
                {
                    message += FormatItemInfo(pc.LeftHand);
                }
                currentCharacter.Write(message + CHARACTER_LEFTHAND_END);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterInventory(Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_INVENTORY;
                foreach (Item item in currentCharacter.wearing)
                {
                    message += FormatInventoryItemInfo(item) + ISPLIT;
                }
                if (message.Length > CHARACTER_INVENTORY.Length) { message = message.Substring(0, message.Length - ISPLIT.Length); }
                currentCharacter.Write(message + CHARACTER_INVENTORY_END);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterSack(Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_SACK;
                foreach (Item item in currentCharacter.sackList)
                {
                    message += FormatItemInfo(item) + ISPLIT;
                }
                if (message.Length > CHARACTER_SACK.Length) { message = message.Substring(0, message.Length - ISPLIT.Length); }
                currentCharacter.Write(message + CHARACTER_SACK_END);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterPouch(Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_POUCH;
                foreach (Item item in currentCharacter.pouchList)
                {
                    message += FormatItemInfo(item) + ISPLIT;
                }
                if (message.Length > CHARACTER_POUCH.Length) { message = message.Substring(0, message.Length - ISPLIT.Length); }
                currentCharacter.Write(message + CHARACTER_POUCH_END);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterBelt(Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_BELT;
                foreach (Item item in currentCharacter.beltList)
                {
                    message += FormatItemInfo(item) + ISPLIT;
                }
                if (message.Length > CHARACTER_BELT.Length) { message = message.Substring(0, message.Length - ISPLIT.Length); }
                currentCharacter.Write(message + CHARACTER_BELT_END);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterRings(Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_RINGS;
                Item item = null;
                for (int ring = 1; ring <= Character.MAX_RINGS; ring++)
                {
                    switch (ring)
                    {
                        case 1:
                            item = currentCharacter.RightRing1;
                            break;
                        case 2:
                            item = currentCharacter.RightRing2;
                            break;
                        case 3:
                            item = currentCharacter.RightRing3;
                            break;
                        case 4:
                            item = currentCharacter.RightRing4;
                            break;
                        case 5:
                            item = currentCharacter.LeftRing1;
                            break;
                        case 6:
                            item = currentCharacter.LeftRing2;
                            break;
                        case 7:
                            item = currentCharacter.LeftRing3;
                            break;
                        case 8:
                            item = currentCharacter.LeftRing4;
                            break;
                    }
                    if (item != null)
                    {
                        item.wearOrientation = (Globals.eWearOrientation)ring;
                        message += FormatInventoryItemInfo(item);
                    }
                    message += ISPLIT;
                }
                if (message.Length > CHARACTER_RINGS.Length) { message = message.Substring(0, message.Length - ISPLIT.Length); }
                currentCharacter.Write(message + CHARACTER_RINGS_END);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterLocker(Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_LOCKER;
                foreach (Item item in (currentCharacter as PC).lockerList)
                {
                    message += FormatItemInfo(item) + ISPLIT;
                }
                if (message.Length > CHARACTER_LOCKER.Length) { message = message.Substring(0, message.Length - ISPLIT.Length); }
                currentCharacter.Write(message + CHARACTER_LOCKER_END);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterSpells(PC pc, Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_SPELLS;
                foreach (int a in currentCharacter.spellDictionary.Keys)
                {
                    message += a + VSPLIT + currentCharacter.spellDictionary[a] + ISPLIT;
                }
                
                if (message.Length > CHARACTER_SPELLS.Length) { message = message.Substring(0, message.Length - ISPLIT.Length); }
                message += CHARACTER_SPELLS_END;
                pc.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterTalents(PC pc, Character currentCharacter)
        {
            try
            {
                string message = CHARACTER_TALENTS;
                foreach (string a in currentCharacter.talentsDictionary.Keys)
                {
                    message += a + VSPLIT + currentCharacter.talentsDictionary[a] + ISPLIT;
                }

                if (message.Length > CHARACTER_TALENTS.Length) { message = message.Substring(0, message.Length - ISPLIT.Length); }
                message += CHARACTER_TALENTS_END;
                pc.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterEffects(Character currentCharacter)
        {
            try
            {
                string message = ProtocolYuusha.CHARACTER_EFFECTS;
                foreach (Effect effect in currentCharacter.EffectsList.Values)
                {
                    message += FormatEffectInfo(effect) + ISPLIT;
                }
                if (message.Length > ProtocolYuusha.CHARACTER_EFFECTS.Length)
                {
                    message = message.Substring(0, message.Length - ISPLIT.Length);
                }
                message += ProtocolYuusha.CHARACTER_EFFECTS_END;
                currentCharacter.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCharacterWornEffects(Character currentCharacter)
        {
            try
            {
                string message = ProtocolYuusha.CHARACTER_WORNEFFECTS;
                foreach (Effect wornEffect in currentCharacter.WornEffectsList)
                {
                    message += FormatEffectInfo(wornEffect) + ISPLIT;
                }
                if (message.Length > ProtocolYuusha.CHARACTER_WORNEFFECTS.Length)
                {
                    message = message.Substring(0, message.Length - ISPLIT.Length);
                }
                message += ProtocolYuusha.CHARACTER_WORNEFFECTS_END;
                currentCharacter.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return;
            }
            return;
        }

        public static void SendCurrentCharacterID(PC ch)
        {
            ch.Write(ProtocolYuusha.CHARACTER_CURRENT + ch.UniqueID + ProtocolYuusha.CHARACTER_CURRENT_END);
            //if (ch.ImpLevel == Globals.eImpLevel.DEV)
            //{
            //    if (!ch.sentImplementorInformation)
            //    {
            //        ProtocolYuusha.SendImplementorCharacterFields(ch);
            //        ch.sentImplementorInformation = true;
            //    }
            //}

            SendCharacterRightHand(ch as PC, ch);
            SendCharacterLeftHand(ch as PC, ch);
        }

        public static void SendCharacterCastSpell(PC ch, int spellID)
        {
            ch.Write(ProtocolYuusha.CHARACTER_SPELLCAST + spellID + CHARACTER_SPELLCAST_END);
        }

        public static void SendCharacterMacros(PC pc, PC ch)
        {
            // no macros have been set yet
            if (ch.macros.Count <= 0) return;

            string macroList = "";

            foreach (string macro in pc.macros)
                macroList += macro + ISPLIT;

            // remove the last ISPLIT
            if(macroList.Length > 0)
                macroList = macroList.Substring(0, macroList.Length - ISPLIT.Length);

            ch.Write(ProtocolYuusha.CHARACTER_MACROS + macroList + ProtocolYuusha.CHARACTER_MACROS_END);
        }

        public static void SendCharacterServerSettings(PC pc, PC ch)
        {

        }
        #endregion

        #region World Information
        public static void sendWorldLands(Character ch)
        {
            try
            {
                string message = ProtocolYuusha.WORLD_LANDS;
                foreach (Land land in World.GetFacetByIndex(0).Lands)
                {
                    message += FormatLandInfo(land) + ISPLIT;
                }
                message = message.Substring(0, message.Length - ISPLIT.Length);
                message += ProtocolYuusha.WORLD_LANDS_END;
                ch.WriteLine(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void sendWorldMaps(Character ch)
        {
            try
            {
                string message = ProtocolYuusha.WORLD_MAPS;
                for (int a = 0; a < World.GetFacetByIndex(0).Lands.Count; a++)
                {
                    Land land = World.GetFacetByIndex(0).GetLandByIndex(a);
                    foreach (Map map in land.Maps)
                    {
                        message += FormatMapInfo(map) + ISPLIT;
                    }
                }
                message = message.Substring(0, message.Length - ISPLIT.Length);
                message += ProtocolYuusha.WORLD_MAPS_END;
                ch.WriteLine(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void sendWorldNews(Character ch)
        {
            ch.Write(ProtocolYuusha.NEWS + DragonsSpineMain.Instance.Settings.ServerNews + ProtocolYuusha.NEWS_END);
        }

        public static void SendWorldSpells(Character ch)
        {
            try
            {
                string message = ProtocolYuusha.WORLD_SPELLS;
                foreach (GameSpell spell in GameSpell.GameSpellDictionary.Values)
                {
                    message += FormatSpellInfo(spell) + ISPLIT;
                }
                message = message.Substring(0, message.Length - ISPLIT.Length);
                message += ProtocolYuusha.WORLD_SPELLS_END;
                ch.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void SendWorldQuests(Character ch)
        {
            try
            {
                string message = WORLD_QUESTS;
                foreach (GameQuest quest in GameQuest.QuestDictionary.Values)
                {
                    message += FormatQuestInfo(quest) + ISPLIT;
                }
                message = message.Substring(0, message.Length - ISPLIT.Length);
                message += WORLD_QUESTS_END;
                ch.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void SendWorldTalents(Character ch)
        {
            try
            {
                string message = WORLD_TALENTS;
                foreach (GameTalent talent in GameTalent.GameTalentDictionary.Values)
                {
                    message += FormatTalentInfo(talent) + ISPLIT;
                }
                message = message.Substring(0, message.Length - ISPLIT.Length);
                message += WORLD_TALENTS_END;
                ch.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void SendWorldVersion(Character ch)
        {
            try
            {
                string message = ProtocolYuusha.VERSION_SERVER + DragonsSpineMain.Instance.Settings.ServerVersion + ProtocolYuusha.VERSION_SERVER_END;
                message += ProtocolYuusha.VERSION_CLIENT + DragonsSpineMain.Instance.Settings.ClientVersion + ProtocolYuusha.VERSION_CLIENT_END;
                message += ProtocolYuusha.VERSION_MASTERROUNDINTERVAL + DragonsSpineMain.MasterRoundInterval + ProtocolYuusha.VERSION_MASTERROUNDINTERVAL_END;
                ch.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void sendWorldScores(Character ch)
        {
            try
            {
                System.Collections.ArrayList scores = DAL.DBWorld.p_getScores();
                string message = ProtocolYuusha.WORLD_SCORES;
                for (int a = 0; a < MAX_SCORES; a++)
                {
                    message += FormatScoreInfo((PC)scores[a]) + ISPLIT;
                }
                message = message.Substring(0, message.Length - ISPLIT.Length);
                message += ProtocolYuusha.WORLD_SCORES_END;
                ch.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void sendWorldCharGen(Character ch)
        {
            try
            {
                foreach (Character newbie in CharGen.Newbies)
                {
                    if (!newbie.IsHybrid) // These aren't newbies in CharGen.
                    {
                        string message = ProtocolYuusha.WORLD_CHARGEN_INFO;
                        message += FormatCharGenInfo(newbie);
                        message += ProtocolYuusha.WORLD_CHARGEN_INFO_END;
                        if (message.Length > ProtocolYuusha.WORLD_CHARGEN_INFO.Length + ProtocolYuusha.WORLD_CHARGEN_INFO_END.Length)
                        {
                            ch.Write(message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }
        #endregion

        public static void SendImplementorCharacterFields(Character ch)
        {
            try
            {
                string message = IMP_CHARACTERFIELDS;
                System.Reflection.FieldInfo[] fieldInfo = typeof(Character).GetFields();
                foreach (System.Reflection.FieldInfo info in fieldInfo)
                {
                    message += info.Name + ISPLIT;
                }
                message = message.Substring(0, message.Length - ISPLIT.Length);
                message += IMP_CHARACTERFIELDS_END;
                ch.Write(message);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        #region Message Box
        public static void sendMessageBox(Character ch, string message)
        {
            ch.Write(ProtocolYuusha.MESSAGEBOX + message + ProtocolYuusha.MESSAGEBOX_END);
        }

        public static void sendMessageBox(Character ch, string message, string caption)
        {
            ch.Write(ProtocolYuusha.MESSAGEBOX + message + VSPLIT + caption + ProtocolYuusha.MESSAGEBOX_END);
        }

        public static void sendMessageBox(Character ch, string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
        {
            ch.Write(ProtocolYuusha.MESSAGEBOX +
                message + VSPLIT +
                caption + VSPLIT +
                buttons.ToString() + VSPLIT +
                icon.ToString() +
                ProtocolYuusha.MESSAGEBOX_END);
        } 
        #endregion

        private static void SendGamePrompt(Character ch)
        {
            if (ch.Stunned == 0 && !ch.IsFeared && !ch.IsBlind && !ch.IsResting && !ch.IsMeditating)
            {
                ch.Write(ProtocolYuusha.CHARACTER_PROMPT + PromptStates.Normal.ToString() + ProtocolYuusha.CHARACTER_PROMPT_END);
            }
            else
            {
                if (ch.Stunned > 0)
                {
                    ch.Write(ProtocolYuusha.CHARACTER_PROMPT + PromptStates.Stunned.ToString() + ProtocolYuusha.CHARACTER_PROMPT_END);
                }

                if (ch.IsFeared)
                {
                    ch.Write(ProtocolYuusha.CHARACTER_PROMPT + PromptStates.Feared.ToString() + ProtocolYuusha.CHARACTER_PROMPT_END);
                }

                if (ch.IsBlind)
                {
                    ch.Write(ProtocolYuusha.CHARACTER_PROMPT + PromptStates.Blind.ToString() + ProtocolYuusha.CHARACTER_PROMPT_END);
                }

                if (ch.IsResting && !ch.IsMeditating)
                {
                    ch.Write(ProtocolYuusha.CHARACTER_PROMPT + PromptStates.Resting.ToString() + ProtocolYuusha.CHARACTER_PROMPT_END);
                }

                if (ch.IsMeditating)
                {
                    ch.Write(ProtocolYuusha.CHARACTER_PROMPT + PromptStates.Meditating.ToString() + ProtocolYuusha.CHARACTER_PROMPT_END);
                }
            }
        }

        public static void ShowMap(Character ch)
        {
            if (ch.CurrentCell == null) return;

            ch.seenList.Clear();

            try
            {
                if (ch.IsPeeking)
                {
                    Character target = ch.EffectsList[Effect.EffectTypes.Peek].Target;

                    if (target != null && target.CurrentCell != null)
                        ch.CurrentCell = target.CurrentCell;
                }

                if (ch.Map != null && ch.CurrentCell != null)
                    ch.Map.UpdateCellVisible(ch.CurrentCell); // update visible cells before displaying the map

                var cellArray = Cell.GetApplicableCellArray(ch.CurrentCell, ch.GetVisibilityDistance());
                var fullCellArray = Cell.GetApplicableCellArray(ch.CurrentCell, Cell.DEFAULT_VISIBLE_DISTANCE);

                ch.Write(GAME_NEW_ROUND);

                for (int j = 0; j < cellArray.Length; j++)
                {
                    if (cellArray[j] == null && ch.CurrentCell.visCells[j] && fullCellArray.Length >= j + 1 && fullCellArray[j] != null)
                    {
                        if (fullCellArray[j].HasLightSource(out Globals.eLightSource lightsource) && !AreaEffect.CellContainsLightAbsorbingEffect(fullCellArray[j]))
                        {
                            cellArray[j] = fullCellArray[j];
                        }
                    }

                    // cell is null
                    if (cellArray[j] == null)
                    {
                        ch.Write(ProtocolYuusha.GAME_CELL + ProtocolYuusha.GAME_CELL_END);
                        continue;
                    }

                    // character is immortal, can see all cells TODO: add clairvoyance here
                    if (ch.IsImmortal || ch.HasEffect(Effect.EffectTypes.Gnostikos))
                    {
                        ch.Write(ProtocolYuusha.GAME_CELL + ProtocolYuusha.FormatCellInfo(cellArray[j], ch) + ProtocolYuusha.GAME_CELL_END);
                        continue;
                    }

                    if (ch.IsBlind)// && !ch.HasTalent(Talents.GameTalent.TALENTS.BlindFighting))
                    {
                        ch.Write(ProtocolYuusha.GAME_CELL + ProtocolYuusha.GAME_CELL_END);
                        continue;
                    }
                    else if (ch.IsMeditating)
                    {
                        ch.Write(ProtocolYuusha.GAME_CELL + ProtocolYuusha.GAME_CELL_END);
                        continue;
                    }
                    else if ((ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || ch.CurrentCell.IsAlwaysDark) && !ch.HasNightVision && !ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Light))
                    {
                        ch.Write(ProtocolYuusha.GAME_CELL + ProtocolYuusha.GAME_CELL_END);
                        continue;
                    }

                    if (!ch.CurrentCell.visCells[j])
                        ch.Write(ProtocolYuusha.GAME_CELL + ProtocolYuusha.GAME_CELL_END);
                    else
                        ch.Write(ProtocolYuusha.GAME_CELL + ProtocolYuusha.FormatCellInfo(cellArray[j], ch) + ProtocolYuusha.GAME_CELL_END);
                }

                // update direction pointer
                ch.Write(ch.dirPointer + ProtocolYuusha.GAME_POINTER_UPDATE);

                // end game round
                ch.Write(ProtocolYuusha.GAME_END_ROUND);                

                //Protocol.SendCharacterStats((ch as PC), ch);

                //if(ch.Stunned > 0 || ch.IsFeared || ch.IsBlind || ch.IsResting || ch.IsMeditating)
                    ProtocolYuusha.SendGamePrompt(ch);

                // update right and left hand to client
                ProtocolYuusha.SendCharacterRightHand((ch as PC), ch);
                ProtocolYuusha.SendCharacterLeftHand((ch as PC), ch);

            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }
    }
}