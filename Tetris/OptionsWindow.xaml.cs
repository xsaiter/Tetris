using System.Windows;

namespace Tetris {
    public partial class OptionsWindow : Window {
        public OptionsWindow(int speed) {
            InitializeComponent();
            SliderSpeed.Value = speed;
        }

        void OK_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }              
    }
}
