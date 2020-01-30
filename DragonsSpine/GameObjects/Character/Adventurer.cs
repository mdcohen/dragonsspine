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
using System.Timers;
using System.Collections.Generic;
using System.Reflection;
using DragonsSpine.Autonomy.EntityBuilding;

namespace DragonsSpine
{
    public class Adventurer : NPC
    {
        public static int AdventurerCount;
        public static Dictionary<string, int> AdventurerCountPerMap = new Dictionary<string, int>();

        //private Dictionary<int, string> _chatReceivedFromPlayers;
        private System.Timers.Timer _changeNameTimer;

        public Adventurer(PC clone, NPC npc)
        {
            #region Fields copy.
            foreach (System.Reflection.FieldInfo npcInfo in npc.GetType().GetFields())
            {
                foreach (System.Reflection.FieldInfo advInfo in this.GetType().GetFields())
                {
                    if (npcInfo.Name == advInfo.Name)
                    {
                        switch (npcInfo.Name)
                        {
                            case "exp":
                            case "X":
                            case "Y":
                            case "Z":
                            case "cell":
                            case "wearing":
                            case "sackList":
                            case "beltList":
                            case "lockerList":
                            case "RightHand":
                            case "LeftHand":
                            case "RightRing1":
                            case "RightRing2":
                            case "RightRing3":
                            case "RightRing4":
                            case "LeftRing1":
                            case "LeftRing2":
                            case "LeftRing3":
                            case "LeftRing4":
                                break;
                            default:
                                advInfo.SetValue(this, npc.GetType().GetField(npcInfo.Name).GetValue(npc));
                                break;
                        }
                    }
                }
            } 
            #endregion

            #region Properties copy.
            foreach (System.Reflection.PropertyInfo npcPropInfo in npc.GetType().GetProperties())
            {
                foreach (System.Reflection.PropertyInfo advPropInfo in this.GetType().GetProperties())
                {
                    switch (advPropInfo.Name)
                    {
                        case "Experience":
                        case "CurrentCell": // do not set CurrentCell as this will create a ghost duplicate Adventurer
                            continue;
                        default:
                            if (npcPropInfo.Name == advPropInfo.Name && advPropInfo.CanWrite && advPropInfo.CanRead)
                            {
                                advPropInfo.SetValue(this,
                                                     npc.GetType().GetProperty(npcPropInfo.Name).GetValue(npc, new object[0]),
                                                     new object[0]);
                            }
                            break;
                    }
                }
            } 
            #endregion

            this.Name = "";

            _changeNameTimer = new System.Timers.Timer();
            _changeNameTimer.Elapsed += new ElapsedEventHandler(NameAdventurer);
            _changeNameTimer.Interval = Rules.RollD(1, 100) > 50 ? 1000 * 60 * 30 : 1000 * 60 * 60; // .5 or 1 hours            
            _changeNameTimer.Start();

            this.UniqueID = GameWorld.World.GetNextNPCUniqueID();

            Item held = DAL.DBPlayer.LoadPlayerHeld(clone.UniqueID, false);
            if (held != null)
            {
                this.SetItemVariables(held);
                this.EquipLeftHand(held);
            }
            held = DAL.DBPlayer.LoadPlayerHeld(clone.UniqueID, true);
            if (held != null)
            {
                this.SetItemVariables(held);
                this.EquipRightHand(held);
            }

            // Fill an Adventurer's sack half way with balm.
            this.sackList = new List<Item>();
            for (int a = 0; a < Character.MAX_SACK / 2; a++)
                this.SackItem(Item.CopyItemFromDictionary(Item.ID_BALM));

            this.pouchList = new List<Item>();

            this.wearing = new List<Item>(DAL.DBPlayer.LoadPlayerWearing(clone.UniqueID));
            foreach (Item item in this.wearing)
                this.SetItemVariables(item);
            this.beltList = DAL.DBPlayer.LoadPlayerBelt(clone.UniqueID);
            foreach (Item item in this.beltList)
                this.SetItemVariables(item);

            // as of 10/18/2015 rings should not need variables set for Adventurers...
            this.RightRing1 = DAL.DBPlayer.LoadPlayerRings(clone.UniqueID, (int)Globals.eWearOrientation.RightRing1);
            this.RightRing2 = DAL.DBPlayer.LoadPlayerRings(clone.UniqueID, (int)Globals.eWearOrientation.RightRing2);
            this.RightRing3 = DAL.DBPlayer.LoadPlayerRings(clone.UniqueID, (int)Globals.eWearOrientation.RightRing3);
            this.RightRing4 = DAL.DBPlayer.LoadPlayerRings(clone.UniqueID, (int)Globals.eWearOrientation.RightRing4);
            this.LeftRing1 = DAL.DBPlayer.LoadPlayerRings(clone.UniqueID, (int)Globals.eWearOrientation.LeftRing1);
            this.LeftRing2 = DAL.DBPlayer.LoadPlayerRings(clone.UniqueID, (int)Globals.eWearOrientation.LeftRing2);
            this.LeftRing3 = DAL.DBPlayer.LoadPlayerRings(clone.UniqueID, (int)Globals.eWearOrientation.LeftRing3);
            this.LeftRing4 = DAL.DBPlayer.LoadPlayerRings(clone.UniqueID, (int)Globals.eWearOrientation.LeftRing4);

            var rings = this.GetRings();

            foreach (var ring in rings)
            {
                this.SetItemVariables(ring); // aligned and attuned rings

                // set first recall ring found to spawn coordinates
                if (ring.itemID == Item.ID_RECALLRING)
                {
                    if(this.CurrentCell != null)
                    {
                        Item.SetRecallVariables(ring, this);
                    }
                    else
                    {
                        // turns all other recall rings into regular gold rings for now
                        ring.isRecall = false;
                    }
                }
            }

            DAL.DBPlayer.LoadPlayerSkills(clone);

            foreach(Globals.eSkillType skillType in Enum.GetValues(typeof(Globals.eSkillType)))
                SetSkillExperience(skillType, clone.GetSkillExperience(skillType));

            NameAdventurer(this, null);

            if(Rules.RollD(1, 100) >= 90)
                SetRandomClassFullName();

            EntityBuilder.SetHitsStaminaMana(this);
            npc.Experience = EntityBuilder.DetermineExperienceValue(this);

            IsPC = false;

            if (IsSpellUser)
                NPC.CreateGenericSpellList(this);

            if (spellDictionary.Count > 0)
                Spells.GameSpell.FillSpellLists(this);

            clone.RemoveFromWorld();

            AdventurerCount++;

            if (npc.Map != null && npc.Map.ZPlanes[npc.Z].spawnAlignment != Globals.eAlignment.None)
            {
                npc.Alignment = npc.Map.ZPlanes[npc.Z].spawnAlignment;

                if (npc.Group != null)
                {
                    foreach (NPC ch in npc.Group.GroupNPCList)
                        ch.Alignment = npc.Alignment;
                }
            }
        }

