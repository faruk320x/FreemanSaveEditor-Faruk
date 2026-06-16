using QuickType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FreemanSaveEditor
{
    public partial class FrmMain
    {
        private long ShowInventorySelection(bool returnId = false, int RowIndex = -1, int ColIndex = -1)
        {
            if (!returnId && dgvInventory.SelectedCells.Count < 1) return 0;

            using (FrmEquipSelection equipSelection = new FrmEquipSelection(FrmEquipSelection.EquipSelectionMode.Inventory))
            {
                equipSelection.ShowDialog();

                if (equipSelection.Id > 0)
                {
                    if (returnId)
                        return equipSelection.Id;

                    if (RowIndex > -1 && ColIndex > -1)
                    {
                        dgvInventory.Rows[RowIndex].Cells[ColIndex].Value = equipSelection.Id;
                        return equipSelection.Id;
                    }
                    foreach (DataGridViewCell cell in dgvInventory.SelectedCells)
                    {
                        cell.Value = equipSelection.Id;
                    }
                    return equipSelection.Id;
                }
                return 0;
            }
        }

        private void ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode mode, int RowIndex, int ColIndex)
        {
            List<DataGridViewRow> rows = new List<DataGridViewRow>();

            if (RowIndex > -1 && ColIndex > -1)
                rows.Add(dgvSquadEquips.Rows[RowIndex]);
            else if (RowIndex == -99)
            {
                rows = SelectedRows;
            }
            else
            {
                return;
            }

            using (FrmEquipSelection equipSelection = new FrmEquipSelection(mode))
            {
                equipSelection.ShowDialog();

                if (equipSelection.Id > 0)
                {
                    foreach (DataGridViewRow row in rows)
                    {
                        row.Cells[ColIndex].Value = equipSelection.Id;
                    }
                }
            }
        }

        private void ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode mode)
        {
            using (FrmEquipSelection equipSelection = new FrmEquipSelection(mode))
            {
                equipSelection.ShowDialog();

                if (equipSelection.Id > 0)
                {
                    switch (mode)
                    {
                        case FrmEquipSelection.EquipSelectionMode.Inventory:
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Helmet:
                            Program.CurrentPlayer.HelmetId = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Mask:
                            Program.CurrentPlayer.MaskId = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Shirt:
                            Program.CurrentPlayer.ShirtId = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Armor:
                            Program.CurrentPlayer.ArmorId = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Pistol:
                            Program.CurrentPlayer.PistolId = equipSelection.Id;
                            Program.CurrentPlayer.Pa1Id = Program.CurrentPlayer.Pa2Id = Program.CurrentPlayer.Pa3Id = Program.CurrentPlayer.Pa4Id
                            = Program.WeaponList.Where(x => x.Id == equipSelection.Id).First().AmmoType;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Weapon:
                            Program.CurrentPlayer.RifleId = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Pants:
                            Program.CurrentPlayer.PantsId = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Weapon2:
                            Program.CurrentPlayer.LauncherId = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc:
                            Program.CurrentPlayer.Misc1Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc2:
                            Program.CurrentPlayer.Misc2Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc3:
                            Program.CurrentPlayer.Misc3Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc4:
                            Program.CurrentPlayer.Misc4Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc5:
                            Program.CurrentPlayer.Misc5Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc6:
                            Program.CurrentPlayer.Misc6Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc7:
                            Program.CurrentPlayer.Misc7Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc8:
                            Program.CurrentPlayer.Misc8Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc9:
                            Program.CurrentPlayer.Misc9Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc10:
                            Program.CurrentPlayer.Misc10Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc11:
                            Program.CurrentPlayer.Misc11Id = equipSelection.Id;
                            break;

                        case FrmEquipSelection.EquipSelectionMode.Misc12:
                            Program.CurrentPlayer.Misc12Id = equipSelection.Id;
                            break;

                        default:
                            break;
                    }
                }
                RefreshDescriptions();
            }
        }

        private void PicHelmet_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Helmet);
        }

        private void PicMask_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Mask);
        }

        private void PicShirt_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Shirt);
        }

        private void PicArmor_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Armor);
        }

        private void PicPants_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Pants);
        }

        private void PicPistol_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Pistol);
        }

        private void PicPistolSilencer_Click(object sender, EventArgs e)
        {
            var silencerId = Program.ItemList.Where(x => x.SlotType == SlotType.Pistolsilencer).First().Id;

            if (Program.CurrentPlayer.PistolSilencerId == 0)
                Program.CurrentPlayer.PistolSilencerId = silencerId;
            else
                Program.CurrentPlayer.PistolSilencerId = 0;
            RefreshDescriptions();
        }

        private void PicWep1_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Weapon);
        }

        private void PicWep1Scope_Click(object sender, EventArgs e)
        {
            using (FrmEquipSelection equipSelection = new FrmEquipSelection(FrmEquipSelection.EquipSelectionMode.Scope))
            {
                equipSelection.ShowDialog();

                if (equipSelection.Id > 0)
                {
                    Program.CurrentPlayer.RifleScopeIdL = equipSelection.Id;
                }
                RefreshDescriptions();
            }
        }

        private void PicWep1Silencer_Click(object sender, EventArgs e)
        {
            var silencerId = Program.ItemList.Where(x => x.SlotType == SlotType.Riflesilencer).First().Id;

            if (Program.CurrentPlayer.RifleSilencerIdL == 0)
                Program.CurrentPlayer.RifleSilencerIdL = silencerId;
            else
                Program.CurrentPlayer.RifleSilencerIdL = 0;
            RefreshDescriptions();
        }

        private void PicWep2_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Weapon2);
        }

        private void PicWep2Scope_Click(object sender, EventArgs e)
        {
            using (FrmEquipSelection equipSelection = new FrmEquipSelection(FrmEquipSelection.EquipSelectionMode.Scope))
            {
                equipSelection.ShowDialog();

                if (equipSelection.Id > 0)
                {
                    Program.CurrentPlayer.RifleScopeIdR = equipSelection.Id;
                }
                RefreshDescriptions();
            }
        }

        private void PicWep2Silencer_Click(object sender, EventArgs e)
        {
            var silencerId = Program.ItemList.Where(x => x.SlotType == SlotType.Riflesilencer).First().Id;

            if (Program.CurrentPlayer.RifleSilencerIdR == 0)
                Program.CurrentPlayer.RifleSilencerIdR = silencerId;
            else
                Program.CurrentPlayer.RifleSilencerIdR = 0;
            RefreshDescriptions();
        }

        private void PicMisc1_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc);
        }

        private void PicMisc2_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc2);
        }

        private void PicMisc3_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc3);
        }

        private void PicMisc4_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc4);
        }

        private void picMisc5_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc5);
        }

        private void PicMisc6_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc6);
        }

        private void PicMisc7_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc7);
        }

        private void PicMisc8_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc8);
        }

        private void PicMisc9_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc9);
        }

        private void PicMisc10_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc10);
        }

        private void PicMisc11_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc11);
        }

        private void PicMisc12_Click(object sender, EventArgs e)
        {
            ShowPlayerEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc12);
        }
    }
}