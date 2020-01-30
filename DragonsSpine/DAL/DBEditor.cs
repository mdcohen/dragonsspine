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
using System.Data;
using System.Data.SqlClient;
using System.Collections;

namespace DragonsSpine.DAL
{
    /// <summary>
    /// Summary description for DBEditor.
    /// </summary>
    public class DBEditor
    {
        public DBEditor()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        internal static bool CellInfoExists(string MapName, int XCord, int YCord)
        {
            try
            {
                SqlStoredProcedure sp = new SqlStoredProcedure("prApp_CellsInfo_Select_One", DataAccess.GetSQLConnection());
                sp.AddParameter("@MapName", SqlDbType.NVarChar, 50, ParameterDirection.Input, MapName);
                sp.AddParameter("@XCord", SqlDbType.Int, 4, ParameterDirection.Input, XCord);
                sp.AddParameter("@YCord", SqlDbType.Int, 4, ParameterDirection.Input, YCord);
                if (sp.ExecuteNonQuery() == 1) {return true;}
                else {return false;}
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return false;																	
            }
        }

        internal static int UpdateCellInfo(bool newCellInfo, int cellID, string notes, string mapName, int xCord, int yCord, int zCord, string segue, string description, string cellLock, bool portal, bool pvpEnabled, bool singleCustomer, bool teleport, bool mailbox)
        {
            try
            {
                string sptouse = "prApp_CellsInfo_Update";
                if (newCellInfo) {sptouse = "prApp_CellsInfo_Insert";}

                SqlStoredProcedure sp = new SqlStoredProcedure(sptouse, DataAccess.GetSQLConnection());
                sp.AddParameter("@mapName", SqlDbType.VarChar,	50, ParameterDirection.Input, mapName);
                sp.AddParameter("@xCord", SqlDbType.Int, 4, ParameterDirection.Input, xCord);
                sp.AddParameter("@yCord", SqlDbType.Int, 4, ParameterDirection.Input, yCord);
                sp.AddParameter("@description", SqlDbType.NVarChar, 510, ParameterDirection.Input, description);
                sp.AddParameter("@lock", SqlDbType.NVarChar, 510, ParameterDirection.Input, cellLock);
                sp.AddParameter("@notes", SqlDbType.VarChar, 255,	ParameterDirection.Input, notes);
                sp.AddParameter("@portal", SqlDbType.Bit, 1, ParameterDirection.Input, portal);
                sp.AddParameter("@zCord", SqlDbType.Int, 4, ParameterDirection.Input, zCord);
                sp.AddParameter("@segue", SqlDbType.VarChar, 50, ParameterDirection.Input, segue);
                sp.AddParameter("@pvpEnabled", SqlDbType.Bit, 1, ParameterDirection.Input, pvpEnabled);
                sp.AddParameter("@singleCustomer", SqlDbType.Bit, 	1, ParameterDirection.Input, singleCustomer);
                sp.AddParameter("@teleport", SqlDbType.Bit, 1, ParameterDirection.Input, teleport);
                sp.AddParameter("@mailbox",SqlDbType.Bit, 1, ParameterDirection.Input, mailbox);
                sp.AddParameter("@cellID",SqlDbType.Int, 4, ParameterDirection.Input, cellID);
	return sp.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return -1;
            }
        }

        internal static int DeleteCellInfo(string MapName, int XCord, int YCord)
        {
            try
            {
                SqlStoredProcedure sp = new SqlStoredProcedure("prApp_CellsInfo_Delete", DataAccess.GetSQLConnection());
                sp.AddParameter("@MapName", SqlDbType.NVarChar, 50, ParameterDirection.Input, MapName);
                sp.AddParameter("@XCord", SqlDbType.Int, 4, ParameterDirection.Input, XCord);
                sp.AddParameter("@YCord", SqlDbType.Int, 4, ParameterDirection.Input, YCord);
                return sp.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return -1;
            }
        }

