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

namespace DragonsSpine.Commands
{
    [CommandAttribute("showsaves", "Display character saving throws.", (int)Globals.eImpLevel.USER, new string[] { "show saves", "show savingthrows", "saves" },
        0, new string[] { "There are no arguments for the show resists command." }, Globals.ePlayerState.PLAYING, Globals.ePlayerState.CONFERENCE)]
    public class ShowSavesCommand : ICommandHandler
    {
        public bool OnCommand(Character chr, string args)
        {
            // Types:
            // PPD (Paralyzation, Poison, Death Magic)
            // PP (Petrification or Polymorph)
            // RSW (Rod, Staff, Wand)
            // S (Spells) (Concussion)
            // BW (Breath Weapon)

            chr.WriteToDisplay("Saving Throws (Adjustments Incl.)");
            chr.WriteToDisplay("Breath Weapon                     : " + GetBWSavingThrow(chr));
            chr.WriteToDisplay("Paralyzation, Poison, Death Magic : " + GetPPDSavingThrow(chr));
            chr.WriteToDisplay("Petrification or Polymorph        : " + GetPPSavingThrow(chr));
            chr.WriteToDisplay("Rod, Staff, Wand                  : " + GetRSWSavingThrow(chr));
            chr.WriteToDisplay("Spells                            : " + GetSpellsSavingThrow(chr));
            chr.WriteToDisplay("");
            chr.WriteToDisplay("Saving Throw Adjustments:");
            chr.WriteToDisplay("Profession: " + Utils.FormatEnumString(chr.BaseProfession.ToString()) + ", Homeland: " + chr.race);
            chr.WriteToDisplay("Breath Weapon                     : " + GetProfessionAdjustment(chr.BaseProfession, Combat.SavingThrow.BreathWeapon) + ", " + GetHomelandAdjustment(chr.race, Combat.SavingThrow.BreathWeapon));
            chr.WriteToDisplay("Paralyzation, Poison, Death Magic : " + GetProfessionAdjustment(chr.BaseProfession, Combat.SavingThrow.ParalyzationPoisonDeath) + ", " + GetHomelandAdjustment(chr.race, Combat.SavingThrow.ParalyzationPoisonDeath));
            chr.WriteToDisplay("Petrification or Polymorph        : " + GetProfessionAdjustment(chr.BaseProfession, Combat.SavingThrow.PetrificationPolymorph) + ", " + GetHomelandAdjustment(chr.race, Combat.SavingThrow.PetrificationPolymorph));
            chr.WriteToDisplay("Rod, Staff, Wand                  : " + GetProfessionAdjustment(chr.BaseProfession, Combat.SavingThrow.RodStaffWand) + ", " + GetHomelandAdjustment(chr.race, Combat.SavingThrow.RodStaffWand));
            chr.WriteToDisplay("Spells                            : " + GetProfessionAdjustment(chr.BaseProfession, Combat.SavingThrow.Spell) + ", " + GetHomelandAdjustment(chr.race, Combat.SavingThrow.Spell));

            return true;
        }

        private static int GetBWSavingThrow(Character chr)
        {
            int BW = 0;

            switch (chr.BaseProfession)
            {
                case Character.ClassType.Thief:
                    BW += 1;
                    break;
                case Character.ClassType.Fighter:
                case Character.ClassType.Martial_Artist:
                case Character.ClassType.Thaumaturge:
                case Character.ClassType.Sorcerer:
                case Character.ClassType.Druid:
                    BW += 0;
                    break;
                case Character.ClassType.Berserker:
                case Character.ClassType.Ranger:
                case Character.ClassType.Knight:
                case Character.ClassType.Wizard:
                    BW += -1;
                    break;
                case Character.ClassType.Ravager:
                    BW += -3;
                    break;
                default:
                    break;
            }

            switch (chr.race)
            {
                case "Barbarian":
                case "the plains":
                case "Draznia":
                case "Hovath":
                case "Illyria":
                case "Lemuria":
                    BW += 0;
                    break;
                case "Leng":
                    BW += -1;
                    break;
                case "Mnar":
                case "Mu":
                    BW += -2;
                    break;
                default:
                    break;
            }

            switch (chr.Level)
            {
                case 0:
                    return 20 + BW;
                case 1:
                case 2:
                    return 19 + BW;
                case 3:
                case 4:
                    return 18 + BW;
                case 5:
                case 6:
                    return 17 + BW;
                case 7:
                case 8:
                    return 16 + BW;
                case 9:
                case 10:
                case 11:
                    return 15 + BW;
                case 12:
                case 13:
                case 14:
                case 15:
                    return 14 + BW;
                case 16:
                case 17:
                case 18:
                case 19:
                    return 13 + BW;
                case 20:
                case 21:
                case 22:
                case 23:
                    return 12 + BW;
                case 24:
                case 25:
                    return 11 + BW;
                default:
                    return 11 + BW;
            }
        }

