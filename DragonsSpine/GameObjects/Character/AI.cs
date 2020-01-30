using DragonsSpine.GameWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using CastMode = DragonsSpine.NPC.CastMode;
using ClassType = DragonsSpine.Character.ClassType;
using EntityLists = DragonsSpine.Autonomy.EntityBuilding.EntityLists;
using GameCommand = DragonsSpine.Commands.GameCommand;
using GameSpell = DragonsSpine.Spells.GameSpell;
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine
{
    /// <summary>
    /// 
    /// </summary>
    public static class AI
    {
        #region Enumerations (2)
        public enum ActionType { None, Take, Use, Cast, Special, Move, Combat }

        /// <summary>
        /// Key = NPC Unique ID, List = Item IDs to ignore as they've been looked at to manipulate already.
        /// A Pivot Item is an Item object the NPC has assessed to pick up or use.
        /// </summary>
        public static Dictionary<int, List<int>> IgnoredPivotItems = new Dictionary<int, List<int>>();

        /// <summary>
        /// Key = NPC Unique ID, Dictionary.Key = target.UniqueID, Value = new Dictionary.Key = SpellID, Value = Resist Count.
        /// Value = Dictionary<target.UniqueID, Dictionary<GameSpell.ID, Resist Count>
        /// </summary>
        //public static Dictionary<int, Dictionary<int, Dictionary<int, int>>> ResistedSpells = new Dictionary<int,Dictionary<int, Dictionary<int, int>>>();

        /// <summary>
        /// The order of the Priority enum is crucial. Higher level priorities will outweight lower level when making AI decisions.
        /// </summary>
        public enum Priority
        {
            None,
            Wander, // wander around
            GetObject, // pick up a nearby valuable object
            Advance, // move closer to an enemy
            Attack, // attack an enemy
            // moved Attack from this position 9/28/2019
            Investigate, // mostHated is null, but wasn't a round ago so investigate where mostHated was
            InvestigateMagic, // investigate where magic was warmed
            PrepareSpell, // prepare a spell
            GetWeapon, // get a weapon, either here, somewhere in sight or from belt (pick up = ActionType.Take, belt = ActionType.Use)
            ManipulateObject, // belt or wear items, empty hands if necessary
            RangeMove, // move away for a ranged attack, preferably 2 cells away, atleast 1 cell away
            GoHome, // lair critters return home, quest npcs return to spawn coords
            RaiseDead, // priests raise the dead
            Buff, // cast buff spells on self or allies
            // moved CommandPets_Attack from this position 9/28/2019
            Rest, // rest (eg: cancel prepared spell)
            Meditate, // meditate (limiting this to humans
            SpellSling, // warm or cast a prepared spell
            SpellSlingMemorizedChant,
            CommandPets_Attack, // ActionType.Combat, one or more pets is not attacking mostHated so send them in - otherwise instruct them to follow
            SummonFlagged, // entities in EntityLists.SUMMONER will summon players/NPCs on their flagged list
            FleeEffect, // flee from an effect in the npc's cell
            Flee, // flee
            Enforce, // enforce the local law
            LairDefense, // defend our lair
            Heal, // heal myself or ally -- sorcerers use lifeleech -- drink balm if ActionType = USE and Priority = HEAL
            Recall // added 10/18/2015 -- Adventurers will know how to recall
        }
        #endregion

        private static readonly object lockObject = new object();

        /// <summary>
        /// Called every game round to determine what other Character objects are visible to an NPC object.
        /// </summary>
        /// <param name="npc">The NPC creating its contact list.</param>
        /// <returns>True if the contact list was created.</returns>
        public static bool CreateContactList(NPC npc)
        {
            if (npc == null) { return false; }
            if (npc.IsDead) { return false; } // return if the creature is dead
            if (npc.CurrentCell == null) { return false; } // return if the npc does not have a cell.

            npc.TotalFearLove = 0;
            npc.TotalHate = 0;
            npc.HateCenterX = 0;
            npc.HateCenterY = 0;
            npc.FearCenterX = 0;
            npc.FearCenterY = 0;
            npc.enemyList.Clear();
            npc.friendList.Clear();
            npc.targetList.Clear();
            npc.localCells.Clear();
            npc.seenList.Clear();

            Cell[] cellArray = null;
            int j = 0;

            try
            {
                cellArray = Cell.GetApplicableCellArray(npc.CurrentCell, npc.GetVisibilityDistance());
                var fullCellArray = Cell.GetApplicableCellArray(npc.CurrentCell, Cell.DEFAULT_VISIBLE_DISTANCE);

                for (j = 0; j < cellArray.Length; j++)
                {
                    if (cellArray[j] == null && npc.CurrentCell.visCells[j] && fullCellArray.Length >= j + 1 && fullCellArray[j] != null)
                    {
                        Globals.eLightSource lightsource; // no use for this yet

                        if (fullCellArray[j].HasLightSource(out lightsource) && !AreaEffect.CellContainsLightAbsorbingEffect(fullCellArray[j]))
                        {
                            cellArray[j] = fullCellArray[j];
                        }
                    }

                    if (cellArray[j] == null || !npc.CurrentCell.visCells[j])// || ((cellArray[j].AreaEffects.ContainsKey(Effect.EffectType.Darkness) ||  cellArray[j].IsAlwaysDark) && !npc.HasNightVision))
                    {
                        continue;
                    }
                    else
                    {
                        #region create list of targets, friends and enemies
                        lock (lockObject)
                        {
                            foreach (Character chr in cellArray[j].Characters.Values) //new List<Character>(cellArray[j].Characters))
                            {
                                //for (int a = cellArray[j].Characters.Count - 1; a >= 0; a--)
                                //{
                                if (chr == null) continue;

                                if (chr != npc && !chr.IsDead && !npc.IsBlind)
                                {
                                    if (Rules.DetectHidden(chr, npc) && Rules.DetectInvisible(chr, npc)) // add detected hidden only
                                    {
                                        // Never add immortal characters to any lists.
                                        if (!chr.IsImmortal)
                                        {
                                            npc.targetList.Add(chr); // add to visible target list
                                            npc.seenList.Add(chr);
                                            /*
                                             * Caution should be used here when determining which array the visible
                                             * character object is added to. We do not want the AI to be too intelligent... or do we? -Eb
                                             * 
                                             * Since alignment is the key factor in determining if a creature is an enemy, we use
                                             * Rules.DetectAlignment to determine if we're going to add this chr to the enemy list.
                                             * Note that if a player is flagged Rules.DetectAlignment will return true
                                             * 
                                             */

                                            // Override for pets to remain friendly to their pet owner no matter what the circumstance. They are a pet.
                                            if(chr is NPC && (chr as NPC).enemyList.Contains(npc) && !npc.enemyList.Contains(chr))
                                            {
                                                npc.enemyList.Add(chr);
                                            }

                                            if (npc.PetOwner != null && npc.PetOwner == chr && chr.FlaggedUniqueIDs.Contains(npc.PetOwner.UniqueID) && !npc.enemyList.Contains(chr))
                                            {
                                                npc.enemyList.Add(chr);

                                                //if (!npc.friendList.Contains(chr)) npc.friendList.Add(chr);
                                            }

                                            if (Rules.DetectAlignment(chr, npc) || npc.FlaggedUniqueIDs.Contains(chr.UniqueID))
                                            {
                                                // Detected alignment (which is a pretty screwy bit of code at the moment) 2/4/2017 Eb
                                                if(!(npc is Merchant) && !npc.enemyList.Contains(chr))
                                                    npc.enemyList.Add(chr);
                                            }
                                            else
                                            {
                                                // Pet Owner is already an enemy, make the pet an enemy too.
                                                if(chr.PetOwner != null && npc.enemyList.Contains(chr.PetOwner))
                                                {
                                                    if(!npc.enemyList.Contains(chr))
                                                    {
                                                        npc.enemyList.Add(chr);

                                                        if (npc.friendList.Contains(chr)) npc.friendList.Remove(chr);
                                                    }
                                                }
                                                else  npc.friendList.Add(chr);
                                            }

                                            if(npc.friendList.Contains(chr) && npc.enemyList.Contains(chr))
                                            {
                                                // Remove from friends list and see how it goes. 2/4/2017 Eb
                                                npc.friendList.Remove(chr);
                                                //Utils.Log(npc.GetLogString() + " has " + chr.GetLogString() + " in both friend and enemy lists. This is a problem.", Utils.LogType.DebugAI);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        if (npc.GetCellCost(cellArray[j]) <= 2)
                            npc.localCells.Add(cellArray[j]);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                Utils.Log("AI.CreateContactList Exception for " + npc.GetLogString() + " AND " + cellArray[j].GetLogString(true), Utils.LogType.ExceptionDetail);
            }

            try
            {
                List<int> idToRemoveList;

                foreach (Character friend in new List<Character>(npc.friendList))
                {
                    if (friend != null)
                    {
                        idToRemoveList = new List<int>();

                        foreach (int playerID in new List<int>(friend.FlaggedUniqueIDs))
                        {
                            PC pc = PC.GetOnline(playerID);

                            // Player is no longer online, add to temp list and remove from flagged list
                            if (pc == null)
                            {
                                idToRemoveList.Add(playerID);
                                continue;
                            }

                            // Remove player ID from friend's flagged list if not same  map
                            if (pc.MapID != friend.MapID)
                            {
                                friend.FlaggedUniqueIDs.RemoveAll(id => id == playerID);
                                continue;
                            }

                            // This is where flagging is shared among friendly AI. Commented out 7/6/2019. Eb
                            //if (!npc.PlayersFlagged.Contains(playerID))
                            //    npc.PlayersFlagged.Add(playerID);
                        }

                        if (idToRemoveList.Count > 0)
                        {
                            foreach (int playerID in idToRemoveList)
                            {
                                friend.FlaggedUniqueIDs.RemoveAll(id => id == playerID);
                                npc.FlaggedUniqueIDs.RemoveAll(id => id == playerID);
                            }
                        }
                    }
                }

                if (npc.MostHated == null || !npc.targetList.Contains(npc.MostHated))
                    npc.TargetID = 0;

                // If NPC has a quest, and is currently being commanded by something (preferrably a player), do escort quest logic.
                if (npc.QuestList.Count > 0 && npc.PetOwner != null)
                    EscortQuestLogic(npc);

                return true;
            }
            catch (Exception e)
            {
                Utils.Log("Error in AI.CreateContactList lower: " + npc.GetLogString(), Utils.LogType.ExceptionDetail);
                Utils.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// Assign Fear/Love levels to all Character objects in a contacts list for an NPC. Used by AI to determine further actions for each contact.
        /// </summary>
        /// <param name="npc">The NPC assigning Fear/Love values.</param>
        public static void AssignHateFearLove(NPC npc)
        {
            int highfear = 1;
            int highhate = 1;
            int lowfear = -1;
            int minFL = 0;
            int maxFL = 0;
            int fearLoveX = 0;
            int fearLoveY = 0;
            int minH = 0;
            int maxH = 0;
            int hateX = 0;
            int hateY = 0;
            int fear = 0;
            int hate = 0;

            npc.MostFeared = null;
            npc.MostLoved = null;
            npc.MostHated = null;

            if (npc.previousMostHated != null || npc.previousMostHatedsCell != null)
            {
                bool clearPreviousMostHated = false;

                // Determine the distance the NPC is willing to travel to go after the one they hated.
                // This is IMPORTANT. Currently as of 10/29/2015, NPCs will travel throughout a Z plane until they reach their previous mostHated.

                if (npc.previousMostHated != null)
                {
                    // Player logged out.
                    if (npc.previousMostHated is PC && !Character.PCInGameWorld.Contains(npc.previousMostHated as PC))
                        clearPreviousMostHated = true;

                    // Dead.
                    if (npc.previousMostHated.IsDead) clearPreviousMostHated = true;

                    // No longer on the same map. TODO: allow NPCs to move maps and chase their enemies! 9/2/2019
                    if (npc.previousMostHated.MapID != npc.MapID) clearPreviousMostHated = true;

                    // Previous most hated is hidden, on the same ZPlane, and more than 11 distance away. Stop chasing.
                    if (npc.previousMostHated.IsHidden && npc.previousMostHated.Z == npc.Z && Cell.GetCellDistance(npc.X, npc.Y, npc.previousMostHated.X, npc.previousMostHated.Y) >= 12)
                        clearPreviousMostHated = true;
                   
                }

                if (!clearPreviousMostHated && npc.previousMostHatedsCell != null)
                {
                    // This NPC is at the spot its previous most hated was last seen. Do they continue to look for them?
                    if (npc.CurrentCell == npc.previousMostHatedsCell) clearPreviousMostHated = true;
                }

                // check map for previous most hated

                if (clearPreviousMostHated)
                { npc.previousMostHated = null; npc.previousMostHatedsCell = null; }
            }

            Character target;

            try
            {
                // friends: (1000 * (friend->perceivedStrength() / own->perceivedStrength())) / distance
                // enemies: (-1000 * (enemy->perceivedStrength() / own->perceivedStrength())) / distance
                for (int a = 0; a < npc.targetList.Count; a++)
                {
                    target = npc.targetList[a];
                    fear = 0;
                    hate = 0;

                    if (target != null)
                    {
                        int distance = Cell.GetCellDistance(npc.X, npc.Y, target.X, target.Y) + 1;

                        fear = GetFearLevel(npc, target, distance);

                        if (npc.enemyList.Contains(target)) // target is in enemyList
                        {
                            hate = GetHateLevel(npc, target, distance);

                            npc.TotalHate += hate;

                            if (hate > highhate)
                            {
                                highhate = hate;
                                npc.MostHated = target;
                            }
                        }

                        if (fear > highfear)
                        {
                            highfear = fear;
                            npc.MostLoved = target;
                        }

                        if (fear < lowfear)
                        {
                            lowfear = fear;
                            npc.MostFeared = target;
                        }

                        npc.TotalFearLove += fear;

                        #region Fear / Love

                        try
                        {
                            if (npc.MostLoved != null || npc.MostFeared != null)
                            {
                                minFL = Math.Min(Math.Abs(npc.TotalFearLove), Math.Abs(fear));
                                maxFL = Math.Max(Math.Abs(npc.TotalFearLove), Math.Abs(fear));
                                fearLoveX += (int)((target.X - fearLoveX) * ((float)minFL / maxFL));
                                fearLoveY += (int)((target.Y - fearLoveY) * ((float)minFL / maxFL));
                                npc.FearCenterX = fearLoveX;
                                npc.FearCenterY = fearLoveY;
                            }
                        }
                        catch
                        {
                            Utils.Log("AI (AssignFearLove - fear/love): " + npc.GetLogString(),
                                      Utils.LogType.SystemFailure);
                            if (World.GetFacetByID(npc.FacetID).Spawns.ContainsKey(npc.SpawnZoneID))
                                World.GetFacetByID(npc.FacetID).Spawns[npc.SpawnZoneID].NumberInZone--;
                            npc.RemoveFromWorld();
                            return;
                        }

                        #endregion

                        #region Hate

                        try
                        {
                            if (npc.MostHated != null)
                            {
                                minH = Math.Min(npc.TotalHate, hate);
                                maxH = Math.Max(npc.TotalHate, hate);
                                hateX += (int)((target.X - hateX) * ((float)minH / maxH));
                                hateY += (int)((target.Y - hateY) * ((float)minH / maxH));
                                npc.HateCenterX = hateX;
                                npc.HateCenterY = hateY;
                            }
                        }
                        catch
                        {
                            Utils.Log("AI (AssignFearLove - hate): " + npc.GetLogString(), Utils.LogType.SystemFailure);
                            if (World.GetFacetByID(npc.FacetID).Spawns.ContainsKey(npc.SpawnZoneID))
                                World.GetFacetByID(npc.FacetID).Spawns[npc.SpawnZoneID].NumberInZone--;
                            npc.RemoveFromWorld();
                            return;
                        }

                        #endregion
                    }
                }

                if (npc.MostHated != null)
                {
                    npc.MostHated.NumAttackers++;

                    if (npc.Group != null && npc.Group.GroupNPCList.Count > 0)
                        npc.MostHated.NumAttackers += npc.Group.GroupNPCList.Count - 1; // do not include the leader

                    npc.previousMostHated = npc.MostHated;
                    npc.previousMostHatedsCell = npc.MostHated.CurrentCell;                    
                }

                if (npc.FearCenterX == 0 && npc.FearCenterY == 0)
                {
                    npc.FearCenterX = npc.X;
                    npc.FearCenterY = npc.Y;
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        /// <summary>
        /// Determine fear level of a Character object in an NPCs contact list.
        /// </summary>
        /// <param name="npc">Evaluating NPC.</param>
        /// <param name="target">Evaluative Character object.</param>
        /// <param name="distance">The distance between npc and target.</param>
        /// <returns>The fear level.</returns>
        private static int GetFearLevel(NPC npc, Character target, int distance)
        {
            if (target == null) return 0;

            if (npc.animal || npc.IsUndead)
            {
                if (npc.enemyList.Contains(target)) return (int)(-1000 * (float)(GetPerceivedStrength(target) / GetPerceivedStrength(npc))) / distance;
                else
                {
                    if (target.UniqueID == npc.FollowID) return 100000;
                    else return (int)(1000 * (float)(GetPerceivedStrength(target) / GetPerceivedStrength(npc))) / distance;
                }
            }
            else
            {
                if (npc.enemyList.Contains(target))
                {
                    return (int)((-500 * ((float)GetPerceivedStrength(target) / GetPerceivedStrength(npc))) +
                                (-500 * ((float)GetPerceivedDanger(target) / GetPerceivedDanger(npc)))) / distance;
                }
                else
                {
                    if (npc.Name == target.Name)
                    {
                        if (target.UniqueID == npc.FollowID) return 100000;
                        else return (int)((500 * ((float)GetPerceivedStrength(target) / GetPerceivedStrength(npc))) +
                            (500 * ((float)GetPerceivedDanger(target) / GetPerceivedDanger(npc)))) / distance;
                    }
                    else
                        return 0;
                }
            }
        }

        private static int GetHateLevel(NPC npc, Character target, int distance)
        {
            if (target == null || npc == null)
                return 0;

            if (target.CurrentCell != null && target.CurrentCell.IsWithinTownLimits &&
                (!npc.IsSpellWarmingProfession && npc.RightHand == null || (npc.RightHand != null && npc.RightHand.skillType != Globals.eSkillType.Bow)))
                return 0;

            int dividend = 1800;
            int divisor = distance;

            // undead vs. wisdom casters
            if (npc.IsUndead)
            {
                if (target.IsWisdomCaster) dividend += 15000;
                else dividend += 10000;
            }

            // animals and non druids
            if (npc.animal && target.BaseProfession != ClassType.Druid) dividend += 500;

            // enforcers, sheriffs, knights vs players with karma
            if(target is PC pc && pc.currentKarma > 0 && (npc.aiType == NPC.AIType.Enforcer || npc.aiType == NPC.AIType.Sheriff || npc.BaseProfession == ClassType.Knight))
            {
                dividend += pc.currentKarma * 250;
            }

            foreach (Item weapon in new List<Item>() { target.RightHand, target.LeftHand })
            {
                if (weapon != null)
                {
                    if (EntityLists.SILVER_REQUIRED.Contains(npc.entity) && weapon.silver) dividend += 1000;
                    if (EntityLists.BLUEGLOW_REQUIRED.Contains(npc.entity) && weapon.blueglow) dividend += 1000;

                    if (EntityLists.WEAPON_REQUIREMENT.ContainsKey(npc.entity))
                    {
                        foreach (string specialAttack in EntityLists.WEAPON_REQUIREMENT[npc.entity])
                        {
                            if (weapon.special.ToLower().Contains(specialAttack))
                                dividend += 5000;
                            break;
                        }
                    }
                }
            }

            return dividend / divisor;
        }

        private static float GetPerceivedStrength(Character target)
        {
            int strength = Rules.GetFullAbilityStat(target, Globals.eAbilityStat.Strength);
            int modifier = Math.Max(target.GetWeaponSkillLevel(target.RightHand), Skills.GetSkillLevel(target.magic));
            if (EntityLists.IsGiantKin(target)) modifier += target.Level;
            float health = (float)target.Hits / target.HitsFull;
            return (strength * health) + modifier;
        }

        private static int GetPerceivedDanger(Character target)
        {
            int danger = 0;

            if (target.GetInventoryItem(Globals.eWearLocation.Torso) is Item torso)
                danger = (int)torso.armorClass * 100;

            if (target.RightHand != null)
            {
                if (target.RightHand.skillType == Globals.eSkillType.Bow)
                {
                    danger += 400;

                    if (target.RightHand.IsNocked)
                        danger += 100;
                }

                if (target.RightHand.returning) danger += 50;
                if (target.RightHand.special.ToLower().Contains("hummingbird")) danger += 50;
                if (target.RightHand.venom > 0) danger += target.RightHand.venom;

                danger += 50;
            }

            if (target.LeftHand != null)
            {
                if (target.LeftHand.baseType == Globals.eItemBaseType.Shield) danger += 50;
                if (target.LeftHand.returning) danger += 50;
                if (target.LeftHand.special.ToLower().Contains("hummingbird")) danger += 50;
                if (target.LeftHand.venom > 0) danger += target.LeftHand.venom;

                danger += 50;
            }

            if (danger == 0) { danger = 50; } // assign some danger to AC 0

            return danger;
        }

        public static void Rate(NPC npc)
        {
            // Punching bags?
            if (npc.Alignment == Globals.eAlignment.Amoral) return;

            Priority cur_pri = Priority.None;
            ActionType action = ActionType.None;
            Priority new_pri = Priority.None;

            try
            {
                // Animals, plants and pets do not rate taking or using items. Pets must be commanded to take or use items.
                if (!EntityLists.ANIMAL.Contains(npc.entity) && npc.species != Globals.eSpecies.Plant && npc.PetOwner == null)
                {
                    try
                    {
                        new_pri = Rate_TAKE(npc);
                    }
                    catch(Exception e)
                    {
                        Utils.LogException(e);
                    }

                    if (new_pri > cur_pri)
                    {
                        action = ActionType.Take;
                        cur_pri = new_pri;
                    }

                    try
                    { 
                        new_pri = Rate_USE(npc, cur_pri);
                    }
                    catch (Exception e)
                    {
                        Utils.LogException(e);
                    }

                    if (new_pri > cur_pri)
                    {
                        action = ActionType.Use;
                        cur_pri = new_pri;
                    }
                }

                new_pri = Rate_CAST(npc);

                if (new_pri > cur_pri)
                {
                    action = ActionType.Cast;
                    cur_pri = new_pri;
                }

                // Pets do not perform special actions.
                if (npc.PetOwner == null)
                {
                    new_pri = Rate_SPECIAL(npc);

                    if (new_pri > cur_pri)
                    {
                        action = ActionType.Special;
                        cur_pri = new_pri;
                    }
                }

                // ManipulateObject = wield and belt weapons. GetWeapon = pick up fumbled or better weapon.
                if (cur_pri != Priority.ManipulateObject && cur_pri != Priority.GetWeapon)
                {
                    new_pri = Rate_MOVE(npc);

                    // Override for guard zone spawns. They stay put until enemies are nearby.
                    if (new_pri == Priority.Wander && npc.Map.ZPlanes[npc.Z].zAutonomy != null && npc.Hits >= npc.HitsFull && npc.enemyList.Count <= 0)
                    {
                        if (npc.Map.ZPlanes[npc.Z].zAutonomy.guardZone)
                        {
                            new_pri = Priority.None;
                        }
                    }

                    if (new_pri > cur_pri)
                    {
                        action = ActionType.Move;
                        cur_pri = new_pri;
                    }
                    new_pri = Rate_COMBAT(npc);

                    if (new_pri > cur_pri)
                    {
                        action = ActionType.Combat;
                        cur_pri = new_pri;
                    }
                }

                ExecuteAction(npc, action, cur_pri);
                return;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                cur_pri = Priority.None;
                action = ActionType.None;
                return;
            }
        }

        #region Rate Actions
        public static Priority Rate_TAKE(NPC npc)
        {
            Priority pri = Priority.None;

            if (npc.CurrentCell == null) return pri;

            // Merchants should not pick up anything unless instructed to elsewhere
            if (npc is Merchant) return pri;

            // Pets must be instructed to take items.
            if (npc.PetOwner != null) return pri;

            // Plants will not take items.
            if (npc.species == Globals.eSpecies.Plant) return pri;

            // Animals will not take items.
            if (npc.animal || EntityLists.ANIMAL.Contains(npc.entity)) return pri;

            // to avoid NPCs not wielding different weapon types
            if (npc.Group != null) return pri;

            if (npc.lairCritter || EntityLists.UNIQUE.Contains(npc.entity)) return pri;

            if (npc.CurrentCell != null && npc.CurrentCell.IsLair) return pri;

            // Override. Don't take anything if you have an enemy and are already wielding a weapon.
            // There is a bug somewhere in this code where NPCs continuously swap weapons. 1/11/2017 -Eb
            if (npc.MostHated != null && npc.BaseProfession != ClassType.Martial_Artist &&
                npc.RightHand != null && npc.RightHand.itemType == Globals.eItemType.Weapon)
                return pri;

            // The itemID already exists in ignored items. Don't take it again.
            if (npc.PivotItem != null && AI.IgnoredPivotItems.ContainsKey(npc.UniqueID))
            {
                if (AI.IgnoredPivotItems[npc.UniqueID].Contains(npc.PivotItem.itemID)) return pri;
                //else if (npc.PivotItem.itemType == Globals.eItemType.Weapon) return Priority.GetWeapon;
            }

            // Right hand is empty, there is an item in pItem (likely placed there during fumble) and it is a weapon.
            if (npc.RightHand == null && npc.PivotItem != null && npc.PivotItem.itemType == Globals.eItemType.Weapon)
            {
                if (Item.IsItemOnGround(npc.PivotItem.itemID, npc.CurrentCell))
                    pri = Priority.GetWeapon;
            }

            #region Not already picking up a weapon, not a lair critter, not a martial artist and not a unique entity. Evaluate weapons.
            if (pri < Priority.GetWeapon && !npc.lairCritter && npc.BaseProfession != Character.ClassType.Martial_Artist)
            {
                Priority currentPriority = pri;

                // Check belt for weapons we can use.
                // Belt is checked in rate_USE and priority should beat Priority.GetWeapon.

                // Check current cell for weapons we can use.
                if (npc.CurrentCell.Items.Count > 0)
                {
                    var weaponsList = new List<Item>(); // weapons in our current cell

                    foreach (Item item in npc.CurrentCell.Items)
                    {
                        if (item.itemType == Globals.eItemType.Weapon) // weapon
                            if ((item.attunedID == 0 || item.attunedID == npc.UniqueID)
                                && (item.alignment == Globals.eAlignment.None || item.alignment == npc.Alignment)) // not bound to a player or wrong alignment
                            {
                                // better skill level than unarmed, not a two handed weapon, or a two handed weapon and left hand is empty
                                if ((Skills.GetSkillLevel(npc.unarmed) < Skills.GetSkillLevel(npc, item.skillType)) && !item.TwoHandedPreferred() || (item.TwoHandedPreferred() && npc.LeftHand == null))
                                    weaponsList.Add(item);
                            }
                    }

                    if (npc.RightHand != null && npc.RightHand.itemType == Globals.eItemType.Weapon)
                        weaponsList.Add(npc.RightHand);

                    // If there are no weapons on the ground, check belt in rate_USE.
                    if (weaponsList.Count > 0)
                    {
                        // npc is a pure melee or they are not undead and pass a perception check
                        //if (npc is Adventurer || npc.IsPureMelee || Rules.CheckPerception(npc) && !npc.IsUndead)
                        //{
                        var skillTypes = new List<Globals.eSkillType>();

                        foreach (Item item in weaponsList)
                            skillTypes.Add(item.skillType);

                        // Determine which weapon on the ground is the best for this AI.
                        Globals.eSkillType bestSkillType = Skills.GetBestSkill(npc, skillTypes);

                        var preferredWeapons = new List<Item>();
                        foreach (Item w in weaponsList)
                        {
                            if ((npc.RightHand == null || (npc.RightHand != null && npc.RightHand.itemID != w.itemID && npc.RightHand.skillType != bestSkillType))
                                && w.skillType == bestSkillType) preferredWeapons.Add(w);
                        }

                        if (preferredWeapons.Count > 0)
                        {
                            pri = Priority.GetWeapon;

                            if ((npc.IsPureMelee && npc.Level >= 9) || npc is Adventurer) // this is where things get hairy - level 9+ pure melee classes know which weapon would be best
                            {
                                preferredWeapons.Sort((w1, w2) => w1.combatAdds.CompareTo(w2.combatAdds));
                                npc.PivotItem = preferredWeapons[0];
                            }
                            else
                                npc.PivotItem = preferredWeapons[Rules.Dice.Next(0, preferredWeapons.Count - 1)]; // pick up random preferred weapon

                            // Cancel take and priority GetWeapon if weapon is already in right hand.
                            if (npc.PivotItem != null && npc.PivotItem == npc.RightHand)
                            {
                                pri = currentPriority;
                                npc.PivotItem = null;
                            }

#if DEBUG
                            if (npc.PivotItem != null)
                            {
                                npc.SendToAllDEVInSight(npc.Name + " will take " + npc.PivotItem.notes + " as a preferred weapon.");
                            }
#endif
                        }
                        else
                        {
#if DEBUG
                            npc.SendToAllDEVInSight(npc.Name + " did not add any items to their preffered weapons list selection.");
#endif
                        }

                    }
                    else
                    {
#if DEBUG
                        npc.SendToAllDEVInSight(npc.Name + " did not add any items to their weapons list selection.");
#endif
                    }
                }
            }
            #endregion            

            else if (npc.MostHated == null && npc.IsGreedy && npc.CurrentCell.Items.Count > 0)
            {
                try
                {
                    foreach (Item item in new List<Item>(npc.CurrentCell.Items))
                    {
                        // Hardcode for Alia, Vulcan's daughter, to steal the obsidian egg.
                        if (npc.MapID == Map.ID_AXEGLACIER && npc.entity == EntityLists.Entity.Alia && item.name.ToLower() == "egg")
                        {
                            npc.PivotItem = item;

                            if (npc.PivotItem != null)
                                pri = Priority.GetObject;
                            break;
                        }

                        if (item.itemID == Item.ID_BALM)
                        {
                            if (npc.SackCountMinusGold < Character.MAX_SACK)
                            {
                                npc.PivotItem = item;

                                if (npc.PivotItem != null)
                                    pri = Priority.GetObject;

                                break;
                                //TODO add other priority items here 10/18/2015 Eb
                            }
                        }

                        if (item.itemType == Globals.eItemType.Coin && Rules.GetEncumbrance(npc) <= Globals.eEncumbranceLevel.Lightly)
                        {
                            npc.PivotItem = item;

                            if (npc.PivotItem != null)
                                pri = Priority.GetObject;

                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.LogException(e);
                }
            }

            return pri;
        }

        public static Priority Rate_USE(NPC npc, Priority cur_pri)
        {
            Priority pri = Priority.None;

            // Merchants should not use anything unless instructed to elsewhere? 12/10/2015 Eb
            if (npc is Merchant) return pri;

            #region Use recall item. ActionType.Use, Priority.Recall.
            // 20 percent health, is an Adventurer, or is a human/humanoid and passes perception check -- TODO: work on perception check 10/18/2015 Eb
            if (npc.Hits <= (int)(npc.HitsFull * .2) &&
                (npc is Adventurer || (EntityLists.IsHumanOrHumanoid(npc) && Rules.CheckPerception(npc))))
            {
                var rings = npc.GetRings();

                rings.RemoveAll(r1 => !r1.isRecall);

                // As of 10/18/2015 early AM only one recall ring is set for Adventurers...
                if (rings.Count > 0)
                {
                    npc.PivotItem = rings[0];
                    return Priority.Recall; // currently highest priority there is
                }

                // NPCs do not currently wander between ZPlanes.

                // Sort by distance from current cell.
                // rings.Sort((r1, r2) => Cell.GetCellDistance(npc.X, npc.Y, r1.recallX, r1.recallY).CompareTo(Cell.GetCellDistance(npc.X, npc.Y, r2.recallX, r2.recallY)));

                // Pull the first ring. Gosh, now the NPC has to make a hand available.. right?
            }
            #endregion

            #region Quaff a balm from sack. ActionType.Use, Priority.Heal
            // Adventurers will always quaff a balm. Other human/humanoids entities will check perception.
            if (pri < Priority.Heal && npc.Hits <= (int)(npc.HitsFull * .5) &&
                 !EntityLists.UNDEAD.Contains(npc.entity) &&(npc is Adventurer || (EntityLists.IsHumanOrHumanoid(npc.entity) && Rules.CheckPerception(npc))))
            {
                bool hasCureSpell = npc.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Cure);

                if (!hasCureSpell || (hasCureSpell && !npc.HasManaAvailable((int)GameSpell.GameSpellID.Cure)))
                {
                    foreach (Item sackItem in npc.sackList)
                    {
                        if (sackItem.itemID == Item.ID_BALM) // npc will quaff a balm
                        {
                            npc.PivotItem = sackItem;
                            pri = Priority.Heal;
                            break;
                        }
                    }
                }
            }
            #endregion

            if (!EntityLists.UNIQUE.Contains(npc.entity) && !npc.lairCritter)
            {
                if (pri < Priority.ManipulateObject && npc.LeftHand != null)
                {
                    // Not a shield in left hand.
                    if (npc.LeftHand.baseType != Globals.eItemBaseType.Shield)
                    {
                        // Not a weapon in left hand.
                        if (npc.LeftHand.itemType != Globals.eItemType.Weapon)
                        {
                            if (npc.LeftHand.size <= Globals.eItemSize.Belt_Or_Sack)
                            {
                                npc.PivotItem = npc.LeftHand;
                                pri = Priority.ManipulateObject;
                            }
                        }
                        else // Is a weapon in left hand. Check two handed preferred.
                        {
                            if (npc.RightHand != null && npc.RightHand.TwoHandedPreferred())
                            {
                                npc.PivotItem = npc.LeftHand;
                                pri = Priority.ManipulateObject;
                            }
                        }
                    }
                    else
                    {
                        // Shield should be belted if wielding a two handed weapon.
                        if (npc.RightHand != null && npc.RightHand.TwoHandedPreferred())
                        {
                            npc.PivotItem = npc.LeftHand; // shield should be belted
                            pri = Priority.ManipulateObject;
                        }
                    }
                }
                else if (pri < Priority.ManipulateObject && npc.LeftHand == null)
                {
                    if (npc.RightHand != null && npc.RightHand.itemType == Globals.eItemType.Weapon && !npc.RightHand.TwoHandedPreferred())
                    {
                        // TODO: Better check for shield on belt.
                        foreach (Item w in npc.beltList)
                        {
                            if (w.baseType == Globals.eItemBaseType.Shield)
                            {
                                npc.PivotItem = w;
                                pri = Priority.ManipulateObject;
                                break;
                            }
                        }
                    }
                }

                #region Check belt for a weapon. ActionType.Use, Priority.ManipulateObject
                if (npc.beltList != null && npc.beltList.Count > 0 && pri <= Priority.GetWeapon && npc.RightHand == null && npc.BaseProfession != Character.ClassType.Martial_Artist)
                {
                    List<Item> weaponsList = new List<Item>(); // weapons from our belt to review for wielding

                    foreach (Item item in npc.beltList)
                    {
                        // Weapon item type, alignment check, attuned ID check.
                        if (item.itemType == Globals.eItemType.Weapon &&
                            (((item.alignment == Globals.eAlignment.None || item.alignment == npc.Alignment) && (item.attunedID == 0 || item.attunedID == npc.UniqueID))))
                            weaponsList.Add(item);
                    }

                    // Add a weapon the NPC may be looking at on the ground.
                    if (cur_pri == Priority.GetWeapon && npc.PivotItem != null && npc.PivotItem.itemType == Globals.eItemType.Weapon)
                        weaponsList.Add(npc.PivotItem);

                    // Add held weapon.
                    if (npc.RightHand != null && npc.RightHand.itemType == Globals.eItemType.Weapon)
                        weaponsList.Add(npc.RightHand);

                    if (weaponsList.Count > 0)
                    {
                        var skillTypes = new List<Globals.eSkillType>();

                        foreach (Item w in weaponsList)
                            skillTypes.Add(w.skillType);

                        Globals.eSkillType bestSkillType = Skills.GetBestSkill(npc, skillTypes);

                        //TODO: check fighter specialization here 10/19/2015 Eb

                        // Check unarmed skill versus best skillType on belt.
                        if (Skills.GetSkillLevel(npc, bestSkillType) > Skills.GetSkillLevel(npc.unarmed))
                        {
                            weaponsList.RemoveAll(w => w.skillType != bestSkillType); // remove any weapons from the list that don't match best skill type
                            if (npc.RightHand != null) weaponsList.RemoveAll(w => w.itemID.Equals(npc.RightHand.itemID)); // remove if same weapon as held

                            // Simplest type of check to see what weapon is best -- combat adds.
                            if (npc is Adventurer || (EntityLists.IsHumanOrHumanoid(npc) && Rules.CheckPerception(npc)))
                            {
                                weaponsList.Sort((w1, w2) => w1.combatAdds.CompareTo(w2.combatAdds));

                                if (npc.IsPureMelee && npc.Level >= Character.WARRIOR_SPECIALIZATION_LEVEL)
                                {
                                    weaponsList.RemoveAll(w1 => w1.combatAdds < weaponsList[0].combatAdds);
                                    weaponsList.Sort((w1, w2) => w1.maxDamage.CompareTo(w2.maxDamage));
                                }
                            }

                            if (weaponsList[0] != npc.PivotItem && weaponsList[0] != npc.RightHand)
                            {
                                npc.PivotItem = weaponsList[0];
                                pri = Priority.ManipulateObject;
                                //#if DEBUG
                                //                                npc.SendToAllDEVInSight(npc.Name + " will wield " + npc.pItem.notes + ".");
                                //                                Utils.Log(npc.GetLogString() + " will wield " + npc.pItem.notes + ".", Utils.LogType.Debug);
                                //#endif
                            }
                        }
                    }
                }
                #endregion
            }

            return pri;
        }

        public static Priority Rate_CAST(NPC npc)
        {
            // Amoral alignment never rate cast priority.
            if (npc.Alignment == Globals.eAlignment.Amoral) return Priority.None;

            // If NPC cannot cast it does not rate.
            if (npc.castMode == NPC.CastMode.Never) return Priority.None;

            // NPC is silenced and must warm spells. Do not rate.
            if (npc.HasEffect(Effect.EffectTypes.Silence) && npc.castMode != NPC.CastMode.NoPrep) return Priority.None;

            Priority pri = Priority.None;

            int distance = 0;

            try
            {
                #region Enforcer AI will cast unlimited death spells at an enemy
                if (npc.aiType == NPC.AIType.Enforcer) // enforcer AI
                    if (npc.MostHated != null)
                        return Priority.Enforce;
                #endregion

                #region Priest AI will raise the dead if no enemy is present
                if (npc.aiType == NPC.AIType.Priest && npc.MostHated == null) // look in all visible cells for a player's corpse to raise
                {
                    // priest will sense the need for their services...
                    var cellArray = Cell.GetApplicableCellArray(npc.CurrentCell, Cell.DEFAULT_VISIBLE_DISTANCE);

                    for (int j = 0; j < cellArray.Length; j++)
                    {
                        if (cellArray[j] == null || !npc.CurrentCell.visCells[j])
                        {
                            // do nothing
                        }
                        else
                        {
                            for (int a = 0; a < cellArray[j].Items.Count; a++)
                            {
                                Item item = cellArray[j].Items[a];
                                if (item.itemType == Globals.eItemType.Corpse)
                                {
                                    PC deadPC = PC.GetOnline(item.special);
                                    if (deadPC != null && deadPC.IsDead)
                                        return Priority.RaiseDead;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region Get the distance to our most hated enemy
                if (npc.MostHated != null && npc.CurrentCell != null && npc.MostHated.CurrentCell != null)
                {
                    distance = Cell.GetCellDistance(npc.CurrentCell.X, npc.CurrentCell.Y, npc.MostHated.CurrentCell.X, npc.MostHated.CurrentCell.Y); // get distance between me and my most hated
                }
                #endregion

                #region Unlimited spell casting based on species or if creature is an animal with a spell available
                if (npc.castMode == NPC.CastMode.Unlimited)
                {
                    #region mostHated != null
                    if (npc.MostHated != null)
                    {
                        if(EntityLists.ARACHNID.Contains(npc.entity) && npc.MostHated.CurrentCell.DisplayGraphic != Cell.GRAPHIC_WEB)
                        {
                            pri = Priority.SpellSling;
                        }
                        else if(EntityLists.IsFullBloodedWyrmKin(npc) && distance >= 0 && distance <= 6)
                        {
                            if(Rules.RollD(1, 100) <= 20)
                                pri = Priority.SpellSling;
                        }
                        else if(npc.entity == EntityLists.Entity.Thisson && distance > 0) // Thisson always goes melee combat at distance 0.
                        {
                            if (Rules.RollD(1, 100) <= 50)
                                pri = Priority.SpellSling;
                        }
                        else
                        {
                            if (Rules.RollD(1, 100) <= Math.Min(85, 10 + npc.Level))
                                pri = Priority.SpellSling;
                        }
                    }
                    #endregion
                    else
                    {
                        // TODO: beneficial unlimited spell casting
                    }
                }
                #endregion

                #region else if Limited casting ability (spells must be warmed [prepared] and mana will be spent)
                else if (npc.castMode == CastMode.Limited) // for professions that focus on casting spells
                {
                    if (npc.IsSpellWarmingProfession && npc.HasEffect(Effect.EffectTypes.Silence) && npc.preppedSpell != null) // NPC won't attempt to cast a spell if silenced
                    {
                        return Priority.Rest;
                    }

                    #region mostHated != null
                    if (npc.MostHated != null) // we have an enemy
                    {
                        if(!string.IsNullOrEmpty(npc.MemorizedSpellChant) && !npc.HasEffect(Effect.EffectTypes.Silence))
                        {
                            foreach(int spellID in npc.spellDictionary.Keys)
                            {
                                if(npc.spellDictionary[spellID] == npc.MemorizedSpellChant && GameSpell.GetSpell(spellID) is GameSpell gameSpell && !gameSpell.IsBeneficial)
                                {
                                    npc.preppedSpell = gameSpell;
                                    return Priority.SpellSling;
                                }
                            }
                        }

                        if (npc.preppedSpell != null) // cast a spell if we have mostHated and a prepped spell -- doesn't matter if it is a buff or not
                        {
                            return Priority.SpellSling;
                        }
                        else
                        {
                            // Thieves, knights and ravagers with a target will not cast spells. In the future this could be changed to re-venom weapons.
                            if (npc.BaseProfession == ClassType.Thief)
                            {
                                // cast venom on a weapon, currently the only offensive type spell a thief has 1/15/2017 Eb
                                // if mostHated is already poisoned then don't cast venom
                                if (!npc.MostHated.EffectsList.ContainsKey(Effect.EffectTypes.Venom) && npc.RightHand != null && npc.RightHand.IsPiercingWeapon()
                                    && npc.RightHand.venom <= 0 && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Venom))
                                    return Priority.SpellSling;
                                else if(SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Summon_Humanoid) && npc.Pets.Count < GameSpell.MAX_PETS && Rules.CheckPerception(npc))
                                {
                                    return Priority.SpellSling;
                                }
                                else return Priority.None;
                            }

                            if (npc.IsHybrid) // knights and ravagers
                            {
                                if (npc.Pets.Count == 0 && npc.HasKnightRing && npc.Mana >= 3)
                                    return Priority.SpellSling; // summon hellhound or lamassu
                                else return Priority.None;
                            }

                            if (distance == 0)
                            {
                                if (npc.aiType != NPC.AIType.Priest && npc.aiType != NPC.AIType.Enforcer)
                                {
                                    // If an NPC caster is back to 33% mana move away and cast.
                                    if (npc.Mana >= .33 * npc.ManaMax)
                                    {
                                        if (!npc.ExcludedPrioritiesList.Contains(Priority.RangeMove))
                                            pri = Priority.RangeMove;
                                        else pri = Priority.PrepareSpell;
                                    }
                                }
                                else pri = Priority.PrepareSpell;
                            }
                            else pri = Priority.PrepareSpell;
                        }
                    }
                    #endregion
                    else // we do not have an enemy
                    {
                        if (npc.preppedSpell != null) // we have a prepped spell
                        {
                            // spell types that do not require an enemy...
                            List<Globals.eSpellType> beneficialSpellTypes = new List<Globals.eSpellType>()
                            { Globals.eSpellType.Conjuration, Globals.eSpellType.Divination, Globals.eSpellType.Enchantment, Globals.eSpellType.Illusion, Globals.eSpellType.Necromancy };

                            if (!npc.preppedSpell.IsBeneficial && !beneficialSpellTypes.Contains(npc.preppedSpell.SpellType)) // prepped spell is not beneficial
                            {
                                pri = Priority.Rest; // cancel the non beneficial prepped spell
                            }
                            else pri = Priority.SpellSling;
                        }
                        else
                        {
                            // Check to cast light spell here.
                            //if (!npc.IsUndead && !npc.CurrentCell.IsAlwaysDark && !npc.HasNightVision && Rules.CheckPerception(npc) &&
                            //       npc.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) && npc.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Light) &&
                            //       npc.HasManaAvailable((int)GameSpell.GameSpellID.Light))
                            //{
                            //    npc.buffSpellCommand = "light";
                            //    pri = Priority.PrepareSpell;
                            //}

                            // Thieves opt to hide if no enemies present.
                            //if (npc.BaseProfession == Character.ClassType.Thief && !npc.IsHidden && Map.IsNextToWall(npc))
                            //{
                            //    // first priority for a thief is to hide in shadows... for now
                            //    GameSpell spell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Hide_in_Shadows);
                            //    if (npc.HasManaAvailable(spell.ID))
                            //    {
                            //        npc.buffSpellCommand = spell.Command;
                            //        npc.BuffTargetID = npc.UniqueID;
                            //        pri = Priority.Buff;
                            //    }
                            //}

                            if (npc.ManaFull > 0 && (double)npc.Mana / npc.ManaFull * 100 >= .25) // Do not buff if we are less than 25% mana.
                            {
                                List<string> beneficialSpellCommands = new List<string>();

                                // Abjuration spells only for now.
                                foreach (int spellID in npc.spellDictionary.Keys)
                                {
                                    GameSpell spell = GameSpell.GetSpell(spellID);

                                    if (spell.IsBeneficial)
                                    {
                                        beneficialSpellCommands.Add(spell.Command.ToLower());
                                    }
                                }

                                if (beneficialSpellCommands.Count > 0)
                                {
                                    switch (npc.BaseProfession)
                                    {
                                        case ClassType.Ranger:
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Hunter__s_Mark) && !npc.HasEffect(Effect.EffectTypes.Hunter__s_Mark) && npc.RightHand != null)
                                            {
                                                npc.buffSpellCommand = "huntersmark";
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Barkskin) && !npc.HasEffect(Effect.EffectTypes.Barkskin) && !npc.HasEffect(Effect.EffectTypes.Stoneskin))
                                            {
                                                npc.buffSpellCommand = "barkskin";
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            if (pri == Priority.Buff) break;
                                            break;
                                        case ClassType.Druid:
                                            #region Druid buffs -- mostly self, though will buff their own pets with haste or mana regen
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Neutralize_Poison) && npc.Hits > (int)(npc.HitsFull * .75))
                                            {
                                                if (npc.EffectsList.ContainsKey(Effect.EffectTypes.Poison) || npc.EffectsList.ContainsKey(Effect.EffectTypes.Venom) || npc.Poisoned > 0)
                                                {
                                                    npc.buffSpellCommand = "neutralize";
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
                                                }
                                                else if (npc.Group != null)
                                                {
                                                    foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                    {
                                                        if (groupNPC.BaseProfession != npc.BaseProfession && (groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Poison) || groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Venom) || groupNPC.Poisoned > 0))
                                                        {
                                                            npc.buffSpellCommand = "neutralize";
                                                            npc.BuffTargetID = groupNPC.UniqueID;
                                                            pri = Priority.Buff;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Strength) && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Temporary_Strength))
                                            {
                                                npc.buffSpellCommand = "strength";
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Ataraxia) && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Ataraxia))
                                            {
                                                npc.buffSpellCommand = "ataraxia";
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Barkskin) && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Barkskin))
                                            {
                                                npc.buffSpellCommand = "barkskin";
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Regeneration) && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Regeneration))
                                            {
                                                npc.buffSpellCommand = "regeneration";
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Protection_from_Fire_and_Ice) && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire_and_Ice))
                                            {
                                                npc.buffSpellCommand = "prfireice";
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            break; 
                                        #endregion
                                        case ClassType.Thaumaturge:
                                            #region Thaumaturge Buffs (neutralize poison, strength, prfireice or prfire and price)
                                            try
                                            {
                                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Neutralize_Poison))
                                                {
                                                    if ((npc.EffectsList.ContainsKey(Effect.EffectTypes.Poison) || npc.EffectsList.ContainsKey(Effect.EffectTypes.Venom) || npc.Poisoned > 0) && npc.Hits > (int)(npc.HitsFull * .75))
                                                    {
                                                        npc.buffSpellCommand = "neutralize";
                                                        npc.BuffTargetID = npc.UniqueID;
                                                        pri = Priority.Buff;
                                                    }
                                                    else if (npc.Group != null)
                                                    {
                                                        foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                        {
                                                            if (groupNPC.BaseProfession != npc.BaseProfession && (groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Poison) || groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Venom) || groupNPC.Poisoned > 0))
                                                            {
                                                                npc.buffSpellCommand = "neutralize";
                                                                npc.BuffTargetID = groupNPC.UniqueID;
                                                                pri = Priority.Buff;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Utils.Log("Error in AI, while casting neutralize poison buff as a thaumaturge.", Utils.LogType.SystemFailure);
                                                Utils.LogException(e);
                                            }

                                            if (pri == Priority.Buff) break;

                                            try
                                            {
                                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Strength))
                                                {
                                                    if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Temporary_Strength))
                                                    {
                                                        npc.buffSpellCommand = "strength";
                                                        npc.BuffTargetID = npc.UniqueID;
                                                        pri = Priority.Buff;
                                                    }
                                                    else if (npc.Group != null)
                                                    {
                                                        foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                        {
                                                            if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Temporary_Strength))
                                                            {
                                                                npc.buffSpellCommand = "strength";
                                                                npc.BuffTargetID = groupNPC.UniqueID;
                                                                pri = Priority.Buff;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Utils.Log("Error in AI, while casting strength buff as a thaumaturge.", Utils.LogType.SystemFailure);
                                                Utils.LogException(e);
                                            }

                                            if (pri == Priority.Buff) break;

                                            try
                                            {
                                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Protection_from_Fire_and_Ice))
                                                {
                                                    if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire_and_Ice) &&
                                                        (!npc.immuneFire && !npc.immuneCold))
                                                    {
                                                        npc.buffSpellCommand = "prfireice";
                                                        npc.BuffTargetID = npc.UniqueID;
                                                        pri = Priority.Buff;
                                                    }
                                                    else if (npc.Group != null)
                                                    {
                                                        foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                        {
                                                            if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire_and_Ice) &&
                                                                (!npc.immuneFire && !npc.immuneCold))
                                                            {
                                                                npc.buffSpellCommand = "prfireice";
                                                                npc.BuffTargetID = groupNPC.UniqueID;
                                                                pri = Priority.Buff;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Utils.Log("Error in AI, while casting prfireice buff as a thaumaturge.", Utils.LogType.SystemFailure);
                                                Utils.LogException(e);
                                            }

                                            if (pri == Priority.Buff) break;

                                            try
                                            {
                                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Protection_from_Fire))
                                                {
                                                    if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire) && !npc.immuneFire)
                                                    {
                                                        npc.buffSpellCommand = "prfire";
                                                        npc.BuffTargetID = npc.UniqueID;
                                                        pri = Priority.Buff;
                                                    }
                                                    else if (npc.Group != null)
                                                    {
                                                        foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                        {
                                                            if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire) && !npc.immuneFire)
                                                            {
                                                                npc.buffSpellCommand = "prfire";
                                                                npc.BuffTargetID = groupNPC.UniqueID;
                                                                pri = Priority.Buff;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Utils.Log("Error in AI, while casting prfire buff as a thaumaturge.", Utils.LogType.SystemFailure);
                                                Utils.LogException(e);
                                            }

                                            if (pri == Priority.Buff) break;

                                            try
                                            {
                                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Protection_from_Cold))
                                                {
                                                    if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire) && !npc.immuneCold)
                                                    {
                                                        npc.buffSpellCommand = "prcold";
                                                        npc.BuffTargetID = npc.UniqueID;
                                                        pri = Priority.Buff;
                                                    }
                                                    else if (npc.Group != null)
                                                    {
                                                        foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                        {
                                                            if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire) && !npc.immuneCold)
                                                            {
                                                                npc.buffSpellCommand = "prcold";
                                                                npc.BuffTargetID = groupNPC.UniqueID;
                                                                pri = Priority.Buff;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Utils.Log("Error in AI, while casting prcold buff as a thaumaturge.", Utils.LogType.SystemFailure);
                                                Utils.LogException(e);
                                            }
                                            break;
                                        #endregion
                                        case ClassType.Wizard:
                                            #region Wizard Buffs (shield, prfireice, or prfire & price)
                                            if (beneficialSpellCommands.Contains("shield") && npc.HasManaAvailable("shield"))
                                            {
                                                if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Shield))
                                                {
                                                    npc.buffSpellCommand = "shield";
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
                                                }
                                                else if (npc.Group != null)
                                                {
                                                    foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                    {
                                                        if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Shield))
                                                        {
                                                            npc.buffSpellCommand = "shield";
                                                            npc.BuffTargetID = groupNPC.UniqueID;
                                                            pri = Priority.Buff;
                                                        }
                                                    }
                                                }
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (beneficialSpellCommands.Contains("prfireice") && npc.HasManaAvailable("prfireice"))
                                            {
                                                if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire_and_Ice) &&
                                                    (!npc.immuneFire && !npc.immuneCold))
                                                {
                                                    npc.buffSpellCommand = "prfireice";
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
                                                }
                                                else if (npc.Group != null)
                                                {
                                                    foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                    {
                                                        if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire_and_Ice) &&
                                                            (!npc.immuneFire && !npc.immuneCold))
                                                        {
                                                            npc.buffSpellCommand = "prfireice";
                                                            npc.BuffTargetID = groupNPC.UniqueID;
                                                            pri = Priority.Buff;
                                                        }
                                                    }
                                                }
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (beneficialSpellCommands.Contains("prfire") && npc.HasManaAvailable("prfire"))
                                            {
                                                if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire) && !npc.immuneFire)
                                                {
                                                    npc.buffSpellCommand = "prfire";
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
                                                }
                                                else if (npc.Group != null)
                                                {
                                                    foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                    {
                                                        if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire) && !npc.immuneFire)
                                                        {
                                                            npc.buffSpellCommand = "prfire";
                                                            npc.BuffTargetID = groupNPC.UniqueID;
                                                            pri = Priority.Buff;
                                                        }
                                                    }
                                                }
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (beneficialSpellCommands.Contains("prcold") && npc.HasManaAvailable("prcold"))
                                            {
                                                if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire) && !npc.immuneCold)
                                                {
                                                    npc.buffSpellCommand = "prcold";
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
                                                }
                                                else if (npc.Group != null)
                                                {
                                                    foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                    {
                                                        if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Fire) && !npc.immuneCold)
                                                        {
                                                            npc.buffSpellCommand = "prcold";
                                                            npc.BuffTargetID = groupNPC.UniqueID;
                                                            pri = Priority.Buff;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        #endregion
                                        case ClassType.Sorcerer:
                                            #region Sorcerer Buffs (shield, protection from undead, protection from acid)
                                            if (beneficialSpellCommands.Contains("shield") && npc.HasManaAvailable("shield"))
                                            {
                                                if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Shield))
                                                {
                                                    npc.buffSpellCommand = "shield";
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
                                                }
                                                else if (npc.Group != null)
                                                {
                                                    foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                    {
                                                        if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Shield))
                                                        {
                                                            npc.buffSpellCommand = "shield";
                                                            npc.BuffTargetID = groupNPC.UniqueID;
                                                            pri = Priority.Buff;
                                                        }
                                                    }
                                                }
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (beneficialSpellCommands.Contains("prundead") && npc.HasManaAvailable("prundead"))
                                            {
                                                if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Undead))
                                                {
                                                    npc.buffSpellCommand = "prundead";
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
                                                }
                                                else if (npc.Group != null)
                                                {
                                                    foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                    {
                                                        if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Undead))
                                                        {
                                                            npc.buffSpellCommand = "prundead";
                                                            npc.BuffTargetID = groupNPC.UniqueID;
                                                            pri = Priority.Buff;
                                                        }
                                                    }
                                                }
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (beneficialSpellCommands.Contains("pracid") && npc.HasManaAvailable("pracid"))
                                            {
                                                if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Acid))
                                                {
                                                    npc.buffSpellCommand = "pracid";
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
                                                }
                                                else if (npc.Group != null)
                                                {
                                                    foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                    {
                                                        if (groupNPC.BaseProfession != npc.BaseProfession && !groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Protection_from_Acid))
                                                        {
                                                            npc.buffSpellCommand = "pracid";
                                                            npc.BuffTargetID = groupNPC.UniqueID;
                                                            pri = Priority.Buff;
                                                        }
                                                    }
                                                }
                                            }
                                            break;
                                        #endregion
                                        case ClassType.Thief:
                                            #region Thieves (hide, speed, venom, disguise)
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Neutralize_Poison))
                                            {
                                                if (npc.EffectsList.ContainsKey(Effect.EffectTypes.Poison) || npc.EffectsList.ContainsKey(Effect.EffectTypes.Venom) || npc.Poisoned > 0)
                                                {
                                                    npc.buffSpellCommand = "neutralize";
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
                                                }
                                                else if (npc.Group != null)
                                                {
                                                    foreach (NPC groupNPC in new List<NPC>(npc.Group.GroupNPCList))
                                                    {
                                                        if (groupNPC.BaseProfession != npc.BaseProfession && (groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Poison) || groupNPC.EffectsList.ContainsKey(Effect.EffectTypes.Venom) || groupNPC.Poisoned > 0))
                                                        {
                                                            npc.buffSpellCommand = "neutralize";
                                                            npc.BuffTargetID = groupNPC.UniqueID;
                                                            pri = Priority.Buff;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (npc.HasManaAvailable("hide") && !npc.IsHidden && Map.IsNextToWall(npc))
                                            {
                                                npc.buffSpellCommand = "hide";
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (!npc.HasSpeed && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Speed))
                                            {
                                                npc.buffSpellCommand = GameSpell.GameSpellID.Speed.ToString().ToLower(); ;
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Venom) && npc.RightHand != null && npc.RightHand.IsPiercingWeapon()
                                                && npc.RightHand.venom <= 0)
                                            {
                                                    npc.buffSpellCommand = GameSpell.GameSpellID.Venom.ToString().ToLower();
                                                    npc.BuffTargetID = npc.UniqueID;
                                                    pri = Priority.Buff;
#if DEBUG
                                                    npc.SendToAllDEVInSight(npc.GetNameForActionResult() + " will cast venom spell...");
#endif
                                            }
                                            if (pri == Priority.Buff) break;
                                            if (!npc.HasOfuscation && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Obfuscation))
                                            {
                                                npc.buffSpellCommand = GameSpell.GameSpellID.Obfuscation.ToString().ToLower();
                                                npc.BuffTargetID = npc.UniqueID;
                                                pri = Priority.Buff;
                                            }
                                            break;
                                        #endregion
                                        default:
                                            break;
                                    }
                                } // end buffs
                            }
                        }
                    }
                }
                #endregion

                #region No warming spell, only limited by mana pool (knights, innate spell abilities with mana limitations and most summoned creatures
                else if (npc.castMode == NPC.CastMode.NoPrep)
                {
                    if (npc.MostHated != null)
                    {
                        #region Summoned or Pets
                        if (npc.IsSummoned || npc.PetOwner != null)
                        {
                            if (npc.evocationSpells.Count > 0)
                            {
                                if (Rules.RollD(1, 100) <= 15)
                                {
                                    //TODO: This should be more random. Randomly choose a !IsBeneficial spell and if we don't have mana, try once more. If still not enough mana return Priority.None.
                                    foreach (int spellID in npc.evocationSpells.Keys)
                                    {
                                        GameSpell spell = GameSpell.GetSpell(spellID);
                                        if (!spell.IsBeneficial && npc.Mana >= spell.ManaCost)
                                        {
                                            pri = Priority.SpellSling;
                                            npc.preppedSpell = spell;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (pri != Priority.SpellSling && distance >= 2 && npc.evocationAreaEffectSpells.Count > 0 && npc.enemyList.Count > Rules.RollD(2, 2))
                            {
                                // 15% chance to cast a spell
                                if (Rules.RollD(1, 100) <= 15)
                                {
                                    foreach (int spellID in npc.evocationAreaEffectSpells.Keys)
                                    {
                                        GameSpell spell = GameSpell.GetSpell(spellID);
                                        if (!spell.IsBeneficial && npc.Mana >= spell.ManaCost)
                                        {
                                            pri = Priority.SpellSling;
                                            npc.preppedSpell = spell;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                        else
                        {
                            switch (npc.BaseProfession)
                            {
                                case ClassType.Knight:
                                    if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Cure) && npc.Hits < (int)(npc.HitsFull * .5))
                                    {
                                        npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Cure);
                                        pri = Priority.SpellSling;
                                        break;
                                    }
                                    else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Summon_Lamassu) && (npc.Pets == null || npc.Pets.Count <= 0))
                                    {
                                        npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Summon_Lamassu);
                                        pri = Priority.SpellSling;
                                        break;
                                    }
                                    else if(SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Blessing_of_the_Faithful) && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Bless))
                                    {
                                        npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Blessing_of_the_Faithful);
                                        pri = Priority.SpellSling;
                                        break;
                                    }
                                    else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Strength) && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Temporary_Strength) && npc.Hits >= (int)(npc.HitsFull * .6))
                                    {
                                        npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Strength);
                                        pri = Priority.SpellSling;
                                        break;
                                    }
                                    if (pri != Priority.SpellSling) pri = Priority.None;
                                    break;
                                case ClassType.Ravager:
                                    if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Lifeleech) && npc.Hits < (int)(npc.HitsFull * .5))
                                    {
                                        npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Lifeleech);
                                        pri = Priority.SpellSling;
                                        break;
                                    }
                                    else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Summon_Hellhound) && (npc.Pets == null || npc.Pets.Count <= 0))
                                    {
                                        // Ravager has an enemy. Summon a hellhound.
                                        GameSpell spell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Summon_Hellhound);
                                        if (npc.Mana >= spell.ManaCost)
                                        {
                                            npc.preppedSpell = spell;
                                            pri = Priority.SpellSling;
                                            break;
                                        }
                                    }
                                    else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Flame_Shield) && !npc.HasEffect(Effect.EffectTypes.Flame_Shield))
                                    {
                                        npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Flame_Shield);
                                        pri = Priority.SpellSling;
                                        break;
                                    }
                                    else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Strength) && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Temporary_Strength) && npc.Hits >= (int)(npc.HitsFull * .6))
                                    {
                                        npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Strength);
                                        pri = Priority.SpellSling;
                                        break;
                                    }
                                    if (pri != Priority.SpellSling) pri = Priority.None;
                                    break;
                                default:
                                    break;
                            }

                            // Last resort.
                            if (pri != Priority.SpellSling && Rules.RollD(1, 100) >= 60 && npc.Mana >= npc.ManaFull / 5 &&
                                (npc.evocationAreaEffectSpells.Count > 0 || npc.alterationHarmfulSpells.Count > 0 || npc.evocationSpells.Count > 0))
                                pri = Priority.SpellSling;
                        }
                    }
                    else
                    {
                        #region Spell user hybrids may have buffs to cast. Currently only knights will make it to this portion of the code.
                        if (npc.IsSpellUser && npc.IsHybrid)
                        {
                            switch (npc.BaseProfession)
                            {
                                case Character.ClassType.Knight:
                                    #region Knights will cast bless and strength on self and group members if mana is available.
                                    if (npc.HasManaAvailable("bless") && !npc.EffectsList.ContainsKey(Effect.EffectTypes.Bless))
                                    {
                                        npc.buffSpellCommand = "bless";
                                        npc.BuffTargetID = npc.UniqueID;
                                        pri = Priority.Buff;
                                    }
                                    else if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Temporary_Strength) && npc.HasManaAvailable("strength"))
                                    {
                                        npc.buffSpellCommand = "strength";
                                        npc.BuffTargetID = npc.UniqueID;
                                        pri = Priority.Buff;
                                    }
                                    break;
                                #endregion
                                case Character.ClassType.Ravager:
                                    #region Ravagers will cast strength and burnshield on self only.
                                    if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Temporary_Strength) && npc.HasManaAvailable("strength"))
                                    {
                                        npc.buffSpellCommand = "strength";
                                        npc.BuffTargetID = npc.UniqueID;
                                        pri = Priority.Buff;
                                    }
                                    else if (!npc.EffectsList.ContainsKey(Effect.EffectTypes.Flame_Shield) && npc.HasManaAvailable("flameshield"))
                                    {
                                        npc.buffSpellCommand = "flameshield";
                                        npc.BuffTargetID = npc.UniqueID;
                                        pri = Priority.Buff;
                                    }
                                    break;
                                #endregion
                                default:
                                    break;
                            }
                        }
                        #endregion
                    }
                }
                #endregion

                #region Look for a wounded ally or heal myself if I can, have mana, and do not have a spell prepped
                /*  Lawful healers will look for an ally at or below 50% health to heal and if they find none, will heal themselves if
                    they are at or below %50. All other alignments will heal themselves first and then look for an ally. Any AI that doesn't
                    have a mostHated will remain in "heal mode" until they are back at full health. */
                #region NPC can heal others, has mana available for cure spell, and doesn't have a spell warmed = warm cure spell if necessary (view code).
                if (npc.IsHealer() && // we are a healer
                            npc.HasManaAvailable(GameSpell.GetSpell("cure").ID) && // we have mana to cast the cure spell
                            npc.preppedSpell == null) // we do not have a spell prepared
                // if this creature can heal itself and others
                {
                    Character wounded = npc;

                    #region Lawful Healers
                    if (npc.Alignment == Globals.eAlignment.Lawful) // lawful creatures will try to heal others first
                    {
                        for (int a = 0; a < npc.friendList.Count; a++) // loop through target array and see if there is another wounded
                        {
                            Character friend = npc.friendList[a];
                            if (friend.Hits / friend.HitsFull < wounded.Hits / wounded.HitsFull) // designate the most wounded
                                wounded = friend;
                        }
                        if (npc.aiType == NPC.AIType.Priest) // NPC.AIType.Priest will heal up to and including 75% health
                        {
                            if (wounded != null && wounded.Hits > (int)(wounded.HitsFull * .75))
                                wounded = null;
                        }
                        else // all other healers will heal up to and including 50% health
                        {
                            if (wounded != null && wounded.Hits > (int)(wounded.HitsFull * .50))
                                wounded = null;
                        }
                    }
                    #endregion

                    #region Non Lawful Healers
                    else
                    {
                        wounded = null; // null the wounded character
                        if (npc.Hits <= (int)(npc.HitsFull * .50)) // if creatures health is below 50% designate itself as wounded
                            wounded = npc;
                        else if (npc.friendList.Count > 0)
                        {
                            for (int a = 0; a < npc.friendList.Count; a++) // loop through target array and see if there is another wounded
                            {
                                Character friend = npc.friendList[a];

                                if (wounded == null)
                                    wounded = friend;
                                else if (friend.Hits / friend.HitsFull < wounded.Hits / wounded.HitsFull) // designate the most wounded
                                    wounded = friend;
                            }

                            if (npc.aiType == NPC.AIType.Priest) // even though there are currently no non lawful NPC.AIType.Priest, there may be in the future
                            {
                                if (wounded != null && wounded.Hits >= (int)(wounded.HitsFull * .75)) // do not heal a wounded character if health is above 75%
                                    wounded = null;
                            }
                            else if (wounded != null && wounded.Hits >= (int)(wounded.HitsFull * .50)) // do not heal a wounded character if health is above 50%
                                wounded = null;
                        }
                    }
                    #endregion

                    if (wounded != null && (wounded == npc || npc.friendList.Contains(wounded)))
                    {
                        // currently limited to cure spell TODO modify this and sorcerer AI for sorcerers to heal servants
                        if (wounded != null && !wounded.IsUndead && npc.HasManaAvailable(GameSpell.GetSpell("cure").ID)) // creature or a friend is wounded and I can cure them
                        {
                            npc.BuffTargetID = wounded.UniqueID;
                            pri = Priority.Heal;
                        }
                        else if (wounded != null && npc.MostHated == null && !npc.IsUndead && npc.Hits < npc.HitsFull && npc.HasManaAvailable((int)GameSpell.GameSpellID.Cure)) // top off my hits
                        {
                            npc.BuffTargetID = wounded.UniqueID;
                            pri = Priority.Heal;
                        }
                    }
                }
                #endregion
                #endregion

                return pri;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return pri;
            }
        }

        /// <summary>
        /// Lair creatures return home. NPCs rest, flee, or meditate.
        /// </summary>
        /// <param name="npc"></param>
        /// <returns></returns>
        public static Priority Rate_SPECIAL(NPC npc)
        {
            Priority pri = Priority.None;

            if (npc.MostHated == null)
            {
                // npc was immobile, is now mobile, has no follow target, and is not at spawn coords
                if ((!npc.IsMobile || npc.wasImmobile) && npc.PetOwner == null &&
                    npc.CurrentCell != Cell.GetCell(npc.FacetID, npc.LandID, npc.MapID, npc.spawnXCord, npc.spawnYCord, npc.spawnZCord))
                    return Priority.GoHome;

                if (npc.preppedSpell != null) // return no SPECIAL priority if we have a spell prepped
                    return pri;

                // SUMMONER entities summon all flagged players to them
                if(EntityLists.SUMMONER.Contains(npc.entity) && npc.FlaggedUniqueIDs != null && npc.FlaggedUniqueIDs.Count > 0)
                {
                    foreach(int id in npc.FlaggedUniqueIDs)
                    {
                        if(id >= 0 && PC.GetOnline(id) is PC flaggedPC && TargetAcquisition.FindTargetInView(npc, id, false, true) == null)
                        {
                            CommandTasker.ParseCommand(npc, "impsummon", flaggedPC.Name);
                            //npc.TargetID = id;
                            //return Priority.SummonFlagged;
                        }
                        // don't look for NPCs yet 10/19/19 Eb
                    }
                }

                if (npc.lairCritter)
                {
                    #region Lair creatures return to their lair and defend it.
                    if (npc.X != npc.lairXCord && npc.Y != npc.lairYCord) // not at our lair coordinate
                    {
                        int distance = Cell.GetCellDistance(npc.X, npc.Y, npc.lairXCord, npc.lairYCord);

                        if (distance >= 10) // warp back to our lair
                        {
                            npc.CurrentCell = Cell.GetCell(npc.FacetID, npc.LandID, npc.MapID, npc.lairXCord, npc.lairYCord, npc.lairZCord);
                        }
                        else if (distance < 10) // within view of our lair
                        {
                            if (npc.Hits < npc.HitsFull) // we may rest or we may test our lair defenses
                            {
                                pri = Priority.LairDefense;
                            }
                        }
                        else if (npc.preppedSpell == null && !npc.HasPatrol && npc.PetOwner == null)
                        {
                            pri = Priority.GoHome;
                        }
                    }
                    else // we are at our lair coordinate
                    {
                        if (npc.Hits < npc.HitsFull) // we're not casting a spell, we're at our lair, we're wounded... so let's rest
                        {
                            pri = Priority.LairDefense;
                        }
                    }
                    #endregion
                }
                else // not a lair critter
                {
                    if ((double)npc.Hits / npc.HitsFull * 100 <= .85) // we're below 85% health so let's rest
                    {
                        if ((double)npc.Hits / npc.HitsFull * 100 <= .25 && npc.Group == null && npc.DamageRound <= DragonsSpineMain.GameRound - 2)
                        {
                            pri = Priority.Flee;
                        }
                        else pri = Priority.Rest;
                    }
                    else if (EntityLists.IsHumanOrHumanoid(npc) && npc.IsSpellUser && (npc.castMode == CastMode.Limited || npc.castMode == CastMode.NoPrep) && (double)npc.Mana / npc.ManaFull <= .50)
                    {
                        // remember there is no enemy present. let's meditate if we're human, or humanoid. it's a high priority.
                        pri = Priority.Meditate;
                    }
                }
            }
            else
            {
                if (npc.lairCritter && Cell.GetCellDistance(npc.X, npc.Y, npc.lairXCord, npc.lairYCord) >= 10)
                    npc.CurrentCell = Cell.GetCell(npc.FacetID, npc.LandID, npc.MapID, npc.lairXCord, npc.lairYCord, npc.lairZCord);
            }

            return pri;
        }

        public static Priority Rate_MOVE(NPC npc)
        {
            Priority pri = Priority.None;

            if (npc.EffectsList.ContainsKey(Effect.EffectTypes.Ensnare))
                return Priority.None;

            // Pets have a mostHated after being instructed to attack.
            if (npc.PetOwner != null && npc.MostHated != null && npc.CurrentCell != npc.MostHated.CurrentCell && !npc.IsGuarding)
                return Priority.Advance;

            // NPC is not mobile.
            if (!npc.IsMobile)
            {
                // AI has no enemy or is truly not mobile (currently only statues, check property)
                if (npc.MostHated == null || npc.IsTrulyImmobile ||
                    (npc is Merchant && (npc as Merchant).interactiveType != Merchant.InteractiveType.None) || (npc is Merchant && (npc as Merchant).trainerType != Merchant.TrainerType.None))
                {
                    if(!EntityLists.MOBILE.Contains(npc.entity))
                        return Priority.None;
                }

                if (npc.MostHated != null) // we have a most hated and it should be possible for us to move since we are not truly immobile
                {
                    npc.IsMobile = true;
                    npc.wasImmobile = true;
                }
                else return pri; // return Priority.None if we're not supposed to be moving around
            }
            else if (npc.MostHated == null && npc.wasImmobile && npc.PetOwner == null)
            {
                {
                    if (npc.X != npc.spawnXCord || npc.Y != npc.spawnYCord || npc.Z != npc.spawnZCord)
                    {
                        return Priority.GoHome;
                    }
                    else // we are at our spawn coordinates so let's become immobile again and return no move priority
                    {
                        npc.IsMobile = false;
                        npc.wasImmobile = false;
                        return pri;
                    }
                }
            }

            // return no priority if the AI is supposed to be standing still
            if (npc.MostHated == null && npc.EffectsList.ContainsKey(Effect.EffectTypes.Hello_Immobility))
                return pri;
            else if (npc.MostHated != null && npc.EffectsList.ContainsKey(Effect.EffectTypes.Hello_Immobility))
                npc.EffectsList[Effect.EffectTypes.Hello_Immobility].StopCharacterEffect();

            // NPC will flee if below 20% health. Pets must be instructed to flee.
            if (npc.Hits < (int)(npc.HitsFull / 5))
            {
                // not a lair critter, not undead, no friends and not in a group
                if (!npc.lairCritter && !npc.IsUndead && npc.friendList.Count <= 0 &&
                    (npc.Group == null || npc.Group.GroupNPCList.Count <= 0) && npc.enemyList.Count > 0)
                {
                    if (!FightNotFlight(npc))
                    {
                        pri = Priority.Flee;
                        return pri;
                    }
                }
            }

            if (npc.TotalFearLove == 0 && npc.TotalHate == 0)
            {
                if (npc.PetOwner == null)
                {
                    // temporary means of keeping non permanent hide spell active TODO: upgrade this 10/18/2015 Eb
                    if (npc.IsHidden && !npc.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent)
                    {
                        if (Rules.RollD(1, 100) > 75) pri = Priority.Wander;
                    }
                    else pri = Priority.Wander;

                    // Investigate a mostHated that left the area.
                    if (npc.previousMostHated != null && Rules.CheckPerception(npc) && !npc.previousMostHated.IsHidden)
                    {
                        // The distance to scan may need to be reduced in the future.
                        var nearbyCellScan = Cell.GetApplicableCellArray(npc.CurrentCell, 8);
                        foreach (var cell in nearbyCellScan)
                        {
                            if (cell == null) continue; //TODO: fix GetApplicableCellArray, make it a List<Cell> or remove null cells. 10/10/2015 Eb

                            if (cell.Characters.ContainsKey(npc.previousMostHated.UniqueID))
                            {
                                pri = Priority.Investigate;
                                break;
                            }
                        }
                    }

                    if (pri < Priority.Investigate && EntityLists.MAGIC_SNIFFER.Contains(npc.entity))
                    {
                        var nearbyCellScan = Cell.GetApplicableCellArray(npc.CurrentCell, 10);
                        foreach (var cell in nearbyCellScan)
                        {
                            if (cell == null) continue; //TODO: fix GetApplicableCellArray, make it a List<Cell> or remove null cells. 10/10/2015 Eb

                            if (cell.Characters.Count > 0)
                            {
                                foreach (Character chr in cell.Characters.Values)
                                {
                                    if (Rules.DetectAlignment(chr, npc) && chr.preppedSpell != null)
                                    {
                                        npc.gotoWarmedMagic = chr.X + "|" + chr.Y + "|" + chr.Z;
                                        pri = Priority.InvestigateMagic;
                                        break;
                                    }
                                }
                            }

                            if (pri >= Priority.Investigate)
                                break;
                        }
                    }
                }
            }
            else if (npc.TotalFearLove > 0 && npc.TotalHate == 0)
            {
                // Animals make it here even with a mostHated.

                if (npc.PetOwner == null)
                {
                    // Thieves won't wander as much...
                    if (npc.IsHidden && !npc.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent && Rules.RollD(1, 100) > 75) pri = Priority.Wander;
                    else if (npc.MostHated == null) pri = Priority.Wander;

                    // move toward friend FLCenter - normal
                    //if (Cell.GetCellDistance(npc.X, npc.Y, npc.FearCenterX, npc.FearCenterY) > 2)
                    //    pri = Priority.Socialize;

                    if (npc.MostHated == null && npc.previousMostHated != null)
                        pri = Priority.Investigate;

                    if (pri < Priority.InvestigateMagic && EntityLists.MAGIC_SNIFFER.Contains(npc.entity))
                    {
                        var nearbyCellScan = Cell.GetApplicableCellArray(npc.CurrentCell, 10);
                        foreach (var cell in nearbyCellScan)
                        {
                            if (cell == null) continue; //TODO: fix GetApplicableCellArray, make it a List<Cell> or remove null cells. 10/10/2015 Eb

                            if (cell.Characters.Count > 0)
                            {
                                foreach (Character chr in cell.Characters.Values)
                                {
                                    if (Rules.DetectAlignment(chr, npc) && chr.preppedSpell != null)
                                    {
                                        npc.gotoWarmedMagic = chr.X + "|" + chr.Y + "|" + chr.Z;
                                        pri = Priority.InvestigateMagic;
                                        break;
                                    }
                                }
                            }

                            if (pri >= Priority.Investigate)
                                break;
                        }
                    }
                }
            }
            else if (npc.TotalFearLove < 0 && Math.Abs(npc.TotalFearLove) > npc.TotalHate)
            {
                // Animals often arrive here when rating a move.

                if (!EntityLists.ANIMAL.Contains(npc.entity))
                {
                    if (npc.RightHand != null && npc.RightHand.skillType == Globals.eSkillType.Bow && !npc.RightHand.IsNocked)
                    {
                        pri = Priority.RangeMove;
                    }
                    else if (npc.MostHated != null)
                    {
                        if (npc.TargetID != npc.MostHated.UniqueID)
                            npc.EmitSound(npc.attackSound);

                        npc.TargetID = npc.MostHated.UniqueID;
                        npc.FollowID = npc.MostHated.UniqueID;

                        if (npc.RightHand != null && (!npc.RightHand.returning || !npc.RightHand.RangePreferred()))
                            pri = Priority.Advance;
                    }
                    else pri = Priority.Wander;
                }
                else if (npc.MostHated != null)
                {
                    if (npc.TargetID != npc.MostHated.UniqueID)
                        npc.EmitSound(npc.attackSound);

                    npc.TargetID = npc.MostHated.UniqueID;
                    npc.FollowID = npc.MostHated.UniqueID;

                    pri = Priority.Advance;
                    // All animals will currently advance. TODO: Animal range attacks, ie: manticore barbs, spiders spitting venom 10/7/2015
                }
                else pri = Priority.Wander;
            }
            else if (npc.MostHated != null)
            {
                if (!EntityLists.ANIMAL.Contains(npc.entity))
                {
                    if (npc.RightHand != null && npc.RightHand.skillType == Globals.eSkillType.Bow && !npc.RightHand.IsNocked)
                    {
                        pri = Priority.RangeMove;
                    }
                    else
                    {
                        if (npc.TargetID != npc.MostHated.UniqueID)
                            npc.EmitSound(npc.attackSound);

                        npc.TargetID = npc.MostHated.UniqueID;
                        npc.FollowID = npc.MostHated.UniqueID;

                        if (npc.RightHand != null && (!npc.RightHand.returning || !npc.RightHand.RangePreferred()))
                            pri = Priority.Advance;
                    }
                }
                else
                {
                    if (npc.TargetID != npc.MostHated.UniqueID)
                        npc.EmitSound(npc.attackSound);

                    npc.TargetID = npc.MostHated.UniqueID;
                    npc.FollowID = npc.MostHated.UniqueID;

                    pri = Priority.Advance;
                    // All animals will currently advance. TODO: Animal range attacks, ie: manticore barbs, spiders spitting venom 10/7/2015
                }
            }
            else if (npc.IsHidden && !npc.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent)
            {
                if (Rules.RollD(1, 100) > 75) pri = Priority.Wander;
            }
            else
            {
                if (npc.MostHated == null && npc.previousMostHated != null)
                    pri = Priority.Investigate;

                if (pri < Priority.InvestigateMagic && EntityLists.MAGIC_SNIFFER.Contains(npc.entity))
                {
                    var nearbyCellScan = Cell.GetApplicableCellArray(npc.CurrentCell, 10);
                    foreach (var cell in nearbyCellScan)
                    {
                        if (cell == null) continue; //TODO: fix GetApplicableCellArray, make it a List<Cell> or remove null cells. 10/10/2015 Eb

                        if (cell.Characters.Count > 0)
                        {
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if (Rules.DetectAlignment(chr, npc) && chr.preppedSpell != null)
                                {
                                    npc.gotoWarmedMagic = chr.X + "|" + chr.Y + "|" + chr.Z;
                                    pri = Priority.InvestigateMagic;
                                    break;
                                }
                            }
                        }

                        if (pri >= Priority.Investigate)
                            break;
                    }
                }

                if(pri <= Priority.Wander)
                    pri = Priority.Wander;
            }

            return pri;
        }

        public static Priority Rate_COMBAT(NPC npc)
        {
            // Start out with no combat priority.
            Priority pri = Priority.None;

            #region NPC is a pet and has been ordered to attack.
            if (npc.PetOwner != null && npc.MostHated != null && npc.CurrentCell == npc.MostHated.CurrentCell)
            {
                return Priority.Attack;
            }
            #endregion

            #region NPC health is below 20%.
            if (npc.Hits < (int)(npc.HitsFull / 5))
            {
                if (!npc.lairCritter && // not a lair critter
                    !npc.IsUndead && // not undead
                    !npc.IsSummoned && // not summoned
                    npc.friendList.Count <= 0 && // no allies nearby
                    (npc.Group == null || npc.Group.GroupNPCList.Count <= 0) // not in a group
                    && npc.enemyList.Count > 0) // enemies present
                {
                    // determine fight or flight
                    if (!FightNotFlight(npc))
                    {
                        pri = Priority.Flee;
                        return pri;
                    }
                }
            }
            #endregion

            #region NPC has Most Hated enemy.
            if (npc.MostHated != null)
            {
                // Most Hated is dead or dying.
                if (npc.MostHated.Hits <= 0) { return pri; }

                // NPC has pets. TODO: Expand on this if pets are already engaging enemy.
                if (npc.Pets.Count > 0)
                {
                    foreach (NPC pet in npc.Pets)
                    {
                        if (pet.MostHated != npc.MostHated) // pets aren't attacking the NPC's enemy, command them to do so
                        {
                            pri = Priority.CommandPets_Attack;
                            return pri;
                        }
                    }
                }

                int distance = Cell.GetCellDistance(npc.X, npc.Y, npc.MostHated.X, npc.MostHated.Y);

                // Deprecated code here. Needs removal or reworking. -Eb 10/7/2015
                if (!EntityLists.ANIMAL.Contains(npc.entity) && ((npc.RightHand != null && npc.RightHand.returning) || (npc.LeftHand != null && npc.LeftHand.returning)))
                {
                    int ldiff = (Rules.GetExpLevel(npc.MostHated.Level) - npc.Level) * 10;
                    int chance = Rules.GetFullAbilityStat(npc, Globals.eAbilityStat.Intelligence) + Rules.GetFullAbilityStat(npc, Globals.eAbilityStat.Wisdom);//mlt for range move vs combat decision
                    if (ldiff >= 50) { chance += 20; }
                    else if (ldiff <= -50) { chance -= 20; }
                    chance = chance + ldiff;
                    if (chance > 80) { chance = 80; }
                    else if (chance < 20) { chance = 20; }

                    if (npc.RightHand != null && npc.RightHand.returning)
                    {
                        if (npc.CurrentCell != npc.MostHated.CurrentCell) { pri = Priority.Attack; }
                        else if (Rules.RollD(1, 100) <= chance) { pri = Priority.RangeMove; }
                        else { pri = Priority.Attack; }
                        return pri;
                    }
                    else if (npc.LeftHand != null && npc.LeftHand.returning)
                    {
                        if (npc.CurrentCell != npc.MostHated.CurrentCell) { pri = Priority.Attack; }

                        else if (Rules.RollD(1, 100) <= chance) { pri = Priority.RangeMove; }
                        else { pri = Priority.Attack; }
                        return pri;
                    }
                }

                switch (distance)
                {
                    case 0:
                        pri = Priority.Attack;
                        // holding a bow that is not nocked at distance 0, move away
                        if (npc.RightHand != null && npc.RightHand.RangePreferred() && !npc.ExcludedPrioritiesList.Contains(Priority.RangeMove) &&
                            npc.RightHand.baseType == Globals.eItemBaseType.Bow && !npc.RightHand.IsNocked) // holding a bow at distance 0
                            pri = Priority.RangeMove;
                        break;
                    case 1:
                        if (npc.RightHand != null && npc.RightHand.RangePreferred())
                        {
                            // target is wielding a polearm, move further away
                            if (npc.MostHated != null && npc.MostHated.RightHand != null
                                && (npc.MostHated.RightHand.skillType == Globals.eSkillType.Polearm || npc.RightHand.baseType == Globals.eItemBaseType.Halberd)
                                && Rules.CheckPerception(npc))
                                pri = Priority.RangeMove;
                            else pri = Priority.Attack;
                        }
                        else if (EntityLists.EntityListContains(EntityLists.LONGARMED, npc.entity)) // longarmed attack
                            pri = Priority.Attack;
                        else if (npc.RightHand != null && (npc.RightHand.baseType == Globals.eItemBaseType.Halberd || npc.RightHand.skillType == Globals.eSkillType.Polearm)) // wielding a halberd, will poke
                            pri = Priority.Attack;
                        else if (npc.species == Globals.eSpecies.Plant) // plants can reach out and attack from 1 cell away
                            pri = Priority.Attack;
                        else if (npc.BaseProfession == Character.ClassType.Martial_Artist && npc.Stamina >= (int)(npc.StaminaMax * .15) && Rules.RollD(1, 10) <= npc.Level) // martial artist
                            pri = Priority.Attack;
                        else if (npc.BaseProfession == ClassType.Thief && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Assassinate) && Talents.AssassinateTalent.MeetsRequirements(npc, npc.MostHated) && Rules.CheckPerception(npc))
                            pri = Priority.Attack;
                        else if (npc.BaseProfession == ClassType.Thief && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Backstab) && npc.RightHand != null && Talents.BackstabTalent.AllowedBackstabItemBaseTypes.Contains(npc.RightHand.baseType) && Rules.CheckPerception(npc))
                            pri = Priority.Attack;
                        else pri = Priority.Advance;
                        break;
                    case 2:
                        if (npc.RightHand != null && npc.RightHand.RangePreferred()) // holding a bow at distance 2
                        {
                            pri = Priority.Attack;
                        }
                        else if (npc.BaseProfession == Character.ClassType.Martial_Artist && npc.Stamina >= (int)(npc.StaminaMax * .15) && Rules.RollD(1, 20) <= npc.Level)
                            pri = Priority.Attack;
                        else if (npc.BaseProfession == ClassType.Thief && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Assassinate) && Talents.AssassinateTalent.MeetsRequirements(npc, npc.MostHated) && Rules.CheckPerception(npc))
                            pri = Priority.Attack;
                        else if (npc.BaseProfession == ClassType.Thief && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Backstab) && npc.RightHand != null && Talents.BackstabTalent.AllowedBackstabItemBaseTypes.Contains(npc.RightHand.baseType) && Rules.CheckPerception(npc))
                            pri = Priority.Attack;
                        else pri = Priority.Advance;
                        break;
                    case 3:
                        if (npc.RightHand != null && npc.RightHand.RangePreferred())
                        {
                            pri = Priority.Attack;
                        }
                        else if (npc.BaseProfession == Character.ClassType.Martial_Artist && npc.GetWeaponSkillLevel(null) >= 9 &&
                            npc.Stamina >= (int)(npc.StaminaMax * .15) && Rules.RollD(1, 30) < npc.Level)// MA with 3rd Dan+ skill
                            pri = Priority.Attack;
                        else pri = Priority.Advance;
                        break;
                    default:
                        break;
                }
            }
            #endregion

            return pri;
        }
        #endregion

        /// <summary>
        /// Rating has been completed. Execute highest rated action.
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="actionType"></param>
        /// <param name="pri"></param>
        public static void ExecuteAction(NPC npc, ActionType actionType, Priority pri)
        {
            if (npc == null) return;

            if (npc.CurrentCell == null)
            {
                Utils.Log(npc.GetLogString() + " removing from world - null CELL", Utils.LogType.SystemFailure);
                npc.RemoveFromWorld();
                return;
            }

            npc.CurrentActionType = actionType;
            npc.CurrentPriority = pri;

            if (npc.Group != null && npc.Group.GroupLeaderID == npc.UniqueID)
            {
                #region Group Related
                try
                {
                    foreach (NPC groupCreature in new List<NPC>(npc.Group.GroupNPCList))
                    {
                        if (npc.Group != null && npc.Group.GroupNPCList != null)
                        {
                            if (groupCreature != null && groupCreature != npc && !groupCreature.IsDead)
                            {
                                groupCreature.MostHated = npc.MostHated;
                                groupCreature.MostFeared = npc.MostFeared;
                                groupCreature.MostLoved = npc.MostLoved;
                                groupCreature.targetList = npc.targetList;
                                groupCreature.friendList = npc.friendList;
                                groupCreature.seenList = npc.seenList;
                                groupCreature.enemyList = npc.enemyList;
                                //groupCreature.hate = npc.hate;
                                groupCreature.HateCenterX = npc.HateCenterX;
                                groupCreature.HateCenterY = npc.HateCenterY;
                                //groupCreature.fear = npc.fear;
                                groupCreature.FearCenterX = npc.FearCenterX;
                                groupCreature.FearCenterY = npc.FearCenterY;
                                groupCreature.TotalFearLove = npc.TotalFearLove;
                                groupCreature.TotalHate = npc.TotalHate;
                                groupCreature.localCells = npc.localCells;
                                groupCreature.PivotItem = npc.PivotItem; // only the leader deals with items

                                try
                                {
                                    // group creatures will cast and do combat against the most hated...
                                    if (groupCreature.MostHated != null && (actionType == ActionType.Cast || actionType == ActionType.Combat))
                                    {
                                        AI.ExecuteAction(groupCreature, actionType, pri);
                                    }
                                }
                                catch
                                {
                                    Utils.Log("Group related issue in AI.ExecuteAction.", Utils.LogType.SystemWarning);
                                    continue;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Utils.Log("Failure at ExecuteAction(" + npc.GetLogString() + ", " + actionType.ToString() + ", " + pri.ToString() + ") Group Related Failure", Utils.LogType.SystemFailure);
                    Utils.Log("Exception Data: " + e.Data + " Source: " + e.Source, Utils.LogType.ExceptionDetail);
                    Utils.LogException(e);
                }
                #endregion
            }

            if (npc.MostHated != null)
                npc.previousMostHated = npc.MostHated;

            #region ActionType assignments
            if (pri == Priority.None) actionType = ActionType.None;

            if (pri == Priority.Recall && actionType != ActionType.Use)
                actionType = ActionType.Use;

            if (pri == Priority.Flee && actionType != ActionType.Move)
                actionType = ActionType.Move;

            //if (pri == Priority.RangeMove && actionType != ActionType.Move)
            //    actionType = ActionType.Move;

            if (pri == Priority.Rest && actionType != ActionType.Special)
                actionType = ActionType.Special;

            if (pri == Priority.Buff && actionType != ActionType.Cast)
                actionType = ActionType.Cast;

            // Commented out on 10/18/2015. Priority.Heal is also used in ActionType.Use to quaff an available balm.
            //if (pri == Priority.Heal && actionType != ActionType.Cast)
            //{
            //    actionType = ActionType.Cast;
            //}

            if (pri == Priority.Attack && actionType != ActionType.Combat)//mlt rweap from left work
                actionType = ActionType.Combat;

            if (pri == Priority.SpellSling && actionType != ActionType.Cast)
                actionType = ActionType.Cast;

            if (pri == Priority.SummonFlagged && actionType != ActionType.Special)
                actionType = ActionType.Special;

            #endregion

            var distance = -1;

            if (actionType != ActionType.Cast)
            {
                //Override to clear a prepped spell.
                npc.preppedSpell = null;
            }

            try
            {
                switch (actionType)
                {
                    case ActionType.None:
                        if (npc.RightHand != null && npc.RightHand.skillType == Globals.eSkillType.Bow && npc.RightHand.name.ToLower().Contains("crossbow") && !npc.RightHand.IsNocked)
                            CommandTasker.ParseCommand(npc, "nock", null);
                        break;
                    case ActionType.Take:
                        /** If pItem is null nothing will be taken. 
                         *  Priority.GetWeapon -- pick up a weapon to use.
                         *  Priority.GetObject -- currently only coins and balms are picked up. Alia in Axe Glacier also picks up the egg for RAxe quest.
                        **/
                        #region Take
                        if (npc.PivotItem == null) // pItem should not be null if this action is being executed...
                            return;

                        if (pri == Priority.GetWeapon) // we're picking up a weapon
                        {
                            if (npc.RightHand != null)
                            {
                                if (npc.SackItem(npc.RightHand))
                                    npc.UnequipRightHand(npc.RightHand);
                                else if (npc.BeltItem(npc.RightHand))
                                    npc.UnequipRightHand(npc.RightHand);
                                else
                                {
                                    AI.AddToIgnoredPivotItems(npc, npc.RightHand);
                                    CommandTasker.ParseCommand(npc, "drop", "right");
                                }
                            }
                            else
                            {
                                CommandTasker.ParseCommand(npc, "take", npc.PivotItem.itemID.ToString());
                                npc.PivotItem = null;
                            }
                        }
                        else if (pri == Priority.GetObject && npc.PivotItem != null)
                        {
                            int count = 0;

                            foreach (Item item in npc.CurrentCell.Items)
                            {
                                if (item.name == npc.PivotItem.name)
                                {
                                    count++;
                                    if (item == npc.PivotItem) break;
                                }
                            }

                            if (count > 0)
                            {
                                if(npc.PivotItem.itemType == Globals.eItemType.Coin)
                                    CommandTasker.ParseCommand(npc, "scoop", "coins");
                                else if (npc.PivotItem.size == Globals.eItemSize.Belt_Or_Sack || npc.PivotItem.size == Globals.eItemSize.Sack_Only)
                                    CommandTasker.ParseCommand(npc, "scoop", "all " + npc.PivotItem.name);
                                else if(npc.PivotItem.size == Globals.eItemSize.Pouch_Only || npc.PivotItem.size == Globals.eItemSize.Sack_Or_Pouch)
                                    CommandTasker.ParseCommand(npc, "pscoop", "all " + npc.PivotItem.name);
                            }

                            npc.PivotItem = null;
                        }

                        #endregion
                        break;
                    case ActionType.Use:
                        /** If pItem is null nothing will be used.
                         *  Priority.Recall -- use pItem as recall.
                         *  Priority.Heal -- quaff command.
                         *  Priority.ManipulateObject -- wield or belt items.
                        **/
                        #region Use
                        if (npc.PivotItem == null) // pItem should not be null if this action is being executed...
                            return;

                        if (pri == Priority.Recall)
                        {
                            // pItem should be recall ring -- in the future check gauntlets worn, items held... but for now make this simple 10/18/2015
                            Item.Recall(npc, npc.PivotItem, (int)Globals.eWearOrientation.None); // recall item disappears
                            Utils.Log(npc.GetLogString() + " used recall magic in " + npc.PivotItem.notes, Utils.LogType.Debug);
                            npc.PivotItem = null;
                            break;
                        }

                        // if Priority.Heal -- use balm or cure staff
                        if (pri == Priority.Heal) // npc has a balm in their sack
                        {
                            // quaffing takes an entire round
                            CommandTasker.ParseCommand(npc, "quaff", "");
                            npc.PivotItem = null;
                            break;
                        }

                        if (pri == Priority.ManipulateObject)
                        {
                            if (npc.PivotItem != null && npc.beltList.Contains(npc.PivotItem)) // item is on belt
                            {
                                CommandTasker.ParseCommand(npc, "wield", npc.PivotItem.UniqueID.ToString());
                            }
                            else if (npc.PivotItem != null && npc.PivotItem == npc.LeftHand)
                            {
                                // belt a shield or weapon, or put it into sack if it fits
                                if (npc.PivotItem.baseType == Globals.eItemBaseType.Shield || npc.PivotItem.itemType == Globals.eItemType.Weapon)
                                {
                                    if (!npc.BeltItem(npc.LeftHand))
                                    {
                                        if (!npc.SackItem(npc.LeftHand))
                                        {
                                            CommandTasker.ParseCommand(npc, "drop", "left");
                                            AI.AddToIgnoredPivotItems(npc, npc.LeftHand);
                                        }
                                        else npc.UnequipLeftHand(npc.LeftHand);
                                    }
                                    else npc.UnequipLeftHand(npc.LeftHand);
                                }
                                else if (npc.SackItem(npc.LeftHand))
                                    npc.UnequipLeftHand(npc.LeftHand);
                                else
                                {
                                    CommandTasker.ParseCommand(npc, "drop", "left");
                                    AI.AddToIgnoredPivotItems(npc, npc.LeftHand);
                                }

                                npc.PivotItem = null;
                            }
                            else if (npc.PivotItem != null)
                            {
                                CommandTasker.ParseCommand(npc, "wield", npc.PivotItem.UniqueID.ToString());
                            }

                            npc.PivotItem = null;
                        }
                        #endregion
                        break;
                    case ActionType.Cast:
                        #region Cast
                        #region Priority.RaiseDead

                        if (pri == Priority.RaiseDead)
                        {
                            // uses default visible distance regardless of visibility, because cries for help etc...
                            var cellArray = Cell.GetApplicableCellArray(npc.CurrentCell,
                                                                           Cell.DEFAULT_VISIBLE_DISTANCE);

                            for (int j = 0; j < cellArray.Length; j++)
                            {
                                if (cellArray[j] == null || !npc.CurrentCell.visCells[j])
                                {
                                    // do nothing
                                }
                                else
                                {
                                    for (int a = 0; a < cellArray[j].Items.Count; a++)
                                    {
                                        Item item = cellArray[j].Items[a];
                                        if (item.itemType == Globals.eItemType.Corpse)
                                        {
                                            PC deadPC = PC.GetOnline(item.special);
                                            if (deadPC != null && deadPC.IsDead)
                                            {
                                                if (npc.CurrentCell != cellArray[j])
                                                {
                                                    npc.AIGotoXYZ(cellArray[j].X, cellArray[j].Y, cellArray[j].Z);
                                                }
                                                else
                                                {
                                                    goto castAction;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    #endregion
                        // casting memorized spell chant
                        else if(pri == Priority.SpellSlingMemorizedChant)
                        {
                            CommandTasker.ParseCommand(npc, "cast", npc.MostHated.UniqueID.ToString());
                            npc.ExcludedPrioritiesList.Clear();
                            return;
                        }

                    castAction:
                        switch (npc.castMode)
                        {
                            case NPC.CastMode.Never:
                                // cannot cast spells (possible use in future to toggle a spellcasters ability)
                                break;
                            case NPC.CastMode.Limited:
                                // spells need to be warmed and then cast, spending mana when cast

                                #region Limited

                                if (npc.preppedSpell == null)
                                {
                                    if (!AI.PrepareSpell(npc, pri)) // cannot prepare a spell (eg: not enough mana)
                                    {
#if DEBUG
                                        npc.SendToAllDEVInSight(npc.GetNameForActionResult() + " failed to prepare a spell in ExecuteAction.");
#endif
                                        goto alternativeOptions;
                                    }
                                    else
                                    {
                                        if (npc.preppedSpell != null && npc.Group != null && npc.Group.GroupMemberWarmedSpells != null)
                                        {
                                                npc.Group.GroupMemberWarmedSpells.Add(Tuple.Create(npc.preppedSpell.ID,
                                                                                               npc.preppedSpell.
                                                                                                   IsBeneficial
                                                                                                   ? npc.BuffTargetID
                                                                                                   : npc.MostHated.
                                                                                                         UniqueID));
                                        }
                                        return;
                                    }
                                }
                                else
                                {
                                    if (npc.BuffTargetID == npc.UniqueID || npc.preppedSpell.TargetType == Globals.eSpellTargetType.Point_Blank_Area_Effect)
                                    {
                                        GameCommand.GameCommandDictionary["cast"].Handler.OnCommand(npc, "");
                                    }
                                    else if(npc.preppedSpell.SpellType == Globals.eSpellType.Conjuration)
                                    {
                                        // thaums, druids, rangers
                                        if(npc.preppedSpell.ID == (int)GameSpell.GameSpellID.Summon_Phantasm)
                                        {
                                            List<int> availablePower = Spells.SummonPhantasmSpell.GetAvailablePhantasms(Skills.GetSkillLevel(npc.magic));
                                            //availablePower.Sort();

                                            int power = availablePower[availablePower.Count - 1];

                                            while (power > 1 && npc.ManaFull < Spells.SummonPhantasmSpell.GetManaRequiredForPower(power))
                                                power--;

                                            GameCommand.GameCommandDictionary["cast"].Handler.OnCommand(npc, power.ToString());
                                        }
                                        else if(npc.preppedSpell.ID == (int)GameSpell.GameSpellID.Create_Snake)
                                        {
                                            List<int> availablePower = new List<int>() { 1, 2, 3, 4, 5 };
                                            int power = availablePower[Rules.Dice.Next(availablePower.Count - 1)];

                                            if (Rules.CheckPerception(npc)) power = availablePower[availablePower.Count - 1];

                                            GameCommand.GameCommandDictionary["cast"].Handler.OnCommand(npc, power.ToString());
                                        }
                                        else // summon natures ally, summon demon, summon hellhound, summon humanoid, summon lamassu, wizard eye, makerecall
                                        {
                                            GameCommand.GameCommandDictionary["cast"].Handler.OnCommand(npc, "");
                                        }
                                    }
                                    else if (npc.preppedSpell.IsBeneficial) // find BuffTargetID
                                    {
                                        #region Beneficial spell.
                                        if (TargetAcquisition.FindTargetInSeenList(npc.seenList, npc.BuffTargetID) != null || npc.BuffTargetID == npc.UniqueID)
                                        {
                                            if (npc.BuffTargetID == npc.UniqueID)
                                                GameCommand.GameCommandDictionary["cast"].Handler.OnCommand(npc, "");
                                            else GameCommand.GameCommandDictionary["cast"].Handler.OnCommand(npc, npc.BuffTargetID.ToString());
                                        }
                                        else
                                        {
                                            goto cancelCast;
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        if (npc.MostHated != null && TargetAcquisition.FindTargetInView(npc, npc.MostHated.UniqueID.ToString(), false, false) != null)
                                        // should this include hidden since mostHated had to have been seen already?
                                        {
                                            List<int> allowedAreaEffectSpells = new List<int>
                                            {
                                                (int)GameSpell.GameSpellID.Concussion,
                                                (int)GameSpell.GameSpellID.Create_Web,
                                                (int)GameSpell.GameSpellID.Disintegrate,
                                                (int)GameSpell.GameSpellID.Dragon__s_Breath,
                                            };

                                            if (npc.FireProtection > 0 || npc.immuneFire)
                                            {
                                                allowedAreaEffectSpells.Add((int)GameSpell.GameSpellID.Bonfire);
                                                allowedAreaEffectSpells.Add((int)GameSpell.GameSpellID.Fireball);
                                                allowedAreaEffectSpells.Add((int)GameSpell.GameSpellID.Firestorm);
                                            }
                                            if (npc.ColdProtection > 0 || npc.immuneCold)
                                            {
                                                allowedAreaEffectSpells.Add((int)GameSpell.GameSpellID.Icestorm);
                                                if(npc.immuneCold)
                                                    allowedAreaEffectSpells.Add((int)GameSpell.GameSpellID.Blizzard);
                                            }
                                            if (npc.AcidProtection > 0 || npc.immuneAcid)
                                            {
                                                allowedAreaEffectSpells.Add((int)GameSpell.GameSpellID.Acid_Rain);
                                            }
                                            if (npc.LightningProtection > 0 || npc.immuneLightning)
                                            {
                                                allowedAreaEffectSpells.Add((int)GameSpell.GameSpellID.Lightning_Bolt);
                                                allowedAreaEffectSpells.Add((int)GameSpell.GameSpellID.Lightning_Storm);
                                            }

                                            if (npc.preppedSpell.TargetType == Globals.eSpellTargetType.Area_Effect)
                                            {
                                                if (!allowedAreaEffectSpells.Contains(npc.preppedSpell.ID) && Cell.GetCellDistance(npc.X, npc.Y, npc.MostHated.X, npc.MostHated.Y) <= 1)
                                                {
                                                    goto cancelCast;
                                                }
                                            }

                                            CommandTasker.ParseCommand(npc, "cast", npc.preppedSpell.Command + " " + npc.MostHated.UniqueID.ToString());
                                        }
                                        else
                                        {
                                            if (TargetAcquisition.FindTargetInSeenList(npc.seenList, npc.BuffTargetID) != null || npc.BuffTargetID == npc.UniqueID)
                                            {
                                                if(npc.BuffTargetID == npc.UniqueID)
                                                    GameCommand.GameCommandDictionary["cast"].Handler.OnCommand(npc, "");
                                                else GameCommand.GameCommandDictionary["cast"].Handler.OnCommand(npc, npc.BuffTargetID.ToString());
                                            }
                                            else
                                            {
                                                goto cancelCast;
                                            }
                                        }
                                    }
                                }
                                break;

                            #endregion

                            case NPC.CastMode.Unlimited:
                                // no prep is needed and no mana is used (eg: NPC.AIType.Enforcer aka Axe Glacier lawful statues, salamander, dragons, ice lizards)

                                #region Unlimited

                                // If unable to prepare a spell, log NPC and priority then break out of here.
                                if (!AI.PrepareSpell(npc, pri))
                                {
                                    goto alternativeOptions;
                                }
                                else
                                {
                                    if (npc.preppedSpell != null && npc.Group != null)
                                    {
                                        npc.Group.GroupMemberWarmedSpells.Add(Tuple.Create(npc.preppedSpell.ID,
                                                                                           npc.preppedSpell.IsBeneficial
                                                                                               ? npc.BuffTargetID
                                                                                               : npc.MostHated.UniqueID));
                                    }
                                }

                                if (npc.preppedSpell.TargetType == Globals.eSpellTargetType.Point_Blank_Area_Effect)
                                {
                                    CommandTasker.ParseCommand(npc, "impcast", npc.preppedSpell.Command);
                                }
                                else if (npc.preppedSpell.IsBeneficial)
                                {
                                    if (TargetAcquisition.FindTargetInView(npc, npc.BuffTargetID, true, true) != null)
                                    {
                                        CommandTasker.ParseCommand(npc, "impcast",
                                                                   npc.preppedSpell.Command + " " + npc.BuffTargetID);
                                    }
                                    else
                                    {
                                        npc.preppedSpell = null;
                                    }
                                }
                                else
                                {
                                    if (TargetAcquisition.FindTargetInView(npc, npc.MostHated.Name, false, false) !=
                                        null)
                                    {
                                        string args = npc.preppedSpell.Command + " " + npc.MostHated.Name;

                                        if (npc.species == Globals.eSpecies.CloudDragon && Rules.RollD(0, 100) >= 75)
                                            args = npc.preppedSpell.Command + " stormbreath " + npc.MostHated.Name;

                                        CommandTasker.ParseCommand(npc, "impcast", args);
                                    }
                                    else
                                    {
                                        npc.preppedSpell = null;
                                    }
                                }
                                break;

                            #endregion

                            case NPC.CastMode.NoPrep:
                                // no prep time is needed, but mana is spent when spell is cast (eg: knights)

                                #region NoPrep

                                if (!AI.PrepareSpell(npc, pri))
                                {
                                    goto alternativeOptions;
                                }
                                else
                                {
                                    if (npc.preppedSpell != null && npc.Group != null)
                                    {
                                        npc.Group.GroupMemberWarmedSpells.Add(Tuple.Create(npc.preppedSpell.ID,
                                                                                           npc.preppedSpell.IsBeneficial
                                                                                               ? npc.BuffTargetID
                                                                                               : npc.MostHated.UniqueID));
                                    }
                                }

                                if (npc.preppedSpell.TargetType == Globals.eSpellTargetType.Point_Blank_Area_Effect ||
                                    npc.BuffTargetID == npc.UniqueID)
                                {
                                    CommandTasker.ParseCommand(npc, "cast", "");
                                }
                                else if (npc.preppedSpell.IsBeneficial)
                                {
                                    CommandTasker.ParseCommand(npc, "cast", npc.BuffTargetID.ToString());
                                }
                                else
                                {
                                    CommandTasker.ParseCommand(npc, "cast", npc.MostHated.UniqueID.ToString());
                                }
                                break;

                            #endregion

                            default:
                                break;
                        }
                        break;
                    cancelCast:
                        if (npc.MostHated == null)
                        {
                            ExecuteAction(npc, ActionType.Special, Priority.Rest);
                            break;
                        }
                        else
                        {
                            if (npc.preppedSpell != null)
                            {
                                npc.preppedSpell = null;
                                npc.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                            }
                            // continue to alternativeOptions
                        }

                    #region alternativeOptions: could not prepare a spell even though it was our priority

                    alternativeOptions:
                        if (npc.MostHated != null)
                        {
                            distance = Cell.GetCellDistance(npc.X, npc.Y, npc.MostHated.X, npc.MostHated.Y);
                        }

                        if (pri >= Priority.PrepareSpell && pri < Priority.Buff)
                        {
                            if (npc.MostHated != null)
                            {
                                if (distance > 0)
                                {
                                    ExecuteAction(npc, ActionType.Move, Priority.Advance);
                                }
                                else
                                {
                                    ExecuteAction(npc, ActionType.Combat, Priority.Attack);
                                }
                            }
                        }
                        else if (pri == Priority.Buff)
                        {
                            ExecuteAction(npc, ActionType.Special, Priority.Rest);
                        }
                        else if (pri == Priority.Heal)
                        {
                            // 4/23/2014 Do not believe this code is currently reachable.
                            if (npc.BuffTargetID == npc.UniqueID) // flee if we were trying to heal and could not
                            {
                                if (Rules.CheckPerception(npc)) ExecuteAction(npc, ActionType.Move, Priority.RangeMove);
                                else
                                {
                                    if (npc.MostHated != null)
                                    {
                                        if (distance > 0) ExecuteAction(npc, ActionType.Move, Priority.Advance);
                                        else ExecuteAction(npc, ActionType.Combat, Priority.Attack);
                                    }
                                    else ExecuteAction(npc, ActionType.Special, Priority.Rest);
                                }
                            }
                            else
                            {
                                if (npc.MostHated != null)
                                {
                                    if (distance > 0)
                                        ExecuteAction(npc, ActionType.Move, Priority.Advance);
                                    else ExecuteAction(npc, ActionType.Combat, Priority.Attack);
                                }
                                else
                                {
                                    if (TargetAcquisition.FindTargetInSeenList(npc.seenList, npc.BuffTargetID) != null)
                                        CommandTasker.ParseCommand(npc, "cast", npc.BuffTargetID.ToString());
                                    else ExecuteAction(npc, ActionType.Special, Priority.Rest);
                                }
                            }
                        }

                        #endregion

                        #endregion
                        break;
                    case ActionType.Special:
                        #region Special

                        #region Priority.Rest
                        if (pri == Priority.Rest)
                        {
                            #region Rest
                            // lair creatures at their home point gain 5% of hits/stam/mana
                            if (npc.lairCritter && npc.X == npc.lairXCord && npc.Y == npc.lairYCord)
                            {
                                double pct = .05;
                                // lair creature heals less if diseased
                                if (npc.EffectsList.ContainsKey(Effect.EffectTypes.Contagion)) pct = .02;

                                if (npc.Hits < npc.HitsFull) // increase stats
                                {
                                    npc.Hits += (int)(npc.HitsFull * pct);
                                    if (npc.Hits > npc.HitsFull)
                                        npc.Hits = npc.HitsFull;
                                }

                                if (npc.Stamina < npc.StaminaFull)
                                {
                                    npc.Stamina += (int)(npc.StaminaFull * pct);
                                    if (npc.Stamina > npc.StaminaFull)
                                        npc.Stamina = npc.StaminaFull;
                                }

                                if (npc.Mana < npc.ManaFull)
                                {
                                    npc.Mana += (int)(npc.ManaFull * pct);
                                    if (npc.Mana > npc.ManaFull)
                                        npc.Mana = npc.ManaFull;
                                }
                            }

                            if (!CommandTasker.ParseCommand(npc, "rest", null))
                                ExecuteAction(npc, ActionType.Move, Priority.Wander);

                            break;
                            #endregion
                        }
                        else if (pri == Priority.Meditate)
                        {
                            #region Meditate
                            if (npc.lairCritter &&
                                                    npc.X == npc.lairXCord && npc.Y == npc.lairYCord)
                            {
                                double pct = .05;
                                // lair creature heals less if diseased
                                if (npc.EffectsList.ContainsKey(Effect.EffectTypes.Contagion)) pct = .02;

                                if (npc.Hits < npc.HitsFull) // increase stats
                                {
                                    npc.Hits += (int)(npc.HitsFull * pct);
                                    if (npc.Hits > npc.HitsFull)
                                        npc.Hits = npc.HitsFull;
                                }

                                if (npc.Stamina < npc.StaminaFull)
                                {
                                    npc.Stamina += (int)(npc.StaminaFull * pct);
                                    if (npc.Stamina > npc.StaminaFull)
                                        npc.Stamina = npc.StaminaFull;
                                }

                                if (npc.Mana < npc.ManaFull)
                                {
                                    npc.Mana += (int)(npc.ManaFull * pct);
                                    if (npc.Mana > npc.ManaFull)
                                        npc.Mana = npc.ManaFull;
                                }
                            }

                            if(!CommandTasker.ParseCommand(npc, "meditate", null))
                                ExecuteAction(npc, ActionType.Move, Priority.Wander);
                            break;
                            #endregion
                        }
                        #endregion

                        if (pri == Priority.GoHome)
                        {
                            if (npc.lairCritter)
                            {
                                npc.AIGotoXYZ(npc.lairXCord, npc.lairYCord, npc.lairZCord);
                                break;
                            }
                            else
                            {
                                // warp the npc back home
                                npc.CurrentCell = Cell.GetCell(npc.FacetID, npc.LandID, npc.MapID, npc.spawnXCord,
                                                               npc.spawnYCord, npc.spawnZCord);

                                // for quest npcs that remain immortal until following / active with a player character
                                if (npc.WasImmortal)
                                {
                                    npc.WasImmortal = false;
                                    npc.IsImmortal = true;
                                }
                                break;
                            }
                        }

                        if (pri == Priority.LairDefense)
                        {
                            string spellCommand = " ";
                            switch (npc.species)
                            {
                                case Globals.eSpecies.Pheonix:
                                    spellCommand = "fireball";
                                    break;
                                case Globals.eSpecies.WindDragon:
                                case Globals.eSpecies.FireDragon:
                                case Globals.eSpecies.IceDragon:
                                    spellCommand = "drbreath";
                                    break;
                                case Globals.eSpecies.LightningDrake:
                                    spellCommand = "lightning";
                                    break;
                                case Globals.eSpecies.TundraYeti:
                                    spellCommand = "blizzard";
                                    break;
                            }

                            #region Special Lair Critter Defense

                            switch (npc.species)
                            {
                                case Globals.eSpecies.FireDragon:
                                case Globals.eSpecies.IceDragon:
                                case Globals.eSpecies.LightningDrake:
                                case Globals.eSpecies.TundraYeti:
                                case Globals.eSpecies.Pheonix:
                                case Globals.eSpecies.WindDragon:
                                    if (Rules.Dice.Next(npc.HitsFull) >= npc.Hits)
                                    {
                                        switch (Rules.Dice.Next(8))
                                        {
                                            case 0:
                                            case 1:
                                                npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                                     " snorts and then unleashes an ear piercing roar!");
                                                AI.DragonFear(npc);
                                                break;
                                            case 2:
                                            case 3:
                                                if (npc.species == Globals.eSpecies.FireDragon)
                                                {
                                                    npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                                         " hisses and puffs smoke from " +
                                                                         Character.POSSESSIVE[(int)npc.gender].
                                                                             ToLower() +
                                                                         " nostrils as it looks about " +
                                                                         Character.POSSESSIVE[(int)npc.gender].
                                                                             ToLower() + " lair.");
                                                }
                                                else
                                                {
                                                    npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                                         " growls and scans around " +
                                                                         Character.POSSESSIVE[(int)npc.gender].
                                                                             ToLower() + " lair for intruders.");
                                                }
                                                break;

                                            case 4:
                                            case 5:
                                                npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                                     " emits a guttural growl as " +
                                                                     Character.PRONOUN[(int)npc.gender].ToLower() +
                                                                     " moves about " +
                                                                     Character.POSSESSIVE[(int)npc.gender].ToLower() +
                                                                     " lair.");
                                                AI.DragonFear(npc);
                                                if (!Map.IsNextToWall(npc))
                                                {
                                                    npc.DoAIMove();
                                                    if (!Map.IsNextToWall(npc))
                                                    {
                                                        npc.DoAIMove();
                                                    }
                                                }
                                                break;
                                            case 6:
                                            case 7:
                                                string direction = "";
                                                do
                                                {
                                                    switch (Rules.RollD(1, 8))
                                                    {
                                                        case 1:
                                                            direction = "n";
                                                            break;
                                                        case 2:
                                                            direction = "s";
                                                            break;
                                                        case 3:
                                                            direction = "e";
                                                            break;
                                                        case 4:
                                                            direction = "w";
                                                            break;
                                                        case 5:
                                                            direction = "ne";
                                                            break;
                                                        case 6:
                                                            direction = "nw";
                                                            break;
                                                        case 7:
                                                            direction = "se";
                                                            break;
                                                        case 8:
                                                            direction = "sw";
                                                            break;
                                                    }
                                                } while (
                                                    Map.IsSpellPathBlocked(Map.GetCellRelevantToCell(
                                                        npc.CurrentCell, direction, false)));
                                                CommandTasker.ParseCommand(npc, "impcast",
                                                                           spellCommand + " " + direction);
                                                break;
                                            default:
                                                npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                                     " roars fiercely and scrapes the floor of " +
                                                                     Character.POSSESSIVE[(int)npc.gender].ToLower() +
                                                                     " lair with " +
                                                                     Character.POSSESSIVE[(int)npc.gender].ToLower() +
                                                                     " massive claws.");
                                                AI.DragonFear(npc);
                                                break;
                                        }
                                    }
                                    break;
                                default:
                                    break;

                            }

                            #region Giant kin defenses.
                            if (EntityLists.IsGiantKin(npc) && Rules.Dice.Next(npc.HitsFull) >= npc.Hits)
                            {
                                switch (Rules.Dice.Next(9))
                                {
                                    case 0:
                                    case 1:
                                        npc.SendToAllInSight(npc.GetNameForActionResult() + " stomps " +
                                                             Character.POSSESSIVE[(int)npc.gender].ToLower() +
                                                             " massive foot!");
                                        AI.GiantStomp(npc);
                                        break;
                                    case 2:
                                    case 3:
                                        npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                             " sniffs the air and squints " +
                                                             Character.POSSESSIVE[(int)npc.gender].ToLower() +
                                                             " eyes.");
                                        break;
                                    case 4:
                                    case 5:
                                        if (npc.RightHand == null)
                                        {
                                            npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                                 " growls and then slams " +
                                                                 Character.POSSESSIVE[(int)npc.gender].
                                                                     ToLower() + " " +
                                                                 (npc.RightHand == null
                                                                      ? "fist"
                                                                      : npc.RightHand.name) +
                                                                 " into the ground!");
                                        }
                                        else
                                        {
                                            npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                                 " roars loudly and slams " +
                                                                 Character.POSSESSIVE[(int)npc.gender].
                                                                     ToLower() + " " +
                                                                 (npc.RightHand == null
                                                                      ? "fist"
                                                                      : npc.RightHand.name) +
                                                                 " into the ground!");
                                        }
                                        AI.GiantStomp(npc);
                                        break;
                                    case 6:
                                    case 7:
                                        npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                             " growls and begins to look around " +
                                                             Character.POSSESSIVE[(int)npc.gender].ToLower() +
                                                             " lair.");
                                        if (!Map.IsNextToWall(npc))
                                        {
                                            NPC.AIRandomlyMoveCharacter(npc);
                                        }
                                        break;
                                    default:
                                        npc.SendToAllInSight(npc.GetNameForActionResult() +
                                                             " speaks, 'Me iz gonna find da little ting dat pokes me an' hidez like orcy.'");
                                        if (!Map.IsNextToWall(npc))
                                        {
                                            npc.DoAIMove();
                                            if (!Map.IsNextToWall(npc))
                                                npc.DoAIMove();
                                        }
                                        break;
                                }

                            }
                            #endregion
                            #endregion
                        }

                        //if(pri == Priority.SummonFlagged)
                        //{
                        //    CommandTasker.ParseCommand(npc, "impsummon", npc.TargetID.ToString());
                        //}

                        #endregion
                        break;
                    case ActionType.Move:
                        #region Move

                        if (!string.IsNullOrEmpty(npc.idleSound) && npc.MostHated == null&& Rules.RollD(1, 100) <= 10)
                        {
                            if (!npc.IsHidden || (npc.IsHidden && npc.EffectsList[Effect.EffectTypes.Hide_in_Shadows].IsPermanent))
                            {
                                npc.EmitSound(npc.idleSound);

                                if (npc.MoveString != "")
                                    npc.SendShout(npc.MoveString);
                            }
                        }

                        if (!npc.IsMobile)
                        {
                            return;
                        } // return if this creature is not mobile

                        // no matter what the priority, if the NPC is wielding a crossbow then nock it
                        if (npc.RightHand != null && npc.RightHand.skillType == Globals.eSkillType.Bow && npc.RightHand.name.ToLower().Contains("crossbow") && !npc.RightHand.IsNocked)
                            CommandTasker.ParseCommand(npc, "nock", null);

                        #region Get distance to mostHated

                        if (npc.MostHated != null)
                        {
                            distance = Cell.GetCellDistance(npc.X, npc.Y, npc.MostHated.X, npc.MostHated.Y);
                        }

                        #endregion

                        #region FleeEffect

                        if (pri == Priority.FleeEffect)
                        {
                            npc.InitiativeModifier -= 5;

                            var cList = Map.GetAdjacentCells(npc.CurrentCell, npc);
                            if (cList != null)
                            {
                                var rand = Rules.Dice.Next(cList.Count);
                                var nCell = (Cell)cList[rand];
                                if (!npc.IsPC)
                                {
                                    if (npc.Group != null)
                                    {
                                        npc.Group.Remove(npc);
                                    }
                                }
                                npc.AIGotoXYZ(nCell.X, nCell.Y, nCell.Z);
                            }
                            return;
                        }
                        #endregion

                        #region Priority.Flee

                        else if (pri == Priority.Flee)
                        {
                            npc.InitiativeModifier -= 5;

                            if (npc.MostHated != null)
                            {
                                if (!npc.IsPC)
                                {
                                    if (npc.Group != null)
                                    {
                                        npc.Group.Remove(npc);
                                    }
                                }
                                AI.BackAwayFromCell(npc, npc.MostHated.CurrentCell);
                            }
                            else
                            {
                                AI.BackAwayFromCell(npc, npc.CurrentCell);
                            }
                            return;
                        }
                        #endregion

                        #region Priority.RangeMove

                        else if (pri == Priority.RangeMove)
                        {
                            if (npc.MostHated != null)
                            {
                                switch (distance)
                                {
                                    case 0:
                                        // currently only crossbows stay nocked when moving, so if it is another type of bow just shoot
                                        if (npc.RightHand != null &&
                                                npc.RightHand.skillType == Globals.eSkillType.Bow && !npc.RightHand.name.Contains("crossbow") && npc.RightHand.IsNocked)
                                        {
                                            CommandTasker.ParseCommand(npc, "shoot", npc.MostHated.Name);
                                            break;
                                        }
                                        // This code belongs in the rate method for determining Priority.RangeMove. But for now, it works.
                                        List<Cell> cellOptions = new List<Cell>();

                                        foreach (Cell cell in npc.localCells)
                                        {
                                            if (
                                                Cell.GetCellDistance(npc.MostHated.X, npc.MostHated.Y, cell.X, cell.Y) >=
                                                2)
                                            {
                                                cellOptions.Add(cell);
                                                npc.AIGotoXYZ(cell.X, cell.Y, cell.Z);
                                                return;
                                            }
                                        }

                                        if (cellOptions.Count > 0)
                                        {
                                            Cell cellChoice = cellOptions[Rules.Dice.Next(cellOptions.Count) - 1];
                                            npc.AIGotoXYZ(cellChoice.X, cellChoice.Y, cellChoice.Z);
                                            return;
                                        }
                                        else
                                        {
                                            if (!npc.ExcludedPrioritiesList.Contains(Priority.RangeMove))
                                            {
                                                npc.ExcludedPrioritiesList.Add(Priority.RangeMove);
                                                Rate(npc);
                                                return;
                                            }
                                        }
                                        break;
                                    default:
                                        if (npc.RightHand != null &&
                                                npc.RightHand.skillType == Globals.eSkillType.Bow)
                                        {
                                            if (!npc.RightHand.IsNocked)
                                                CommandTasker.ParseCommand(npc, "nock", null);
                                            else CommandTasker.ParseCommand(npc, "shoot", npc.MostHated.Name);
                                            break;
                                        }
                                        else if (npc.RightHand.RangePreferred())
                                        {
                                            CommandTasker.ParseCommand(npc, "shoot", npc.MostHated.Name);
                                        }
                                        // Returns false if unable to move.
                                        //if (!AI.BackAwayFromCell(npc, npc.mostHated.CurrentCell))
                                        //{
                                        //    if (npc.RightHand != null &&
                                        //        npc.RightHand.skillType == Globals.eSkillType.Bow)
                                        //    {
                                        //        if (!npc.RightHand.IsNocked)
                                        //        {
                                        //            CommandTasker.ParseCommand(npc, "nock", null);
                                        //        }
                                        //        else
                                        //        {
                                        //            CommandTasker.ParseCommand(npc, "shoot", npc.mostHated.Name);
                                        //        }
                                        //        break;
                                        //    }
                                        //    else
                                        //    {
                                        //        npc.DoAIMove();
                                        //        return;
                                        //    }
                                        //}
                                        break;
                                }
                                return;
                            }
                            npc.DoAIMove();
                            return;
                        }
                        #endregion

                        #region Priority.Advance

                        else if (pri == Priority.Advance)
                        {
                            if (npc.MostHated != null)
                            {
                                npc.AIGotoXYZ(npc.MostHated.X, npc.MostHated.Y, npc.MostHated.Z);

                                if (npc.Speed <= 3)
                                {
                                    if (distance == 1)
                                    {
                                        // mostHated is 1 cell away and NPC is wielding a halberd, attack with poke command 
                                        if ((npc.RightHand != null &&
                                             npc.RightHand.baseType == Globals.eItemBaseType.Halberd) ||
                                            npc.species == Globals.eSpecies.Plant)
                                        {
                                            AI.ExecuteAction(npc, AI.ActionType.Combat, AI.Priority.Attack);
                                        }
                                    }
                                }
                                else
                                {
                                    #region Faster moving NPCs

                                    switch (npc.Speed)
                                    {
                                        case 4:
                                            if (Rules.RollD(1, 100) <= 25) // 25 percent chance to move and attack
                                            {
                                                AI.ExecuteAction(npc, AI.ActionType.Combat, AI.Priority.Attack);
                                            }
                                            break;
                                        case 5:
                                            if (Rules.RollD(1, 100) <= 50) // 50 percent chance to move and attack
                                            {
                                                AI.ExecuteAction(npc, AI.ActionType.Combat, AI.Priority.Attack);
                                            }
                                            break;
                                        case 6:
                                            if (Rules.RollD(1, 100) <= 75)
                                            {
                                                AI.ExecuteAction(npc, AI.ActionType.Combat, AI.Priority.Attack);
                                            }
                                            break;
                                        default:
                                            AI.ExecuteAction(npc, AI.ActionType.Combat, AI.Priority.Attack);
                                            break;
                                    }

                                    #endregion
                                }
                            }
                            else if (npc.previousMostHated != null && Rules.CheckPerception(npc))
                            //mlt follow up or down add in here,should be a chance
                            {
                                npc.AIGotoXYZ(npc.previousMostHated.X, npc.previousMostHated.Y,
                                              npc.previousMostHated.Z);
                            }
                            return;
                        }
                        #endregion

                        #region Priority.InvestigateMagic

                        else if (pri == Priority.InvestigateMagic)
                        {
                            string[] xyz = npc.gotoWarmedMagic.Split("|".ToCharArray());
                            int rx = Convert.ToInt32(xyz[0]);
                            int ry = Convert.ToInt32(xyz[1]);
                            int rz = Convert.ToInt32(xyz[2]);
                            npc.AIGotoXYZ(rx, ry, rz);
                        }
                        #endregion

                        #region Priority.Investigate

                        else if (pri == Priority.Investigate && npc.previousMostHated != null)
                        {
                            npc.AIGotoXYZ(npc.previousMostHated.X, npc.previousMostHated.Y, npc.previousMostHated.Z);
                            //npc.previousMostHated = null;
                            //TODO: if current cell is previous MostHated's, then nullify previousMostHated
                            return;
                        }

                        #endregion

                        // below is usually Priority.Wander

                        if (npc.TotalFearLove == 0 && npc.TotalHate == 0) // low priority move
                        {
                            #region Creature with a patrol route

                            if (npc.HasPatrol && npc.patrolKeys != null)
                            {
                                if (npc.MoveList.Count > 0)
                                {
                                    npc.patrolWaitRoundsRemaining = 0;

                                    npc.DoNextListMove();
                                }
                                else if (npc.patrolWaitRoundsRemaining > 0)
                                {
                                    npc.patrolWaitRoundsRemaining--;
                                }
                                else
                                {
                                    npc.patrolWaitRoundsRemaining = 0;

                                    string[] xyz = npc.patrolKeys[npc.patrolCount].Split("|".ToCharArray());

                                    if (xyz[0].ToLower() == "w")
                                    {
                                        // Random wait period or not.
                                        if (xyz[1].ToLower() == "x")
                                        {
                                            npc.patrolWaitRoundsRemaining = Rules.RollD(5, 6);
                                        }
                                        else npc.patrolWaitRoundsRemaining = Convert.ToInt32(xyz[1]);
                                    }
                                    else
                                    {

                                        int rx = Convert.ToInt32(xyz[0]);
                                        int ry = Convert.ToInt32(xyz[1]);
                                        int rz = Convert.ToInt32(xyz[2]);

                                        if (rz != npc.Z) // switching z coordinates
                                        {
                                            npc.AIGotoNewZ(rx, ry, rz);
                                        }
                                        else
                                        {
                                            npc.AIGotoXYZ(rx, ry, rz);
                                        }

                                        npc.patrolCount++;

                                        if (npc.patrolCount > npc.patrolKeys.Count - 1)
                                        {
                                            npc.patrolCount = 0;
                                        }
                                    }
                                }
                            }
                            #endregion

                            else npc.DoAIMove();
                        }
                        else if (npc.TotalHate == 0 && npc.TotalFearLove > 0) // move toward friend FLCenter - normal
                        {
                            #region Creature with a patrol route

                            if (npc.HasPatrol && npc.patrolKeys != null) // if the creature has a patrol route
                            {
                                if (npc.MoveList.Count > 0)
                                {
                                    npc.patrolWaitRoundsRemaining = 0;
                                    npc.DoNextListMove();
                                }
                                else if (npc.patrolWaitRoundsRemaining > 0)
                                {
                                    npc.patrolWaitRoundsRemaining--;
                                }
                                else
                                {
                                    npc.patrolWaitRoundsRemaining = 0;

                                    string[] xyz = npc.patrolKeys[npc.patrolCount].Split("|".ToCharArray());

                                    // npc is being instructed to wait xyz[1] rounds
                                    if (xyz[0].ToLower() == "w")
                                    {
                                        //Utils.Log(npc.GetLogString() + " is now waiting on patrol. " + npc.patrolWaitRoundsRemaining.ToString() + " rounds remaining.", Utils.LogType.SystemWarning);

                                        // Random wait period or not.
                                        if (xyz[1].ToLower() == "x")
                                        {
                                            npc.patrolWaitRoundsRemaining = Rules.RollD(5, 6);
                                        }
                                        else npc.patrolWaitRoundsRemaining = Convert.ToInt32(xyz[1]);
                                    }
                                    else
                                    {
                                        int rx = Convert.ToInt32(xyz[0]);
                                        int ry = Convert.ToInt32(xyz[1]);
                                        int rz = Convert.ToInt32(xyz[2]);

                                        if (rz != npc.Z)
                                        {
                                            npc.AIGotoNewZ(rx, ry, rz);
                                        }
                                        else
                                        {
                                            npc.AIGotoXYZ(rx, ry, rz);
                                        }

                                        if (npc.X == rx && npc.Y == ry && npc.Z == rz)
                                        {
                                            npc.patrolCount++;
                                        }

                                        if (npc.patrolCount > npc.patrolKeys.Count - 1)
                                        {
                                            npc.patrolCount = 0;
                                        }
                                    }
                                }
                            }
                            #endregion

                            else
                            {
                                npc.DoAIMove();
                            }
                        }
                        else if (npc.TotalFearLove < 0 && Math.Abs(npc.TotalFearLove) > npc.TotalHate)
                        // run away - too dangerous - highest
                        {
                            AI.BackAwayFromCell(npc, npc.MostFeared.CurrentCell);
                        }
                        else // move toward center of hate - high
                        {
                            npc.AIGotoXYZ(npc.HateCenterX, npc.HateCenterY, npc.Z);
                        }

                        #endregion
                        break;
                    case ActionType.Combat:
                        #region Combat
                        try
                        {
                            // mostHated may have been killed by a group member
                            if (npc.MostHated == null || npc.MostHated.IsDead) break;

                            // Target is not in view. Break out.
                            if (TargetAcquisition.FindTargetInView(npc as Character, npc.MostHated.Name, false, false) == null) break;

                            // Priority to command pets to attack.
                            if (pri == Priority.CommandPets_Attack)
                            {
                                CommandTasker.ParseCommand(npc, "all,", "attack " + npc.MostHated.UniqueID);

                                return;
                            }

                            #region Get distance to mostHated

                            distance = Cell.GetCellDistance(npc.X, npc.Y, npc.MostHated.X, npc.MostHated.Y);

                            #endregion

                            // wielding a polearm (halberd) type weapon
                            if (npc.RightHand != null && npc.RightHand.baseType == Globals.eItemBaseType.Halberd)
                            {
                                CommandTasker.ParseCommand(npc, "poke", npc.MostHated.Name);
                            }
                            else if (npc.RightHand != null && npc.RightHand.skillType == Globals.eSkillType.Bow)
                            // wielding a bow
                            {
                                if (npc.RightHand.IsNocked)
                                    CommandTasker.ParseCommand(npc, "shoot", npc.MostHated.Name);
                                else
                                    CommandTasker.ParseCommand(npc, "nock", null);
                            }
                            else if (npc.BaseProfession == Character.ClassType.Martial_Artist)
                            {
                                #region Martial Artist
                                switch (distance)
                                {
                                    case 0:
                                        if (npc.Stamina >= (int)(npc.StaminaFull * .25) && Rules.Dice.Next(100) >= 50)
                                        // 2 in 4 chance of kicking 
                                        // *note change this in the future to consider if the creature is wearing damage boots or gauntlets
                                        {
                                            // check for rapidkicks or (add legsweep check in the future when AI is fighting against groups in same cell)
                                            if (TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.RapidKicks) &&
                                                 Rules.Dice.Next(100) <= 10 + npc.Level)
                                            {
                                                CommandTasker.ParseCommand(npc, "rapidkicks", npc.MostHated.Name);
                                            }
                                            else if (TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.LegSweep) &&
                                                Rules.Dice.Next(100) <= 10 + npc.Level)
                                            {
                                                CommandTasker.ParseCommand(npc, "legsweep", npc.MostHated.Name);
                                            }
                                            else CommandTasker.ParseCommand(npc, "kick", npc.MostHated.Name);
                                        }
                                        else
                                        {
                                            CommandTasker.ParseCommand(npc, "kill", npc.MostHated.Name);
                                        }
                                        break;
                                    case 1:
                                    case 2:
                                    case 3:
                                        // random name means this is a humanoid NPC so they're gonna yell a ki yah
                                        // now determine talents use based on how much stamina available

                                        string fightingWords = "";
                                        if (EntityLists.HUMAN.Contains(npc.entity) || npc.HasRandomName)
                                            fightingWords = "/Ki yah!/!";

                                        if (TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.FlyingFury))
                                            CommandTasker.ParseCommand(npc, "flyingfury", npc.MostHated.Name + fightingWords);
                                        else
                                            CommandTasker.ParseCommand(npc, "jumpkick", npc.MostHated.Name + fightingWords);
                                        break;
                                }
                                #endregion
                            }
                            else if (npc.RightHand != null && npc.RightHand.returning && distance > 0) // right hand returning weapon
                            {
                                #region Right hand returning weapon
                                if (npc.CurrentCell != npc.MostHated.CurrentCell)
                                {
                                    CommandTasker.ParseCommand(npc, "throw",
                                                               npc.RightHand.name + " at " + npc.MostHated.UniqueID);

                                    if (npc.LeftHand != null && npc.LeftHand.returning)
                                        CommandTasker.ParseCommand(npc, "throw", "left at " + npc.MostHated.UniqueID);
                                }
                                else
                                {
                                    CommandTasker.ParseCommand(npc, "kill", npc.MostHated.UniqueID.ToString());
                                }

                                #endregion
                            }
                            else if (npc.LeftHand != null && npc.LeftHand.returning && distance > 0) // left hand returning weapon
                            {
                                #region Left hand returning weapon
                                if (npc.CurrentCell != npc.MostHated.CurrentCell)
                                {
                                    CommandTasker.ParseCommand(npc, "throw", "left at " + npc.MostHated.UniqueID);
                                }
                                else
                                {
                                    CommandTasker.ParseCommand(npc, "kill", npc.MostHated.UniqueID.ToString());
                                }
                                #endregion
                            }
                            else
                            {
                                // Not wielding a two handed weapon, no shield. Check belt for shield and wield it.
                                if (npc.RightHand != null && npc.LeftHand == null &&
                                    !npc.RightHand.TwoHandedPreferred())
                                {
                                    foreach (Item shieldItem in npc.beltList)
                                    {
                                        if (shieldItem.baseType == Globals.eItemBaseType.Shield)
                                        {
                                            CommandTasker.ParseCommand(npc, "wield", "shield");
                                            break;
                                        }
                                    }
                                }

                                // If NPC has dual wield and a weapon on their belt, wield it now before attacking.
                                if (npc.RightHand != null && npc.LeftHand == null && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.DualWield) &&
                                    !npc.RightHand.TwoHandedPreferred())
                                {
                                    foreach (Item beltItem in npc.beltList)
                                    {
                                        if (beltItem.itemType == Globals.eItemType.Weapon)
                                        {
                                            CommandTasker.ParseCommand(npc, "wield", beltItem.name);
                                            break;
                                        }
                                    }
                                }


                                // determine talent usage here
                                //if(npc.BaseProfession == ClassType.Thief && npc.IsHidden && distance <= 2 && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Backstab))
                                //{
                                //    CommandTasker.ParseCommand(npc, "backstab", npc.mostHated.Name);
                                //}
                                //else

                                // Talents use here...
                                //List<string> commandsList = new List<string>() { "kill" };
                                string command = "kill";

                                if (distance > 0 && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Charge) && Talents.BattleChargeTalent.MeetsRequirements(npc, npc.MostHated))
                                {
                                    command = "charge";
                                }
                                else if (distance == 0 && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Bash) && Talents.ShieldBashTalent.MeetsRequirements(npc, npc.MostHated) && npc.LeftHand != null && npc.LeftHand.baseType == Globals.eItemBaseType.Shield)
                                {
                                    command = "bash";
                                }
                                else if (distance == 0 && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Cleave) && Talents.CleaveTalent.MeetsRequirements(npc, npc.MostHated))
                                {
                                    command = "cleave";
                                }
                                else if (distance > 0 && distance < 3 && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Assassinate) && npc.RightHand != null && Talents.AssassinateTalent.MeetsRequirements(npc, npc.MostHated))
                                {
                                    //TODO finish MeetsRequirements in GameTalent.Assassinate
                                    command = "assassinate";
                                }
                                else if (distance > 0 && distance < 3 && TalentAvailabilityCheck(npc, Talents.GameTalent.TALENTS.Backstab) && npc.RightHand != null && Talents.BackstabTalent.AllowedBackstabItemBaseTypes.Contains(npc.RightHand.baseType))
                                {
                                    //TODO finish MeetsRequirements in GameTalent.Backstab
                                    command = "backstab";
                                }

                                CommandTasker.ParseCommand(npc, command, npc.MostHated.UniqueID.ToString());
                            }
                        }
                        catch (Exception e)
                        {
                            if (npc != null)
                            {
                                Utils.Log(
                                    "Failure at AI.ExecuteAction(" + npc.GetLogString() + ", " + actionType.ToString() +
                                    ", " + pri.ToString() + ") Combat Switch", Utils.LogType.ExceptionDetail);
                            }
                            Utils.LogException(e);
                        }

                        #endregion
                        break;
                }

                // After an action has been executed, clear out excluded priorities for a new decision.
                npc.ExcludedPrioritiesList.Clear();
            }
            catch (Exception e)
            {
                Utils.Log("Failure at AI.execute_action " + npc.GetLogString() + " Action: " + actionType.ToString() + " Priority: " + pri.ToString() + " (" + npc.LastCommand + ")", Utils.LogType.SystemFailure);
                Utils.LogException(e);
            }
        }

        #region Command AI
        public static void Command_Attack(Character pet, Character commander, string[] sArgs)
        {
            // sArgs = "attack orc" or "attack # orc"
            if (pet.canCommand && pet.PetOwner != null && pet.PetOwner == commander) // && previousMostHated == null? currently this allows two attacks if commanded while already attacking
            {
                Character target = TargetAcquisition.AcquireTarget(pet, sArgs, Cell.DEFAULT_VISIBLE_DISTANCE, 0);

                if (target == null)
                {
                    if (pet.IsUndead)
                    {
                        pet.SendToAllInSight(pet.GetNameForActionResult() + " moans.");
                        pet.EmitSound(pet.idleSound);

                    }
                    else commander.WriteToDisplay(pet.GetNameForActionResult() + " looks confused.");
                }
                else
                {
                    commander.CommandType = CommandTasker.CommandType.NPC_Command_Attack;

                    pet.EmitSound(pet.attackSound);
                    pet.TargetID = target.UniqueID;

                    pet.BreakFollowMode();

                    if (pet is NPC)
                    {
                        (pet as NPC).MostHated = target;
                        (pet as NPC).previousMostHated = target;
                        Rate(pet as NPC);
                    }
                }
            }
        }

        public static void Command_Begone(Character pet, Character commander)
        {
            // TODO: handle demons that can be sent back to hell with begone command here
            if ((commander.IsImmortal || (pet.canCommand && pet.PetOwner != null && pet.PetOwner == commander)) && pet is NPC)
            {
                if (pet.special.Contains("figurine"))
                    Rules.DespawnFigurine(pet as NPC);
                else if ((pet as NPC).IsSummoned) Rules.UnsummonCreature(pet as NPC);
                else if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Charm_Animal))
                    pet.EffectsList[Effect.EffectTypes.Charm_Animal].StopCharacterEffect();
                else if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Command_Undead))
                    pet.EffectsList[Effect.EffectTypes.Command_Undead].StopCharacterEffect();
                else
                {
                    if (commander.Pets.Contains(pet as NPC))
                        commander.Pets.Remove(pet as NPC);

                    pet.PetOwner = null;
                    pet.canCommand = false;

                    if (pet.special.Contains(" enslaved"))
                        pet.special.Replace(" enslaved", "");
                }
            }
            else if (pet.species == Globals.eSpecies.Demon || EntityLists.DEMONS.Contains((pet as NPC).entity))
            {
                if ((pet.Alignment == Globals.eAlignment.Lawful && commander.Alignment == Globals.eAlignment.Lawful) ||
                    (pet.Alignment == Globals.eAlignment.Amoral || pet.Alignment == Globals.eAlignment.None) || commander.Pets.Contains(pet as NPC))
                {
                    Rules.UnsummonCreature(pet as NPC);
                }

                //TODO what happens when a commander attempts to unsummon a demon?
            }
        }

        public static void Command_Climb(Character pet, Character commander, string args)
        {
            if (pet.canCommand && pet.PetOwner != null && pet.PetOwner == commander)
            {
                // npc will stop guarding
                //(pet as NPC).IsGuarding = false;

                if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Hello_Immobility)) // remove perma-root
                {
                    pet.EffectsList[Effect.EffectTypes.Hello_Immobility].StopCharacterEffect();
                }

                CommandTasker.ParseCommand(pet, "climb", args);

                // NPC stops following and waits for further instructions.
                pet.BreakFollowMode();
            }
        }

        public static void Command_Follow(Character pet, Character commander)
        {
            if (pet.canCommand)
            {
                if (pet.QuestList.Count > 0)
                {
                    foreach (GameQuest q in pet.QuestList)
                    {
                        if (q.ResponseStrings.ContainsKey("follow") && !q.PlayerMeetsRequirements((PC)commander, true))
                        {
                            commander.WriteToDisplay("You do not meet all requirements for the quest \"" + q.Name + ".\"");
                            return;
                        }
                    }
                }

                if (pet is NPC) (pet as NPC).IsGuarding = false;

                if (pet.FollowID == commander.UniqueID)
                {
                    commander.WriteToDisplay(pet.GetNameForActionResult() + " is already following you.");
                }
                else
                {
                    if (pet.FollowID != 0 && PC.GetOnline(pet.FollowID) != null)
                    {
                        if (TargetAcquisition.FindTargetInView(pet, pet.FollowID, false, false) != null)
                        {
                            commander.WriteToDisplay(pet.GetNameForActionResult() + " is already following someone.");
                            return;
                        }
                    }

                    // remove immobility effect
                    if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Hello_Immobility))
                        pet.EffectsList[Effect.EffectTypes.Hello_Immobility].StopCharacterEffect();

                    // npc will stop guarding
                    if (pet is NPC)
                        (pet as NPC).IsGuarding = false;

                    pet.FollowID = commander.UniqueID;

                    // set pet owner
                    pet.PetOwner = commander;

                    // add this npc to ch pets
                    if (!commander.Pets.Contains(pet as NPC))
                        commander.Pets.Add(pet as NPC);

                    // catch summoned salamanders trying to talk here
                    if ((pet as NPC).IsSummoned && !pet.animal && !EntityLists.ANIMAL.Contains(pet.entity))
                    {
                        switch (Rules.RollD(1, 4))
                        {
                            case 1:
                                commander.WriteToDisplay(pet.Name + ": I will follow you, master.");
                                break;
                            case 2:
                                commander.WriteToDisplay(pet.Name + ": Your wish is my command.");
                                break;
                            case 3:
                                commander.WriteToDisplay(pet.Name + ": Yes, master.");
                                break;
                            default:
                                commander.WriteToDisplay(pet.Name + ": You are my master.");
                                break;
                        }
                    }
                    else commander.WriteToDisplay(pet.GetNameForActionResult() + " begins to follow you.");

                    if (pet is NPC && !(pet as NPC).IsMobile)
                    {
                        (pet as NPC).wasImmobile = true;
                        (pet as NPC).IsMobile = true;
                    }

                    if (pet.IsImmortal)
                    {
                        pet.IsImmortal = false;
                        pet.WasImmortal = true;
                    }
                }
            }
            return;
        }

        public static void Command_Guard(Character pet, Character commander, string args)
        {
            if (commander.IsImmortal || (pet.canCommand && pet.PetOwner != null && pet.PetOwner == commander))
            {
                pet.IsResting = false;

                if (pet is NPC && !(pet as NPC).IsGuarding)
                {
                    (pet as NPC).IsGuarding = true; // used in AI.cs to determine if auto-attack
                    if ((pet as NPC).IsSummoned && !pet.animal && !EntityLists.ANIMAL.Contains(pet.entity))
                    {
                        switch (Rules.RollD(1, 4))
                        {
                            case 1:
                                commander.WriteToDisplay(pet.Name + ": I will be on guard, master.");
                                break;
                            case 2:
                                commander.WriteToDisplay(pet.Name + ": Your wish is my command.");
                                break;
                            case 3:
                                commander.WriteToDisplay(pet.Name + ": Yes, master.");
                                break;
                            default:
                                commander.WriteToDisplay(pet.Name + ": You are my master.");
                                break;
                        }
                    }
                    else commander.WriteToDisplay(pet.GetNameForActionResult() + " begins to guard the area.");
                }

                pet.BreakFollowMode();

                if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Hello_Immobility)) // remove perma-root
                    pet.EffectsList[Effect.EffectTypes.Hello_Immobility].StopCharacterEffect();
            }
        }

        public static void Command_Move(Character pet, Character commander, string command, string[] sArgs)
        {
            if (commander.IsImmortal || (pet.canCommand && pet.PetOwner != null && pet.PetOwner == commander))
            {
                //if (pet is NPC)
                //    (pet as NPC).IsGuarding = false;

                // remove immobility effect
                if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Hello_Immobility))
                    pet.EffectsList[Effect.EffectTypes.Hello_Immobility].StopCharacterEffect();

                string[] allowedDirections = new string[] {"n", "s", "e", "w", "nw", "sw", "ne", "se", "up",
                                "u", "d", "down" };

                if (command.ToLower() == "swim")
                {
                    CommandTasker.ParseCommand(pet, "swim", sArgs.Length > 0 ? sArgs[0] : "");
                }
                else
                {
                    int speed = 3; // pet won't move off screen on first command

                    // sArgs comes in as full movement directions
                    if (Array.IndexOf(allowedDirections, command.ToLower()) != -1)
                    {
                        CommandTasker.ParseCommand(pet, command, "");
                        speed--;
                        pet.BreakFollowMode();
                    }

                    if (sArgs == null || sArgs.Length == 0)
                        return;

                    foreach (string dir in sArgs)
                    {
                        if (speed > 0 && Array.IndexOf(allowedDirections, dir.ToLower()) != -1)
                        {
                            CommandTasker.ParseCommand(pet, dir, "");
                            speed--;
                            if (speed <= 0) break;
                        }
                    }
                }
            }
        }

        public static void Command_Obey(Character pet, Character commander, string args)
        {
            // TODO: handle demons that can be sent back to hell with begone command here
            if (pet.canCommand && pet.PetOwner != null && pet.PetOwner == commander)
            {
                Character target = TargetAcquisition.FindTargetInView(commander, args, false, true);

                if (target == null)
                {
                    commander.WriteToDisplay(pet.Name + ": I do not see " + args + " here.");
                    return;
                }

                if (!target.IsPC)
                {
                    commander.WriteToDisplay("Pets cannot currently be transferred to non player characters.");
                    return;
                }

                if (target.Pets.Count >= GameSpell.MAX_PETS)
                {
                    commander.WriteToDisplay(target.GetNameForActionResult() + " cannot control anymore pets.");
                    return;
                }

                if (pet is NPC)
                {
                    if (commander.Pets.Contains(pet as NPC))
                        commander.Pets.Remove(pet as NPC);

                    if (!target.Pets.Contains(pet as NPC))
                        target.Pets.Add(pet as NPC);
                }

                pet.PetOwner = target;
                pet.Alignment = target.Alignment;

                commander.WriteToDisplay(pet.GetNameForActionResult() + " begins to obey and follow " + target.GetNameForActionResult(true));
                target.WriteToDisplay(pet.GetNameForActionResult() + " begins to obey and follow you.");

                pet.FollowID = target.UniqueID;
                pet.EmitSound(pet.idleSound);
            }
        }

        public static void Command_Rest(Character pet, Character commander)
        {
            if (commander.IsImmortal || (pet.canCommand && pet.PetOwner == commander))
            {
                pet.BreakFollowMode();

                //if (pet is NPC)
                //    (pet as NPC).IsGuarding = false;

                CommandTasker.ParseCommand(pet, "rest", "");
            }
        }

        public static void Command_Take(Character pet, Character commander, string args)
        {
            if ((commander.IsImmortal || (pet.canCommand && pet.PetOwner != null && pet.PetOwner == commander)) && !pet.animal)
                CommandTasker.ParseCommand(pet, "take", args);
        }

        public static void Command_Drop(Character pet, Character commander, string args)
        {
            if (commander.IsImmortal || (pet.canCommand && pet.PetOwner == commander && !pet.animal))
                CommandTasker.ParseCommand(pet, "drop", args);
        }

        public static void Command_Wield(Character pet, Character commander, string args)
        {
            if (commander.IsImmortal || (pet.canCommand && pet.PetOwner == commander && !pet.animal))
                CommandTasker.ParseCommand(pet, "wield", args);
        }

        public static void Command_Belt(Character pet, Character commander, string args)
        {
            if (commander.IsImmortal || (pet.canCommand && pet.PetOwner == commander && !pet.animal))
                CommandTasker.ParseCommand(pet, "belt", args);
        }
        #endregion

        public static bool Interact(Character commander, Character target, string command, string args)
        {
            if (args.Contains(";"))
            {
                commander.WriteToDisplay("Interaction with an NPC cannot be joined with another command.");
                return false;
            }

            try
            {
                bool targetResponded = false; // used to detect quest response

                string movementArgs = args;

                args = args.Replace(command, ""); // can't do this for movement

                args = args.Trim();

                // args does not contain the command
                string[] sArgs = args.Split(" ".ToCharArray());

                #region Quest Logic for Players
                if (target.QuestList != null && target.QuestList.Count > 0 && commander is PC)
                {
                    for (int a = 0; a < target.QuestList.Count; a++)
                    {
                        GameQuest q = target.QuestList[a];

                        GameQuest activeQuest = target.GetQuest(q.QuestID);

                        if (activeQuest != null)
                        {
                            if (!activeQuest.IsRepeatable && activeQuest.TimesCompleted > 0)
                            {
                                continue;
                            }
                        }

                        #region Handle Quest Step Start Strings
                        foreach (string stepString in q.StepStrings.Keys)
                        {
                            if (command.ToLower().Contains(stepString.ToLower()))
                            {
                                q.BeginQuest((PC)commander, true);
                                if (activeQuest == null)
                                {
                                    activeQuest = commander.GetQuest(q.QuestID);
                                }
                                activeQuest.FinishStep((NPC)target, (PC)commander, q.StepStrings[stepString.ToLower()]);
                                targetResponded = true;
                                goto questResponses;
                            }
                        }
                    #endregion

                    questResponses:
                        #region Handle Quest Responses
                        foreach (string respString in q.ResponseStrings.Keys)
                        {
                            if (command.ToLower().Contains(respString.ToLower()))
                            {
                                string emote = Utils.ParseEmote(q.ResponseStrings[respString.ToLower()]);
                                string response = q.ResponseStrings[respString.ToLower()];

                                if (emote.Length > 0)
                                {
                                    response = response.Replace("{" + emote + "}", "");
                                    commander.WriteToDisplay(target.Name + " " + emote);
                                }

                                if (response.Length > 0)
                                {
                                    commander.WriteToDisplay(target.Name + ": " + response);
                                }

                                if (respString.ToLower() != "follow")
                                {
                                    Effect.CreateCharacterEffect(Effect.EffectTypes.Hello_Immobility, 0, target, Rules.Dice.Next(3, 6), null);
                                }

                                targetResponded = true;
                            }
                        }
                        #endregion

                        #region Handle Quest Flags
                        foreach (string flagString in q.FlagStrings.Keys)
                        {
                            if (command.ToLower().Contains(flagString.ToLower()))
                            {
                                if (!commander.QuestFlags.Contains(q.RewardFlags[q.FlagStrings[flagString]]))
                                {
                                    if (!q.StepOrder || (activeQuest != null && activeQuest.CurrentStep == q.FlagStrings[flagString]))
                                    {
                                        if (activeQuest == null)
                                        {
                                            q.BeginQuest((PC)commander, true);
                                            activeQuest = commander.GetQuest(q.QuestID);
                                        }

                                        if (activeQuest != null && !activeQuest.CompletedSteps.Contains(q.FlagStrings[flagString]))
                                        {
                                            activeQuest.FinishStep((NPC)target, (PC)commander, q.FlagStrings[flagString]);
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        if (activeQuest != null)
                                        {
                                            if (q.FailStrings.ContainsKey(activeQuest.CurrentStep))
                                            {
                                                string emote = Utils.ParseEmote(q.FailStrings[activeQuest.CurrentStep]);
                                                string failure = q.FailStrings[activeQuest.CurrentStep];
                                                if (emote.Length > 0)
                                                {
                                                    failure = failure.Replace("{" + emote + "}", "");
                                                    commander.WriteToDisplay(target.Name + " " + emote);
                                                }
                                                if (failure.Length > 0)
                                                {
                                                    commander.WriteToDisplay(target.Name + ": " + failure);
                                                }
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            commander.WriteToDisplay(target.Name + ": I cannot help you with that, " + commander.Name + ".");
                                            return true;
                                        }
                                    }
                                }
                                else
                                {
                                    commander.WriteToDisplay(target.Name + ": I have already helped you with that, " + commander.Name + ".");
                                    return true;
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion

                switch (command)
                {
                    case "hail":
                    case "hello":
                    case "hi":
                    case "hola":
                    case "yo":
                    case "oi":
                        #region hail, hello, hi
                        if (target is NPC && target.QuestList.Count > 0 && commander is PC)
                        {
                            int qCount = 0;
                            GameQuest q = null;

                            // only get a hint string from a quest that the player can do
                            do
                            {
                                q = target.QuestList[qCount];
                                qCount++;
                            }
                            while (!q.PlayerMeetsRequirements((PC)commander, false) && qCount < commander.QuestList.Count);

                            if (q.HintStrings.Count > 0 && q.PlayerMeetsRequirements((PC)commander, false))
                            {
                                int selection = Rules.Dice.Next(0, q.HintStrings.Count - 1);
                                string emote = Utils.ParseEmote(q.HintStrings[selection]);
                                string hint = q.HintStrings[selection];

                                if (emote.Length > 0)
                                {
                                    hint = hint.Replace("{" + emote + "}", "");
                                    commander.WriteToDisplay(target.Name + " " + emote);
                                }

                                if (hint.Length > 0)
                                    commander.WriteToDisplay(target.Name + ": " + hint);

                                if ((target as NPC).IsMobile || (target as NPC).HasPatrol)
                                    Effect.CreateCharacterEffect(Effect.EffectTypes.Hello_Immobility, 0, target, 2, null);

                                break;
                            }
                            //else
                            //    commander.WriteToDisplay(target.Name + ": Hello, " + commander.Name + ".");
                        }
                        else // responses if the target does not have a quest to give
                        {
                            if (target.Alignment == Globals.eAlignment.Lawful && commander.Alignment <= Globals.eAlignment.Neutral && EntityLists.ELVES.Contains(target.entity))
                            {
                                commander.WriteToDisplay(target.Name + ": Mae govannen!");
                            }
                            else if ((target is Merchant) && (target as Merchant).merchantType > Merchant.MerchantType.None)
                                commander.WriteToDisplay(target.Name + ": Hello.");
                            else if(EntityLists.IsHumanOrHumanoid(target.entity))
                            {
                                commander.WriteToDisplay(target.Name + ": Oloth plynn dos!"); // taken from the old game for posterity's sake
                            }
                            //else commander.WriteToDisplay(target.GetNameForActionResult(false) + " doesn't respond."); // taken from the old game for posterity's sake
                        }
                        #endregion
                        break;
                    case "app":
                    case "appraise":
                        #region appraise
                        if (!(target is Merchant) || ((target is Merchant) && (target as Merchant).merchantType == Merchant.MerchantType.None))
                        {
                            commander.WriteToDisplay(target.Name + ": Speak to a merchant about that.");
                            return true;
                        }
                        else
                        {
                            if (args == null || args == "")
                            {
                                commander.WriteToDisplay(target.Name + ": What do you want me to appraise?");
                                return true;
                            }
                            else (target as Merchant).MerchantAppraise(commander as PC, args);
                        }
                        break;
                    #endregion
                    case "show":
                        #region show
                        if ((target is Merchant) && (target as Merchant).trainerType == Merchant.TrainerType.HP_Doctor)
                        {
                            #region HP Doctor
                            int doctoredHPLimit = World.DoctoredHPLimits[(int)commander.BaseProfession]; // get doctored hp limit

                            commander.WriteToDisplay(target.Name + ": The maximum hit points you may have doctored is " + doctoredHPLimit + ".");
                            commander.WriteToDisplay(target.Name + ": You currently have " + (commander.HitsDoctored <= 0 ? "no" : commander.HitsDoctored.ToString()) + " doctored hit " + (commander.HitsDoctored == 1 ? "point" : "points") + ".");

                            if (commander.HitsDoctored + 1 < doctoredHPLimit)
                            {
                                long nextHPCost = Rules.Formula_DoctoredHPCost(commander.HitsDoctored + 1, commander.BaseProfession);

                                commander.WriteToDisplay(target.Name + ": Your next doctored hit point will cost " + String.Format("{0:n0}", nextHPCost) + " coins.");
                            }
                            return true;
                            #endregion
                        }

                        if (String.IsNullOrEmpty(args))
                        {
                            commander.WriteToDisplay(target.Name + ": What do you want me to show you?");
                            break;
                        }

                        if (args.Equals("list") || args.Equals("prices") || args.Equals("items"))
                        {
                            if ((target is Merchant) && (target as Merchant).merchantType > Merchant.MerchantType.None)
                            { (target as Merchant).SendMerchantList(commander as PC); }
                            else { commander.WriteToDisplay(target.Name + ": I do not have anything to sell."); }
                        }
                        else if (args.Equals("spells"))
                        {
                            #region Show Spells
                            if ((target is Merchant) && (target as Merchant).trainerType == Merchant.TrainerType.Spell && commander.BaseProfession == target.BaseProfession)
                            {
                                commander.WriteToDisplay((target as Merchant).GetMerchantSpellList(commander as PC));
                            }
                            else
                            {
                                if ((target is Merchant) && (target as Merchant).trainerType == Merchant.TrainerType.Spell)
                                {
                                    switch (commander.BaseProfession)
                                    {
                                        case ClassType.Druid:
                                        case ClassType.Ranger:
                                            commander.WriteToDisplay(target.Name + ": I know nothing of your odd forest magic.");
                                            break;
                                        case ClassType.Berserker:
                                        case ClassType.Fighter:
                                        case ClassType.Martial_Artist:
                                        case ClassType.Knight:
                                        case ClassType.Ravager:
                                            commander.WriteToDisplay(target.Name + ": I cannot teach magic to a " + commander.classFullName + ", therefore I do not have any spells to show you.");
                                            break;
                                        case ClassType.Thaumaturge:
                                            if (commander.LandID == Land.ID_BEGINNERSGAME)
                                            {
                                                if (commander.MapID == Map.ID_KESMAI)
                                                { commander.WriteToDisplay(target.Name + ": I am not skilled in the ways of thaumaturgy. Speak with Sven in the temple."); }
                                                else if (commander.MapID == Map.ID_LENG)
                                                { commander.WriteToDisplay(target.Name + ": I am not a thaumaturgist. You should visit the priest that dwells in the east, beyond the dust plains."); }
                                                else if (commander.MapID == Map.ID_AXEGLACIER)
                                                { commander.WriteToDisplay(target.Name + ": I am not a thaumaturge. Travel to the temple near the portal, or head up to Lockpick Town and speak to the priests there."); }
                                                else if (commander.MapID == Map.ID_OAKVAEL)
                                                { commander.WriteToDisplay(target.Name + ": Go to the temple and speak to the priests there. I do not practice thaumaturgy."); }
                                                else { commander.WriteToDisplay(target.Name + ": I do not practice thaumaturgy. Ask a priest in one of the temples to help you."); }
                                            }
                                            break;
                                        case ClassType.Thief:
                                            if (target.BaseProfession == ClassType.Thaumaturge) { commander.WriteToDisplay(target.Name + ": I do not dabble in nor do I tolerate the foul art of shadow magic. Begone!"); }
                                            else { commander.WriteToDisplay(target.Name + ": I do not recognize your aura... shadow magic I bet. I cannot help you."); }
                                            break;
                                        case ClassType.Wizard:
                                            commander.WriteToDisplay(target.Name + ": I am not versed in the ways of wizardry. Search elsewhere for assistance.");
                                            break;
                                        default:
                                            commander.WriteToDisplay(target.GetNameForActionResult() + ": I do not know anything about your magical ability. I suggest you look elsewhere.");
                                            break;
                                    }
                                }
                                else { commander.WriteToDisplay(target.Name + ": I do not know anything about magic."); }
                            } 
                            #endregion
                        }
                        else if (args.Equals("balance"))
                        {
                            if ((target is Merchant) && (target as Merchant).interactiveType == Merchant.InteractiveType.Banker)
                            { (target as Merchant).MerchantShowBalance(commander as PC); }
                            else { commander.WriteToDisplay(target.Name + ": I am not a banker."); }
                        }
                        else if ((target is Merchant) && (target as Merchant).interactiveType == Merchant.InteractiveType.Mentor && args.StartsWith("talent"))
                        {
                            commander.WriteToDisplay((target as Merchant).GetMerchantTalentList(commander as PC));
                        }
                        else if ((target is Merchant) && (target as Merchant).MerchantShowItem(commander, args))
                        {
                            break;
                        }
                        break;
                    #endregion
                    case "sell":
                        #region sell
                        if (String.IsNullOrEmpty(args))
                        {
                            if ((target is Merchant) && (target as Merchant).merchantType != Merchant.MerchantType.None)
                            { commander.WriteToDisplay(target.Name + ": What do you want me to sell to you?"); }
                            else { commander.WriteToDisplay(target.Name + ": I have nothing to sell. Speak with a merchant."); }
                            break;
                        }

                        if ((target is Merchant) && ((target as Merchant).merchantType != Merchant.MerchantType.None || (target as Merchant).trainerType == Merchant.TrainerType.Spell))
                        {
                            if (target.BaseProfession == commander.BaseProfession && (target as Merchant).trainerType == Merchant.TrainerType.Spell)
                            {
                                if (args == "spellbook" || args == "totem")
                                {
                                    commander.TargetID = target.UniqueID;
                                    commander.WriteToDisplay((target as Merchant).MerchantSellItem(commander as PC, args));
                                }
                                else
                                {
                                    commander.WriteToDisplay(target.Name + ": I do not sell that. However I do sell spellbooks.");
                                }
                                break;
                            }
                            else if (target.BaseProfession != commander.BaseProfession && (target as Merchant).trainerType == Merchant.TrainerType.Spell)
                            {
                                switch (commander.BaseProfession)
                                {
                                    case ClassType.Sorcerer:
                                    case ClassType.Thaumaturge:
                                    case ClassType.Wizard:
                                    case ClassType.Thief:
                                    case ClassType.Druid:
                                    case ClassType.Ranger:
                                        if (sArgs[1].ToLower() == "spellbook")
                                            commander.WriteToDisplay(target.Name + ": You should speak to someone of your profession about scribing a new spellbook.");
                                        else
                                            commander.WriteToDisplay(target.Name + ": I have nothing to sell to you.");
                                        break;
                                    default:
                                        commander.WriteToDisplay(target.Name + ": I have nothing to sell to a " + commander.classFullName.ToLower() + ".");
                                        break;
                                }
                                break;
                            }
                            else
                            {
                                target.TargetID = commander.UniqueID;
                                commander.WriteToDisplay((target as Merchant).MerchantSellItem(commander as PC, args));
                                break;
                            }
                        }
                        else
                        {
                            commander.WriteToDisplay(target.Name + ": I have nothing to sell. Speak to a merchant.");
                        }
                        break;
                    #endregion
                    case "buy":
                        #region buy
                        if (!(target is Merchant) || ((target is Merchant) && (target as Merchant).merchantType == Merchant.MerchantType.None))
                            commander.WriteToDisplay(target.Name + ": I am not a merchant.");
                        else
                        {
                            if (String.IsNullOrEmpty(args))
                                commander.WriteToDisplay(target.Name + ": What do you want me to buy from you?");
                            else
                                commander.WriteToDisplay((target as Merchant).MerchantBuyItem(commander as PC, args));
                        }
                        break;
                    #endregion
                    case "specialize":
                        #region specialize
                        if (!(target is Merchant) || ((target is Merchant) && (target as Merchant).trainerType != Merchant.TrainerType.Weapon))
                        {
                            commander.WriteToDisplay(target.Name + ": I do not know how to do that.");
                            break;
                        }
                        if (Array.IndexOf(World.WeaponSpecializationProfessions, commander.BaseProfession) == -1)
                        {
                            commander.WriteToDisplay(target.Name + ": I only teach those with a true warrior's spirit how to hone their skills.");
                            break;
                        }
                        if (Rules.GetExpLevel(commander.Experience) < Character.WARRIOR_SPECIALIZATION_LEVEL)
                        {
                            commander.WriteToDisplay(target.Name + ": I admire your ambition, " + commander.Name + ", however you will need to return to me after you have learned more.");
                            break;
                        }
                        if (commander.RightHand == null)
                        {
                            commander.WriteToDisplay(target.Name + ": Wield the weapon you wish to specialize with in your right hand and ask me again.");
                            break;
                        }
                        if (commander.RightHand.skillType == Globals.eSkillType.Unarmed)
                        {
                            commander.WriteToDisplay(target.Name + ": That weapon is best suited for a martial artist.");
                            break;
                        }
                        if (commander.RightHand.skillType == Globals.eSkillType.Bash)
                        {
                            commander.WriteToDisplay(target.Name + ": You may not specialize with " + commander.RightHand.shortDesc + "...yet.");
                            break;
                        }
                        if (commander.fighterSpecialization == Globals.eSkillType.None)
                        {
                            commander.WriteToDisplay(target.Name + ": From this point forward you will receive specialized training in your " + Utils.FormatEnumString(commander.RightHand.skillType.ToString()).ToLower() + " skill.");
                            commander.fighterSpecialization = commander.RightHand.skillType;
                            break;
                        }
                        else
                        {
                            if (commander.fighterSpecialization == commander.RightHand.skillType)
                            {
                                commander.WriteToDisplay(target.Name + ": You are already specialized with that weapon.");
                                break;
                            }

                            Item coins = Item.RemoveItemFromGround("coins", commander.CurrentCell);

                            if (coins == null || coins.coinValue < 2000000)
                            {
                                commander.WriteToDisplay(target.Name + ": The cost to respecialize is substantial. I will do it for no less than 2,000,000 gold coins.");
                                break;
                            }

                            coins.coinValue -= 2000000;

                            if (coins.coinValue > 0)
                                commander.CurrentCell.Add(coins);

                            commander.WriteToDisplay(target.Name + ": From this point forward you will receive specialized training in your " + Utils.FormatEnumString(commander.RightHand.skillType.ToString()).ToLower() + " skill.");
                            commander.fighterSpecialization = commander.RightHand.skillType;
                            //commander.WriteToDisplay(target.Name + ": You are already specialized with that. (" + Utils.FormatEnumString(commander.fighterSpecialization.ToString()).ToLower() + ")");
                            break;
                        }
                    #endregion
                    case "train":
                        #region train
                        if (!(target is Merchant) || ((target is Merchant) && (target as Merchant).trainerType == Merchant.TrainerType.None))
                            commander.WriteToDisplay(target.Name + ": I am not a trainer.");
                        else (target as Merchant).MerchantTrain(commander as PC, sArgs);
                        break;
                    #endregion
                    case "teach":
                        #region teach
                        if (targetResponded)
                            break;

                        // at a Mentor attempting to learn a talent that exists
                        if (sArgs.Length == 1 && target is Merchant && (target as Merchant).interactiveType == Merchant.InteractiveType.Mentor &&
                            Talents.GameTalent.GameTalentDictionary.ContainsKey(sArgs[0]))
                        {
                            (target as Merchant).MerchantMentor(commander as PC, sArgs[0]);
                            break;
                        }

                        if (!(target is Merchant) || ((target is Merchant) && (target as Merchant).trainerType != Merchant.TrainerType.Spell))
                        {
                            commander.WriteToDisplay(target.Name + ": I do not know how to teach that.");
                            break;
                        }

                        if (string.IsNullOrEmpty(args))
                        {
                            commander.WriteToDisplay(target.Name + ": What do you want me to teach you?");
                            break;
                        }

                        if (target.BaseProfession != commander.BaseProfession)
                        {
                            commander.WriteToDisplay(target.Name + ": I do not know how to teach you.");
                            break;
                        }

                        var whichHand = commander.WhichHand("spellbook");

                        if (whichHand != (int)Globals.eWearOrientation.None)// commander.RightHand != null && commander.RightHand.baseType == Globals.eItemBaseType.Book)
                        {
                            Book book = whichHand == (int)Globals.eWearOrientation.Right ? (Book)commander.RightHand : (Book)commander.LeftHand;
                            if (book.BookType == Book.BookTypes.Spellbook)
                            {
                                if (book.attunedID == commander.UniqueID)
                                {
                                    //args = args.Substring(args.IndexOf(' ') + 1);

                                    GameSpell spell = GameSpell.GetSpell(args.ToLower());

                                    if (spell == null || (spell != null && !spell.IsAvailableAtTrainer))
                                    {
                                        commander.WriteToDisplay(target.Name + ": I do not know a spell called \"" + args + "\".");
                                        break;
                                    }

                                    if (commander.spellDictionary.ContainsKey(spell.ID))
                                    {
                                        commander.WriteToDisplay(target.Name + ": You already know the spell " + spell.Name + ".");
                                        break;
                                    }

                                    if (Skills.GetSkillLevel(commander.magic) < spell.RequiredLevel)
                                    {
                                        commander.WriteToDisplay(target.Name + ": You are not ready to learn the spell " + spell.Name + " yet.");
                                        break;
                                    }

                                    Item coins = Item.RemoveItemFromGround("coins", target.FacetID, target.LandID, target.MapID, target.X, target.Y, target.Z);

                                    if (coins == null)
                                    {
                                        coins = Map.RemoveItemFromCounter(target, "coins");

                                        if (coins == null)
                                        {
                                            commander.WriteToDisplay(target.Name + ": I do not see any coins here.");
                                            break;
                                        }
                                    }

                                    if (coins.coinValue == spell.TrainingPrice)
                                    {
                                        GameSpell.TeachSpell(commander, args.ToLower(), target.GetNameForActionResult());
                                    }
                                    else if (coins.coinValue >= spell.TrainingPrice)
                                    {
                                        coins.coinValue = coins.coinValue - spell.TrainingPrice;

                                        if (coins.coinValue > 0)
                                        {
                                            if (Map.IsNextToCounter(target))
                                            {
                                                Map.PutItemOnCounter(target, coins);
                                            }
                                            else
                                            {
                                                target.CurrentCell.Add(coins);
                                            }
                                        }

                                        GameSpell.TeachSpell(commander, spell.Command, target.GetNameForActionResult());
                                    }
                                    else
                                    {
                                        if (Map.IsNextToCounter(target))
                                        {
                                            Map.PutItemOnCounter(target, coins);
                                        }
                                        else
                                        {
                                            target.CurrentCell.Add(coins);
                                        }
                                        commander.WriteToDisplay(target.Name + ": There aren't enough coins here to cover the cost of that spell.");
                                    }
                                }
                                else
                                {
                                    commander.WriteToDisplay(target.Name + ": That spellbook is soulbound to another being.");
                                }
                            }
                            else
                            {
                                commander.WriteToDisplay(target.Name + ": You must be holding your spellbook while learning a new spell.");
                            }
                        }
                        else
                        {
                            commander.WriteToDisplay(target.Name + ": You must be holding your spellbook while learning a new spell.");
                        }
                        break;
                    #endregion
                    case "give":
                        #region give
                        foreach (GameQuest quest in commander.QuestList)
                        {
                            if (quest.ResponseStrings.ContainsKey("give"))
                            {
                                targetResponded = true;
                                break;
                            }
                        }

                        if (!targetResponded)
                            commander.WriteToDisplay(target.Name + ": Perhaps you should give me something first.");
                        break;
                    #endregion
                    case "critique":
                    case "crit":
                        #region critique
                        if (!(target is Merchant))
                            break;

                        if (args == null || args == "" || args == "right") // trainer, critique OR trainer, critique right
                        {
                            if (commander.RightHand != null) (target as Merchant).MerchantCritique(commander as PC, Utils.FormatEnumString(commander.RightHand.skillType.ToString()));
                            else (target as Merchant).MerchantCritique(commander as PC, "unarmed");
                            break;
                        }

                        if (args == "left")
                        {
                            if (commander.LeftHand != null) (target as Merchant).MerchantCritique(commander as PC, Utils.FormatEnumString(commander.LeftHand.skillType.ToString()));
                            else (target as Merchant).MerchantCritique(commander as PC, "unarmed");
                            break;
                        }

                        if (sArgs.Length >= 1) // trainer, critique <skill>
                        {
                            if (sArgs[0].ToLower() == "greatsword" || sArgs[0].ToLower() == "two" ||
                                sArgs[0].ToLower() == "twohanded" || sArgs[0].ToLower() == "broadsword")
                                sArgs[0] = "two handed";

                            (target as Merchant).MerchantCritique(commander as PC, sArgs[0]);
                            break;
                        }
                        else // catch
                        {
                            if (commander.RightHand == null) (target as Merchant).MerchantCritique(commander as PC, Globals.eSkillType.Unarmed.ToString());
                            else (target as Merchant).MerchantCritique(commander as PC, Utils.FormatEnumString(commander.RightHand.skillType.ToString()));
                        }
                        break;
                    #endregion
                    case "dep":
                    case "deposit":
                        #region deposit
                        if (!(target is Merchant) || ((target is Merchant) && (target as Merchant).interactiveType != Merchant.InteractiveType.Banker))
                        {
                            commander.WriteToDisplay(target.Name + ": I am not a banker.");
                            return true;
                        }

                        if (sArgs.Length >= 1) (target as Merchant).MerchantDeposit(commander as PC, sArgs[0]);
                        else (target as Merchant).MerchantDeposit(commander as PC, "all");

                        break;
                    #endregion
                    case "withdraw":
                    case "wd":
                        #region withdraw
                        if (!(target is Merchant) || ((target is Merchant) && (target as Merchant).interactiveType != Merchant.InteractiveType.Banker))
                        {
                            commander.WriteToDisplay(target.Name + ": I am not a banker.");
                            return true;
                        }
                        else (target as Merchant).MerchantWithdraw(commander as PC, sArgs[0]);
                        break;
                    #endregion

                    #region Pets and Quest NPC Commands
                    case "climb":
                        AI.Command_Climb(target, commander, args);
                        break;
                    case "follow":
                    case "fol":
                        AI.Command_Follow(target, commander);
                        break;
                    case "unfollow":
                    case "unfol":
                    case "stop":
                        if (target.FollowID == commander.UniqueID)
                        {
                            commander.WriteToDisplay(target.GetNameForActionResult() + " stops following you.");
                            target.BreakFollowMode();
                        }
                        break;
                    case "rest":
                        AI.Command_Rest(target, commander);
                        break;
                    case "guard": // TODO: add ability to guard targets
                        AI.Command_Guard(target, commander, "");
                        break;
                    case "ob":
                    case "obey":
                        if (sArgs.Length >= 2)
                            AI.Command_Obey(target, commander, args);
                        break;
                    case "begone":
                        AI.Command_Begone(target, commander);
                        break;
                    case "swim":
                    case "go":
                        AI.Command_Move(target, commander, command, sArgs);
                        break;
                    case "attack":
                    case "fight":
                    case "kill":
                    case "f":
                        AI.Command_Attack(target, commander, sArgs);
                        break;
                    case "t":
                    case "take":
                    case "get":
                        AI.Command_Take(target, commander, args);
                        break;
                    case "drop":
                        AI.Command_Drop(target, commander, args);
                        break;
                    case "wield":
                    case "draw":
                        AI.Command_Wield(target, commander, args);
                        break;
                    case "belt":
                    case "sheathe":
                        AI.Command_Belt(target, commander, args);
                        break;
                    case "n":
                    case "s":
                    case "e":
                    case "w":
                    case "ne":
                    case "se":
                    case "nw":
                    case "sw":
                    case "dn":
                    case "down":
                    case "u":
                    case "up":
                        AI.Command_Move(target, commander, command, movementArgs.Split(" ".ToCharArray()));
                        break; 
                    #endregion

                    case "diag":
                    case "diagnose":
                    case "mend":
                        if (target is Merchant && (target as Merchant).interactiveType == Merchant.InteractiveType.Mender)
                        {
                            (target as Merchant).MerchantMender(commander, command, args);
                        }
                        else commander.WriteToDisplay(target.GetNameForActionResult() + ": I have no idea what you're talking about. Have you spoken to a Mender yet?");
                        break;
                    default:
                        if(target.PetOwner == commander || (commander is PC) && (commander as PC).ImpLevel > Globals.eImpLevel.USER)
                        {
                            CommandTasker.ParseCommand(target, command, args);
                        }
                        targetResponded = false;
                        break;
                }

                if (targetResponded) { commander.CommandType = CommandTasker.CommandType.NPCInteraction; }

                // Target is following someone, or being commanded.
                if (target != null && target.FollowID != 0)
                    Effect.CreateCharacterEffect(Effect.EffectTypes.Hello_Immobility, 0, target, Rules.RollD(3, 6), null);

                return true;
            }
            catch (Exception e)
            {
                Utils.Log("AI.Interact [command: " + command + " args: " + args + "]", Utils.LogType.SystemFailure);
                Utils.LogException(e);
                return false;
            }
        }

        /// <summary>
        /// The Character will move away from a specified cell.
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="cellToBackAwayFrom"></param>
        /// <returns>Returns true if the Character is able to move away, false otherwise.</returns>
        public static bool BackAwayFromCell(Character npc, Cell cellToBackAwayFrom)
        {
            if (npc == null || npc.CurrentCell == null || cellToBackAwayFrom == null)
                return false;

            if (npc.CurrentCell.IsStairsUp && Rules.CheckPerception(npc))
            {
                CommandTasker.ParseCommand(npc, "up", "");
                return true;
            }

            if (npc.CurrentCell.IsStairsDown && Rules.CheckPerception(npc))
            {
                CommandTasker.ParseCommand(npc, "down", "");
                return true;
            }

            ArrayList directions = new ArrayList();

            Map.Direction targetDirection = Map.GetDirection(npc.CurrentCell, cellToBackAwayFrom);

            switch (targetDirection)
            {
                case Map.Direction.East:
                    directions.Add(Map.Direction.West.ToString());
                    directions.Add(Map.Direction.Northwest.ToString());
                    directions.Add(Map.Direction.Southwest.ToString());
                    break;
                case Map.Direction.North:
                    directions.Add(Map.Direction.South.ToString());
                    directions.Add(Map.Direction.Southwest.ToString());
                    directions.Add(Map.Direction.Southeast.ToString());
                    break;
                case Map.Direction.Northeast:
                    directions.Add(Map.Direction.South.ToString());
                    directions.Add(Map.Direction.Southwest.ToString());
                    directions.Add(Map.Direction.West.ToString());
                    break;
                case Map.Direction.Northwest:
                    directions.Add(Map.Direction.South.ToString());
                    directions.Add(Map.Direction.Southeast.ToString());
                    directions.Add(Map.Direction.East.ToString());
                    break;
                case Map.Direction.South:
                    directions.Add(Map.Direction.North.ToString());
                    directions.Add(Map.Direction.Northwest.ToString());
                    directions.Add(Map.Direction.Northeast.ToString());
                    break;
                case Map.Direction.Southeast:
                    directions.Add(Map.Direction.North.ToString());
                    directions.Add(Map.Direction.Northwest.ToString());
                    directions.Add(Map.Direction.West.ToString());
                    break;
                case Map.Direction.Southwest:
                    directions.Add(Map.Direction.North.ToString());
                    directions.Add(Map.Direction.Northeast.ToString());
                    directions.Add(Map.Direction.East.ToString());
                    break;
                case Map.Direction.None:
                    List<Cell> cList = Map.GetAdjacentCells(npc.CurrentCell, npc);
                    if (cList != null)
                    {
                        int rand = Rules.Dice.Next(cList.Count - 1);
                        Cell nCell = (Cell)cList[rand];
                        npc.AIGotoXYZ(nCell.X, nCell.Y, nCell.Z);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }

            for (int a = 0; a < directions.Count; a++)
            {
                Cell cell = Map.GetCellRelevantToCell(npc.CurrentCell, directions[a].ToString().ToLower(), true);
                if (npc.GetCellCost(cell) <= 2)
                {
                    npc.AIGotoXYZ(cell.X, cell.Y, cell.Z);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called if NPC has no allies and is below 20% health.
        /// Should not be used for lair critters, undead, and creatures in groups.
        /// </summary>
        /// <param name="npc">The NPC determining fight or flight.</param>
        /// <returns>True if fight, false if flight.</returns>
        public static bool FightNotFlight(NPC npc)
        {
            if (EntityLists.EntityListContains(EntityLists.FEARLESS, npc.entity) || npc.IsUndead) return true;

            if (npc.Map.ZPlanes[npc.Z].zAutonomy != null && npc.Map.ZPlanes[npc.Z].zAutonomy.guardZone) return true;

            try
            {
                if (npc.MostHated != null)
                {
                    // only one enemy present
                    if (npc.enemyList.Count < 2)
                    {
                        // fight if enemy is blind, feared, stunned or below 20% health
                        if (npc.MostHated.IsBlind || npc.MostHated.IsFeared || npc.MostHated.Stunned > 0 || npc.MostHated.Hits < npc.MostHated.HitsFull / 5)
                        {
                            return true;
                        }

                        // this could be expanded upon later. If friends are near then stay and fight.
                        if (npc.friendList.Count > 0)
                        {
                            return true;
                        }

                        // spell user will evaluate mana available if it passes perception check
                        if (npc.IsSpellUser && npc.BaseProfession == npc.MostHated.BaseProfession && Rules.CheckPerception(npc) &&
                            npc.MostHated.Mana / npc.MostHated.ManaMax <= 10)
                        {
                            return true;
                        }

                        // a pure fighter wielding a weapon it is specialized with will fight to the death
                        if (npc.RightHand != null && npc.fighterSpecialization == npc.RightHand.skillType)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }

        public static void DragonFear(NPC dragon)
        {
            int bitcount = 0;
            Cell curCell = null;

            try
            {
                //loop through all visible cells
                for (int ypos = -3; ypos <= 3; ypos += 1)
                {
                    //dragon looks at each cell
                    for (int xpos = -3; xpos <= 3; xpos += 1)
                    {
                        //Check the PC list, and Char list of the cell
                        if (dragon.CurrentCell.visCells[bitcount])
                        {
                            curCell = Cell.GetCell(dragon.FacetID, dragon.LandID, dragon.MapID, dragon.X + xpos, dragon.Y + ypos, dragon.Z);

                            //Look for the character in the charlist of the cell
                            foreach (Character chr in curCell.Characters.Values)
                            {
                                if (chr != dragon)
                                {
                                    if (Combat.DoSpellDamage(dragon, chr, null, 0, "dragon fear") == 1)
                                    {
                                        Effect.CreateCharacterEffect(Effect.EffectTypes.Fear, 1, chr, Rules.RollD(1, 4), dragon);
                                    }
                                }
                            }
                        }
                        bitcount += 1;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void GiantStomp(NPC giant)
        {
            int bitcount = 0;
            Cell curCell = null;

            try
            {
                //loop through all visable cells
                for (int ypos = -3; ypos <= 3; ypos += 1)
                {
                    //giant looks at each cell
                    for (int xpos = -3; xpos <= 3; xpos += 1)
                    {
                        //Check the PC list, and Char list of the cell
                        if (giant.CurrentCell.visCells[bitcount])
                        {

                            curCell = Cell.GetCell(giant.FacetID, giant.LandID, giant.MapID, giant.X + xpos, giant.Y + ypos, giant.Z);

                            //Look for the character in the charlist of the cell
                            foreach (Character chr in curCell.Characters.Values)
                            {
                                if (chr != giant && chr.Name != "giant")
                                {
                                    if (Combat.DoSpellDamage(giant, chr, null, 0, "giant stomp") == 1)
                                    {
                                        chr.WriteToDisplay("You are jolted by the shaking ground!");
                                        Combat.DoDamage(chr, giant, Rules.Dice.Next(2) + 1, false);
                                    }
                                }
                            }
                        }
                        bitcount += 1;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void EscortQuestLogic(NPC escortedNPC)
        {
            // the escort NPC has the escort-to NPC's npcid in required items, as well as any response strings and finish strings (2 of them)
            // the escort-to NPC has the complete quest in it, with item rewards, and the escorted NPC's npcID in RequiredItems[1]
            // please note there should be two separate QuestIDs, one for the NPC being escorted and one for the escort-to NPC
            try
            {
                // Iterate through escorted NPCs quest list.
                foreach (GameQuest q in escortedNPC.QuestList)
                {
                    // Quest has an NPC requirement.
                    if (q.Requirements.ContainsValue(GameQuest.QuestRequirement.NPC))
                    {
                        // Iterate through the values and look for an NPC ID of target.
                        foreach (int npcid in q.RequiredItems.Values)
                        {
                            // Find the NPC in the current cell.
                            NPC npc = TargetAcquisition.FindQuestNPCInCell(escortedNPC, npcid);

                            // Found the NPC we were to be escorted to.
                            if (npc != null)
                            {
                                q.FinishStep(escortedNPC, (PC)escortedNPC.PetOwner, q.CurrentStep);

                                foreach (GameQuest q2 in npc.QuestList)
                                {
                                    if (q2.Requirements.ContainsValue(GameQuest.QuestRequirement.NPC))
                                    {
                                        foreach (int escortedID in q2.RequiredItems.Values)
                                        {
                                            if (escortedID == escortedNPC.npcID)
                                            {
                                                q2.FinishStep(npc, (PC)escortedNPC.PetOwner, q2.CurrentStep);
                                            }
                                        }
                                    }
                                }

                                escortedNPC.BreakFollowMode(); // clear follow mode

                                escortedNPC.Pets.Remove(escortedNPC); // remove escorted NPC from pets list

                                escortedNPC.canCommand = false; // no longer command the escorted npc

                                Effect.CreateCharacterEffect(Effect.EffectTypes.Hello_Immobility, 0, escortedNPC, -1, null); // perma root                                

                                escortedNPC.PetOwner = null; // escorted NPC no longer has an owner

                                // Quest is set to despawn escorted NPC. (this will reset the spawn timer for the escorted NPC so the quest could be done again)
                                if (q.DespawnsNPC)
                                {
                                    escortedNPC.RoundsRemaining = 0;
                                    escortedNPC.special += " despawn"; // flag npc to despawn in RoundsRemaining rounds
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static bool PrepareSpell(NPC npc, AI.Priority pri) // creature spell selection
        {
            if (npc.preppedSpell != null) return true; // overrides in place to prep a spell during action rating

            #region Priority.Heal
            if (pri == AI.Priority.Heal)
            {
                if (npc.castMode == CastMode.Unlimited)
                {
                    npc.preppedSpell = GameSpell.GetSpell("cure");
                    return true;
                }
                else
                {
                    GameSpell spell = null;

                    if (npc.BaseProfession == ClassType.Sorcerer)
                    {
                        spell = GameSpell.GetSpell("lifeleech");
                        if (npc.HasManaAvailable(spell.ID))
                        {
                            npc.preppedSpell = spell;
                            return true;
                        }
                    }
                    else
                    {
                        spell = GameSpell.GetSpell("cure");

                        if (npc.HasManaAvailable(spell.ID))
                        {
                            if (npc.castMode == CastMode.NoPrep)
                            {
                                npc.preppedSpell = spell;
                                return true;
                            }
                            else if (npc.castMode == CastMode.Limited)
                            {
                                string[] chant = npc.spellDictionary[spell.ID].Split(" ".ToCharArray());
                                CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                return true;
                            }
                        }
                    }
                }
            }
            #endregion

            #region Priority.RaiseDead
            else if (pri == AI.Priority.RaiseDead)
            {
                GameSpell raiseDeadSpell = GameSpell.GetSpell("raisedead");

                if (npc.castMode == CastMode.Unlimited)
                {
                    npc.preppedSpell = raiseDeadSpell;
                    return true;
                }
                else
                {
                    if (SpellAvailabilityCheck(npc, raiseDeadSpell.ID))
                    {
                        if (npc.castMode == CastMode.NoPrep)
                        {
                            npc.preppedSpell = raiseDeadSpell;
                            return true;
                        }
                        else if (npc.castMode == CastMode.Limited)
                        {
                            string[] chant = npc.spellDictionary[raiseDeadSpell.ID].Split(" ".ToCharArray());
                            CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                            return true;
                        }
                    }
                }
            }
            #endregion

            else if (pri >= AI.Priority.PrepareSpell) // we're preparing a spell to attack an enemy
            {
                #region mostHated != null
                if (npc.MostHated != null)
                {
                    #region AI Enforcer will cast death spell, dismiss undead or icespear.
                    if (npc.aiType == NPC.AIType.Enforcer)
                    {
                        // if undead target cast Dismiss Undead
                        if (npc.MostHated.IsUndead)
                            npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Dismiss_Undead);
                        // otherwise cast death spell
                        else if (!npc.MostHated.immuneDeath && !EntityLists.IMMUNE_DEATH.Contains(npc.MostHated.entity)) npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Death);
                        else npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Icespear);
                        return true;
                    }
                    #endregion

                    #region Use hard-coded species types and npc.animal attribute to prepare a spell. This may no longer be necessary with the above conditional statement.
                    // Use hard-coded species type to determine a spell.
                    switch (npc.species)
                    {
                        case Globals.eSpecies.Pheonix:
                            npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Firebolt);
                            return true;
                        case Globals.eSpecies.FireDragon: // TODO: dragons that cast spells other than dragon's breath
                        case Globals.eSpecies.IceDragon:
                        case Globals.eSpecies.WindDragon:
                        case Globals.eSpecies.CloudDragon:
                            npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Dragon__s_Breath);
                            return true;
                        case Globals.eSpecies.LightningDrake:
                            npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Lightning_Bolt);
                            return true;
                        case Globals.eSpecies.TundraYeti:
                            npc.preppedSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Blizzard);
                            return true;
                        case Globals.eSpecies.Unknown:
                        default:
                            break;
                    }
                    #endregion

                    #region If NPC has spells in spelllist with NO PREP required, or unlimited cast, prepare random spell?
                    if (npc.spellDictionary != null && npc.spellDictionary.Count > 0 &&
                        (npc.castMode == CastMode.NoPrep || npc.castMode == CastMode.Unlimited) &&
                        !npc.IsHybrid)
                    {
                        // get random spell
                        npc.preppedSpell = GameSpell.GetSpell(GameSpell.GetRandomSpellID(npc.spellDictionary));
                        int attempts = 0;
                        while (npc.preppedSpell.IsBeneficial && attempts < 10)
                        {
                            npc.preppedSpell = GameSpell.GetSpell(GameSpell.GetRandomSpellID(npc.spellDictionary));
                            attempts++;
                        }

                        if (npc.preppedSpell.IsBeneficial)
                        {
                            npc.preppedSpell = null;
                            return false;
                        }

                        return true;
                    }
                    #endregion

                    // Currently animals don't make tough decisions, and should always have harmful spells (salamander firewall)
                    if (npc.animal && npc.spellDictionary.Count > 0)
                    {
                        npc.preppedSpell = GameSpell.GetSpell(GameSpell.GetRandomSpellID(npc.spellDictionary));
                        int attempts = 0;
                        while (npc.preppedSpell.IsBeneficial && attempts < 10)
                        {
                            npc.preppedSpell = GameSpell.GetSpell(GameSpell.GetRandomSpellID(npc.spellDictionary));
                            attempts++;
                        }

                        if (npc.preppedSpell.IsBeneficial)
                        {
                            npc.preppedSpell = null;
                            return false;
                        }

                        return true;
                    }

                    int distance = Cell.GetCellDistance(npc.X, npc.Y, npc.MostHated.X, npc.MostHated.Y);

                    int chosenSpellID = -1;

                    string[] chant; // chant used to warm the AI's chosen spell

                    // Memorized spell chant.
                    if(!string.IsNullOrEmpty(npc.MemorizedSpellChant) &&
                        SpellAvailabilityCheck(npc, npc.MemorizedSpellChant, out string spellCommand) && Rules.RollD(1, 100) >= 40 - npc.Level)
                    {
                        ExecuteAction(npc, ActionType.Cast, Priority.SpellSlingMemorizedChant);
                        return true;
                    }

                    // Override for all classes. Summon Humanoid.
                    if (npc.Pets.Count < GameSpell.MAX_PETS && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Summon_Humanoid) && Rules.CheckPerception(npc) && Rules.RollD(1, 100) >= 55)
                    {
                        chant = npc.spellDictionary[(int)GameSpell.GameSpellID.Summon_Humanoid].Split(" ".ToCharArray());
                        CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                        return true;
                    }

                    switch (npc.BaseProfession)
                    {
                        case ClassType.Ranger:
                            {
                                #region Rangers
                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Stoneskin) && !npc.HasEffect(Effect.EffectTypes.Stoneskin))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Stoneskin;
                                    npc.BuffTargetID = npc.UniqueID;
                                    npc.buffSpellCommand = GameSpell.GameSpellID.Stoneskin.ToString().ToLower();
                                }
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Hunter__s_Mark) && !npc.HasEffect(Effect.EffectTypes.Hunter__s_Mark) && npc.RightHand != null)
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Hunter__s_Mark;
                                    npc.BuffTargetID = npc.UniqueID;
                                    npc.buffSpellCommand = GameSpell.GameSpellID.Hunter__s_Mark.ToString().ToLower();
                                }
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Faerie_Fire) && !npc.MostHated.HasEffect(Effect.EffectTypes.Faerie_Fire) && Rules.RollD(1, 20) >= 10)
                                    chosenSpellID = (int)GameSpell.GameSpellID.Faerie_Fire;
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Ensnare) && !npc.MostHated.HasEffect(Effect.EffectTypes.Ensnare) && Rules.RollD(1, 20) > 8)
                                    chosenSpellID = (int)GameSpell.GameSpellID.Ensnare;
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Summon_Nature__s_Ally) && npc.Pets.Count < GameSpell.MAX_PETS && npc.enemyList.Count >= Rules.Dice.Next(6))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Summon_Nature__s_Ally;
                                }

                                // give non spell casting pets Trochilidae (haste) and casting pets Ataraxia (mana regen)
                                if (chosenSpellID <= -1 && npc.Pets != null && npc.Pets.Count > 0)
                                {
                                    foreach (NPC pet in npc.Pets)
                                    {
                                        if (!SpellAvailabilityCheck(pet, (int)GameSpell.GameSpellID.Barkskin))
                                        {
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Barkskin) && !pet.HasEffect(Effect.EffectTypes.Barkskin) && !pet.HasEffect(Effect.EffectTypes.Stoneskin))
                                            {
                                                chosenSpellID = (int)GameSpell.GameSpellID.Barkskin;
                                                npc.BuffTargetID = pet.UniqueID;
                                                npc.buffSpellCommand = GameSpell.GameSpellID.Barkskin.ToString().ToLower();
                                            }
                                        }
                                        else if (!SpellAvailabilityCheck(pet, (int)GameSpell.GameSpellID.Hunter__s_Mark))
                                        {
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Hunter__s_Mark) && !pet.HasEffect(Effect.EffectTypes.Hunter__s_Mark))
                                            {
                                                chosenSpellID = (int)GameSpell.GameSpellID.Hunter__s_Mark;
                                                npc.BuffTargetID = pet.UniqueID;
                                                npc.buffSpellCommand = "huntersmark";
                                            }
                                        }

                                        if (chosenSpellID > -1) break;
                                    }
                                }

                                if (chosenSpellID > -1)
                                {
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                break; 
                                #endregion
                            }
                        case ClassType.Druid:
                            {
                                #region Druids
                                #region Light vs undead
                                if (npc.MostHated.IsUndead && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Light))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Light;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                #endregion
                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Stoneskin) && !npc.HasEffect(Effect.EffectTypes.Stoneskin))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Stoneskin;
                                    npc.BuffTargetID = npc.UniqueID;
                                    npc.buffSpellCommand = GameSpell.GameSpellID.Stoneskin.ToString().ToLower();
                                }
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Cynosure) && !npc.MostHated.HasEffect(Effect.EffectTypes.Cynosure) && Rules.RollD(1, 20) < 10)
                                    chosenSpellID = (int)GameSpell.GameSpellID.Cynosure;
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Faerie_Fire) && !npc.MostHated.HasEffect(Effect.EffectTypes.Faerie_Fire) && Rules.RollD(1, 20) < 10)
                                    chosenSpellID = (int)GameSpell.GameSpellID.Faerie_Fire;
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Ensnare) && !npc.MostHated.HasEffect(Effect.EffectTypes.Ensnare) && Rules.RollD(1, 20) <= 5)
                                    chosenSpellID = (int)GameSpell.GameSpellID.Ensnare;
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Charm_Animal) && EntityLists.ANIMAL.Contains(npc.MostHated.entity) && npc.Pets.Count < GameSpell.MAX_PETS && npc.MostHated.Level <= npc.Level + 4)
                                    chosenSpellID = (int)GameSpell.GameSpellID.Charm_Animal;
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Summon_Nature__s_Ally) && npc.Pets.Count < GameSpell.MAX_PETS && npc.enemyList.Count >= Rules.Dice.Next(6))
                                    chosenSpellID = (int)GameSpell.GameSpellID.Summon_Nature__s_Ally;
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Trochilidae) && !npc.HasEffect(Effect.EffectTypes.Trochilidae))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Trochilidae;
                                    npc.BuffTargetID = npc.UniqueID;
                                    npc.buffSpellCommand = GameSpell.GameSpellID.Trochilidae.ToString().ToLower();
                                }
                                else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Iceshard) && !npc.MostHated.immuneCold && !EntityLists.IMMUNE_COLD.Contains(npc.MostHated.entity) && npc.enemyList.Count > Rules.Dice.Next(6))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Iceshard;
                                }

                                // give non spell casting pets Trochilidae (haste) and casting pets Ataraxia (mana regen)
                                if (chosenSpellID <= -1 && npc.Pets != null && npc.Pets.Count > 0)
                                {
                                    foreach (NPC pet in npc.Pets)
                                    {
                                        if (!pet.IsSpellWarmingProfession && !SpellAvailabilityCheck(pet, (int)GameSpell.GameSpellID.Trochilidae))
                                        {
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Trochilidae) && !pet.HasEffect(Effect.EffectTypes.Trochilidae))
                                            {
                                                chosenSpellID = (int)GameSpell.GameSpellID.Trochilidae;
                                                npc.BuffTargetID = pet.UniqueID;
                                                npc.buffSpellCommand = GameSpell.GameSpellID.Trochilidae.ToString().ToLower();
                                            }
                                        }
                                        else if (pet.IsSpellWarmingProfession && !SpellAvailabilityCheck(pet, (int)GameSpell.GameSpellID.Ataraxia))
                                        {
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Ataraxia) && !pet.HasEffect(Effect.EffectTypes.Ataraxia))
                                            {
                                                chosenSpellID = (int)GameSpell.GameSpellID.Ataraxia;
                                                npc.BuffTargetID = pet.UniqueID;
                                                npc.buffSpellCommand = GameSpell.GameSpellID.Ataraxia.ToString().ToLower();
                                            }
                                        }
                                        else if (!pet.HasEffect(Effect.EffectTypes.Stoneskin) && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Stoneskin))
                                        {
                                            chosenSpellID = (int)GameSpell.GameSpellID.Stoneskin;
                                            npc.BuffTargetID = pet.UniqueID;
                                            npc.buffSpellCommand = GameSpell.GameSpellID.Stoneskin.ToString().ToLower();
                                        }
                                        else if (!pet.HasEffect(Effect.EffectTypes.Stoneskin) && !pet.HasEffect(Effect.EffectTypes.Barkskin))
                                        {
                                            chosenSpellID = (int)GameSpell.GameSpellID.Barkskin;
                                            npc.BuffTargetID = pet.UniqueID;
                                            npc.buffSpellCommand = GameSpell.GameSpellID.Barkskin.ToString().ToLower();
                                        }

                                        if (chosenSpellID > -1) break;
                                    }
                                }

                                if (chosenSpellID > -1)
                                {
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                break; 
                                #endregion
                            }
                        case ClassType.Thaumaturge: // thaums typically want to cast either evocation or alteration non beneficial
                            {
                                #region Thaumaturges

                                #region Turn Undead vs undead
                                if (npc.MostHated.IsUndead && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Turn_Undead))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Turn_Undead;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }

                                if (npc.MostHated.IsUndead && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Light))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Light;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                #endregion

                                #region Banish vs summoned
                                if (npc.MostHated is NPC hatedNPC && hatedNPC.IsSummoned && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Banish))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Banish;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                #endregion

                                GameSpell stunSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Stun);

                                #region if target is stunned, blind, or feared choose an evocation spell
                                // mostHated is stunned or feared -- cast curse or death
                                if (npc.MostHated.Stunned > 0 || npc.MostHated.IsFeared || npc.MostHated.IsBlind)
                                {
                                    if ((SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Create_Snake) || SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Summon_Phantasm))
                                        && npc.Pets.Count <= 0) // CURRENTLY ONLY ONE PET IS SUMMONED
                                    {
                                        if (Rules.RollD(1, 100) < Rules.Dice.Next(50, 66))
                                            goto conjuration;
                                        else goto evocation;
                                    }
                                    else goto evocation;
                                }
                                // mostHated is NOT stunned, feared or blind -- 90+% chance to see if we can stun, fear or blind
                                else if (npc.alterationHarmfulSpells.Count > 0 && Rules.RollD(1, 100) >= Rules.Dice.Next(90, 101))
                                {
                                    goto alteration;
                                }
                                #endregion

                                // summon an ally most of the time if this NPC is not already a pet of some sort
                                if(npc.PetOwner == null && (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Create_Snake) ||
                                    SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Summon_Phantasm)) && npc.Pets.Count < GameSpell.MAX_PETS)
                                {
                                    if (Rules.RollD(1, 100) < Rules.Dice.Next(50, 66))
                                        goto conjuration;
                                }

                                #region if target is in my cell try a harmful alteration spell (eg: blind, fear, stun)
                                if (distance == 0 &&  // if the enemy is next to us try to alter it (eg: fear) 
                                    (npc.MostHated.Stunned <= 0 && !npc.MostHated.IsFeared && !npc.MostHated.IsBlind))
                                {
                                    if (Rules.RollD(1, 100) >= Rules.Dice.Next(80, 101)) // 80+% chance to use a form of alteration such as blind or fear
                                    {
                                        goto alteration;
                                    }
                                    // otherwise move down to evocation
                                }
                            #endregion

                            evocation:
                                #region evocation
                                if (npc.evocationSpells.Count > 0)
                                {
                                    chosenSpellID = GameSpell.GetRandomSpellID(npc.evocationSpells);
                                    if (!SpellAvailabilityCheck(npc, chosenSpellID))  // could not find a suitable evocation spell
                                    {
                                        goto alteration;
                                    }

                                    #region only cast Banish if the target is summoned
                                    if (chosenSpellID == GameSpell.GetSpell("banish").ID)
                                    {
                                        if (!((npc.MostHated is NPC) && (npc.MostHated as NPC).IsSummoned)) { goto evocation; }
                                    }
                                    #endregion

                                    #region choose Death over Curse
                                    if (chosenSpellID == GameSpell.GetSpell("curse").ID)
                                    {
                                        GameSpell deathSpell = GameSpell.GetSpell("death");
                                        if (npc.evocationSpells.ContainsKey(deathSpell.ID) && npc.HasManaAvailable(deathSpell.ID))
                                        {
                                            if (Rules.CheckPerception(npc) && (npc.MostHated.Hits < Skills.GetSkillLevel(npc.magic) * GameSpell.CURSE_SPELL_MULTIPLICAND_NPC))
                                            {
                                                // do nothing - leave chosen spell as Curse spell 
                                            }
                                            else chosenSpellID = deathSpell.ID;
                                        }
                                    }
                                    #endregion

                                    chant = npc.evocationSpells[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                            #endregion
                            alteration:
                                #region alteration
                                if (npc.alterationHarmfulSpells.Count > 0)
                                {
                                    // there are allies nearby, blind or stun the target if we can
                                    if (npc.friendList.Count > 0)
                                    {
                                        // thaumaturge has both blind and stun spell
                                        if (npc.alterationHarmfulSpells.ContainsKey((int)GameSpell.GameSpellID.Blind) && npc.alterationHarmfulSpells.ContainsKey((int)GameSpell.GameSpellID.Stun))
                                        {
                                            // 50% chance to choose stun or blind spell (this could probably be reworked 
                                            if (Rules.RollD(1, 100) < 50 && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Blind))
                                                chosenSpellID = (int)GameSpell.GameSpellID.Blind;
                                            else if(SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Stun)) chosenSpellID = (int)GameSpell.GameSpellID.Stun;
                                        }
                                        else
                                        {
                                            if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Blind))
                                                chosenSpellID = (int)GameSpell.GameSpellID.Blind;
                                            else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Stun))
                                                chosenSpellID = (int)GameSpell.GameSpellID.Stun;
                                            else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Fear))
                                                chosenSpellID = (int)GameSpell.GameSpellID.Fear;
                                        }
                                    }
                                    // else if no allies nearby, fear the target if we can
                                    else if (npc.friendList.Count == 0 && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Fear))
                                    {
                                        chosenSpellID = (int)GameSpell.GameSpellID.Fear;
                                    }
                                    // otherwise choose a random harmful alteration spell 
                                    else
                                    {
                                        chosenSpellID = GameSpell.GetRandomSpellID(npc.alterationHarmfulSpells);
                                    }

                                    if (!SpellAvailabilityCheck(npc, chosenSpellID)) { goto conjuration; } // could not find a suitable harmful alteration spell

                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                            #endregion
                            conjuration:
                                #region conjuration
                                if (npc.conjurationSpells.Count > 0)
                                {
                                    // summon phantasm
                                    if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Summon_Phantasm) && npc.Pets.Count < GameSpell.MAX_PETS)
                                    {
                                        chosenSpellID = (int)GameSpell.GameSpellID.Summon_Phantasm;
                                        chant = npc.conjurationSpells[chosenSpellID].Split(" ".ToCharArray());
                                        CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                        return true;
                                    }
                                    // summon snakes
                                    if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Create_Snake) && npc.Pets.Count < GameSpell.MAX_PETS)
                                    {
                                        chosenSpellID = (int)GameSpell.GameSpellID.Create_Snake;
                                        chant = npc.conjurationSpells[chosenSpellID].Split(" ".ToCharArray());
                                        CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                        return true;
                                    }
                                }
                                break;
                                #endregion
                                #endregion
                            }
                        case ClassType.Wizard:
                            {
                                #region Wizards

                                #region Light vs undead
                                if (npc.MostHated.IsUndead && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Light))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Light;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                #endregion

                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Lightning_Lance))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Lightning_Lance;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }

                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Icespear))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Icespear;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }

                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Firebolt) && !Rules.CheckPerception(npc))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Firebolt;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }

                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Magic_Missile) && npc.enemyList.Count == 1)
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Magic_Missile;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }

                                #region Wizards that have nightvision will cast darkness.
                                if (npc.alterationHarmfulSpells.ContainsKey((int)GameSpell.GameSpellID.Darkness) && npc.HasNightVision)
                                {
                                    // if the area is already dark, cast some sort of direct damage if we know an evocation spell
                                    if (npc.MostHated.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || npc.MostHated.CurrentCell.IsAlwaysDark)
                                    {
                                        if (npc.evocationSpells.Count > 0) { goto directDamage; }
                                        else return false;
                                    }

                                    if (npc.HasManaAvailable((int)GameSpell.GameSpellID.Darkness))
                                    {
                                        chant = npc.alterationHarmfulSpells[(int)GameSpell.GameSpellID.Darkness].Split(" ".ToCharArray()); ;
                                        CommandTasker.ParseCommand((Character)npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                        return true;
                                    }
                                }
                                #endregion

                                #region Wizards will cast web if the mostHated is not in their current cell and not already in a web.

                                if (distance > 0 && npc.alterationHarmfulSpells.ContainsKey((int)GameSpell.GameSpellID.Create_Web) && npc.MostHated.CurrentCell.DisplayGraphic != Cell.GRAPHIC_WEB &&
                                    npc.MostHated.CurrentCell.DisplayGraphic != Cell.GRAPHIC_FIRE)
                                {
                                    if (!SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Create_Web)) { goto directDamage; }
                                    chant = npc.alterationHarmfulSpells[(int)GameSpell.GameSpellID.Create_Web].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                #endregion

                                #region If mostHated is not in a web, check perception to determine if wizard will cast direct damage over area effect.
                                if (npc.friendList.Count > 0 && Rules.CheckPerception(npc))
                                {
                                    for (int a = 0; a < npc.friendList.Count; a++)
                                    {
                                        Character ally = npc.friendList[a];

                                        // If our ally is near our mostHated, don't fry them. Go to direct damage.
                                        if (Cell.GetCellDistance(ally.X, ally.Y, npc.MostHated.X, npc.MostHated.Y) <= 1)
                                        {
                                            goto directDamage;
                                        }
                                    }
                                }
                                #endregion

                                #region If target is more than 1 cell away choose an area effect spell
                                if (distance > 1 && npc.evocationAreaEffectSpells.Count > 0)
                                {
                                    chosenSpellID = GameSpell.GetRandomSpellID(npc.evocationAreaEffectSpells);
                                    if (!SpellAvailabilityCheck(npc, chosenSpellID)) { goto directDamage; }
                                    chant = npc.evocationAreaEffectSpells[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                            #endregion

                            directDamage:
                                #region Wizards ultimately focus on direct damage spells.
                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Lightning_Lance))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Lightning_Lance;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                    
                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Icespear))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Icespear;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }

                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Firebolt) && !Rules.CheckPerception(npc))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Firebolt;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }

                                if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Magic_Missile))
                                {
                                    chosenSpellID = (int)GameSpell.GameSpellID.Magic_Missile;
                                    chant = npc.spellDictionary[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                else
                                {
                                    chosenSpellID = GameSpell.GetRandomSpellID(npc.evocationSpells);
                                    if (!SpellAvailabilityCheck(npc, chosenSpellID)) { return false; }
                                    chant = npc.evocationSpells[chosenSpellID].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                #endregion
                                #endregion
                            }
                        case ClassType.Sorcerer:
                            {
                                #region Sorcerers
                                // if hurting, use lifeleech
                                if (npc.Hits <= (.50 * npc.HitsFull) && !EntityLists.UNDEAD.Contains(npc.MostHated.entity) && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Lifeleech))
                                {
                                    chant = npc.spellDictionary[(int)GameSpell.GameSpellID.Lifeleech].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }// charm an animal
                                else if (npc.MostHated != null && (npc.MostHated.animal || EntityLists.ANIMAL.Contains(npc.MostHated.entity)) &&
                                    npc.Pets != null && npc.Pets.Count < GameSpell.MAX_PETS &&
                                    SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Charm_Animal))
                                {
                                    chant = npc.spellDictionary[(int)GameSpell.GameSpellID.Charm_Animal].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }// next step is to silence a caster
                                else if (!npc.MostHated.HasEffect(Effect.EffectTypes.Silence) && (npc.MostHated.IsSpellWarmingProfession || ((npc.MostHated is NPC) && (npc.MostHated as NPC).castMode == CastMode.Limited)) && !npc.MostHated.IsHybrid &&
                                    SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Power_Word___Silence))
                                {
                                    chant = npc.spellDictionary[(int)GameSpell.GameSpellID.Power_Word___Silence].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                else if (npc.MostHated != null && !npc.MostHated.EffectsList.ContainsKey(Effect.EffectTypes.Contagion) && Rules.RollD(1, 100) <= 50 && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Contagion))
                                {
                                    chant = npc.spellDictionary[(int)GameSpell.GameSpellID.Contagion].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                                // finally, use ghods hooks or acid orb
                                else
                                {
                                    if (Rules.RollD(1, 100) <= 50 && npc.MostHated != null && npc.MostHated.Stunned <= 0 && SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Ghod__s_Hooks))
                                    {
                                        chant = npc.spellDictionary[(int)GameSpell.GameSpellID.Ghod__s_Hooks].Split(" ".ToCharArray());
                                        CommandTasker.ParseCommand((Character)npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                        return true;
                                    }
                                    else if (SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Acid_Orb))
                                    {
                                        chant = npc.spellDictionary[(int)GameSpell.GameSpellID.Acid_Orb].Split(" ".ToCharArray());
                                        CommandTasker.ParseCommand((Character)npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                        return true;
                                    }
                                }
                                break;
                                #endregion
                            }
                        case ClassType.Thief:
                            {
                                if (npc.RightHand != null && npc.RightHand.venom <= 0 && npc.RightHand.IsPiercingWeapon() &&
                                    SpellAvailabilityCheck(npc, (int)GameSpell.GameSpellID.Venom))
                                {
                                    chant = npc.spellDictionary[(int)GameSpell.GameSpellID.Venom].Split(" ".ToCharArray());
                                    CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                    return true;
                                }
                            }
                            break;
                    }
                }
                #endregion
                else
                {
                    #region No enemy. Buff spell is already chosen.
                    if (npc.buffSpellCommand.Length > 0)
                    {
                        if (npc.castMode == CastMode.NoPrep)
                        {
                            npc.preppedSpell = GameSpell.GetSpell(npc.buffSpellCommand);
                            return true;
                        }

                        GameSpell buffSpell = null;

                        foreach (int spellID in npc.spellDictionary.Keys)
                        {
                            buffSpell = GameSpell.GetSpell(spellID);
                            if (buffSpell.Command == npc.buffSpellCommand)
                            {
                                string[] chant = npc.spellDictionary[buffSpell.ID].Split(" ".ToCharArray());
                                CommandTasker.ParseCommand(npc, chant[0], chant[1] + " " + chant[2] + " " + chant[3]);
                                npc.buffSpellCommand = "";
                                return true;
                            }
                        }
                        npc.buffSpellCommand = "";
                    }
                    #endregion
                }
            }
            return false;
        }

        public static void DoDeath(NPC npc)
        {
            if (IgnoredPivotItems.ContainsKey(npc.UniqueID))
                IgnoredPivotItems.Remove(npc.UniqueID);

            //if (ResistedSpells.ContainsKey(npc.UniqueID))
            //    ResistedSpells.Remove(npc.UniqueID);
        }

        private static void AddToIgnoredPivotItems(NPC npc, Item item)
        {
            if (npc == null || item == null) return;

            if (AI.IgnoredPivotItems.ContainsKey(npc.UniqueID))
            {
                if (!AI.IgnoredPivotItems[npc.UniqueID].Contains(item.itemID))
                {
                    AI.IgnoredPivotItems[npc.UniqueID].Add(item.itemID);
                }
                else
                {
                    // Log the AI has already tried to ignore this itemUniqueID.
                    Utils.Log(npc.GetLogString() + " attempted to add " + item.GetLogString() + " to AI.IgnoredPItems multiple times.", Utils.LogType.Debug);
                }
            }
            else
            {
                AI.IgnoredPivotItems.Add(npc.UniqueID, new List<int>() { item.itemID });
            }
        }

        public static bool SpellAvailabilityCheck(NPC npc, int gameSpellID)
        {
            if (npc.spellDictionary == null || npc.spellDictionary.Count <= 0) return false;

            if (gameSpellID == (int)GameSpell.GameSpellID.Summon_Humanoid && !Spells.SummonHumanoidSpell.SummonHumanoidAvailability.Contains(npc.entity))
                return false;

            // Unique entities get access to all spells.
            if (!npc.spellDictionary.ContainsKey(gameSpellID)) return false;

            if (!npc.HasManaAvailable(gameSpellID)) return false;

            return true;
        }

        public static bool SpellAvailabilityCheck(NPC npc, string spellChant, out string spellCommand)
        {
            spellCommand = "";
            if (npc.spellDictionary == null || npc.spellDictionary.Count <= 0) return false;

            foreach (int spellID in npc.spellDictionary.Keys)
            {
                if(npc.spellDictionary[spellID].ToLower() == spellChant.ToLower())
                {
                    spellCommand = GameSpell.GetSpell(spellID).Command;
                    return SpellAvailabilityCheck(npc, spellID);
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a talent is available to use. AI uses this to check if this talent will be formed when executing an action.
        /// </summary>
        /// <param name="npc"></param>
        /// <param name="talent"></param>
        /// <returns></returns>
        public static bool TalentAvailabilityCheck(NPC npc, Talents.GameTalent.TALENTS talent)
        {
            if (npc.talentsDictionary == null) return false;

            if (!npc.talentsDictionary.ContainsKey(talent.ToString().ToLower())) return false;

            Talents.GameTalent talentCheck = Talents.GameTalent.GetTalent(talent);

            if (!talentCheck.MeetsPerformanceCost(npc)) return false;

            // call the talent and check other requirements such as weapon type

            return true;
        }

        /// <summary>
        /// Check if a target has resisted a spell numberOfResistsCount times.
        /// </summary>
        /// <param name="casterUniqueID"></param>
        /// <param name="targetUniqueID"></param>
        /// <param name="spellID"></param>
        /// <param name="numberOfResistsCount">False if the target has not resisted the spell enough times to choose another.</param>
        /// <returns></returns>
        //private static bool TargetHasResistedOrSpellUnknown(Character caster, Character target, int spellID, int numberOfResistsCount)
        //{
        //    // safety nets
        //    if (target == null || caster == null) return true;

        //    if (caster.spellDictionary == null || !caster.spellDictionary.ContainsKey(spellID))
        //        return true;

        //    if (ResistedSpells.ContainsKey(caster.UniqueID) && ResistedSpells[caster.UniqueID].ContainsKey(target.UniqueID) &&
        //        ResistedSpells[caster.UniqueID][target.UniqueID].ContainsKey(spellID) && ResistedSpells[caster.UniqueID][target.UniqueID][spellID] >= numberOfResistsCount)
        //        return true;

        //    return false;
        //}
    }
}
