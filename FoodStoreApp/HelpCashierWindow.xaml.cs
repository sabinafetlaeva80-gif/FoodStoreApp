using System.Windows;

namespace FoodStoreApp
{
    public partial class HelpCashierWindow : Window
    {
        public HelpCashierWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}