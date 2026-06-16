using QuickType;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FreemanSaveEditor
{
    internal static class Program
    {
        public static Player CurrentPlayer { get; internal set; }
        public static List<Cloth> ShirtList { get; internal set; }
        public static List<Weapon> WeaponList { get; internal set; }
        public static List<Item> ItemList { get; internal set; }
        public static IEnumerable<object> CurrentHelmets { get; internal set; }
        public static IEnumerable<object> CurrentMasks { get; internal set; }
        public static IEnumerable<object> CurrentArmors { get; internal set; }
        public static IEnumerable<object> CurrentShirts { get; internal set; }
        public static IEnumerable<object> CurrentPants { get; internal set; }
        public static IEnumerable<object> CurrentPistols { get; internal set; }
        public static IEnumerable<object> CurrentScopes { get; internal set; }
        public static IEnumerable<object> CurrentWeapons { get; internal set; }

        public static IEnumerable<object> CurrentMisc { get; internal set; }
        public static IEnumerable<object> CurrentItems { get; internal set; }

        public static string CurrentPath { get; internal set; }
        public static Dictionary<string, Localization> LocalizationList { get; internal set; }
        public static List<Soldier> SolderList { get; internal set; }
        public static Hero CurrentHeroes { get; internal set; }
        public static List<InventoryRow> CurrentInventory { get; internal set; }
        public static List<DesignChart> DesignChart { get; internal set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmMain());
        }
    }
}