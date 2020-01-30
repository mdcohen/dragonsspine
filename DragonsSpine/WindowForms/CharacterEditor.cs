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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Threading;
using World = DragonsSpine.GameWorld.World;

namespace DragonsSpine
{
    public partial class CharacterEditor : Form
    {
        bool npcMode = false;
        private List<Account> accounts;
        private NPC[] creatures;
        private Account currentAccount;
        private Character currentNPC;
        private PC currentPC;
        
        public CharacterEditor()
        {
            InitializeComponent();
            populateAccounts();
            populateAccountListing();
            populatePlayerComboBoxes();
        }

        private void populateAccounts()
        {
            accounts = DAL.DBAccount.GetAllAccounts();
        }

        private void populateCreatures()
        {
            creatures = new NPC[Character.NPCInGameWorld.Count];
            Character.NPCInGameWorld.CopyTo(creatures);
            //creatures = (ArrayList)Character.NPCList.Clone();
        }

        private void populateAccountListing()
        {
            lstbxAccountListing.Items.Clear();
            tbxAccountSearch.Clear();
            lstbxAccountListing.Sorted = true;
            for (int a = 0; a < accounts.Count; a++)
            {
                Account account = (Account)this.accounts[a];
                lstbxAccountListing.Items.Add(account.accountName);
            }
        }

        private void populateCreatureListing()
        {
            lstbxAccountListing.Items.Clear();
            tbxAccountSearch.Clear();
            lstbxAccountListing.Sorted = false;
            for (int a = 0; a < Character.NPCInGameWorld.Count; a++)
            {
                Character ch = this.creatures[a];
                try
                {
                    lstbxAccountListing.Items.Add(ch.Name + " (" + ch.Land.ShortDesc + " - " + ch.Land.MapDictionary[ch.MapID].Name + ")");
                }
                catch (Exception e)
                {
                    Utils.Log("Error populating Creature Listing at creature " + ch.GetLogString(), Utils.LogType.Unknown);
                    Utils.LogException(e);
                }

            }
        }

        private void populatePlayerComboBoxes()
        {
            int a;
            currentPC = new PC();
            for (a = 0; a < Enum.GetNames(currentPC.gender.GetType()).Length; a++) // gender combo box
            {
                cmbobxPlayerGender.Items.Add(Enum.GetName(currentPC.gender.GetType(), a));
            }
            for (a = 0; a < Enum.GetNames(currentPC.BaseProfession.GetType()).Length; a++) // class type combo box
            {
                cmbobxPlayerClass.Items.Add(Utils.FormatEnumString(Enum.GetName(currentPC.BaseProfession.GetType(), a)));
            }
            for (a = 0; a < Enum.GetNames(currentPC.Alignment.GetType()).Length; a++) // alignment combo box
            {
                cmbobxPlayerAlignment.Items.Add(Enum.GetName(currentPC.Alignment.GetType(), a));
            }
            for (a = 0; a < World.GetFacetByIndex(0).Lands.Count; a++) // lands combo box
            {
                cmbobxPlayerLand.Items.Add(World.GetFacetByIndex(0).GetLandByIndex(a).Name);
            }
            for (a = 0; a < Enum.GetNames(currentPC.ImpLevel.GetType()).Length; a++) // implevel combo box
            {
                cmbobxPlayerImpLevel.Items.Add(Enum.GetName(currentPC.ImpLevel.GetType(), a) + "(" + a.ToString() + ")");
            }
        }

        private Account getAccount(string accountName)
        {
            for (int a = 0; a < accounts.Count; a++)
            {
                Account account = (Account)accounts[a];
                if (accountName == account.accountName)
                {
                    return account;
                }
            }
            return null;
        }

        private void populateAccountGroup()
        {
            int a;
            tbxAccountName.Text = currentAccount.accountName;
            tbxAccountPassword.Text = currentAccount.password;
            tbxAccountVerifiedPassword.Text = currentAccount.password;
            tbxAccountIPAddress.Text = currentAccount.ipAddress;

            lstbxAccountIPList.Items.Clear();
            for (a = 0; a < currentAccount.ipAddressList.Length; a++)
            {
                if (currentAccount.ipAddressList[a] != null)
                {
                    lstbxAccountIPList.Items.Add(currentAccount.ipAddressList[a]);
                }
            }

            this.lstbxAccountPlayers.Items.Clear();
            for (a = 0; a < currentAccount.players.Length; a++)
            {
                if (currentAccount.players[a] != null)
                {
                    lstbxAccountPlayers.Items.Add(currentAccount.players[a]);
                }
            }

            numAccountCurrentMarks.Value = currentAccount.currentMarks;
            tbxAccountLifetimeMarks.Text = currentAccount.lifetimeMarks.ToString();
            tbxAccountLastOnline.Text = currentAccount.lastOnline.ToString();

            tbxAccountNotes.Text = currentAccount.notes;

            if(currentAccount.lastOnline.Add(TimeSpan.FromDays(120)) < DateTime.Now)
            {
                tbxAccountLastOnline.ForeColor = Color.DarkRed;
            }
            else
            {
                tbxAccountLastOnline.ForeColor = Color.Black;
            }
        }

