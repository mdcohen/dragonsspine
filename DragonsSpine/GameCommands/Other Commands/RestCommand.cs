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
    [CommandAttribute("rest", "Start resting.", (int)Globals.eImpLevel.USER, 2, new string[] { "There are no arguments for the rest command." }, Globals.ePlayerState.PLAYING)]
    public class RestCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.preppedSpell != null) // lose spell
            {
                chr.preppedSpell = null;
                chr.WriteToDisplay("You have lost your warmed spell.");
                chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
            }

            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Rest command not processed.");
                return false;
            }

            if (chr.IsWizardEye)
            {
                chr.EffectsList[Effect.EffectTypes.Wizard_Eye].StopCharacterEffect();
                return true;
            }
            else if (chr.IsPeeking)
            {
                chr.EffectsList[Effect.EffectTypes.Peek].StopCharacterEffect();
                return true;
            }
            else if (chr.IsDead && chr is PC) // if the character is dead and is a player
            {
                Rules.DeadRest(chr as PC);
                return true;
            }

            if(chr.EffectsList.ContainsKey(Effect.EffectTypes.Ensnare))
            {
                chr.WriteToDisplay("You are ensnared and thus unable to rest properly.");
                return false;
            }

            foreach (Effect.EffectTypes effectType in chr.CurrentCell.AreaEffects.Keys)
            {
                if (MeditateCommand.NoMedOrRestEffects.Contains(effectType))
                {
                    chr.WriteToDisplay("You find it difficult to meditate in the " + Utils.FormatEnumString(effectType.ToString()) + ".");
                    return false;
                }
            }

            if (chr.EffectsList.ContainsKey(Effect.EffectTypes.Contagion))
            {
                chr.WriteToDisplay("You are diseased and thus unable to rest properly.");
                return false;
            }

            if (chr.Poisoned > 0)
            {
                chr.WriteToDisplay("You are poisoned and thus unable to rest properly.");
                return false;
            }

            System.Collections.Generic.List<string> DisallowedRestCells = new System.Collections.Generic.List<string>()
            {
                GameWorld.Cell.GRAPHIC_OPEN_DOOR_HORIZONTAL, GameWorld.Cell.GRAPHIC_OPEN_DOOR_VERTICAL
            };

            if (GameWorld.Map.IsNextToCounter(chr) ||
                DisallowedRestCells.Contains(chr.CurrentCell.DisplayGraphic) ||
                chr.CurrentCell.IsLair || chr.CurrentCell.IsLocker || chr.CurrentCell.IsOrnicLocker || chr.CurrentCell.IsMapPortal)
            {
                chr.WriteToDisplay("You find it difficult to rest here.");
                return false;
            }

            chr.CommandType = CommandTasker.CommandType.Rest;

            // character starts to rest
            chr.IsResting = true;
            chr.IsMeditating = false; // stop meditating

            // Regeneration now takes place in round event if IsResting is true.
            #region Regenerate hits, stamina and mana if not damaged in the last 3 rounds, and not diseased (EffectType.Contagion).
            //if (!chr.EffectsList.ContainsKey(Effect.EffectType.Contagion) && chr.damageRound < DragonsSpineMain.GameRound - 3 && !chr.IsImmortal)
            //{
            //    if (chr.Hits < chr.HitsFull)  // increase stats
            //    {
            //        chr.Hits++;
            //        if (chr.hitsRegen > 0 && chr.Hits < chr.HitsFull) // gain additional mana if hpregen item/effect is active
            //        {
            //            chr.Hits += chr.hitsRegen;
            //            if (chr.Hits > chr.HitsFull) { chr.Hits = chr.HitsFull; } // confirm we didn't regen more hits than we have
            //        }
            //    }
            //    if (chr.Stamina < chr.StaminaFull)
            //    {
            //        chr.Stamina++;
            //        if (chr.staminaRegen > 0 && chr.Stamina < chr.StaminaFull)
            //        {
            //            chr.Stamina += chr.staminaRegen;
            //            if (chr.Stamina > chr.StaminaFull) { chr.Stamina = chr.StaminaFull; } // confirm we didn't regen more stamina than we have
            //        }
            //    }
            //    if (chr.Mana < chr.ManaFull)
            //    {
            //        chr.Mana++;
            //        if (chr.manaRegen > 0 && chr.Mana < chr.ManaFull) // gain additional mana if manaRegeneration item or effect is active
            //        {
            //            chr.Mana += chr.manaRegen;
            //            if (chr.Mana > chr.ManaFull) { chr.Mana = chr.ManaFull; } // confirm we didn't regen more mana than we have
            //        }
            //    }
            //} 
            #endregion

            if (!chr.IsPC) { return true; } // end the rest now if this is not a player

            // do a level up if hits and stamina are at max
            #region Level Up Logic -- No NPCs
            if (chr.IsPC && Rules.GetExpLevel(chr.Experience) > chr.Level)
            {
                if (chr.Hits >= chr.HitsFull && chr.Stamina >= chr.StaminaFull)
                {
                    if (!chr.EffectsList.ContainsKey(Effect.EffectTypes.Contagion))
                    {
                        chr.Level += 1;

                        int hitsGain = Rules.GetHitsGain(chr, 1); // determine hits gain amount
                        int staminaGain = Rules.GetStaminaGain(chr, 1); // determine stamina gain amount
                        int hitsLimit = Rules.GetMaximumHits(chr); // get hits limit for class type
                        int staminaLimit = Rules.GetMaximumStamina(chr); // get stamina limit for class type

                        // perform adjustments if gain is over limit
                        if (chr.HitsMax + hitsGain > hitsLimit)
                            hitsGain = hitsLimit - chr.HitsMax;
                        if (chr.StaminaMax + staminaGain > staminaLimit)
                            staminaGain = staminaLimit - chr.StaminaMax;

                        chr.HitsMax += hitsGain; // add hitsGain to hitsmax
                        chr.StaminaMax += staminaGain; // add staminaGain to stamina

                        string pts = "point";
                        if (hitsGain != 1) { pts = "points"; }
                        chr.WriteToDisplay("You have gained " + hitsGain + " hit " + pts + ".");
                        if (staminaGain != 1) { pts = "points"; }
                        else { pts = "point"; }
                        chr.WriteToDisplay("You have gained " + staminaGain + " stamina " + pts + ".");

                        #region Mana for spell users
                        if (chr.IsSpellUser && !chr.IsHybrid)
                        {
                            int manaGain = Rules.GetManaGain(chr, 1);
                            int manaLimit = Rules.GetMaximumMana(chr);

                            if (chr.ManaMax + manaGain > manaLimit)
                                manaGain = manaLimit - chr.ManaMax;

                            if (manaGain != 1) { pts = "points"; }
                            else { pts = "point"; }
                            chr.ManaMax += manaGain;
                            chr.WriteToDisplay("You have gained " + manaGain + " mana " + pts + ".");
                        }
                        #endregion

                        #region Determine strength add based on 15+ strength, classType and level
                        if (chr.Strength >= 15) // add a strength add at appropriate level
                        {
                            int levelAdd = 7;

                            if (chr.IsPureMelee) // melee and hybrid
                                levelAdd = 5;
                            else if (chr.IsHybrid)
                                levelAdd = 6;

                            for (int a = levelAdd; a <= Globals.MAX_EXP_LEVEL; a += 4)
                            {
                                if (chr.Level == a)
                                {
                                    chr.strengthAdd += 1;
                                    chr.WriteToDisplay("You have gained 1 strength add.");
                                    break;
                                }
                            }
                        }
                        #endregion

                        #region Determine dexterity add based on 15+ dexterity, classType and level
                        if (chr.Dexterity >= 15)
                        {
                            int levelAdd = 7;

                            if (chr.IsPureMelee) // melee and hybrid
                                levelAdd = 5;
                            else if (chr.IsHybrid)
                                levelAdd = 6;

                            for (int a = levelAdd; a <= Globals.MAX_EXP_LEVEL; a += 4)
                            {
                                if (chr.Level == a)
                                {
                                    chr.dexterityAdd += 1;
                                    chr.WriteToDisplay("You have gained 1 dexterity add.");
                                    break;
                                }
                            }
                        }
                        #endregion

                        chr.SendSound(Sound.GetCommonSound(Sound.CommonSound.LevelUp));

                        chr.WriteToDisplay("You are now a level " + chr.Level + " " + chr.classFullName.ToLower() + "!!");
                        
                        chr.Hits = chr.HitsFull;
                        chr.Stamina = chr.StaminaFull;
                        chr.Mana = chr.ManaFull;

                        if (chr is PC && chr.protocol == DragonsSpineMain.Instance.Settings.DefaultProtocol)
                            ProtocolYuusha.SendCharacterStats(chr as PC, chr);
                    }
                    else chr.WriteToDisplay("You cannot level up until you remove your contagion.");
                }
            }
            #endregion

            return true;
        }
    }
}
