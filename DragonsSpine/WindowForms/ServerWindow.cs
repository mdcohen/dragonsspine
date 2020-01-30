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
using System.Collections.Specialized;
using System.Threading;

namespace DragonsSpine
{
    public partial class ServerWindow : Form
    {
        ArrayList usersOnline = new ArrayList();
        
        public ServerWindow()
        {
            InitializeComponent();
            addLogOptionsClickHandler();
        }

        public void UpdateWindow()
        {
            this.Text = DragonsSpineMain.Instance.Settings.ServerName + " " + DragonsSpineMain.Instance.Settings.ServerVersion + " - " + DragonsSpineMain.ServerStatus.ToString();
            this.toolStripStatusLabel1.Text = "Round: " + DragonsSpineMain.GameRound.ToString();
            this.toolStripStatusLabel2.Text = "Creatures: " + Character.NPCInGameWorld.Count.ToString();
            //this.npcroundLabel.Text = "NPCRound: " + DragonsSpineMain.NPCRound.ToString();

            int p_login = Character.LoginList.Count;
            int p_charGen = Character.CharGenList.Count;
            int p_menu = Character.MenuList.Count;
            int p_conf = Character.ConfList.Count;
            int p_game = Character.PCInGameWorld.Count;
            int p_total = p_login + p_charGen + p_menu + p_conf + p_game;

            this.toolStripStatusLabel3.Text = "Players: Login " + p_login + ", CharGen " + p_charGen + ", Menu " + p_menu +
                " Conference " + p_conf + ", Game " + p_game + ", Total = " + p_total;
        }

        public bool Display(Utils.LogType logType)
        {
            bool append = false;

            switch (logType)
            {
                case Utils.LogType.CoinLogging:
                    append = this.coinLoggingToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.Connection:
                    append = this.iPAddressesToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.DeleteAccount:
                    append = this.accountDeleteToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.DeathCreature:
                    append = this.deathCreatureToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.DeathLair:
                    append = deathLairToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.DeathPlayer:
                    append = this.deathPlayersToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.Disconnect:
                    append = this.disconnectToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.ExceptionDetail:
                    append = false; // all exceptions are logged to a separate log
                    break;
                case Utils.LogType.ExperienceLevelGain:
                    append = this.levelGainToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.ExperienceMeleeKill:
                    append = this.meleeKillToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.ExperienceSpellAEKill:
                    append = this.spellAEKillToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.ExperienceSpellCasting:
                    append = this.spellcastingToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.ExperienceSpellKill:
                    append = this.spellKillToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.ExperienceTraining:
                    append = this.trainingToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.CombatDamageToCreature:
                    append = this.combatDamageToCreatureToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.CombatDamageToPlayer:
                    append = this.combatDamageToPlayerToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.CommandAllPlayer:
                    append = this.allPlayerCommandsToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.CommandFailure:
                    append = this.commandFailureToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.CommandImmortal:
                    append = this.immortalToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.CriticalCombatDamageToCreature:
                    append = this.criticalCombatDamageToCreatureToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.CriticalCombatDamageToPlayer:
                    append = this.criticalCombatDamageToPlayerToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.DeletePlayer:
                    append = this.playerDeleteToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.DisplayCreature:
                    append = this.creatureDisplayMessagesToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.DisplayPlayer:
                    append = this.playerDisplayMessagesToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.ItemAttuned:
                    append = this.attunedToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.ItemFigurineUse:
                    append = this.figurinesToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.Karma:
                    append = true; // always logged
                    break;
                case Utils.LogType.Login:
                    append = this.loginToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.Logout:
                    append = this.logoutToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.LootAlways:
                    append = this.alwaysItemsToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.LootBelt:
                    append = this.beltItemsToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.LootRare:
                    append = this.rareToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.LootVeryRare:
                    append = this.veryRareToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.Mark:
                    append = true; // always logged
                    break;
                case Utils.LogType.MerchantBuy:
                    append = this.buyToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.MerchantSell:
                    append = this.sellToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.MerchantTanning:
                    append = this.tanningToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.QuestCompletion:
                    append = this.questCompletionToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SkillGainCombat:
                    append = this.gainFromcombatToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SkillGainNonCombat:
                    append = this.gainFromnonCombatToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SkillGainRisk:
                    append = this.skillriskToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SkillTraining:
                    append = this.trainingToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SpellBeneficialFromCreature:
                    append = this.castingBeneficialFromCreatureToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SpellBeneficialFromPlayer:
                    append = this.castingBeneficialFromPlayerToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SpellDamageFromMapEffect:
                    append = this.spellDamageFromMapEffectToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SpellDamageToCreature:
                    append = this.spellDamageToCreatureToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SpellDamageToPlayer:
                    append = this.spellDamageToPlayerToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SpellHarmfulFromCreature:
                    append = this.castingHarmfulFromCreatureToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SpellHarmfulFromPlayer:
                    append = this.castingHarmfulFromPlayerToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SpellWarmingFromCreature:
                    append = this.warmingFromCreatureToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SpellWarmingFromPlayer:
                    append = this.warmingFromPlayerToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SystemFailure:
                    append = true;
                    break;
                case Utils.LogType.SystemFatalError:
                    append = true;
                    break;
                case Utils.LogType.SystemGo:
                    append = this.systemGoToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.SystemWarning:
                    append = this.systemWarningToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.Timeout:
                    append = this.timeoutToolStripMenuItem.Checked;
                    break;
                case Utils.LogType.Unknown:
                    append = true;
                    break;
            }
            return append;
        }

