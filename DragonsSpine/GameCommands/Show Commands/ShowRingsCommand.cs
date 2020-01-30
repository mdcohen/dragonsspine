#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;

namespace DragonsSpine.Commands
{
    [CommandAttribute("showrings", "Display a list of worn rings.", (int)Globals.eImpLevel.USER, new string[] { "show rings", "show ring", "rings" },
        0, new string[] { "There are no arguments for the show rings command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowRingsCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            string right1 = "empty";
            string right2 = "empty";
            string right3 = "empty";
            string right4 = "empty";
            string left1 = "empty";
            string left2 = "empty";
            string left3 = "empty";
            string left4 = "empty";

            try
            {
                if (chr.RightRing1 != null)
                {
                    right1 = chr.RightRing1.unidentifiedName + " (" + chr.RightRing1.name + ")";
                    if (chr.RightRing1.identifiedList.Contains(chr.UniqueID))
                        right1 = chr.RightRing1.identifiedName + " (" + chr.RightRing1.name + ")";

                    try
                    {
                        if (chr.RightRing1.isRecall && chr.Map.ZPlanes[chr.RightRing1.recallZ].name != "")
                            right1 += " [" + chr.Map.ZPlanes[chr.RightRing1.recallZ].name + "]";
                    }
                    catch { right1 += " [Broken Recall Link]"; chr.RightRing1.isRecall = false; }

                }
                if (chr.RightRing2 != null)
                {
                    right2 = chr.RightRing2.unidentifiedName + " (" + chr.RightRing2.name + ")";
                    if (chr.RightRing2.identifiedList.Contains(chr.UniqueID))
                        right2 = chr.RightRing2.identifiedName + " (" + chr.RightRing2.name + ")";

                    try
                    { 
                    if (chr.RightRing2.isRecall && chr.Map.ZPlanes[chr.RightRing2.recallZ].name != "")
                        right2 += " [" + chr.Map.ZPlanes[chr.RightRing2.recallZ].name + "]";
                    }
                    catch { right2 += " [Broken Recall Link]"; chr.RightRing2.isRecall = false; }
        }
                if (chr.RightRing3 != null)
                {
                    right3 = chr.RightRing3.unidentifiedName + " (" + chr.RightRing3.name + ")";
                    if (chr.RightRing3.identifiedList.Contains(chr.UniqueID))
                        right3 = chr.RightRing3.identifiedName + " (" + chr.RightRing3.name + ")";

                    try
                    {
                        if (chr.RightRing3.isRecall && chr.Map.ZPlanes[chr.RightRing3.recallZ].name != "")
                            right3 += " [" + chr.Map.ZPlanes[chr.RightRing3.recallZ].name + "]";
                    }
                    catch { right3 += " [Broken Recall Link]"; chr.RightRing3.isRecall = false; }
        }
                if (chr.RightRing4 != null)
                {
                    right4 = chr.RightRing4.unidentifiedName + " (" + chr.RightRing4.name + ")";
                    if (chr.RightRing4.identifiedList.Contains(chr.UniqueID))
                        right4 = chr.RightRing4.identifiedName + " (" + chr.RightRing4.name + ")";

                    try
                    {
                        if (chr.RightRing4.isRecall && chr.Map.ZPlanes[chr.RightRing4.recallZ].name != "")
                            right4 += " [" + chr.Map.ZPlanes[chr.RightRing4.recallZ].name + "]";
                    }
                    catch { right4 += " [Broken Recall Link]"; chr.RightRing4.isRecall = false; }
        }
                if (chr.LeftRing1 != null)
                {
                    left1 = chr.LeftRing1.unidentifiedName + " (" + chr.LeftRing1.name + ")";
                    if (chr.LeftRing1.identifiedList.Contains(chr.UniqueID))
                        left1 = chr.LeftRing1.identifiedName + " (" + chr.LeftRing1.name + ")";

                    try { 
                    if (chr.LeftRing1.isRecall && chr.Map.ZPlanes[chr.LeftRing1.recallZ].name != "")
                        left1 += " [" + chr.Map.ZPlanes[chr.LeftRing1.recallZ].name + "]";
                    }
                    catch { left1 += " [Broken Recall Link]"; chr.LeftRing1.isRecall = false; }
                }
                if (chr.LeftRing2 != null)
                {
                    left2 = chr.LeftRing2.unidentifiedName + " (" + chr.LeftRing2.name + ")";
                    if (chr.LeftRing2.identifiedList.Contains(chr.UniqueID))
                        left2 = chr.LeftRing2.identifiedName + " (" + chr.LeftRing2.name + ")";

                    try { 
                    if (chr.LeftRing2.isRecall && chr.Map.ZPlanes[chr.LeftRing2.recallZ].name != "")
                        left2 += " [" + chr.Map.ZPlanes[chr.LeftRing2.recallZ].name + "]";
                    }
                    catch { left2 += " [Broken Recall Link]"; chr.LeftRing2.isRecall = false; }
                }
                if (chr.LeftRing3 != null)
                {
                    left3 = chr.LeftRing3.unidentifiedName + " (" + chr.LeftRing3.name + ")";
                    if (chr.LeftRing3.identifiedList.Contains(chr.UniqueID))
                        left3 = chr.LeftRing3.identifiedName + " (" + chr.LeftRing3.name + ")";

                    try { 
                    if (chr.LeftRing3.isRecall && chr.Map.ZPlanes[chr.LeftRing3.recallZ].name != "")
                        left3 += " [" + chr.Map.ZPlanes[chr.LeftRing3.recallZ].name + "]";
                    }
                    catch { left3 += " [Broken Recall Link]"; chr.LeftRing3.isRecall = false; }
                }
                if (chr.LeftRing4 != null)
                {
                    left4 = chr.LeftRing4.unidentifiedName + " (" + chr.LeftRing4.name + ")";
                    if (chr.LeftRing4.identifiedList.Contains(chr.UniqueID))
                        left4 = chr.LeftRing4.identifiedName + " (" + chr.LeftRing4.name + ")";

                    try { 
                    if (chr.LeftRing4.isRecall && chr.Map.ZPlanes[chr.LeftRing4.recallZ].name != "")
                        left4 += " [" + chr.Map.ZPlanes[chr.LeftRing4.recallZ].name + "]";
                    }
                    catch { left4 += " [Broken Recall Link]"; chr.LeftRing4.isRecall = false; }
                }
            }
            catch (Exception e)
            { Utils.LogException(e); }

            chr.WriteToDisplay("Right 1: " + right1);
            chr.WriteToDisplay("Right 2: " + right2);
            chr.WriteToDisplay("Right 3: " + right3);
            chr.WriteToDisplay("Right 4: " + right4);
            chr.WriteToDisplay("Left  1: " + left1);
            chr.WriteToDisplay("Left  2: " + left2);
            chr.WriteToDisplay("Left  3: " + left3);
            chr.WriteToDisplay("Left  4: " + left4);

            return true;
        }
    }
}
