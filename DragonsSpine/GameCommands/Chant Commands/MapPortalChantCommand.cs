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
using System.Linq;
using System.Text;
using DemonName = DragonsSpine.Commands.DemonSummoningChantCommand.DemonName;

namespace DragonsSpine.Commands
{
    [CommandAttribute("ashtugnindaanghiziddaarrflug", "Speaking the portal chant at a designated location will teleport you to another area.",
        (int)Globals.eImpLevel.USER, new string[] { "ashtug ninda anghizidda arrflug" }, 3, new string[] { "There are no arguments for this chant." },
        Globals.ePlayerState.PLAYING)]
    public class MapPortalChantCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            chr.CommandType = CommandTasker.CommandType.Chant;
            chr.SendToAllInSight(chr.Name + ": " + GameSystems.Text.TextManager.CHANT_PORTAL);

            if (chr.CommandWeight <= 3)
            {
                //portal failure if character is an ancestor in the Underworld
                if (chr.InUnderworld && (chr as PC).IsAncestor)
                {
                    chr.WriteToDisplay("Your time in the realm of the living has ended. You will never return.");
                    return true;
                }

                #region Underworld Related
                //portal failure if character is in the Underworld and has not completed all four quests
                if (chr.InUnderworld && (!(chr as PC).UW_hasIntestines || !(chr as PC).UW_hasLiver || !(chr as PC).UW_hasLungs || !(chr as PC).UW_hasStomach))
                {
                    chr.WriteToDisplay("You cannot return to the realm of the living until you are whole.");
                    return true;
                }
                if (chr.InUnderworld && ((chr as PC).UW_hasIntestines || (chr as PC).UW_hasLiver || (chr as PC).UW_hasLungs || (chr as PC).UW_hasStomach))
                {
                    // send them back to kesmai
                    if (chr.CurrentCell.X == 115 && chr.CurrentCell.Y == 27 && chr.CurrentCell.Z == 0) // todo: better way of handling this
                    {
                        Rules.ReturnFromUnderworld(chr);
                        return true;
                    }
                    else
                    {
                        chr.WriteToDisplay(GameSystems.Text.TextManager.VISION_BLUR);
                        return true;
                    }
                } 
                #endregion

                if (chr.CurrentCell.IsMapPortal)
                {
                    #region fail to portal if holding a corpse
                    if (chr.RightHand != null && chr.RightHand.itemType == Globals.eItemType.Corpse)
                    {
                        chr.WriteToDisplay(GameSystems.Text.TextManager.VISION_BLUR);
                        return true;
                    }
                    if (chr.LeftHand != null && chr.LeftHand.itemType == Globals.eItemType.Corpse)
                    {
                        chr.WriteToDisplay(GameSystems.Text.TextManager.VISION_BLUR);
                        return true;
                    }
                    #endregion

                    chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MapPortal));

                    GameWorld.Segue segue = chr.CurrentCell.Segue;

                    if (segue != null)
                    {
                        // One Way Portal logic commented out on 12/10/2015 Eb -- portalling between the lands will be allowed for now
                        //if (segue.LandID != chr.CurrentCell.LandID)
                        //{
                        //    Rules.OneWayPortal(chr as PC, World.GetFacetByID(chr.FacetID).GetLandByID(segue.LandID));
                        //}

                        chr.SendSoundToAllInRange(Sound.GetCommonSound(Sound.CommonSound.MapPortal)); // send sound as portal occurs
                        chr.CurrentCell = GameWorld.Cell.GetCell(chr.FacetID, segue.LandID, segue.MapID, segue.X, segue.Y, segue.Z); // change cell to segue

                        if (chr.IsHidden) // break hide
                        {
                            chr.IsHidden = false;
                        }

                        int recallReset = 0;

                        // reset recall rings when portal used
                        foreach (Item ring in chr.GetRings())
                        {
                            if (ring.isRecall)
                            {
                                ring.isRecall = false;
                                ring.wasRecall = true;
                                recallReset++;
                            }
                        }

                        if (recallReset > 0)
                        {
                            chr.SendSound(Sound.GetCommonSound(Sound.CommonSound.RecallReset));

                            if (recallReset == 1)
                            {
                                chr.WriteToDisplay("Your recall ring has been cleared! You must remove and reset it.");
                            }
                            else if (recallReset > 1)
                            {
                                chr.WriteToDisplay("Your recall rings have been cleared! You must remove and reset them.");
                            }
                        }

                        chr.dirPointer = "^";
                    }
                    else
                    {
                        return false;
                    }
                }
                else chr.WriteToDisplay(GameSystems.Text.TextManager.VISION_BLUR);
            }
            else
            {
                chr.WriteToDisplay("Enabling portal magic requires your full concentration.");
            }
            return true;
        }
    }
}

