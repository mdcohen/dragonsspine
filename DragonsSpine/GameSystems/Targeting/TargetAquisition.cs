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
using Cell = DragonsSpine.GameWorld.Cell;

namespace DragonsSpine.GameSystems.Targeting
{
    public static class TargetAquisition
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public static Character FindTargetInCell(Character ch, string targetName)
        {
            // TODO: add blind fighting
            if (ch.CurrentCell != null &&
                (ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) ||
                                    ch.CurrentCell.IsAlwaysDark) && !ch.HasNightVision)
            {
                return null;
            }

            if (ch.IsBlind && !ch.HasTalent(Talents.GameTalent.TALENTS.BlindFighting))
                return null;

            int SubStrLen = 0;
            Cell cell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.X, ch.Y, ch.Z);

            Int32.TryParse(targetName, out int id);

            // look for the target in the seenlist first.
            if (ch.seenList.Count > 0)
            {
                foreach (Character chr in new List<Character>(ch.seenList))
                {
                    if (id == chr.UniqueID)
                    {
                        return chr;
                    }
                    else
                    {
                        if (chr.Name.Length < targetName.Length)
                            continue;
                        SubStrLen = targetName.Length;
                        if (chr.Name.ToLower().Substring(0, SubStrLen) == targetName.ToLower() && chr != ch && chr.CurrentCell == cell)
                            return chr;
                    }
                }
            }

            // look for the character in the charlist of the cell
            if (cell.Characters.Count > 0)
            {
                foreach (Character chr in cell.Characters.Values)
                {
                    if (id == chr.UniqueID)
                    {
                        return chr;
                    }
                    else
                    {
                        if (chr.Name.Length < targetName.Length)
                            continue;

                        SubStrLen = targetName.Length;

                        // compare the substring against the targetName and see if we find a match
                        if (chr.Name.ToLower().Substring(0, SubStrLen) == targetName.ToLower() && chr != ch && !chr.IsInvisible)
                            return chr;
                    }
                }
            }
            return null;
        }

        public static Character FindTargetInCell(Character ch, string targetName, int countTo)
        {
            int SubStrLen = 0;
            short localCount = 1;

            #region NPCs use seenList
            // look for the target in the seenlist first.
            if (ch.seenList.Count > 0)
            {
                foreach (Character chr in new List<Character>(ch.seenList))
                {
                    if (chr.Name.Length < targetName.Length)
                        continue;

                    SubStrLen = targetName.Length;

                    if (chr.Name.ToLower().Substring(0, SubStrLen) == targetName.ToLower() && chr != ch &&
                        chr.CurrentCell == ch.CurrentCell && localCount == countTo)
                    {
                        return chr;
                    }
                    else localCount++;
                }
            }
            #endregion

            localCount = 1;

            // look for the character in the charlist of the cell
            foreach (Character target in ch.CurrentCell.Characters.Values)
            {
                if (target.Name.Length < targetName.Length)
                    continue;
                SubStrLen = targetName.Length;

                // compare the substring against the targetName and see if we find a match
                if (target.Name.ToLower().Substring(0, SubStrLen) == targetName.ToLower() && target != ch && Rules.DetectInvisible(target, ch) &&
                    Rules.DetectHidden(target, ch) && localCount == countTo)
                {
                    return target;
                }
                else localCount++;
            }
            return null;
        }

        /// <summary>
        /// Not currently used.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="worldNPCID"></param>
        /// <returns></returns>
        public static Character FindTargetInCell(Cell cell, long worldNPCID)
        {
            foreach (Character chr in cell.Characters.Values)
                if (chr is NPC && chr.UniqueID == worldNPCID)
                    return chr;

            return null;
        }

        /// <summary>
        /// Gauranteed to find a target in SeenList if id matches playerID or worldNPCID.
        /// </summary>
        /// <param name="seenList">The list of visible Characters.</param>
        /// <param name="id">Player ID or World NPC ID.</param>
        /// <returns>The found Character object or null.</returns>
        public static Character FindTargetInSeenList(List<Character> seenList, int id)
        {
            foreach (Character chr in seenList)
            {
                if (chr is NPC && chr.UniqueID == id) return chr;
                else if (chr.UniqueID == id) return chr;
            }

            return null;
        }

        /// <summary>
        /// Find a target in view using player ID or unique world NPC ID aassigned upon NPC spawning.
        /// </summary>
        /// <param name="ch">The character that is attempting to find the target.</param>
        /// <param name="id">Either the player ID or the NPC's world ID.</param>
        /// <param name="includeSelf">True if should include self in search.</param>
        /// <param name="includeHidden">True if should include hidden targets.</param>
        /// <returns>The found Character object or null.</returns>
        public static Character FindTargetInView(Character ch, int id, bool includeSelf, bool includeHidden)
        {
            if (ch.UniqueID == id)
                return ch;

            if (ch.CurrentCell != null && !includeSelf &&
                (ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) ||
                                    ch.CurrentCell.IsAlwaysDark) && !ch.HasNightVision)
                return null;

            if (ch.IsBlind) return null;

            Cell cell = null;
            int bitcount = 0;
            int ypos = -15;
            int xpos = -15;

            try
            {
                //loop through all visible cells
                for (ypos = -3; ypos <= 3; ypos += 1)
                {
                    for (xpos = -3; xpos <= 3; xpos += 1)
                    {
                        if (ch.CurrentCell != null)
                        {
                            if (ch.CurrentCell.visCells != null)
                            {
                                if (ch.CurrentCell.visCells.Count >= bitcount + 1)
                                {
                                    if (ch.CurrentCell.visCells[bitcount])
                                    {
                                        if (ch.CurrentCell.Characters != null && ch.CurrentCell.Characters.Values.Count > 0)
                                        {
                                            try
                                            {
                                                cell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.X + xpos, ch.Y + ypos, ch.Z);

                                                // look for the character in the charlist of the cell
                                                foreach (Character chr in cell.Characters.Values)
                                                {
                                                    // If the found character is the target seeker and not including self, iterate.
                                                    if (chr == ch && !includeSelf)
                                                        continue;

                                                    // PCs looking for a player ID
                                                    if (chr.IsPC && chr.UniqueID == id)
                                                    {
                                                        if (Rules.DetectHidden(chr, ch) && Rules.DetectInvisible(chr, ch))
                                                            return chr;
                                                    }

                                                    // NPCs looking for an NPC world ID.
                                                    else if ((!ch.IsPC && chr.UniqueID == id) && (ch.Alignment == chr.Alignment))
                                                    {
                                                        // NPCs of the same alignment always know where the other NPC is...
                                                        if (ch.Alignment == chr.Alignment)
                                                            return chr;

                                                        // NPCs of differing alignments need to use hiding and invis rules
                                                        else if (Rules.DetectHidden(chr, ch) && Rules.DetectInvisible(chr, ch))
                                                            return chr;
                                                    }
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                Utils.LogException(e);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        bitcount++;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                //Utils.Log("Failure at Map.FindTargetInView. Character: " + ch.GetLogString() + " TargetID: " + id + " xpos = " + xpos + " ypos = " + ypos, Utils.LogType.Unknown);
                return null;
            }
        }

        /// <summary>
        /// Find a target in view by using a target name.
        /// </summary>
        /// <param name="ch">The Character searching for a target.</param>
        /// <param name="targetNameOrID">The name or unique ID of the target to search for.</param>
        /// <param name="includeSelf">True to include Character doing the searching.</param>
        /// <param name="includeHidden">True to include all hidden objects in view.</param>
        /// <returns>The found Character object or null.</returns>
        public static Character FindTargetInView(Character ch, string targetNameOrID, bool includeSelf, bool includeHidden)
        {
            if (!ch.IsImmortal && ch.CurrentCell != null && ((ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || ch.CurrentCell.IsAlwaysDark) && !ch.HasNightVision) || ch.IsBlind)
            {
                if (!ch.HasTalent(Talents.GameTalent.TALENTS.BlindFighting))
                    return null;
                else
                {
                    // commands allowed if Character has BlindFighting
                    if (!ch.CommandsProcessed.Contains(CommandTasker.CommandType.Attack) && !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shield_Bash) &&
                        !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                        return null;
                }
            }

            int SubStrLen = 0;
            int bitcount = 0;
            if (ch == null) return null;
            Cell cell = null;

            Int32.TryParse(targetNameOrID, out int id);

            if (ch.seenList.Count > 0)
            {
                foreach (Character chr in new List<Character>(ch.seenList))
                {
                    if (id != 0 && chr.UniqueID == id)
                    {
                        return chr;
                    }
                    else
                    {
                        if (chr.Name.Length < targetNameOrID.Length)
                            continue;

                        SubStrLen = targetNameOrID.Length;

                        if (chr.Name.ToLower().Substring(0, SubStrLen) == targetNameOrID.ToLower() && chr != ch)
                            return chr;
                    }
                }
            }

            try
            {
                // loop through all visible cells
                // TODO: if visibility restrictions are once again put into place these numbers will be changed
                for (int ypos = -3; ypos <= 3; ypos += 1)
                {
                    for (int xpos = -3; xpos <= 3; xpos += 1)
                    {
                        if (ch.CurrentCell.visCells[bitcount])
                        {
                            cell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.X + xpos, ch.Y + ypos, ch.Z);

                            if (cell == null)
                                continue;

                            // look for the character in the charlist of the cell
                            foreach (Character chr in cell.Characters.Values)
                            {
                                if (id != 0 && chr.UniqueID == id)
                                {
                                    return chr;
                                }
                                else
                                {

                                    if (chr.Name.Length < targetNameOrID.Length)
                                        continue;

                                    SubStrLen = targetNameOrID.Length;

                                    if ((includeHidden && Rules.DetectHidden(chr, ch)) || (!chr.IsHidden && Rules.DetectInvisible(chr, ch)))
                                    {
                                        if (includeSelf)
                                        {
                                            if (chr.Name.ToLower().Substring(0, SubStrLen) == targetNameOrID.ToLower())
                                            {
                                                return chr;
                                            }
                                        }
                                        else
                                        {
                                            if (chr.Name.ToLower().Substring(0, SubStrLen) == targetNameOrID.ToLower() && chr != ch)
                                            {
                                                return chr;
                                            }
                                        }
                                    }
                                }

                            }//end foreach
                        }
                        bitcount++;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        /// <summary>
        /// Finds a target in view. Checks current cell first, then starts in top left, across to right, down (back to left) to right etc.
        /// </summary>
        /// <param name="ch">The character attempting to find a target.</param>
        /// <param name="lookTarget">The target name.</param>
        /// <param name="countTo">The number of targets with the same name to skip through.</param>
        /// <returns>The found Character object or null.</returns>
        public static Character FindTargetInView(Character ch, string lookTarget, int countTo)
        {
            try
            {
                if (ch == null) { return null; }

                if (ch.CurrentCell != null && ((ch.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || ch.CurrentCell.IsAlwaysDark) && !ch.HasNightVision) || ch.IsBlind)
                {
                    if (!ch.HasTalent(Talents.GameTalent.TALENTS.BlindFighting))
                        return null;
                    else
                    {
                        // commands allowed if Character has BlindFighting
                        if (!ch.CommandsProcessed.Contains(CommandTasker.CommandType.Attack) && !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Shield_Bash) &&
                            !ch.CommandsProcessed.Contains(CommandTasker.CommandType.Kick))
                            return null;
                    }
                }

                int subLen = lookTarget.Length;
                int itemcount = 1;

                if (countTo <= 0) countTo = 1;
                if (subLen <= 0) return null; // someone entered a command of "fight (or attack) 'something'" where 'something' is a space or other character not decipherable.

                foreach (Character target in ch.CurrentCell.Characters.Values)
                {
                    if (target != ch && Rules.DetectHidden(target, ch) && Rules.DetectInvisible(target, ch) &&
                        target.Name.ToLower().StartsWith(lookTarget.ToLower()))
                    {
                        if (countTo == itemcount)
                        {
                            return target;
                        }
                        itemcount++;
                    }
                }

                var cellArray = Cell.GetApplicableCellArray(ch.CurrentCell, ch.GetVisibilityDistance());
                var fullCellArray = Cell.GetApplicableCellArray(ch.CurrentCell, Cell.DEFAULT_VISIBLE_DISTANCE);

                for (int j = 0; j < cellArray.Length; j++)
                {
                    if (cellArray[j] == null && ch.CurrentCell.visCells[j] && fullCellArray.Length >= j + 1 && fullCellArray[j] != null)
                    {
                        Globals.eLightSource lightsource; // no use for this yet

                        if (fullCellArray[j].HasLightSource(out lightsource) && !AreaEffect.CellContainsLightAbsorbingEffect(fullCellArray[j]))
                        {
                            cellArray[j] = fullCellArray[j];
                        }
                    }

                    if (cellArray[j] == null || ch.CurrentCell.visCells[j] == false)
                    {
                        // do nothing
                    }
                    else
                    {
                        // create array of targets
                        if (cellArray[j] != ch.CurrentCell && cellArray[j].Characters.Count > 0)
                        {
                            foreach (Character target in cellArray[j].Characters.Values)
                            {
                                if (target != ch && Rules.DetectHidden(target, ch) && Rules.DetectInvisible(target, ch) &&
                                    target.Name.ToLower().StartsWith(lookTarget.ToLower()))
                                {
                                    if (countTo == itemcount)
                                    {
                                        return target;
                                    }
                                    itemcount++;
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return null;
            }
        }

        public static Character FindTargetInNextCells(Character ch, string targetName)
        {
            int SubStrLen = 0;
            int bitcount = 0;

            Cell curCell = null;

            //loop through all visable cells
            for (int ypos = -1; ypos <= 1; ypos += 1)
            {
                for (int xpos = -1; xpos <= 1; xpos += 1)
                {
                    curCell = Cell.GetCell(ch.FacetID, ch.LandID, ch.MapID, ch.X + xpos, ch.Y + ypos, ch.Z);
                    //Look for the character in the charlist of the cell
                    foreach (Character chr in curCell.Characters.Values)
                    {
                        if (chr.Name.Length < targetName.Length)
                            continue;

                        SubStrLen = targetName.Length;

                        //compare the substring against the targetName and see if we find a match
                        if (chr.Name.ToLower().Substring(0, SubStrLen) == targetName.ToLower() && chr != ch)
                        {
                            return chr;
                        }
                    }
                    bitcount += 1;
                }

            }
            return null;
        }

        public static NPC FindQuestNPCInCell(Character ch, int id)
        {
            foreach (Character npc in ch.CurrentCell.Characters.Values)
            {
                if ((npc is NPC) && (npc as NPC).npcID == id)
                    return npc as NPC;
            }
            return null;
        }

        /// <summary>
        /// This method is always performed first when acquiring a target with arguments.
        /// </summary>
        /// <param name="targeter">The Character object doing the targetting.</param>
        /// <param name="args">The full arguments for target acquisition.</param>
        /// <returns>Character object target or null.</returns>
        public static Character AcquireTarget(Character targeter, string args, int maxDistance, int minDistance)
        {
            string[] sArgs = args.Split(" ".ToCharArray());
            int countTo = 0;

            Character target = null;

            // acquiring target in targeter's current cell
            if (maxDistance == 0 && minDistance == 0)
            {
                if (Int32.TryParse(sArgs[0], out countTo))
                {
                    if (sArgs.Length >= 2)
                        target = TargetAquisition.FindTargetInCell(targeter, sArgs[1], countTo);
                    else target = TargetAquisition.FindTargetInCell(targeter, sArgs[0]);
                }
                else target = TargetAquisition.FindTargetInCell(targeter, sArgs[0]);
            }

            if (target == null)
            {
                if (sArgs.Length >= 2 && Int32.TryParse(sArgs[0], out countTo))
                    target = TargetAquisition.FindTargetInView(targeter, sArgs[1], countTo);
                else target = TargetAquisition.FindTargetInView(targeter, sArgs[0], false, true);
            }

            if (target == null || Cell.GetCellDistance(targeter.X, targeter.Y, target.X, target.Y) > maxDistance)
                return null;

            return target;
        }

        public static Character AcquireTarget(Character targeter, string[] sArgs, int maxDistance, int minDistance)
        {
            int countTo = 0;

            Character target = null;

            // acquiring target in targeter's current cell
            if (maxDistance == 0)
            {
                if (Int32.TryParse(sArgs[0], out countTo))
                {
                    if (sArgs.Length >= 2) // # target
                        target = FindTargetInCell(targeter, sArgs[1], countTo);
                    else target = FindTargetInCell(targeter, sArgs[0]); // target is unique ID
                }
                else target = FindTargetInCell(targeter, sArgs[0]);
            }

            if (sArgs.Length >= 2 && Int32.TryParse(sArgs[0], out countTo)) // <command> # <target>
                target = FindTargetInView(targeter, sArgs[1], countTo);
            else target = FindTargetInView(targeter, sArgs[0], false, true);

            if (target == null || Cell.GetCellDistance(targeter.X, targeter.Y, target.X, target.Y) > maxDistance)
                return null;

            return target;
        }

        public static Character FindTargetInView(Character targeter, Character target)
        {
            foreach (Character chr in targeter.seenList)
            {
                if (chr == target)
                    return chr;
            }

            return null;
        }
    }
}
