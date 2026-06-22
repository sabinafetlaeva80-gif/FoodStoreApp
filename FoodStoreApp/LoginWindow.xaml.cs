using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MMarketApp.Database;

namespace MMarketApp
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => txtLogin.Focus();
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible)
            {
                txtVisiblePassword.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtVisiblePassword.Visibility = Visibility.Visible;
                eyeIcon.Text = "🙈";
            }
            else
            {
                txtPassword.Password = txtVisiblePassword.Text;
                txtVisiblePassword.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;
                eyeIcon.Text = "👁️";
            }
        }

        private string GetPassword()
        {
            if (txtPassword.Visibility == Visibility.Visible)
                return txtPassword.Password;
            else
                return txtVisiblePassword.Text;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = GetPassword();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                lblError.Text = "Введите логин и пароль";
                return;
            }

            if (DatabaseHelper.Login(login, password, out string role, out string fullName, out int userId))
            {
                App.CurrentUserId = userId;
                App.CurrentUserRole = role;
                App.CurrentUserName = fullName;

                if (role == "Admin")
                {
                    AdminWindow adminWin = new AdminWindow();
                    adminWin.Show();
                    this.Close();
                }
                else
                {
                    var activeShift = DatabaseHelper.GetActiveShift(userId);

                    if (activeShift != null)
                    {
                        var result = MessageBox.Show(
                            $"У вас есть открытая смена №{activeShift.ShiftId}\n" +
                            $"Начало: {activeShift.StartTime:HH:mm}\n\n" +
                            "Хотите продолжить эту смену?",
                            "Открытая смена",
                            MessageBoxButton.YesNoCancel,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            App.CurrentShiftId = activeShift.ShiftId;
                            OpenSellerWindow();
                        }
                        else if (result == MessageBoxResult.No)
                        {
                            DatabaseHelper.EndShift(activeShift.ShiftId);
                            int newShiftId = DatabaseHelper.StartShift(userId);
                            App.CurrentShiftId = newShiftId;
                            OpenSellerWindow();
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show(
                            "У вас нет открытых смен.\n\nХотите открыть новую смену?",
                            "Новая смена",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            int newShiftId = DatabaseHelper.StartShift(userId);
                            App.CurrentShiftId = newShiftId;
                            OpenSellerWindow();
                        }
                    }
                }
            }
            else
            {
                lblError.Text = "Неверный логин или пароль";
            }
        }

        private void OpenSellerWindow()
        {
            SellerWindow sellerWin = new SellerWindow();
            sellerWin.Show();
            this.Close();
        }
    }
}