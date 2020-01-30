using System;
using System.Collections.Generic;
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine.Commands
{
    [CommandAttribute("document", "Output documentation files to the logs directory.", (int)Globals.eImpLevel.DEVJR, new string[] { "docu" },
        0, new string[] { "docu commands, docu commands user, docu spells, docu talents" }, Globals.ePlayerState.CONFERENCE)]
    public class DocumentCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            bool outputAsCSV = args.ToLower().Contains("csv");

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

                    foreach (GameCommand sortedCommand in cmdsList)
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

                        foreach (string alias in GameCommand.GameCommandAliases.Keys)
                        {
                            if (GameCommand.GameCommandAliases[alias] == sortedCommand.Command)
                            {
                                aliasList += alias + ", ";
                            }
                        }

                        if (aliasList.Length > 14)
                        {
                            aliasList = aliasList.Substring(0, aliasList.Length - 2);
                            chr.WriteLine(aliasList, ProtocolYuusha.TextType.Help);

                            Utils.Log(aliasList, Utils.LogType.DocumentationCommands);
                        }
                        //chr.WriteLine("-----", ProtocolYuusha.TextType.Help);
                        Utils.Log("-----", Utils.LogType.DocumentationCommands);
                    } 
	#endregion
                    break;
                case "spells":
                    #region Spells
		            var spellsList = new List<GameSpell>(GameSpell.GameSpellDictionary.Values);
                    if (!outputAsCSV)
                        spellsList.Sort((s1, s2) => s1.RequiredLevel.CompareTo(s2.RequiredLevel)); // sort by level
                    else spellsList.Sort((s1, s2) => s1.Name.CompareTo(s2.Name)); // alpha sort by name
                    var professionChoice = Character.ClassType.All;
                    if(sArgs.Length > 1)
                    {
                        if (!Enum.TryParse<Character.ClassType>(sArgs[1], true, out professionChoice)) // docu spells druid csv
                            professionChoice = Character.ClassType.All;
                    }

                    if (outputAsCSV)
                    {
                        string header = "ID,Name,Description,Professions,Level,Mana,Price,Command,School,Target,Beneficial,Available at Trainer,Found for Casting,Found for Scribing,Found Only in Lairs";
                        Utils.Log(header, Utils.LogType.CSVFormat);
                    }

                    foreach (GameSpell spell in spellsList)
                    {
                        if (!outputAsCSV)
                        {
                            if (professionChoice != Character.ClassType.All && Array.IndexOf(spell.ClassTypes, professionChoice) == -1)
                            { continue; }

                        
                            Utils.Log("Spell: " + spell.Name, Utils.LogType.DocumentationSpells);
                            Utils.Log("Description: " + spell.Description, Utils.LogType.DocumentationSpells);
                            string professionsList = "Professions: ";
                            foreach (Character.ClassType profession in spell.ClassTypes)
                            {
                                professionsList += Utils.FormatEnumString(profession.ToString()) + ", ";
                            }
                            professionsList = professionsList.Substring(0, professionsList.Length - 2);
                            if (spell.ClassTypes.Length == 1) professionsList = professionsList.Replace("Professions:", "Profession:");
                            else if (spell.ClassTypes.Length == 0) professionsList = "Profession: None";
                            Utils.Log(professionsList, Utils.LogType.DocumentationSpells);
                            Utils.Log("Skill Level: " + spell.RequiredLevel, Utils.LogType.DocumentationSpells);
                            Utils.Log("Mana Cost: " + spell.ManaCost, Utils.LogType.DocumentationSpells);
                            Utils.Log("Price: " + spell.TrainingPrice, Utils.LogType.DocumentationSpells);
                            Utils.Log("Command: " + spell.Command, Utils.LogType.DocumentationSpells);
                            Utils.Log("Spell Type: " + Utils.FormatEnumString(spell.SpellType.ToString()), Utils.LogType.DocumentationSpells);
                            Utils.Log("Target Type: " + Utils.FormatEnumString(spell.TargetType.ToString()), Utils.LogType.DocumentationSpells);
                            Utils.Log("Beneficial (for AI): " + spell.IsBeneficial, Utils.LogType.DocumentationSpells);
                            Utils.Log("Available at Trainer: " + spell.IsAvailableAtTrainer, Utils.LogType.DocumentationSpells);
                            Utils.Log("Found for Casting (scroll): " + spell.IsFoundForCasting, Utils.LogType.DocumentationSpells);
                            Utils.Log("Found for Scribing (scroll): " + spell.IsFoundForScribing, Utils.LogType.DocumentationSpells);
                            Utils.Log("Only Found in Lairs: " + spell.OnlyFoundInLairs, Utils.LogType.DocumentationSpells);
                            Utils.Log("-----", Utils.LogType.DocumentationSpells);
                        }
                        else
                        {
                            string literalQ = "\",\""; // literal ","
                            string outputRow = "\"" + spell.ID + literalQ + spell.Name + literalQ + spell.Description + literalQ;
                            foreach (Character.ClassType profession in spell.ClassTypes)
                            {
                                outputRow += Utils.FormatEnumString(profession.ToString()) + " ";
                            }
                            outputRow = outputRow.TrimEnd();
                            outputRow += literalQ + spell.RequiredLevel + literalQ + spell.ManaCost + literalQ + spell.TrainingPrice + literalQ + spell.Command + literalQ +
                                Utils.FormatEnumString(spell.SpellType.ToString()) + literalQ + Utils.FormatEnumString(spell.TargetType.ToString()) + literalQ +
                                spell.IsBeneficial + literalQ + spell.IsAvailableAtTrainer + literalQ + spell.IsFoundForCasting + literalQ + spell.IsFoundForScribing + literalQ +
                                spell.OnlyFoundInLairs + "\""; // end quote
                            Utils.Log(outputRow, Utils.LogType.CSVFormat);

                            // only logged locally for now -- could send to client here. Could also use Reflection and CSV all public properties...
                        }
                    } 
	                #endregion
                    break;
                case "talents":
                    #region Talents
		            var talentsList = new List<Talents.GameTalent>(Talents.GameTalent.GameTalentDictionary.Values);
                    talentsList.Sort((t1, t2) => t1.MinimumLevel.CompareTo(t2.MinimumLevel)); // alpha sort by talent name
                    //TODO: sorting by profession
                    foreach (Talents.GameTalent talent in talentsList)
                    {
                        Utils.Log("Talent: " + talent.Name, Utils.LogType.DocumentationTalents);
                        Utils.Log("Description: " + talent.Description, Utils.LogType.DocumentationTalents);
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
                        Utils.Log(professionsList, Utils.LogType.DocumentationTalents);
                        Utils.Log("Level: " + talent.MinimumLevel, Utils.LogType.DocumentationTalents);
                        Utils.Log("Passive: " + talent.IsPassive, Utils.LogType.DocumentationTalents);
                        Utils.Log("Stamina Cost: " + talent.PerformanceCost, Utils.LogType.DocumentationTalents);
                        Utils.Log("Price: " + talent.PurchasePrice, Utils.LogType.DocumentationTalents);
                        if(!talent.IsPassive) Utils.Log("Command: " + talent.Command, Utils.LogType.DocumentationTalents);
                        if (talent.DownTime.TotalSeconds <= 0) Utils.Log("Down Time: None", Utils.LogType.DocumentationTalents);
                        else
                        {
                            Utils.Log("Down Time: " + talent.DownTime.TotalSeconds + " seconds (" + Utils.TimeSpanToRounds(talent.DownTime) + " rounds)", Utils.LogType.DocumentationTalents);
                        }
                        Utils.Log("Available at Mentor: " + talent.IsAvailableAtMentor, Utils.LogType.DocumentationTalents);
                        Utils.Log("-----", Utils.LogType.DocumentationTalents);
                    } 
	                #endregion
                    break;
                case "quests":
                    #region Quests
		            var questsList = new List<DragonsSpine.GameQuest>(DragonsSpine.GameQuest.QuestDictionary.Values);
                    questsList.Sort((q1, q2) => q1.Name.CompareTo(q2.Name)); // alpha sort by quest name
                    foreach (GameQuest quest in questsList)
                    {
                        Utils.Log("Quest: " + quest.Name, Utils.LogType.DocumentationQuests);
                        Utils.Log("Notes: " + quest.Notes, Utils.LogType.DocumentationQuests);
                        Utils.Log("Desc: " + quest.Description, Utils.LogType.DocumentationQuests);
                        Utils.Log("Alignments: " + Utils.ConvertListToString(quest.Alignments.ToArray()), Utils.LogType.DocumentationQuests);
                        Utils.Log("Professions: " + Utils.ConvertListToString(quest.ClassTypes.ToArray()), Utils.LogType.DocumentationQuests);
                        Utils.Log("Minimum Level: " + quest.MinimumLevel, Utils.LogType.DocumentationQuests);
                        Utils.Log("Maximum Level: " + quest.MaximumLevel, Utils.LogType.DocumentationQuests);
                        Utils.Log("Completed Desc: " + quest.CompletedDescription, Utils.LogType.DocumentationQuests);
                        Utils.Log("Repeatable: " + quest.IsRepeatable.ToString(), Utils.LogType.DocumentationQuests);
                    } 
	                #endregion
                    break;
            }

            chr.WriteToDisplay("Documention written to logs directory.");
            return true;
        }
    }
}