        public void DisplayLogMessage(string message, Utils.LogType logType, RichTextBox richTextBox)
        {
            bool append = false;
            bool colorLog = false;
            if (System.Configuration.ConfigurationManager.AppSettings["ColorLog"].ToLower() == "true")
                colorLog = true;

            if (richTextBox == null) { richTextBox = this.rtbxLog; }

            richTextBox.SelectionBackColor = richTextBox.BackColor;
            richTextBox.SelectionColor = richTextBox.ForeColor;

            if (richTextBox == this.rtbxLog)
            {
                switch (logType)
                {
                    case Utils.LogType.ExceptionDetail:
                        if (colorLog)
                        {
                            richTextBox.SelectionBackColor = Color.Red;
                            richTextBox.SelectionColor = Color.White;
                        }
                        richTextBox.AppendText(message);
                        return;
                    case Utils.LogType.SystemFailure:
                        richTextBox.SelectionColor = Color.Red;
                        this.rtbxLog.AppendText(message);
                        return;
                    case Utils.LogType.SystemFatalError:
                        richTextBox.SelectionBackColor = Color.Red;
                        richTextBox.SelectionColor = Color.White;
                        this.rtbxLog.AppendText(message);
                        return;
                    case Utils.LogType.SystemWarning:
                        richTextBox.SelectionColor = Color.Yellow;
                        this.rtbxLog.AppendText(message);
                        return;
                    case Utils.LogType.Unknown:
                        richTextBox.SelectionColor = Color.LightSalmon;
                        richTextBox.AppendText(message);
                        return;
                    default:
                        append = this.Display(logType);
                        break;
                
                }
            }
            else
            {
                append = true;
            }

            if (append)
            {
                string time = message.Substring(0, message.IndexOf("M: {") + 3);
                message = message.Replace(time, "");
                string logMessage = message.Substring(0, message.IndexOf("} ") + 1);
                message = message.Replace(logMessage, "");
                message = message.Substring(1);

                if (colorLog)
                    richTextBox.SelectionColor = Color.DarkCyan;

                richTextBox.AppendText(time);

                if(colorLog)
                    richTextBox.SelectionColor = Color.Cyan;

                richTextBox.AppendText(logMessage);
                richTextBox.AppendText(" ");
                switch (logType) // choose a color for the logType
                {
                    case Utils.LogType.DisplayCreature:
                        richTextBox.SelectionBackColor = Color.DarkGreen;
                        richTextBox.SelectionColor = Color.White;
                        break;
                    case Utils.LogType.DisplayPlayer:
                        richTextBox.SelectionBackColor = Color.Blue;
                        richTextBox.SelectionColor = Color.White;
                        break;
                    default:
                        richTextBox.SelectionBackColor = richTextBox.BackColor;
                        richTextBox.SelectionColor = richTextBox.ForeColor;
                        break;
                }
                richTextBox.AppendText(message);
            }
        }

