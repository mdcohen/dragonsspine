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

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Raise_the_Dead, "raisedead", "Raise the Dead", "Call upon your deity to restore life to a fallen being who has not yet left limbo.",
        Globals.eSpellType.Alteration, Globals.eSpellTargetType.Point_Blank_Area_Effect, 10, 8, 900, "0231", false, true, false, true, false, Character.ClassType.Thaumaturge)]
    public class RaiseTheDeadSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            if (caster.CurrentCell.Items.Count > 0)
            {
                Item corpse;

                for (int x = 0; x < caster.CurrentCell.Items.Count; x++)
                {
                    corpse = caster.CurrentCell.Items[x];

                    // Currently only players can be raised from the dead.
                    if (corpse is Corpse && (corpse as Corpse).IsPlayerCorpse)
                    {
                        foreach (PC player in Character.PCInGameWorld)
                        {
                            if (player.IsDead && player.Name == (corpse as Corpse).Ghost.Name)
                            {
                                player.IsDead = false;
                                player.IsInvisible = false;
                                player.CurrentCell = caster.CurrentCell;
                                player.Hits = (int)(player.HitsMax / 3);
                                player.Stamina = (int)(player.StaminaMax / 3);
                                if (player.ManaMax > 0)
                                    player.Mana = (int)(player.ManaMax / 3);
                                player.WriteToDisplay("You have been raised from the dead by " + caster.GetNameForActionResult(true) + "!");
                                caster.CurrentCell.EmitSound(Sound.GetCommonSound(Sound.CommonSound.DeathRevive));
                                caster.CurrentCell.Items.RemoveAt(x);
                                return true;
                            }
                        }
                    }
                }
            }

            caster.WriteToDisplay("There is nothing here to raise from the dead.");

            return true;
        }
    }
}
