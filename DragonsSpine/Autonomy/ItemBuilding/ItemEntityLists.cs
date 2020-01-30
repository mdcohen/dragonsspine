using System.Collections.Generic;

namespace DragonsSpine.Autonomy.ItemBuilding
{
    public static class ItemEntityLists
    {
        public enum ItemEntity
        {
            None,
            Falchion, Shortsword, Longsword, Katana, // Sword
            Broadsword, Greatsword, Scythe, Maul, Greataxe, BattleAxe, Claymore, // Two_Handed
            Shortbow, Longbow, Crossbow, Flatbow, // Bow
            Dagger, Dirk, Stiletto, Katar, Kris, Knife, Tanto, // Dagger
            Flail, Meteor, Morningstar, Nunchaku, // Flail
            Bone, Broadaxe, Mace, Hammer, Axe, // Mace
            Threestaff, // Threestaff
            Staff, Spear, Wand, Broom, Shovel, // Staff
            Halberd, // Polearm
            Cutlass, Rapier, Sabre, // Rapier
            Shuriken, // Shuriken TODO: rename this Projectile
            Shield,
            Totem, Torch,
            Book, Scroll, Gem, Egg, Spellbook,
            Shawl, // Shoulders
            Cloak, Robe, Coat, Kimono, Jacket, Vest // Back

        }

        public static List<ItemEntity> BOW = new List<ItemEntity>
        {
            ItemEntity.Shortbow, ItemEntity.Longbow, ItemEntity.Crossbow
        };

        public static List<ItemEntity> DAGGER = new List<ItemEntity>
        {
            ItemEntity.Dagger, ItemEntity.Dirk, ItemEntity.Stiletto,
            ItemEntity.Katar, ItemEntity.Knife, ItemEntity.Kris,
            ItemEntity.Tanto
        };

        public static List<ItemEntity> FLAIL = new List<ItemEntity>
        {
            ItemEntity.Flail, ItemEntity.Meteor, ItemEntity.Morningstar, ItemEntity.Nunchaku
        };

        public static List<ItemEntity> POLEARM = new List<ItemEntity>
        {
            ItemEntity.Halberd
        };

        public static List<ItemEntity> MACE = new List<ItemEntity>
        {
            ItemEntity.Bone, ItemEntity.Broadaxe, ItemEntity.Mace, ItemEntity.Axe, ItemEntity.Hammer
        };

        public static List<ItemEntity> RAPIER = new List<ItemEntity>
        {
            ItemEntity.Cutlass, ItemEntity.Rapier, ItemEntity.Sabre
        };

        public static List<ItemEntity> SHURIKEN = new List<ItemEntity>
        {
        };

        public static List<ItemEntity> STAFF = new List<ItemEntity>
        {
        };

        public static List<ItemEntity> SWORD = new List<ItemEntity>
        {
            ItemEntity.Falchion, ItemEntity.Shortsword, ItemEntity.Longsword, ItemEntity.Katana
        };

        public static List<ItemEntity> THREESTAFF = new List<ItemEntity>
        {
        };

        public static List<ItemEntity> TWO_HANDED = new List<ItemEntity>
        {
        };

        public static List<ItemEntity> UNARMED = new List<ItemEntity> // Martial arts.
        {
        };
    }
}
