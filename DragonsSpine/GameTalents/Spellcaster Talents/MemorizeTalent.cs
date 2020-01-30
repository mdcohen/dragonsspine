namespace DragonsSpine.Talents
{
    [TalentAttribute("memorize", "Memorize", "Use your vast knowledge of magic to memorize the chant for a spell you've learned. The spell is always available to cast, albeit slightly more taxing on your mind and body.",
        false, 15, 300000, 13, 50, true, new string[] { "memorize <spell chant>"},
        Character.ClassType.Druid, Character.ClassType.Ranger, Character.ClassType.Sorcerer, Character.ClassType.Thaumaturge, Character.ClassType.Thief, Character.ClassType.Wizard)]
    public class MemorizeTalent : ITalentHandler
    {
        public static int STAMINA_COST = 2; // per cast of a memorized spell

        public static bool MeetsRequirements(Character chr, string spellChant)
        {
            if (chr.WhichHand("spellbook") == (int)Globals.eWearOrientation.None)
            {
                chr.WriteToDisplay("You must be holding your spellbook when memorizing a spell chant.");
                return false;
            }

            if (!chr.IsMeditating)
            {
                chr.WriteToDisplay("You must be meditating to memorize a spell chant.");
                return false;
            }

            if (!chr.spellDictionary.ContainsValue(spellChant))
            {
                chr.WriteToDisplay("You do not know a spell with that chant.");
                return false;
            }

            return true;
        }

        public bool OnPerform(Character chr, string args)
        {
            if (args == null)
            {
                chr.WriteToDisplay("Memorize which chant?");
                return false;
            }

            if (!MeetsRequirements(chr, args)) return false;            

            chr.MemorizedSpellChant = args;

            foreach (int spellID in chr.spellDictionary.Keys)
            {
                if (chr.spellDictionary[spellID] == args)
                {
                    chr.WriteToDisplay("You have memorized the chant for your " + Spells.GameSpell.GetSpell(spellID).Name + " spell.");
                    break;
                }
            }

            return true;
        }
    }
}
