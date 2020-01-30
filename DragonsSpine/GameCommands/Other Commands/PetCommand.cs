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
using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Commands
{
    [CommandAttribute("pet", "Pet a player to remove a self-defense flag. Pet a figurine to unsummon them. Pet an animal.", (int)Globals.eImpLevel.USER, new string[] { },
        0, new string[] { "pet <target>" }, Globals.ePlayerState.PLAYING)]
    public class PetCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null || args == "")
            {
                chr.WriteToDisplay("Who do you want to pet?");
                return false;
            }

            Character target = TargetAcquisition.FindTargetInCell(chr, args);

            if (target == null)
            {
                chr.WriteToDisplay("You do not see " + args + " here.");
                return true;
            }

            if ((target is NPC) && (target as NPC).IsSummoned && target.Alignment == chr.Alignment)
            {
                if (target.special.Contains("figurine"))
                {
                    chr.WriteToDisplay("You pet " + target.GetNameForActionResult(true) + ".");
                    Rules.DespawnFigurine(target as NPC);
                }
                else
                {
                    chr.WriteToDisplay("You pet " + target.GetNameForActionResult(true) + ".");
                }
                return true;
            }

            chr.WriteToDisplay("You pet " + target.GetNameForActionResult(true) + ".");

            if (target.canCommand)
                Effect.CreateCharacterEffect(Effect.EffectTypes.Dog_Follow, 0, target, Rules.RollD(1, 10), chr);

            // good luck dogs
            if (target.entity == Autonomy.EntityBuilding.EntityLists.Entity.Dog && (target.Alignment == Globals.eAlignment.Lawful || target.Alignment == chr.Alignment))
            {
                if (Rules.RollD(1, 20) > 12)
                {
                    chr.WriteToDisplay("The dog wags its tail.");
                    target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.DogBark));
                    Effect.CreateCharacterEffect(Effect.EffectTypes.Animal_Affinity, 0, chr, Rules.RollD(2, 100) + 10, null);
                }
                else if (Rules.RollD(1, 100) < 50)
                {
                    target.SendToAllInSight(target.Name + ": Woof woof woof!");
                    target.EmitSound(Sound.GetCommonSound(Sound.CommonSound.DogBark));
                }

                return true;
            }

            if (target.IsPC)
            {
                target.WriteToDisplay(chr.GetNameForActionResult(false) + " is petting you.");

                if (chr.FlaggedUniqueIDs.RemoveAll(id => id == target.UniqueID) > 0)
                    chr.WriteToDisplay(target.GetNameForActionResult(false) + " is no longer flagged.");
            }

            return true;
        }
    }
}
