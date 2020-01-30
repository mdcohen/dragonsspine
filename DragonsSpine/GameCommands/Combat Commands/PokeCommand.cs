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
    [CommandAttribute("poke", "Attack a nearby target with a polearm weapon.", (int)Globals.eImpLevel.USER, 2, new string[] { "poke <target>" }, Globals.ePlayerState.PLAYING)]
    public class PokeCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.RightHand == null)
            {
                chr.WriteToDisplay("You are not holding a weapon in your right hand.");
                return true;
            }

            if (chr.RightHand.baseType != Globals.eItemBaseType.Halberd)
            {
                chr.WriteToDisplay("You cannot poke with that weapon.");
                return true;
            }

            if (args == null || args == "")
            {
                chr.WriteToDisplay("Poke what?");
                return true;
            }

            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Poke command not processed.");
                return true;
            }

            if (Rules.GetEncumbrance(chr) > Globals.eEncumbranceLevel.Lightly)
            {
                if (chr.CommandsProcessed.Contains(CommandTasker.CommandType.Movement))
                {
                    chr.WriteToDisplay("You cannot move and poke while " + Rules.GetEncumbrance(chr).ToString().ToLower() + " encumbered.");
                    return true;
                }
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = new Character();

            target = GameSystems.Targeting.TargetAquisition.FindTargetInNextCells(chr, sArgs[0]);

            if (target == null)
            {
                target = GameSystems.Targeting.TargetAquisition.FindTargetInCell(chr, sArgs[0]);
            }

            if (target == null)
            {
                chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(sArgs[0]));
                return true;
            }

            if (chr.RightHand.IsAttunedToOther(chr))
            {
                chr.CurrentCell.Add(chr.RightHand);
                chr.WriteToDisplay("The " + chr.RightHand.name + " leaps from your hands!");
                chr.UnequipRightHand(chr.RightHand);
                return true;
            }

            if (!chr.RightHand.AlignmentCheck(chr))
            {
                chr.CurrentCell.Add(chr.RightHand);
                chr.WriteToDisplay("The " + chr.RightHand.name + " singes your hand and falls to the ground!");
                chr.UnequipRightHand(chr.RightHand);
                Combat.DoDamage(chr, chr, Rules.RollD(1, 4), false);
                return true;
            }

            chr.CommandType = CommandTasker.CommandType.Poke;

            Combat.DoCombat(chr, target, chr.RightHand);

            Combat.CheckDoubleAttack(chr, target, chr.RightHand);

            return true;
        }
    }
}
