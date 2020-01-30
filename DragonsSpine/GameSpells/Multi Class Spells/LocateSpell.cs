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
using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Locate, "locate", "Locate Entity", "Divine the approximate location of a living entity.",
        Globals.eSpellType.Divination, Globals.eSpellTargetType.Single, 3, 8, 0, "0226", false, true, false, true, false, Character.ClassType.Ranger, Character.ClassType.Knight, Character.ClassType.Ravager)]
    public class LocateSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            List<Character> locatedTargets = new List<Character>();

            foreach (Character ch in Character.AllCharList){
                if (ch.FacetID == caster.FacetID && ch.LandID == caster.LandID && ch.MapID == caster.MapID){
                    if (ch.Name.ToLower().StartsWith(sArgs[sArgs.Length - 1].ToLower())){
                        if (!ch.IsInvisible){
                            locatedTargets.Add(ch);
                        }
                    }
                }
            }

            ReferenceSpell.SendGenericCastMessage(caster, null, true);

            if (locatedTargets.Count < 1)
            {
                caster.WriteToDisplay("You cannot sense your target.");
                return true;
            }

            Character target = locatedTargets[0];

            foreach (Character ch in locatedTargets)
            {
                if (Cell.GetCellDistance(caster.X, caster.Y, target.X, target.Y) >
                    Cell.GetCellDistance(caster.X, caster.Y, ch.X, ch.Y))
                    target = ch;
            }

            string directionString = Map.GetDirection(caster.CurrentCell, target.CurrentCell).ToString().ToLower();

            if (directionString.ToLower() == "none")
            {
                caster.WriteToDisplay(target.GetNameForActionResult(false) + " is directly in front of you.");
                return true;
            }

            if (target.Z == caster.Z)
            {
                #region Distance Information

                int distance = Cell.GetCellDistance(caster.X, caster.Y, target.X, target.Y);
                string distanceString = "";
                if (distance <= 6)
                {
                    distanceString = "very close!";
                }
                else if (distance > 6 && distance <= 12)
                {
                    distanceString = "fairly close.";
                }
                else if (distance > 12 && distance <= 18)
                {
                    distanceString = "close.";
                }
                else if (distance > 18 && distance <= 24)
                {
                    distanceString = "far away.";
                }
                else if (distance > 24)
                {
                    distanceString = "very far away.";
                }
                #endregion

                caster.WriteToDisplay("You sense that " + target.GetNameForActionResult(true) + " is to the " + directionString + " and " + distanceString);

                //caster.WriteToDisplay("You sense that your target is to the " + directionString + " and " + distanceString);
            }
            else
            {
                #region Height Information
                string heightString = "";
                int heightDifference = 0;
                if (target.Z > caster.Z)
                {
                    heightDifference = Math.Abs(Math.Abs(caster.Z) - Math.Abs(target.Z));

                    if (heightDifference > 0 && heightDifference <= 60)
                    {
                        heightString = "above you";
                    }
                    else if (heightDifference > 60 && heightDifference <= 140)
                    {
                        heightString = "far above you";
                    }
                    else
                    {
                        heightString = "very far above you";
                    }
                }
                else
                {
                    heightDifference = Math.Abs(Math.Abs(target.Z) - Math.Abs(caster.Z));
                    if (heightDifference > 0 && heightDifference <= 60)
                    {
                        heightString = "below you";
                    }
                    else if (heightDifference > 60 && heightDifference <= 140)
                    {
                        heightString = "far below you";
                    }
                    else
                    {
                        heightString = "very far below you";
                    }
                }
                #endregion

                caster.WriteToDisplay("You sense that " + target.GetNameForActionResult(true) + " is " + heightString + " and to the " + directionString + ".");

                //caster.WriteToDisplay("You sense that your target is " + heightString + " and to the " + directionString + ".");
            }

            return true;
        }
    }
}
