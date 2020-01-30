using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("roll", "Roll dice for a random result. End your command with 'silent' to keep results to yourself.", (int)Globals.eImpLevel.USER, new string[] { "dice" },
        0, new string[] { "Roll <#>, roll <#> <#> (min and max), roll #d# (number of sided dice)." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class RollDiceCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            bool silent = args.ToLower().Contains("silent") ||
                args.ToLower().Contains("private") ||
                args.ToLower().EndsWith(" s") ||
                args.ToLower() == "s";

            int max = 100;

            if (args == null || args == "" || args.ToLower() == "silent" || args.ToLower() == "private" || args.ToLower() == "s")
            {
                SendResult(chr, max, Rules.Dice.Next(1, 100), silent);
                return true;
            }
            
            if(args.Contains(" "))
            {
                string[] sArgs = args.Split(" ".ToCharArray());

                if(sArgs.Length < 2)
                {
                    chr.WriteToDisplay(GameSystems.Text.TextManager.COMMAND_NOT_UNDERSTOOD);
                    return true;
                }

                try
                {
                    max = Convert.ToInt32(sArgs[1]);
                    int min = Convert.ToInt32(sArgs[0]);

                    if(min >= max)
                    {
                        chr.WriteToDisplay("The maximum value must be greater than and not equal to the minimum value.");
                        return true;
                    }

                    SendResult(chr, min, max, Rules.Dice.Next(min, max + 1), silent);
                }
                catch(Exception)
                {
                    return false;
                }
            }

            return false;
        }

        private void SendResult(Character chr, int max, int result, bool silent)
        {
            chr.WriteToDisplay("Your random result between 1 and " + max + " is " + result + ".", ProtocolYuusha.TextType.DiceRoll);
            if (!silent)
            {
                if (!chr.IsPC || chr.PCState == Globals.ePlayerState.PLAYING)
                    chr.SendToAllInSight("DICE: " + chr.GetNameForActionResult() + "'s random result between 1 and " + max + " is " + result + ".");
                else (chr as PC).SendToAllInConferenceRoom("DICE: " + chr.GetNameForActionResult() + "'s random result between 1 and " + max + " is " + result + ".", ProtocolYuusha.TextType.DiceRoll);
            }
            }

        private void SendResult(Character chr, int min, int max, int result, bool silent)
        {
            chr.WriteToDisplay("Your random result between " + min + " and " + max + " is " + result + ".", ProtocolYuusha.TextType.DiceRoll);
            if (!silent)
            {
                if (!chr.IsPC || chr.PCState == Globals.ePlayerState.PLAYING)
                    chr.SendToAllInSight("DICE: " + chr.GetNameForActionResult() + " rolls a " + max + " sided die: " + result + ".");
                else (chr as PC).SendToAllInConferenceRoom("DICE: " + chr.GetNameForActionResult() + " rolls a " + max + " sided die: " + result + ".", ProtocolYuusha.TextType.DiceRoll);
            }
        }
    }
}
