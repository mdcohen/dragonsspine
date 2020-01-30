using System;
using System.Collections.Generic;
using System.Collections;
using DragonsSpine.GameWorld;
using Cell = DragonsSpine.GameWorld.Cell;
using Map = DragonsSpine.GameWorld.Map;
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine
{
    public class AreaEffect : Effect
    {
        public static AreaEffectType[] LightSourceAreaEffects = new AreaEffectType[]
                                                                    {
                                                                        AreaEffectType.Fire,
                                                                        AreaEffectType.Dragon__s_Breath_Fire,
                                                                        AreaEffectType.Lightning,
                                                                        AreaEffectType.Lightning_Storm,
                                                                        AreaEffectType.Fire_Storm, AreaEffectType.Lava,
                                                                        AreaEffectType.Ornic_Flame
                                                                    };

        public static AreaEffectType[] LightAbsorbingAreaEffects = new AreaEffectType[]
                                                                       {
                                                                           AreaEffectType.Darkness, AreaEffectType.Ice,
                                                                           AreaEffectType.Poison_Cloud,
                                                                           AreaEffectType.Dragon__s_Breath_Ice,
                                                                           AreaEffectType.Whirlwind,
                                                                           AreaEffectType.Black_Fog,
                                                                           AreaEffectType.Blizzard
                                                                       };

        public static EffectTypes[] NoInitialDoAreaEffectCall = new EffectTypes[]
                                                                        {
                                                                            EffectTypes.Fire_Storm,
                                                                            EffectTypes.Locust_Swarm,
                                                                            EffectTypes.Whirlwind,
                                                                            EffectTypes.Lightning_Storm,
                                                                            EffectTypes.Blizzard,
                                                                            EffectTypes.Poison_Cloud,
                                                                            EffectTypes.Lava,
                                                                            EffectTypes.Tempest,
                                                                        };

        public enum AreaEffectType : int
        {
            None,
            Fire,
            Ice,
            Darkness,
            Poison_Cloud,
            Web,
            Light,
            Concussion,
            Find_Secret_Door,
            Dragon__s_Breath_Fire, // 10
            Dragon__s_Breath_Ice,
            Turn_Undead,
            Whirlwind,
            Unlocked_Horizontal_Door,
            Unlocked_Vertical_Door, // 75
            Find_Secret_Rockwall,
            Hide_Door,
            Black_Fog,
            Lightning_Storm,
            Fire_Storm,
            Lava,
            Lightning, // 85
            Blizzard,
            Ornic_Flame,
            Dragon__s_Breath_Wind
        }

        private string effectGraphic; // graphic shown on the map, if blank, then no graphic or this is an effect attached to a character

        private readonly List<Cell> cellList = new List<Cell>(); // list of cells modified by the AreaEffect

        public AreaEffect()
        {

        }

        /// <summary>
        /// Called to create a single Cell AreaEffect.
        /// </summary>
        /// <param name="effectType"></param>
        /// <param name="effectGraphic"></param>
        /// <param name="effectAmount"></param>
        /// <param name="duration"></param>
        /// <param name="caster"></param>
        /// <param name="cell"></param>
        public AreaEffect(EffectTypes effectType, string effectGraphic, int effectAmount, int duration, Character caster, Cell cell)
        {
            m_effectType = effectType;

            if (string.IsNullOrEmpty(effectGraphic)) // invisible effect (Thunderwave) so display what should be seen
            {
                this.effectGraphic = cell.DisplayGraphic;
            }
            else
            {
                this.effectGraphic = effectGraphic;
                cell.DisplayGraphic = effectGraphic;
            }

            m_power = effectAmount;
            m_duration = duration;
            Caster = caster;

            cellList = new List<Cell>
            {
                cell
            };

            if(!Map.IsSpellPathBlocked(cell))
                cell.Add(this);

            // Found the issue why area effect spells cast with a power were able to cover blocked spell paths.
            // There was no check here to see if the path was blocked. 2/5/2017 Eb
            if (!string.IsNullOrEmpty(this.effectGraphic) && !Map.IsSpellPathBlocked(cell))
            {
                cell.DisplayGraphic = this.effectGraphic;
            }

            EffectTimer = new System.Timers.Timer(DragonsSpineMain.MasterRoundInterval);
            EffectTimer.Elapsed += new System.Timers.ElapsedEventHandler(AreaEffectEvent);
            EffectTimer.Start();

            // Delayed call to event means the caster has a chance to run away.
            if (Array.IndexOf(NoInitialDoAreaEffectCall, EffectType) != -1)
            {
                return;
            }
            else
            {
                DoAreaEffect(cell, this);
            }
        }

        /// <summary>
        /// Create an AreaEffect in an array of Cells.
        /// </summary>
        /// <param name="effectType"></param>
        /// <param name="effectGraphic"></param>
        /// <param name="effectAmount"></param>
        /// <param name="duration"></param>
        /// <param name="caster"></param>
        /// <param name="cellList"></param>
        public AreaEffect(EffectTypes effectType, string effectGraphic, int effectAmount, int duration, Character caster, ArrayList cellList)
        {
            //effect = new AreaEffect(EffectType, Cell.GRAPHIC_DARKNESS, 0, EffectPower, caster, areaCellList)

            foreach (Cell cell in cellList)
            {
                if (!cell.AreaEffects.ContainsKey(effectType))
                {
                    var areaEffect = new AreaEffect(effectType, effectGraphic, effectAmount, duration, caster, cell);
                }
            }
        }

        private void AreaEffectEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (DragonsSpineMain.ServerStatus != DragonsSpineMain.ServerState.Running)
                return;

            // Permanent area effect.
            if (Duration < 0)
            {
                foreach (Cell cell in new List<Cell>(this.cellList))
                    DoAreaEffect(cell, this);
                return;
            }

            try
            {
                if (Duration > 0)
                    m_duration--;

                if (Duration == 0)
                {
                    EffectTimer.Stop();
                    StopAreaEffect();
                }
                else
                {
                    foreach (Cell cell in new List<Cell>(this.cellList))
                        DoAreaEffect(cell, this);
                }
            }
            catch (Exception ex)
            {
                Utils.LogException(ex);
            }
        }

        public void StopAreaEffect()
        {
            try
            {
                Effect effect;

                foreach (Cell cell in this.cellList)
                {
                    cell.Remove(EffectType);

                    if (cell.AreaEffects.Count <= 0)
                    {
                        if (cell.DisplayGraphic != cell.CellGraphic)
                            cell.DisplayGraphic = cell.CellGraphic;

                        switch (this.EffectType)
                        {
                            case EffectTypes.Hide_Door:
                                cell.IsSecretDoor = false;
                                break;
                            case EffectTypes.Find_Secret_Door:
                                #region Find Secret Door
                                // If something is standing in the doorway the secret door is destroyed. This is a remnant from the old game.
                                int visibleCharacterCount = 0;
                                foreach (Character ch in cell.Characters.Values)
                                {
                                    if (!ch.IsInvisible)
                                    {
                                        visibleCharacterCount++;
                                        break;
                                    }
                                }
                                if (visibleCharacterCount > 0)
                                {
                                    effect = new AreaEffect(EffectTypes.Concussion, "", Rules.RollD(1, 20), 0, null, cell);
                                    cell.IsSecretDoor = false;
                                    if (Rules.RollD(1, 2) == 1)
                                    {
                                        cell.CellGraphic = Cell.GRAPHIC_RUINS_RIGHT;
                                        cell.DisplayGraphic = cell.CellGraphic;
                                    }
                                    else
                                    {
                                        cell.CellGraphic = Cell.GRAPHIC_RUINS_LEFT;
                                        cell.DisplayGraphic = cell.CellGraphic;
                                    }
                                }
                                else
                                {
                                    cell.DisplayGraphic = Cell.GRAPHIC_WALL;
                                    cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.CloseDoor));
                                }
                                break;
                            #endregion
                            case EffectTypes.Find_Secret_Rockwall:
                                #region Find Secret Rockwall
                                visibleCharacterCount = 0;
                                foreach (Character ch in cell.Characters.Values)
                                {
                                    if (!ch.IsInvisible)
                                    {
                                        visibleCharacterCount++;
                                        break;
                                    }
                                }
                                if (visibleCharacterCount > 0)
                                {
                                    effect = new AreaEffect(EffectTypes.Concussion, "", Rules.RollD(3, 20), 0, null, cell);
                                    effect = new AreaEffect(EffectTypes.Find_Secret_Rockwall, Cell.GRAPHIC_EMPTY, 0, 8, null, cell);
                                }
                                else
                                {
                                    cell.DisplayGraphic = Cell.GRAPHIC_MOUNTAIN;
                                    cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SlidingRockDoor));
                                }
                                break;
                            #endregion
                            case EffectTypes.Fire:
                            case EffectTypes.Fire_Storm:
                            case EffectTypes.Lava:
                                #region Fire
                                if (cell.CellGraphic == Cell.GRAPHIC_FOREST_LEFT || cell.CellGraphic == Cell.GRAPHIC_FOREST_FROSTY_LEFT)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_BURNT_LEFT, 0, Rules.Dice.Next(180, 200), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_RIGHT || cell.CellGraphic == Cell.GRAPHIC_FOREST_FROSTY_RIGHT)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_BURNT_RIGHT, 0, Rules.Dice.Next(180, 200), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_FULL || cell.CellGraphic == Cell.GRAPHIC_FOREST_FROSTY_FULL)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_BURNT_FULL, 0, Rules.Dice.Next(180, 200), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_WEB && cell.DisplayGraphic == Cell.GRAPHIC_WEB)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, Rules.Dice.Next(60, 80), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_GRASS_THICK)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_GRASS_LIGHT, 0, Rules.Dice.Next(180, 200), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_ICE)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, Rules.Dice.Next(120, 140), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL || cell.CellGraphic == Cell.GRAPHIC_CLOSED_DOOR_VERTICAL)
                                {
                                    Commands.OpenCommand.OpenDoor(cell);
                                    if (Rules.RollD(1, 100) >= 90 - this.Power)
                                    {
                                        if(Rules.RollD(1, 2) == 1)
                                            effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_LEFT, 0, Rules.Dice.Next(120, 140), null, cell);
                                        else effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 0, Rules.Dice.Next(120, 140), null, cell);
                                    }
                                }
                                break; 
                                #endregion
                            case EffectTypes.Acid:
                                #region Acid
                                // note that acid stops berry growth (Cell.IsBarren property)
                                switch (cell.CellGraphic)
                                {
                                    case Cell.GRAPHIC_EMPTY: // empty cells become random barren cell
                                        if (cell.IsOutdoors) // TODO: destroy walls with acid
                                        {
                                            int nearbyWallsCount = 0;
                                            Cell[] nearbyCells = Cell.GetApplicableCellArray(cell, 1);
                                            foreach (Cell possibleWall in nearbyCells)
                                            {
                                                if (possibleWall != null && possibleWall.CellGraphic == Cell.GRAPHIC_WALL)
                                                {
                                                    nearbyWallsCount++;
                                                    break;
                                                }
                                            }

                                            if (nearbyWallsCount <= 0)
                                            {
                                                if (Rules.RollD(1, 100) > 50)
                                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_LEFT, 0, Rules.Dice.Next(360, 380), null, cell);
                                                else effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_RIGHT, 0, Rules.Dice.Next(360, 380), null, cell);
                                            }
                                        }
                                        break;
                                    // forest and grass become barren cells
                                    case Cell.GRAPHIC_FOREST_FULL:
                                    case Cell.GRAPHIC_FOREST_BURNT_FULL:
                                    case Cell.GRAPHIC_FOREST_FROSTY_FULL:
                                    case Cell.GRAPHIC_GRASS_THICK:
                                    case Cell.GRAPHIC_GRASS_LIGHT:
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_FULL, 0, Rules.Dice.Next(360, 380), null, cell);
                                        break;
                                    case Cell.GRAPHIC_FOREST_LEFT:
                                    case Cell.GRAPHIC_FOREST_BURNT_LEFT:
                                    case Cell.GRAPHIC_FOREST_FROSTY_LEFT:
                                    case Cell.GRAPHIC_WEB:
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_LEFT, 0, Rules.Dice.Next(360, 380), null, cell);
                                        break;
                                    case Cell.GRAPHIC_FOREST_RIGHT:
                                    case Cell.GRAPHIC_FOREST_BURNT_RIGHT:
                                    case Cell.GRAPHIC_FOREST_FROSTY_RIGHT:
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_RIGHT, 0, Rules.Dice.Next(360, 380), null, cell);
                                        break;
                                    case Cell.GRAPHIC_BRIDGE:
                                        if (cell.IsOutdoors)
                                        {
                                            List<Cell> adjacentBridgeCells = Map.GetAdjacentCells(cell);
                                            bool foundAdjacentWater = false;
                                            foreach (Cell abCell in adjacentBridgeCells)
                                            {
                                                if (abCell.CellGraphic == Cell.GRAPHIC_WATER)
                                                {
                                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_WATER, 0, Rules.Dice.Next(360, 380), null, cell);
                                                    foundAdjacentWater = true;
                                                    break;
                                                }
                                            }
                                            if (!foundAdjacentWater)
                                            {
                                                if (Rules.RollD(1, 100) > 50)
                                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_LEFT, 0, Rules.Dice.Next(360, 380), null, cell);
                                                else effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_RIGHT, 0, Rules.Dice.Next(360, 380), null, cell);
                                            }
                                        }
                                        break;
                                    case Cell.GRAPHIC_ICE:
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_WATER, 0, Rules.Dice.Next(340, 360), null, cell);
                                        break;
                                    case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                                    case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                                    case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                                    case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                                        {
                                            if (Rules.RollD(1, 100) >= 90 - this.Power)
                                            {
                                                if (Rules.RollD(1, 2) == 1)
                                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_LEFT, 0, Rules.Dice.Next(120, 140), null, cell);
                                                else effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 0, Rules.Dice.Next(120, 140), null, cell);
                                            }
                                        }
                                        break;
                                }
                                break; 
                                #endregion
                            case EffectTypes.Ice:
                                #region Ice
                                // ice covers the trees in frost
                                if (cell.CellGraphic == Cell.GRAPHIC_FOREST_LEFT)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_FROSTY_LEFT, 0, 120, null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_RIGHT)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_FROSTY_RIGHT, 0, 120, null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_FULL)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_FROSTY_FULL, 0, 120, null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_WEB && cell.DisplayGraphic == Cell.GRAPHIC_WEB)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, 120, null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_WATER)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_ICE, 0, 120, null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_GRASS_THICK)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_GRASS_FROZEN, 0, 120, null, cell);
                                break;
                                #endregion
                            case EffectTypes.Blizzard:
                                #region Blizzard
                                if (!cell.AreaEffects.ContainsKey(EffectTypes.Blizzard))
                                {
                                    // ice covers the trees in frost
                                    if (cell.CellGraphic == Cell.GRAPHIC_FOREST_LEFT)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_FROSTY_LEFT, 0, Rules.Dice.Next(120, 140), null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_RIGHT)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_FROSTY_RIGHT, 0, Rules.Dice.Next(120, 140), null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_FULL)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_FROSTY_FULL, 0, Rules.Dice.Next(120, 140), null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_WEB && cell.DisplayGraphic == Cell.GRAPHIC_WEB)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, Rules.Dice.Next(120, 140), null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_WATER)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_ICE, 0, Rules.Dice.Next(120, 140), null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_GRASS_THICK)
                                        effect = new AreaEffect(EffectTypes.Illusion, ", ", 0, Rules.Dice.Next(120, 140), null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_FIRE)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, Rules.Dice.Next(120, 140), null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_WATER)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_ICE, 0, Rules.Dice.Next(60, 80), null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL || cell.CellGraphic == Cell.GRAPHIC_CLOSED_DOOR_VERTICAL)
                                    {
                                        Commands.OpenCommand.OpenDoor(cell);
                                        if (Rules.RollD(1, 100) >= 90 - this.Power)
                                        {
                                            if (Rules.RollD(1, 2) == 1)
                                                effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_LEFT, 0, Rules.Dice.Next(120, 140), null, cell);
                                            else effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 0, Rules.Dice.Next(120, 140), null, cell);
                                        }
                                    }
                                    else if (cell.CellGraphic == Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL || cell.CellGraphic == Cell.GRAPHIC_OPEN_DOOR_VERTICAL)
                                    {
                                        if (Rules.RollD(1, 100) >= 90 - this.Power)
                                        {
                                            if (Rules.RollD(1, 2) == 1)
                                                effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_LEFT, 0, Rules.Dice.Next(120, 140), null, cell);
                                            else effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 0, Rules.Dice.Next(120, 140), null, cell);
                                        }
                                    }
                                }
                                break;
                            #endregion
                            case EffectTypes.Whirlwind:
                                #region Whirlwind
                                if (!cell.AreaEffects.ContainsKey(EffectTypes.Whirlwind))
                                {
                                    switch (cell.CellGraphic)
                                    {
                                        case Cell.GRAPHIC_FOREST_FROSTY_FULL:
                                        case Cell.GRAPHIC_FOREST_BURNT_FULL:
                                            effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_FULL, 0, Rules.Dice.Next(120, 140), null, cell);
                                            break;
                                        case Cell.GRAPHIC_FOREST_FROSTY_LEFT:
                                        case Cell.GRAPHIC_FOREST_BURNT_LEFT:
                                            effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_LEFT, 0, Rules.Dice.Next(120, 140), null, cell);
                                            break;
                                        case Cell.GRAPHIC_FOREST_FROSTY_RIGHT:
                                        case Cell.GRAPHIC_FOREST_BURNT_RIGHT:
                                            effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_RIGHT, 0, Rules.Dice.Next(120, 140), null, cell);
                                            break;
                                        case Cell.GRAPHIC_FOREST_FULL:
                                        case Cell.GRAPHIC_FOREST_LEFT:
                                        case Cell.GRAPHIC_FOREST_RIGHT:
                                            effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_GRASS_LIGHT, 0, Rules.Dice.Next(120, 140), null, cell);
                                            break;
                                        case Cell.GRAPHIC_FIRE: // extinguishes fires
                                            effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, Rules.Dice.Next(120, 140), null, cell);
                                            break;
                                        case Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL:
                                        case Cell.GRAPHIC_OPEN_DOOR_VERTICAL:
                                        case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL: // destroyes doors
                                        case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                                            {
                                                if (cell.CellGraphic == Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL || cell.CellGraphic == Cell.GRAPHIC_CLOSED_DOOR_VERTICAL)
                                                    Commands.OpenCommand.OpenDoor(cell);
                                                else if(cell.IsOpenDoor)
                                                {

                                                }

                                                if (Rules.RollD(1, 100) >= 90 - this.Power)
                                                {
                                                    if (Rules.RollD(1, 2) == 1)
                                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_LEFT, 0, Rules.Dice.Next(120, 140), null, cell);
                                                    else effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_RUINS_RIGHT, 0, Rules.Dice.Next(120, 140), null, cell);
                                                }
                                            }
                                            break;

                                    }
                                }
                                break; 
                            #endregion
                            case EffectTypes.Dragon__s_Breath_Fire:
                                #region Dragon's Breath Fire
                                if (cell.CellGraphic == Cell.GRAPHIC_FOREST_LEFT || cell.CellGraphic == Cell.GRAPHIC_FOREST_FROSTY_LEFT)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_BURNT_LEFT, 0, Rules.Dice.Next(180, 200), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_RIGHT || cell.CellGraphic == Cell.GRAPHIC_FOREST_FROSTY_RIGHT)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_BURNT_RIGHT, 0, Rules.Dice.Next(180, 200), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_FULL || cell.CellGraphic == Cell.GRAPHIC_FOREST_FROSTY_FULL)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_BURNT_FULL, 0, Rules.Dice.Next(180, 200), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_WEB && cell.DisplayGraphic == Cell.GRAPHIC_WEB)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, Rules.Dice.Next(60, 80), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_GRASS_THICK)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_GRASS_LIGHT, 0, Rules.Dice.Next(180, 200), null, cell);
                                else if (cell.CellGraphic == Cell.GRAPHIC_ICE)
                                    effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, Rules.Dice.Next(180, 200), null, cell);
                                break; 
                                #endregion
                            case EffectTypes.Lightning_Storm:
                                #region Lightning Storm
                                if (!cell.AreaEffects.ContainsKey(EffectTypes.Lightning_Storm))
                                {
                                    if (cell.CellGraphic == Cell.GRAPHIC_FOREST_LEFT)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_BURNT_LEFT, 1, 90, null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_RIGHT)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_BURNT_RIGHT, 1, 90, null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_FULL)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_FOREST_BURNT_FULL, 1, 90, null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_WEB && cell.DisplayGraphic == Cell.GRAPHIC_WEB)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_EMPTY, 0, 90, null, cell);
                                    else if (cell.CellGraphic == Cell.GRAPHIC_GRASS_LIGHT || cell.CellGraphic == Cell.GRAPHIC_GRASS_THICK)
                                        effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_FULL, 1, 90, null, cell);
                                    break;
                                }
                                #endregion
                                break;
                            case EffectTypes.Locust_Swarm:
                                #region Locust Swarm
                                if (!cell.AreaEffects.ContainsKey(EffectTypes.Locust_Swarm))
                                {
                                    switch (cell.CellGraphic)
                                    {
                                        case Cell.GRAPHIC_FOREST_FULL:
                                        case Cell.GRAPHIC_FOREST_BURNT_FULL:
                                        case Cell.GRAPHIC_FOREST_FROSTY_FULL:
                                        case Cell.GRAPHIC_GRASS_THICK:
                                        case Cell.GRAPHIC_GRASS_LIGHT:
                                        case Cell.GRAPHIC_WEB:
                                            effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_FULL, 0, Rules.Dice.Next(320, 360), null, cell);
                                            break;
                                        case Cell.GRAPHIC_FOREST_LEFT:
                                        case Cell.GRAPHIC_FOREST_BURNT_LEFT:
                                        case Cell.GRAPHIC_FOREST_FROSTY_LEFT:
                                            effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_LEFT, 0, Rules.Dice.Next(320, 360), null, cell);
                                            break;
                                        case Cell.GRAPHIC_FOREST_RIGHT:
                                        case Cell.GRAPHIC_FOREST_BURNT_RIGHT:
                                        case Cell.GRAPHIC_FOREST_FROSTY_RIGHT:
                                            effect = new AreaEffect(EffectTypes.Illusion, Cell.GRAPHIC_BARREN_RIGHT, 0, Rules.Dice.Next(320, 360), null, cell);
                                            break;
                                        case Cell.GRAPHIC_CLOSED_DOOR_VERTICAL:
                                        case Cell.GRAPHIC_CLOSED_DOOR_HORIZONTAL:
                                            {
                                                Commands.OpenCommand.OpenDoor(cell);
                                            }
                                            break;
                                    }
                                }
                                break; 
                                #endregion
                            default:
                                break;

                        }
                    }
                    else
                    {
                        cell.DisplayGraphic = cell.CellGraphic;

                        foreach (AreaEffect cellEffect in new List<AreaEffect>(cell.AreaEffects.Values))
                        {
                            if (!string.IsNullOrEmpty(cellEffect.effectGraphic) && cellEffect.effectGraphic != cell.CellGraphic)
                                cell.DisplayGraphic = cellEffect.effectGraphic;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
        }

        public static void DoAreaEffect(Cell cell, AreaEffect effect)
        {

//#if DEBUG
//            if (effect.caster != null && effect.caster.IsPC)
//                effect.caster.WriteToDisplay(effect.EffectType.ToString() + " Power is " + effect.Power + ".");
//#endif
            
            // Area effects are not processed when the server is not running.
            if (DragonsSpineMain.ServerStatus != DragonsSpineMain.ServerState.Running)
                return;

            // Area effects are not processed when a map is empty and the server is toggled to not process empty worlds.
            if (!DragonsSpineMain.Instance.Settings.ProcessEmptyWorld && World.GetNumberPlayersInMap(cell.MapID) == 0)
                return;
            try
            {
                switch (effect.EffectType)
                {
                    case EffectTypes.Concussion:
                        cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Explosion));
                        break;
                    case EffectTypes.Dragon__s_Breath_Fire:
                    case EffectTypes.Fire:
                    case EffectTypes.Fire_Storm:
                    case EffectTypes.Lava:
                        BurnFlammables(cell, effect); // burn flammable items
                        #region Move a FireStorm
                        if (effect.EffectType == EffectTypes.Fire_Storm)
                        {
                            if (effect.MoveAndExpandStormAreaEffect(cell) is Cell newStormCell && Rules.RollD(1, 20) == 1)
                            {
                                newStormCell.SendShout("a scorching fire storm!");
                                newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Fireball));
                            }
                        }
                        else if(effect.EffectType == EffectTypes.Lava)
                        {
                            if (effect.MoveAndExpandStormAreaEffect(cell) is Cell newStormCell && Rules.RollD(1, 20) == 1)
                            {
                                newStormCell.SendShout("the nearly silent creeping death of a lava flow.");
                                newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Fireball));
                            }
                        }
                        #endregion
                        #region Check for Smokey
                        // Check to see if cell is in Kesmai and is a forest cell or in tree preserve
                        // If true then find Smokey and summon if passes chance check
                        if (cell.LandID == World.LAND_BG && cell.MapID == 0 && cell.Z == 0)
                        {
                            bool doSmokey = false;
                            if ((cell.CellGraphic == Cell.GRAPHIC_FOREST_LEFT || cell.CellGraphic == Cell.GRAPHIC_FOREST_RIGHT) && cell.X < 110)//any tree cell on island has a chance
                            {
                                if (Rules.Dice.Next(100) < 10)
                                    doSmokey = true;
                            }
                            else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_FULL && cell.X < 110)//any forest cell on island has a better chance
                            {
                                if (Rules.Dice.Next(100) < 20)
                                    doSmokey = true;
                            }
                            else if (cell.CellGraphic == Cell.GRAPHIC_FOREST_FULL && cell.X >= 110 && cell.X <= 140)//forest cell in tree preserve is best chance
                            {
                                if (Rules.Dice.Next(100) < 30)
                                    doSmokey = true;
                            }
                            else if ((cell.CellGraphic == Cell.GRAPHIC_FOREST_LEFT || cell.CellGraphic == Cell.GRAPHIC_FOREST_RIGHT) && cell.X >= 110 && cell.X <= 140)//any tree cell in tree preserve is a chance
                            {
                                if (Rules.Dice.Next(100) < 20)
                                    doSmokey = true;
                            }
                            if (doSmokey)
                            {
                                NPC smokey = null;

                                foreach (NPC npcInWorld in Character.NPCInGameWorld)
                                {
                                    if (npcInWorld.npcID == 80)
                                    {
                                        smokey = npcInWorld;
                                        break;
                                    }
                                }

                                if (smokey != null && smokey.MapID == World.MAP_HELL)
                                {
                                    smokey.CurrentCell = cell;
                                    smokey.Age = 0;
                                }
                            }
                        }
                        #endregion
                        break;
                    case EffectTypes.Poison_Cloud:
                        #region Move a Poison Cloud
                        var poisonCells = Map.GetAdjacentCells(cell);
                        if (poisonCells != null)
                        {
                            if (effect.MoveAndExpandStormAreaEffect(cell) is Cell newStormCell && Rules.RollD(1, 20) == 1)
                            {
                                newStormCell.SendShout("a slight hissing sound and beings gasping for air!");
                                //newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Whirlwind));
                            }
                        }
                        #endregion
                        break;
                    case EffectTypes.Locust_Swarm:
                        #region Move and expand Locust Swarm
                        var locustCells = Map.GetAdjacentCells(cell);
                        if (locustCells != null)
                        {
                            if (effect.MoveAndExpandStormAreaEffect(cell) is Cell newStormCell && Rules.RollD(1, 20) == 1)
                            {
                                newStormCell.SendShout("the sound of a million wings buzzing!");
                                newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Whirlwind));
                            }
                        }
                        break;
                        #endregion
                    case EffectTypes.Whirlwind:
                        #region Move and expand a Whirlwind
                        var celllist = Map.GetAdjacentCells(cell);
                        if (celllist != null)
                        {
                            if (effect.MoveAndExpandStormAreaEffect(cell) is Cell newStormCell && Rules.RollD(1, 20) == 1)
                            {
                                newStormCell.SendShout("a raging whirlwind!");
                                newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Whirlwind));
                            }

                            foreach (Cell windCell in celllist)
                            {
                                if (windCell.AreaEffects.ContainsKey(EffectTypes.Fog))
                                    windCell.AreaEffects[EffectTypes.Fog].StopAreaEffect();
                                if (windCell.AreaEffects.ContainsKey(EffectTypes.Poison_Cloud))
                                    windCell.AreaEffects[EffectTypes.Poison_Cloud].StopAreaEffect();
                                if(windCell.AreaEffects.ContainsKey(EffectTypes.Fire))
                                {
                                    windCell.AreaEffects[EffectTypes.Fire].MoveAndExpandStormAreaEffect(windCell);
                                }
                            }
                        }
                        
                        break;
                    #endregion
                    case EffectTypes.Blizzard:
                        #region Move and expand a Blizzard
                        var blizzardCells = Map.GetAdjacentCells(cell);
                        if (blizzardCells != null)
                        {
                            if (effect.MoveAndExpandStormAreaEffect(cell) is Cell newStormCell && Rules.RollD(1, 20) == 1)
                            {
                                newStormCell.SendShout("a howling blizzard!");
                                newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Whirlwind));
                                newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.IceStorm));
                            }
                        }
                        break;
                    #endregion
                    case EffectTypes.Lightning_Storm:
                        #region Move and expand a Lightning Storm
                        var lightningCells = Map.GetAdjacentCells(cell);
                        if (lightningCells != null)
                        {
                            if (effect.MoveAndExpandStormAreaEffect(cell) is Cell newStormCell && Rules.RollD(1, 20) == 1)
                            {
                                newStormCell.SendShout("a lightning storm!");
                                newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Whirlwind));
                                newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.ThunderClap));
                            }
                        }
                        break;
                    #endregion
                    case EffectTypes.Tempest:
                        #region Move and expand a Tempest wind
                        var tempestCells = Map.GetAdjacentCells(cell);
                        if (tempestCells != null)
                        {
                            if (effect.MoveAndExpandStormAreaEffect(cell) is Cell newStormCell && Rules.RollD(1, 20) == 1)
                            {
                                newStormCell.SendShout("a divine tempest!");
                                newStormCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.Whirlwind));
                            }
                        }
                        break;
                    #endregion
                    case EffectTypes.Ornic_Flame:
                        #region Ornic Flame
                        {
                            if (cell.Items.Count > 0)
                            {
                                Item tsavorite = null;
                                double coinValue = 0;

                                foreach (Item item in new List<Item>(cell.Items))
                                {
                                    if (item.itemID == Item.ID_DAZZLING_TSAVORITE && tsavorite == null)
                                    {
                                        tsavorite = item;
                                        if (cell.Items.Count > 1)
                                        {
                                            cell.SendToAllInSight("The violet flames roar for a moment and then die down.");
                                        }
                                    }
                                    else if (item.itemID == Item.ID_DAZZLING_TSAVORITE && tsavorite != null)
                                    {
                                        World.CollectFeeForLottery(World.FEE_ORNIC_FLAME_USE, cell.LandID, ref item.coinValue);
                                        coinValue += item.coinValue;
                                        cell.Remove(item);
                                    }
                                    else if ((item is Corpse) && !(item as Corpse).IsPlayerCorpse)
                                    {
                                        Corpse.DumpCorpse(item as Corpse, cell);
                                        cell.Remove(item);
                                    }
                                    else if (item.attunedID <= 0)
                                    {
                                        World.CollectFeeForLottery(World.FEE_ORNIC_FLAME_USE, cell.LandID, ref item.coinValue);
                                        coinValue += item.coinValue;
                                        cell.Remove(item);
                                    }
                                }

                                // Add value to existing tsavorite gem.
                                if (tsavorite != null)
                                {
                                    tsavorite.coinValue += coinValue;
                                }
                                else if(coinValue > 0) // Or create a new tsavorite gem if items of value were present.
                                {
                                    tsavorite = Item.CopyItemFromDictionary(Item.ID_DAZZLING_TSAVORITE);
                                    tsavorite.coinValue = coinValue;
                                    cell.EmitSound(GameSpell.GetSpell((int)GameSpell.GameSpellID.Bonfire).SoundFile);
                                    cell.SendToAllInSight("The violet flames roar for a moment and then die down.");
                                    cell.Add(tsavorite);
                                }
                            }
                        }
                        break; 
                        #endregion
                    default:
                        break;
                }

                #region Create list of affected characters.
                ArrayList affectedCharacters = new ArrayList();

                foreach (Character target in cell.Characters.Values)
                {
                    if (!target.IsInvisible && !target.IsImmortal && !target.IsDead)
                    {
                        if (effect.EffectType == EffectTypes.Turn_Undead)
                        {
                            if (target.IsUndead)
                                affectedCharacters.Add(target);
                        }
                        else
                        {
                            switch(effect.EffectType)
                            {
                                // AreaEffects that do not harm the caster.
                                case EffectTypes.Dragon__s_Breath_Acid:
                                case EffectTypes.Dragon__s_Breath_Fire:
                                case EffectTypes.Dragon__s_Breath_Ice:
                                case EffectTypes.Dragon__s_Breath_Poison:
                                case EffectTypes.Dragon__s_Breath_Storm:
                                case EffectTypes.Dragon__s_Breath_Wind:
                                case EffectTypes.Thunderwave: // seriously need to code another way to do this
                                    if (effect.Caster != null && target != effect.Caster)
                                        affectedCharacters.Add(target);
                                    break;
                                default:
                                    affectedCharacters.Add(target);
                                    break;
                            }
                        }
                    }
                }
                #endregion

                if (affectedCharacters.Count > 0)
                {
                    foreach (Character eft in affectedCharacters)
                    {
                        #region Black Fog (random teleportation)
                        if (effect.EffectType == EffectTypes.Black_Fog)
                        {
                            int chance = Rules.Dice.Next(1, 101);

                            foreach (Character target in cell.Characters.Values)
                            {
                                if (Rules.RollD(1, 100) <= 25) // 25% chance to move the character
                                {
                                    //locate a fog cell to put character in
                                    if (!target.IsDead && target.IsPC)
                                    {
                                        target.WriteToDisplay("You are enveloped in a black fog.");
                                        List<Cell> EcellList = Map.GetAdjacentCells(cell, target, 6, effect.EffectType);
                                        if (EcellList != null)
                                        {
                                            Cell newCell = EcellList[Rules.Dice.Next(1, EcellList.Count)];
                                            target.CurrentCell = newCell;
                                        }
                                    }
                                }
                            }
                            continue;
                        }
                        #endregion

                        // send message if applicable

                        // do damage if applicable
                        if (Combat.DoSpellDamage(effect.Caster, eft, null, effect.Power, Utils.FormatEnumString(effect.EffectType.ToString()).ToLower()) == 1)
                        {
                            if (effect.Caster != null)
                            {
                                Rules.GiveAEKillExp(effect.Caster, eft);
                                Skills.GiveSkillExp(effect.Caster, eft, Globals.eSkillType.Magic);
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

        private Cell MoveAndExpandStormAreaEffect(Cell cell)
        {
            var stormCells = Map.GetAdjacentCells(cell);

            if (stormCells != null)
            {
                var newStormCell = stormCells[Rules.Dice.Next(stormCells.Count)];
                var stormCellsArray = Cell.GetApplicableCellArray(newStormCell, 3);
                var newStormAreaCellList = new ArrayList();
                foreach (Cell appCell in stormCellsArray)
                    if (!Map.IsSpellPathBlocked(appCell) && !newStormAreaCellList.Contains(appCell) && !appCell.IsOutOfBounds)
                        newStormAreaCellList.Add(appCell);
                var newFirestorm = new AreaEffect(EffectType, effectGraphic, Power, Duration, Caster, newStormAreaCellList);

                Duration = 1;

                return newStormCell;
            }

            return null;
        }

        public static bool CellContainsLightAbsorbingEffect(Cell cell)
        {
            foreach (Effect effect in cell.AreaEffects.Values)
            {
                if (Array.IndexOf(LightAbsorbingAreaEffects, effect) > -1) return true;
            }

            return false;
        }
    }
}