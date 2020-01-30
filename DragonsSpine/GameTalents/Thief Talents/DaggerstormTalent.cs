using TargetAcquisition = DragonsSpine.GameSystems.Targeting.TargetAquisition;

namespace DragonsSpine.Talents
{
    [Talent("daggerstorm", "DaggerStorm", "Use your precision to throw a barrage of daggers and smiliar weapons, from your belt, at a single target.",
        false, 15, 150000, 12, 10, true, new string[] { "daggerstorm <target>", "daggerstorm # <target>" },
        Character.ClassType.Thief)]
    public class DaggerstormTalent : ITalentHandler
    {
        private readonly System.Collections.Generic.List<Globals.eItemBaseType> AllowedBaseItemTypes = new System.Collections.Generic.List<Globals.eItemBaseType>()
        {
            Globals.eItemBaseType.Dagger, Globals.eItemBaseType.Fan, Globals.eItemBaseType.Shuriken
        };

        public bool MeetsRequirements(Character chr, Character target)
        {
            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("DaggerStorm who?");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            Character target = TargetAcquisition.AcquireTarget(chr, sArgs, GameWorld.Cell.DEFAULT_VISIBLE_DISTANCE, 0);

            // failed to find the target
            if (target == null)
            {
                chr.WriteToDisplay("You don't see a " + (sArgs.Length >= 2 ? sArgs[0] + " " + sArgs[1] : sArgs[0]) + " here.");
                return false;
            }

            // one hand must be free
            if(chr.RightHand != null && chr.LeftHand != null)
            {
                chr.WriteToDisplay("You must have one hand free to perform DaggerStorm.");
                return false;
            }

            int daggerCount = (Skills.GetSkillLevel(chr.dagger) / 2) + 1;

            int prevSavageryPower = 0;
            if(!chr.EffectsList.ContainsKey(Effect.EffectTypes.Savagery))
                Effect.CreateCharacterEffect(Effect.EffectTypes.Savagery, 50, chr, 0, null);
            else if (chr.EffectsList[Effect.EffectTypes.Savagery].Power < 50)
            {
                prevSavageryPower = chr.EffectsList[Effect.EffectTypes.Savagery].Power;
                chr.EffectsList[Effect.EffectTypes.Savagery].Power = 50;
            }

            for(int a = 0; a <= daggerCount; a++)
            {
                Item shadowDagger = Item.CopyItemFromDictionary(Item.ID_DAGGER_PLUS_TWO);
                shadowDagger.special += " " + Item.EXTRAPLANAR;
                shadowDagger.name = "shadowdagger";
                shadowDagger.combatAdds = daggerCount;
                shadowDagger.longDesc = "a slender dagger made from the very fabric of shadow";

                if (!chr.EquipEitherHand(shadowDagger))
                {
                    if (chr.RightHand != null && chr.RightHand.name == "shadowdagger")
                        chr.UnequipRightHand(chr.RightHand);
                    if (chr.LeftHand != null && chr.LeftHand.name == "shadowdagger")
                        chr.UnequipLeftHand(chr.LeftHand);

                    return true;
                }

                if (!target.IsDead)
                {
                    CommandTasker.ParseCommand(chr, "throw", "shadowdagger at " + target.UniqueID);
                    chr.CommandWeight = 0;
                }
                else
                {
                    if (chr.RightHand != null && chr.RightHand.name == "shadowdagger")
                        chr.UnequipRightHand(chr.RightHand);
                    if (chr.LeftHand != null && chr.LeftHand.name == "shadowdagger")
                        chr.UnequipLeftHand(chr.LeftHand);
                    return true;
                }
            }

            if(prevSavageryPower > 0)
            {
                chr.EffectsList[Effect.EffectTypes.Savagery].Power = prevSavageryPower;
            }
            else if(chr.EffectsList.ContainsKey(Effect.EffectTypes.Savagery))
                chr.EffectsList[Effect.EffectTypes.Savagery].StopCharacterEffect();

            if (chr.RightHand != null && chr.RightHand.name == "shadowdagger")
                chr.UnequipRightHand(chr.RightHand);
            if (chr.LeftHand != null && chr.LeftHand.name == "shadowdagger")
                chr.UnequipLeftHand(chr.LeftHand);

            return true;
        }
    }
}