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
    [CommandAttribute("impwarmspell", "Warm a spell as if it was in your spellbook or item.", (int)Globals.eImpLevel.DEVJR, new string[] { "impprep", "impwarm" },
        0, new string[] { "impwarm <spell command>" }, Globals.ePlayerState.PLAYING)]
    public class ImpWarmSpellCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("What spell do you want to cast?");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            chr.preppedSpell = GameSpell.GetSpell(sArgs[0].ToLower());

            if (chr.preppedSpell == null)
            {
                chr.WriteToDisplay("Failed to find the spell " + sArgs[0] + ".");
                return false;
            }

            chr.preppedSpell.WarmSpell(chr);

            chr.SendToAllInSight(chr.Name + ": " + GameSpell.GenerateMagicWords());

            return true;
        }
    }
}
