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
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Net.Sockets;
using System.Collections.Specialized;
using DragonsSpine.Commands;
using DragonsSpine.GameWorld;
using GameSpell = DragonsSpine.Spells.GameSpell;
using TextManager = DragonsSpine.GameSystems.Text.TextManager;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine
{
    public class CommandTasker
    {
        /// <summary>
        /// CommandType enumeration is in no particular order.
        /// </summary>
        public enum CommandType
        {
            Attack, // Full Round
            Jumpkick, // Full Round
            Kick, // Full Round
            Poke, // Full Round
            Shoot, // Full Round
            Throw, // Full Round
            Rest, // Full Round
            Crawl, // Full Round
            Movement, // Full Round
            Chant, // Full Round
            Quit,
            Cast, // Full Round
            NPCInteraction,
            Shield_Bash, // Full Round
            Search, 
            Take,
            Meditate,
            Climb, // Full Round
            Backstab, // Full Round
            BattleCharge, // Full Round
            RoundhouseKick, // Full Round
            Nock, // FullRound
            NPC_Command_Attack,
            Cleave, // Full Round
            Leg_Sweep, // Full Round
            Sweep, // with a broom, Full Round
            Assassinate // Full Round
        }

        /// <summary>
        /// Commands that require a full round before another command may be processed.
        /// </summary>
        public static List<CommandType> FullRoundCommands = new List<CommandType>() { CommandType.Attack, CommandType.Jumpkick, CommandType.Kick,
            CommandType.Poke, CommandType.Rest, CommandType.Crawl, CommandType.Movement, CommandType.Chant, CommandType.Quit, CommandType.Cast,
            CommandType.Shield_Bash, CommandType.Search, CommandType.Meditate, CommandType.Throw, CommandType.Nock, CommandType.NPC_Command_Attack,
            CommandType.NPCInteraction, CommandType.RoundhouseKick, CommandType.Sweep, CommandType.Assassinate, CommandType.Shoot };

        public static List<CommandType> BreakFollowCommands = new List<CommandType>() { CommandType.Attack, CommandType.Jumpkick, CommandType.Kick,
            CommandType.Poke, CommandType.Rest, CommandType.Crawl, CommandType.Movement, CommandType.Chant, CommandType.Quit, CommandType.Cast,
            CommandType.NPCInteraction, CommandType.Shield_Bash, CommandType.Search, CommandType.Meditate, CommandType.Sweep };

        private readonly Character _chr = null;

        #region Constructor (1)
        public CommandTasker(Character chr)
        {
            this._chr = chr;
        }
        #endregion

        public bool this[string command, string args]
        {
            get
            {
                if (GameCommand.GameCommandDictionary.ContainsKey(command))
                    return GameCommand.GameCommandDictionary[command].Handler.OnCommand(_chr, args);
                else return false;
            }
        }

        public bool this[string command]
        {
            get { return GameCommand.GameCommandDictionary[command].Handler.OnCommand(_chr, ""); }
        }

        public static bool ParseCommand(Character chr, string command, string args)
        {
            if (!chr.IsPC)
            {
                chr.LastCommand = command + " " + args;
                chr.CommandWeight = 0; // always reset an NPC's command weight...
            }

            var cmd = new CommandTasker(chr);
            var fullinput = command + " " + args;
            var newArgs = "";

            if (command.ToLower().StartsWith("say"))
            {
                command = "/";
            }

            if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol || chr.CommandWeight <= 3)
            {
                if (!command.StartsWith("$"))
                {
                    var chatPos = 0;
                    var i = 0;

                    #region goto SlashChat
                    if (fullinput.IndexOf("\"") != -1 && fullinput.IndexOf("/") != -1)
                    {
                        if (fullinput.IndexOf("/") < fullinput.IndexOf("\""))
                        {
                            goto SlashChat;
                        }
                    }
                    #endregion

                    chatPos = fullinput.IndexOf("\"");

                    #region Found quotation mark, separate command from chat
                    if (chatPos > -1) // Break out the chat string
                    {
                        string chatCommand = fullinput.Substring(chatPos + 1);

                        if (chatPos > 0)
                        {
                            string[] commandOne = fullinput.Substring(0, chatPos).Split(" ".ToCharArray());// newCommand[0].Split(" ".ToCharArray());

                            for (i = 1; i <= commandOne.GetUpperBound(0); i++)
                            {
                                newArgs = newArgs + " " + commandOne[i];
                            }

                            command = commandOne[0].Trim();

                            args = newArgs.Trim();

                            cmd.InterpretCommand(command, args);
                        }

                        cmd.InterpretCommand("/", chatCommand);

                        return true;
                    }
                    #endregion

                SlashChat:
                    #region Separate command from slash chat
                    chatPos = fullinput.IndexOf("/");

                    if (chatPos > -1) // Break out the chat string
                    {
                        string chatCommand = fullinput.Substring(chatPos + 1);

                        if (chatPos > 0)
                        {
                            string[] commandOne = fullinput.Substring(0, chatPos).Split(" ".ToCharArray());// newCommand[0].Split(" ".ToCharArray());

                            for (i = 1; i <= commandOne.GetUpperBound(0); i++)
                            {
                                newArgs = newArgs + " " + commandOne[i];
                            }

                            command = commandOne[0].Trim();

                            args = newArgs.Trim();

                            cmd.InterpretCommand(command, args);
                        }

                        cmd.InterpretCommand("/", chatCommand);
                        return true;
                    }
                    #endregion
                }

                cmd.InterpretCommand(command, args);

                if (chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol) ProtocolYuusha.ShowMap(chr);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Interpret incoming command. To be moved to a Command Manager or GameCommand in the near future.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <returns>False if invalid command. True if the command was completed.</returns>
        public bool InterpretCommand(string command, string args)
        {
            var i = 0;
            var sCmdAndArgs = command + " " + args;
            var newArgs = "";

            // Dead characters can only rest or quit.
            if (_chr.IsDead && command.ToLower() != "rest" && command.ToLower() != "quit")
            {
                _chr.WriteToDisplay("You are dead, you can either wait to be resurrected or rest.");
                return true;
            }

            #region If a command is being interpreted then the character is no longer resting or meditating.
            if (_chr.IsResting && !_chr.IsMeditating)
            {
                if (command.ToLower() != "rest" &&
                    !command.StartsWith(((char)27).ToString()) &&
                    !command.StartsWith("show"))
                {
                    _chr.WriteToDisplay("You are no longer resting.");

                    _chr.IsResting = false;
                }
            }

            if (_chr.IsMeditating)
            {
                // using the "meditate" command while meditating will cancel meditation
                if (command.ToLower() != "memorize" &&
                    !command.StartsWith(((char)27).ToString()) && // protocol command
                    !command.StartsWith("show"))
                {
                    _chr.WriteToDisplay("You are no longer meditating.");
                    _chr.IsResting = false;
                    _chr.IsMeditating = false;
                }
            } 
            #endregion

            // Any command while peeking will be an automatic rest and break the character out of the trance.
            if (_chr.IsPeeking)
            {
                command = "rest";
                args = "";
            }

            #region If wizard eye
            // Only movement, look, rest and again are acceptable commands when in wizard eye.
            if (_chr.IsWizardEye)
            {
                command = command.ToLower();
                switch (command.ToLower())
                {
                    case "n":
                    case "north":
                    case "s":
                    case "south":
                    case "e":
                    case "east":
                    case "w":
                    case "west":
                    case "ne":
                    case "northeast":
                    case "nw":
                    case "northwest":
                    case "se":
                    case "southeast":
                    case "sw":
                    case "southwest":
                    case "d":
                    case "down":
                    case "u":
                    case "up":
                    case "a":
                    case "again":
                    case "l":
                    case "look":
                    case "rest":
                        break;
                    default:
                        _chr.WriteToDisplay("You cannot use that command while polymorphed.");
                        return true;

                }
            } 
            #endregion

            if (_chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
            {
                if (ProtocolYuusha.CheckGameCommand(_chr, command, args))
                    return true;
            }

            // catch some commands before they go through the parser

            #region If Speech. Command starts with a / or a " while in game.
            if (command.StartsWith("/") || command.StartsWith("\""))
            {
                if (!_chr.IsImmortal && _chr.HasEffect(Effect.EffectTypes.Silence))
                {
                    _chr.WriteToDisplay("You have been silenced and are unable to speak.");
                    return true;
                }

                if (args.IndexOf("/!") != -1)
                {
                    _chr.SendShout(_chr.Name + ": " + args.Substring(0, args.IndexOf("/!")));
                    if (_chr.IsPC)
                    {
                        _chr.WriteToDisplay("You shout: " + args.Substring(0, args.IndexOf("/!")));
                        Utils.Log(_chr.Name + ": " + args.Substring(0, args.IndexOf("/!")), Utils.LogType.PlayerChat);
                    }
                    return true;
                }
                else if (args.IndexOf("\"!") != -1)
                {
                    _chr.SendShout(_chr.Name + ": " + args.Substring(0, args.IndexOf("\"!")));
                    if (_chr.IsPC)
                    {
                        _chr.WriteToDisplay("You shout: " + args.Substring(0, args.IndexOf("\"!")));
                        Utils.Log(_chr.Name + ": " + args.Substring(0, args.IndexOf("\"!")), Utils.LogType.PlayerChat);
                    }
                    return true;
                }
                else
                {
                    _chr.SendToAllInSight(_chr.Name + ": " + args);
                    if (_chr.IsPC)
                    {
                        _chr.WriteToDisplay("You say: " + args);
                        Utils.Log(_chr.Name + ": " + args, Utils.LogType.PlayerChat);
                    }
                    return true;
                }
            }
            #endregion

            #region NPC interaction. Command ends with a comma (,)
            else if (command.EndsWith(","))// && !command.ToLower().StartsWith("all"))
            {
                if (!_chr.IsImmortal && _chr.HasEffect(Effect.EffectTypes.Silence))
                {
                    _chr.WriteToDisplay("You have been silenced and are unable to speak.");
                    return true;
                }

                try
                {
                    // djinn, take halberd
                    // djinn, take 2 halberd
                    // 2 djinn, take 2 halberd
                    // all, follow me

                    string targetName = command.Substring(0, command.IndexOf(",")); // definitely going to be a comma

                    string[] targetArgs = targetName.Split(" ".ToCharArray()); // if this is longer than 1 in length probably using #

                    args = args.Trim();

                    string[] sArgs = args.Split(" ".ToCharArray());

                    if (String.IsNullOrEmpty(sArgs[0]) || sArgs.Length == 0) return false;

                    string order = sArgs[0];

                    args = args.Replace(order, "");

                    args = args.Trim();

                    #region All, do something
                    if (command.ToLower().StartsWith("all"))
                    {
                        if (_chr.Pets == null || _chr.Pets.Count == 0)
                        {
                            _chr.WriteToDisplay("There is nothing here to interact with.");
                            return true;
                        }

                        bool interactPositive = false;

                        foreach (NPC pet in new List<NPC>(_chr.Pets))
                        {
                            if (TargetAcquisition.FindTargetInView(_chr, pet) != null)
                            {
                                interactPositive = AI.Interact(_chr, pet, order, args);
                            }
                        }

                        return interactPositive;
                    } 
                    #endregion

                    Character target = null;

                    var countTo = 0;

                    // Not using ALL, try 2 <target>, do something.
                    if (targetArgs.Length > 1 && Int32.TryParse(targetArgs[0], out countTo))
                        target = TargetAcquisition.FindTargetInView(_chr, targetArgs[1].ToLower(), countTo);

                    // Not using ALL, try <target>, do something.
                    if (target == null && targetArgs.Length == 1)
                    {
                        target = TargetAcquisition.FindTargetInView(_chr, targetArgs[0].ToLower(), 1);

                        if (target == null)
                            target = TargetAcquisition.FindTargetInView(_chr, targetName, false, false);
                    }

                    if (target == null)
                    {
                        _chr.WriteToDisplay("You do not see " + targetName + " here.");
                        return true;
                    }
                    else return AI.Interact(_chr, target, order, args);
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                    return false;
                }
            }
            #endregion
            else
            {
                int andStrPos = 0;

                #region Check for command joining, split them then parse and int
                // If command has the word "and" for joining two commands.
                // 9/24/2019 We don't still need this.
                //if (_chr.PCState == Globals.ePlayerState.PLAYING && sCmdAndArgs.IndexOf(" and ") != -1 && command.ToLower() != "tell" && )
                //    sCmdAndArgs = sCmdAndArgs.Replace(" and ", ";");

                andStrPos = sCmdAndArgs.IndexOf(";", 0, sCmdAndArgs.Length);

                if (andStrPos > 0)
                {
                    //Break out the two commands
                    string[] sCommands = sCmdAndArgs.Split(";".ToCharArray());

                    if (sCommands[0].Length > 0)
                    {
                        //get the command and the args
                        sCommands[0] = sCommands[0].Trim();//.ToLower();

                        string[] sArgss = sCommands[0].Split(" ".ToCharArray());

                        for (i = 1; i <= sArgss.GetUpperBound(0); i++)
                        {
                            newArgs = newArgs + " " + sArgss[i];
                        }

                        ParseCommand(_chr, sArgss[0].ToString().Trim(), newArgs.Trim());
                        //chr.FirstJoinedCommand = chr.CommandsProcessed[0];
                        //chr.FirstJoinedCommand = chr.CommandType;
                    }

                    if (sCommands[1].Length > 0)
                    {
                        //get the command and the args
                        sCommands[1] = sCommands[1].Trim();//.ToLower();

                        string[] s2Argss = sCommands[1].Split(" ".ToCharArray());

                        newArgs = "";

                        for (i = 1; i <= s2Argss.GetUpperBound(0); i++)
                        {
                            if (s2Argss[i].ToLower() != "it")
                            { 
                                newArgs = newArgs + " " + s2Argss[i];
                            }
                            else
                            {
                                string[] sArgss = sCommands[0].Split(" ".ToCharArray());
                                newArgs = newArgs + " " + sArgss[1];
                            }
                        }

                        ParseCommand(_chr, s2Argss[0].ToString().Trim(), newArgs.Trim());
                        //chr.FirstJoinedCommand = CommandType.None;
                    }

                    return true;
                }
                #endregion

                if (command.StartsWith("$"))
                {
                    if (command.Contains("list")) { args = "list"; }
                    else if (command.Length > 1 && command.Length < 4)
                    {
                        args = command.Substring(1, command.Length - 1) + " " + args;
                        args.Trim();
                    }

                    command = "macro";
                }

                // lower case
                command = command.ToLower();

                #region Check command aliases and change the command if an alias is found.
                if (GameCommand.GameCommandAliases.ContainsKey(command))
                {
                    command = GameCommand.GameCommandAliases[command];
                }
                else if (GameCommand.GameCommandAliases.ContainsKey(command + " " + args))
                {
                    command = GameCommand.GameCommandAliases[command + " " + args];
                } 
                #endregion

                // check for the command in the dictionary
                if (GameCommand.GameCommandDictionary.ContainsKey(command))
                {
                    // non player characters make no checks to perform commands
                    if (!(_chr is PC))
                    {
                        return GameCommand.GameCommandDictionary[command].Handler.OnCommand(_chr, args);
                    }

                    GameCommand gc = GameCommand.GameCommandDictionary[command];

                    // check player ImpLevel and the command may be executed while playing the game
                    if ((_chr as PC).ImpLevel >= (Globals.eImpLevel)gc.PrivLevel &&
                        Array.IndexOf(gc.States, _chr.PCState) > -1)
                    {
                        _chr.CommandWeight += gc.Weight;

                        return GameCommand.GameCommandDictionary[command].Handler.OnCommand(_chr, args);
                    }

                    return false;
                }
                else if (Talents.GameTalent.GameTalentDictionary.ContainsKey(command))
                {
                    Talents.GameTalent gameTalent = Talents.GameTalent.GameTalentDictionary[command];

                    // immortal flag added for testing purposes
                    if ((_chr.IsImmortal || _chr.talentsDictionary.ContainsKey(command)) && !gameTalent.IsPassive)
                    {
                        if (_chr.IsImmortal || (DateTime.UtcNow - gameTalent.DownTime >= _chr.talentsDictionary[command]))
                        {
                            if (!gameTalent.MeetsPerformanceCost(_chr))
                            {
                                _chr.WriteToDisplay("You do not have enough stamina to perform the " + gameTalent.Name + " talent.");
                                return true;
                            }

                            if (!Talents.GameTalent.GameTalentDictionary[command].Handler.OnPerform(_chr, args))
                                _chr.WriteToDisplay("Talent not performed.");
                            else
                                gameTalent.SuccessfulPerformance(_chr);
                        }
                        else
                        {
                            int roundsRemaining = Utils.TimeSpanToRounds(gameTalent.DownTime - (DateTime.UtcNow - _chr.talentsDictionary[command]));
                            _chr.WriteToDisplay("Talent timer has not reset yet. It will be available " + (roundsRemaining > 0 ? "in " + roundsRemaining : "next") + " round" + (roundsRemaining > 1 ? "s" : "") + ".");
                        }
                    }
                    else if (_chr.talentsDictionary.ContainsKey(command) && gameTalent.IsPassive)
                        _chr.WriteToDisplay(gameTalent.Name + " is a passive talent.");
                    else
                        _chr.WriteToDisplay("You do not know how to perform the " + gameTalent.Name + " talent.");

                    return true;
                }
                else // command does not exist in the dictionary
                {
                    MethodInfo[] methodInfo = typeof(CommandTasker).GetMethods();

                    try
                    {
                        foreach (MethodInfo m in methodInfo)
                        {
                            int length = m.Name.Length;
                            if (m.Name.IndexOf("_") != -1)
                            {
                                length = m.Name.IndexOf("_");
                            }
                            if (m.Name.ToLower().Substring(0, length) == command)
                            {
                                _chr.CommandWeight += 1; // add one to the command weight

                                object[] obj = new object[1];

                                obj[0] = args;

                                try
                                {
                                    m.Invoke(this, obj);
                                }
                                catch (Exception e)
                                {
                                    Utils.LogException(e);
                                }
                                return true;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _chr.WriteToDisplay("Error processing your command. Please report this.");
                        Utils.Log("Failure at Command.interpretCommand(" + command + ", " + args + ")", Utils.LogType.SystemFailure);
                        Utils.LogException(e);
                        return false;
                    }
                }
            }

            #region Spell Chant Exists
            if (_chr.spellDictionary.ContainsValue(command.ToLower() + " " + args.ToLower()))
            {
                if (!_chr.IsImmortal && _chr.InUnderworld)
                {
                    _chr.WriteToDisplay(TextManager.COMMAND_NOT_UNDERSTOOD);
                    return true;
                }

                if(!_chr.IsImmortal && _chr.HasEffect(Effect.EffectTypes.Silence))
                {
                    _chr.WriteToDisplay("You have been silenced and are unable to warm spells.");
                    return true;
                }

                if (_chr.preppedSpell != null) // TODO talent here to automatically clear prepped spell
                {
                    _chr.WriteToDisplay("You already have a spell warmed. Either cast it or rest to clear your mind.");
                    return true;
                }
                else
                {
                    _chr.CommandWeight += 3;

                    if (_chr.CommandWeight == 3)
                    {
                        _chr.CommandType = CommandType.Chant;
                        foreach (int spellID in _chr.spellDictionary.Keys)
                        {
                            if (_chr.spellDictionary[spellID] == (command.ToLower() + " " + args.ToLower()))
                            {
                                _chr.WarmSpell(spellID);
                                break;
                            }
                        }

                        //if(_chr is PC)
                        //    World.magicCordThisRound.Add(_chr.FacetID + "|" + _chr.LandID + "|" + _chr.MapID + "|" + _chr.X + "|" + _chr.Y + "|" + _chr.Z);
                        
                        _chr.PreppedRound = DragonsSpineMain.GameRound;
                        int bitcount = 0;
                        Cell curCell = null;
                        // this is the same logic from chr.sendToAllInSight, however we add spell info in some situations
                        // loop through all visible cells

                        for (int ypos = -3; ypos <= 3; ypos += 1)
                        {
                            for (int xpos = -3; xpos <= 3; xpos += 1)
                            {
                                if (Cell.CellRequestInBounds(_chr.CurrentCell, xpos, ypos))
                                {
                                    if (_chr.CurrentCell.visCells[bitcount]) // check the PC list, and char list of the cell
                                    {
                                        curCell = Cell.GetCell(_chr.FacetID, _chr.LandID, _chr.MapID, _chr.X + xpos, _chr.Y + ypos, _chr.Z);

                                        if (curCell != null)
                                        {
                                            foreach (Character character in curCell.Characters.Values) // search for the character in the charlist of the cell
                                            {
                                                if (character != _chr && character is PC) // players sending messages to other players
                                                {
                                                    if (Array.IndexOf((character as PC).ignoreList, _chr.UniqueID) == -1) // ignore list
                                                    {
                                                        if ((_chr.Group != null && _chr.preppedSpell != null && _chr.Group.GroupMemberIDList.Contains(character.UniqueID)) ||
                                                            (character as PC).ImpLevel >= Globals.eImpLevel.GM ||
                                                            character.HasEffect(Effect.EffectTypes.Gnostikos))
                                                        {
                                                            character.WriteToDisplay(_chr.Name + ": " + command + " " + args + " <" + _chr.preppedSpell.Name + ">");
                                                        }
                                                        else
                                                        {
                                                            character.WriteToDisplay(_chr.Name + ": " + command + " " + args);
                                                        }
                                                    }
                                                }
                                                else if (character != _chr && character.IsPC && !_chr.IsPC) // non players sending messages to other players
                                                {
                                                    character.WriteToDisplay(_chr.Name + ": " + command + " " + args);
                                                }

                                                //TODO: else... detect spell casting by sensitive NPC
                                            }
                                        }
                                    }
                                    bitcount++;
                                }
                            }
                        }
                    }
                    else
                    {
                        _chr.WriteToDisplay("Warming a spell requires your full concentration.");
                    }
                }
                return true;
            }
            #endregion

            Utils.Log(_chr.GetLogString() + " Invalid command: " + command + " args: " + args, Utils.LogType.CommandFailure);
            _chr.WriteToDisplay("I don't understand your command. For a full list of game commands visit the Dragon's Spine forums.");
            return false;
        }

        #region Implementor Commands

        public void impunlock(string args)
        {
            if ((_chr as PC).ImpLevel >= Globals.eImpLevel.GM)
            {
                string[] sArgs = args.Split(" ".ToCharArray());
                //string dir = sArgs[0];

                Cell cell = Map.GetCellRelevantToCell(_chr.CurrentCell, sArgs[0], true);

                if (cell != null)
                {
                    Map.UnlockDoor(cell, cell.cellLock.key);
                    _chr.WriteToDisplay("You unlock the door.");
                }
            }
            else
            {
                _chr.WriteToDisplay("I don't understand your command.");
            }
        }

        public void impgetnpcfx(string args)
        {
            if ((_chr as PC).ImpLevel < Globals.eImpLevel.DEV)
            {
                _chr.WriteToDisplay("I don't understand your command.");
                return;
            }
            else
            {
                if (args == null)
                {
                    _chr.WriteToDisplay("getnpceffects (target)");
                    return;
                }
                else
                {
                    string[] sArgs = args.Split(" ".ToCharArray());
                    Character target = TargetAcquisition.FindTargetInView(_chr, sArgs[0], false, true);
                    if (target != null)
                    {
                        string effectslisting = "";
                        foreach(Effect fx in target.EffectsList.Values)
                        {
                            effectslisting += " [" + Effect.GetEffectName(fx.EffectType) + " Amount: " + fx.Power + " Duration: " + fx.Duration + "]";
                        }
                        _chr.WriteToDisplay(target.Name + "'s Effects " + effectslisting);
                        _chr.WriteToDisplay(target.Name + "'s Protections FIRE: Resist: " + target.FireResistance + " Pro: " + target.FireProtection + " ICE: Resist: " + target.ColdResistance + " Pro: " + target.ColdProtection + " LIGHTNING: Resist: " + target.LightningResistance + " Pro: " + target.LightningProtection +
                            " POISON: Resist: " + target.PoisonResistance + " Pro: " + target.PoisonProtection + " BLIND: Resist: " + target.BlindResistance + " STUN: Resist: " + target.StunResistance + " DEATH: Resist: " + target.DeathResistance + " Pro: " + target.DeathProtection + " BRWATER: " + target.CanBreatheWater.ToString() +
                            " FEATHERFALL: " + target.HasFeatherFall.ToString() + " CANSWIM: " + target.CanBreatheWater.ToString() + " CANFLY: " + target.CanFly.ToString() + " CANCOMMAND: " + target.canCommand.ToString() + " Shield: " + target.Shielding + " NV: " + target.HasNightVision.ToString());
                    }
                    else
                    {
                        _chr.WriteToDisplay("Could not find " + sArgs[0] + ".");
                        return;
                    }
                }
            }
        }

        public void impgetpcfx(string args)
        {
            if ((_chr as PC).ImpLevel < Globals.eImpLevel.DEV)
            {
                _chr.WriteToDisplay("I don't understand your command.");
                return;
            }
            else
            {
                if (args == null)
                {
                    _chr.WriteToDisplay("getpceffects (player)");
                    return;
                }
                else
                {
                    string[] sArgs = args.Split(" ".ToCharArray());
                    Character target = new Character();
                    foreach (Character chra in Character.PCInGameWorld)
                    {
                        if (chra.Name.ToLower() == sArgs[0].ToLower())
                        {
                            target = chra;
                        }
                    }
                    if (target != null || target.Name != "Nobody")
                    {
                        string effectslisting = "";
                        foreach (Effect fx in target.EffectsList.Values)
                        {
                            effectslisting += " [" + Effect.GetEffectName(fx.EffectType) + " Amount: " + fx.Power + " Duration: " + fx.Duration + "]";
                        }
                        _chr.WriteToDisplay(target.Name + "'s Effects " + effectslisting);
                        _chr.WriteToDisplay(target.Name + "'s Protections FIRE: Resist: " + target.FireResistance + " Pro: " + target.FireProtection + " ICE: Resist: " + target.ColdResistance + " Pro: " + target.ColdProtection + " LIGHTNING: Resist: " + target.LightningResistance + " Pro: " + target.LightningProtection +
                            " POISON: Resist: " + target.PoisonResistance + " Pro: " + target.PoisonProtection + " BLIND: Resist: " + target.BlindResistance + " STUN: Resist: " + target.StunResistance + " DEATH: Resist: " + target.DeathResistance + " Pro: " + target.DeathProtection + " BRWATER: " + target.CanBreatheWater.ToString() +
                            " FEATHERFALL: " + target.HasFeatherFall.ToString() + " CANSWIM: " + target.CanBreatheWater.ToString() + " CANFLY: " + target.CanFly.ToString() + " CANCOMMAND: " + target.canCommand.ToString() + " Shield: " + target.Shielding);
                    }
                    else
                    {
                        _chr.WriteToDisplay("Could not find " + sArgs[0] + ".");
                        return;
                    }
                }
            }
        }

        public void impgetpcstats(string args)
        {
            if ((_chr as PC).ImpLevel < Globals.eImpLevel.DEV)
            {
                _chr.WriteToDisplay("I don't understand your command.");
                return;
            }
            else
            {
                if (args == null)
                {
                    _chr.WriteToDisplay("getpcstats (player)");
                    return;
                }
                else
                {
                    string[] sArgs = args.Split(" ".ToCharArray());
                    PC target = new PC();
                    foreach (PC chra in Character.PCInGameWorld)
                    {
                        if (chra.Name.ToLower() == sArgs[0].ToLower())
                        {
                            target = chra;
                        }
                    }
                    if (target.Name != "Nobody")
                    {
                        _chr.WriteToDisplay(target.Name + "'s stats: CLASS: " + target.BaseProfession + " LEVEL: " + target.Level + "("+Rules.GetExpLevel(target.Experience)+")" +
                            " STR:" + target.Strength + " DEX:" + target.Dexterity + " INT:" + target.Intelligence + " WIS:" + target.Wisdom +
                            " CON:" + target.Constitution + " CHR:" + target.Charisma + " HP:" + target.Hits + "/" + target.HitsFull +
                            " STAM:" + target.Stamina + "/" + target.StaminaFull + " MANA:" + target.Mana + "/" + target.ManaFull + " XP:" + target.Experience +
                            " BANK:" + target.bankGold + " KILLS:" + target.Kills + " DEATHS:" + target.Deaths + " MANAREGEN: " + target.manaRegen);
                    }
                    else
                    {
                        _chr.WriteToDisplay("Could not find " + sArgs[0] + ".");
                        return;
                    }
                }
            }
        }

        public void imptakexp(string args)
        {
            if ((_chr as PC).ImpLevel < Globals.eImpLevel.DEV)
            {
                _chr.WriteToDisplay("I don't understand your command.");
                return;
            }
            else
            {
                if (args == null)
                {
                    _chr.WriteToDisplay("takexp (amount) (target)");
                }
                else
                {
                    String[] sArgs = args.Split(" ".ToCharArray());
                    int amount = Convert.ToInt32(sArgs[0]);
                    Character target = new Character();
                    foreach (Character chra in Character.PCInGameWorld)
                    {
                        if (chra.Name.ToLower() == sArgs[1].ToLower())
                        {
                            target = chra;
                        }
                    }
                    if (target.Name != "Nobody")
                    {
                        target.Experience -= amount;
                    }
                    else
                    {
                        _chr.WriteToDisplay("Could not find target.");
                        return;
                    }
                }
            }
        }

        public void impsetstat(string args)
        {
            if ((_chr as PC).ImpLevel < Globals.eImpLevel.DEV)
            {
                _chr.WriteToDisplay("I don't understand your command.");
                return;
            }
            else
            {
                if (args == null)
                {
                    _chr.WriteToDisplay("impsetstat ('str ... cha, implevel, invis, align') (value) (player)");
                    return;
                }
                else
                {
                    String[] sArgs = args.Split(" ".ToCharArray());

                    if(sArgs.Length < 3)
                    {
                        _chr.WriteToDisplay("impsetstat ('str ... cha, implevel, invis, align') (value) (player)");
                        return;
                    }

                    string stat = sArgs[0].ToLower();

                    int num = 0;

                    Int32.TryParse(sArgs[1], out num);

                    PC target = null;

                    foreach (PC chra in Character.PCInGameWorld)
                    {
                        if (chra.Name.ToLower() == sArgs[2].ToLower())
                        {
                            target = chra;
                        }
                    }

                    if (target != null)
                    {
                        switch (stat)
                        {
                            case "con":
                                _chr.WriteToDisplay(target.Name + "'s constitution changed from " + target.Constitution + " to " + num + ".");
                                target.Constitution = num;
                                break;
                            case "str":
                                _chr.WriteToDisplay(target.Name + "'s strength changed from " + target.Strength + " to " + num + ".");
                                target.Strength = num;
                                break;
                            case "dex":
                                _chr.WriteToDisplay(target.Name + "'s dexterity changed from " + target.Dexterity + " to " + num + ".");
                                target.Dexterity = num;
                                break;
                            case "int":
                                _chr.WriteToDisplay(target.Name + "'s intelligence changed from " + target.Intelligence + " to " + num + ".");
                                target.Intelligence = num;
                                break;
                            case "wis":
                                _chr.WriteToDisplay(target.Name + "'s wisdom changed from " + target.Wisdom + " to " + num + ".");
                                target.Wisdom = num;
                                break;
                            case "cha":
                                _chr.WriteToDisplay(target.Name + "'s charisma changed from " + target.Charisma + " to " + num + ".");
                                target.Charisma = num;
                                break;
                            case "implevel":
                                _chr.WriteToDisplay(target.Name + "'s implevel changed from " + target.ImpLevel + " to " + num + ".");
                                target.ImpLevel = (Globals.eImpLevel)num;
                                break;
                            case "invis":
                                if (num == 1)
                                {
                                    _chr.WriteToDisplay(target.Name + " is now invisible.");
                                    if (target.Name != _chr.Name) { target.WriteToDisplay("You are now invisible."); }
                                    target.IsInvisible = true;
                                }
                                else
                                {
                                    _chr.WriteToDisplay(target.Name + " is no longer invisible.");
                                    if (target.Name != _chr.Name) { target.WriteToDisplay("You are no longer invisible."); }
                                    target.IsInvisible = false;
                                }
                                break;
                            case "align":
                            case "alignment":
                                {
                                    Globals.eAlignment alignment = target.Alignment;

                                    try
                                    {
                                        alignment = (Globals.eAlignment)Enum.Parse(typeof(Globals.eAlignment), sArgs[1], false);
                                    }
                                    catch
                                    {
                                        _chr.WriteToDisplay("Invalid alignment '" + sArgs[2] + "'");
                                    }

                                    _chr.WriteToDisplay(target.Name + "'s alignment changed from " + target.Alignment + " to " + alignment + ".");

                                    target.Alignment = alignment;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    return;
                }
            }
        }
        #endregion

        public void forcequit(string args)
        {
            if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Locked)
            {
                if (_chr.protocol == "old-kesmai") { _chr.Write(Map.KP_ENHANCER_DISABLE); }
                _chr.RemoveFromWorld();
                _chr.PCState = Globals.ePlayerState.CONFERENCE;
                (_chr as PC).AddToConf();
                Conference.Header(_chr as PC, true);
                PC pc = (PC)_chr;
                System.Threading.Thread saveThread = new System.Threading.Thread(pc.Save);
                saveThread.Start();
                return;
            }
        }
    }
}
