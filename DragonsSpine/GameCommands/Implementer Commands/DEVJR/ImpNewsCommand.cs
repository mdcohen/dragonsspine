using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impnews", "Update server or client news.", (int)Globals.eImpLevel.DEVJR, new string[] { },
        0, new string[] { "impnews [server|client] <text>" }, Globals.ePlayerState.CONFERENCE)]
    public class ImpNewsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay("You must specifiy if you will update server or client news. impnews [server|client] <text>");
                return false;
            }

            string[] sArgs = args.Split(" ".ToCharArray());

            if (sArgs.Length != 2)
            {
                chr.WriteToDisplay("Invalid number of arguments.");
                return true;
            }

            switch(sArgs[0].ToLower())
            {
                case "server":
                    break;
                case "client":
                    break;
                default:
                    chr.WriteToDisplay("You must specifiy if you will update server or client news. impnews [server|client] <text>");
                    return false;
            }

            return true;
        }
    }
}
