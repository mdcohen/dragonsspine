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

namespace DragonsSpine.Commands
{
    [CommandAttribute("showspelleffects", "Display a list of current spell effects.", (int)Globals.eImpLevel.USER, new string[] { "show effects", "show effects s", "show effects spell", "show fx", "show fx s" },
        0, new string[] { "There are no arguments for the show spell effects command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowSpellEffectsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.WriteToDisplay("Spell Effect (Amount) [Time Remaining]:");

            System.Collections.Generic.List<string> list = new System.Collections.Generic.List<string>();

            foreach (Effect effect in chr.EffectsList.Values)
            {
                list.Add(Effect.GetEffectName(effect.EffectType) + " (" + effect.Power + ") [" + Utils.RoundsToTimeSpan(effect.Duration) + "]");
            }

            list.Sort();

            int count = 1;

            foreach (string s in list)
            {
                chr.WriteToDisplay(count + ". " + s);
                count++;
            }

            chr.WriteToDisplay("Total = " + list.Count);

            return true;
        }
    }
}
