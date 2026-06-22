using System.Windows;

namespace FoodStoreApp
{
    public partial class HelpAdminWindow : Window
    {
        public HelpAdminWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}