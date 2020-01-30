using System;
using DragonsSpine.Autonomy.EntityBuilding;
using DragonsSpine.Autonomy.ItemBuilding;

namespace DragonsSpine.Commands
{
    [CommandAttribute("impcreate", "Create an item, npc, scroll or coins.", (int)Globals.eImpLevel.DEVJR, new string[] { "impcr", "icreate" },
        0, new string[] { "impcreate [ entity <entity> <profession> <description> | npc <npc id> | item <item id> | coin <amount> | scroll <spell command> ]" }, Globals.ePlayerState.PLAYING)]
    public class ImpCreateCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            if (String.IsNullOrEmpty(args))
            {
                chr.WriteToDisplay(
                    "Format: impcreate [ entity <entity> <profession> <description> | npc <npc id> | item <item id> | coin <amount> | scroll <spell command> ]");
                return true;
            }

            //if (chr == null)
            //    chr = new Character();

            try
            {
                var sArgs = args.Split(" ".ToCharArray());

                if (sArgs[0].ToLower().Equals("adventurer") || sArgs[0].ToLower().Equals("adv"))
                {
                    #region Adventurer
                    // impcreate adv0 level1 entity2 profession3

                    if (sArgs.Length < 2 || !int.TryParse(sArgs[1], out int level))
                    {
                        chr.WriteToDisplay("Format: impcreate adv [level]");
                        return true;
                    }

                    EntityLists.Entity entity = EntityLists.Entity.Fighter;

                    if (sArgs.Length < 3 || !Enum.TryParse<EntityLists.Entity>(sArgs[2], true, out entity))
                        chr.WriteToDisplay("Unable to parse profession type. Used default of " + Utils.FormatEnumString(entity.ToString()));

                    string profession = sArgs.Length >= 4 ? sArgs[3] : Utils.FormatEnumString(entity.ToString());

                    EntityBuilder builder = new EntityBuilder();
                    NPC createdNPC = builder.BuildEntity("perplexed", entity, chr.Map.ZPlanes[chr.Z], profession);

                    var cloneList = DAL.DBWorld.GetScoresWithoutSP(createdNPC.BaseProfession, 40, true, "", false);

                    // quickly randomize the list
                    int n = cloneList.Count;
                    Random rnd = new Random();
                    while (n > 1)
                    {
                        int k = (rnd.Next(0, n) % n);
                        n--;
                        PC value = cloneList[k];
                        cloneList[k] = cloneList[n];
                        cloneList[n] = value;
                    }

                    foreach (PC clone in cloneList)
                    {
                        if (clone.Level == level && clone.ImpLevel == Globals.eImpLevel.USER)
                        {
                            Adventurer newAdventurer = new Adventurer(clone, createdNPC);

                            Utils.Log(newAdventurer.GetLogString() + " manually created from " + clone.GetLogString(), Utils.LogType.Adventurer);
                            chr.WriteToDisplay(newAdventurer.GetLogString() + " created from " + clone.GetLogString());

                            newAdventurer.SpawnZoneID = -1;
                            newAdventurer.CurrentCell = chr.CurrentCell;
                            newAdventurer.AddToWorld();
                            newAdventurer.RoundTimer.Start();
                            newAdventurer.ThirdRoundTimer.Start();

                            createdNPC.CurrentCell = null;
                            createdNPC.RemoveFromWorld();
                            return true;
                        }
                    }
                    return true; 
                    #endregion
                }

                if (sArgs[0].ToLower().Equals("entity"))
                {
                    #region Entity
                    // sArgs[1] = entity
                    // sArgs[2] = profession
                    // sArgs[3] = description

                    EntityLists.Entity entity = EntityLists.Entity.None;
                    string desc = "gigantic";
                    string profession = "fighter";

                    if (sArgs.Length < 2 || !Enum.TryParse<EntityLists.Entity>(sArgs[1], true, out entity))
                    {
                        chr.WriteToDisplay("Entity does not exist.");
                        return false;
                    }

                    if (sArgs.Length >= 3) profession = sArgs[2];
                    if (sArgs.Length >= 4) desc = sArgs[3];

                    EntityBuilder builder = new EntityBuilder();
                    NPC createdNPC = builder.BuildEntity(desc, entity, chr.Map.ZPlanes[chr.Z], profession);

                    if (createdNPC != null)
                    {
                        createdNPC.UniqueID = GameWorld.World.GetNextNPCUniqueID();

                        LootTable lootTable = LootManager.GetLootTable(createdNPC, chr.Map.ZPlanes[chr.Z]);
                        LootManager.GiveLootToNPC(createdNPC, lootTable);

                        createdNPC.CurrentCell = chr.CurrentCell;
                        createdNPC.AddToWorld();
                        createdNPC.RoundTimer.Start();
                        createdNPC.ThirdRoundTimer.Start();
                    }

                    return true; 
                    #endregion
                }

                if (sArgs[0].ToLower().Equals("item"))
                {
                    #region Item
                    var itemNum = Convert.ToInt32(sArgs[1]);
                    var item = Item.CopyItemFromDictionary(itemNum);

                    if (item == null)
                    {
                        chr.WriteToDisplay("Item " + sArgs[1] + " not found in item catalog.");
                        return false;
                    }

                    if (item.vRandLow > 0)
                    {
                        item.coinValue = Rules.Dice.Next(item.vRandLow, item.vRandHigh);
                    }
                    item.whoCreated = chr.GetLogString();
                    bool wasImmortal = chr.IsImmortal;
                    if (item.attuneType != Globals.eAttuneType.None)
                        chr.IsImmortal = true; // this is done to prevent binding the weapon to the dev creator
                    chr.EquipEitherHandOrDrop(item);
                    if (!wasImmortal) chr.IsImmortal = false;
                    chr.WriteToDisplay(item.notes + " created.");
                    return true; 
                    #endregion
                }

                if (sArgs[0].ToLower().Equals("scroll"))
                {
                    #region Scroll
                    var scroll = ScrollManager.CreateSpellScroll(Spells.GameSpell.GetSpell(sArgs[1]));
                    scroll.whoCreated = chr.GetLogString();
                    chr.CurrentCell.Add(scroll);
                    chr.WriteToDisplay(scroll.notes + " created.");
                    return true; 
                    #endregion
                }

                if (sArgs[0].ToLower().Equals("npc"))
                {
                    #region NPC
                    try
                    {
                        NPC createdNPC = NPC.LoadNPC(Convert.ToInt32(sArgs[1]), chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z, -1);
                        createdNPC.AddToWorld();
                        chr.WriteToDisplay("NPC created.");
                    }
                    catch
                    {
                        // sArgs[1] will be entity or profession
                        int entityID = EntityCreationManager.GetAutoCreatedNPCID(sArgs[1]);
                        if (entityID != -1)
                        {
                            NPC createdNPC = NPC.LoadNPC(Convert.ToInt32(sArgs[1]), chr.FacetID, chr.LandID, chr.MapID, chr.X, chr.Y, chr.Z, -1);
                            createdNPC.AddToWorld();
                            chr.WriteToDisplay("NPC created.");
                        }
                    }
                    return true; 
                    #endregion
                }

                if (sArgs[0].ToLower() == "coin" || sArgs[0].ToLower() == "coins")
                {
                    #region Coins
                    var coins = Item.CopyItemFromDictionary(Item.ID_COINS);

                    double amount;

                    if (!Double.TryParse(sArgs[1], out amount) || amount <= 0)
                    {
                        chr.WriteToDisplay("Invalid coin amount.");
                        return false;
                    }

                    coins.coinValue = amount;
                    chr.CurrentCell.Add(coins);
                    chr.WriteToDisplay(amount + " coins created.");
                    return true; 
                    #endregion
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }

            // failure to impcreate
            chr.WriteToDisplay(
                "Format: impcreate [ entity <entity> <profession> <description> | <npc <npc id> | item <item id> | coin <amount> | scroll <spell command> ]");

            return false;
        }
    }
}
