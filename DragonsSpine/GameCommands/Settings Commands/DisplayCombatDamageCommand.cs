namespace DragonsSpine.Commands
{
    [CommandAttribute("displaycombatdamage", "Toggle display of combat damage.", (int)Globals.eImpLevel.USER, new string[] { "displaycombatdmg" },
        0, new string[] { "displaycombatdmg", "displaycombatdmg [off | on]" }, Globals.ePlayerState.CONFERENCE, Globals.ePlayerState.PLAYING)]
    public class DisplayCombatDamageCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (string.IsNullOrEmpty(args))
            {
                if ((chr as PC).DisplayCombatDamage)
                {
                    (chr as PC).DisplayCombatDamage = false;
                    chr.WriteToDisplay("Display combat damage set to OFF.");
                }
                else
                {
                    (chr as PC).DisplayCombatDamage = true;
                    chr.WriteToDisplay("Display combat damage set to ON.");
                }
            }
            else
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                if (sArgs[0].ToLower() == "off") { (chr as PC).DisplayCombatDamage = false; chr.WriteToDisplay("Display combat damage set to OFF."); }
                else if (sArgs[0].ToLower() == "on") { (chr as PC).DisplayCombatDamage = true; chr.WriteToDisplay("Display combat damage set to ON."); }
                else { chr.WriteToDisplay("Format: displaycombatdmg [ on | off ]"); }
            }

            return true;
        }
    }
}
