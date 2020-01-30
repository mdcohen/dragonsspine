using System;
using System.Collections.Generic;
using DragonsSpine.GameWorld;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Image, "image", "Image", "Create an illusion of yourself to deceive enemies.",
        Globals.eSpellType.Illusion, Globals.eSpellTargetType.Area_Effect, 4, 3, 11400, "0271", false, false, true, false, false, Character.ClassType.Sorcerer)]
    public class ImageSpell : ISpellHandler
    {
        private const int IMAGE_CAST_DIRECTION_SKILL_REQUIREMENT = 11;

        public GameSpell ReferenceSpell
        {
            get;
            set;
        }

        public bool OnCast(Character caster, string args)
        {
            try
            {
                int skillLevel = Skills.GetSkillLevel(caster.magic);
                int numExistingImages = 0;
                int maxImages = skillLevel/ReferenceSpell.RequiredLevel;

                // Currently only one Image may be projected.
                foreach (NPC npc in Character.NPCInGameWorld)
                    if (npc.EffectsList.ContainsKey(Effect.EffectTypes.Image) &&
                        npc.EffectsList[Effect.EffectTypes.Image].Caster == caster)
                    {
                        numExistingImages++;
                        if(numExistingImages >= maxImages)
                        {
                            caster.WriteToDisplay("You do not possess enough magic skill to project another image.");
                            return true;
                        }
                    }

                if (args != null)
                {
                    args = args.Replace(ReferenceSpell.Command + " " + caster.Name, "");
                    args = args.Replace(ReferenceSpell.Command, "");
                    args = args.Trim();
                }

                Cell cell = null;

                if (!String.IsNullOrEmpty(args))
                {
                    cell = Map.GetCellRelevantToCell(caster.CurrentCell, args, false);

                    if (cell != null && skillLevel < IMAGE_CAST_DIRECTION_SKILL_REQUIREMENT)
                        caster.WriteToDisplay("You are not skilled enough to project an image in a direction.");
                }

                NPC image = null;

                ReferenceSpell.SendGenericCastMessage(caster, null, false);

                image = new NPC();
                image.Name = GameSpell.IMAGE_IDENTIFIER + caster.Name; // for displaying to players only
                image.Age = caster.Age;
                image.race = caster.race;
                image.species = Globals.eSpecies.Human;
                image.RightHand = caster.RightHand;
                image.LeftHand = caster.LeftHand;
                image.gender = caster.gender;
                image.BaseProfession = caster.BaseProfession;
                image.Alignment = caster.Alignment;
                image.wearing = new List<Item>(caster.wearing);
                image.IsPC = false; // this prevents saving to the database
                if (cell != null)
                    image.CurrentCell = cell;
                else image.CurrentCell = caster.CurrentCell; // image will appear where the spell was cast
                image.HitsMax = skillLevel * Rules.GetFullAbilityStat(caster, Globals.eAbilityStat.Charisma) + Rules.RollD(1, 10);
                image.Hits = image.HitsMax;
                Effect.CreateCharacterEffect(Effect.EffectTypes.Image, 1, image, -1, caster);
                image.visualKey = caster.visualKey; // visual image for yuusha and other clients...
                image.AddToWorld();
                image.EmitSound(ReferenceSpell.SoundFile);
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            return true;
        }
    }
}
