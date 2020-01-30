using System;
using System.Collections.Generic;
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine.Commands
{
    [CommandAttribute("displaydocument", "Output documentation to the conference display.", (int)Globals.eImpLevel.DEVJR, new string[] { "dispdocu" },
        0, new string[] { "dispdocu commands, dispdocu commands user, dispdocu spells [profession], dispdocu talents" }, Globals.ePlayerState.CONFERENCE)]
    public class DisplayDocumentCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            switch (sArgs[0])
            {
                case "commands":
                case "comms":
                    #region Commands List
                    var cmdsList = new List<Commands.GameCommand>();
                    var privLevel = (int)(chr as PC).ImpLevel;
                    if (args.ToLower().Contains("user"))
                        privLevel = (int)Globals.eImpLevel.USER;

                    // Add GameCommand to list if user priveledge level meets requirements.
                    foreach (DragonsSpine.Commands.GameCommand gameCommand in DragonsSpine.Commands.GameCommand.GameCommandDictionary.Values)
                    {
                        if (gameCommand.PrivLevel <= privLevel && !cmdsList.Contains(gameCommand))
                        {
                            cmdsList.Add(gameCommand);
                        }
                    }

                    chr.WriteLine("You have " + cmdsList.Count + " commands available.", ProtocolYuusha.TextType.Help);

                    // Sorts alphabetically by Command. (attack, cast, etc)
                    cmdsList.Sort((s1, s2) => s1.Command.CompareTo(s2.Command));

                    foreach (Commands.GameCommand sortedCommand in cmdsList)
                    {
                        var usageList = "  Usage List: ";
                        var aliasList = "  Alias List: ";

                        chr.WriteLine(sortedCommand.Command + " - " + sortedCommand.Description, ProtocolYuusha.TextType.Help);

                        Utils.Log(sortedCommand.Command + " - " + sortedCommand.Description, Utils.LogType.DocumentationCommands);

                        foreach (string usage in sortedCommand.Usages)
                        {
                            usageList += usage + ", ";
                        }

                        if (usageList.Length > 14)
                        {
                            usageList = usageList.Substring(0, usageList.Length - 2);
                            chr.WriteLine(usageList, ProtocolYuusha.TextType.Help);
                            Utils.Log(usageList, Utils.LogType.DocumentationCommands);
                        }

                        foreach (string alias in Commands.GameCommand.GameCommandAliases.Keys)
                        {
                            if (Commands.GameCommand.GameCommandAliases[alias] == sortedCommand.Command)
                            {
                                aliasList += alias + ", ";
                            }
                        }

                        if (aliasList.Length > 14)
                        {
                            aliasList = aliasList.Substring(0, aliasList.Length - 2);
                            chr.WriteLine(aliasList, ProtocolYuusha.TextType.Help);
                        }
                        chr.WriteLine("-----");
                    }
                    #endregion
                    break;
                case "spells":
                    #region Spells
                    var spellsList = new List<GameSpell>(GameSpell.GameSpellDictionary.Values);
                    spellsList.Sort((s1, s2) => s1.Name.CompareTo(s2.Name)); // alpha sort by spell name
                    var professionChoice = Character.ClassType.All;
                    if (sArgs.Length > 1)
                    {
                        if (!Enum.TryParse<Character.ClassType>(sArgs[1], true, out professionChoice))
                            professionChoice = Character.ClassType.All;
                    }

                    foreach (GameSpell spell in spellsList)
                    {
                        if (professionChoice != Character.ClassType.All && Array.IndexOf(spell.ClassTypes, professionChoice) == -1)
                        { continue; }

                        chr.WriteLine("Spell: " + spell.Name, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Description: " + spell.Description, ProtocolYuusha.TextType.Help);
                        string professionsList = "Professions: ";
                        foreach (Character.ClassType profession in spell.ClassTypes)
                        {
                            professionsList += Utils.FormatEnumString(profession.ToString()) + ", ";
                        }
                        professionsList = professionsList.Substring(0, professionsList.Length - 2);
                        if (spell.ClassTypes.Length == 1) professionsList = professionsList.Replace("Professions:", "Profession:");
                        else if (spell.ClassTypes.Length == 0) professionsList = "Profession: None";
                        chr.WriteLine(professionsList, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Mana Cost: " + spell.ManaCost, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Price: " + spell.TrainingPrice, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Command: " + spell.Command, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Spell Type: " + Utils.FormatEnumString(spell.SpellType.ToString()), ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Target Type: " + Utils.FormatEnumString(spell.TargetType.ToString()), ProtocolYuusha.TextType.Help);
                        //Utils.Log("Considered Beneficial (for AI): " + spell.IsBeneficial, Utils.LogType.Documentation);
                        chr.WriteLine("Available at Trainer: " + spell.IsAvailableAtTrainer, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("-----", ProtocolYuusha.TextType.Help);
                    }
                    #endregion
                    break;
                case "talents":
                    #region Talents
                    var talentsList = new List<Talents.GameTalent>(Talents.GameTalent.GameTalentDictionary.Values);
                    talentsList.Sort((t1, t2) => t1.Name.CompareTo(t2.Name)); // alpha sort by talent name

                    professionChoice = Character.ClassType.All;
                    if (sArgs.Length > 1)
                    {
                        if (!Enum.TryParse<Character.ClassType>(sArgs[1], true, out professionChoice))
                            professionChoice = Character.ClassType.All;
                    }

                    foreach (Talents.GameTalent talent in talentsList)
                    {
                        if (professionChoice != Character.ClassType.All && talent.IsProfessionElgible(professionChoice))
                        { continue; }

                        chr.WriteLine("Talent: " + talent.Name, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Description: " + talent.Description, ProtocolYuusha.TextType.Help);
                        string professionsList = "Professions: ";
                        int professionCount = 0;
                        foreach (Character.ClassType profession in Enum.GetValues(typeof(Character.ClassType)))
                        {
                            if (talent.IsProfessionElgible(profession))
                            {
                                professionsList += Utils.FormatEnumString(profession.ToString()) + ", ";
                                professionCount++;
                            }
                        }
                        professionsList = professionsList.Substring(0, professionsList.Length - 2);
                        if (professionCount == 1) professionsList = professionsList.Replace("Professions: ", "Profession: ");
                        else if (professionCount == 0) professionsList = "Professions: None";
                        chr.WriteLine(professionsList, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Level: " + talent.MinimumLevel, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Passive: " + talent.IsPassive, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Stamina Cost: " + talent.PerformanceCost, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Price: " + talent.PurchasePrice, ProtocolYuusha.TextType.Help);
                        if (!talent.IsPassive) chr.WriteLine("Command: " + talent.Command, ProtocolYuusha.TextType.Help);
                        if (talent.DownTime.TotalSeconds <= 0) chr.WriteLine("Down Time: None", ProtocolYuusha.TextType.Help);
                        else
                        {
                            chr.WriteLine("Down Time: " + talent.DownTime.TotalSeconds + " seconds (" + Utils.TimeSpanToRounds(talent.DownTime) + " rounds)", ProtocolYuusha.TextType.Help);
                        }
                        chr.WriteLine("Available at Mentor: " + talent.IsAvailableAtMentor, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("-----", ProtocolYuusha.TextType.Help);
                    }
                    #endregion
                    break;
                case "quests":
                    #region Quests
                    var questsList = new List<DragonsSpine.GameQuest>(DragonsSpine.GameQuest.QuestDictionary.Values);
                    questsList.Sort((q1, q2) => q1.Name.CompareTo(q2.Name)); // alpha sort by quest name
                    foreach (GameQuest quest in questsList)
                    {
                        chr.WriteLine("Quest: " + quest.Name, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Notes: " + quest.Notes, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Desc: " + quest.Description, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Alignments: " + Utils.ConvertListToString(quest.Alignments.ToArray()), ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Professions: " + Utils.ConvertListToString(quest.ClassTypes.ToArray()), ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Minimum Level: " + quest.MinimumLevel, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Maximum Level: " + quest.MaximumLevel, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Completed Desc: " + quest.CompletedDescription, ProtocolYuusha.TextType.Help);
                        chr.WriteLine("Repeatable: " + quest.IsRepeatable.ToString(), ProtocolYuusha.TextType.Help);
                    }
                    #endregion
                    break;
            }

            return true;
        }
    }
}