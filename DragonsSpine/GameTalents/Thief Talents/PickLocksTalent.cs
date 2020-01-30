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

namespace DragonsSpine.Talents
{
    [TalentAttribute("picklock", "Pick Locks", "Pick a lock using various thieves tools.", false, 3, 13500, 8, 10, true, new string[] { "picklock <direction>", "picklock <item>" },
        Character.ClassType.Thief)]
    public class PickLocksTalent : ITalentHandler
    {
        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            string[] sArgs = args.Split(" ".ToCharArray());

            GameWorld.Cell cell = GameWorld.Map.GetCellRelevantToCell(chr.CurrentCell, sArgs[0], true);

            if (cell == null || (!cell.IsLockedHorizontalDoor && !cell.IsLockedVerticalDoor))
            {
                chr.WriteToDisplay("There is no locked door there.");
                return false;
            }

            if (cell.cellLock != null && cell.cellLock.key != "")
            {
                chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.UnlockingDoor));
                chr.WriteToDisplay("You have failed to pick the lock.");

                if (!Rules.FullStatCheck(chr, Globals.eAbilityStat.Dexterity))
                {
                    Rules.BreakHideSpell(chr);
                }
                return true;
            }

            Item lockpick = chr.FindHeldItem("lockpick");

            if (lockpick == null || (lockpick != null && lockpick.skillType != Globals.eSkillType.Thievery))
            {
                chr.WriteToDisplay("You are not holding a lockpick.");
                return false;
            }

            Skills.GiveSkillExp(chr, Skills.GetSkillLevel(chr.thievery) * 50, Globals.eSkillType.Thievery);

            // 50 percent base chance to steal
            int successChance = 50;

            // this is the 100 sided die roll plus the character's thievery skill level
            int roll = Rules.RollD(1, 100);

            // you successfully steal when the roll plus thievery skill level x 2 is greater than the chance
            if (roll + (Skills.GetSkillLevel(chr.thievery) * 2) > successChance)
            {
                chr.WriteToDisplay("You have successfully picked the lock.");
                chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.UnlockingDoor));

                // give more experience
                Skills.GiveSkillExp(chr, Skills.GetSkillLevel(chr.thievery) * 50, Globals.eSkillType.Thievery);

                GameWorld.Map.UnlockDoor(cell, cell.cellLock.key);

                
                if(!Rules.FullStatCheck(chr, Globals.eAbilityStat.Dexterity))
                {
                    chr.WriteToDisplay("You have failed to remain hidden.");
                    Rules.BreakHideSpell(chr);
                }
            }
            else
            {
                chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.UnlockingDoor));
                chr.WriteToDisplay("You have failed to pick the lock.");

                if (!Rules.FullStatCheck(chr, Globals.eAbilityStat.Dexterity))
                {
                    chr.WriteToDisplay("You have failed to remain hidden.");
                    Rules.BreakHideSpell(chr);
                }
            }

            return true;
        }
    }
}
