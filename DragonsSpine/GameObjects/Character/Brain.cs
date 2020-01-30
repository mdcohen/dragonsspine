using System;
using System.Collections;
using System.Collections.Generic;

namespace DragonsSpine
{
    /// <summary>
    /// 
    /// </summary>
    public class Brain
    {
        public enum ActionType { None, Take, Use, Cast, Special, Move, Combat, Follow }
        public enum CastMode { Never, Limited, Unlimited, NoPrep }

        public enum Priority
        {
            None,
            Wander, // wander around
            Socialize, // get to a friend
            Interact, // get to a friend
            SearchCorpse, // search through a corpse
            GetObject, // pick up a nearby valuable object
            Advance, // move closer to an enemy
            Attack, // attack an enemy
            Investigate, // mostHated is null, but wasn't a round ago so investigate where mostHated was
            PrepareSpell, // prepare a spell
            GetWeapon, // get a weapon, either here or somewhere in sight
            RangeMove, // move away for a ranged attack
            GoHome, // lair critters return home, quest Owners return to spawn coords
            RaiseDead, // priests raise the dead
            Buff,
            Rest, // rest (eg: cancel prepared spell)
            SpellSling, // cast a prepared spell
            Evaluate, // creature is acquiring a target
            FleeEffect, // flee from an effect in the Owner's cell
            Flee, // flee
            Enforce, // enforce the local law
            LairDefense, // defend our lair
            Heal // heal myself or ally
        }
        Point[] directions = {
            new Point(-1,-1), 
            new Point(-1, 0),
            new Point(-1, 1),
            new Point( 0,-1),
            new Point( 0, 1),
            new Point( 1,-1),
            new Point( 1, 0),
            new Point( 1, 1)
		    };
        #region Private
        protected NPC m_owner;
        protected int m_totalFearLove;
        protected int m_totalHate;
        protected List<Character> m_enemyList;
        protected List<Character> m_friendList;
        protected List<Character> m_targetList;
        protected List<Character> m_seenList;
        protected List<Cell> m_localCells;
        protected Character m_mostFeared;
        protected Character m_mostLoved;
        protected Character m_mostHated;
        protected Character m_previousMostHated;
        protected int[] m_fear;
        protected int[] m_hate;
        protected int m_hateCenterX;
        protected int m_hateCenterY;
        protected int m_fearCenterX;
        protected int m_fearCenterY;
        protected Item m_pItem;
        protected List<string> m_moveList;
        #endregion
        #region Public
        public Character MostFeared
        {
            get { return m_mostFeared; }
            set { m_mostFeared = value; }
        }
        public Character MostLoved
        {
            get { return m_mostLoved; }
            set { m_mostLoved = value; }
        }
        public Character MostHated
        {
            get { return m_mostHated; }
            set { m_mostHated = value; }
        }
        public Character PreviousMostHated
        {
            get { return m_previousMostHated; }
            set { m_previousMostHated = value; }
        }
        /// <summary>
        /// Gets the GameNPC owner of the brain.
        /// </summary>
        public NPC Owner
        {
            get { return m_owner; }
        }
        public List<string> MoveList
        {
            get { return m_moveList; }
            set { m_moveList = value; }
        }
        public List<Character> SeenList
        {
            get { return m_seenList; }
            set { m_seenList = value; }
        }
        public Item PreferredItem
        {
            get { return m_pItem; }
            set { m_pItem = value; }
        }
        #endregion

        public Brain(NPC owner)
        {
            m_owner = owner;
            m_totalFearLove = 0;
            m_totalHate = 0;
            m_enemyList = new List<Character>();
            m_friendList = new List<Character>();
            m_targetList = new List<Character>();
            m_localCells = new List<Cell>();
            m_moveList = new List<string>();
            m_seenList = new List<Character>();
        }
        public virtual void Think()
        {
            // Dead brains do not think.
            if (Owner.IsDead)
            {
                return;
            }
            lock (this)
            {
                CreateContactList();
            }
        }

