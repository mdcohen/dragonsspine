using System;

namespace DragonsSpine.Commands
{
    [CommandAttribute("nock", "Nock an arrow in your bow. Load a sling, or other range weapon, with a projectile.", (int)Globals.eImpLevel.USER, new string[] { "load" },
        1, new string[] { "There are no arguments for the nock command." }, Globals.ePlayerState.PLAYING)]
    public class NockCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                chr.WriteToDisplay("Command weight limit exceeded. Nock command not processed.");
                return true;
            }

            Item bow = null;

            // Holding no items.
            if (chr.RightHand == null && chr.LeftHand == null)
            {
                foreach(Item item in chr.wearing)
                {
                    if(item.wearLocation == Globals.eWearLocation.Hands && item.baseType == Globals.eItemBaseType.Bow)
                    {
                        bow = item;
                        break;
                    }
                }

                if(bow == null)
                    chr.WriteToDisplay("You are not holding a bow.");

                return true;
            }

            if(bow != null && bow.returning)
            {
                chr.WriteToDisplay("Your " + bow.name + " automatically nock upon use.");
                return true;
            }

            int freeHand = chr.GetFirstFreeHand();

            // Wristbows that are not auto nocking.
            if(freeHand == (int)Globals.eWearLocation.None && bow != null && bow.wearLocation == Globals.eWearLocation.Hands && !bow.returning)
            {
                chr.WriteToDisplay("You must have one empty hand to nock your " + bow.name + ".");
                return true;
            }

            // Holding a bow, but no free hands.
            if (((chr.RightHand != null && chr.RightHand.skillType != Globals.eSkillType.Bow) || (chr.LeftHand != null && chr.LeftHand.skillType != Globals.eSkillType.Bow)) &&
                freeHand == (int)Globals.eWearOrientation.None)
            {
                chr.WriteToDisplay("You must have one empty hand to nock a bow.");
                return true;
            }

            switch (freeHand)
            {
                case (int)Globals.eWearOrientation.Right:
                    bow = chr.LeftHand;
                    break;
                case (int)Globals.eWearOrientation.Left:
                    bow = chr.RightHand;
                    break;
                default:
                    break;
            }

            if (bow != null)
            {
                if(bow.baseType != Globals.eItemBaseType.Bow && bow.baseType != Globals.eItemBaseType.Sling)
                {
                    chr.WriteToDisplay("You cannot nock that.");
                    return true;
                }

                if (bow.returning)
                {
                    chr.WriteToDisplay("Your " + bow.name + " is equipped with an auto-nocking mechanism.");
                    return true;
                }
                
                if(!bow.returning && chr.CommandsProcessed.Contains(CommandTasker.CommandType.Shoot))
                {
                    chr.WriteToDisplay("Your " + bow.name + " cannot be shot and nocked that quickly. Perhaps you should search for a " + bow.name + " fitted with an auto-nocking mechanism.");
                    return true;
                }

                if (bow.IsNocked)
                {
                    chr.WriteToDisplay("Your " + bow.name + " is already nocked.");
                    return true;
                }

                chr.CommandType = CommandTasker.CommandType.Nock;

                bow.IsNocked = true;

                if (bow.name == "crossbow" || bow.longDesc.ToLower().Contains("crossbow"))
                    chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.NockCrossbow));
                else
                    chr.EmitSound(Sound.GetCommonSound(Sound.CommonSound.NockBow));
            }

            return true;
        }
    }
}
