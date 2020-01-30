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
using System.Linq;
using System.Text;
using DragonsSpine.GameWorld;

namespace DragonsSpine.Commands
{
    [CommandAttribute("gulfrraaddizihgnaadninguthsa", "Speaking the portal chant in reverse will teleport you to the docks of the Island of Kesmai.",
        (int)Globals.eImpLevel.USER, new string[] { "gulfrra addizihgna adnin guthsa" }, 3, new string[] { "There are no arguments for the reverse portal chant." },
        Globals.ePlayerState.PLAYING)]
    public class ReversePortalChantCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.CommandType = CommandTasker.CommandType.Chant;

            // Must be in the AG and be standing on a map portal cell.
            if (chr.CurrentCell != null && !chr.CurrentCell.IsMapPortal && chr.LandID != Land.ID_ADVANCEDGAME)
            {
                chr.WriteToDisplay("You feel dizzy and nearly collapse from reciting the words.");
                return false;
            }

            chr.WriteToDisplay("Your vision blurs and, at first, you feel dizzy. You slide into a state of somewhere between dreams and reality. Awakening after what seems like an eternity, you open your eyes and realize you're back where you started.");

            // Remove effects.
            foreach (Effect effect in chr.EffectsList.Values)
                effect.StopCharacterEffect();

            // Change location.
            chr.LandID = 0;
            chr.MapID = 0;
            chr.X = 41;
            chr.Y = 33;
            chr.Z = 0;
            chr.CurrentCell = Cell.GetCell(0, 0, 0, 41, 33, 0);

            // Reset recall rings.
            ResetRecallRings(chr);

            // HSM reduction, 75%
            if(Rules.RollD(1, 100) <= 75)
            {
                chr.WriteToDisplay("The journey backwards has nearly slain you. You feel extremely weak.");
                chr.Hits = Rules.RollD(1, 6);
                chr.Stamina = Rules.RollD(1, 6);
                if (chr.IsSpellUser) chr.Mana = 0;
            }

            // Drop held items, 50%.
            if(Rules.RollD(1, 100) <= 50)
            {
                if(chr.RightHand != null)
                {
                    chr.WriteToDisplay("You drop your " + chr.RightHand.name + ".");
                    chr.CurrentCell.Add(chr.RightHand);
                    chr.UnequipRightHand(chr.RightHand);
                }

                if(chr.LeftHand != null)
                {
                    chr.WriteToDisplay("You drop your " + chr.LeftHand.name + ".");
                    chr.CurrentCell.Add(chr.LeftHand);
                    chr.UnequipLeftHand(chr.LeftHand);
                }
            }

            return true;
        }

        private void ResetRecallRings(Character chr)
        {
            int recallReset = 0;

            // reset recall rings when portal used
            foreach (Item ring in chr.GetRings())
            {
                if (ring.isRecall)
                {
                    ring.isRecall = false;
                    ring.wasRecall = true;
                    recallReset++;
                }
            }

            if (recallReset > 0)
            {
                chr.SendSound(Sound.GetCommonSound(Sound.CommonSound.RecallReset));

                if (recallReset == 1)
                {
                    chr.WriteToDisplay("Your recall ring has been cleared! You must remove and reset it.");
                }
                else if (recallReset > 1)
                {
                    chr.WriteToDisplay("Your recall rings have been cleared! You must remove and reset them.");
                }
            }
        }
    }
}

