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
using DragonsSpine.GameWorld;

namespace DragonsSpine
{
    public partial class MapEditor : Form
    {
        private Character target = new Character();
        private int landIndex = 0;
        private int mapIndex = 0;
        private Map map;

        public MapEditor()
        {
            InitializeComponent();
            PopulateOpenMapMenuItems();
        }

        void PopulateOpenMapMenuItems()
        {
            ToolStripMenuItem menuItem;
            ToolStripMenuItem menuItem2;
            for (int a = 0; a < World.GetFacetByIndex(0).Lands.Count; a++) // loop through lands and add menu item
            {
                menuItem = new ToolStripMenuItem(World.GetFacetByIndex(0).GetLandByIndex(a).Name);
                for (int b = 0; b < World.GetFacetByIndex(0).GetLandByIndex(a).MapDictionary.Count; b++) // loop through maps and add to land menu item
                {
                    menuItem2 = new ToolStripMenuItem(World.GetFacetByIndex(0).GetLandByIndex(a).GetMapByIndex(b).Name);
                    menuItem2.Click += new EventHandler(openMap_Click);
                    menuItem.DropDownItems.Add(menuItem2);
                }
                openMapToolStripDropDownButton.DropDownItems.Add(menuItem);
            }
        }

        void PopulateNPCCatalogList()
        {
            foreach (NPC npc in NPC.NPCInGameWorld)
            {

            }
        }

        private void openMap_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem mapItem = (ToolStripMenuItem)sender;
            ToolStripMenuItem landItem = (ToolStripMenuItem)mapItem.OwnerItem;

            this.landIndex = openMapToolStripDropDownButton.DropDownItems.IndexOf(mapItem.OwnerItem);
            this.mapIndex = landItem.DropDownItems.IndexOf(mapItem);

            this.OpenMap();
        }

        public void OpenMap()
        {
            try
            {
                map = World.GetFacetByIndex(0).GetLandByIndex(this.landIndex).GetMapByIndex(this.mapIndex);
                this.Text = "Dragon's Spine Map Editor [" + World.GetFacetByIndex(0).GetLandByIndex(this.landIndex).Name + " - " + map.Name + "]";

                this.mapTabControl.Controls.Clear();
                TabControl.TabPageCollection collection = new TabControl.TabPageCollection(this.mapTabControl);

                int[] zPlanesCopy = new int[map.ZPlanes.Count];
                map.ZPlanes.Keys.CopyTo(zPlanesCopy, 0);
                Array.Sort(zPlanesCopy);

                for (int z = 0; z < zPlanesCopy.Length; z++)
                {
                    TabPage tabPage = new TabPage(zPlanesCopy[z].ToString());
                    DataGridView dataGridView = new DataGridView();
                    DataGridViewCellStyle style = new DataGridViewCellStyle();
                    style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    style.Font = new Font("Courier New", 12);
                    style.WrapMode = DataGridViewTriState.False;
                    style.ForeColor = Color.Black;
                    style.BackColor = Color.White;
                    dataGridView.DefaultCellStyle = style;
                    dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.ColumnHeader;
                    dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
                    dataGridView.MultiSelect = false;
                    dataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
                    dataGridView.Name = map.Name + " [" + zPlanesCopy[z].ToString() + "]";
                    dataGridView.ColumnCount = map.ZPlanes[zPlanesCopy[z]].xcordMax - map.ZPlanes[zPlanesCopy[z]].xcordMin;
                    dataGridView.RowCount = map.ZPlanes[zPlanesCopy[z]].ycordMax - map.ZPlanes[zPlanesCopy[z]].ycordMin;
                    for (int y = map.ZPlanes[zPlanesCopy[z]].ycordMin, b = 0; y <= map.ZPlanes[zPlanesCopy[z]].ycordMax; y++, b++)
                    {
                        for (int x = map.ZPlanes[zPlanesCopy[z]].xcordMin, a = 0; x <= map.ZPlanes[zPlanesCopy[z]].xcordMax; x++, a++)
                        {
                            Cell cell = Cell.GetCell(map.FacetID, map.LandID, map.MapID, x, y, zPlanesCopy[z]);
                            if (cell != null)
                            {
                                if (dataGridView.Columns.Count > a && dataGridView.Rows.Count > b)
                                {
                                    //if (this.rbtnColorSpawnZones.Checked && cell.SpawnZoneList.Count > 0)
                                    //{
                                    //    dataGridView[a, b].Style.BackColor = Color.Green;
                                    //}
                                    if (this.rbtnColorItemLocations.Checked && cell.Items.Count > 0)
                                    {
                                        dataGridView[a, b].Style.BackColor = Color.Green;
                                    }
                                    else if (this.rbtnColorCreatureLocations.Checked && cell.Characters.Count > 0)
                                    {
                                        foreach (Character ch in cell.Characters.Values)
                                        {
                                            if (!ch.IsPC)
                                            {
                                                dataGridView[a, b].Style.BackColor = Color.Green;
                                                break;
                                            }
                                        }
                                    }
                                    else if (this.rbtnColorPlayerLocations.Checked && cell.Characters.Count > 0)
                                    {
                                        foreach (Character ch in cell.Characters.Values)
                                        {
                                            if (ch.IsPC)
                                            {
                                                dataGridView[a, b].Style.BackColor = Color.Green;
                                                break;
                                            }
                                        }
                                    }
                                    dataGridView[a, b].Value = cell.CellGraphic;
                                    if (dataGridView[a, b].OwningColumn.Width != dataGridView[a, b].OwningRow.Height)
                                    {
                                        dataGridView[a, b].OwningColumn.Width = dataGridView[a, b].OwningRow.Height;
                                    }
                                    if ((string)dataGridView[a, b].OwningRow.HeaderCell.Value != cell.Y.ToString())
                                    {
                                        dataGridView[a, b].OwningRow.HeaderCell.Value = cell.Y.ToString();
                                    }
                                    if (dataGridView[a, b].OwningColumn.HeaderText != cell.X.ToString())
                                    {
                                        dataGridView[a, b].OwningColumn.HeaderText = cell.X.ToString();
                                    }
                                }
                            }
                        }
                    }
                    
                    tabPage.Controls.Add(dataGridView);
                    collection.Add(tabPage);
                }
            }
            catch (Exception e)
            {
                Utils.LogException(e);
            }
            
            populateLandTab();
            populateMapTab(map);
        }

