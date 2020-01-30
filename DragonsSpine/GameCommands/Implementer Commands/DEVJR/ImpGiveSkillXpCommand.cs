using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impgiveskillxp", "Give skill experience to a player.", (int)Globals.eImpLevel.DEVJR, new string[] { },
        0, new string[] { "impgiveskillxp <skillType> <amount> <target>" }, Globals.ePlayerState.PLAYING)]
    public class ImpGiveSkillXpCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("impgiveskillxp <skillType> <amount> <target>");
            }
            else
            {
                try
                {
                    string[] sArgs = args.Split(" ".ToCharArray());

                    int amount = Convert.ToInt32(sArgs[1]);

                    Character target = null;

                    Globals.eSkillType skillType = Globals.eSkillType.None;
                    Enum.TryParse<Globals.eSkillType>(sArgs[0], true, out skillType);

                    if (skillType == Globals.eSkillType.None)
                    {
                        chr.WriteToDisplay("Check skill type value. Remember to use underscores for spaces.");
                        return false;
                    }

                    foreach (PC pc in Character.PCInGameWorld)
                    {
                        if (pc.Name.ToLower() == sArgs[2].ToLower())
                            target = pc;
                    }

                    if (target != null && target.Name != "Nobody")
                    {
                        Skills.GiveSkillExp(target, amount, skillType);
                    }
                    else if (target == null)
                    {
                        target = GameSystems.Targeting.TargetAquisition.FindTargetInView(chr, sArgs[2], false, true);

                        if (target != null)
                            Skills.GiveSkillExp(target, amount, skillType);
                        else
                        {
                            chr.WriteToDisplay("Could not find target: " + sArgs[2] + ".");
                        }
                    }
                }
                catch(Exception e)
                {
                    Utils.LogException(e);
                }
            }

            return true;
        }
    }
}