        private static int GetSpellsSavingThrow(Character chr)
        {
            int S = 0;

            switch (chr.BaseProfession)
            {
                case Character.ClassType.Berserker:
                case Character.ClassType.Fighter:
                case Character.ClassType.Martial_Artist:
                case Character.ClassType.Ranger:
                    S += 0;
                    break;
                case Character.ClassType.Thaumaturge:
                case Character.ClassType.Thief:
                case Character.ClassType.Druid:
                    S += -1;
                    break;
                case Character.ClassType.Knight:
                case Character.ClassType.Ravager:
                case Character.ClassType.Sorcerer:
                    S += -2;
                break;
                
                case Character.ClassType.Wizard:
                    S += -3;
                    break;
                default:
                    break;
            }

            switch (chr.race)
            {
                case "Draznia":
                case "Illyria":
                case "Mnar":
                case "the plains":
                case "Barbarian":
                    S += 0;
                    break;
                case "Leng":
                case "Mu":
                    S += -1;
                    break;
                case "Lemuria":
                case "Hovath":
                    S += -2;
                    break;
                default:
                    break;
            }

            switch (chr.Level)
            {
                case 0:
                    return 20 + S;
                case 1:
                case 2:
                    return 19 + S;
                case 3:
                case 4:
                    return 18 + S;
                case 5:
                case 6:
                    return 17 + S;
                case 7:
                    return 16 + S;
                case 8:
                case 9:
                    return 15 + S;
                case 10:
                case 11:
                case 12:
                case 13:
                    return 14 + S;
                case 14:
                case 15:
                case 16:
                case 17:
                    return 13 + S;
                case 18:
                case 19:
                case 20:
                case 21:
                    return 12 + S;
                case 22:
                case 23:
                    return 11 + S;
                case 24:
                case 25:
                    return 10 + S;
                default:
                    return 10 + S;
            }
        }

        public static int GetSavingThrow(Character chr, Combat.SavingThrow savingThrow)
        {
            switch (savingThrow)
            {
                case Combat.SavingThrow.BreathWeapon:
                    return GetBWSavingThrow(chr);
                case Combat.SavingThrow.ParalyzationPoisonDeath:
                    return GetPPDSavingThrow(chr);
                case Combat.SavingThrow.PetrificationPolymorph:
                    return GetPPSavingThrow(chr);
                case Combat.SavingThrow.RodStaffWand:
                    return GetRSWSavingThrow(chr);
                case Combat.SavingThrow.Spell:
                default:
                    return GetSpellsSavingThrow(chr);
            }
        }

        private static int GetRSWSavingThrow(Character chr)
        {
            int RSW = 0;

            switch (chr.BaseProfession)
            {
                case Character.ClassType.Berserker:
                    RSW += 1;
                    break;
                case Character.ClassType.Thaumaturge:
                    RSW += 0;
                    break;
                case Character.ClassType.Fighter:
                case Character.ClassType.Martial_Artist:
                case Character.ClassType.Sorcerer:
                case Character.ClassType.Thief:
                case Character.ClassType.Ranger:
                    RSW += -1;
                    break;
                case Character.ClassType.Knight:
                case Character.ClassType.Ravager:
                    RSW += -2;
                    break;
                case Character.ClassType.Druid:
                case Character.ClassType.Wizard:
                    RSW += -3;
                    break;
                default:
                    break;
            }

            switch (chr.race)
            {
                case "Mnar":
                    RSW += 0;
                    break;
                case "Draznia":
                case "Hovath":
                case "Illyria":
                case "Leng":
                case "Mu":
                    RSW += -1;
                    break;
                case "the plains":
                case "Barbarian":
                case "Lemuria":
                    RSW += -2;
                    break;
                default:
                    break;
            }

            switch (chr.Level)
            {
                case 0:
                    return 20 + RSW;
                case 1:
                case 2:
                    return 19 + RSW;
                case 3:
                case 4:
                    return 18 + RSW;
                case 5:
                    return 17 + RSW;
                case 6:
                case 7:
                    return 16 + RSW;
                case 8:
                case 9:
                    return 15 + RSW;
                case 10:
                case 11:
                case 12:
                case 13:
                    return 14 + RSW;
                case 14:
                case 15:
                    return 13 + RSW;
                case 16:
                case 17:
                case 18:
                case 19:
                    return 12 + RSW;
                case 20:
                case 21:
                case 22:
                case 23:
                    return 11 + RSW;
                case 24:
                case 25:
                    return 10 + RSW;
                default:
                    return 10 + RSW;
            }
        }

