using System;
using ArrayList = System.Collections.ArrayList;

namespace DragonsSpine.Spells
{
    // The identify has always gathered different information than a shopkeeper.
    // Perhaps it is time to make them the same. 1/25/2017 Eb
    [SpellAttribute(GameSpell.GameSpellID.Identify, "identify", "Identify", "Gather information, through divination, about an item.",
        Globals.eSpellType.Divination, Globals.eSpellTargetType.Self, 5, 3, 400, "0226", true, true, false, true, false, Character.ClassType.Thief)]
    public class IdentifySpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            try
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                Item iditem = caster.FindHeldItem(args);

                if (iditem == null)
                {
                    if (caster.RightHand != null)
                    {
                        iditem = caster.RightHand;
                    }
                    else if (caster.LeftHand != null)
                    {
                        iditem = caster.LeftHand;
                    }
                    else
                    {
                        caster.WriteToDisplay("You must hold the item to identify in your hands.");
                        return false;
                    }
                }

                var itmeffect = "";
                var itmspell = "";
                var itmspecial = "";
                var itmalign = "";
                var itmattuned = "";

                ReferenceSpell.SendGenericCastMessage(caster, caster, true);

                #region Spell and Charges
                if (iditem.spell > 0)
                {
                    var spell = GameSpell.GetSpell(iditem.spell);

                    itmspell = " It contains the spell of " + spell.Name;

                    if (iditem.charges == 0)
                    {
                        if (iditem.baseType == Globals.eItemBaseType.Scroll)
                        {
                            if (caster.IsSpellWarmingProfession && spell.IsClassSpell(caster.BaseProfession))
                                itmspell += " that you may scribe into your spellbook.";
                            else itmspell += " that can be scribed into a spellbook.";
                        }
                        else itmspell += ", but there are no charges remaining.";
                    }
                    else if (iditem.charges > 100) { itmspell += " with unlimited charges."; }
                    else if (iditem.charges > 1) { itmspell += " with " + iditem.charges + " charges remaining."; }
                    else if (iditem.charges == 1) { itmspell += " with 1 charge remaining."; }
                    else // -1 or less
                    {
                        itmspell += " with unlimited charges.";
                    }
                } 
                #endregion

                var sb = new System.Text.StringBuilder(100);

                // Figurine info.
                if (iditem.baseType == Globals.eItemBaseType.Figurine || iditem.figExp > 0)
                    sb.AppendFormat(" The {0}'s avatar has " + iditem.figExp + " experience.", iditem.name);

                // Combat adds.
                if (iditem.combatAdds > 0)
                    sb.AppendFormat(" The combat adds are {0}.", iditem.combatAdds);

                // Silver or mithril silver.
                if (iditem.silver)
                {
                    string silver = "silver";

                    if (iditem.longDesc.ToLower().Contains("mithril") || iditem.armorType == Globals.eArmorType.Mithril)
                        silver = "mithril silver";

                    sb.AppendFormat(" The {0} is " + silver + ".", iditem.name);
                }

                // Blue glow.
                if (iditem.blueglow)
                    sb.AppendFormat(" The {0} is emitting a faint blue glow.", iditem.name);

                itmspecial = sb.ToString();

                //item effects
                #region Enchantments
                if (iditem.effectType.Length > 0)
                {
                    string[] itmEffectType = iditem.effectType.Split(" ".ToCharArray());
                    string[] itmEffectAmount = iditem.effectAmount.Split(" ".ToCharArray()); // GameSpell IDs for procs

                    #region Enchantment Effects
                    if (itmEffectType.Length == 1 && Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) != "")
                    {
                        if (iditem.baseType == Globals.eItemBaseType.Bottle)
                        {
                            itmeffect = " Inside the bottle is a potion of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                        }
                        else
                        {
                            string effectName = Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0]));

