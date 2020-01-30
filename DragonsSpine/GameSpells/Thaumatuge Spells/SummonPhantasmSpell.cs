using System;
using System.Collections.Generic;
using DragonsSpine.Autonomy.ItemBuilding.ArmorSets;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Summon_Phantasm, "summonphantasm", "Summon Phantasm", "Summon a phantasm from another plane to do your bidding.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Self, 20, 11, 16000, "", false, false, true, false, true, Character.ClassType.Thaumaturge)]
    public class SummonPhantasmSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        private enum PhantasmPower
        {
            Phantasm = 1,
            Eidolon = 2,
            Djinn = 3,
            Salamander = 4,
            Efreet = 5,
            Marid = 6,
            Dao = 7,
        };

        public static List<int> GetAvailablePhantasms(int skillLevel)
        {
            List<int> availablePower = new List<int>() { 1 };

            // phantasm
            //12 eidolon
            //13 djinn
            //15 salamander
            //17 efreet
            //18 marid
            //19 dao

            if (skillLevel >= 12)
                availablePower.Add(2);
            if (skillLevel >= 13)
                availablePower.Add(3);
            if (skillLevel >= 15)
                availablePower.Add(4);
            if (skillLevel >= 17)
                availablePower.Add(5);
            if (skillLevel >= 18)
                availablePower.Add(6);
            if (skillLevel >= 19)
                availablePower.Add(7);

            return availablePower;
        }

        public static int GetManaRequiredForPower(int power)
        {
            if (power <= 1) return 20;

            else if (power >= 7)
                return 42;
            else if (power == 6)
                return 40;
            else if (power == 5)
                return 35;
            else if (power == 4)
                return 32;
            else if (power == 3)
                return 30;
            else return 23;
        }

        public bool OnCast(Character caster, string args)
        {
            /*      Power   Mana    Type        Armor (item IDs)            Skill (base)    Spells
             *      1       20      phantasm    leather (8010, 15010)       7               none
             *      2       23      eidolon     chain (8015, 15015)         8               magic missile
             *      3       30      djinn       banded mail (8020, 15020)   9               ice storm
             *      4       32      salamander  sally scales (8102)         10              firewall
             *      5       35      efreet      steel (8021, 15021)         11              concussion
             *      6       40      marid       steel (8021, 15021)         12              icespear
             *      7       42      dao         steel (8021, 15021)         13              lightninglance
            */

            args = args.Replace(ReferenceSpell.Command, "");

            args = args.Trim();

            string[] sArgs = args.Split(" ".ToCharArray());

            #region Determine power.
            PhantasmPower power = PhantasmPower.Phantasm; // default power

            if (sArgs.Length > 0)
            {
                try
                {
                    power = (PhantasmPower)Convert.ToInt32(sArgs[0]);

                    if (power > PhantasmPower.Dao)
                        power = PhantasmPower.Dao;
                }
                catch (Exception)
                {
                    power = PhantasmPower.Phantasm;
                }
            }
            #endregion

            int magicSkillLevel = Skills.GetSkillLevel(caster.magic);
            if (caster.IsImmortal)
                magicSkillLevel = 19;

            #region Verify skill level for power of spell.
            if (!caster.IsImmortal)
            {
                if (magicSkillLevel < 19)
                {
                    if (magicSkillLevel < 19 && power == PhantasmPower.Dao)
                    {
                        caster.WriteToDisplay("You are not skilled enough yet to summon dao.");
                        return true;
                    }

                    if (magicSkillLevel < 18 && power == PhantasmPower.Marid)
                    {
                        caster.WriteToDisplay("You are not skilled enough yet to summon marid.");
                        return true;
                    }

                    if (magicSkillLevel < 17 && power == PhantasmPower.Efreet)
                    {
                        caster.WriteToDisplay("You are not skilled enough yet to summon efreeti.");
                        return true;
                    }

                    if (magicSkillLevel < 15 && power == PhantasmPower.Salamander)
                    {
                        caster.WriteToDisplay("You are not skilled enough yet to summon salamanders.");
                        return true;
                    }

                    if (magicSkillLevel < 13 && power == PhantasmPower.Djinn)
                    {
                        caster.WriteToDisplay("You are not skilled enough yet to summon djinn.");
                        return true;
                    }

                    if(magicSkillLevel < 12 && power == PhantasmPower.Eidolon)
                    {
                        caster.WriteToDisplay("You are not skilled enough yet to summon eidolon.");
                        return true;
                    }
                }
            }
            #endregion

            #region Determine number of pets. Return false if at or above MAX_PETS.
            int petCount = 0;

            foreach (NPC pet in caster.Pets)
            {
                if (pet.QuestList.Count == 0)
                    petCount++;
            }

            // TODO: item or skill/talent to summon more pets
            if (!caster.IsImmortal && petCount >= GameSpell.MAX_PETS)
            {
                caster.WriteToDisplay("You do not possess the mental fortitude to control another pet.");
                return false;
            }
            #endregion

            #region Setup the summoned spirit.
            int npcID = 902;
            List<int> armorToWear = new List<int>();

            Autonomy.EntityBuilding.EntityLists.Entity entity = Autonomy.EntityBuilding.EntityLists.Entity.None;

            switch (power)
            {
                case PhantasmPower.Phantasm: // phantasm with leather
                    if (caster.Mana < ReferenceSpell.ManaCost)
                        return false;
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Phantasm;
                    break;
                case PhantasmPower.Eidolon: // eidolon with chain
                    if (caster.Mana < ReferenceSpell.ManaCost + 3)
                    {
                        caster.Mana -= 3;
                        return false;
                    }
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Eidolon;
                    caster.Mana -= 5;
                    break;
                case PhantasmPower.Djinn: // djinn with banded mail
                    if (caster.Mana < ReferenceSpell.ManaCost + 10)
                    {
                        caster.Mana -= 10;
                        return false;
                    }
                    caster.Mana -= 10;
                    npcID = 903; // djinn with banded mail and icestorm
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Djinn;
                    break;
                case PhantasmPower.Salamander: // salamander
                    if (caster.Mana < ReferenceSpell.ManaCost + 12)
                    {
                        caster.Mana -= 12;
                        return false;
                    }
                    caster.Mana -= 12;
                    npcID = 37; // salamander with scales and firewall
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Salamander;
                    break;
                case PhantasmPower.Efreet: // efreet with plate
                    if (caster.Mana < ReferenceSpell.ManaCost + 15)
                    {
                        caster.Mana -= 15;
                        return false;
                    }
                    caster.Mana -= 15;
                    npcID = 904; // efreet with steel plate and concussion
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Efreet;
                    break;
                case PhantasmPower.Marid: // efreet with plate
                    if (caster.Mana < ReferenceSpell.ManaCost + 20)
                    {
                        caster.Mana -= 20;
                        return false;
                    }
                    caster.Mana -= 20;
                    npcID = 904; // marid with steel plate and icespear
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Marid;
                    break;
                case PhantasmPower.Dao: // efreet with plate
                    if (caster.Mana < ReferenceSpell.ManaCost + 23)
                    {
                        caster.Mana -= 23;
                        return false;
                    }
                    caster.Mana -= 23;
                    npcID = 904; // dao with steel plate and lightninglance
                    entity = Autonomy.EntityBuilding.EntityLists.Entity.Dao;
                    break;
                default:
                    break;
            }

            // Create the summoned spirit.
            NPC phantasm = NPC.LoadNPC(npcID, caster.FacetID, caster.LandID, caster.MapID, caster.X, caster.Y, caster.Z, -1);

            foreach (Item wornItem in new List<Item>(phantasm.wearing))
                phantasm.RemoveWornItem(wornItem);

            phantasm.wearing.Clear();

            Autonomy.EntityBuilding.EntityBuilder builder = new Autonomy.EntityBuilding.EntityBuilder();

            phantasm.Level = caster.Level + (int)power;

            phantasm.entity = entity;
            builder.SetOnTheFlyVariables(phantasm);
            builder.SetName(phantasm, phantasm.BaseProfession.ToString());
            builder.SetDescriptions("", phantasm, caster.Map.ZPlanes[caster.Z], phantasm.BaseProfession.ToString().ToLower());
            Autonomy.EntityBuilding.EntityBuilder.SetVisualKey(phantasm.entity, phantasm);

            /*      Power   Mana    Type        Armor (item IDs)            Skill (base)    Spells
             *      1       20      phantasm    leather (8010, 15010)       7               none
             *      2       23      eidolon     chain (8015, 15015)         8               magic missile
             *      3       30      djinn       banded mail (8020, 15020)   9               ice storm
             *      4       32      salamander  sally scales (8102)         10              firewall
             *      5       35      efreet      steel (8021, 15021)         11              concussion
             *      6       40      marid       steel (8021, 15021)         12              icespear
             *      7       43      dao         steel (8021, 15021)         13              lightninglance
            */

            // basic phantasm
            if (power <= PhantasmPower.Phantasm)
            {
                phantasm.HitsMax += (int)power * 100;
                phantasm.ManaMax = 0;
                phantasm.castMode = NPC.CastMode.Never;
                phantasm.magic = 0;
            }
            else
            {
                phantasm.ManaMax = (int)power * (3 + Rules.RollD(1,6));
                phantasm.castMode = NPC.CastMode.NoPrep;
                phantasm.magic = Skills.GetSkillForLevel(((int)power * 2) + Rules.RollD(1, 3));
                phantasm.spellDictionary.Clear();

                switch (power)
                {
                    case PhantasmPower.Eidolon: // eidolon with magic missile
                        if (!phantasm.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Magic_Missile))
                            phantasm.spellDictionary.Add((int)GameSpell.GameSpellID.Magic_Missile, GameSpell.GenerateMagicWords());
                        break;
                    case PhantasmPower.Djinn: // djinn with icestorm
                        if (!phantasm.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Icestorm))
                            phantasm.spellDictionary.Add((int)GameSpell.GameSpellID.Icestorm, GameSpell.GenerateMagicWords());
                        // talents
                        if(!phantasm.talentsDictionary.ContainsKey(Talents.GameTalent.TALENTS.DualWield.ToString().ToLower()))
                            phantasm.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.DualWield).Command, DateTime.UtcNow);
                        break;
                    case PhantasmPower.Salamander: // salamander with firewall and firebolt
                        if (!phantasm.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Firewall))
                            phantasm.spellDictionary.Add((int)GameSpell.GameSpellID.Firewall, GameSpell.GenerateMagicWords());
                        if (!phantasm.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Firebolt))
                            phantasm.spellDictionary.Add((int)GameSpell.GameSpellID.Firebolt, GameSpell.GenerateMagicWords());
                        // talent
                        if (!phantasm.talentsDictionary.ContainsKey(Talents.GameTalent.TALENTS.DoubleAttack.ToString().ToLower()))
                            phantasm.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.DoubleAttack).Command, DateTime.UtcNow);
                        break;
                    case PhantasmPower.Efreet: // efreet with concussion
                        if (!phantasm.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Concussion))
                            phantasm.spellDictionary.Add((int)GameSpell.GameSpellID.Concussion, GameSpell.GenerateMagicWords());
                        // talents
                        if (!phantasm.talentsDictionary.ContainsKey(Talents.GameTalent.TALENTS.DualWield.ToString().ToLower()))
                            phantasm.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.DualWield).Command, DateTime.UtcNow);
                        if (!phantasm.talentsDictionary.ContainsKey(Talents.GameTalent.TALENTS.DoubleAttack.ToString().ToLower()))
                            phantasm.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.DoubleAttack).Command, DateTime.UtcNow);
                        break;
                    case PhantasmPower.Marid:
                        if (!phantasm.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Icespear))
                            phantasm.spellDictionary.Add((int)GameSpell.GameSpellID.Icespear, GameSpell.GenerateMagicWords());
                        // talents
                        if (!phantasm.talentsDictionary.ContainsKey(Talents.GameTalent.TALENTS.DualWield.ToString().ToLower()))
                            phantasm.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.DualWield).Command, DateTime.UtcNow);
                        if (!phantasm.talentsDictionary.ContainsKey(Talents.GameTalent.TALENTS.DoubleAttack.ToString().ToLower()))
                            phantasm.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.DoubleAttack).Command, DateTime.UtcNow);
                        break;
                    case PhantasmPower.Dao:
                        // lightning lance
                        if (!phantasm.spellDictionary.ContainsKey((int)GameSpell.GameSpellID.Lightning_Lance))
                            phantasm.spellDictionary.Add((int)GameSpell.GameSpellID.Lightning_Lance, GameSpell.GenerateMagicWords());
                        // talents (dual wield, double attack, riposte)
                        if (!phantasm.talentsDictionary.ContainsKey(Talents.GameTalent.TALENTS.DualWield.ToString().ToLower()))
                            phantasm.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.DualWield).Command, DateTime.UtcNow);
                        if (!phantasm.talentsDictionary.ContainsKey(Talents.GameTalent.TALENTS.DoubleAttack.ToString().ToLower()))
                            phantasm.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.DoubleAttack).Command, DateTime.UtcNow);
                        if (!phantasm.talentsDictionary.ContainsKey(Talents.GameTalent.TALENTS.Riposte.ToString().ToLower()))
                            phantasm.talentsDictionary.Add(Talents.GameTalent.GetTalent(Talents.GameTalent.TALENTS.Riposte).Command, DateTime.UtcNow);
                        break;
                    default:
                        phantasm.spellDictionary.Clear();
                        break;
                }
            }

            // Armor sets.

            switch (power)
            {
                case PhantasmPower.Phantasm: // phantasm with leather
                    armorToWear = ArmorSet.ArmorSetDictionary[ArmorSet.BASIC_LEATHER].GetArmorList(phantasm);
                    break;
                case PhantasmPower.Eidolon: // eidolon with chain
                    armorToWear = ArmorSet.ArmorSetDictionary[ArmorSet.BASIC_CHAINMAIL].GetArmorList(phantasm);
                    break;
                case PhantasmPower.Djinn: // djinn with banded mail
                    armorToWear = ArmorSet.ArmorSetDictionary[ArmorSet.BASIC_BANDED_MAIL].GetArmorList(phantasm);
                    break;
                case PhantasmPower.Salamander: // salamander
                    armorToWear.Add(Item.ID_FIRE_SALAMANDER_SCALE_VEST);
                    break;
                case PhantasmPower.Efreet: // efreet with plate
                case PhantasmPower.Marid:
                case PhantasmPower.Dao:
                    armorToWear = ArmorSet.ArmorSetDictionary[ArmorSet.BASIC_STEEL].GetArmorList(phantasm);
                    break;
                default:
                    break;
            }

            // Wear armor.
            foreach (int id in armorToWear)
            {
                Item armor = Item.CopyItemFromDictionary(id);
                // It's basic armor sets only. Label them as ethereal. (They will go back with the phantasm to their home plane. Given items drop.)
                armor.special += " " + Item.EXTRAPLANAR;
                phantasm.WearItem(armor);
            }

            if (phantasm.RightHand != null) phantasm.RightHand.special += " " + Item.EXTRAPLANAR;
            if (phantasm.LeftHand != null) phantasm.LeftHand.special += " " + Item.EXTRAPLANAR;

            GameSpell.FillSpellLists(phantasm);

            phantasm.Hits = phantasm.HitsFull;
            phantasm.Mana = phantasm.ManaFull;
            phantasm.Stamina = phantasm.StaminaFull;

            //phantasm.Alignment = (Globals.eAlignment)Enum.Parse(typeof(Globals.eAlignment), caster.Alignment.ToString());
            phantasm.Alignment = caster.Alignment;
            phantasm.Age = 0;
            phantasm.special = "despawn";

            int fiveMinutes = Utils.TimeSpanToRounds(new TimeSpan(0, 5, 0));
            // 30 minutes + 5 minutes for every skill level past 11 minus 5 minutes for every power of the spell beyond 1.
            phantasm.RoundsRemaining = (fiveMinutes * 6) + ((magicSkillLevel - ReferenceSpell.RequiredLevel) * fiveMinutes) - (((int)power - 1) * fiveMinutes);
            phantasm.species = Globals.eSpecies.Magical; // this may need to be changed for AI to work properly

            phantasm.canCommand = true;
            phantasm.IsMobile = true;
            phantasm.IsSummoned = true;
            phantasm.IsUndead = false;

            phantasm.FollowID = caster.UniqueID;

            phantasm.PetOwner = caster;
            caster.Pets.Add(phantasm);
            #endregion

            if (phantasm.CurrentCell != caster.CurrentCell)
                phantasm.CurrentCell = caster.CurrentCell;

            phantasm.EmitSound(phantasm.idleSound);

            phantasm.AddToWorld();

            return true;
        }
    }
}