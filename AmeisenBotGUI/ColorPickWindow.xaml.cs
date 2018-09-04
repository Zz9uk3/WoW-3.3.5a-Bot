using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AmeisenBotGUI
{
    /// <summary>
    /// Interaktionslogik für ColorPickWindow.xaml
    /// </summary>
    public partial class ColorPickWindow : Window
    {
        public Color ActiveColor { get; private set; }
        public bool ApplyColor { get; private set; }

        public ColorPickWindow()
        {
            InitializeComponent();
        }

        private void UpdateColor(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                ActiveColor = Color.FromArgb(
                            (byte)sliderAlpha.Value,
                            (byte)sliderRed.Value,
                            (byte)sliderGreen.Value,
                            (byte)sliderBlue.Value);

                colorRect.Background =
                    new SolidColorBrush(
                        ActiveColor
                        );
            }
            catch { }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ApplyColor = true;
            Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