        internal static int InsertNPC(bool Update,
                int NPCIdent,
                int NpcID,
                string Name,
                string MovementString,
                string ShortDesc,
                int ArmorType,
                string Special,
                int NpcTypeCode,
                int BaseTypeCode,
                int Gold,
                string CharacterClassCode,
                int Experience,
                int HitsMax,
                int AlignCode,
                int Stamina,
                int ManaMax,
                int Speed,
                int Strength,
                int Dexterity,
                int Intelligence,
                int Wisdom,
                int Constitution,
                int Charisma,
                int Unarmed,
                int LootVeryCommonAmount,
                string LootVeryCommonArray,
                int LootVeryCommonOdds,
                int LootCommonAmount,
                string LootCommonArray,
                int LootCommonOdds,
                int LootRareAmount,
                string LootRareArray,
                int LootRareOdds,
                int LootVeryRareAmount,
                string LootVeryRareArray,
                int LootVeryRareOdds,
                int LootLairAmount,
                string LootLairArray,
                int LootLairOdds,
                string LootAlwaysArray,
                int LootBeltAmount,
                string LootBeltArray,
                int LootBeltOdds,
                string SpawnArmorArray,
                string SpawnLeftHandArray,
                int SpawnLeftHandOdds,
                string SpawnRightHandArray,
                long MaceSkill,
                long BowSkill,
                long DaggerSkill,
                long FlailSkill,
                long RapierSkill,
                long TwohandedSkill,
                long StaffSkill,
                long ShurikenSkill,
                long SwordSkill,
                long ThreestaffSkill,
                long HalberdSkill,
                long ThieverySkill,
                long MagicSkill,
                int Difficulty,
                bool IsAnimal,
                string TanningResult,
                bool IsUndead,
                bool IsHidden,
                int IsPoisonous,
                bool CanFly,
                bool CanBreatheWater,
                bool HasNightvision,
                bool IsLairCritter,
                string LairCells,
                bool IsMobile,
                bool CanCommand,
                bool RandomName,
                int CanCast,
                string AttackString1,
                string AttackString2,
                string AttackString3,
                string AttackString4,
                string AttackString5,
                string AttackString6,
                string BlockString1,
                string BlockString2,
                string BlockString3,
                bool IsMerchant,
                int MerchantType,
                double MerchantMarkup,
                int TrainerType,
                int gender,
                string Race,
                int Age,
                int Poisoned,
                bool hasPatrol,
                string patrolRoute,
                bool ImmuneFire,
                bool ImmuneCold,
                bool ImmuneLightning,
                bool ImmuneCurse,
                bool ImmuneDeath,
                bool ImmuneStun,
                bool ImmuneFear,
                bool ImmuneBlind,
                bool ImmunePoison)
        {

            string spToUse = "prApp_NPC_Insert";
            if(Update)
            {
                spToUse = "prApp_NPC_Update";
            }
            try
            {
				
                SqlStoredProcedure sp = new SqlStoredProcedure(spToUse, DataAccess.GetSQLConnection());
                if(Update)
                {
                    sp.AddParameter("@NPCIdent", SqlDbType.Int, 4,	ParameterDirection.Input, NPCIdent);
                }
                sp.AddParameter("@NpcID", SqlDbType.Int, 4, ParameterDirection.Input, NpcID);
                sp.AddParameter("@Name", SqlDbType.VarChar, 255,ParameterDirection.Input, Name);
                sp.AddParameter("@MovementString", SqlDbType.VarChar, 255, ParameterDirection.Input, MovementString);
                sp.AddParameter("@ShortDesc",	 SqlDbType.VarChar,	50, ParameterDirection.Input, ShortDesc);
                sp.AddParameter("@BaseArmorClass", SqlDbType.Int,	 4, ParameterDirection.Input, ArmorType);
                sp.AddParameter("@Special", SqlDbType.VarChar, 255, ParameterDirection.Input, Special);
                sp.AddParameter("@NpcTypeCode", SqlDbType.Int, 4, ParameterDirection.Input, NpcTypeCode);
                sp.AddParameter("@BaseTypeCode", SqlDbType.Int, 4, ParameterDirection.Input, BaseTypeCode);
                sp.AddParameter("@Gold", SqlDbType.Int,	4, ParameterDirection.Input, Gold);
                sp.AddParameter("@CharacterClassCode", SqlDbType.VarChar, 255, ParameterDirection.Input, CharacterClassCode);
                sp.AddParameter("@Experience", SqlDbType.Int, 4, ParameterDirection.Input, Experience);
                sp.AddParameter("@HitsMax", SqlDbType.Int, 4, ParameterDirection.Input, HitsMax);
                sp.AddParameter("@AlignCode", SqlDbType.Int, 4, ParameterDirection.Input, AlignCode);
                sp.AddParameter("@Stamina", SqlDbType.Int, 4, ParameterDirection.Input, Stamina);
                sp.AddParameter("@ManaMax",	SqlDbType.Int, 4, ParameterDirection.Input, ManaMax);
                sp.AddParameter("@Speed", SqlDbType.Int, 4, ParameterDirection.Input, Speed);
                sp.AddParameter("@Strength", SqlDbType.Int, 4, ParameterDirection.Input, Strength);
                sp.AddParameter("@Dexterity", 	SqlDbType.Int, 4, ParameterDirection.Input, Dexterity);
                sp.AddParameter("@Intelligence", SqlDbType.Int, 4, ParameterDirection.Input, Intelligence);
                sp.AddParameter("@Wisdom", SqlDbType.Int, 4, ParameterDirection.Input, Wisdom);
                sp.AddParameter("@Constitution", SqlDbType.Int, 4, ParameterDirection.Input, Constitution);
                sp.AddParameter("@Charisma",	SqlDbType.Int, 4, ParameterDirection.Input, Charisma);
                sp.AddParameter("@Unarmed",	SqlDbType.Int, 4, ParameterDirection.Input, Unarmed);
                sp.AddParameter("@LootVeryCommonAmount", SqlDbType.Int, 4, ParameterDirection.Input, LootVeryCommonAmount);
                sp.AddParameter("@LootVeryCommonArray", SqlDbType.VarChar, 255, ParameterDirection.Input, LootVeryCommonArray);
                sp.AddParameter("@LootVeryCommonOdds", SqlDbType.Int, 4, ParameterDirection.Input, LootVeryCommonOdds);
                sp.AddParameter("@LootCommonAmount",	SqlDbType.Int, 4, ParameterDirection.Input, LootCommonAmount);
                sp.AddParameter("@LootCommonArray", SqlDbType.VarChar, 255, ParameterDirection.Input, LootCommonArray);
                sp.AddParameter("@LootCommonOdds", SqlDbType.Int, 4, ParameterDirection.Input, LootCommonOdds);
                sp.AddParameter("@LootRareAmount", SqlDbType.Int, 4, ParameterDirection.Input, LootRareAmount);
                sp.AddParameter("@LootRareArray", SqlDbType.VarChar, 255, ParameterDirection.Input, LootRareArray);
                sp.AddParameter("@LootRareOdds", SqlDbType.Int, 4, ParameterDirection.Input, LootRareOdds);
                sp.AddParameter("@LootVeryRareAmount", SqlDbType.Int, 4, ParameterDirection.Input, LootVeryRareAmount);
                sp.AddParameter("@LootVeryRareArray", SqlDbType.VarChar, 255, ParameterDirection.Input, LootVeryRareArray);
                sp.AddParameter("@LootVeryRareOdds", SqlDbType.Int, 4, ParameterDirection.Input, LootVeryRareOdds);
                sp.AddParameter("@LootLairAmount", SqlDbType.Int,	4, ParameterDirection.Input, LootLairAmount);
                sp.AddParameter("@LootLairArray", SqlDbType.VarChar, 255, ParameterDirection.Input, LootLairArray);
                sp.AddParameter("@LootLairOdds", SqlDbType.Int, 4, ParameterDirection.Input, LootLairOdds);
                sp.AddParameter("@LootAlwaysArray", SqlDbType.VarChar, 255, ParameterDirection.Input, LootAlwaysArray);
                sp.AddParameter("@LootBeltAmount", SqlDbType.Int,	4, ParameterDirection.Input, LootBeltAmount);
                sp.AddParameter("@LootBeltArray", SqlDbType.VarChar, 255, ParameterDirection.Input, LootBeltArray);
                sp.AddParameter("@LootBeltOdds", SqlDbType.Int, 4,	 ParameterDirection.Input, LootBeltOdds);
                sp.AddParameter("@SpawnArmorArray", SqlDbType.VarChar, 255, ParameterDirection.Input, SpawnArmorArray);
                sp.AddParameter("@SpawnLeftHandArray", SqlDbType.VarChar,	255, ParameterDirection.Input, SpawnLeftHandArray);
                sp.AddParameter("@SpawnLeftHandOdds",	SqlDbType.Int, 4, ParameterDirection.Input, SpawnLeftHandOdds);
                sp.AddParameter("@SpawnRightHandArray", SqlDbType.VarChar, 255, ParameterDirection.Input, SpawnRightHandArray);
                sp.AddParameter("@MaceSkill",	SqlDbType.BigInt, 8,	ParameterDirection.Input, MaceSkill);
                sp.AddParameter("@BowSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, BowSkill);
                sp.AddParameter("@FlailSkill", 	SqlDbType.BigInt, 8,	ParameterDirection.Input, FlailSkill);
                sp.AddParameter("@DaggerSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, DaggerSkill);
                sp.AddParameter("@RapierSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, RapierSkill);
                sp.AddParameter("@TwohandedSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, TwohandedSkill);
                sp.AddParameter("@StaffSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, StaffSkill);
                sp.AddParameter("@ShurikenSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, ShurikenSkill);
                sp.AddParameter("@SwordSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, SwordSkill);
                sp.AddParameter("@ThreestaffSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, ThreestaffSkill);
                sp.AddParameter("@HalberdSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, HalberdSkill);
                sp.AddParameter("@ThieverySkill", SqlDbType.BigInt, 8, ParameterDirection.Input, ThieverySkill);
                sp.AddParameter("@MagicSkill", SqlDbType.BigInt, 8, ParameterDirection.Input, MagicSkill);
                sp.AddParameter("@Difficulty", SqlDbType.Int, 4, ParameterDirection.Input, Difficulty);
                sp.AddParameter("@IsAnimal", SqlDbType.Bit, 1, ParameterDirection.Input, IsAnimal);
                sp.AddParameter("@TanningResult", SqlDbType.VarChar, 255, ParameterDirection.Input, TanningResult);
                sp.AddParameter("@IsUndead",	SqlDbType.Bit, 1, ParameterDirection.Input, IsUndead);
                sp.AddParameter("@IsHidden", 	SqlDbType.Bit, 1, ParameterDirection.Input, IsHidden);
                sp.AddParameter("@IsPoisonous", SqlDbType.Int, 4, ParameterDirection.Input, IsPoisonous);
                sp.AddParameter("@CanFly", SqlDbType.Bit, 1, ParameterDirection.Input, CanFly);
                sp.AddParameter("@CanBreatheWater", SqlDbType.Bit, 1, ParameterDirection.Input, CanBreatheWater);
                sp.AddParameter("@HasNightvision", SqlDbType.Bit, 	1, ParameterDirection.Input, HasNightvision);
                sp.AddParameter("@IsLairCritter", SqlDbType.Bit, 1,	ParameterDirection.Input, IsLairCritter);
                sp.AddParameter("@LairCells", SqlDbType.VarChar, 255, ParameterDirection.Input, LairCells);
                sp.AddParameter("@Mobile", SqlDbType.Bit, 1, ParameterDirection.Input, IsMobile);
                sp.AddParameter("@CanCommand", SqlDbType.Bit, 1, ParameterDirection.Input, CanCommand);			
                sp.AddParameter("@RandomName", SqlDbType.Bit, 1, ParameterDirection.Input, RandomName);
                sp.AddParameter("@CanCast", SqlDbType.Int, 4, ParameterDirection.Input, CanCast);
                sp.AddParameter("@AttackString1", SqlDbType.VarChar, 255, ParameterDirection.Input, AttackString1);
                sp.AddParameter("@AttackString2", SqlDbType.VarChar, 255, ParameterDirection.Input, AttackString2);
                sp.AddParameter("@AttackString3", SqlDbType.VarChar, 255, ParameterDirection.Input, AttackString3);
                sp.AddParameter("@AttackString4", SqlDbType.VarChar, 255, ParameterDirection.Input, AttackString4);
                sp.AddParameter("@AttackString5", SqlDbType.VarChar, 255, ParameterDirection.Input, AttackString5);
                sp.AddParameter("@AttackString6", SqlDbType.VarChar, 255, ParameterDirection.Input, AttackString6);
                sp.AddParameter("@BlockString1", SqlDbType.VarChar, 255, ParameterDirection.Input, BlockString1);
                sp.AddParameter("@BlockString2", SqlDbType.VarChar, 255, ParameterDirection.Input, BlockString2);
                sp.AddParameter("@BlockString3", SqlDbType.VarChar, 255, ParameterDirection.Input, BlockString3);
                sp.AddParameter("@IsMerchant", SqlDbType.Bit, 1, ParameterDirection.Input, IsMerchant);
                sp.AddParameter("@MerchantType", SqlDbType.Int, 4, ParameterDirection.Input, MerchantType);
                sp.AddParameter("@MerchantMarkup", SqlDbType.Float, 8, ParameterDirection.Input, MerchantMarkup);
                sp.AddParameter("@TrainerType", SqlDbType.Int, 4, ParameterDirection.Input, TrainerType);
                sp.AddParameter("@Gender", SqlDbType.Int, 4, ParameterDirection.Input, gender);
                sp.AddParameter("@Race", SqlDbType.VarChar, 255, ParameterDirection.Input, Race);
                sp.AddParameter("@Age", SqlDbType.Int, 4, ParameterDirection.Input, Age);
                sp.AddParameter("@Poisoned", SqlDbType.Int, 4, ParameterDirection.Input, Poisoned);
                sp.AddParameter("@hasPatrol", 	SqlDbType.Bit, 1, ParameterDirection.Input, hasPatrol);
                sp.AddParameter("@patrolRoute", SqlDbType.VarChar, 255, ParameterDirection.Input, patrolRoute);
                sp.AddParameter("@ImmuneFire", SqlDbType.Bit, 1, ParameterDirection.Input, ImmuneFire);
                sp.AddParameter("@ImmuneCold", SqlDbType.Bit, 1,	ParameterDirection.Input, ImmuneCold);
                sp.AddParameter("@ImmunePoison", SqlDbType.Bit, 1, ParameterDirection.Input, ImmunePoison);
                sp.AddParameter("@ImmuneLightning", SqlDbType.Bit, 1, ParameterDirection.Input, ImmuneLightning);
                sp.AddParameter("@ImmuneCurse", SqlDbType.Bit, 1, ParameterDirection.Input, ImmuneCurse);
                sp.AddParameter("@ImmuneDeath", SqlDbType.Bit, 1, ParameterDirection.Input, ImmuneDeath);
                sp.AddParameter("@ImmuneStun", SqlDbType.Bit, 1,	ParameterDirection.Input, ImmuneStun);
                sp.AddParameter("@ImmuneFear", SqlDbType.Bit, 1,	ParameterDirection.Input, ImmuneFear);
                sp.AddParameter("@ImmuneBlind", SqlDbType.Bit, 1, ParameterDirection.Input, ImmuneBlind);
                return sp.ExecuteNonQuery();
			
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return -1;
            }
        }

        internal static int UpdateSpawnZone(bool newSZL, string notes, bool enabled, int npcID, int spawnTimer, int maxAllowedInZone, string spawnMessage, string npcList,
            int minZone, int maxZone, int spawnLand, int spawnMap, int spawnX, int spawnY, int spawnZ, int spawnRadius, string spawnZRange)
        {
            string sptouse = "prApp_SpawnZoneLink_Update";

            if (newSZL) { sptouse = "prApp_SpawnZoneLink_Insert"; }

            try
            {
                SqlStoredProcedure sp = new SqlStoredProcedure(sptouse, DataAccess.GetSQLConnection());

                sp.AddParameter("@notes", SqlDbType.VarChar, 255, ParameterDirection.Input, notes);
                sp.AddParameter("@enabled", SqlDbType.Bit, 1, ParameterDirection.Input, enabled);
                sp.AddParameter("@npcID", SqlDbType.Int, 4, ParameterDirection.Input, npcID);
                sp.AddParameter("@spawnTimer", SqlDbType.Int, 4, ParameterDirection.Input, spawnTimer);
                sp.AddParameter("@maxAllowedInZone", SqlDbType.Int, 4, ParameterDirection.Input, maxAllowedInZone);
                sp.AddParameter("@spawnMessage", SqlDbType.VarChar, 255, ParameterDirection.Input, spawnMessage);
                sp.AddParameter("@npcList", SqlDbType.VarChar, 255, ParameterDirection.Input, npcList);
                sp.AddParameter("@minZone", SqlDbType.Int, 4, ParameterDirection.Input, minZone);
                sp.AddParameter("@maxZone", SqlDbType.Int, 4, ParameterDirection.Input, maxZone);
                sp.AddParameter("@spawnLand", SqlDbType.Int, 4, ParameterDirection.Input, spawnLand);
                sp.AddParameter("@spawnMap", SqlDbType.Int, 4, ParameterDirection.Input, spawnMap);
                sp.AddParameter("@spawnX", SqlDbType.Int, 4, ParameterDirection.Input, spawnX);
                sp.AddParameter("@spawnY", SqlDbType.Int, 4, ParameterDirection.Input, spawnY);
                sp.AddParameter("@spawnZ", SqlDbType.Int, 4, ParameterDirection.Input, spawnZ);
                sp.AddParameter("@spawnRadius", SqlDbType.Int, 4,	ParameterDirection.Input, spawnRadius);
                sp.AddParameter("@spawnZRange", SqlDbType.VarChar, 255, ParameterDirection.Input, spawnZRange);
                return sp.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return -1;
            }
        }

        internal static int DeleteSpawnZone(int spawnZoneID)
        {
            try
            {
                SqlStoredProcedure sp = new SqlStoredProcedure("prApp_SpawnZone_Delete", DataAccess.GetSQLConnection());
                sp.AddParameter("@zoneID", SqlDbType.Int, 4, ParameterDirection.Input, spawnZoneID);
                return sp.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Utils.LogException(e);
                return -1;
            }
        }

        internal static int InsertItem(bool Update,
                int catalogID,
                string Notes,
                int ItemID,
                int ItemTypeCode,
                int ItemBaseCode,
                string ItemName,
                string ShortDesc,
                string LongDesc,
                int WearLocationCode,
                int Weight,
                int CoinValue,
                int SizeCode,
                int BlockRankCode,
                string EffectTypeCode,
                string EffectAmount,
                int EffectDuration,
                string Special,
                int MinDamage,
                int MaxDamage,
                string SkillType,
                int NumThrowAttacks,
                int HeldRange,
                int ThrowRange,
                int BookTypeCode,
                int CurrentPage,
                int MaxPages,
                int vRandLow,
                int vRandHigh,
                string KeyName,
                bool IsRecall,
                bool WasRecall,
                int Venom,
                int Alignment,
                int MagicRegen,
                int ItemCharges,
                bool HasSpells,
                int AttackRankCode,
                int Resistance,
                string AttackTypeCode,
                string Enchantment,
                int ProcChance,
                bool Returning,
                bool willAttune,
                long FigurineExperience,
                int ArmorClass,
                string page1,
                string page2,
                string page3,
                string page4,
                string page5,
                string page6,
                string page7,
                string page8,
                string page9)
        {
            string spToUse = "prApp_CatalogItem_Insert";
            if(Update)
            {
                spToUse = "prApp_CatalogItem_Update";
            }
            try 
            {
                SqlStoredProcedure sp = new SqlStoredProcedure(spToUse,DataAccess.GetSQLConnection());
                sp.AddParameter("@Notes", SqlDbType.NVarChar, 255, ParameterDirection.Input, Notes);
                if(Update)
                {
                    sp.AddParameter("@CatalogItemID", SqlDbType.Int, 4, ParameterDirection.Input, catalogID);
                }
                sp.AddParameter("@ItemID", SqlDbType.Int, 4, ParameterDirection.Input, ItemID);
                sp.AddParameter("@ItemTypeCode", SqlDbType.Int, 4, ParameterDirection.Input, ItemTypeCode);
                sp.AddParameter("@ItemBaseCode", SqlDbType.Int, 4, ParameterDirection.Input, ItemBaseCode);
                sp.AddParameter("@ItemName", SqlDbType.NVarChar, 255, ParameterDirection.Input, ItemName);
                sp.AddParameter("@ShortDesc", SqlDbType.NVarChar, 255, ParameterDirection.Input, ShortDesc);
                sp.AddParameter("@LongDesc", SqlDbType.NVarChar, 255, ParameterDirection.Input,	 LongDesc);
                sp.AddParameter("@WearLocationCode", SqlDbType.Int, 4, ParameterDirection.Input,	WearLocationCode);
                sp.AddParameter("@Weight", SqlDbType.Int, 4, ParameterDirection.Input, Weight);
                sp.AddParameter("@CoinValue", SqlDbType.Int, 4, ParameterDirection.Input, CoinValue);
                sp.AddParameter("@SizeCode", SqlDbType.Int, 4, ParameterDirection.Input,	SizeCode);
                sp.AddParameter("@BlockRankCode", SqlDbType.Int, 4, ParameterDirection.Input, BlockRankCode);
                sp.AddParameter("@EffectTypeCode", SqlDbType.NVarChar, 255, ParameterDirection.Input, EffectTypeCode);
                sp.AddParameter("@EffectAmount", SqlDbType.NVarChar, 255,	ParameterDirection.Input, EffectAmount);
                sp.AddParameter("@EffectDuration", SqlDbType.Int, 4, ParameterDirection.Input, EffectDuration);
                sp.AddParameter("@Special", SqlDbType.VarChar, 255, ParameterDirection.Input, Special);
                sp.AddParameter("@MinDamage", SqlDbType.Int, 4, ParameterDirection.Input, MinDamage);
                sp.AddParameter("@MaxDamage", SqlDbType.Int, 4,	ParameterDirection.Input, MaxDamage);
                sp.AddParameter("@SkillType", SqlDbType.NVarChar, 255, ParameterDirection.Input,	SkillType);
                sp.AddParameter("@NumThrowAttacks", SqlDbType.Int, 4, ParameterDirection.Input, NumThrowAttacks);
                sp.AddParameter("@HeldRange", SqlDbType.Int, 4, ParameterDirection.Input, HeldRange);
                sp.AddParameter("@ThrowRange", SqlDbType.Int, 4,	ParameterDirection.Input, ThrowRange);
                sp.AddParameter("@BookTypeCode", SqlDbType.Int, 4, ParameterDirection.Input, BookTypeCode);
                sp.AddParameter("@CurrentPage", SqlDbType.Int, 4, ParameterDirection.Input, CurrentPage);
                sp.AddParameter("@MaxPages", SqlDbType.Int, 4, ParameterDirection.Input, MaxPages);
                sp.AddParameter("@vRandLow", SqlDbType.Int, 4, ParameterDirection.Input, vRandLow);
                sp.AddParameter("@VRandHigh", SqlDbType.Int, 4, ParameterDirection.Input, vRandHigh);
                sp.AddParameter("@KeyName",	SqlDbType.VarChar, 50, ParameterDirection.Input, KeyName);
                sp.AddParameter("@IsRecall", SqlDbType.Bit, 1, ParameterDirection.Input,	IsRecall);
                sp.AddParameter("@WasRecall", SqlDbType.Int, 4, ParameterDirection.Input, WasRecall);
                sp.AddParameter("@Venom", SqlDbType.Int, 4, ParameterDirection.Input, Venom);
                sp.AddParameter("@Alignment", SqlDbType.Int, 4, ParameterDirection.Input, Alignment);
                sp.AddParameter("@MagicRegen", SqlDbType.Int, 4, ParameterDirection.Input, MagicRegen);
                sp.AddParameter("@ItemCharges", SqlDbType.Int, 4,	ParameterDirection.Input, ItemCharges);
                sp.AddParameter("@HasSpells", SqlDbType.Bit, 1, ParameterDirection.Input, HasSpells);
                sp.AddParameter("@AttackRankCode", SqlDbType.Int, 4, ParameterDirection.Input, AttackRankCode);
                sp.AddParameter("@Resistance", SqlDbType.Int, 4, ParameterDirection.Input, Resistance);
                sp.AddParameter("@AttackTypeCode", SqlDbType.VarChar, 50, ParameterDirection.Input, AttackTypeCode);
                sp.AddParameter("@Enchantment", SqlDbType.VarChar, 50, ParameterDirection.Input,	 Enchantment);
                sp.AddParameter("@ProcChance", SqlDbType.Int, 4, ParameterDirection.Input, ProcChance);
                sp.AddParameter("@isReturning", SqlDbType.Bit, 1, ParameterDirection.Input, Returning);
                sp.AddParameter("@willAttune", SqlDbType.Bit, 1, ParameterDirection.Input, willAttune);
                sp.AddParameter("@FigurineExperience", SqlDbType.Int, 4, ParameterDirection.Input,	FigurineExperience);
                sp.AddParameter("@ArmorClass", SqlDbType.Int, 4, ParameterDirection.Input, ArmorClass);
                sp.AddParameter("@page1", SqlDbType.VarChar, 255, ParameterDirection.Input, page1);
                sp.AddParameter("@page2", SqlDbType.VarChar, 255, ParameterDirection.Input, page2);
                sp.AddParameter("@page3", SqlDbType.VarChar, 255, ParameterDirection.Input, page3);
                sp.AddParameter("@page4", SqlDbType.VarChar, 255, ParameterDirection.Input, page4);
                sp.AddParameter("@page5", SqlDbType.VarChar, 255, ParameterDirection.Input, page5);
                sp.AddParameter("@page6", SqlDbType.VarChar, 255, ParameterDirection.Input, page6);
                sp.AddParameter("@page7", SqlDbType.VarChar, 255, ParameterDirection.Input, page7);
                sp.AddParameter("@page8", SqlDbType.VarChar, 255, ParameterDirection.Input, page8);
                sp.AddParameter("@page9", SqlDbType.VarChar, 255, ParameterDirection.Input, page9);
			
                return sp.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Utils.LogException(e);
            }
            return 0;
        }
    }
}
