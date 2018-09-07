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
        public bool ApplyColor { get; set; }
        private bool InteractionPossible { get; set; }

        public ColorPickWindow(Color activeColor)
        {
            InitializeComponent();
            ActiveColor = activeColor;
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
            if (InteractionPossible)
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
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sliderAlpha.Value = ActiveColor.A;
            sliderRed.Value = ActiveColor.R;
            sliderGreen.Value = ActiveColor.G;
            sliderBlue.Value = ActiveColor.B;

            colorRect.Background =
                new SolidColorBrush(
                    ActiveColor
                    );
            
            InteractionPossible = true;
        }
    }
}