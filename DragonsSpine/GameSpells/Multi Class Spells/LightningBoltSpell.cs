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
    [SpellAttribute(GameSpell.GameSpellID.Lightning_Bolt, "lightning", "Lightning Bolt", "Strike a location from above with a lightning bolt.",
        Globals.eSpellType.Evocation, Globals.eSpellTargetType.Area_Effect, 5, 6, 800, "0066", false, true, false, true, false, Character.ClassType.Druid, Character.ClassType.Thaumaturge)]
    public class LightningBoltSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            try
            {
                Character target = ReferenceSpell.FindAndConfirmSpellTarget(caster, args);
                Cell cell = null;
                if (target == null) cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false);
                else cell = target.CurrentCell;

                if (cell == null && target == null)
                    return false;

                #region Path testing.
                PathTest pathTest = new PathTest(PathTest.RESERVED_NAME_AREAEFFECT + PathTest.RESERVED_NAME_COMMANDSUFFIX, caster.CurrentCell);

                if (!pathTest.SuccessfulPathTest(cell))
                    cell = caster.CurrentCell;

                pathTest.RemoveFromWorld();
                #endregion

                cell.SendShout("a thunder clap!");
                cell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.ThunderClap));

                List<Character> theAffected = new List<Character>(cell.Characters.Values);

                if (theAffected.Count > 0)
                {
                    int dmgMultiplier = 6;

                    if (caster is PC) dmgMultiplier = 8;
                    int damage = 0;
                    if (Skills.GetSkillLevel(caster.magic) >= 4)
                    {
                        damage = Skills.GetSkillLevel(caster.magic) * dmgMultiplier + GameSpell.GetSpellDamageModifier(caster);
                    }
                    else
                    {
                        damage = 6 * dmgMultiplier + GameSpell.GetSpellDamageModifier(caster);
                    }

                    if (!caster.IsPC)
                    {
                        if (caster.species == Globals.eSpecies.LightningDrake)
                        {
                            damage = Rules.RollD(Skills.GetSkillLevel(caster.magic), 14);
                        }
                    }

                    foreach (Character affected in new List<Character>(theAffected))
                    {
                        if (Combat.DoSpellDamage(caster, affected, null, damage, "lightning") == 1)
                        {
                            Rules.GiveAEKillExp(caster, affected);
                            Skills.GiveSkillExp(caster, affected, Globals.eSkillType.Magic);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
            return true;
        }
    }
}