        private void addLogOptionsClickHandler()
        {
            for (int a = 0; a < logOptionsToolStripMenuItem.DropDownItems.Count; a++)
            {
                ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)logOptionsToolStripMenuItem.DropDownItems[a];
                for (int b = 0; b < toolStripMenuItem.DropDownItems.Count; b++)
                {
                    ToolStripMenuItem toolStripMenuItem2 = (ToolStripMenuItem)toolStripMenuItem.DropDownItems[b];
                    toolStripMenuItem2.CheckOnClick = true;
                    toolStripMenuItem2.Click += new System.EventHandler(this.logOptionsMenuItem_Click);

                }
            }
        }

        private void logOptionsMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            toolStripStatusLabel4.Text = toolStripMenuItem.Text.ToString().Replace("&", "") + " logging set to " + toolStripMenuItem.Checked.ToString().ToUpper() + ".";
        }

        private void enableAllLogOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem enableToolStripMenuItem = (ToolStripMenuItem)sender;
            if (enableToolStripMenuItem.Text == "Enable All Log Options")
            {
                enableToolStripMenuItem.Text = "Disable All Log Options";
                for (int a = 0; a < logOptionsToolStripMenuItem.DropDownItems.Count; a++)
                {
                    ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)logOptionsToolStripMenuItem.DropDownItems[a];
                    for (int b = 0; b < toolStripMenuItem.DropDownItems.Count; b++)
                    {
                        ToolStripMenuItem toolStripMenuItem2 = (ToolStripMenuItem)toolStripMenuItem.DropDownItems[b];
                        if (toolStripMenuItem2.Enabled) { toolStripMenuItem2.Checked = true; }

                    }
                }
                toolStripStatusLabel4.Text = "Enabled all log options.";
            }
            else
            {
                enableToolStripMenuItem.Text = "Enable All Log Options";
                for (int a = 0; a < logOptionsToolStripMenuItem.DropDownItems.Count; a++)
                {
                    ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)logOptionsToolStripMenuItem.DropDownItems[a];
                    for (int b = 0; b < toolStripMenuItem.DropDownItems.Count; b++)
                    {
                        ToolStripMenuItem toolStripMenuItem2 = (ToolStripMenuItem)toolStripMenuItem.DropDownItems[b];
                        if (toolStripMenuItem2.Enabled) { toolStripMenuItem2.Checked = false; }

                    }
                }
                toolStripStatusLabel4.Text = "Disabled all log options.";
            }
        }

        private void lockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DragonsSpineMain.ServerStatus == DragonsSpineMain.ServerState.Locked)
            {
                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Running;
                this.lockToolStripMenuItem.Text = "&Lock";
                this.toolStripStatusLabel4.Text = "The server was unlocked at " + DateTime.Now.ToString() + ".";
            }
            else
            {
                DragonsSpineMain.ServerStatus = DragonsSpineMain.ServerState.Locked;
                this.lockToolStripMenuItem.Text = "Un&lock";
                this.toolStripStatusLabel4.Text = "The server was locked at " + DateTime.Now.ToString() + ".";
            }
        }

        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel4.Text = "Shutting down the server...";
            this.Close();
        }

        private void ServerWindow_Closing(object sender, EventArgs e)
        {
            IO.Close();
            System.Environment.Exit(0);
        }

        private void mapEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MapEditor mapEditor = new MapEditor();
            mapEditor.Show();
        }

        private void characterEditorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CharacterEditor characterEditor = new CharacterEditor();
            characterEditor.Show();
        }

        private void viewFullLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form form = new Form();
            form.Text = "";
        }

        private void announceToolStripTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                System.Windows.Forms.DialogResult result = MessageBox.Show("ANNOUNCE: " + this.announceToolStripTextBox.Text, "Confirm Announcement", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);

                if (result == DialogResult.OK)
                {
                    Conference.ChatCommands(null, "/announce", this.announceToolStripTextBox.Text);
                }

                this.announceToolStripTextBox.Clear();
                this.rtbxLog.Focus();
            }
        }
    }
}