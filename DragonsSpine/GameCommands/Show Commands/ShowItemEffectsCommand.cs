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

namespace DragonsSpine.Commands
{
    [CommandAttribute("showitemeffects", "Display a list of current item effects.", (int)Globals.eImpLevel.USER, new string[] { "show effects i", "show effects item", "show item effects", "show fx i" },
        0, new string[] { "There are no arguments for the show item effects command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowItemEffectsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.WriteToDisplay("Item Effect (Amount):");

            List<string> list = new List<string>();
            Dictionary<Effect.EffectTypes, int> itemFX = new Dictionary<Effect.EffectTypes, int>();

            foreach (Effect effect in chr.WornEffectsList)
            {
                if (itemFX.ContainsKey(effect.EffectType))
                {
                    itemFX[effect.EffectType] += effect.Power;
                }
                else itemFX.Add(effect.EffectType, effect.Power);
            }

            int count = 1;

            foreach (Effect.EffectTypes effectType in itemFX.Keys)
            {
                chr.WriteToDisplay(count + ". " + Utils.FormatEnumString(effectType.ToString()) + " (" + itemFX[effectType] + ")");
                count++;
            }

            return true;
        }
    }
}
