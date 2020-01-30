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
using System.Collections;
using System.IO;

namespace DragonsSpine
{
    public class Bottle : Item
    {
        private bool m_open;

        public string drinkDesc; // the description displayed when the player drinks the bottle
        public string fluidDesc; // the description displayed when a player looks at an open bottle

        public bool IsOpen
        { get { return m_open; } set { m_open = value; } }

        public Bottle()
        {
            m_open = false;
        }

        public Bottle(System.Data.DataRow dr) : base(dr)
        {
            this.drinkDesc = dr["drinkDesc"].ToString();
            this.fluidDesc = dr["fluidDesc"].ToString();
        }

        public bool IsEmpty()
        {
            return this.special.ToLower().Contains("empty");
        }

        public static bool OpenBottle(Bottle bottle, Character ch)
        {
            if(bottle.m_open)
                return false;
            else
            {
                bottle.m_open = true;
                ch.EmitSound(Sound.GetCommonSound(Sound.CommonSound.OpenBottle));
                return true;
            }
        }

        public static bool CloseBottle(Bottle bottle)
        {
            if(bottle.m_open)
            {
                bottle.m_open = false;
                return true;
            }
            return false;
        }

        public static void DrinkBottle(Bottle bottle, Character ch)
        {
            if(bottle.m_open)
            {
                if(!bottle.special.Contains("empty "))
                {
                    if (bottle.itemID == Item.ID_BALM && ch.Land.LandID == GameWorld.Land.ID_ADVANCEDGAME)
                        Bottle.ConvertBalmToAGBalm(bottle);

                    string[] effectList = bottle.effectType.Split(" ".ToCharArray());
                    string[] amountList = bottle.effectAmount.Split(" ".ToCharArray());
                    string[] durationList = bottle.effectDuration.Split(" ".ToCharArray());

                    for (int a = 0; a < effectList.Length; a++)
                    {
                        Effect.CreateCharacterEffect((Effect.EffectTypes)Convert.ToInt32(effectList[a]), Convert.ToInt32(amountList[a]),
                        ch, Convert.ToInt32(durationList[a]), ch);
                    }

                    if (bottle.drinkDesc != "")
                    {
                        ch.WriteToDisplay(bottle.drinkDesc);
                    }

                    bottle.special = "empty " + bottle.special;
                    bottle.coinValue = 0;
                    ch.EmitSound(Sound.GetCommonSound(Sound.CommonSound.DrinkBottle));

                    //TODO: make this spam optional
                    ch.SendToAllInSight(ch.GetNameForActionResult() + " drinks from " + bottle.longDesc + ".");
                }
                else ch.WriteToDisplay("The bottle is empty.");
            }
            else ch.WriteToDisplay("You must first open the bottle.");
        }

        public static string GetFluidDesc(Bottle bottle)
        {
            if (bottle.special.Contains("empty "))
                return " The " + bottle.name + " is empty.";

            if (bottle.fluidDesc != "")
            {
                if(!Bottle.HasCork(bottle))
                    return " Inside the " + bottle.name + " is " + bottle.fluidDesc;
                else if (bottle.m_open)
                    return " Inside the " + bottle.name + " is " + bottle.fluidDesc + " The " + bottle.name + " is open.";
                else
                    return " The " + bottle.name + " is closed.";
            }
            return "";
        }

        public static void ConvertBalmToAGBalm(Item balm)
        {
            balm.coinValue = Merchant.AG_BALM_PRICE;
            balm.effectAmount = Merchant.AG_BALM_EFFECT_AMOUNT.ToString(); // this works because balms only have 1 effect
        }

        public static bool HasCork(Item bottle)
        {
            if (bottle.name.ToLower() == "mug")
                return false;

            return true;
        }
    }
}
