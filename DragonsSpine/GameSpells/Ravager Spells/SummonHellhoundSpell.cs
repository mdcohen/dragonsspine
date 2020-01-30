using System;
using DragonsSpine.Autonomy.EntityBuilding;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Summon_Hellhound, "summonhellhound", "Summon Hellhound", "Summon a hellhound from another plane to do your bidding.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Self, 3, 8, 0, "", false, true, false, false, false, Character.ClassType.Ravager)]
    public class SummonHellhoundSpell : ISpellHandler
    {
        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            // Ravagers may only have one pet at any time.
            if (caster.Pets.Count > 0)
            {
                caster.WriteToDisplay("You may only control one pet.");
                return false;
            }

            NPC hellhound = null;

            foreach (NPC npc in EntityCreationManager.AutoCreatedNPCDictionary.Values)
            {
                if (npc.entity == EntityLists.Entity.Hellhound)
                {
                    hellhound = npc.CloneNPC();
                    break;
                }
            }

            var builder = new EntityBuilder();

            if (hellhound == null)
            {
                hellhound = new NPC();

                if (!builder.BuildEntity("hellhound|from the pits of hell", EntityLists.Entity.Hellhound, hellhound, caster.Map.ZPlanes[caster.Z], Character.ClassType.Fighter.ToString().ToLower()))
                {
                    caster.WriteToDisplay("Your attempt to summon a hellhound failed.");
                    return false;
                }
            }

            if (hellhound == null)
            {
                caster.WriteToDisplay("Your attempt to summon a hellhound failed.");
                return false;
            }

            hellhound.Level = caster.Level;
            hellhound.entity = EntityLists.Entity.Hellhound;
            builder.SetOnTheFlyVariables(hellhound);

            hellhound.Hits = hellhound.HitsFull;
            hellhound.Stamina = hellhound.StaminaFull;
            hellhound.Mana = hellhound.ManaFull;

            hellhound.shortDesc = "hellhound";

            hellhound.Alignment = Globals.eAlignment.Evil;
            hellhound.Age = 0;
            hellhound.special = "despawn";
            int timeRemaining = caster.Level / 4 * 10; // 20 minutes at level 8, 2 minutes per level after level 8
            hellhound.RoundsRemaining = Utils.TimeSpanToRounds(new TimeSpan(0, timeRemaining, 0));
            hellhound.species = Globals.eSpecies.Magical; // this may need to be changed for AI to work properly

            hellhound.canCommand = true;
            hellhound.IsMobile = true;
            hellhound.IsSummoned = true;
            hellhound.IsUndead = false;
            hellhound.FollowID = caster.UniqueID;

            hellhound.PetOwner = caster;
            caster.Pets.Add(hellhound);

            hellhound.LandID = caster.LandID;
            hellhound.MapID = caster.MapID;
            hellhound.X = caster.X;
            hellhound.Y = caster.Y;
            hellhound.Z = caster.Z;

            hellhound.AddToWorld();

            caster.CurrentCell.EmitSound(GameSpell.GameSpellDictionary[(int)GameSpell.GameSpellID.Firestorm].SoundFile);
            AreaEffect flames = new AreaEffect(Effect.EffectTypes.Fire, GameWorld.Cell.GRAPHIC_FIRE, 0, 1, caster, caster.CurrentCell);
            caster.SendToAllInSight("A portal ringed by flames flashes into existence and an enormous, sinewy hellhound slowly walks out and over to " + caster.GetNameForActionResult(true) + "'s side.");
            caster.WriteToDisplay("A portal ringed by flames flashes into existence and an enormous, sinewy hellhound slowly walks out and over to your side.");

            hellhound.EmitSound(hellhound.attackSound);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Flame_Shield, hellhound.Level, hellhound, -1, hellhound);
            hellhound.SendToAllInSight(hellhound.GetNameForActionResult() + " is surrounded by a shield of flames.");

            return true;
        }
    }
}