        private static int GetPPDSavingThrow(Character chr)
        {
            int PPD = 0;

            switch (chr.BaseProfession)
            {
                case Character.ClassType.Wizard:
                    PPD += 0;
                    break;
                case Character.ClassType.Fighter:
                case Character.ClassType.Martial_Artist:
                case Character.ClassType.Sorcerer:
                case Character.ClassType.Thaumaturge:
                case Character.ClassType.Druid:
                    PPD += -1;
                    break;
                case Character.ClassType.Ranger:
                case Character.ClassType.Ravager:
                    PPD += -2;
                    break;
                case Character.ClassType.Berserker:
                case Character.ClassType.Knight:
                    PPD += -3;
                    break;
                case Character.ClassType.Thief:
                    PPD += -4;
                    break;
            }

            switch (chr.race)
            {
                case "Draznia":
                case "Illyria":
                    PPD += -2;
                    break;
                case "Lemuria":
                case "Leng":
                case "Mu":
                    PPD += 0;
                    break;
                case "the plains":
                case "Barbarian":
                case "Hovath":
                case "Mnar":
                    PPD += -1;
                    break;
                default:
                    break;
            }

            switch (chr.Level)
            {
                case 0:
                    return 20 + PPD;
                case 1:
                    return 19 + PPD;
                case 2:
                    return 18 + PPD;
                case 3:
                    return 18 + PPD;
                case 4:
                    return 17 + PPD;
                case 5:
                    return 16 + PPD;
                case 6:
                    return 16 + PPD;
                case 7:
                    return 15 + PPD;
                case 8:
                    return 14 + PPD;
                case 9:
                    return 14 + PPD;
                case 10:
                    return 13 + PPD;
                case 11:
                    return 12 + PPD;
                case 12:
                    return 12 + PPD;
                case 13:
                    return 12 + PPD;
                case 14:
                    return 12 + PPD;
                case 15:
                    return 12 + PPD;
                case 16:
                    return 11 + PPD;
                case 17:
                    return 11 + PPD;
                case 18:
                    return 10 + PPD;
                case 19:
                    return 10 + PPD;
                case 20:
                    return 10 + PPD;
                case 21:
                    return 10 + PPD;
                case 22:
                    return 9 + PPD;
                case 23:
                    return 9 + PPD;
                case 24:
                    return 8 + PPD;
                case 25:
                    return 8 + PPD;
                default:
                    return 7 + PPD;

            }
        }

        private static int GetPPSavingThrow(Character chr)
        {
            int PP = 0;

            switch (chr.BaseProfession)
            {
                case Character.ClassType.Wizard:
                    PP += 0;
                    break;
                case Character.ClassType.Fighter:
                case Character.ClassType.Martial_Artist:
                case Character.ClassType.Sorcerer:
                case Character.ClassType.Thaumaturge:
                case Character.ClassType.Thief:
                    PP += -1;
                    break;
                case Character.ClassType.Druid:
                case Character.ClassType.Ravager:
                    PP += -2;
                    break;
                case Character.ClassType.Berserker:
                case Character.ClassType.Knight:
                case Character.ClassType.Ranger:
                    PP += -3;
                    break;
            }

            switch (chr.race)
            {
                case "Hovath":
                case "Lemuria":
                case "Mu":
                    PP += 0;
                    break;
                case "Barbarian":
                case "the plains":
                case "Leng":
                case "Mnar":
                    PP += -1;
                    break;
                case "Draznia":
                case "Illyria":
                    PP += -2;
                    break;
                default:
                    break;
            }

            switch (chr.Level)
            {
                case 0:
                    return 20 + PP;
                case 1:
                    return 19 + PP;
                case 2:
                    return 19 + PP;
                case 3:
                    return 18 + PP;
                case 4:
                    return 17 + PP;
                case 5:
                    return 17 + PP;
                case 6:
                    return 16 + PP;
                case 7:
                    return 16 + PP;
                case 8:
                    return 15 + PP;
                case 9:
                    return 14 + PP;
                case 10:
                    return 14 + PP;
                case 11:
                    return 14 + PP;
                case 12:
                    return 13 + PP;
                case 13:
                    return 13 + PP;
                case 14:
                    return 13 + PP;
                case 15:
                    return 13 + PP;
                case 16:
                    return 12 + PP;
                case 17:
                    return 12 + PP;
                case 18:
                    return 11 + PP;
                case 19:
                    return 11 + PP;
                case 20:
                    return 11 + PP;
                case 21:
                    return 11 + PP;
                case 22:
                    return 10 + PP;
                case 23:
                    return 10 + PP;
                case 24:
                    return 10 + PP;
                case 25:
                    return 10 + PP;
                default:
                    return 9 + PP;
            }
        }

