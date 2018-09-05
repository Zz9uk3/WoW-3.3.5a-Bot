using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            ApplyColor = true;
            Close();
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}