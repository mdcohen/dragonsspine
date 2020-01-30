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

namespace DragonsSpine.GameEffects
{
    public class CharacterEffect : Effect
    {
        public enum CharacterEffectTypes
        {
            None, // 0
            Breathe_Water, // 14
            Night_Vision, // 15
            Wizard_Eye,
            Peek,
            HitsMax,
            Hits,
            StaminaMax, // 20
            Stamina, // 21
            // --
            Permanent_Strength, // 24
            Permanent_Dexterity, // 25
            Permanent_Intelligence,
            Permanent_Wisdom,
            Permanent_Constitution,
            Permanent_Charisma,
            Temporary_Strength, // 30
            Temporary_Dexterity,
            Temporary_Intelligence,
            Temporary_Wisdom,
            Temporary_Constitution,
            Temporary_Charisma, // 35
            // --
            Shield, // 36
            Regenerate_Hits,
            Regenerate_Mana,
            Regenerate_Stamina,
            Protection_from_Fire, // 40
            Protection_from_Cold,
            Protection_from_Poison,
            Protection_from_Fire_and_Ice,
            Protection_from_Blind_and_Fear,
            Protection_from_Stun_and_Death, // 45
            Resist_Fear,
            Resist_Blind,
            Resist_Stun,
            Resist_Lightning,
            Resist_Death, // 50
            Resist_Zonk,
            Knight_Ring,
            Bless,
            Blind,
            Fear, // 55
            Poison,
            Balm, // 57
            // --
            Naphtha, // 58
            Ale,
            Wine, // 60
            Beer,
            Coffee,
            Water,
            Youth_Potion,
            Drake_Potion, // 65
            Blindness_Cure,
            Mana_Restore,
            Stamina_Restore,
            Nitro,
            OrcBalm, // 70
            Permanent_Mana,
            Whiskey,
            Rum,
            // --
            Hide_in_Shadows, // 76
            Flight, // 77
            // --
        };

        //private Character m_target;

        //public Character Target
        //{
        //    get { return m_target; }
        //}
    }
}
