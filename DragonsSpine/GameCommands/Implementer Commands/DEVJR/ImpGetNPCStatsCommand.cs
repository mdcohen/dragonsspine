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
    [CommandAttribute("impgetnpcstats", "All proceeding text is sent to every player in your current map.", (int)Globals.eImpLevel.DEVJR, new string[] { "getnpcstats", "npcstats" },
        0, new string[] { "impgetnpcstats <npc in view or world>" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class ImpGetNPCStatsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Usage: impgetnpcstats <npc in view or npc name>");
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            NPC target = (NPC)GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[0], true, true);

            if (target == null)
            {
                for (int a = 0; a < Character.NPCInGameWorld.Count; a++)
                {
                    target = (NPC)Character.NPCInGameWorld[a];

                    if (target.Name.ToLower() == sArgs[0].ToLower())
                    {
                        if (target.MapID == chr.MapID)
                            break;
                    }
                }

                if (target.Name.ToLower() != sArgs[0].ToLower())
                {
                    for (int a = 0; a < Character.NPCInGameWorld.Count; a++)
                    {
                        target = (NPC)Character.NPCInGameWorld[a];
                        if (target.Name.ToLower() == sArgs[0].ToLower())
                        {
                            break;
                        }
                    }
                }
            }

            if (target.Name.ToLower() == sArgs[0].ToLower())
            {
                chr.WriteToDisplay(target.GetLogString());
                chr.WriteToDisplay("STR: " + Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Strength) + " (" + target.Strength.ToString() + ")");
                chr.WriteToDisplay("DEX: " + Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Dexterity) + " (" + target.Dexterity.ToString() + ")");
                chr.WriteToDisplay("INT: " + Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Intelligence) + " (" + target.Intelligence.ToString() + ")");
                chr.WriteToDisplay("WIS: " + Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Wisdom) + " (" + target.Wisdom.ToString() + ")");
                chr.WriteToDisplay("CON: " + Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Constitution) + " (" + target.Constitution.ToString() + ")");
                chr.WriteToDisplay("CHR: " + Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Charisma) + " (" + target.Charisma.ToString() + ")");
                chr.WriteToDisplay("Hits: " + target.Hits + " / " + target.HitsFull);
                chr.WriteToDisplay("Stam: " + target.Stamina + " / " + target.StaminaFull);
                chr.WriteToDisplay("Mana: " + target.Mana + " / " + target.ManaFull);
                chr.WriteToDisplay("Experience Value: " + target.Experience);
                chr.WriteToDisplay("Species: " + target.species.ToString());
                chr.WriteToDisplay("Entity: " + target.entity.ToString());
                chr.WriteToDisplay("ShortDesc: " + target.shortDesc);
                chr.WriteToDisplay("LongDesc: " + target.longDesc);
                chr.WriteToDisplay("VisualKey: " + target.visualKey);
                chr.WriteToDisplay("SpawnZoneID: " + target.SpawnZoneID);
                chr.WriteToDisplay("TotalFearLove: " + target.TotalFearLove);
                chr.WriteToDisplay("TotalHate: " + target.TotalHate);
                if (target.MostHated != null)
                    chr.WriteToDisplay("MostHated: " + target.MostHated.GetNameForActionResult());
                if (target.previousMostHated != null)
                    chr.WriteToDisplay("PreviousMostHated: " + target.previousMostHated.GetNameForActionResult());
                if (target.previousMostHatedsCell != null)
                    chr.WriteToDisplay("PMHCell: " + target.previousMostHatedsCell != null ? target.previousMostHatedsCell.ToString() : "NULL");
                chr.WriteToDisplay("ActionType: " + target.CurrentActionType + " Priority: " + target.CurrentPriority);
                chr.WriteToDisplay("Speed: " + target.Speed);
                chr.WriteToDisplay("Mobile: " + target.IsMobile.ToString());
                chr.WriteToDisplay("[Sounds] Attack: " + target.attackSound + " Idle: " + target.idleSound + " Death: " + target.deathSound);
                chr.WriteToDisplay("Base AC: " + target.baseArmorClass);
                chr.WriteToDisplay("Armor Rating: " + Combat.AC_GetArmorClassRating(target));
                chr.WriteToDisplay("THAC0 Adj: " + target.THAC0Adjustment);
                

                if (target is Merchant)
                {
                    chr.WriteToDisplay("TrainerType: " + (target as Merchant).trainerType);
                    chr.WriteToDisplay("MerchantType: " + (target as Merchant).merchantType);
                    chr.WriteToDisplay("InteractiveType: " + (target as Merchant).interactiveType);
                }

                if (sArgs.Length > 1 && sArgs[1].ToLower() == "full")
                {
                    chr.WriteToDisplay("Kills: " + target.Kills);
                    chr.WriteToDisplay("Hits Regen: " + target.hitsRegen);
                    chr.WriteToDisplay("Stam Regen: " + target.staminaRegen);
                    chr.WriteToDisplay("Mana Regen: " + target.manaRegen);                    

                    if (target.IsSpellUser)
                    {
                        chr.WriteToDisplay("Magic Skill: " + Skills.GetSkillTitle(Globals.eSkillType.Magic, target.BaseProfession, target.magic, target.gender) + " (" + Skills.GetSkillLevel(target.magic) + ")");
                        chr.WriteToDisplay("Abjuration Spells: " + target.abjurationSpells.Count);
                        chr.WriteToDisplay("Alteration Spells: " + target.alterationSpells.Count);
                        chr.WriteToDisplay("Alteration Harmful Spells: " + target.alterationHarmfulSpells.Count);
                        chr.WriteToDisplay("Conjuration Spells: " + target.conjurationSpells.Count);
                        chr.WriteToDisplay("Divination Spells: " + target.divinationSpells.Count);
                        chr.WriteToDisplay("Evocation Spells: " + target.evocationSpells.Count);
                        chr.WriteToDisplay("Evocation AE Spells: " + target.evocationAreaEffectSpells.Count);
                        chr.WriteToDisplay("Necromancy Spells: " + target.necromancySpells.Count);
                    }

                    if (target.preppedSpell != null)
                        chr.WriteToDisplay("Prepped Spell: " + Spells.GameSpell.GetLogString(target.preppedSpell));

                    string spellsKnown = "";
                    foreach (int spellID in target.spellDictionary.Keys)
                    {
                        spellsKnown += Spells.GameSpell.GetSpell(spellID).Name + ", ";
                    }

                    if (spellsKnown != "")
                    {
                        chr.WriteToDisplay("Spellbook: " + spellsKnown.Substring(0, spellsKnown.Length - 2));
                    }

                    string talentsKnown = "";

                    foreach (string talentCommand in target.talentsDictionary.Keys)
                    {
                        talentsKnown += Talents.GameTalent.GameTalentDictionary[talentCommand].Name + " (" + target.talentsDictionary[talentCommand].ToShortTimeString() + "), ";
                    }

                    if (talentsKnown != "")
                    {
                        chr.WriteToDisplay("Talents: " + talentsKnown.Substring(0, talentsKnown.Length - 2));
                    }
                    else chr.WriteToDisplay("No talents known.");
                }
            }
            else
            {
                chr.WriteToDisplay("Could not find " + args + ".");
            }

            return true;
        }
    }
}
