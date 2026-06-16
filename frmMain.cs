using FreemanSaveEditor.Properties;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using QuickType;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FreemanSaveEditor
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();

            dgvSquadEquips.AutoGenerateColumns = false;
            dgvSquadStats.AutoGenerateColumns = false;

            //var col = new List<KeyValuePair<string, long>>();

            //for (int i = 1; i <= 100; i++)
            //{
            //    col.Add(new KeyValuePair<string, long>(i.ToString(), i));
            //}

            //colSmgPoint.DataSource = col;
            //colSmgPoint.DisplayMember = "Key";
            //colSmgPoint.ValueMember = "Value";
        }

        private void BtBrowseGame_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                folderBrowserDialog.SelectedPath = txtGameLocation.Text;
                folderBrowserDialog.ShowNewFolderButton = false;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (!Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.exe", SearchOption.TopDirectoryOnly).Any(x => x.Contains("StartFGW.exe")))
                        {
                            _ = MessageBox.Show("StartFGW.exe not found, please make sure to select root folder of installation folder",
                                                "Warning",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Exclamation);
                            txtGameLocation.Text = "";
                            Properties.Settings.Default.Save();
                            return;
                        }
                        txtGameLocation.Text = folderBrowserDialog.SelectedPath;
                        Properties.Settings.Default.Save();
                    }
                    catch (IOException ex)
                    {
                        _ = MessageBox.Show(ex.Message,
                                               "Error",
                                               MessageBoxButtons.OK,
                                               MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtBrowseSave_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                folderBrowserDialog.SelectedPath = txtSaveLocation.Text;
                folderBrowserDialog.ShowNewFolderButton = false;
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        if (!Directory.GetFiles(folderBrowserDialog.SelectedPath, "*.json", SearchOption.AllDirectories).Any(x => x.Contains("MapAgent.json")))
                        {
                            _ = MessageBox.Show("Save Files not found, please make sure to select root folder of saves",
                                                "Warning",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Exclamation);
                            txtSaveLocation.Text = "";
                            Properties.Settings.Default.Save();
                            return;
                        }
                        txtSaveLocation.Text = folderBrowserDialog.SelectedPath;
                        Properties.Settings.Default.Save();
                    }
                    catch (IOException ex)
                    {
                        _ = MessageBox.Show(ex.Message,
                                               "Error",
                                               MessageBoxButtons.OK,
                                               MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void TxtGameLocation_MouseClick(object sender, MouseEventArgs e)
        {
            BtBrowseGame_Click(sender, e);
            _ = btBrowseGame.Focus();
        }

        private void TxtSaveLocation_MouseClick(object sender, MouseEventArgs e)
        {
            BtBrowseSave_Click(sender, e);
            _ = btBrowseSave.Focus();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtSaveLocation.Text))
            {
                Guid localLowId = new Guid("A520A1A4-1780-4FF6-BD18-167343C5AF16");
                string LocalLowFolderPath = GetKnownFolderPath(localLowId);

                var saveLocation = Path.Combine(LocalLowFolderPath, "KK Game Studio", "Freeman Guerrilla Warfare");

                if (Directory.Exists(saveLocation))
                    txtSaveLocation.Text = saveLocation;
            }
        }

        private string GetKnownFolderPath(Guid knownFolderId)
        {
            IntPtr pszPath = IntPtr.Zero;
            try
            {
                int hr = SHGetKnownFolderPath(knownFolderId, 0, IntPtr.Zero, out pszPath);
                if (hr >= 0)
                    return Marshal.PtrToStringAuto(pszPath);
                throw Marshal.GetExceptionForHR(hr);
            }
            finally
            {
                if (pszPath != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pszPath);
            }
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr pszPath);

        private void BtStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSaveLocation.Text) || string.IsNullOrWhiteSpace(txtGameLocation.Text)) return;

            menuSaveList.Items.Clear();
            foreach (var directory in Directory.GetDirectories(txtSaveLocation.Text))
            {
                if (Directory.GetFiles(directory, "MapAgent.json", SearchOption.AllDirectories).Count() > 0)
                {
                    var folder = new ToolStripMenuItem(Path.GetFileName(directory).Split('_')[0])
                    {
                        Tag = directory
                    };

                    menuSaveList.Items.Add(folder);

                    foreach (var save in Directory.GetFiles(directory, "MapAgent.json", SearchOption.AllDirectories))
                    {
                        folder.DropDownItems.Add(new ToolStripMenuItem(Path.GetFileName(Path.GetDirectoryName(save)).Split('_')[0], null, OnClick)
                        {
                            Tag = Path.GetDirectoryName(save)
                        });
                    }
                }
            }
            menuSaveList.Show(btStart, new System.Drawing.Point(0, btStart.Height));
        }

        private void OnClick(object sender, EventArgs e)
        {
            loaded = false;
            Program.CurrentPath = (sender as ToolStripMenuItem).Tag.ToString();
            SetDataSources(Program.CurrentPath);

            RefreshDescriptions();

            SetTabMain();
            loaded = true;
            tabMenu.SelectedIndex = 1;
        }

        private void SetDataSources(string path)
        {
            Program.CurrentPlayer = Player.FromJson(File.ReadAllText(Directory.GetFiles(path, "MapAgent.json", SearchOption.AllDirectories).First()));

            Squads.Insert(0, new Squad()
            {
                Name = "-Unassigned Units-",
                guid = new Guid(),
                Soldiers = Program.CurrentPlayer.The0_MapAgent.UnassignedUnits.UnassignedSoldiers
            });

            Program.CurrentHeroes = Hero.FromJson(File.ReadAllText(Directory.GetFiles(path, "HeroAgent.json", SearchOption.AllDirectories).First()));
            Program.ShirtList = Cloth.FromJson(File.ReadAllText(Directory.GetFiles(Settings.Default.GameLocation, "Clothes.json", SearchOption.AllDirectories).First())).Values.ToList();
            Program.WeaponList = Weapon.FromJson(File.ReadAllText(Directory.GetFiles(Settings.Default.GameLocation, "Weapon.json", SearchOption.AllDirectories).First())).Values.ToList();
            Program.ItemList = Item.FromJson(File.ReadAllText(Directory.GetFiles(Settings.Default.GameLocation, "Item.json", SearchOption.AllDirectories).First())).Values.ToList();
            Program.LocalizationList = Localization.FromJson(File.ReadAllText(Directory.GetFiles(Settings.Default.GameLocation, "Localization.json", SearchOption.AllDirectories).First()));
            Program.SolderList = Soldier.FromJson(File.ReadAllText(Directory.GetFiles(Settings.Default.GameLocation, "Soldier.json", SearchOption.AllDirectories).First())).Values.ToList();
            Program.DesignChart = DesignChart.FromJson(File.ReadAllText(Directory.GetFiles(Settings.Default.GameLocation, "DesignChart.json", SearchOption.AllDirectories).First())).Values.ToList();

            foreach (var design in Program.DesignChart)
            {
                var suffix = "-" + design.DesignType1.ToString()[0] + design.DesignType2.ToString()[0];

                var baseWeapon = Program.WeaponList.Where(x => x.Id == design.WeaponId).First().DeepCopy();
                var baseWeaponAsItem = Program.ItemList.Where(x => x.Id == design.WeaponId).First();

                var modWeaponAsItem = Program.ItemList.Where(x => x.Id == design.Id).First();

                modWeaponAsItem.Cost = baseWeaponAsItem.Cost + design.AddPrice;

                //baseWeaponAsItem.Name += suffix;
                //baseWeaponAsItem.Description = baseWeapon.Name + " Black Market Variant";
                //baseWeaponAsItem.Id = design.Id;
                baseWeapon.Id = design.Id;
                baseWeapon.Name += suffix;

                ModifyWeapon(baseWeapon, design.DesignType1, design.AddValue1);
                ModifyWeapon(baseWeapon, design.DesignType2, design.AddValue2);

                Program.WeaponList.Add(baseWeapon);
                //Program.ItemList.Add(baseWeaponAsItem);
            }

            Program.CurrentHelmets = (from i in Program.ItemList
                                      join c in Program.ShirtList on i.Id equals c.Id
                                      where i.SlotType == SlotType.Helmet
                                      select new
                                      {
                                          i.Id,
                                          i.Name,
                                          //  Description = GetDescription(i.Description),
                                          i.Cost,
                                          c.ArmorValue,
                                          DesertCamo = c.CamouflageType == QuickType.CamouflageType.DesertCamouflage ? c.Camouflage : 0,
                                          JungleCamo = c.CamouflageType == QuickType.CamouflageType.JungleCamouflage ? c.Camouflage : 0,
                                          NightCamo = c.CamouflageType == QuickType.CamouflageType.NightCamouflage ? c.Camouflage : 0,
                                          SnowCamo = c.CamouflageType == QuickType.CamouflageType.SnowfieldCamouflage ? c.Camouflage : 0
                                      });

            Program.CurrentMasks = (from i in Program.ItemList
                                    join c in Program.ShirtList on i.Id equals c.Id
                                    where i.SlotType == SlotType.Mask
                                    select new
                                    {
                                        i.Id,
                                        i.Name,
                                        //  Description = GetDescription(i.Description),
                                        i.Cost,
                                        c.ArmorValue,
                                        DesertCamo = c.CamouflageType == QuickType.CamouflageType.DesertCamouflage ? c.Camouflage : 0,
                                        JungleCamo = c.CamouflageType == QuickType.CamouflageType.JungleCamouflage ? c.Camouflage : 0,
                                        NightCamo = c.CamouflageType == QuickType.CamouflageType.NightCamouflage ? c.Camouflage : 0,
                                        SnowCamo = c.CamouflageType == QuickType.CamouflageType.SnowfieldCamouflage ? c.Camouflage : 0
                                    });

            Program.CurrentArmors = (from i in Program.ItemList
                                     join c in Program.ShirtList on i.Id equals c.Id
                                     where i.SlotType == SlotType.Armor
                                     select new
                                     {
                                         i.Id,
                                         i.Name,
                                         //   Description = GetDescription(i.Description),
                                         i.Cost,
                                         c.ArmorValue,
                                         DesertCamo = c.CamouflageType == QuickType.CamouflageType.DesertCamouflage ? c.Camouflage : 0,
                                         JungleCamo = c.CamouflageType == QuickType.CamouflageType.JungleCamouflage ? c.Camouflage : 0,
                                         NightCamo = c.CamouflageType == QuickType.CamouflageType.NightCamouflage ? c.Camouflage : 0,
                                         SnowCamo = c.CamouflageType == QuickType.CamouflageType.SnowfieldCamouflage ? c.Camouflage : 0
                                     });
            Program.CurrentShirts = (from i in Program.ItemList
                                     join c in Program.ShirtList on i.Id equals c.Id
                                     where i.SlotType == SlotType.Shirt
                                     select new
                                     {
                                         i.Id,
                                         i.Name,
                                         //   Description = GetDescription(i.Description),
                                         i.Cost,
                                         c.ArmorValue,
                                         DesertCamo = c.CamouflageType == QuickType.CamouflageType.DesertCamouflage ? c.Camouflage : 0,
                                         JungleCamo = c.CamouflageType == QuickType.CamouflageType.JungleCamouflage ? c.Camouflage : 0,
                                         NightCamo = c.CamouflageType == QuickType.CamouflageType.NightCamouflage ? c.Camouflage : 0,
                                         SnowCamo = c.CamouflageType == QuickType.CamouflageType.SnowfieldCamouflage ? c.Camouflage : 0
                                     });

            Program.CurrentPants = (from i in Program.ItemList
                                    join c in Program.ShirtList on i.Id equals c.Id
                                    where i.SlotType == SlotType.Pants
                                    select new
                                    {
                                        i.Id,
                                        i.Name,
                                        // Description = GetDescription(i.Description),
                                        i.Cost,
                                        c.ArmorValue,
                                        DesertCamo = c.CamouflageType == QuickType.CamouflageType.DesertCamouflage ? c.Camouflage : 0,
                                        JungleCamo = c.CamouflageType == QuickType.CamouflageType.JungleCamouflage ? c.Camouflage : 0,
                                        NightCamo = c.CamouflageType == QuickType.CamouflageType.NightCamouflage ? c.Camouflage : 0,
                                        SnowCamo = c.CamouflageType == QuickType.CamouflageType.SnowfieldCamouflage ? c.Camouflage : 0
                                    });

            Program.CurrentPistols = (from i in Program.ItemList
                                      join c in Program.WeaponList on i.Id equals c.Id
                                      where i.SlotType == SlotType.Pistol
                                      select new
                                      {
                                          i.Id,
                                          i.Name,
                                          // Description = GetDescription(i.Description),
                                          i.Cost,
                                          c.Damage,
                                          c.BulletSpeed,
                                          c.MagSize,
                                          c.Interval,
                                          c.MarksmanshipRequirement,
                                          c.BurstAmount,
                                          c.ShotNum,
                                          c.Spread,
                                          c.Volume
                                      });

            Program.CurrentWeapons = (from i in Program.ItemList
                                      join c in Program.WeaponList on i.Id equals c.Id
                                      where i.SlotType == SlotType.Rifle
                                      select new
                                      {
                                          i.Id,
                                          i.Name,
                                          //Description = GetDescription(i.Description),
                                          i.Cost,
                                          c.WeaponType,
                                          c.Damage,
                                          c.BulletSpeed,
                                          c.MagSize,
                                          Ammo = GetDescription(c.AmmoType),
                                          c.Interval,
                                          MarksmanReq = c.MarksmanshipRequirement,
                                          c.Spread
                                      }).OrderBy(x => x.Name).ToList();

            Program.CurrentScopes = (from i in Program.ItemList

                                     where i.SlotType == SlotType.Riflescope
                                     select new
                                     {
                                         i.Id,
                                         i.Name,
                                         Description = GetDescription(i.Description),
                                         i.Cost
                                     });

            Program.CurrentItems = (from i in Program.ItemList

                                    select new
                                    {
                                        i.Id,
                                        i.Name,
                                        Description = GetDescription(i.Description),
                                        i.Cost
                                    });

            Program.CurrentMisc = Program.ItemList.Where(x => x.SlotType == SlotType.Misc)

                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    Description = GetDescription(x.Description),
                    x.Cost
                });

            Program.CurrentInventory = new List<InventoryRow>();

            for (int i = 0; i < Program.CurrentPlayer.Items.Count;)
            {
                var newRow = InventoryRow.New(Program.CurrentPlayer.Items, ref i);
                Program.CurrentInventory.Add(newRow);
            }

            cbSquads.DataSource = Squads;
            dgvInventory.AutoGenerateColumns = false;
            dgvInventory.DataSource = Program.CurrentInventory;
            SetPartySize();
        }

        private void ModifyWeapon(Weapon baseWeapon, DesignType designType, decimal addValue)
        {
            switch (designType)
            {
                case DesignType.Accuracy:
                    baseWeapon.Inaccuracy -= addValue;
                    break;

                case DesignType.Ammotype:
                    baseWeapon.AmmoType = (long)addValue;
                    break;

                case DesignType.Bulletspeed:
                    baseWeapon.BulletSpeed += (long)addValue;
                    break;

                case DesignType.Damage:
                    baseWeapon.Damage += (long)addValue;
                    break;

                case DesignType.Empty:
                    break;

                case DesignType.Firerate:
                    baseWeapon.Interval += addValue;
                    break;

                case DesignType.Magsize:
                    baseWeapon.MagSize += (long)addValue;
                    baseWeapon.PerReload += (long)addValue;
                    break;

                case DesignType.Shotnum:
                    baseWeapon.ShotNum += (long)addValue;
                    break;

                default:
                    break;
            }
        }

        private void SetPartySize()
        {
            lbPartySize.Text = string.Format("Party Size: {0}/{1}",
            Squads.Select(x => x.Soldiers.Count).Sum(),
            Program.CurrentPlayer.Level + 10 + (Program.CurrentPlayer.Leadership * 8));
            SetSquadSize();
        }

        private void SetSquadSize()
        {
            lbSquadSize.Text = string.Format("Squad Size: {0}", (cbSquads.SelectedItem as Squad).Soldiers.Count);
        }

        private object GetDescription(string description)
        {
            if (!Program.LocalizationList.ContainsKey(description)) return description;

            return Program.LocalizationList[description].English;
        }

        private bool loaded;

        private void SetTabMain()
        {
            txtagility.Text = Program.CurrentPlayer.Agility.ToString();
            txtarmorPoint.Text = Program.CurrentPlayer.ArmorPoint.ToString();
            txtassaultRiflePoint.Text = Program.CurrentPlayer.AssaultRiflePoint.ToString();
            txtattributePoints.Text = Program.CurrentPlayer.AttributePoints.ToString();
            txtbattlesLose.Text = Program.CurrentPlayer.BattlesLose.ToString();
            txtbattlesWon.Text = Program.CurrentPlayer.BattlesWon.ToString();
            txtCharacterName.Text = Program.CurrentPlayer.CharacterName.ToString();
            txtconstitution.Text = Program.CurrentPlayer.Constitution.ToString();
            txtcredits.Text = Program.CurrentPlayer.Credits.ToString();
            txtExp.Text = Program.CurrentPlayer.CurExp.ToString();
            txtintelligence.Text = Program.CurrentPlayer.Intelligence.ToString();
            txtlauncherPoint.Text = Program.CurrentPlayer.LauncherPoint.ToString();
            txtleadership.Text = Program.CurrentPlayer.Leadership.ToString();
            txtlevel.Text = Program.CurrentPlayer.Level.ToString();
            txtmachineGunPoint.Text = Program.CurrentPlayer.MachineGunPoint.ToString();
            txtmarksmanship.Text = Program.CurrentPlayer.Marksmanship.ToString();
            txtpartyDeathes.Text = Program.CurrentPlayer.PartyDeathes.ToString();
            txtpartyKills.Text = Program.CurrentPlayer.PartyKills.ToString();
            txtpistolPoint.Text = Program.CurrentPlayer.PistolPoint.ToString();
            txtplayerDeathes.Text = Program.CurrentPlayer.PlayerDeathes.ToString();
            txtplayerPoints.Text = Program.CurrentPlayer.PlayerPoints.ToString();
            txtplayerTotalKills.Text = Program.CurrentPlayer.PlayerTotalKills.ToString();
            txtPositionX.Text = Program.CurrentPlayer.Position.First(x => x.X.HasValue).X.ToString();
            txtPositionY.Text = Program.CurrentPlayer.Position.First(x => x.Y.HasValue).Y.ToString();
            txtPositionZ.Text = Program.CurrentPlayer.Position.First(x => x.Z.HasValue).Z.ToString();
            txtRenownValue.Text = Program.CurrentPlayer.RenownValue.ToString();
            txtrepution.Text = Program.CurrentPlayer.Reputation.ToString();
            txtriflePoint.Text = Program.CurrentPlayer.RiflePoint.ToString();
            txtshotGunPoint.Text = Program.CurrentPlayer.ShotGunPoint.ToString();
            txtSmgPoint.Text = Program.CurrentPlayer.SmgPoint.ToString();
            txtthrowingPoint.Text = Program.CurrentPlayer.ThrowingPoint.ToString();
            txtweaponPoint.Text = Program.CurrentPlayer.WeaponPoint.ToString();

            txtcommanding.Text = Program.CurrentPlayer.Commanding.ToString();
            txtdiplomacy.Text = Program.CurrentPlayer.Diplomacy.ToString();
            txtfirstaid.Text = Program.CurrentPlayer.Firstaid.ToString();
            txtinventorymanagement.Text = Program.CurrentPlayer.Inventorymanagement.ToString();
            txtlooting.Text = Program.CurrentPlayer.Looting.ToString();
            txtprisonermanagement.Text = Program.CurrentPlayer.Prisonermanagement.ToString();
            txtmedical.Text = Program.CurrentPlayer.Medical.ToString();
            txtnavigation.Text = Program.CurrentPlayer.Navigation.ToString();
            txtstealing.Text = Program.CurrentPlayer.Stealing.ToString();
            txttrading.Text = Program.CurrentPlayer.Trading.ToString();
            txttraining.Text = Program.CurrentPlayer.Training.ToString();
        }

        private void SaveTabMain()
        {
            if (!loaded) return;

            Program.CurrentPlayer.Agility = GetInt64Value(txtagility.Text, Program.CurrentPlayer.Agility);
            Program.CurrentPlayer.ArmorPoint = GetInt64Value(txtarmorPoint.Text, Program.CurrentPlayer.ArmorPoint);
            Program.CurrentPlayer.AssaultRiflePoint = GetInt64Value(txtassaultRiflePoint.Text, Program.CurrentPlayer.AssaultRiflePoint);
            Program.CurrentPlayer.AttributePoints = GetInt64Value(txtattributePoints.Text, Program.CurrentPlayer.AttributePoints);
            Program.CurrentPlayer.BattlesLose = GetInt64Value(txtbattlesLose.Text, Program.CurrentPlayer.BattlesLose);
            Program.CurrentPlayer.BattlesWon = GetInt64Value(txtbattlesWon.Text, Program.CurrentPlayer.BattlesWon);
            Program.CurrentPlayer.CharacterName = txtCharacterName.Text;
            Program.CurrentPlayer.Constitution = GetInt64Value(txtconstitution.Text, Program.CurrentPlayer.Constitution);
            Program.CurrentPlayer.Credits = GetInt64Value(txtcredits.Text, Program.CurrentPlayer.Credits);
            Program.CurrentPlayer.CurExp = GetInt64Value(txtExp.Text, Program.CurrentPlayer.CurExp);

            Program.CurrentPlayer.Intelligence = GetInt64Value(txtintelligence.Text, Program.CurrentPlayer.Intelligence);
            Program.CurrentPlayer.LauncherPoint = GetInt64Value(txtlauncherPoint.Text, Program.CurrentPlayer.LauncherPoint);
            Program.CurrentPlayer.Leadership = GetInt64Value(txtleadership.Text, Program.CurrentPlayer.Leadership);
            Program.CurrentPlayer.Level = GetInt64Value(txtlevel.Text, Program.CurrentPlayer.Level);
            Program.CurrentPlayer.MachineGunPoint = GetInt64Value(txtmachineGunPoint.Text, Program.CurrentPlayer.MachineGunPoint);
            Program.CurrentPlayer.Marksmanship = GetInt64Value(txtmarksmanship.Text, Program.CurrentPlayer.Marksmanship);
            Program.CurrentPlayer.PartyDeathes = GetInt64Value(txtpartyDeathes.Text, Program.CurrentPlayer.PartyDeathes);
            Program.CurrentPlayer.PartyKills = GetInt64Value(txtpartyKills.Text, Program.CurrentPlayer.PartyKills);
            Program.CurrentPlayer.PistolPoint = GetInt64Value(txtpistolPoint.Text, Program.CurrentPlayer.PistolPoint);
            Program.CurrentPlayer.PlayerDeathes = GetInt64Value(txtplayerDeathes.Text, Program.CurrentPlayer.PlayerDeathes);
            Program.CurrentPlayer.PlayerPoints = GetInt64Value(txtplayerPoints.Text, Program.CurrentPlayer.PlayerPoints);
            Program.CurrentPlayer.PlayerTotalKills = GetInt64Value(txtplayerTotalKills.Text, Program.CurrentPlayer.PlayerTotalKills);
            Program.CurrentPlayer.Position.First(x => x.X.HasValue).X = Convert.ToDecimal(txtPositionX.Text);
            Program.CurrentPlayer.Position.First(x => x.X.HasValue).Y = Convert.ToDecimal(txtPositionY.Text);
            Program.CurrentPlayer.Position.First(x => x.X.HasValue).Z = Convert.ToDecimal(txtPositionZ.Text);
            Program.CurrentPlayer.RenownValue = GetInt64Value(txtRenownValue.Text, Program.CurrentPlayer.RenownValue);
            Program.CurrentPlayer.Reputation = GetInt64Value(txtrepution.Text, Program.CurrentPlayer.Reputation);
            Program.CurrentPlayer.RiflePoint = GetInt64Value(txtriflePoint.Text, Program.CurrentPlayer.RiflePoint);
            Program.CurrentPlayer.ShotGunPoint = GetInt64Value(txtshotGunPoint.Text, Program.CurrentPlayer.ShotGunPoint);
            Program.CurrentPlayer.SmgPoint = GetInt64Value(txtSmgPoint.Text, Program.CurrentPlayer.SmgPoint);
            Program.CurrentPlayer.ThrowingPoint = GetInt64Value(txtthrowingPoint.Text, Program.CurrentPlayer.ThrowingPoint);
            Program.CurrentPlayer.WeaponPoint = GetInt64Value(txtweaponPoint.Text, Program.CurrentPlayer.WeaponPoint);

            Program.CurrentPlayer.Commanding = GetInt64Value(txtcommanding.Text, Program.CurrentPlayer.Commanding);
            Program.CurrentPlayer.Diplomacy = GetInt64Value(txtdiplomacy.Text, Program.CurrentPlayer.Diplomacy);
            Program.CurrentPlayer.Firstaid = GetInt64Value(txtfirstaid.Text, Program.CurrentPlayer.Firstaid);
            Program.CurrentPlayer.Inventorymanagement = GetInt64Value(txtinventorymanagement.Text, Program.CurrentPlayer.Inventorymanagement);
            Program.CurrentPlayer.Looting = GetInt64Value(txtlooting.Text, Program.CurrentPlayer.Looting);
            Program.CurrentPlayer.Prisonermanagement = GetInt64Value(txtprisonermanagement.Text, Program.CurrentPlayer.Prisonermanagement);
            Program.CurrentPlayer.Medical = GetInt64Value(txtmedical.Text, Program.CurrentPlayer.Medical);
            Program.CurrentPlayer.Navigation = GetInt64Value(txtnavigation.Text, Program.CurrentPlayer.Navigation);
            Program.CurrentPlayer.Stealing = GetInt64Value(txtstealing.Text, Program.CurrentPlayer.Stealing);
            Program.CurrentPlayer.Trading = GetInt64Value(txttrading.Text, Program.CurrentPlayer.Trading);
            Program.CurrentPlayer.Training = GetInt64Value(txttraining.Text, Program.CurrentPlayer.Training);
        }

        private void SaveHeroes()
        {
            foreach (var squad in Squads)
            {
                foreach (var hero in squad.Soldiers.Where(x => x.IsHero == 1))
                {
                    UpdateHero(hero);
                }
            }

            foreach (var hero in Program.CurrentPlayer.The0_MapAgent.UnassignedUnits.UnassignedSoldiers.Where(x => x.IsHero == 1))
            {
                UpdateHero(hero);
            }
        }

        private void UpdateHero(Prisoner hero)
        {
            var heroAgent = Program.CurrentHeroes.HeroAgents.Where(x => x.Ssd.Count(s => s.Id == hero.Id) > 0).FirstOrDefault();

            var heroRec = heroAgent.Ssd.First(x => x.Id == hero.Id);

            heroRec.Agility = hero.Agility;
            heroRec.ArmorId = hero.ArmorId;
            heroRec.ArmorPoint = hero.ArmorPoint;
            heroRec.AssaultRiflePoint = hero.AssaultRiflePoint;
            heroRec.Constitution = hero.Constitution;
            heroRec.Exp = hero.Exp;
            heroRec.HelmetId = hero.HelmetId;
            heroRec.LauncherPoint = hero.LauncherPoint;
            heroRec.Level = hero.Level;
            heroRec.MachineGunPoint = hero.MachineGunPoint;
            heroRec.Marksmanship = hero.Marksmanship;
            heroRec.MaskId = hero.MaskId;
            heroRec.Misc1Id = hero.Misc1Id;
            heroRec.Misc2Id = hero.Misc2Id;
            heroRec.Misc3Id = hero.Misc3Id;
            heroRec.Misc4Id = hero.Misc4Id;
            heroRec.Morale = hero.Morale;
            heroRec.Name = hero.Name;
            heroRec.PantsId = hero.PantsId;
            heroRec.PistolId = hero.PistolId;
            heroRec.PistolPoint = hero.PistolPoint;
            heroRec.RifleId = hero.RifleId;
            heroRec.RiflePoint = hero.RiflePoint;
            heroRec.ShirtId = hero.ShirtId;
            heroRec.ShotGunPoint = hero.ShotGunPoint;
            heroRec.SightBonus = hero.SightBonus;
            heroRec.SmgPoint = hero.SmgPoint;
            heroRec.ThrowingPoint = hero.ThrowingPoint;
            heroRec.Will = hero.Will;
        }

        private long GetInt64Value(string text, long prevValue)
        {
            if (!Int64.TryParse(text, out _))
                return prevValue;

            return Convert.ToInt64(text);
        }

        private void TxtNumeric_KeyDown_Check(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back || char.IsDigit((char)e.KeyValue) || (e.KeyValue >= 96 && e.KeyValue <= 105))
                e.SuppressKeyPress = false;
            else
                e.SuppressKeyPress = true;
        }

        internal void RefreshDescriptions()
        {
            lbHELMET.Text = GetDescription(Program.CurrentPlayer.HelmetId);
            lbMask.Text = GetDescription(Program.CurrentPlayer.MaskId);
            lbArmor.Text = GetDescription(Program.CurrentPlayer.ArmorId);
            lbClothes.Text = GetDescription(Program.CurrentPlayer.ShirtId);
            lbPants.Text = GetDescription(Program.CurrentPlayer.PantsId);
            lbPistol.Text = GetDescription(Program.CurrentPlayer.PistolId);
            lbPistolAcc.Text = GetDescription(Program.CurrentPlayer.PistolSilencerId);
            lbWep1.Text = GetDescription(Program.CurrentPlayer.RifleId) + (Program.CurrentPlayer.RifleId > 0 ? "(" + GetDescription(Program.WeaponList.Where(x => x.Id == Program.CurrentPlayer.RifleId).First().AmmoType) + ")" : "");
            lbWep2.Text = GetDescription(Program.CurrentPlayer.LauncherId) + (Program.CurrentPlayer.LauncherId > 0 ? "(" + GetDescription(Program.WeaponList.Where(x => x.Id == Program.CurrentPlayer.LauncherId).First().AmmoType) + ")" : "");
            lbWepAcc1.Text = GetDescription(Program.CurrentPlayer.RifleScopeIdL);
            lbWepAcc2.Text = GetDescription(Program.CurrentPlayer.RifleSilencerIdL);
            lbWep2Acc1.Text = GetDescription(Program.CurrentPlayer.RifleScopeIdR);
            lbWep2Acc2.Text = GetDescription(Program.CurrentPlayer.RifleSilencerIdR);

            lbMisc1.Text = GetDescription(Program.CurrentPlayer.Misc1Id);
            lbMisc2.Text = GetDescription(Program.CurrentPlayer.Misc2Id);
            lbMisc3.Text = GetDescription(Program.CurrentPlayer.Misc3Id);
            lbMisc4.Text = GetDescription(Program.CurrentPlayer.Misc4Id);
            lbMisc5.Text = GetDescription(Program.CurrentPlayer.Misc5Id);
            lbMisc6.Text = GetDescription(Program.CurrentPlayer.Misc6Id);
            lbMisc7.Text = GetDescription(Program.CurrentPlayer.Misc7Id);
            lbMisc8.Text = GetDescription(Program.CurrentPlayer.Misc8Id);
            lbMisc9.Text = GetDescription(Program.CurrentPlayer.Misc9Id);
            lbMisc10.Text = GetDescription(Program.CurrentPlayer.Misc10Id);
            lbMisc11.Text = GetDescription(Program.CurrentPlayer.Misc11Id);
            lbMisc12.Text = GetDescription(Program.CurrentPlayer.Misc12Id);
        }

        private string GetDescription(long Id)
        {
            if (Id <= 0) return "None";

            return Program.ItemList.First(x => x.Id == Id).Name;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if ((keyData == (Keys.Control | Keys.Tab)) || (keyData == (Keys.Control | Keys.Shift | Keys.Tab)))
            {
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BtSave_Click(object sender, EventArgs e)
        {
            btSave.Enabled = false;
            SaveTabMain();
            SaveHeroes();
            SaveInventory();
            SetAllSquadRanking();

            if ((sender as Button).Name != nameof(btSaveAndContinue))
            {
                loaded = false;
                Squads.Remove(Squads.First(x => x.guid == new Guid()));
            }
            btSave.Update();

            backgroundWorker1.RunWorkerAsync((sender as Button).Name);
        }

        private void SaveInventory()
        {
            Program.CurrentPlayer.Items.Clear();

            for (int i = 0; i < Program.CurrentInventory.Count; i++)
            {
                Program.CurrentPlayer.Items.AddRange(Program.CurrentInventory[i].GetItems());
            }
        }

        private void BackgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var player = Program.CurrentPlayer.ToJson();
            var heroes = Program.CurrentHeroes.ToJson();

            ZipFile.CreateFromDirectory(Program.CurrentPath, Path.Combine(
                Directory.GetParent(Program.CurrentPath).FullName,
                Program.CurrentPlayer.CharacterName + DateTime.Now.ToString("yyyyMMddHHmmss") + ".zip"));

            File.WriteAllText(Path.Combine(Program.CurrentPath, "MapAgent.json"), player);
            File.WriteAllText(Path.Combine(Program.CurrentPath, "HeroAgent.json"), heroes);
            e.Result = e.Argument;
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                Application.DoEvents();

                if (!btSave.IsDisposed) btSave.Enabled = true;

                if (e.Result.ToString() == nameof(btSaveAndContinue))
                {
                    return;
                }

                BtCancel_Click(sender, e);
            });
        }

        private void BtRename_Click(object sender, EventArgs e)
        {
            if (cbSquads.SelectedIndex < 1) return;
            var name = (cbSquads.SelectedItem as Squad).Name;

            name = Interaction.InputBox("Enter new squad name", "Squad Name", name);

            if (!string.IsNullOrWhiteSpace(name) && name != (cbSquads.SelectedItem as Squad).Name)
            {
                (cbSquads.SelectedItem as Squad).Name = name;
                cbSquads.DataSource = null;
                cbSquads.DataSource = Squads;
            }
        }

        private void BtAddNew_Click(object sender, EventArgs e)
        {
            var name = Interaction.InputBox("Enter new squad name", "Squad Name", $"My Squad {Squads.Count + 1}");
            if (!string.IsNullOrWhiteSpace(name))
            {
                Squads.Add(new Squad()
                {
                    Name = name,
                    IconName = "Anna",
                    SquadLefts = 7,
                    FormationIndex = 0
                });
                cbSquads.DataSource = null;
                cbSquads.DataSource = Squads;
                cbSquads.SelectedIndex = Squads.Count - 1;
            }
        }

        private void BtDelete_Click(object sender, EventArgs e)
        {
            if (cbSquads.SelectedIndex < 1) return;
            DialogResult dialogResult;
            using (FrmMultiple frmMultiple = new FrmMultiple("Confirmation", $"Confirm To Delete Squad \"{cbSquads.Text}\"", "Delete", "Cancel", "Delete But Keep Units"))
                if ((dialogResult = frmMultiple.ShowDialog(this)) != DialogResult.Cancel)
                {
                    var squadToRemove = cbSquads.SelectedItem as Squad;

                    if (dialogResult == DialogResult.No)
                    {
                        foreach (var unit in squadToRemove.Soldiers)
                        {
                            Squads.First(x => x.guid == new Guid()).Soldiers.Add(unit);
                        }
                    }

                    Squads.Remove(squadToRemove);
                    cbSquads.DataSource = null;
                    cbSquads.DataSource = Squads;
                    dgvSquadEquips.DataSource = null;
                    dgvSquadStats.DataSource = null;
                    BtEdit_Click(sender, e);
                }
        }

        private void Txtlevel_TextChanged(object sender, EventArgs e)
        {
            SaveTabMain();
            SetPartySize();
        }

        private void Txtleadership_TextChanged(object sender, EventArgs e)
        {
            SaveTabMain();
            SetPartySize();
        }

        private void BtEdit_Click(object sender, EventArgs e)
        {
            if (!loaded || cbSquads.SelectedIndex < 0) return;
            var squad = Squads.Where(x => x.guid == (cbSquads.SelectedItem as Squad).guid).First();

            dgvSquadEquips.DataSource = squad.Soldiers;
            dgvSquadStats.DataSource = squad.Soldiers;
            SetPartySize();
            panSquad.Visible = true;
        }

        private void DgvSquadStats_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            Int64.TryParse(e.FormattedValue.ToString(), out long value);

            if (dgvSquadStats.Columns[e.ColumnIndex].DataPropertyName == "Morale" && (value < 0 || value > 100))
            {
                MessageBox.Show("Morale Must Be Between 0-100", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
            }

            if (dgvSquadStats.Columns[e.ColumnIndex].DataPropertyName == "Marksmanship" && (value < 0 || value > 15))
            {
                MessageBox.Show("Marksmanship Must Be Between 0-15", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
            }

            if (dgvSquadStats.Columns[e.ColumnIndex].DataPropertyName.EndsWith("Point") && (value < 0 || value > 100))
            {
                MessageBox.Show(dgvSquadStats.Columns[e.ColumnIndex].DataPropertyName.Replace("Point", "") + " Must Be Between 0-100", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                e.Cancel = true;
            }

            if (e.FormattedValue.ToString().Contains("-"))
                e.Cancel = true;
        }

        private void BtAddSoldier_Click(object sender, EventArgs e)
        {
            CreateSoldierMenu(OnSoldierClick);
            menuSoldierList.Show(btAddSoldier, new System.Drawing.Point(0, btAddSoldier.Height));
        }

        private void BtEditSoldier_Click(object sender, EventArgs e)
        {
            CreateSoldierMenu(OnSoldierReplace);
            menuSoldierList.Show(btEditSoldier, new System.Drawing.Point(0, btEditSoldier.Height));
        }

        private void CreateSoldierMenu(EventHandler onSoldierClick)
        {
            menuSoldierList.Items.Clear();

            var regular = new ToolStripMenuItem("Regular Troops");
            var faction = new ToolStripMenuItem("Faction Troops");
            var special = new ToolStripMenuItem("Special Troops");
            var heroes = new ToolStripMenuItem("Heroes");

            menuSoldierList.Items.Add(regular);
            menuSoldierList.Items.Add(faction);
            menuSoldierList.Items.Add(special);
            menuSoldierList.Items.Add(heroes);

            Program.SolderList.OrderBy(x => x.Name).ToList().ForEach(x =>
            {
                if (x.Id < 1000)
                {
                    if (x.Name.StartsWith("FCA ") || x.Name.StartsWith("Federal ") || x.Name.StartsWith("VFA ")
                         || x.Name.StartsWith("Pozna ") || x.Name.StartsWith("CFR ") || x.Name.StartsWith("Uman ") || x.Name.StartsWith("ALPHA "))
                        faction.DropDownItems.Add(new ToolStripMenuItem(x.Name, null, onSoldierClick)
                        {
                            Tag = x.Id
                        });
                    else
                        regular.DropDownItems.Add(new ToolStripMenuItem(x.Name, null, onSoldierClick)
                        {
                            Tag = x.Id
                        });
                }
                else if (x.Id < 10000)
                    special.DropDownItems.Add(new ToolStripMenuItem(x.Name, null, onSoldierClick)
                    {
                        Tag = x.Id
                    });
                else
                    heroes.DropDownItems.Add(new ToolStripMenuItem(x.Name, null, onSoldierClick)
                    {
                        Tag = x.Id
                    });
            });
        }

        private void OnSoldierClick(object sender, EventArgs e)
        {
            Soldier soldier = Program.SolderList.Where(x => x.Id == Convert.ToInt64((sender as ToolStripMenuItem).Tag)).First();
            Prisoner newSoldier = new Prisoner()
            {
                Id = soldier.Id,
                Name = soldier.Name,
                Sex = soldier.Sex,
                IconName = soldier.Icon,
                Exp = 0,
                Health = soldier.Health,
                Level = 1,
                SquadUnitIndex = dgvSquadStats.Rows.Count,
                Morale = 100,
                Cost = soldier.Cost,
                SightBonus = soldier.SightBonus,
                NpcTag = NpcTag.Untagged,
                SmgPoint = soldier.SmgPoint,
                AssaultRiflePoint = soldier.AssaultRiflePoint,
                RiflePoint = soldier.RiflePoint,
                MachineGunPoint = soldier.MachineGunPoint,
                PistolPoint = soldier.PistolPoint,
                Marksmanship = soldier.Marksmanship,
                HeadName = soldier.HeadName,
                FaceTextureName = soldier.FaceTextureName,
                HairName = soldier.HairName,
                EyeName = soldier.EyeName,
                HairColorIndex = soldier.HairColorIndex,
                ArmorPoint = soldier.ArmorPoint,
                ShotGunPoint = soldier.ShotGunPoint,
                LauncherPoint = soldier.LauncherPoint,
                ThrowingPoint = soldier.ThrowingPoint,
                HeroId = soldier.Id < 10000 ? -1 : soldier.Id - 10000,
                IsHero = soldier.Id > 9999 ? 1 : 0,
                FactionId = 1
            };

            (dgvSquadStats.DataSource as SortableBindingList<Prisoner>).Add(newSoldier);
        }

        private void OnSoldierReplace(object sender, EventArgs e)
        {
            Soldier soldier = Program.SolderList.Where(x => x.Id == Convert.ToInt64((sender as ToolStripMenuItem).Tag)).First();

            var units = SelectedUnits;

            if (units == null || units.Count == 0) return;

            foreach (var unit in units)
            {
                unit.Id = soldier.Id;
                unit.Name = soldier.Name;
                unit.Sex = soldier.Sex;
                unit.IconName = soldier.Icon;
                unit.Exp = 0;
                unit.Health = soldier.Health;
                unit.Level = 1;
                unit.SquadUnitIndex = dgvSquadStats.Rows.Count;
                unit.Morale = 100;
                unit.Cost = soldier.Cost;
                unit.SightBonus = soldier.SightBonus;
                unit.SmgPoint = soldier.SmgPoint;
                unit.AssaultRiflePoint = soldier.AssaultRiflePoint;
                unit.RiflePoint = soldier.RiflePoint;
                unit.MachineGunPoint = soldier.MachineGunPoint;
                unit.PistolPoint = soldier.PistolPoint;
                unit.Marksmanship = soldier.Marksmanship;
                unit.HeadName = soldier.HeadName;
                unit.FaceTextureName = soldier.FaceTextureName;
                unit.HairName = soldier.HairName;
                unit.EyeName = soldier.EyeName;
                unit.HairColorIndex = soldier.HairColorIndex;
                unit.ArmorPoint = soldier.ArmorPoint;
                unit.ShotGunPoint = soldier.ShotGunPoint;
                unit.LauncherPoint = soldier.LauncherPoint;
                unit.ThrowingPoint = soldier.ThrowingPoint;
                unit.HeroId = soldier.Id < 10000 ? -1 : soldier.Id - 10000;
                unit.IsHero = soldier.Id > 9999 ? 1 : 0;
            }

            dgvSquadEquips.Refresh();
            dgvSquadStats.Refresh();
        }

        private void BtSetMaxSkill_Click(object sender, EventArgs e)
        {
            txtplayerPoints.Text = "0";
            txtstealing.Text =
            txtdiplomacy.Text =
            txttrading.Text =
            txtfirstaid.Text =
            txtmedical.Text =
            txtlooting.Text =
            txtnavigation.Text =
            txtinventorymanagement.Text =
            txttraining.Text =
            txtprisonermanagement.Text =
            txtcommanding.Text = "5";
        }

        private void BtSetWeaponMax_Click(object sender, EventArgs e)
        {
            txtweaponPoint.Text = "0";

            txtarmorPoint.Text =
            txtassaultRiflePoint.Text =
            txtlauncherPoint.Text =
            txtmachineGunPoint.Text =
            txtpistolPoint.Text =
            txtriflePoint.Text =
            txtshotGunPoint.Text =
            txtSmgPoint.Text =
            txtthrowingPoint.Text = "100";
        }

        private void DgvSquadEquips_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvSquadEquips.Columns[e.ColumnIndex].DataPropertyName.EndsWith("Id"))
            {
                var value = Program.ItemList.Where(x => x.Id.ToString() == e.Value.ToString()).FirstOrDefault();

                if (value != null)
                {
                    e.Value = value.Name;
                }
                else
                {
                    e.Value = "None";
                }
                e.FormattingApplied = true;
            }
        }

        private void DgvSquadEquips_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvSquadEquips.Columns[e.ColumnIndex].DataPropertyName.EndsWith("Id"))
            {
                switch (dgvSquadEquips.Columns[e.ColumnIndex].DataPropertyName)
                {
                    case "HelmetId":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Helmet, e.RowIndex, e.ColumnIndex);
                        return;

                    case "MaskId":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Mask, e.RowIndex, e.ColumnIndex);
                        return;

                    case "ArmorId":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Armor, e.RowIndex, e.ColumnIndex);
                        return;

                    case "ShirtId":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Shirt, e.RowIndex, e.ColumnIndex);
                        return;

                    case "PantsId":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Pants, e.RowIndex, e.ColumnIndex);
                        return;

                    case "PistolId":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Pistol, e.RowIndex, e.ColumnIndex);
                        return;

                    case "RifleId":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Weapon, e.RowIndex, e.ColumnIndex);
                        return;

                    case "MISC1Id":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc11, e.RowIndex, e.ColumnIndex);
                        return;

                    case "MISC2Id":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc2, e.RowIndex, e.ColumnIndex);
                        return;

                    case "MISC3Id":
                        ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc3, e.RowIndex, e.ColumnIndex);
                        return;

                    default:
                        break;
                }
            }
        }

        private void BtToReserve_Click(object sender, EventArgs e)
        {
            foreach (Prisoner soldier in SelectedUnits)
            {
                ActiveSquadData.Remove(soldier);

                var unassignedUnits = Squads.Where(x => x.guid == new Guid()).First();
                unassignedUnits.Soldiers.Add(soldier);
            }
        }

        private void BtDeleteSoldier_Click(object sender, EventArgs e)
        {
            string soldierList = "\n";

            if (SelectedUnits.Count == 0) return;
            if (SelectedUnits.Count == 1) soldierList = SelectedUnits[0].Name;
            else SelectedUnits.ForEach(x => soldierList += x.Name + "\n");

            if (MessageBox.Show($"Confirm To Delete {soldierList} From \"{cbSquads.Text}\" Squad", "Confirmation",
                 MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                foreach (Prisoner soldier in SelectedUnits)
                {
                    ActiveSquadData.Remove(soldier);
                }
        }

        private void GiveExpToSquad_Click(object sender, EventArgs e)
        {
            string expValue = Interaction.InputBox("Enter Experience Point Amount To Give To All Squad Units (Additive, Max 1M) ", "Give Experience ", "0");

            if (!string.IsNullOrWhiteSpace(expValue) && Int64.TryParse(expValue, out long expTotal))
            {
                foreach (var unit in SelectedUnits)
                {
                    if (unit.Level == 15) unit.Exp = 0;
                    else
                        unit.Exp = Math.Min(unit.Exp + expTotal, 1000000);
                }
                dgvSquadStats.Refresh();
            }
        }

        private void GiveExpToAll_Click(object sender, EventArgs e)
        {
            string expValue = Interaction.InputBox("Enter Experience Point Amount To Give To All Squad Units (Additive, Max 1M) ", "Give Experience ", "0");

            if (!string.IsNullOrWhiteSpace(expValue) && Int64.TryParse(expValue, out long expTotal))
            {
                foreach (var squad in Squads)
                {
                    foreach (var soldier in squad.Soldiers)
                    {
                        if (soldier.Level == 15) soldier.Exp = 0;
                        else
                            soldier.Exp = Math.Min(soldier.Exp + expTotal, 1000000);
                    }
                }
                dgvSquadStats.Refresh();
            }
        }

        private DataGridView ActiveSquadGrid
        {
            get
            {
                switch (tabsSquadDetails.SelectedIndex)
                {
                    case 0:
                        return dgvSquadStats;

                    case 1:
                        return
                            dgvSquadEquips;
                    default:
                        return null;
                }
            }
        }

        private SortableBindingList<Prisoner> ActiveSquadData => ActiveSquadGrid.DataSource as SortableBindingList<Prisoner>;

        private List<Prisoner> SelectedUnits
        {
            get
            {
                List<Prisoner> selectedUnits = new List<Prisoner>();

                foreach (DataGridViewCell cell in ActiveSquadGrid.SelectedCells)
                {
                    var unit = cell.OwningRow.DataBoundItem as Prisoner;
                    if (!selectedUnits.Contains(unit))
                        selectedUnits.Add(unit);
                }

                return selectedUnits;
            }
        }

        private List<DataGridViewRow> SelectedRows
        {
            get
            {
                List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();

                foreach (DataGridViewCell cell in ActiveSquadGrid.SelectedCells)
                {
                    if (!selectedRows.Contains(cell.OwningRow))
                        selectedRows.Add(cell.OwningRow);
                }

                return selectedRows;
            }
        }

        private SortableBindingList<Squad> Squads
        {
            get
            {
                return Program.CurrentPlayer.The0_MapAgent.Squads;
            }
        }

        private Prisoner copiedUnit;

        private void MenuCopyUnit_Click(object sender, EventArgs e)
        {
            if (SelectedUnits.Count > 0)
                copiedUnit = ActiveSquadGrid.SelectedCells.OfType<DataGridViewCell>().Last().OwningRow.DataBoundItem as Prisoner;
        }

        private void MenuMoveUnit_DropDownOpening(object sender, EventArgs e)
        {
            if (SelectedUnits.Count > 0)
            {
                menuMoveUnit.DropDownItems.Clear();

                foreach (var squad in Squads.Where(x => (x.guid == new Guid() || x.Soldiers.Count < 7) && x.guid != (cbSquads.SelectedItem as Squad).guid))
                {
                    menuMoveUnit.DropDownItems.Add(new ToolStripMenuItem(squad.Name + (squad.guid == new Guid() ? "" : (" (" + squad.Soldiers.Count.ToString() + " / 7)")), null, SquadMoveClick)
                    {
                        Tag = squad.guid
                    });
                }
            }
            //menuSquadStats.HideImageMargins();
        }

        private void SquadMoveClick(object sender, EventArgs e)
        {
            var targetSquad = Squads.Where(x => x.guid == (Guid)(sender as ToolStripMenuItem).Tag).First();
            var sourceSquad = cbSquads.SelectedItem as Squad;

            if (SelectedUnits.Count + targetSquad.Soldiers.Count > 7 && targetSquad.guid != new Guid())
            {
                MessageBox.Show($"Target Squad Doesn't Have Space For {SelectedUnits.Count} Units.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            foreach (var unitToMove in SelectedUnits)
            {
                MoveUnit(sourceSquad, targetSquad, unitToMove);
            }
        }

        private void MoveUnit(Squad squadFrom, Squad squadTo, Prisoner unitToMove)
        {
            squadFrom.Soldiers.Remove(unitToMove);
            squadTo.Soldiers.Add(unitToMove);
        }

        private void MenuPasteUnit_Click(object sender, EventArgs e)
        {
            if (ActiveSquadData.Count >= 7 && (cbSquads.SelectedItem as Squad).guid != new Guid())
            {
                _ = MessageBox.Show("Squad is full! (7/7)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            ActiveSquadData.Add(copiedUnit.DeepCopy());
        }

        private void SetAllSquadRanking()
        {
            foreach (var squad in Squads)
            {
                for (int i = 0; i < squad.Soldiers.Count; i++)
                {
                    squad.Soldiers[i].SquadUnitIndex = squad.guid == new Guid() ? 0 : i;
                }
            }
        }

        private void Tabs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabs.SelectedIndex == 2)
                BtEdit_Click(sender, e);
        }

        private void BtMaxMorale_Click(object sender, EventArgs e)
        {
            foreach (var squad in Squads)
            {
                foreach (Prisoner unit in squad.Soldiers)
                {
                    unit.Morale = 100;
                }
            }
            dgvSquadStats.Refresh();
        }

        private void DgvSquadEquips_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            SetPartySize();
        }

        private void DgvSquadEquips_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex > -1)
            {
                if (dgvSquadEquips.SelectedCells.Count <= 1)
                {
                    if (dgvSquadEquips.SelectedCells.Count == 1)
                        dgvSquadEquips.SelectedCells[0].Selected = false;
                    dgvSquadEquips.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
                }
            }
        }

        private void DgvSquadStats_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && e.RowIndex > -1)
            {
                if (dgvSquadStats.SelectedCells.Count <= 1)
                {
                    if (dgvSquadStats.SelectedCells.Count == 1)
                        dgvSquadStats.SelectedCells[0].Selected = false;
                    dgvSquadStats.Rows[e.RowIndex].Cells[e.ColumnIndex].Selected = true;
                }
            }
        }

        private void GiveHelmetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Helmet, -99, dgvSquadEquips.Columns["colHelmetId"].Index);
        }

        private void GiveMaskGooglesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Mask, -99, dgvSquadEquips.Columns["colMaskId"].Index);
        }

        private void GiveArmorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Armor, -99, dgvSquadEquips.Columns["colArmorId"].Index);
        }

        private void GiveShirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Shirt, -99, dgvSquadEquips.Columns["colShirtId"].Index);
        }

        private void GivePantsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Pants, -99, dgvSquadEquips.Columns["colPantsId"].Index);
        }

        private void GiveWeaponToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Weapon, -99, dgvSquadEquips.Columns["colRifleId"].Index);
        }

        private void GivePistolToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Pistol, -99, dgvSquadEquips.Columns["colPistolId"].Index);
        }

        private void GiveItem1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc, -99, dgvSquadEquips.Columns["colMISC1Id"].Index);
        }

        private void GiveItem2SlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc, -99, dgvSquadEquips.Columns["colMISC2Id"].Index);
        }

        private void GiveItem3SlotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSquadEquipSelection(FrmEquipSelection.EquipSelectionMode.Misc, -99, dgvSquadEquips.Columns["colMISC3Id"].Index);
        }

        private void BtDuplicate_Click(object sender, EventArgs e)
        {
            if (cbSquads.SelectedIndex < 1) return;
            var squad = (cbSquads.SelectedItem as Squad);

            var name = squad.Name;

            name = Interaction.InputBox("Enter new squad name", "Squad Name", name + " (1)");

            if (!string.IsNullOrWhiteSpace(name))
            {
                Squad newSquad = squad.DeepCopy();

                newSquad.Name = name;
                newSquad.guid = Guid.NewGuid();
                Squads.Add(newSquad);
                cbSquads.DataSource = null;
                cbSquads.DataSource = Squads;
                cbSquads.SelectedIndex = cbSquads.Items.Count - 1;
            }
        }

        private void MenuSaveSetup_Click(object sender, EventArgs e)
        {
            if (dgvSquadEquips.SelectedCells.Count == 0) return;
            var setupOwner = dgvSquadEquips.SelectedCells.OfType<DataGridViewCell>().Last().OwningRow.DataBoundItem as Prisoner;

            var name = Interaction.InputBox("Enter new setup name", "Setup Name", "My Setup");

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (Settings.Default.SavedSetups == null)
                    Settings.Default.SavedSetups = new System.Collections.Specialized.StringCollection();
                Settings.Default.SavedSetups.Add(name + "||" + JsonConvert.SerializeObject(setupOwner));
                Settings.Default.Save();
            }
        }

        private void MenuUseSetup_DropDownOpening(object sender, EventArgs e)
        {
            menuUseSetup.DropDownItems.Clear();
            if (Settings.Default.SavedSetups == null) return;

            foreach (var setup in Settings.Default.SavedSetups)
            {
                var setupName = setup.Split(new string[] { "||" }, 2, StringSplitOptions.RemoveEmptyEntries)[0];
                var setupUnit = setup.Split(new string[] { "||" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];

                menuUseSetup.DropDownItems.Add(new ToolStripMenuItem(setupName, null, OnSetupClicked) { Tag = setupUnit });
            }
        }

        private void OnSetupClicked(object sender, EventArgs e)
        {
            var setup = JsonConvert.DeserializeObject<Prisoner>((sender as ToolStripMenuItem).Tag.ToString());

            foreach (var unit in SelectedUnits)
            {
                unit.ArmorId = setup.ArmorId;
                unit.HelmetId = setup.HelmetId;
                unit.MaskId = setup.MaskId;
                unit.Misc1Id = setup.Misc1Id;
                unit.Misc2Id = setup.Misc2Id;
                unit.Misc3Id = setup.Misc3Id;
                unit.Misc4Id = setup.Misc4Id;
                unit.PantsId = setup.PantsId;
                unit.PistolId = setup.PistolId;
                unit.RifleId = setup.RifleId;
                unit.ShirtId = setup.ShirtId;
            }
            dgvSquadEquips.Refresh();
        }

        private void BtCancel_Click(object sender, EventArgs e)
        {
            tabs.SelectedIndex = 0;
            tabsSquadDetails.SelectedIndex = 0;
            panSquad.Visible = false;
            tabsMain.SelectedIndex = 0;
            tabMenu.SelectedIndex = 0;
        }

        private void DgvInventory_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1 && e.ColumnIndex > -1)
            {
                if (ModifierKeys == Keys.Shift)
                {
                    dgvInventory.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = 0;
                }
                else
                {
                    ShowInventorySelection(false, e.RowIndex, e.ColumnIndex);
                }
            }
        }

        private void DgvInventory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex > -1 && e.ColumnIndex > -1)
            {
                if (e.Value == null)
                {
                    e.Value = "Invalid";
                    e.FormattingApplied = true;
                    return;
                }

                var value = Program.ItemList.Where(x => x.Id.ToString() == e.Value.ToString()).FirstOrDefault();

                if (value != null)
                {
                    e.Value = value.Name;
                    e.CellStyle.BackColor = Color.FromArgb(220, 255, 220);
                }
                else
                {
                    e.Value = "None";
                    e.CellStyle.BackColor = Color.White;
                }
                e.FormattingApplied = true;
            }
        }

        private void MenuAddItem_Click(object sender, EventArgs e)
        {
            var amountStr = Interaction.InputBox("Enter amount to insert", "Item Amount To Insert?", "1");

            if (!string.IsNullOrWhiteSpace(amountStr) && int.TryParse(amountStr, out int amount))
            {
                if (EmptyInventoryCount() < amount)
                {
                    MessageBox.Show($"Not Enough Empty Inventory Space, Only {EmptyInventoryCount()} Spaces Are Empty.");
                    return;
                }
                long newId = ShowInventorySelection(true);
                if (newId > 0)
                {
                    for (int j = 0; j < Program.CurrentInventory.Count; j++)
                    {
                        for (int k = 0; k < InventoryRow.ColCount; k++)
                        {
                            if (Program.CurrentInventory[j][k] == 0)
                            {
                                Program.CurrentInventory[j][k] = newId;
                                amount--;
                            }
                            if (amount == 0)
                            {
                                dgvInventory.Refresh();
                                return;
                            }
                        }
                    }
                }
            }
        }

        private int EmptyInventoryCount()
        {
            int emptyCount = 0;
            foreach (var row in Program.CurrentInventory)
            {
                emptyCount += row.EmptyCount;
            }

            return emptyCount;
        }

        private void MenuReplace_Click(object sender, EventArgs e)
        {
            ShowInventorySelection(false);
        }

        private void dgvSquadStats_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private void dgvSquadStats_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && ModifierKeys == Keys.Control)
            {
                MenuCopyUnit_Click(sender, e);
            }
            if (e.KeyCode == Keys.V && ModifierKeys == Keys.Control)
            {
                MenuPasteUnit_Click(sender, e);
            }
        }

        private void BtAddAll_Click(object sender, EventArgs e)
        {
            if (cbSquads.SelectedIndex == 0)
            {
                Program.SolderList.OrderBy(x => x.Name).ToList().ForEach(x =>
                {
                    if (x.Id < 10000)
                    {
                        OnSoldierClick(new ToolStripMenuItem(x.Name)
                        {
                            Tag = x.Id
                        }, new EventArgs());
                    }
                });
            }
        }

        private void BtSaveSquadSetup_Click(object sender, EventArgs e)
        {
            if (cbSquads.SelectedIndex < 1) return;

            var name = Interaction.InputBox("Enter new squad setup name", "Squad Setup Name", cbSquads.Text + " Squad Setup");

            if (!string.IsNullOrWhiteSpace(name))
            {
                if (Settings.Default.SavedSquads == null)
                    Settings.Default.SavedSquads = new System.Collections.Specialized.StringCollection();
                Settings.Default.SavedSquads.Add(name + "||" + JsonConvert.SerializeObject((cbSquads.SelectedItem as Squad)));

                Squads.Remove((cbSquads.SelectedItem as Squad));

                cbSquads.DataSource = null;
                cbSquads.DataSource = Squads;
                dgvSquadEquips.DataSource = null;
                dgvSquadStats.DataSource = null;
                BtEdit_Click(sender, e);
                Settings.Default.Save();
            }
        }

        private void BtRestoreSquad_Click(object sender, EventArgs e)
        {
            menuRestore.Items.Clear();

            if (Settings.Default.SavedSquads == null) return;

            foreach (var setup in Settings.Default.SavedSquads)
            {
                var setupName = setup.Split(new string[] { "||" }, 2, StringSplitOptions.RemoveEmptyEntries)[0];
                var setupUnit = setup.Split(new string[] { "||" }, 2, StringSplitOptions.RemoveEmptyEntries)[1];

                menuRestore.Items.Add(new ToolStripMenuItem(setupName, null, OnSquadRestoreItemClicked) { Tag = setupUnit });
            }

            menuRestore.Show(btRestoreSquad, new Point(0, btRestoreSquad.Height));
        }

        private void OnSquadRestoreItemClicked(object sender, EventArgs e)
        {
            var setup = JsonConvert.DeserializeObject<Squad>((sender as ToolStripMenuItem).Tag.ToString());

            Squads.Add(setup);

            cbSquads.DataSource = null;
            cbSquads.DataSource = Squads;
            dgvSquadEquips.DataSource = null;
            dgvSquadStats.DataSource = null;

            cbSquads.SelectedIndex = cbSquads.Items.Count - 1;
        }

        private void BtGiveWoodIronTool_Click(object sender, EventArgs e)
        {
            //wood
            for (int i = 0; i < 10; ++i)
                for (int j = 0; j < InventoryRow.ColCount; j++)
                {
                    Program.CurrentInventory[i][j] = 42;
                }

            //iron
            for (int i = 10; i < 20; ++i)
                for (int j = 0; j < InventoryRow.ColCount; j++)
                {
                    Program.CurrentInventory[i][j] = 22;
                }

            //tool
            for (int i = 20; i < Program.CurrentInventory.Count; ++i)
                for (int j = 0; j < InventoryRow.ColCount; j++)
                {
                    Program.CurrentInventory[i][j] = 36;
                }

            dgvInventory.Refresh();
        }
    }
}