        private void populatePlayerGroup()
        {
            if (!npcMode)
            {
                if (currentPC != null)
                {
                    tbxPlayerName.Text = currentPC.Name;
                    cmbobxPlayerGender.SelectedIndex = (int)currentPC.gender;
                    cmbobxPlayerClass.SelectedIndex = (int)currentPC.BaseProfession;
                    tbxPlayerClassFull.Text = currentPC.classFullName;
                    cmbobxPlayerRace.Items.Clear(); // TODO:
                    cmbobxPlayerAlignment.SelectedIndex = (int)currentPC.Alignment;
                    numPlayerAge.Value = currentPC.Age;
                    numPlayerStrength.Value = currentPC.Strength;
                    numPlayerDexterity.Value = currentPC.Dexterity;
                    numPlayerIntelligence.Value = currentPC.Intelligence;
                    numPlayerWisdom.Value = currentPC.Wisdom;
                    numPlayerConstitution.Value = currentPC.Constitution;
                    numPlayerCharisma.Value = currentPC.Charisma;
                    numPlayerStrengthAdd.Value = currentPC.strengthAdd;
                    numPlayerDexterityAdd.Value = currentPC.dexterityAdd;
                    cmbobxPlayerLand.SelectedIndex = currentPC.LandID;
                    for (int a = 0; a < currentPC.Land.MapDictionary.Count; a++) // maps combo box
                    {
                        cmbobxPlayerMap.Items.Add(World.GetFacetByIndex(0).GetLandByIndex(currentPC.LandID).GetMapByIndex(a).Name);
                    }
                    cmbobxPlayerMap.SelectedIndex = currentPC.MapID;
                    numPlayerXCord.Value = currentPC.X;
                    numPlayerYCord.Value = currentPC.Y;
                    cmbobxPlayerImpLevel.SelectedIndex = (int)currentPC.ImpLevel;
                    numPlayerHits.Value = currentPC.Hits;
                    numPlayerHitsMax.Value = currentPC.HitsMax;
                    numPlayerMana.Value = currentPC.Mana;
                    numPlayerManaMax.Value = currentPC.ManaMax;
                    numPlayerStamLeft.Value = currentPC.Stamina;
                    numPlayerStamina.Value = currentPC.StaminaMax;
                    numPlayerLevel.Value = currentPC.Level;
                    numPlayerExperience.Value = currentPC.Experience;
                    numPlayerBankGold.Value = (decimal)currentPC.bankGold;
                    chkPlayerDead.Checked = currentPC.IsDead;
                    chkPlayerHidden.Checked = currentPC.IsHidden;
                    chkPlayerBlind.Checked = currentPC.IsBlind;

                    tbxPlayerNumKills.Text = currentPC.Kills.ToString();
                    tbxPlayerNumDeaths.Text = currentPC.Deaths.ToString();

                    if (currentPC.spellDictionary.Count > 0)
                    {
                        spellsGrid.Enabled = true;
                        spellsGrid.ColumnCount = 2;
                        spellsGrid.RowCount = currentPC.spellDictionary.Count;
                        // TODO: make this a foreach loop through the Keys in the spellList Dictionary
                        //for (int a = 0; a < currentPC.spellList.Count; a++)
                        //{
                        //    spellsGrid[0, a].OwningRow.HeaderCell.Value = Spell.GetSpell((int)currentPC.spellList..ints[a]).Name;
                        //    spellsGrid[0, a].Value = ((int)currentPC.spellList.ints[a]).ToString();
                        //    spellsGrid[1, a].Value = currentPC.spellList.GetString(a);
                        //}
                    }
                    else
                    {
                        spellsGrid.Enabled = false;
                    }
                    tbxPlayerBirthday.Text = currentPC.birthday.ToString();
                    tbxPlayerLastOnline.Text = currentPC.lastOnline.ToString();

                    tbxPlayerNotes.Text = currentPC.Notes;
                }
            }
            else
            {
                if (currentNPC != null)
                {
                    tbxPlayerName.Text = currentNPC.Name;
                    cmbobxPlayerGender.SelectedIndex = (int)currentNPC.gender;
                    cmbobxPlayerClass.SelectedIndex = (int)currentNPC.BaseProfession;
                    tbxPlayerClassFull.Text = currentNPC.classFullName;
                    cmbobxPlayerRace.Items.Clear(); // TODO:
                    cmbobxPlayerAlignment.SelectedIndex = (int)currentNPC.Alignment;
                    numPlayerAge.Value = currentNPC.Age;
                    numPlayerStrength.Value = currentNPC.Strength;
                    numPlayerDexterity.Value = currentNPC.Dexterity;
                    numPlayerIntelligence.Value = currentNPC.Intelligence;
                    numPlayerWisdom.Value = currentNPC.Wisdom;
                    numPlayerConstitution.Value = currentNPC.Constitution;
                    numPlayerCharisma.Value = currentNPC.Charisma;
                    numPlayerStrengthAdd.Value = currentNPC.strengthAdd;
                    numPlayerDexterityAdd.Value = currentNPC.dexterityAdd;
                    cmbobxPlayerLand.SelectedIndex = currentNPC.LandID;
                    for (int a = 0; a < currentNPC.Land.MapDictionary.Count; a++) // maps combo box
                    {
                        cmbobxPlayerMap.Items.Add(World.GetFacetByIndex(0).GetLandByIndex(currentNPC.LandID).GetMapByIndex(a).Name);
                    }
                    cmbobxPlayerMap.SelectedIndex = currentNPC.MapID;
                    numPlayerXCord.Value = currentNPC.X;
                    numPlayerYCord.Value = currentNPC.Y;
                    //cmbobxPlayerImpLevel.SelectedIndex = (int)currentNPC.ImpLevel;
                    numPlayerHits.Value = currentNPC.Hits;
                    numPlayerHitsMax.Value = currentNPC.HitsMax;
                    numPlayerMana.Value = currentNPC.Mana;
                    numPlayerManaMax.Value = currentNPC.ManaMax;
                    numPlayerStamLeft.Value = currentNPC.Stamina;
                    numPlayerStamina.Value = currentNPC.StaminaMax;
                    numPlayerLevel.Value = currentNPC.Level;
                    numPlayerExperience.Value = currentNPC.Experience;
                    //numPlayerBankGold.Value = (decimal)currentNPC.bankGold;
                    chkPlayerDead.Checked = currentNPC.IsDead;
                    chkPlayerHidden.Checked = currentNPC.IsHidden;
                    chkPlayerBlind.Checked = currentNPC.IsBlind;

                    tbxPlayerNumKills.Text = currentNPC.Kills.ToString();
                    tbxPlayerNumDeaths.Text = currentNPC.Deaths.ToString();

                    if (currentNPC.spellDictionary.Count > 0)
                    {
                        spellsGrid.Enabled = true;
                        spellsGrid.ColumnCount = 2;
                        spellsGrid.RowCount = currentNPC.spellDictionary.Count;
                        // TODO: make this a foreach through the Keys in the spellList Dictionary
                        //for (int a = 0; a < currentNPC.spellList.Count; a++)
                        //{
                        //    spellsGrid[0, a].OwningRow.HeaderCell.Value = Spell.GetSpell((int)currentNPC.spellList.ints[a]).Name;
                        //    spellsGrid[0, a].Value = ((int)currentNPC.spellList.ints[a]).ToString();
                        //    spellsGrid[1, a].Value = currentNPC.spellList.GetString(a);
                        //}
                    }
                    else
                    {
                        spellsGrid.Enabled = false;
                    }

                    tbxPlayerBirthday.Text = (currentNPC as PC).birthday.ToString();
                    tbxPlayerLastOnline.Text = (currentNPC as PC).lastOnline.ToString();
                    tbxPlayerNotes.Text = currentNPC.Notes;
                }
            }
        }