        void populateLandTab()
        {
            Land land = World.GetFacetByIndex(0).GetLandByIndex(this.landIndex);
            tbxLandName.Text = land.Name;
            tbxLandShortDesc.Text = land.ShortDesc;
            tbxLandLongDesc.Text = land.LongDesc;

            string[] classTypes = Enum.GetNames(target.BaseProfession.GetType());

            diceGrid.RowCount = classTypes.Length;
            diceGrid.ColumnCount = 3;

            for (int a = 0; a < classTypes.Length; a++)
            {
                diceGrid[0, a].OwningRow.HeaderCell.Value = classTypes[a];
                diceGrid[0, a].Value = land.HitDice[a];
                diceGrid[1, a].Value = land.ManaDice[a];
                diceGrid[2, a].Value = land.StaminaDice[a];
            }
            diceGrid[0, 0].OwningColumn.HeaderText = "Hits";
            diceGrid[1, 0].OwningColumn.HeaderText = "Mana";
            diceGrid[2, 0].OwningColumn.HeaderText = "Stamina";
        }

        void populateMapTab(Map map)
        {
            tbxMapName.Text = map.Name;
            tbxMapShortDesc.Text = map.ShortDesc;
            tbxMapLongDesc.Text = map.LongDesc;
            numMapDifficulty.Value = map.Difficulty;
            numMapExpModifier.Value = Convert.ToDecimal(map.ExperienceModifier);

            string[] climateTypes = Enum.GetNames(map.Climate.GetType());
            for (int a = 0; a < climateTypes.Length; a++)
            {
                cmbobxMapClimateType.Items.Add(climateTypes[a]);
            }
            cmbobxMapClimateType.SelectedItem = map.Climate.ToString();

            chkMapBalmBushes.Checked = map.HasBalmBushes;
            chkMapManaBushes.Checked = map.HasManaBushes;
            chkMapPoisonBushes.Checked = map.HasPoisonBushes;
            chkMapStaminaBushes.Checked = map.HasStaminaBushes;

            lblPlayerRespawn.Text = "Player Respawn: (" + map.ResX + ", " + map.ResY + ", " + map.ResZ+")";
            lblThiefRespawn.Text = "Thief Respawn: (" + map.ThiefResX + ", " + map.ThiefResY + ", " + map.ThiefResZ+")";
            lblKarmaRespawn.Text = "Karma Respawn: (" + map.KarmaResX + ", " + map.KarmaResY + ", " + map.KarmaResZ+")";

            chkMapPvPEnabled.Checked = map.IsPVPEnabled;
        }

        private void lblColorSpawnZones_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rbtnColorSpawnZones.Checked)
            {
                this.OpenMap();
            }
        }

        private void rbtnColorCreatureLocations_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rbtnColorCreatureLocations.Checked)
            {
                this.OpenMap();
            }
        }

        private void rbtnColorPlayerLocations_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rbtnColorPlayerLocations.Checked)
            {
                this.OpenMap();
            }
        }

        private void rbtnColorItemLocations_CheckedChanged(object sender, EventArgs e)
        {
            if (this.rbtnColorItemLocations.Checked)
            {
                this.OpenMap();
            }
        }
    }
}