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
    [CommandAttribute("cast", "Cast a warmed spell or a spell from an item.", (int)Globals.eImpLevel.USER, new string[] { "c" },
        1, new string[] { "cast <target>", "cast <direction>", "cast <power> <direction>" }, Globals.ePlayerState.PLAYING)]
    public class CastCommand : ICommandHandler
    {
        //TODO: Create a single method to handle casting spells from items. 1/6/2017 -Eb
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Cast command not processed.");
                return true;
            }

            chr.CommandType = CommandTasker.CommandType.Cast;

            string outOfCharges = "";

            if (args == null)
                goto normalSpell;

            string[] sArgs = args.Split(" ".ToCharArray());

            #region Cast a spell from an Item
            try
            {
                #region Right Hand
                if (chr.RightHand != null && chr.RightHand.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.RightHand.spell).Command)
                {
                    if (chr.RightHand.charges <= 0)
                    {
                        if (chr.RightHand.baseType == Globals.eItemBaseType.Scroll)
                            outOfCharges += "The " + chr.RightHand.name + " does not contain the proper runes to cast a spell. Perhaps it can be scribed.";
                        else outOfCharges += "The " + chr.RightHand.name + " is out of charges. ";
                        goto checkLeftHand;
                    }

                    var storedSpell = chr.preppedSpell;
                    var storedMagicSkill = chr.magic;

                    chr.preppedSpell = GameSpell.GetSpell(chr.RightHand.spell);

                    if (chr.RightHand.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.RightHand.spellPower);

                    chr.preppedSpell.CastSpell(chr, args);

                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;

                    if (chr.RightHand.charges > 0 && chr.RightHand.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.RightHand.charges--;

                        // scrolls with charges disintegrate when they reach 0 charges
                        if (chr.RightHand.baseType == Globals.eItemBaseType.Scroll && chr.RightHand.charges == 0)
                        {
                            chr.WriteToDisplay("The " + chr.RightHand.name + " disintegrates.");
                            chr.UnequipRightHand(chr.RightHand);
                        }
                    }
                    return true;
                }
                #endregion
            checkLeftHand:
                #region Left Hand
                if (chr.LeftHand != null && chr.LeftHand.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.LeftHand.spell).Command)
                {
                    if (chr.LeftHand.charges <= 0)
                    {
                        if (chr.LeftHand.baseType == Globals.eItemBaseType.Scroll)
                            outOfCharges += "The " + chr.LeftHand.name + " does not contain the proper runes to cast a spell. Perhaps it can be scribed.";
                        else outOfCharges += "The " + chr.LeftHand.name + " is out of charges. ";
                        goto checkRightRing1;
                    }
                    var storedSpell = chr.preppedSpell;
                    var storedMagicSkill = chr.magic;
                    chr.preppedSpell = GameSpell.GetSpell(chr.LeftHand.spell);
                    if (chr.LeftHand.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.LeftHand.spellPower);
                    chr.preppedSpell.CastSpell(chr, args);
                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;
                    if (chr.LeftHand.charges > 0 && chr.LeftHand.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.LeftHand.charges--;

                        // scrolls with charges disintegrate when they reach 0 charges
                        if (chr.LeftHand.baseType == Globals.eItemBaseType.Scroll && chr.LeftHand.charges == 0)
                        {
                            chr.WriteToDisplay("The " + chr.LeftHand.name + " disintegrates.");
                            chr.UnequipLeftHand(chr.LeftHand);
                        }
                    }
                    return true;
                }
                #endregion
            checkRightRing1:
                #region Right Ring 1
                if (chr.RightRing1 != null && chr.RightRing1.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.RightRing1.spell).Command)
                {
                    if (chr.RightRing1.charges == 0)
                    {
                        outOfCharges += "The " + chr.RightRing1.name + " is out of charges. ";
                        goto checkRightRing2;
                    }
                    GameSpell storedSpell = chr.preppedSpell;
                    long storedMagicSkill = chr.magic;
                    chr.preppedSpell = GameSpell.GetSpell(chr.RightRing1.spell);
                    if (args == null)
                    {
                        args = chr.preppedSpell.Command + " " + chr.Name;
                    }
                    if (chr.RightRing1.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.RightRing1.spellPower);
                    chr.preppedSpell.CastSpell(chr, args);
                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;
                    if (chr.RightRing1.charges > 0 && chr.RightRing1.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.RightRing1.charges--;
                    }
                    return true;
                }
                #endregion
            checkRightRing2:
                #region Right Ring 2
                if (chr.RightRing2 != null && chr.RightRing2.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.RightRing2.spell).Command)
                {
                    if (chr.RightRing2.charges == 0)
                    {
                        outOfCharges += "The " + chr.RightRing2.name + " is out of charges. ";
                        goto checkRightRing3;
                    }
                    GameSpell storedSpell = chr.preppedSpell;
                    long storedMagicSkill = chr.magic;
                    chr.preppedSpell = GameSpell.GetSpell(chr.RightRing2.spell);
                    if (args == null)
                    {
                        args = chr.preppedSpell.Command + " " + chr.Name;
                    }
                    if (chr.RightRing2.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.RightRing2.spellPower);
                    chr.preppedSpell.CastSpell(chr, args);
                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;
                    if (chr.RightRing2.charges > 0 && chr.RightRing2.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.RightRing2.charges--;
                    }
                    return true;
                }
                #endregion
            checkRightRing3:
                #region Right Ring 3
                if (chr.RightRing3 != null && chr.RightRing3.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.RightRing3.spell).Command)
                {
                    if (chr.RightRing3.charges == 0)
                    {
                        outOfCharges += "The " + chr.RightRing3.name + " is out of charges. ";
                        goto checkRightRing4;
                    }
                    GameSpell storedSpell = chr.preppedSpell;
                    long storedMagicSkill = chr.magic;
                    chr.preppedSpell = GameSpell.GetSpell(chr.RightRing3.spell);
                    if (args == null)
                    {
                        args = chr.preppedSpell.Command + " " + chr.Name;
                    }
                    if (chr.RightRing3.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.RightRing3.spellPower);
                    chr.preppedSpell.CastSpell(chr, args);
                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;
                    if (chr.RightRing3.charges > 0 && chr.RightRing3.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.RightRing3.charges--;
                    }
                    return true;
                }
                #endregion
            checkRightRing4:
                #region Right Ring 4
                if (chr.RightRing4 != null && chr.RightRing4.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.RightRing4.spell).Command)
                {
                    if (chr.RightRing4.charges == 0)
                    {
                        outOfCharges += "The " + chr.RightRing4.name + " is out of charges. ";
                        goto checkLeftRing1;
                    }
                    GameSpell storedSpell = chr.preppedSpell;
                    long storedMagicSkill = chr.magic;
                    chr.preppedSpell = GameSpell.GetSpell(chr.RightRing4.spell);
                    if (args == null)
                    {
                        args = chr.preppedSpell.Command + " " + chr.Name;
                    }
                    if (chr.RightRing4.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.RightRing4.spellPower);
                    chr.preppedSpell.CastSpell(chr, args);
                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;
                    if (chr.RightRing4.charges > 0 && chr.RightRing4.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.RightRing4.charges--;
                    }
                    return true;
                }
                #endregion
            checkLeftRing1:
                #region Left Ring 1
                if (chr.LeftRing1 != null && chr.LeftRing1.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.LeftRing1.spell).Command)
                {
                    if (chr.LeftRing1.charges == 0)
                    {
                        outOfCharges += "The " + chr.LeftRing1.name + " is out of charges. ";
                        goto checkLeftRing2;
                    }
                    GameSpell storedSpell = chr.preppedSpell;
                    long storedMagicSkill = chr.magic;
                    chr.preppedSpell = GameSpell.GetSpell(chr.LeftRing1.spell);
                    if (args == null)
                    {
                        args = chr.preppedSpell.Command + " " + chr.Name;
                    }
                    if (chr.LeftRing1.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.LeftRing1.spellPower);
                    chr.preppedSpell.CastSpell(chr, args);
                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;
                    if (chr.LeftRing1.charges > 0 && chr.LeftRing1.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.LeftRing1.charges--;
                    }
                    return true;
                }
                #endregion
            checkLeftRing2:
                #region Left Ring 2
                if (chr.LeftRing2 != null && chr.LeftRing2.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.LeftRing2.spell).Command)
                {
                    if (chr.LeftRing2.charges == 0)
                    {
                        outOfCharges += "The " + chr.LeftRing2.name + " is out of charges. ";
                        goto checkLeftRing3;
                    }
                    GameSpell storedSpell = chr.preppedSpell;
                    long storedMagicSkill = chr.magic;
                    chr.preppedSpell = GameSpell.GetSpell(chr.LeftRing2.spell);
                    if (args == null)
                    {
                        args = chr.preppedSpell.Command + " " + chr.Name;
                    }
                    if (chr.LeftRing2.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.LeftRing2.spellPower);
                    chr.preppedSpell.CastSpell(chr, args);
                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;
                    if (chr.LeftRing2.charges > 0 && chr.LeftRing2.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.LeftRing2.charges--;
                    }
                    return true;
                }
                #endregion
            checkLeftRing3:
                #region Left Ring 3
                if (chr.LeftRing3 != null && chr.LeftRing3.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.LeftRing3.spell).Command)
                {
                    if (chr.LeftRing3.charges == 0)
                    {
                        outOfCharges += "The " + chr.LeftRing3.name + " is out of charges. ";
                        goto checkLeftRing4;
                    }
                    GameSpell storedSpell = chr.preppedSpell;
                    long storedMagicSkill = chr.magic;
                    chr.preppedSpell = GameSpell.GetSpell(chr.LeftRing3.spell);
                    if (args == null)
                    {
                        args = chr.preppedSpell.Command + " " + chr.Name;
                    }
                    if (chr.LeftRing3.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.LeftRing3.spellPower);
                    chr.preppedSpell.CastSpell(chr, args);
                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;
                    if (chr.LeftRing3.charges > 0 && chr.LeftRing3.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.LeftRing3.charges--;
                    }
                    return true;
                }
                #endregion
            checkLeftRing4:
                #region Left Ring 4
                if (chr.LeftRing4 != null && chr.LeftRing4.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(chr.LeftRing4.spell).Command)
                {
                    if (chr.LeftRing4.charges == 0)
                    {
                        outOfCharges += "The " + chr.LeftRing4.name + " is out of charges. ";
                        goto checkInventory;
                    }
                    GameSpell storedSpell = chr.preppedSpell;
                    long storedMagicSkill = chr.magic;
                    chr.preppedSpell = GameSpell.GetSpell(chr.LeftRing4.spell);
                    if (args == null)
                    {
                        args = chr.preppedSpell.Command + " " + chr.Name;
                    }
                    if (chr.LeftRing4.spellPower <= 0)
                        chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                    else chr.magic = Skills.GetSkillForLevel(chr.LeftRing4.spellPower);
                    chr.preppedSpell.CastSpell(chr, args);
                    chr.preppedSpell = storedSpell;
                    chr.magic = storedMagicSkill;
                    if (chr.LeftRing4.charges > 0 && chr.LeftRing4.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                    {
                        chr.LeftRing4.charges--;
                    }
                    return true;
                }
                #endregion
            checkInventory:
                #region Inventory

                for (int a = 0; a < chr.wearing.Count; a++)
                {
                    Item item = (Item)chr.wearing[a];
                    if (item != null && item.spell > 0 && sArgs[0].ToLower() == GameSpell.GetSpell(item.spell).Command)
                    {
                        if (item.charges == 0)
                        {
                            if(item.spell != (int)GameSpell.GameSpellID.Venom)
                                outOfCharges += "The " + item.name + " is out of charges. ";
                        }
                        else
                        {
                            GameSpell storedSpell = chr.preppedSpell;
                            long storedMagicSkill = chr.magic;
                            chr.preppedSpell = GameSpell.GetSpell(item.spell);

                            // Found an item with the spell venom. This will mean the item may be charged with venom.
                            if (storedSpell != null && storedSpell.ID != (int)GameSpell.GameSpellID.Venom && chr.preppedSpell.ID == (int)GameSpell.GameSpellID.Venom && sArgs[0].ToLower() == "venom")
                            {
                                chr.WriteToDisplay("Items must be envenomed, or recharged with venom, by the spell.");
                                chr.preppedSpell = storedSpell;
                                continue;
                            }

                            if (args == null)
                            {
                                args = chr.preppedSpell.Command + " " + chr.Name;
                            }
                            if (item.spellPower <= 0)
                                chr.magic = Skills.GetSkillForLevel(chr.preppedSpell.RequiredLevel);
                            else chr.magic = Skills.GetSkillForLevel(item.spellPower);
                            chr.preppedSpell.CastSpell(chr, args);
                            chr.preppedSpell = storedSpell;
                            chr.magic = storedMagicSkill;
                            if (item.charges > 0 && item.charges < Item.NUM_FOR_UNLIMITED_CHARGES)
                            {
                                item.charges--;
                            }
                            chr.wearing[a] = item;
                            return true;
                        }
                    }
                }
                #endregion
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
            #endregion

            #region Knight or Ravager Spell
            if (chr.BaseProfession == Character.ClassType.Knight || chr.BaseProfession == Character.ClassType.Ravager) // caster is a knight or ravager
            {
                if (chr.HasKnightRing) // knight or ravager is wearing their ring
                {
                    try
                    {
                        if (chr.IsPC) // npc knights already prepped their spell in Creature.prepareSpell
                        {
                            GameSpell spell = GameSpell.GetSpell(sArgs[0].ToLower());

                            if (spell != null && spell.IsClassSpell(chr.BaseProfession) && spell.IsAvailableAtTrainer && chr.Level >= spell.RequiredLevel)
                            {
                                chr.preppedSpell = GameSpell.GetSpell(sArgs[0].ToLower());
                            }
                            else
                            {
                                if (outOfCharges != "")
                                {
                                    chr.WriteToDisplay(outOfCharges);
                                }
                                else
                                {
                                    chr.WriteToDisplay("You do not know that spell. You have not learned it or do not meet the level requirement to cast it.");
                                }
                                return true;
                            }
                        }

                        if (chr.preppedSpell != null)
                        {
                            // lawful knight or evil ravager
                            if ((chr.Alignment == Globals.eAlignment.Lawful && chr.BaseProfession == Character.ClassType.Knight) ||
                                (chr.Alignment == Globals.eAlignment.Evil && chr.BaseProfession == Character.ClassType.Ravager))
                            {
                                if (chr.Mana < chr.preppedSpell.ManaCost) // knight does not have enough mana
                                {
                                    chr.WriteToDisplay("You do not have enough mana to cast the spell."); // message
                                    chr.preppedSpell = null; // remove the prepped spell
                                    return true;
                                }
                                chr.preppedSpell.CastSpell(chr, args);
                                chr.Mana -= chr.preppedSpell.ManaCost;
                                chr.preppedSpell = null;
                                chr.updateMP = true;
                            }
                            else // someone other than a lawful knight or evil ravager is using a ring with the "knight effect"
                            {
                                Item ring = null;
                                chr.preppedSpell = null;
                                for (int rnum = 1; rnum < 5; rnum++)
                                {
                                    // check right
                                    ring = chr.GetSpecificRing(true, rnum);
                                    if (ring != null && (ring.itemID == Item.ID_KNIGHTRING || ring.itemID == Item.ID_RAVAGERRING))
                                    {
                                        chr.WriteToDisplay("Your " + ring.identifiedName + " explodes!");
                                        Combat.DoSpellDamage(chr, chr, null, Rules.RollD(2, 12), "concussion");
                                        chr.SetSpecificRing(true, rnum, null);
                                    }
                                    // check left
                                    ring = chr.GetSpecificRing(false, rnum);
                                    if (ring != null && (ring.itemID == Item.ID_KNIGHTRING || ring.itemID == Item.ID_RAVAGERRING))
                                    {
                                        chr.WriteToDisplay("Your " + ring.identifiedName + " explodes!");
                                        Combat.DoSpellDamage(chr, chr, null, Rules.RollD(2, 12), "concussion");
                                        chr.SetSpecificRing(false, rnum, null);
                                    }
                                }
                            }
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        Utils.Log("Command.cast(" + args + ") by " + chr.GetLogString(), Utils.LogType.CommandFailure);
                        Utils.LogException(e);
                        return true;
                    }
                }
                else
                {
                    chr.WriteToDisplay("You are not wearing your " + Utils.FormatEnumString(chr.BaseProfession.ToString()) + "'s ring.");
                    return true;
                }
            }
            #endregion

        normalSpell:
            #region Normal Spell
            try
            {
                bool memorizedSpell = false;

                // Check if a spell is not prepared. Spells ALWAYS go to the preppedSpell object before being cast.
                if (chr.preppedSpell == null)
                {
                    if (outOfCharges != "")
                    {
                        chr.WriteToDisplay(outOfCharges);
                        chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                    }
                    else
                    {
                        chr.WriteToDisplay("You don't have a spell warmed.") ;
                    }

                    if (!string.IsNullOrEmpty(chr.MemorizedSpellChant))
                    {
                        if(chr.HasEffect(Effect.EffectTypes.Silence))
                        {
                            chr.WriteToDisplay("You have been silenced.");
                            return true;
                        }

                        memorizedSpell = true;
                        string[] mArgs = chr.MemorizedSpellChant.Split(" ".ToCharArray());
                        chr.CommandWeight = 0;
                        CommandTasker.ParseCommand(chr, mArgs[0], mArgs[1] + " " + mArgs[2] + " " + mArgs[3]);
                    }
                    else return true;
                }

                if (memorizedSpell)
                {
                    if (chr.Stamina < Talents.MemorizeTalent.STAMINA_COST)
                    {
                        chr.WriteToDisplay("You do not have enough stamina to cast your memorized spell.");
                        chr.preppedSpell = null;
                        chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                        return true;
                    }

                    chr.Stamina -= Talents.MemorizeTalent.STAMINA_COST; // reduce stamina whether the spell is cast or not, or whether if fails or succeeds
                }

                // memorized spell costs more mana
                int manaCost = memorizedSpell ? chr.preppedSpell.ManaCost + (chr.preppedSpell.ManaCost / 2) : chr.preppedSpell.ManaCost;                

                // Check spell failure for newbie thaumaturge players casting higher level spells.
                if (chr.IsWisdomCaster && chr.IsPC)
                {
                    if (Rules.CheckSpellFailure(Skills.GetSkillLevel(chr.magic), chr.preppedSpell.RequiredLevel))
                    {
                        chr.Mana -= manaCost;
                        chr.preppedSpell = null;
                        chr.WriteToDisplay("The spell fizzles.");
                        chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                        return true;
                    }
                }

                // Spell failure if not enough mana.
                if (chr.Mana < manaCost)
                {
                    chr.Mana -= manaCost;
                    chr.preppedSpell = null;
                    chr.WriteToDisplay("The spell fails.");
                    chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                    return true;
                }
                else
                {
                    // If no arguments then this spell will be cast at us (if it requires a target).
                    if (args == null)
                    {
                        args = chr.preppedSpell.Command + " " + chr.Name;
                    }

                    if (chr.preppedSpell.CastSpell(chr, args))
                    {
                        if (chr != null)
                        {
                            // give skill experience for casting a spell that requires mana
                            int magicSkillLevel = Skills.GetSkillLevel(chr.magic);

                            int bonusForHighSkill = 1;

                            if (magicSkillLevel >= 11) bonusForHighSkill = magicSkillLevel - 9;

                            int addend = chr.NumAttackers;

                            // Adjustment here for Thief profession, both because they are hidden when casting and lack numAttackers and they do not
                            // gain as much magic skill experience since they do not target enemies with spells.
                            if (chr.BaseProfession == Character.ClassType.Thief) addend += Skills.GetSkillLevel(chr.magic) / 2;

                            int skillAmount = (manaCost + addend) * magicSkillLevel * bonusForHighSkill;

                            Skills.GiveSkillExp(chr, skillAmount, Globals.eSkillType.Magic);

                            #region Give random experience if current map is 'magic intense'
                            if (chr.Map.HasRandomMagicIntensity && chr.IsSpellWarmingProfession && (Rules.RollD(1, 6) == Rules.RollD(1, 6)))
                            {
                                switch (chr.BaseProfession)
                                {
                                    case Character.ClassType.Wizard:
                                        chr.WriteToDisplay("Static electricity gathers around you and then dissipates gradually.");
                                        break;
                                    case Character.ClassType.Thaumaturge:
                                        chr.WriteToDisplay("An almost overwhelming feeling of serenity passes through you.");
                                        break;
                                    case Character.ClassType.Sorcerer:
                                        chr.WriteToDisplay("Black wisps of acrid smoke rapidly encircle your entire body and then slowly fade away.");
                                        break;
                                    default:
                                        chr.WriteToDisplay("You feel a rush of adrenalin coursing through your body.");
                                        break;

                                }

                                if(chr != null && chr.preppedSpell != null)
                                    Skills.GiveSkillExp(chr, manaCost * (Skills.GetSkillLevel(chr.magic) * Rules.RollD(2, 4)), Globals.eSkillType.Magic);
                            }
                            #endregion

                            if (chr.preppedSpell != null)
                            {
                                chr.Mana -= manaCost;
                                chr.preppedSpell = null;
                            }
                        }
                    }
                    else
                    {
                        if (chr != null)
                        {
                            if (chr.preppedSpell != null)
                                chr.Mana -= chr.preppedSpell.ManaCost;
                            chr.preppedSpell = null;
                            chr.WriteToDisplay(GameSystems.Text.TextManager.YOUR_SPELL_FAILS);
                            chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.SpellFail));
                        }
                    }
                }

                if (chr != null) // NPC could be null due to death
                {
                    chr.CommandWeight += 3;
                }

                return true;
            }
            catch (Exception e)
            {
                Utils.Log("Command.cast(" + args + ") by " + chr.GetLogString(), Utils.LogType.CommandFailure);
                Utils.LogException(e);
                return true;
            }
            #endregion
        }
    }
}
