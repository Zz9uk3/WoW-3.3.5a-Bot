using AmeisenBotUtilities;
using AmeisenBotUtilities.Enums;
using AmeisenBotUtilities.Objects;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für RememberUnitWindow.xaml
    /// </summary>
    public partial class RememberUnitWindow : Window
    {
        public bool ShouldRemember { get; private set; }
        public RememberedUnit UnitToRemmeber { get; private set; }
        public int ZoneID { get; private set; }
        public int MapID { get; private set; }
        public Vector3 Position { get; private set; }
        private string UnitName { get; set; }

        public RememberUnitWindow(Unit unit)
        {
            InitializeComponent();

            unit.Update();
            UnitName = unit.Name;
            ZoneID = unit.ZoneID;
            MapID = unit.MapID;
            Position = unit.pos;
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            ShouldRemember = false;
            Close();
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            UnitToRemmeber = new RememberedUnit
            {
                Name = UnitName,
                ZoneID = ZoneID,
                MapID = MapID,
                Position = Position
            };

            UnitToRemmeber.UnitTraits = new List<UnitTrait>();

            if (checkboxIsFlightmaster.IsChecked == true)
            {
                UnitToRemmeber.UnitTraits.Add(UnitTrait.FLIGHTMASTER);
            }

            if (checkboxSellsFood.IsChecked == true)
            {
                UnitToRemmeber.UnitTraits.Add(UnitTrait.FOOD);
            }

            if (checkboxSellsDrink.IsChecked == true)
            {
                UnitToRemmeber.UnitTraits.Add(UnitTrait.DRINK);
            }

            if (checkboxIsRepair.IsChecked == true)
            {
                UnitToRemmeber.UnitTraits.Add(UnitTrait.REPAIR);
            }

            if (checkboxIsVendor.IsChecked == true)
            {
                UnitToRemmeber.UnitTraits.Add(UnitTrait.SELL);
            }

            if (checkboxIsAuctionmaster.IsChecked == true)
            {
                UnitToRemmeber.UnitTraits.Add(UnitTrait.AUCTIONMASTER);
            }

            ShouldRemember = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            labelName.Content = UnitName;
        }
    }
}