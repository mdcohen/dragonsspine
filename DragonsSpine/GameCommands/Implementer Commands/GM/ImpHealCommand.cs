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
using GameSpell = DragonsSpine.Spells.GameSpell;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impheal", "Heal a target.", (int)Globals.eImpLevel.GM, new string[] { "imph" },
        0, new string[] { "impheal <target in view> or use no arguments to heal yourself" }, Globals.ePlayerState.PLAYING)]
    public class ImpHealCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            try
            {
                if (args != null)
                {
                    string[] sArgs = args.Split(" ".ToCharArray());

                    Character target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[0], true, true);

                    if (target == null)
                    {
                        chr.WriteToDisplay("Target not found.");
                        target = chr;
                    }

                    if (target != null)
                    {
                        if (target.EffectsList.ContainsKey(Effect.EffectTypes.Blind))
                            target.EffectsList[Effect.EffectTypes.Blind].StopCharacterEffect();

                        if (target.EffectsList.ContainsKey(Effect.EffectTypes.Fear))
                            target.EffectsList[Effect.EffectTypes.Fear].StopCharacterEffect();

                        target.Stunned = 0;

                        if (target.Poisoned > 0)
                        {
                            if (target.EffectsList.ContainsKey(Effect.EffectTypes.Poison))
                                target.EffectsList[Effect.EffectTypes.Poison].StopCharacterEffect();

                            target.Poisoned = 0;
                        }

                        if (target.EffectsList.ContainsKey(Effect.EffectTypes.Contagion))
                            target.EffectsList[Effect.EffectTypes.Contagion].StopCharacterEffect();

                        target.Hits = target.HitsFull;
                        target.Stamina = target.StaminaFull;
                        target.Mana = target.ManaFull;

                        target.WriteToDisplay("You have been healed by the Ghods.");

                        chr.WriteToDisplay("You have healed " + target.Name + " with your Ghodly powers.");

                        GameSpell cureSpell = GameSpell.GetSpell((int)GameSpell.GameSpellID.Cure);

                        if(!chr.IsInvisible)
                            target.SendSound(cureSpell.SoundFile);

                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return false;
        }
    }
}