        /// <summary>
        /// Create the arrays of the seen objects around the mob
        /// </summary>
        public virtual void CreateContactList()
        {
            try
            {
                if (Owner.IsDead) { return; } // return if the creature is dead
                if (Owner.CurrentCell == null) { return; } // return if the Owner does not have a cell.
                m_totalFearLove = 0;
                m_totalHate = 0;
                m_enemyList.Clear();
                m_friendList.Clear();
                m_targetList.Clear();
                m_localCells.Clear();
                m_seenList.Clear();
                Owner.seenList.Clear();

                Cell[] cellArray = Cell.GetVisibleCellArray(Owner.CurrentCell, 3);

                for (int j = 0; j < 49; j++)
                {
                    if (cellArray[j] == null || !Owner.CurrentCell.visCells[j] ||
                        ((cellArray[j].Effects.ContainsKey(Effect.EffectType.Darkness) ||
                         cellArray[j].IsAlwaysDark) && !Owner.HasNightVision))
                    {
                        // do nothing
                    }
                    else if ((Owner.CurrentCell.Effects.ContainsKey(Effect.EffectType.Darkness) ||
                        Owner.CurrentCell.IsAlwaysDark) && !Owner.HasNightVision)
                    {
                        // do nothing
                    }
                    else
                    {
                        #region create list of targets, friends and enemies
                        if (cellArray[j].Characters.Count > 0)
                        {
                            foreach (Character chr in new List<Character>(cellArray[j].Characters))
                            {
                                if (chr == null) continue;
                                if (chr != Owner && !chr.IsDead && !Owner.IsBlind)
                                {
                                    if (Rules.DetectHidden(chr, Owner) && Rules.DetectInvisible(chr, Owner)) // add detected hidden only
                                    {
                                        if (!chr.IsImmortal) // do not add immortal characters to target list
                                        {
                                            m_targetList.Add(chr); // add to visible target list
                                            m_seenList.Add(chr);
                                            Owner.seenList.Add(chr);
                                            if (Rules.DetectAlignment(chr, Owner))
                                                m_enemyList.Add(chr);
                                            else
                                                m_friendList.Add(chr);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion

                        if (Owner.Brain.GetCellCost(cellArray[j]) <= 2)
                            Owner.localCells.Add(cellArray[j]);
                    }
                }

                System.Collections.Generic.List<int> idToRemoveList;
                foreach (Character friend in new List<Character>(Owner.friendList))
                {
                    idToRemoveList = new System.Collections.Generic.List<int>();
                    foreach (int playerID in new List<int>(friend.PlayersFlagged))
                    {
                        PC pc = PC.GetOnline(playerID);

                        // player is no longer online, add to temp list and remove from flagged list
                        if (pc == null)
                        {
                            idToRemoveList.Add(playerID);
                            continue;
                        }

                        if (!Owner.PlayersFlagged.Contains(playerID))
                        {
                            Owner.PlayersFlagged.Add(playerID);
                        }
                    }

                    if (idToRemoveList.Count > 0)
                    {
                        foreach (int playerID in idToRemoveList)
                        {
                            friend.PlayersFlagged.Remove(playerID);
                            Owner.PlayersFlagged.Remove(playerID);
                        }
                    }
                }

                if (Owner.Brain.MostHated == null)
                    Owner.TargetName = "";

                if (Owner.QuestList.Count > 0)
                    EscortQuestLogic();

                AssignFearLove();

                RateActions();
            }
            catch (Exception e)
            {
                Utils.Log("Error in Create AI.ContactList(): " + Owner.GetLogString(), Utils.LogType.Exception);
                Utils.LogException(e);
            }
        }
        /// <summary>
        /// Logic for escort type quests
        /// </summary>
        public virtual void EscortQuestLogic()
        {
            return;
        }
        /// <summary>
        /// Assigns the fear / love relationships for the mob
        /// </summary>
        public virtual void AssignFearLove()
        {
            int distance = 0;
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
            m_mostFeared = null;
            m_mostLoved = null;
            m_mostHated = null;
            Character target;
            m_fear = new int[m_targetList.Count];
            m_hate = new int[m_targetList.Count];
            if (Owner.animal || Owner.IsUndead)
            {
                #region Animal Instinct Fear / Love / Hate / Centers
                // friends: (1000 * (friend->perceivedStrength() / own->perceivedStrength())) / distance
                // enemies: (-1000 * (enemy->perceivedStrength() / own->perceivedStrength())) / distance
                for (int a = 0; a < m_targetList.Count; a++)
                {
                    target = m_targetList[a];

                    if (target != null)
                    {
                        distance = Cell.GetCellDistance(Owner.X, Owner.Y, target.X, target.Y) + 1;

                        // If it is an enemy...
                        if (Rules.DetectAlignment(target, Owner as Character))
                        {
                            m_fear[a] = (int)(-1000 * (float)(GetPerceivedStrength(target) / GetPerceivedStrength(Owner as Character))) / distance;

                            if (Owner.IsUndead)
                            {
                                m_hate[a] = 10000 / distance;
                            }
                            else if (Owner.animal)
                            {
                                m_hate[a] = 2300 / distance;
                            }
                            else
                            {
                                m_hate[a] = 1800 / distance;
                            }

                            m_totalHate += m_hate[a];

                            if (m_hate[a] > highhate)
                            {
                                highhate = m_hate[a];
                                m_mostHated = target;
                            }
                        }
                        else
                        {
                            m_fear[a] = (int)(1000 * (float)(Brain.GetPerceivedStrength(target) / Brain.GetPerceivedStrength(Owner as Character))) / distance;
                            if (target.Name == Owner.FollowName)
                            {
                                m_fear[a] = 100000;
                            }
                        }
                        if (m_fear[a] > highfear)
                        {
                            highfear = m_fear[a];
                            m_mostLoved = target;
                        }
                        if (m_fear[a] < lowfear)
                        {
                            lowfear = m_fear[a];
                            m_mostFeared = target;
                        }

                        m_totalFearLove += m_fear[a];
                    }
                    // fear / love
                    try
                    {
                        if (m_mostLoved != null || m_mostFeared != null)
                        {
                            minFL = Math.Min(Math.Abs(m_totalFearLove), Math.Abs(m_fear[a]));
                            maxFL = Math.Max(Math.Abs(m_totalFearLove), Math.Abs(m_fear[a]));
                            fearLoveX += (int)((target.X - fearLoveX) * ((float)minFL / maxFL));
                            fearLoveY += (int)((target.Y - fearLoveY) * ((float)minFL / maxFL));
                            m_fearCenterX = fearLoveX;
                            m_fearCenterY = fearLoveY;
                        }
                    }
                    catch
                    {
                        Utils.Log("AI (animal fear/love): " + Owner, Utils.LogType.SystemFailure);

                        if (World.GetFacetByID(Owner.FacetID).Spawns.ContainsKey(Owner.SpawnZoneID))
                        {
                            World.GetFacetByID(Owner.FacetID).Spawns[Owner.SpawnZoneID].NumberInZone -= 1;
                        }
                        Owner.RemoveFromWorld();
                        return;
                    }
                    // hate
                    try
                    {
                        if (Owner.Brain.MostHated != null)
                        {
                            minH = Math.Min(m_totalHate, m_hate[a]);
                            maxH = Math.Max(m_totalHate, m_hate[a]);
                            hateX += (int)((target.X - hateX) * ((float)minH / maxH));
                            hateY += (int)((target.Y - hateY) * ((float)minH / maxH));
                            m_hateCenterX = hateX;
                            m_hateCenterY = hateY;
                        }
                    }
                    catch
                    {
                        Utils.Log("AI (animal hate): " + Owner, Utils.LogType.SystemFailure);
                        if (World.GetFacetByID(Owner.FacetID).Spawns.ContainsKey(Owner.SpawnZoneID))
                        {
                            World.GetFacetByID(Owner.FacetID).Spawns[Owner.SpawnZoneID].NumberInZone -= 1;
                        }
                        Owner.RemoveFromWorld();
                        return;
                    }
                }
                #endregion
            }
            else
            {
                #region Mind Driven Fear / Love / Hate / Centers for entities seen
                for (int a = 0; a < m_targetList.Count; a++)
                {
                    target = m_targetList[a];
                    if (target != null)
                    {
                        distance = Cell.GetCellDistance(Owner.X, Owner.Y, target.X, target.Y) + 1;
                        if (Rules.DetectAlignment(target, Owner))
                        {
                            m_fear[a] = (int)((-500 * ((float)Brain.GetPerceivedStrength(target) / Brain.GetPerceivedStrength(Owner as Character))) +
                                (-500 * ((float)Brain.GetPerceivedDanger(target) / Brain.GetPerceivedDanger(Owner as Character)))) / distance;

                            m_hate[a] = 1800 / distance;

                            m_totalHate += m_hate[a];

                            if (m_hate[a] > highhate)
                            {
                                highhate = m_hate[a];
                                m_mostHated = target;
                            }
                        }
                        else
                        {
                            if (Owner.Name == target.Name)
                            {
                                m_fear[a] = (int)((500 * ((float)Brain.GetPerceivedStrength(target) / Brain.GetPerceivedStrength(Owner as Character))) +
                                    (500 * ((float)Brain.GetPerceivedDanger(target) / Brain.GetPerceivedDanger(Owner as Character)))) / distance;
                                if (target.Name == Owner.FollowName)
                                {
                                    m_fear[a] = 100000;
                                }
                            }
                            else
                            {
                                m_fear[a] = 0;
                            }

                        }
                        if (m_fear[a] > highfear)
                        {
                            highfear = m_fear[a];
                            m_mostLoved = target;
                        }
                        if (m_fear[a] < lowfear)
                        {
                            lowfear = m_fear[a];
                            m_mostFeared = target;
                        }
                        m_totalFearLove += m_fear[a];
                    }
                    // fear / love
                    try
                    {
                        if (m_mostLoved != null || m_mostFeared != null)
                        {
                            minFL = Math.Min(Math.Abs(m_totalFearLove), Math.Abs(m_fear[a]));
                            maxFL = Math.Max(Math.Abs(m_totalFearLove), Math.Abs(m_fear[a]));
                            fearLoveX += (int)((target.X - fearLoveX) * ((float)minFL / maxFL));
                            fearLoveY += (int)((target.Y - fearLoveY) * ((float)minFL / maxFL));
                            m_fearCenterX = fearLoveX;
                            m_fearCenterY = fearLoveY;
                        }
                    }
                    catch
                    {
                        Utils.Log("AI (creature fear/love): " + Owner, Utils.LogType.SystemFailure);
                        if (World.GetFacetByID(Owner.FacetID).Spawns.ContainsKey(Owner.SpawnZoneID))
                        {
                            World.GetFacetByID(Owner.FacetID).Spawns[Owner.SpawnZoneID].NumberInZone -= 1;
                        }
                        Owner.RemoveFromWorld();
                    }
                    // hate
                    try
                    {
                        if (m_mostHated != null && m_hate[a] > 0)
                        {
                            minH = Math.Min(m_totalHate, m_hate[a]);
                            maxH = Math.Max(m_totalHate, m_hate[a]);
                            hateX += (int)((target.X - hateX) * ((float)minH / maxH));
                            hateY += (int)((target.Y - hateY) * ((float)minH / maxH));
                            m_hateCenterX = hateX;
                            m_hateCenterY = hateY;
                        }
                    }
                    catch
                    {
                        Utils.Log("AI (creature hate): " + Owner, Utils.LogType.SystemFailure);
                        if (World.GetFacetByID(Owner.FacetID).Spawns.ContainsKey(Owner.SpawnZoneID))
                        {
                            World.GetFacetByID(Owner.FacetID).Spawns[Owner.SpawnZoneID].NumberInZone -= 1;
                        }
                        Owner.RemoveFromWorld();
                    }
                }
                #endregion
            }

            if (m_fearCenterX == 0 && m_fearCenterY == 0)
            {
                m_fearCenterX = Owner.X;
                m_fearCenterY = Owner.Y;
            }
        }
        /// <summary>
        /// figure out how strong/healthy something appears to be
        /// </summary>
        /// <param name="target">target to be looked at</param>
        /// <returns>float of the strength</returns>
        protected static float GetPerceivedStrength(Character target)
        {
            int strength = target.Strength;

            int modifier = target.GetWeaponSkillLevel(target.RightHand);

            float health = (float)(target.Hits / target.HitsMax);

            return (strength * health) + modifier;
        }
        /// <summary>
        /// FIgure out how dangerous something appears to be
        /// </summary>
        /// <param name="target">target to be looked at</param>
        /// <returns>int of the danger</returns>
        protected static int GetPerceivedDanger(Character target)
        {
            int danger = 0;

            Item torso = target.GetInventoryItem(Globals.eWearLocation.Torso);

            if (torso != null)
                danger = (int)torso.armorClass * 100;

            if (target.RightHand != null)
            {
                if (target.RightHand.baseType == Globals.eItemBaseType.Bow)
                {
                    danger += 400;
                    if (target.RightHand.nocked)
                        danger += 100;
                }
                danger += 50;
            }

            if (danger == 0) { danger = 50; } 

            return danger;
        }

        /// <summary>
        /// Does the mob have mana available to cast a spell
        /// </summary>
        /// <param name="spellID">ID of the Spell</param>
        /// <returns>true/False</returns>
        public bool HasManaAvailable(int spellID)
        {
            if (Owner.castMode == NPC.CastMode.Unlimited)
            {
                return true;
            }

            if (Owner.Mana >= Spell.GetSpell(spellID).ManaCost)
            {
                return true;
            }

            return false;
        }
        /// <summary>
        /// Does the mob have mana available to cast a spell
        /// </summary>
        /// <param name="spellCommand">Name of the spell</param>
        /// <returns>True/False</returns>
        public bool HasManaAvailable(string spellCommand)
        {
            if (Owner.castMode == NPC.CastMode.Unlimited)
            {
                return true;
            }

            if (Owner.Mana >= Spell.GetSpell(spellCommand).ManaCost)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Rate actions based on priority then execute the highest priority action
        /// </summary>
        public virtual void RateActions()
        {
            Priority cur_pri = Priority.None;
            ActionType action = ActionType.None;
            Priority new_pri = Priority.None;
            if (!Owner.animal) // if creature is not an animal
            {
                new_pri = rate_TAKE();
                if (new_pri > cur_pri)
                {
                    action = ActionType.Take; // pickup
                    cur_pri = new_pri;
                }
                new_pri = rate_USE();
                if (new_pri > cur_pri)
                {
                    action = ActionType.Use;
                    cur_pri = new_pri;
                }
            }
            new_pri = rate_CAST();
            if (new_pri > cur_pri)
            {
                action = ActionType.Cast;
                cur_pri = new_pri;
            }
            new_pri = rate_SPECIAL();
            if (new_pri > cur_pri)
            {
                action = ActionType.Special;
                cur_pri = new_pri;
            }
            new_pri = rate_MOVE();
            if (new_pri > cur_pri)
            {
                action = ActionType.Move; // move
                cur_pri = new_pri;
            }
            new_pri = rate_COMBAT();
            if (new_pri > cur_pri)
            {
                action = ActionType.Combat; // attack
                cur_pri = new_pri;
            }
            //Perform the rated action
            ExecuteAction(action, cur_pri);
            return;
        }
        /// <summary>
        /// rate the priority of the TAKE action
        /// </summary>
        /// <returns>Priority of action</returns>
        public virtual Priority rate_TAKE()
        {
            return Priority.None;
        }
        /// <summary>
        /// rate the priority of the USE action
        /// </summary>
        /// <returns>Priority of action</returns>
        public virtual Priority rate_USE()
        {
            return Priority.None;
        }
        /// <summary>
        /// rate the priority of the CAST action
        /// </summary>
        /// <returns>Priority of action</returns>
        public virtual Priority rate_CAST()
        {
            return Priority.None;
        }
        /// <summary>
        /// rate the priority of the SPECIAL action
        /// </summary>
        /// <returns>Priority of action</returns>
        public virtual Priority rate_SPECIAL()
        {
            return Priority.None;
        }
        /// <summary>
        /// rate the priority of the MOVE action
        /// </summary>
        /// <returns>Priority of action</returns>
        public virtual Priority rate_MOVE()
        {
            return Priority.None;
        }
        /// <summary>
        /// rate the priority of the COMBAT action
        /// </summary>
        /// <returns>Priority of action</returns>
        public virtual Priority rate_COMBAT()
        {
            return Priority.None;
        }
        /// <summary>
        /// Calls the different actionTypes based on the return of the rateActions function
        /// </summary>
        /// <param name="action">actionType to be executed</param>
        /// <param name="pri">Priority of the actionType</param>
        public virtual void ExecuteAction(ActionType action, Priority pri)
        {
        }
        #region Movement Related
        public void Move()
        {
            if (!Owner.IsMobile) { return; }

            MoveList.Clear();

            m_localCells.Clear();

            try
            {
                if (Owner.CurrentCell == null)
                {
                    Utils.Log("CurrentCell is null in Creature.Move for " + Owner.ToString(), Utils.LogType.Unknown);
                }

                List<Cell> cList = Map.GetAdjacentCells(Owner.CurrentCell, Owner);

                if (cList != null)
                {
                    Utilities.Dice dice = new DragonsSpine.Utilities.Dice(1, cList.Count);
                    int rand = dice.Roll() - 1;
                    Cell nCell = (Cell)cList[rand];
                    AIGotoXYZ(nCell.X, nCell.Y, nCell.Z);
                }
            }
            catch (Exception e)
            {
                Utils.Log("Error on Brain.Move()", Utils.LogType.Unknown);
                Utils.LogException(e);
                return;
            }
            return;
        }
        public void AIGotoXYZ(int x, int y, int z)
        {
            if (Owner.X == x && Owner.Y == y && Owner.Z == z) { return; } // bail out if we aren't going anywhere

            if (MoveList.Count <= 0)
            {
                if (BuildMoveList(x, y, z)) // search for target and build a move list
                {
                    DoNextListMove();
                }
                else
                {
                    this.Move(); // search algorithm was unable to reach target so do something random
                }
            }
            else
            {
                DoNextListMove();
            }
        }
        public bool BackAwayFromCell(Cell cellToBackAwayFrom)
        {
            ArrayList directions = new ArrayList();

            Map.Direction targetDirection = Map.GetDirection(Owner.CurrentCell, cellToBackAwayFrom);

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
                    List<Cell> cList = Map.GetAdjacentCells(Owner.CurrentCell, Owner);
                    if (cList != null)
                    {
                        int rand = Rules.dice.Next(cList.Count);
                        Cell nCell = (Cell)cList[rand];
                        Owner.Brain.AIGotoXYZ(nCell.X, nCell.Y, nCell.Z);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
            }

            for (int a = 0; a < directions.Count; a++)
            {
                Cell cell = Map.GetCellRelevantToCell(Owner.CurrentCell, directions[a].ToString().ToLower(), true);
                if (Owner.GetCellCost(cell) <= 2)
                {
                    //Owner.AIGotoXYZ(cell.X, cell.Y, cell.Z);
                    return true;
                }
            }
            return false;
        }
        protected void DoNextListMove()
        {
            for (int i = 0; i < Owner.Speed && MoveList.Count != 0; i++)
            {
                if (MoveList[0].Equals(null) || MoveList[0].Equals(""))
                {
                    break;
                }

                Command.ParseCommand(Owner, (string)MoveList[0], "");

                if (MoveList.Count > 0)
                {
                    MoveList.RemoveAt(0);

                    if (MoveList.Count == 0)
                    {
                        try
                        {
                            if ((!Owner.animal && !Owner.IsUndead && !Owner.IsSpectral && Owner.Brain.MostHated == null && Rules.CheckPerception(Owner)))//|| Owner.HasPatrol)
                            {
                                Cell cell = null;

                                for (int xpos = -1; xpos <= 1; xpos++)
                                {
                                    for (int ypos = -1; ypos <= 1; ypos++)
                                    {
                                        if (Cell.CellRequestInBounds(Owner.CurrentCell, xpos, ypos))
                                        {
                                            cell = Cell.GetCell(Owner.FacetID, Owner.LandID, Owner.MapID, Owner.X + xpos, Owner.Y + ypos, Owner.Z);

                                            if (cell != null)
                                            {
                                                if (cell.IsOpenDoor &&
                                                    Cell.GetCellDistance(Owner.X, Owner.Y, cell.X, cell.Y) == 1 &&
                                                    cell.Items.Count == 0 &&
                                                    cell.Characters.Count == 0)
                                                {
                                                    Command.ParseCommand(Owner, "close", "door " + Map.GetDirection(Owner.CurrentCell, cell).ToString().ToLower());
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Utils.LogException(e);
                            return;
                        }
                    }
                }
            }
        }
        public int GetCellCost(Cell cell)
        {
            int infinity = 10000;
            int cost;

            if (cell == null)
            {
                return infinity;
            }

            if (cell.Z != Owner.Z)
            {
                return infinity;
            }

            if (cell.IsLockedHorizontalDoor && !cell.Effects.ContainsKey(Effect.EffectType.Unlocked_Horizontal_Door))
            {
                return infinity;
            }

            if (cell.IsLockedVerticalDoor && !cell.Effects.ContainsKey(Effect.EffectType.Unlocked_Vertical_Door))
            {
                return infinity;
            }

            try
            {
                switch (cell.DisplayGraphic)
                {
                    case Cell.GRAPHIC_WATER:
                        if (Owner.IsWaterDweller)
                        {
                            cost = 1;
                        }
                        else if (Owner.CanFly)
                        {
                            if (Owner.Brain.MostHated != null)
                            {
                                cost = 1;
                            }
                            else cost = 5;
                        }
                        else cost = infinity;
                        break;
                    case Cell.GRAPHIC_AIR:
                        if (Owner.CanFly)
                        {
                            cost = 5;
                        }
                        else
                        {
                            cost = infinity;
                        }
                        break;
                    case Cell.GRAPHIC_WEB:
                        if (Owner.CanFly)
                        {
                            cost = 5;
                        }
                        else
                        {
                            if (cell.CellGraphic == Cell.GRAPHIC_WATER)
                                cost = infinity;
                            else
                            {
                                if (Owner.Brain.MostHated != null)
                                {
                                    cost = 2;
                                }
                                else
                                {
                                    cost = infinity;
                                }
                            }
                        }
                        break;
                    case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                    case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                    case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                    case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                        cost = 2;
                        break;
                    case Cell.GRAPHIC_ICE:
                        if (cell.Effects.ContainsKey(Effect.EffectType.Ice) ||
                            cell.Effects.ContainsKey(Effect.EffectType.Dragon__s_Breath_Ice))
                        {
                            if (Owner.immuneCold) // immune to fire damage
                            {
                                cost = 1;
                            }
                            else
                            {
                                if (Owner.Brain.MostHated != null)
                                {
                                    cost = 5;
                                }
                                else // do not move into fire if we're not going after an enemy
                                {
                                    cost = infinity;
                                }
                            }
                        }
                        else
                        {
                            cost = 1;
                        }
                        break;
                    case Cell.GRAPHIC_FIRE:
                        if (Owner.immuneFire) // immune to fire damage
                        {
                            cost = 1;
                        }
                        else
                        {
                            if (Owner.Brain.MostHated != null)
                            {
                                cost = 5;
                            }
                            else // do not move into fire if we're not going after an enemy
                            {
                                cost = infinity;
                            }
                        }
                        break;
                    // now the impassable cells
                    case Cell.GRAPHIC_WALL:
                    case Cell.GRAPHIC_MOUNTAIN:
                    case Cell.GRAPHIC_FOREST_IMPASSABLE:
                    case Cell.GRAPHIC_SECRET_DOOR:
                    case Cell.GRAPHIC_SECRET_MOUNTAIN:
                    case Cell.GRAPHIC_LOCKED_DOOR_HORIZONTAL:
                    case Cell.GRAPHIC_LOCKED_DOOR_VERTICAL:
                    case Cell.GRAPHIC_COUNTER:
                    case Cell.GRAPHIC_COUNTER_PLACEABLE:
                    case Cell.GRAPHIC_ALTAR:
                    case Cell.GRAPHIC_ALTAR_PLACEABLE:
                    case Cell.GRAPHIC_REEF:
                    case Cell.GRAPHIC_GRATE:
                        cost = infinity;
                        break;
                    default:
                        cost = 1;
                        break;
                }
                return cost;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return -1;
            }
        }
        public bool BuildMoveList(int x, int y, int z)
        {
            int xMax = World.GetFacetByID(Owner.FacetID).GetLandByID(Owner.LandID).GetMapByID(Owner.MapID).XCordMax[Owner.Z];
            int yMax = World.GetFacetByID(Owner.FacetID).GetLandByID(Owner.LandID).GetMapByID(Owner.MapID).YCordMax[Owner.Z];
            int xMin = World.GetFacetByID(Owner.FacetID).GetLandByID(Owner.LandID).GetMapByID(Owner.MapID).XCordMin[Owner.Z];
            int yMin = World.GetFacetByID(Owner.FacetID).GetLandByID(Owner.LandID).GetMapByID(Owner.MapID).YCordMin[Owner.Z];

            Point origin = new Point(Owner.X, Owner.Y);
            Point target = new Point(x, y);
            Point pos, current;
            int cost;
            int lowest_cost;

            List<Point> unfinished = new List<Point>();
            List<string> moves = new List<string>();
            // the hashtable values are cell cost value
            Hashtable travelled = new Hashtable();
            travelled[origin] = 0;

            pos = current = origin;
            while (pos != target && current != null)
            {
                foreach (Point neighbor in get_neighbor_pos(Owner.Map.cells, current, xMax, yMax, xMin, yMin))
                {
                    try
                    {
                        if (Owner.Map.cells.ContainsKey(neighbor.x + "," + neighbor.y + "," + z))
                        {
                            cost = GetCellCost(Owner.Map.cells[neighbor.x + "," + neighbor.y + "," + z]);
                        }
                        else
                        {
                            cost = 10000;
                        }
                    }
                    catch (Exception)
                    {
                        Utils.Log("Error in BuildMoveList(int x, int y, int z) in Creature.cs line 704 : var cost", Utils.LogType.Exception);
                        continue;
                    }
                    if (cost >= 10000)
                    {
                        continue;
                    }

                    pos = neighbor;

                    int v;
                    if (travelled.Contains(pos))
                    {
                        v = (int)travelled[pos];
                    }
                    else
                    {
                        v = 0;
                    }

                    if (0 == v && pos != origin)
                    {
                        travelled[pos] = 1 + (int)travelled[current] + cost;
                        if (pos == target) break;
                        unfinished.Add(pos);
                    }
                }
                if (pos != target)
                {
                    try
                    {
                        if (unfinished.Count > 0)
                        {
                            current = unfinished[0];
                            unfinished.RemoveAt(0);
                        }
                        else
                        {
                            current = null;
                        }
                    }
                    catch
                    {
                        // No unfinished cells
                        current = null;
                    }
                }
            }

            current = pos;
            if (current != target)
            {
                // Target was unreachable!
                return false;
            }

            // now begin building the MoveList using the cost values
            while (current != origin)
            {
                lowest_cost = 100;
                Point best_point = null;
                foreach (Point neighbor in get_neighbor_pos(Owner.Map.cells, current, xMax, yMax, xMin, yMin))
                {
                    int v;
                    pos = neighbor;
                    if (!travelled.ContainsKey(pos))
                    {
                        continue;
                    }

                    v = (int)travelled[pos];
                    if (v < lowest_cost)
                    {
                        lowest_cost = v;
                        best_point = pos;
                    }
                }
                if (best_point == null) { break; }
                moves.Add(Owner.Brain.GetDirString(best_point, current));
                current = best_point;
            }

            moves.Reverse();
            Owner.Brain.MoveList = moves;
            if (Owner.Brain.MoveList.Count <= 0)
            {
                return false; // Catch for 0 move movelist - Cant get there from here.
            }
            return true;
        }
        private string GetDirString(Point beg, Point end)
        {
            Point dp = end - beg;
            string lhs = "", rhs = "";

            if (dp.y == -1)
                lhs = "n";
            else if (dp.y == 1)
                lhs = "s";

            if (dp.x == -1)
                rhs = "w";
            else if (dp.x == 1)
                rhs = "e";

            return lhs + rhs;
        }
        private List<Point> get_neighbor_pos(Dictionary<string, Cell> cells, Point current, int xmax, int ymax, int xmin, int ymin)
        {
            List<Point> neighbors = new List<Point>();

            try
            {
                if (current == null)
                {
                    return neighbors;
                }

                Point pt;
                foreach (Point d in directions)
                {
                    pt = new Point(current.x + d.x, current.y + d.y);
                    if (pt.x >= xmin && pt.x <= (xmax - 2) && pt.y >= ymin && pt.y <= (ymax - 2))
                    {
                        neighbors.Add(pt);
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
            return neighbors;
        }
        #endregion


        
    }
}
