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
using DragonsSpine.GameWorld;

namespace DragonsSpine.Commands
{
    [CommandAttribute("search", "Search a corpse or for a secret door.", (int)Globals.eImpLevel.USER, 2,
        new string[] { "search corpse", "search # corpse", "search all (corpses)", "search <direction>" }, Globals.ePlayerState.PLAYING)]
    public class SearchCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Search what?");
                return true;
            }
            else if (chr.IsBlind)
            {
                chr.WriteToDisplay("You are blind and cannot see!");
                return true;
            }
            else if (!chr.HasNightVision && (chr.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Darkness) || chr.CurrentCell.IsAlwaysDark) && !chr.CurrentCell.AreaEffects.ContainsKey(Effect.EffectTypes.Light))
            {
                
                chr.WriteToDisplay("You cannot see anything in the darkness.");
                return true;
                
            }
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                switch (sArgs[0].ToLower())
                {
                    case "0":
                    case "corpse":
                        // Leave the corpse behind
                        Item itm = Item.FindItemOnGround(sArgs[0], chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z);
                        if (itm == null)
                        {
                            chr.WriteToDisplay("You don't see a corpse here.");
                            return new CommandTasker(chr)["look"];
                        }
                        // has to be a corpse
                        Corpse.DumpCorpse(itm as Corpse, Cell.GetCell(chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z));

                        // coin generated on npc creation
                        //Look
                        GameCommand.GameCommandDictionary["look"].Handler.OnCommand(chr, "");
                        break;
                    case "all":
                        foreach (Item corpse in new System.Collections.Generic.List<Item>(chr.CurrentCell.Items))
                        {
                            if (corpse != null && corpse.name == "corpse")
                                Corpse.DumpCorpse(corpse as Corpse, chr.CurrentCell);
                        }
                        GameCommand.GameCommandDictionary["look"].Handler.OnCommand(chr, "");
                        break;
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                    case "8":
                    case "9":
                    case "10":
                    case "11":
                    case "12":
                        int countTo = 0;
                        int corpseNum = Convert.ToInt16(sArgs[0]);

                        foreach (Item cItem in chr.CurrentCell.Items)
                            if (cItem is Corpse) { countTo++; }

                        if (countTo < corpseNum)
                        {
                            chr.WriteToDisplay("There aren't that many corpses here.");
                            break;
                        }
                        countTo = 0;
                        foreach (Item cItem in chr.CurrentCell.Items)
                        {
                            if (cItem is Corpse)
                            {
                                countTo++;
                                if (countTo == corpseNum)
                                {
                                    Corpse.DumpCorpse(cItem as Corpse, Cell.GetCell(chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z));
                                    break;
                                }
                            }
                        }
                        GameCommand.GameCommandDictionary["look"].Handler.OnCommand(chr, "");
                        break;
                    default:
                        // if secret door cell has a flag cell lock check for flags
                        if (Array.IndexOf(Map.GetCellRelevantToCell(chr.CurrentCell, sArgs[0].ToLower(), true).cellLock.lockTypes,
                            Cell.CellLock.LockType.Flag) != -1)
                        {
                            if (!chr.QuestFlags.Contains(Map.GetCellRelevantToCell(chr.CurrentCell, sArgs[0].ToLower(), true).cellLock.key))
                            {
                                if (!chr.ContentFlags.Contains(Map.GetCellRelevantToCell(chr.CurrentCell, sArgs[0].ToLower(), true).cellLock.key))
                                {
                                    chr.WriteToDisplay("You don't meet the requirements to open the door.");
                                    break;
                                }
                            }
                        }

                        if (!Map.CheckSecretDoor(Map.GetCellRelevantToCell(chr.CurrentCell, sArgs[0].ToLower(), true)))
                        {
                            if (chr.gender == Globals.eGender.Female)
                            {
                                chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.FemaleHmm));
                            }
                            else if (chr.gender == Globals.eGender.Male)
                            {
                                chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.MaleHmm));
                            }
                        }
                        break;
                }
            }

            return true;
        }
    }
}
