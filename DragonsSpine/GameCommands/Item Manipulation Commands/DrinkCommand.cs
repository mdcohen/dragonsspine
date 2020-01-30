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
using Map = DragonsSpine.GameWorld.Map;

namespace DragonsSpine.Commands
{
    [CommandAttribute("drink", "Drink from a container.", (int)Globals.eImpLevel.USER, 1, new string[] { "drink <item>" }, Globals.ePlayerState.PLAYING)]
    public class DrinkCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (chr.CommandWeight > 3)
            {
                return true;
            }

            if (args == null || args == "")
            {
                if (!chr.CurrentCell.IsBalmFountain && !Map.IsNextToBalmFountain(chr))
                {
                    chr.WriteToDisplay("What do you want to drink?");
                    return true;
                }
                else goto balmFountain;
            }

            if (chr.RightHand != null && chr.RightHand.itemType == Globals.eItemType.Potable)
            {
                if (chr.RightHand.baseType == Globals.eItemBaseType.Bottle)
                {
                    Bottle.DrinkBottle((Bottle)chr.RightHand, chr);
                    return true;
                }
            }
            else if (chr.LeftHand != null && chr.LeftHand.itemType == Globals.eItemType.Potable)
            {
                if (chr.LeftHand.baseType == Globals.eItemBaseType.Bottle)
                {
                    Bottle.DrinkBottle((Bottle)chr.LeftHand, chr);
                    return true;
                }
            }
            else //TODO: expand here to allow drinking from a fluid source (eg: fountains)
            {
                chr.WriteToDisplay("You are not holding a bottle.");
                return true;
            }

        balmFountain:
            if (chr.CurrentCell.IsBalmFountain || Map.IsNextToBalmFountain(chr))
            {
                chr.WriteToDisplay("You drink your fill of the refreshing fluid seeping from the balm tree.");

                int effectAmount = chr.HitsFull - chr.Hits;
                if (effectAmount < 0) effectAmount = 0;

                Effect.CreateCharacterEffect(Effect.EffectTypes.Balm, effectAmount, chr, -1, chr);
            }

            return true;
        }
    }
}
