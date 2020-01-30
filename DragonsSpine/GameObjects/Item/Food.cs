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
namespace DragonsSpine
{
    public static class Food
    {
        private const int BERRY_BALM_EFFECTAMOUNT = 6;
        private const int BERRY_MANA_EFFECTAMOUNT = 6;
        private const int BERRY_STAMINA_EFFECTAMOUNT = 6;
        private const int BERRY_POISON_EFFECTAMOUNT = 5;

        public static void EatFood(Item food, Character ch)
        {
            ch.EmitSound(Sound.GetCommonSound(Sound.CommonSound.EatFood));

            var eatText = "You eat "+food.longDesc+".";

            switch(food.special.ToLower())
            {
                case "balmberry":
                    ch.WriteToDisplay(eatText);
                    if (ch.HitsFull - ch.Hits > BERRY_BALM_EFFECTAMOUNT) { ch.Hits += BERRY_BALM_EFFECTAMOUNT; }
                    else{ch.Hits = ch.HitsFull;}
                    break;
                case "manaberry":
                    ch.WriteToDisplay(eatText+" Mmmm. Those were very sweet!");
                    if (ch.ManaFull - ch.Mana > BERRY_MANA_EFFECTAMOUNT) { ch.Mana += BERRY_MANA_EFFECTAMOUNT; }
                    else{ch.Mana = ch.ManaFull;}
                    break;
                case "poisonberry":
                    ch.WriteToDisplay(eatText+" Ugh those tasted horrible!");
                    ch.Poisoned += BERRY_POISON_EFFECTAMOUNT;
                    break;
                case "stamberry":
                    ch.WriteToDisplay(eatText);
                    if (ch.StaminaFull - ch.Stamina > BERRY_STAMINA_EFFECTAMOUNT) { ch.Stamina += BERRY_STAMINA_EFFECTAMOUNT; }
                    else{ch.Stamina = ch.StaminaFull;}
                    break;
                case "neutralize":
                    if(ch.Poisoned > 0)
                    {
                        ch.Poisoned = 0;
                        ch.WriteToDisplay(eatText+" The poison has been neutralized.");
                        eatText = "";
                    }

                    if(ch.EffectsList.ContainsKey(Effect.EffectTypes.Poison))
                        ch.EffectsList[Effect.EffectTypes.Poison].StopCharacterEffect();

                    if(ch.EffectsList.ContainsKey(Effect.EffectTypes.Venom))
                        ch.EffectsList[Effect.EffectTypes.Venom].StopCharacterEffect();

                    if (ch.EffectsList.ContainsKey(Effect.EffectTypes.Contagion))
                    {
                        if (Rules.Dice.Next(0, 100) >= 90)
                            ch.EffectsList[Effect.EffectTypes.Contagion].StopCharacterEffect();
                        else ch.WriteToDisplay("Your contagion was unable to be neutralized.");
                    }

                    if(eatText.Length > 0)
                        ch.WriteToDisplay(eatText);
                    break;
            }
        }
    }
}