                            if (effectName.ToLower() != Effect.GetEffectName(Effect.EffectTypes.None).ToLower() && effectName.ToLower() != Effect.GetEffectName(Effect.EffectTypes.Weapon_Proc))
                                itmeffect = " The " + iditem.name + " contains the enchantment of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                        }
                    }
                    else
                    {
                        var itemEffectList = new ArrayList();

                        for (int a = 0; a < itmEffectType.Length; a++)
                        {
                            Effect.EffectTypes effectType = (Effect.EffectTypes)Convert.ToInt32(itmEffectType[a]);

                            if (effectType != Effect.EffectTypes.None &&
                                effectType != Effect.EffectTypes.Weapon_Proc)
                            {
                                itemEffectList.Add(Effect.GetEffectName(effectType));
                            }
                        }

                        if (itemEffectList.Count > 0)
                        {
                            if (itemEffectList.Count > 1)
                            {
                                itmeffect = " The " + iditem.name + " contains the enchantments of";
                                for (int a = 0; a < itemEffectList.Count; a++)
                                {
                                    if (a != itemEffectList.Count - 1)
                                    {
                                        itmeffect += " " + (string)itemEffectList[a] + ",";
                                    }
                                    else
                                    {
                                        itmeffect += " and " + (string)itemEffectList[a] + ".";
                                    }
                                }
                            }
                            else if (itemEffectList.Count == 1)
                            {
                                string effectName = Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0]));

                                if (effectName.ToLower() != "none")
                                {
                                    if (iditem.baseType == Globals.eItemBaseType.Bottle)
                                    {
                                        itmeffect = " Inside the bottle is a potion of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                                    }
                                    else
                                    {
                                        itmeffect = " The " + iditem.name + " contains the enchantment of " + Effect.GetEffectName((Effect.EffectTypes)Convert.ToInt32(itmEffectType[0])) + ".";
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                } 
                #endregion

                // Identify spell currently doesn't display weapon procs. 2/10/2017 Eb

                #region Alignment
                //item alignment
                if (iditem.alignment != Globals.eAlignment.None)
                {
                    string aligncolor = "";
                    switch (iditem.alignment)
                    {
                        case Globals.eAlignment.Lawful:
                            aligncolor = "white";
                            break;
                        case Globals.eAlignment.Neutral:
                            aligncolor = "green";
                            break;
                        case Globals.eAlignment.Chaotic:
                            aligncolor = "purple";
                            break;
                        case Globals.eAlignment.ChaoticEvil:
                        case Globals.eAlignment.Evil:
                            aligncolor = "red";
                            break;
                        case Globals.eAlignment.Amoral:
                            aligncolor = "yellow";
                            break;
                        default:
                            break;
                    }
                    itmalign = " The " + iditem.name + " briefly pulses with a " + aligncolor + " glow.";
                }
                #endregion

                #region Attuned
                //item attuned
                if (iditem.attunedID != 0)
                {
                    if (iditem.attunedID > 0)
                    {
                        if (iditem.attunedID == caster.UniqueID)
                        {
                            itmattuned = " The " + iditem.name + " is soulbound to you.";
                        }
                        else
                        {
                            itmattuned = " The " + iditem.name + " is soulbound to " + PC.GetName(iditem.attunedID) + ".";
                        }
                    }
                    else
                    {
                        itmattuned = " The " + iditem.name + " is soulbound to another being.";
                    }
                } 
                #endregion

                //iditem.identified[iditem.identified.Length - 1] = caster.playerID;

                caster.WriteToDisplay("You are looking at " + iditem.longDesc + "." + itmeffect + itmspell + itmspecial + itmalign + itmattuned);

                #region Venom
                if (iditem.venom > 0)
                {
                    var desc = iditem.name;
                    if (iditem.baseType == Globals.eItemBaseType.Bow)
                    {
                        if (iditem.name.Contains("crossbow") || iditem.longDesc.Contains("crossbow"))
                            desc = "nocked bolt";
                        else desc = "nocked arrow";
                    }

                    caster.WriteToDisplay("The " + desc + " drips with a caustic venom.");
                } 
                #endregion

                return true;
            }
            catch (Exception e)
            {
                Utils.LogException(e);
                return false;
            }
        }
    }
}
