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
    [CommandAttribute("impkill", "Kill an NPC.", (int)Globals.eImpLevel.DEVJR, new string[] { },
        0, new string[] { "impkill <target in view>" }, Globals.ePlayerState.PLAYING)]
    public class ImpKillCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                Combat.DoDamage(chr, chr, chr.HitsFull + 1, false);
                return true;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[0], false, true);

            if (target != null)
            {
                if (!chr.IsInvisible && chr.IsPC)
                {
                    target.SendToAllInSight((target.race != "" ? target.Name : "The " + target.Name) + " has been smitten by the Ghods!");

                    if (target.deathSound != "") { target.EmitSound(target.deathSound); }
                }

                Combat.DoDamage(target, chr, target.HitsFull + 1, false);

                return true;
            }
            else
            {
                if (args != null && args.ToLower() == "genocide")
                {
                    foreach(NPC npc in new System.Collections.Generic.List<Character>(Character.NPCInGameWorld))
                    {
                        if(npc.MapID == chr.MapID)
                            Combat.DoDamage(npc, chr, npc.HitsFull + 1, false);
                    }
                    
                }
                else
                    chr.WriteToDisplay(GameSystems.Text.TextManager.NullTargetMessage(sArgs[0]));
            }

            return false;
        }
    }
}
