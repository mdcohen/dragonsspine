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
using DragonsSpine.Autonomy.EntityBuilding;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Summon_Lamassu, "summonlamassu", "Summon Lamassu", "Summon a lawful lamassu to aid you in battle.",
        Globals.eSpellType.Conjuration, Globals.eSpellTargetType.Self, 3, 16, 0, "", false, true, false, false, false, Character.ClassType.Knight)]
    public class SummonLammasuSpell : ISpellHandler
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

            NPC lamassu = null;

            foreach (NPC npc in EntityCreationManager.AutoCreatedNPCDictionary.Values)
            {
                if (npc.entity == EntityLists.Entity.Lamassu)
                {
                    lamassu = npc.CloneNPC();
                    break;
                }
            }

            var builder = new EntityBuilder();

            if (lamassu == null)
            {
                lamassu = new DragonsSpine.NPC();

                if (!builder.BuildEntity("lamassu|from the Hands of An", EntityLists.Entity.Lamassu, lamassu, caster.Map.ZPlanes[caster.Z], Character.ClassType.Fighter.ToString().ToLower()))
                {
                    caster.WriteToDisplay("Your attempt to summon a lamassu failed.");
                    return false;
                }
            }

            if (lamassu == null)
            {
                caster.WriteToDisplay("Your attempt to summon a lamassu failed.");
                return false;
            }

            lamassu.Level = caster.Level;
            lamassu.entity = EntityLists.Entity.Lamassu;

            // Debating to give lammasu the ability to turn undead as a no prep cast. 1/3/2017 -Eb

            builder.SetOnTheFlyVariables(lamassu);

            lamassu.Hits = lamassu.HitsFull;
            lamassu.Stamina = lamassu.StaminaFull;
            lamassu.Mana = lamassu.ManaFull;

            lamassu.shortDesc = "lamassu";

            lamassu.Alignment = Globals.eAlignment.Lawful;
            lamassu.Age = 0;
            lamassu.special = "despawn";
            // 5 minutes plus 10 seconds per level
            lamassu.RoundsRemaining = Utils.TimeSpanToRounds(new TimeSpan(0, 5, caster.Level * 10));
            lamassu.species = Globals.eSpecies.Magical; // this may need to be changed for AI to work properly

            lamassu.canCommand = true;
            lamassu.IsMobile = true;
            lamassu.IsSummoned = true;
            lamassu.IsUndead = false;
            lamassu.FollowID = caster.UniqueID;

            lamassu.PetOwner = caster;
            caster.Pets.Add(lamassu);

            if (lamassu.CurrentCell != caster.CurrentCell)
                lamassu.CurrentCell = caster.CurrentCell;

            caster.CurrentCell.EmitSound(GameSpell.GameSpellDictionary[(int)GameSpell.GameSpellID.Cure].SoundFile);
            caster.SendToAllInSight("A lamassu, with what appears to be a female human head, large feathered wings, and the body of a lion appears before you then moves to " + caster.GetNameForActionResult(true) + "'s side.");
            caster.WriteToDisplay("A lamassu, with what appears to be a female human head, large feathered wings, and the body of a lion appears before you and moves to your side.");

            lamassu.AddToWorld();

            lamassu.EmitSound(lamassu.attackSound);

            Effect.CreateCharacterEffect(Effect.EffectTypes.Bless, lamassu.Level, lamassu, -1, lamassu);
            lamassu.SendToAllInSight(lamassu.GetNameForActionResult() + " is briefly surrounded by a golden hue.");

            return true;
        }
    }
}
