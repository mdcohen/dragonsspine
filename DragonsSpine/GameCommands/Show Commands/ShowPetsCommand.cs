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
    [CommandAttribute("showpets", "Display current pets.", (int)Globals.eImpLevel.USER, new string[] { "show pets", "show pet" },
        0, new string[] { "There are no arguments for the show pets command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowPetsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.Pets.Count < 1)
            {
                chr.WriteToDisplay("You do not own any pets.");
            }
            else
            {
                chr.WriteToDisplay("Current Pets:");

                string petInfo = "";

                foreach (NPC pet in chr.Pets)
                {
                    // Pets with a quest are added to the Pets list for other reasons in the code.
                    if (pet.QuestList.Count < 1)
                    {
                        petInfo = pet.Name.PadRight(15);
                        petInfo += "H: " + pet.Hits + "/" + pet.HitsFull + " S: ";
                        string staminaString = pet.Stamina + "/" + pet.StaminaFull;
                        petInfo += staminaString.PadLeft(7);

                        string manaString = "";
                        // pet has mana
                        if (pet.ManaFull > 0)
                        {
                            manaString = " M: ";
                            manaString += (pet.Mana + "/" + pet.ManaFull).PadLeft(7);
                        }

                        manaString = manaString.PadRight(11);

                        petInfo += manaString.PadRight(14);

                        if (pet.IsSummoned)
                            petInfo += " [" + Utils.RoundsToTimeSpan((pet as NPC).RoundsRemaining) + "]";
                        else if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Charm_Animal))
                            petInfo += " [" + Utils.RoundsToTimeSpan(pet.EffectsList[Effect.EffectTypes.Charm_Animal].Duration) + "]";
                        else if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Command_Undead))
                            petInfo += " [" + Utils.RoundsToTimeSpan(pet.EffectsList[Effect.EffectTypes.Command_Undead].Duration) + "]";

                        // phantasm       H: 100/100 S: 100/100 M: 100/100  [time remaining] <target> (following) (resting)
                        if (pet is NPC && (pet as NPC).MostHated != null) petInfo += " <" + (pet as NPC).MostHated.Name + ">";
                        if (pet.FollowID > 0) { petInfo += " (following)"; }
                        if (pet.IsResting) { petInfo += " (resting)"; }
                        if (pet.IsGuarding) { petInfo += " (guarding)"; pet.BreakFollowMode(); }

                        if (pet.Poisoned > 0 || pet.EffectsList.ContainsKey(Effect.EffectTypes.Venom)) petInfo += " (poisoned)";
                        if (pet.IsBlind) petInfo += " (blind)";
                        if (pet.IsFeared) petInfo += " (feared)";
                        if (pet.EffectsList.ContainsKey(Effect.EffectTypes.Contagion)) petInfo += " (diseased)";

                        chr.WriteToDisplay(petInfo);
                    }
                }
            }

            return true;
        }
    }
}
