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
    [CommandAttribute("impcleareffects", "Clear all effects on a target or in a cell.", (int)Globals.eImpLevel.DEVJR, new string[] { "impclearfx" },
        0, new string[] { "impclearfx <target>", "impclearfx cell" }, Globals.ePlayerState.PLAYING)]
    public class ImpClearEffectsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                foreach (var effect in chr.EffectsList.Values)
                    effect.StopCharacterEffect();
            }
            else if(args.ToLower() == "cell")
            {
                foreach (var effectType in chr.CurrentCell.AreaEffects.Keys)
                {
                    chr.WriteToDisplay("Clearing AreaEffect " + Utils.FormatEnumString(effectType.ToString()));
                    chr.CurrentCell.AreaEffects[effectType].StopAreaEffect();
                }
            }
            else
            {
                var target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, args, true,
                                                                                                        true);
                if(target != null)
                {
                    foreach (var effect in target.EffectsList.Values)
                        effect.StopCharacterEffect();
                }
                else chr.WriteToDisplay("Unable to find target: " + args);
            }

            return true;
        }
    }
}
