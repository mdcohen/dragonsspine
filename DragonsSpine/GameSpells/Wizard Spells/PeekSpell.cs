using System;

namespace DragonsSpine.Spells
{
    [SpellAttribute(GameSpell.GameSpellID.Peek, "peek", "Peek", "Caster is able to briefly see through the eyes of a target.",
        Globals.eSpellType.Divination, Globals.eSpellTargetType.Self, 14, 11, 10000, "0226", false, true, false, true, false, Character.ClassType.Wizard)]
    public class PeekSpell : ISpellHandler
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
                // make sure they have the correct component, otherwise BOOM.

                if (caster is PC && (caster as PC).ImpLevel < Globals.eImpLevel.GM)
                {
                    Item item = caster.RightHand;

                    if (item == null)
                    {
                        item = caster.LeftHand;
                        if (item == null || item.itemID != Item.ID_TIGERSEYE)
                        {
                            caster.WriteToDisplay("You do not have the required material component for the " + ReferenceSpell.Name + " spell.");
                            return false;
                        }
                    }
                }

                string[] sArgs = args.Split(" ".ToCharArray());

                //find and confirm the target
                string tName = sArgs[sArgs.Length - 1];

                Character target = null;
                // find the FIRST match.
                foreach (PC ch in Character.PCInGameWorld)
                {
                    if (ch.MapID == caster.MapID && ch.Name.ToLower() == tName.ToLower())
                    {
                        target = ch;
                        break;
                    }
                }

                if (target == null)
                {
                    foreach (NPC ch in Character.NPCInGameWorld)
                    {
                        if (ch.MapID == caster.MapID && ch.Name.ToLower() == tName.ToLower())
                        {
                            target = ch;
                            break;
                        }
                    }
                }

                if (target == null || target is PC && Array.IndexOf((target as PC).ignoreList, caster.UniqueID) > -1 || (caster is PC && target is PC &&
                    Array.IndexOf((caster as PC).ignoreList, target.UniqueID) > -1))
                {
                    caster.WriteToDisplay("You cannot sense your target.");
                    return true;
                }
                else
                {
                    // alert the target that they are being peeked
                    if (!caster.IsInvisible && !caster.IsImmortal)
                        target.WriteToDisplay("Your eyes tingle for a moment.");

                    string targetDesc = target is PC || (target is NPC) && (target as NPC).longDesc == "" ? target.Name : (target as NPC).longDesc;

                    caster.WriteToDisplay("You see through the eyes of " + targetDesc + ".");

                    Effect.CreateCharacterEffect(Effect.EffectTypes.Peek, 1, target, 1, caster);
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
            return true;
        }
    }
}