        public static bool MeetsAdventurerRequirements(NPC npc)
        {
            if (DragonsSpineMain.Instance.Settings.DebugMode) return false;

            if (npc is Merchant)
                return false;

            if (npc.Alignment == Globals.eAlignment.Lawful)
                return false;

            if (!EntityLists.HUMAN.Contains(npc.entity))
                return false;

            if (!npc.HasRandomName)
                return false;

            if (AdventurerCountPerMap.ContainsKey(npc.Map.Name))
            {
                if (AdventurerCountPerMap[npc.Map.Name] >= Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["MaxAdventurersPerMap"]))
                    return false;
                else
                {
                    AdventurerCountPerMap[npc.Map.Name]++;
                }
            }
            else
            {
                AdventurerCountPerMap.Add(npc.Map.Name, 1);
            }

            return true;
        }

        private void SetItemVariables(Item item)
        {
            if (item == null) return; // catch

            if (item.alignment != Globals.eAlignment.None) // aligned items are typically aligned for a reason
                item.alignment = this.Alignment;

            if (item.attuneType != Globals.eAttuneType.None) // attuned items are usually won or quested for
                item.AttuneItem(this);
        }

        public override string GetLogString()
        {
            try
            {
                return "(ADV) [ID: " + this.npcID + " | UniqueNPCID: " + this.UniqueID + "] " + this.Name +
                        " [" + Utils.FormatEnumString(this.Alignment.ToString()) + " " + Utils.FormatEnumString(this.BaseProfession.ToString()) + " " + this.classFullName + " (" + this.Level + ")] (" +
                        (this.CurrentCell != null ? this.CurrentCell.GetLogString(false) : "Current Cell = null") + ")";
            }
            catch (Exception e)
            {
                Utils.LogException(e);

                try
                {
                    return "(ADV) NPCID: " + this.npcID + " (null)";
                }
                catch
                {
                    return "(ADV) [Exception in GetLogString()]";
                }
            }
        }