        private bool validateAccountSave()
        {
            if (tbxAccountPassword.Text == tbxAccountVerifiedPassword.Text)
            {
                return true;
            }
            else
            {
                MessageBox.Show("Account Password and Verified Password Fields do not match.");
            }
            return false;
        }

        private void tbxAccountSearch_TextChanged(object sender, EventArgs e)
        {
            for (int a = 0; a < lstbxAccountListing.Items.Count; a++)
            {
                if (lstbxAccountListing.Items[a].ToString().ToLower().StartsWith(tbxAccountSearch.Text.ToLower()))
                {
                    lstbxAccountListing.SelectedIndex = a;
                    break;
                }
            }
        }

        private void btnToggleMode_Click(object sender, EventArgs e)
        {
            this.npcMode = !npcMode;
            if (!npcMode)
            {
                splitContainer1.SplitterDistance = 145;
                groupBox2.Show();
                btnToggleMode.Text = "Toggle to NPC Mode";
                groupBox1.Text = "Player Information";
                populateAccounts();
                populateAccountListing();
            }
            else
            {
                groupBox2.Hide();
                splitContainer1.SplitterDistance = 340;
                btnToggleMode.Text = "Toggle to PC Mode";
                groupBox1.Text = "Creature Information";
                populateCreatures();
                populateCreatureListing();
            }
        }