        private static int GetHomelandAdjustment(string race, Combat.SavingThrow savingThrow)
        {
            int PPD = 0; int PP = 0; int RSW = 0; int S = 0; int BW = 0;

            switch (race)
            {
                case "Illyria":
                    PPD += -2;
                    PP += -2;
                    RSW += -1;
                    S += 0;
                    BW += 0;
                    break;
                case "Mu":
                    PPD += 0;
                    PP += 0;
                    RSW += -1;
                    S += -1;
                    BW += -2;
                    break;
                case "Lemuria":
                    PPD += 0;
                    PP += 0;
                    RSW += -2;
                    S += -2;
                    BW += 0;
                    break;
                case "Leng":
                    PPD += 0;
                    PP += -1;
                    RSW += -1;
                    S += -1;
                    BW += -1;
                    break;
                case "Draznia":
                    PPD += -2;
                    PP += -2;
                    RSW += -1;
                    S += 0;
                    BW += 0;
                    break;
                case "Hovath":
                    PPD += -1;
                    PP += 0;
                    RSW += -1;
                    S += -2;
                    BW += 0;
                    break;
                case "Mnar":
                    PPD += -1;
                    PP += -1;
                    RSW += 0;
                    S += 0;
                    BW += -2;
                    break;
                case "the plains":
                case "Barbarian":
                    PPD += -1;
                    PP += -1;
                    RSW += -2;
                    S += 0;
                    BW += 0;
                    break;
                default:
                    //PPD += 0;
                    //PP += -1;
                    //RSW += -1;
                    //S += 0;
                    //BW += -2;
                    break;
            }

            switch (savingThrow)
            {
                case Combat.SavingThrow.BreathWeapon:
                    return BW;
                case Combat.SavingThrow.ParalyzationPoisonDeath:
                    return PPD;
                case Combat.SavingThrow.PetrificationPolymorph:
                    return PP;
                case Combat.SavingThrow.RodStaffWand:
                    return RSW;
                case Combat.SavingThrow.Spell:
                    return S;
                default:
                    return 0;
            }
        }

        private static int GetProfessionAdjustment(Character.ClassType baseProfession, Combat.SavingThrow savingThrow)
        {
            int PPD = 0; int PP = 0; int RSW = 0; int S = 0; int BW = 0;

            switch (baseProfession)
            {
                case Character.ClassType.Berserker:
                    #region PPD+++, PP+++, RSW+, S+, BW++
                    PPD += -4;
                    PP += -4;
                    RSW += -0;
                    S += -0;
                    BW += 2;
                    break;
                #endregion
                case Character.ClassType.Sorcerer:
                    #region PPD++, PP++, RSW+, S++, BW
                    PPD += -2;
                    PP += -2;
                    RSW += -1;
                    S += -2;
                    BW += 0;
                    break;
                    #endregion
                case Character.ClassType.Ravager:
                    #region PPD+++, PP+++, RSW++, S++, BW+++
                    PPD += -3;
                    PP += -3;
                    RSW += -2;
                    S += -2;
                    BW += -3;
                    break;
                    #endregion
                case Character.ClassType.Fighter:
                    #region PPD++, PP++, RSW+, S, BW
                    PPD += -2;
                    PP += -2;
                    RSW += -1;
                    S += 0;
                    BW += 0;
                    break;
                    #endregion
                case Character.ClassType.Knight:
                    #region PPD++++, PP++++, RSW++, S++, BW+
                    PPD += -4;
                    PP += -4;
                    RSW += -2;
                    S += -2;
                    BW += -1;
                    break;
                    #endregion
                case Character.ClassType.Thaumaturge:
                    #region PPD++, PP++, RSW, S+, BW
                    PPD += -2;
                    PP += -2;
                    RSW += 0;
                    S += -1;
                    BW += 0;
                    break;
                    #endregion
                case Character.ClassType.Thief:
                    #region PPD+++++, PP++, RSW+, S+, BW+
                    PPD += -5;
                    PP += -2;
                    RSW += -1;
                    S += -1;
                    BW += 1;
                    break;
                    #endregion
                case Character.ClassType.Martial_Artist:
                    #region PPD++, PP++, RSW+, S, BW
                    PPD += -2;
                    PP += -2;
                    RSW += -1;
                    S += 0;
                    BW += 0;
                    break;
                    #endregion
                case Character.ClassType.Wizard:
                    #region PPD, PP, RSW+++, S+++, BW+
                    PPD += 0;
                    PP += 0;
                    RSW += -3;
                    S += -3;
                    BW += -1;
                    break;
                    #endregion
                default:
                    break;
            }

            switch (savingThrow)
            {
                case Combat.SavingThrow.BreathWeapon:
                    return BW;
                case Combat.SavingThrow.ParalyzationPoisonDeath:
                    return PPD;
                case Combat.SavingThrow.PetrificationPolymorph:
                    return PP;
                case Combat.SavingThrow.RodStaffWand:
                    return RSW;
                case Combat.SavingThrow.Spell:
                    return S;
                default:
                    return 0;
            }
        }
    }
}