        public void NameAdventurer(object sender, ElapsedEventArgs eventArgs)
        {
            // Adventurer may be engaged with a player. Do not rename them.
            if (eventArgs != null && this.MostHated != null && !String.IsNullOrEmpty(this.Name)) return;

            string randomWebName = "";
            string oldName = this.Name;

            do
            {
                //randomWebName = GameSystems.Text.NameGenerator.GetRandomNameFromWeb(this, false, true);
                randomWebName = GameSystems.Text.NameGenerator.GenerateRandomName(this);

                if (randomWebName != null)
                    this.Name = randomWebName;
                else GameSystems.Text.NameGenerator.GetRandomName(this);
            }
            while (DAL.DBPlayer.PlayerExists(this.Name) &&
                Character.AdventurersInGameWorldList.Exists(adv => adv.Name == randomWebName)); // do not give an Adventurer a name that already exists

            if (Rules.RollD(1, 100) > 80) GameSystems.Text.NameGenerator.SetRandomGuildTag(this);

            if (!String.IsNullOrEmpty(oldName)) // was renamed
                Utils.Log("Adventurer " + oldName + " was renamed to " + this.Name + ".", Utils.LogType.Adventurer);

            // Timer elapsed. Change timer interval.
            if (eventArgs != null)
            {
                _changeNameTimer.Stop();
                _changeNameTimer.Interval = Rules.RollD(1, 100) > 50 ? 1000 * 60 * 30 : 1000 * 60 * 60; // .5 or 1 hours
                _changeNameTimer.Start();

                // change the random classFullName
                if(Utils.FormatEnumString(this.BaseProfession.ToString()) != this.classFullName)
                    SetRandomClassFullName();
            }

            this.shortDesc = this.Name;
            this.longDesc = this.Name;
        }

        private void SetRandomClassFullName()
        {
            switch (this.BaseProfession)
            {
                case ClassType.Knight:
                    this.classFullName = EntityBuilder.KNIGHT_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.KNIGHT_SYNONYMS.Length - 1)];
                    break;
                case ClassType.Martial_Artist:
                    this.classFullName = EntityBuilder.MARTIALARTIST_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.MARTIALARTIST_SYNONYMS.Length - 1)];
                    break;
                case ClassType.Ravager:
                    this.classFullName = EntityBuilder.RAVAGER_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.RAVAGER_SYNONYMS.Length - 1)];
                    break;
                case ClassType.Sorcerer:
                    this.classFullName = EntityBuilder.SORCERER_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.SORCERER_SYNONYMS.Length - 1)];
                    break;
                case ClassType.Thaumaturge:
                    this.classFullName = EntityBuilder.THAUMATURGE_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.THAUMATURGE_SYNONYMS.Length - 1)];
                    break;
                case ClassType.Druid:
                    this.classFullName = EntityBuilder.DRUID_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.DRUID_SYNONYMS.Length - 1)];
                    break;
                case ClassType.Ranger:
                    this.classFullName = EntityBuilder.RANGER_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.RANGER_SYNONYMS.Length - 1)];
                    break;
                case ClassType.Thief:
                    this.classFullName = EntityBuilder.THIEF_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.THIEF_SYNONYMS.Length - 1)];
                    break;
                case ClassType.Wizard:
                    this.classFullName = EntityBuilder.WIZARD_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.WIZARD_SYNONYMS.Length - 1)];
                    break;
                case ClassType.Fighter:
                default:
                    this.classFullName = EntityBuilder.FIGHTER_SYNONYMS[Rules.Dice.Next(0, EntityBuilder.FIGHTER_SYNONYMS.Length - 1)];
                    break;
            }

            this.classFullName = Utils.FormatEnumString(this.classFullName); // replaces underscores

            // for multiple words
            string[] sWords = this.classFullName.Split(" ".ToCharArray());

            foreach (string word in sWords)
                this.classFullName = char.ToUpper(word[0]) + word.Substring(1) + " ";

            this.classFullName = this.classFullName.Trim();

            // capitalize first letter
            //this.classFullName = char.ToUpper(this.classFullName[0]) + this.classFullName.Substring(1);
        }
    }
}
