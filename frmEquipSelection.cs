using QuickType;
using System;
using System.Windows.Forms;

namespace FreemanSaveEditor
{
    public partial class FrmEquipSelection : Form
    {
        private readonly string filterField = "";
        public long Id = 0;

        public FrmEquipSelection(EquipSelectionMode mode)
        {
            InitializeComponent();
            dgv.AutoGenerateColumns = true;
            filterField = "Name";

            switch (mode)
            {
                case EquipSelectionMode.Inventory:
                    dgv.DataSource = Program.CurrentItems.CopyToDataTable();
                    break;

                case EquipSelectionMode.Helmet:

                    dgv.DataSource = Program.CurrentHelmets.CopyToDataTable();
                    break;

                case EquipSelectionMode.Mask:
                    dgv.DataSource = Program.CurrentMasks.CopyToDataTable();
                    break;

                case EquipSelectionMode.Shirt:
                    dgv.DataSource = Program.CurrentShirts.CopyToDataTable();
                    break;

                case EquipSelectionMode.Pants:
                    dgv.DataSource = Program.CurrentPants.CopyToDataTable();
                    break;

                case EquipSelectionMode.Armor:
                    dgv.DataSource = Program.CurrentArmors.CopyToDataTable();
                    break;

                case EquipSelectionMode.Pistol:
                    dgv.DataSource = Program.CurrentPistols.CopyToDataTable();
                    break;

                case EquipSelectionMode.Weapon:
                case EquipSelectionMode.Weapon2:
                    dgv.DataSource = Program.CurrentWeapons.CopyToDataTable();
                    break;

                case EquipSelectionMode.Scope:
                    dgv.DataSource = Program.CurrentScopes.CopyToDataTable();
                    break;

                default:
                    dgv.DataSource = Program.CurrentMisc.CopyToDataTable();
                    break;
            }
        }

        public enum EquipSelectionMode
        {
            Inventory,
            Helmet,
            Mask,
            Shirt,
            Armor,
            Pistol,
            Weapon,
            Weapon2,
            Pants,
            Scope,
            Misc,
            Misc2,
            Misc3,
            Misc4,
            Misc5,
            Misc6,
            Misc7,
            Misc8,
            Misc9,
            Misc10,
            Misc11,
            Misc12,
        }

        public enum WeaponType
        {
            ASSAULTRIFLE,
            LAUNCHER,
            MACHINEGUN,
            PISTOL,
            RIFLE,
            SHOTGUN,
            SMG
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            (dgv.DataSource as System.Data.DataTable).DefaultView.RowFilter = string.Format("[{0}] LIKE '%{1}%'", filterField, txtSearch.Text);
        }

        private void Dgv_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgv.Rows.Count)
            {
                Id = Convert.ToInt64(dgv.Rows[e.RowIndex].Cells["Id"].Value);
                this.Close();
            }
        }
    }
}