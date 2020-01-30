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
using Entity = DragonsSpine.Autonomy.EntityBuilding.EntityLists.Entity;

namespace DragonsSpine
{
    public static class Sound
    {
        public enum CommonSound { 
            DogBark,
            MeleeMiss,
            UnarmedMiss,
            ThrownWeapon,
            MetalBlock,
            WoodBlock,
            HandBlock,
            LightDamage,
            ModerateDamage,
            HeavyDamage,
            SevereDamage,
            FatalDamage,
            DeathRevive,
            SpellFail,
            Splash,
            EatFood,
            DrinkBottle,
            OpenBottle,
            Smithy,
            EarthQuake,
            ThunderClap,
            MapPortal,
            BreakingGlass,
            OpenDoor,
            CloseDoor,
            NockCrossbow,
            NockBow,
            MaleHmm,
            MaleFumble,
            MaleGrunt,
            FallingMale,
            MaleSpellWarm,
            FemaleHmm,
            FemaleFumble,
            FemaleGrunt,
            FemaleSpellWarm,
            Beep,
            DeathMalePlayer,
            DeathFemalePlayer,
            FallingFemale,
            LevelUp,
            SkillUp,
            RecallReset,
            ShieldBlock,
            Feared,
            MoveUpStairs,
            MoveDownStairs,
            SlidingRockDoor,
            Whirlwind,
            PickingLock,
            Explosion,
            Fireball,
            IceStorm,
            MaleSnarl,
            FemaleSnarl,
            LongLowWhoosh,
            UnlockingDoor,
            MemorizedChantLoss,
        }

        public static Dictionary<Entity, string[]> SoundFileNumbers = new Dictionary<Entity, string[]>(); // entity, string[] {"attackSound","deathSound","idleSound"}

        public static string GetSoundForDamage(string damageAdjective)
        {// light, moderate, heavy, severe, fatal

            if(damageAdjective.StartsWith("fatal") || damageAdjective.StartsWith("severe"))
                return "0054";

            if (damageAdjective.StartsWith("heavy") || damageAdjective.StartsWith("moderate"))
                return "0053";

            return "0052";
        }

        public static string GetCommonSound(CommonSound commonSound)
        {
            switch (commonSound)
            {
                case CommonSound.DogBark:
                    return "0013";
                case CommonSound.MeleeMiss:
                    if (Rules.RollD(1, 2) == 1)
                    {
                        return "0045";
                    }
                    return "0046";
                case CommonSound.UnarmedMiss:
                    return "0047";
                case CommonSound.ThrownWeapon:
                    return "0048";
                case CommonSound.MetalBlock:
                    return "0049";
                case CommonSound.WoodBlock:
                    return "0050";
                case CommonSound.ShieldBlock:
                case CommonSound.HandBlock:
                    return "0051";
                case CommonSound.LightDamage:
                case CommonSound.ModerateDamage:
                    return "0052";
                case CommonSound.HeavyDamage:
                    return "0053";
                case CommonSound.SevereDamage:
                case CommonSound.FatalDamage:
                    return "0054";
                case CommonSound.DeathRevive:
                    return "0055";
                case CommonSound.SpellFail:
                    return "0056";
                case CommonSound.MoveUpStairs:
                    return "0057";
                case CommonSound.MoveDownStairs:
                    return "0058";
                case CommonSound.Splash:
                    return "0059";
                case CommonSound.PickingLock:
                    return "0060";
                case CommonSound.DrinkBottle:
                    return "0061";
                case CommonSound.EatFood:
                    return "0062";
                case CommonSound.OpenBottle:
                    return "0063";
                case CommonSound.Smithy:
                    return "0064";
                case CommonSound.EarthQuake:
                    return "0065";
                case CommonSound.ThunderClap:
                    return "0066";
                case CommonSound.MapPortal:
                    return "0067";
                case CommonSound.Explosion:
                    return "0068";
                case CommonSound.Fireball:
                    return "0069";
                case CommonSound.IceStorm:
                    return "0070";
                case CommonSound.BreakingGlass:
                    return "0071";
                case CommonSound.Whirlwind:
                    return "0072";
                case CommonSound.OpenDoor:
                    return "0073";
                case CommonSound.CloseDoor:
                    return "0074";
                case CommonSound.NockCrossbow:
                    return "0075";
                case CommonSound.NockBow:
                    return "0076";
                case CommonSound.MaleHmm:
                    return "0077";
                case CommonSound.MaleFumble:
                    return "0078";
                case CommonSound.MaleGrunt:
                    return "0079";
                case CommonSound.MaleSpellWarm:
                    return "0080";
                case CommonSound.FemaleHmm:
                    return "0081";
                case CommonSound.FemaleFumble:
                    return "0082";
                case CommonSound.FemaleGrunt:
                    return "0083";
                case CommonSound.FemaleSpellWarm:
                    return "0084";
                    /* 85 quick tap, wooden weapon block?
                     * 86 quick tap, wooden weapon block higher pitch
                     * 87 shot weapon?
                     * 88 bow string twang?
                    */
                case CommonSound.Beep:
                    return "0089";
                /* 90 goblin idle
                 * 91 orc idle
                 * 92 kobold idle
                 * 93 wyvern/winged creature idle
                 * 94 hobgoblin idle
                 * 95 troll idle
                 * 96 minotaur idle
                */
                case CommonSound.DeathMalePlayer:
                    return "0186";
                case CommonSound.FallingMale:
                    return "0187";
                case CommonSound.MaleSnarl:
                    return "0201";
                case CommonSound.Feared:
                    return "0203";
                case CommonSound.DeathFemalePlayer:
                    return "0205";
                case CommonSound.FemaleSnarl:
                    return "0216";
                case CommonSound.FallingFemale:
                    return "0218";
                case CommonSound.LevelUp:
                    return "0219";
                case CommonSound.SkillUp:
                    return "0220";
                case CommonSound.RecallReset:
                    return "0221";
                case CommonSound.SlidingRockDoor:
                    return "0268";
                case CommonSound.LongLowWhoosh:
                    return "0285";
                case CommonSound.UnlockingDoor:
                    return "0288";
                case CommonSound.MemorizedChantLoss:
                    return "0291";
                default:
                    return "";
            }
        }

        public static string GetWeaponBlockSound(Item attackWeapon, Item blockWeapon)
        {
            if (attackWeapon == null || blockWeapon == null) return Sound.GetCommonSound(CommonSound.HandBlock);

            switch (attackWeapon.name.ToLower())
            {
                case "flail":
                case "broom":
                case "spear":
                case "staff":
                case "threestaff":
                case "crossbow":
                case "shovel":
                    return Sound.GetCommonSound(CommonSound.WoodBlock);
            }

            switch (blockWeapon.name.ToLower())
            {
                case "flail":
                case "broom":
                case "spear":
                case "staff":
                case "threestaff":
                case "crossbow":
                case "shovel":
                    return Sound.GetCommonSound(CommonSound.WoodBlock);
            }

            return Sound.GetCommonSound(CommonSound.MetalBlock);
        }

        public static string GetArmorBlockSound(Item attackWeapon)
        {
            if (attackWeapon == null) return Sound.GetCommonSound(CommonSound.HandBlock);

            switch (attackWeapon.name.ToLower())
            {
                case "flail":
                case "broom":
                case "spear":
                case "staff":
                case "threestaff":
                case "crossbow":
                case "shovel":
                    return Sound.GetCommonSound(CommonSound.WoodBlock);
            }

            return Sound.GetCommonSound(CommonSound.MetalBlock);
        }
    }
}
