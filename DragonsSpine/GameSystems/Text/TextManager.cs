using System;

namespace DragonsSpine.GameSystems.Text
{
    public static class TextManager
    {
        public static string[] JAPANESE_SYLLABLES = new string[] { "a", "i", "u", "e", "o", "ka", "ki", "ku", "ke", "ko", "sa", "shi", "su", "se", "so", "ta", "chi", "tsu", "te", "to",
                "na", "ni", "nu", "ne", "no", "ha", "hi", "fu", "he", "ho", "ma", "mi", "mu", "me", "mo", "ra", "ri", "ru", "re", "ro", "ya", "yu", "yo", "wa", "wo",
                "ga", "gi", "gu", "ge", "go", "za", "ji", "zu", "ze", "zo", "da", "de", "do", "ba", "bi", "bu", "be", "bo", "pa", "pi", "pu", "pe", "po", "n",
                "kya", "kyu", "kyo", "sha", "shu", "sho", "cha", "chu", "cho", "nya", "nyu", "nyo", "hya", "hyu", "hyo", "mya", "myu", "myo", "rya", "ryu", "ryo",
                "gya", "gyu", "gyo", "ja", "ju", "jo", "bya", "byu", "byo", "pya", "pyu", "pyo"};

        public static string[] DROW_PHRASES_TO_ENEMY = new string[] { "Lloth tlu malla. Jal ultrinnah zhah.", "Passajamanal onola?" };
        public static string[] DROW_DEATH_PHRASES = new string[] { "U Vela Gjolf ath karihi julieshe." }; // U Vela Gjölf äth karihi julieshe.

        public static string[] GREETINGS = new string[] { "hello", "hey", "hi" };
        public static string[] FAREWELLS = new string[] { "Happy Hunting", "Happy hunting", "Good Hunting!", "Good hunting." }; 

        public static string COMMAND_NOT_UNDERSTOOD = "I don't understand your command.";

        public static string POISON_DEATH = "You have died from poison.";
        public static string POISON_DEATH_BROADCAST = " slumps to the ground and dies.";
        public static string POISON_TREMOR_BROADCAST = " looks rather ill.";
        public static string VENOM_PAINED_BROADCAST = " looks pained.";
        public static string POISON_TREMOR = "An involuntary tremor runs up and down your spine.";
        public static string POISON_EXPIRATION = "You shiver as the poison runs its course.";
        public static string VISION_BLUR = "Your vision blurs for a moment.";
        public static string PATH_IS_BLOCKED = "The path is blocked";
        public static string CHANT_PORTAL = "ashtug ninda anghizidda arrflug";
        public static string CHANT_PORTAL_REVERSE = "gulfrra addizihgna adnin guthsa";
        public static string CHANT_ANCESTOR_START = "ashak ashtug nushi ilani";
        public static string CHANT_UNDERWORLD_PORTAL = "urruku ya zi xul";
        public static string CHANT_DEMON_SUMMONING = "alsi ku nushi ilani";
        public const string IS_SLAIN_TEXT = " is slain!";
        public const string YOU_HAVE_BEEN_SLAIN_TEXT = "You have been slain!";
        public const string YOUR_SPELL_FAILS = "Your spell fails.";
        public const string REMAINED_HIDDEN = "You have remained hidden!";
        public const string SOULDBOUND_TO_ANOTHER_AFFIX = " is soulbound to another.";
        public const string CROWD_BLOCK = "You are blocked by the crowd.";
        public const string ARTIFACT_POSSESSED = "You already possess this artifact.";
        public const string YOU_DONT_SEE_THAT_HERE = "You don't see that here.";

        public static string NullTargetMessage(string targetArg)
        {
            int id;
            if (String.IsNullOrEmpty(targetArg) || Int32.TryParse(targetArg, out id))
                return "You don't see your target.";

            return "You don't see a " + targetArg + ".";
        }

        public static string NullItemMessage(string itemNameArg)
        {
            if (String.IsNullOrEmpty(itemNameArg) || Int32.TryParse(itemNameArg, out int id))
                return GameSystems.Text.TextManager.YOU_DONT_SEE_THAT_HERE;

            return "You don't see a " + itemNameArg + " here.";
        }

        public static string ConvertNumberToString(double qty)
        {
            if (qty == 2) { return "two "; }
            if (qty == 3) { return "three "; }
            if (qty == 4) { return "four "; }
            if (qty == 5) { return "five "; }
            if (qty == 6) { return "six "; }
            if (qty > 6 && qty <= 10) { return "several "; }
            if (qty > 10) { return "many "; }
            return "";
        }

        public static string MaleToFemale(string name)
        {
            string rName = name;

            if (name == "ogre") rName = "ogress";
            else if (name == "druid") rName = "druidess";
            else if (name == "priest") rName = "priestess";

            return rName;
        }

        public static string Multinames(string name)
        {
            if (name.ToLower() == "molydeus") return "molydei";
            if (name.ToLower() == "lammasu" || name.ToLower() == "lammasus") return "lammasi";

            if (name.ToLower().Contains("lizardman"))
                return name.Replace("man", "men");
            else if (name.ToLower().Contains("snakeman"))
                return name.Replace("man", "men");
            else if (name.ToLower().EndsWith("elf"))
                return name.Replace("elf", "elves");

            if (name.ToLower() == "drow.childs")
                return "drow.children";

            if (name.ToLower() == "drows")
                return "drow";

            if (name.ToLower().Contains("wolf"))
                return name.Replace("wolf", "wolves");
            else if (name.ToLower().EndsWith("shaman"))
                return name;
            else if (name.ToLower().EndsWith("magus"))
                return name.Replace("agus", "agi");
            else if (name.ToLower().EndsWith("s"))
                return name;
            else if (name.ToLower().Contains("staff"))
                return name.Replace("staff", "staves");
            else
            {
                return name + "s";
            }
        }

        public static string Capitalize(string text)
        {
            if (text.Length <= 0) return "";
            if (text.Length == 1) return text.ToUpper();

            return text.Substring(0, 1).ToUpper() + text.Substring(1, text.Length - 2);
        }
    }
}