        private void lstbxAccountListing_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!npcMode)
            {
                currentAccount = getAccount(lstbxAccountListing.SelectedItem.ToString());
                populateAccountGroup();
            }
            else
            {
                currentNPC = (Character)creatures[lstbxAccountListing.SelectedIndex];
                populatePlayerGroup();
            }
        }

        private void lstbxAccountPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            int playerID = DAL.DBPlayer.GetPlayerID(lstbxAccountPlayers.SelectedItem.ToString());

            if (playerID != -1)
            {
                currentPC = DAL.DBPlayer.GetPCByID(playerID);
                populatePlayerGroup();
            }
        }

        private void btnAccountSave_Click(object sender, EventArgs e)
        {
            if (!npcMode)
            {
                if (validateAccountSave())
                {
                    if (currentAccount.Save() == 1)
                    {
                        MessageBox.Show("Account saved.");
                    }
                    else
                    {
                        MessageBox.Show("Account save failed.");
                    }
                }
            }
        }

        private void btnDeleteAccount_Click(object sender, EventArgs e)
        {
            if (!npcMode)
            {
                int playersDeleted = 0;
                ArrayList failedDelete = new ArrayList();

                if (MessageBox.Show("Are you sure you want to delete account (" + currentAccount.accountName +
                    ") and all of the account characters?", "Confirm Account Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    for (int a = 0; a < currentAccount.players.Length; a++)
                    {
                        if (currentAccount.players[a] != null && currentAccount.players[a] != "")
                        {
                            if (!DAL.DBPlayer.DeletePlayerFromDatabase(PC.GetPlayerID(currentAccount.players[a])))
                            {
                                failedDelete.Add(PC.GetPlayerID(currentAccount.players[a]));
                            }
                            else
                            {
                                playersDeleted++;
                                Utils.Log("Deleted " + currentAccount.players[a] + " from the database.", Utils.LogType.DeletePlayer);
                            }
                        }
                    }

                    if (DAL.DBAccount.DeleteAccount(currentAccount.accountID) == 1)
                    {
                        Utils.Log("Deleted " + currentAccount.GetLogString() + " from the database.", Utils.LogType.DeleteAccount);
                        MessageBox.Show("Deleted account " + currentAccount.GetLogString() + " and " + playersDeleted + " characters.");
                        if (failedDelete.Count > 0)
                        {
                            string failed = "";
                            for (int a = 0; a < failedDelete.Count; a++)
                            {
                                failed += "[" + failedDelete[a].ToString() + "] ";
                            }
                            Utils.Log("Failed to delete " + failed + " playerIDs from the database.", Utils.LogType.SystemFailure);
                            MessageBox.Show("Failed to delete " + failed + " player IDs from the database.");
                        }
                        populateAccounts(); // refresh local accounts arraylist
                        populateAccountListing(); // refresh the accounts listbox
                    }
                }
                else
                {
                    MessageBox.Show("Account delete canceled.");
                }
            }
        }

        private void purgeAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to purge accounts?", "Confirm Account Purge", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                int playersPurged = 0;
                int accountsPurged = 0;
                bool failedPlayerPurge = false;

                foreach (Account account in this.accounts)
                {
                    failedPlayerPurge = false;
                    if (account.lastOnline.Add(TimeSpan.FromDays(210)) < DateTime.Now)
                    {
                        if (account.players != null)
                        {
                            for (int a = 0; a < account.players.Length; a++)
                            {
                                if (account.players[a] != null && account.players[a] != "")
                                {
                                    if (!DAL.DBPlayer.DeletePlayerFromDatabase(PC.GetPlayerID(account.players[a])))
                                    {
                                        Utils.Log("Failed to purge character " + account.players[a] + " of account " + account.GetLogString() + " from the database.", Utils.LogType.SystemFailure);
                                        failedPlayerPurge = true;
                                    }
                                    else
                                    {
                                        playersPurged++;
                                        Utils.Log("Purged character " + account.players[a] + " of account " + account.GetLogString() + " from the database.", Utils.LogType.DeletePlayer);
                                    }
                                }
                            }
                        }

                        if (!failedPlayerPurge)
                        {
                            if (DAL.DBAccount.DeleteAccount(account.accountID) == 1)
                            {
                                Utils.Log("Purged " + account.GetLogString() + " with last online of " + account.lastOnline.ToString() + " from the database.", Utils.LogType.DeleteAccount);
                                accountsPurged++;
                            }
                        }
                    }

                }
                Utils.Log("Purged a total of " + playersPurged.ToString() + " players from the database.", Utils.LogType.SystemGo);
                Utils.Log("Purged a total of " + accountsPurged.ToString() + " accounts from the database.", Utils.LogType.SystemGo);
                populateAccounts(); // refresh local accounts arraylist
                populateAccountListing(); // refresh the accounts listbox
            }
        }
    }
}