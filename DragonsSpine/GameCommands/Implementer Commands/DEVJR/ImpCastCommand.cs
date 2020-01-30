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
namespace DragonsSpine.Commands
{
    [CommandAttribute("impcast", "Cast any spell.", (int)Globals.eImpLevel.DEVJR, new string[] { "impc" },
        0, new string[] { "impcast <spell command> <arguments>" }, Globals.ePlayerState.PLAYING)]
    public class ImpCastCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Format: impcast <spell command> <arguments>");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            // some NPCs use this command, but their spells should be prepared in NPC.PrepareSpell
            if (!(chr is NPC) || chr.preppedSpell == null)
            {
                chr.preppedSpell = Spells.GameSpell.GetSpell(sArgs[0].ToString());
            }

            if (chr.preppedSpell == null)
            {
                chr.WriteToDisplay("Failed to find the spell " + sArgs[0] + ".");
                return false;
            }

            long previousMagicSkill = chr.magic;
            chr.magic = Skills.GetSkillForLevel(15);

            chr.preppedSpell.CastSpell(chr, args);
            chr.preppedSpell = null;

            chr.magic = previousMagicSkill;

            return true;
        }
    }
}
