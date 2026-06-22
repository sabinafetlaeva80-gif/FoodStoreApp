using FoodStoreApp;
using MMarketApp.Database;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MMarketApp
{
    public partial class SellerWindow : Window
    {
        private DataTable cart = new DataTable();

        public SellerWindow()
        {
            InitializeComponent();
            lblUser.Text = App.CurrentUserName;
            lblShift.Text = $"Смена #{App.CurrentShiftId}";

            InitializeCart();
            ConfigureProductsGrid();
            ConfigureCartGrid();
            ConfigureOrdersGrid();
            ConfigureOrderDetailsGrid();
            LoadProducts();
            LoadCategories();
            LoadOrders();
            LoadShiftStats();
        }

        private void InitializeCart()
        {
            cart.Columns.Add("ProductID", typeof(int));
            cart.Columns.Add("Name", typeof(string));
            cart.Columns.Add("Price", typeof(decimal));
            cart.Columns.Add("Quantity", typeof(int));
            cart.Columns.Add("Total", typeof(decimal));
            dgCart.ItemsSource = cart.DefaultView;
        }

        private void ConfigureProductsGrid()
        {
            dgProducts.AutoGenerateColumns = false;
            dgProducts.Columns.Clear();

            dgProducts.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding("name"), Width = 220 });
            dgProducts.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new Binding("categoryname"), Width = 120 });

            var priceCol = new DataGridTextColumn { Header = "Цена", Binding = new Binding("price") { StringFormat = "{0:F2} ₽" }, Width = 90 };
            dgProducts.Columns.Add(priceCol);

            dgProducts.Columns.Add(new DataGridTextColumn { Header = "Остаток", Binding = new Binding("stock"), Width = 70 });
            dgProducts.Columns.Add(new DataGridTextColumn { Header = "Ед.", Binding = new Binding("unit"), Width = 50 });
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpCashierWindow help = new HelpCashierWindow();
            help.Owner = this;
            help.ShowDialog();
        }

        private void ConfigureCartGrid()
        {
            dgCart.AutoGenerateColumns = false;
            dgCart.Columns.Clear();

            dgCart.Columns.Add(new DataGridTextColumn { Header = "Товар", Binding = new Binding("Name"), Width = 150 });
            dgCart.Columns.Add(new DataGridTextColumn { Header = "Кол-во", Binding = new Binding("Quantity"), Width = 60 });
            dgCart.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new Binding("Total") { StringFormat = "{0:F2}" }, Width = 80 });

            var deleteCol = new DataGridTemplateColumn { Header = "", Width = 35 };
            var factory = new FrameworkElementFactory(typeof(Button));
            factory.SetValue(Button.ContentProperty, "✕");
            factory.SetValue(Button.WidthProperty, 25.0);
            factory.SetValue(Button.HeightProperty, 25.0);
            factory.SetValue(Button.BackgroundProperty, System.Windows.Media.Brushes.Red);
            factory.SetValue(Button.ForegroundProperty, System.Windows.Media.Brushes.White);
            factory.SetValue(Button.TagProperty, new Binding("ProductID"));
            factory.AddHandler(Button.ClickEvent, new RoutedEventHandler(RemoveFromCart_Click));
            deleteCol.CellTemplate = new DataTemplate { VisualTree = factory };
            dgCart.Columns.Add(deleteCol);
        }

        private void ConfigureOrdersGrid()
        {
            dgOrders.AutoGenerateColumns = false;
            dgOrders.Columns.Clear();

            dgOrders.Columns.Add(new DataGridTextColumn { Header = "№ чека", Binding = new Binding("ordernumber"), Width = 120 });
            dgOrders.Columns.Add(new DataGridTextColumn { Header = "Дата", Binding = new Binding("orderdate"), Width = 140 });
            var amountCol = new DataGridTextColumn { Header = "Сумма", Binding = new Binding("totalamount") { StringFormat = "{0:F2} ₽" }, Width = 100 };
            dgOrders.Columns.Add(amountCol);
            dgOrders.Columns.Add(new DataGridTextColumn { Header = "Оплата", Binding = new Binding("paymentmethod"), Width = 80 });
            dgOrders.Columns.Add(new DataGridTextColumn { Header = "Продавец", Binding = new Binding("sellername"), Width = 150 });
        }

        private void ConfigureOrderDetailsGrid()
        {
            dgOrderDetails.AutoGenerateColumns = false;
            dgOrderDetails.Columns.Clear();

            dgOrderDetails.Columns.Add(new DataGridTextColumn { Header = "Товар", Binding = new Binding("productname"), Width = 200 });
            dgOrderDetails.Columns.Add(new DataGridTextColumn { Header = "Кол-во", Binding = new Binding("quantity"), Width = 80 });
            dgOrderDetails.Columns.Add(new DataGridTextColumn { Header = "Цена", Binding = new Binding("unitprice") { StringFormat = "{0:F2} ₽" }, Width = 100 });
            dgOrderDetails.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new Binding("totalprice") { StringFormat = "{0:F2} ₽" }, Width = 100 });
        }

        private void LoadProducts()
        {
            try
            {
                DataTable products = DatabaseHelper.GetAllProducts();
                dgProducts.ItemsSource = products.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCategories()
        {
            DataTable categories = DatabaseHelper.GetAllCategories();
            DataRow allRow = categories.NewRow();
            allRow["name"] = "Все категории";
            allRow["categoryid"] = 0;
            categories.Rows.InsertAt(allRow, 0);

            cmbCategory.DisplayMemberPath = "name";
            cmbCategory.SelectedValuePath = "categoryid";
            cmbCategory.ItemsSource = categories.DefaultView;
            cmbCategory.SelectedIndex = 0;
        }

        private void LoadOrders()
        {
            DataTable orders = DatabaseHelper.GetOrders();
            dgOrders.ItemsSource = orders.DefaultView;
        }

        private void LoadShiftStats()
        {
            try
            {
                DataTable stats = DatabaseHelper.GetCurrentShiftStats(App.CurrentShiftId);
                if (stats.Rows.Count > 0)
                {
                    var row = stats.Rows[0];
                    lblShift.Text = $"Смена #{App.CurrentShiftId} | Начало: {(row["starttime"] != null ? Convert.ToDateTime(row["starttime"]).ToString("HH:mm") : "--")}";
                }
            }
            catch { }
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataRow row in cart.Rows)
                total += Convert.ToDecimal(row["Total"]);
            lblTotal.Text = $"{total:F2} ₽";
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
                LoadProducts();
            else
                dgProducts.ItemsSource = DatabaseHelper.SearchProducts(txtSearch.Text).DefaultView;
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategory.SelectedValue != null && Convert.ToInt32(cmbCategory.SelectedValue) > 0)
            {
                int categoryId = Convert.ToInt32(cmbCategory.SelectedValue);
                dgProducts.ItemsSource = DatabaseHelper.GetProductsByCategory(categoryId).DefaultView;
            }
            else if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                dgProducts.ItemsSource = DatabaseHelper.SearchProducts(txtSearch.Text).DefaultView;
            }
            else
            {
                LoadProducts();
            }
        }

        private void DgProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddSelectedToCart();
        }

        private void DgProducts_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AddSelectedToCart();
        }

        private void AddSelectedToCart()
        {
            if (dgProducts.SelectedItem is DataRowView row)
            {
                int productId = Convert.ToInt32(row["productid"]);
                string name = row["name"]?.ToString() ?? "";
                decimal price = 0;
                int stock = 0;

                if (row["price"] != null && row["price"] != DBNull.Value)
                    price = Convert.ToDecimal(row["price"]);
                if (row["stock"] != null && row["stock"] != DBNull.Value)
                    stock = Convert.ToInt32(row["stock"]);

                if (stock <= 0)
                {
                    MessageBox.Show($"Товар \"{name}\" отсутствует на складе!", "Нет в наличии", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var existing = cart.Select($"ProductID = {productId}");
                if (existing.Length > 0)
                {
                    int newQty = Convert.ToInt32(existing[0]["Quantity"]) + 1;
                    if (newQty > stock)
                    {
                        MessageBox.Show($"Товара \"{name}\" доступно только {stock} шт!", "Превышение остатка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    existing[0]["Quantity"] = newQty;
                    existing[0]["Total"] = newQty * price;
                }
                else
                {
                    cart.Rows.Add(productId, name, price, 1, price);
                }

                UpdateTotal();
                dgProducts.SelectedItem = null;
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                int productId = Convert.ToInt32(btn.Tag);
                DataRow[] rows = cart.Select($"ProductID = {productId}");
                if (rows.Length > 0)
                    rows[0].Delete();
                UpdateTotal();
            }
        }

        private void BtnClearCart_Click(object sender, RoutedEventArgs e)
        {
            if (cart.Rows.Count > 0 && MessageBox.Show("Очистить корзину?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                cart.Clear();
                UpdateTotal();
            }
        }

        private void BtnSell_Click(object sender, RoutedEventArgs e)
        {
            if (cart.Rows.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string payment = ((ComboBoxItem)cmbPayment.SelectedItem)?.Content?.ToString() ?? "Наличные";
            string paymentMethod = payment == "Наличные" ? "Cash" : payment == "Карта" ? "Card" : "QR";

            string orderNumber = $"ЧЕК-{DateTime.Now:yyyyMMdd-HHmmss}";

            if (DatabaseHelper.CreateOrder(orderNumber, App.CurrentUserId, "Гость", paymentMethod, cart))
            {
                MessageBox.Show($"Продажа оформлена!\nЧек: {orderNumber}\nСумма: {lblTotal.Text}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                cart.Clear();
                UpdateTotal();
                LoadProducts();
                LoadOrders();
            }
            else
            {
                MessageBox.Show("Ошибка при оформлении продажи!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSales_Click(object sender, RoutedEventArgs e)
        {
            panelSales.Visibility = Visibility.Visible;
            panelChecks.Visibility = Visibility.Collapsed;
            panelExpired.Visibility = Visibility.Collapsed;
            btnSales.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#2c3e50");
            btnChecks.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#34495e");
            btnExpired.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#c0392b");
        }

        private void BtnChecks_Click(object sender, RoutedEventArgs e)
        {
            panelSales.Visibility = Visibility.Collapsed;
            panelChecks.Visibility = Visibility.Visible;
            panelExpired.Visibility = Visibility.Collapsed;
            btnChecks.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#2c3e50");
            btnSales.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#34495e");
            btnExpired.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#c0392b");
            LoadOrders();
        }

        private void BtnExpired_Click(object sender, RoutedEventArgs e)
        {
            panelSales.Visibility = Visibility.Collapsed;
            panelChecks.Visibility = Visibility.Collapsed;
            panelExpired.Visibility = Visibility.Visible;
            btnExpired.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#2c3e50");
            btnSales.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#34495e");
            btnChecks.Background = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#34495e");
            LoadExpiredProducts();
        }

        private void LoadExpiredProducts()
        {
            try
            {
                DataTable expired = DatabaseHelper.GetExpiredProducts();
                dgExpiredProducts.ItemsSource = expired.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRefreshExpired_Click(object sender, RoutedEventArgs e)
        {
            LoadExpiredProducts();
        }

        private void DgOrders_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgOrders.SelectedItem is DataRowView row)
            {
                int orderId = Convert.ToInt32(row["orderid"]);
                DataTable details = DatabaseHelper.GetOrderDetails(orderId);
                dgOrderDetails.ItemsSource = details.DefaultView;
                panelOrderDetails.Visibility = Visibility.Visible;
            }
        }

        private void BtnSearchOrders_Click(object sender, RoutedEventArgs e)
        {
            DateTime? from = dpDateFrom.SelectedDate;
            DateTime? to = dpDateTo.SelectedDate;
            dgOrders.ItemsSource = DatabaseHelper.GetOrders(from, to).DefaultView;
        }

        private void BtnReturnOrder_Click(object sender, RoutedEventArgs e)
        {
            if (dgOrders.SelectedItem is DataRowView orderRow)
            {
                int orderId = Convert.ToInt32(orderRow["orderid"]);
                string orderNumber = orderRow["ordernumber"].ToString();

                if (MessageBox.Show($"Вернуть все товары по чеку {orderNumber}?", "Подтверждение возврата",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    if (DatabaseHelper.ReturnOrder(orderId, App.CurrentUserId))
                    {
                        MessageBox.Show("Возврат оформлен успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadOrders();
                        LoadProducts();
                        panelOrderDetails.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при оформлении возврата!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnLayaway_Click(object sender, RoutedEventArgs e)
        {
            if (cart.Rows.Count == 0)
            {
                MessageBox.Show("Корзина пуста!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string orderNumber = "";
            Window inputDialog = new Window();
            inputDialog.Title = "Отложить заказ";
            inputDialog.Width = 400;
            inputDialog.Height = 200;
            inputDialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            inputDialog.Owner = this;
            inputDialog.ResizeMode = ResizeMode.NoResize;

            StackPanel inputStack = new StackPanel { Margin = new Thickness(20) };
            inputStack.Children.Add(new TextBlock { Text = "Введите номер заказа:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) });
            TextBox txtOrderNumber = new TextBox { Height = 35, FontSize = 14, Margin = new Thickness(0, 0, 0, 15) };
            inputStack.Children.Add(txtOrderNumber);

            StackPanel inputButtons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            Button btnOk = new Button { Content = "ОК", Width = 100, Height = 35, Margin = new Thickness(0, 0, 10, 0), Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Cursor = System.Windows.Input.Cursors.Hand };
            Button btnCancelInput = new Button { Content = "Отмена", Width = 100, Height = 35, Cursor = System.Windows.Input.Cursors.Hand };
            inputButtons.Children.Add(btnOk);
            inputButtons.Children.Add(btnCancelInput);
            inputStack.Children.Add(inputButtons);
            inputDialog.Content = new Border { Child = inputStack, Background = System.Windows.Media.Brushes.White, CornerRadius = new CornerRadius(10) };

            string resultNumber = "";
            btnOk.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtOrderNumber.Text))
                {
                    MessageBox.Show("Введите номер заказа!");
                    return;
                }
                resultNumber = txtOrderNumber.Text.Trim();
                inputDialog.DialogResult = true;
                inputDialog.Close();
            };
            btnCancelInput.Click += (s, ev) => inputDialog.Close();

            if (inputDialog.ShowDialog() != true)
                return;

            orderNumber = resultNumber;

            string payment = ((ComboBoxItem)cmbPayment.SelectedItem)?.Content?.ToString() ?? "Наличные";
            string paymentMethod = payment == "Наличные" ? "Cash" : payment == "Карта" ? "Card" : "QR";

            DataTable copyCart = cart.Copy();

            if (DatabaseHelper.CreateLayawayOrder(orderNumber, App.CurrentUserId, paymentMethod, copyCart))
            {
                MessageBox.Show($"Заказ отложен!\nНомер: {orderNumber}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                cart.Clear();
                UpdateTotal();
                LoadProducts();
            }
            else
            {
                MessageBox.Show("Ошибка при откладывании заказа!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRecallLayaway_Click(object sender, RoutedEventArgs e)
        {
            DataTable layawayOrders = DatabaseHelper.GetLayawayOrders(App.CurrentUserId);
            if (layawayOrders == null || layawayOrders.Rows.Count == 0)
            {
                MessageBox.Show("Нет отложенных заказов!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            Window dialog = new Window();
            dialog.Title = "Отложенные заказы";
            dialog.Width = 950;
            dialog.Height = 800;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;

            Border mainBorder = new Border { Background = System.Windows.Media.Brushes.White, CornerRadius = new CornerRadius(10), Padding = new Thickness(20) };
            StackPanel stack = new StackPanel();

            TextBlock titleBlock = new TextBlock
            {
                Text = "УПРАВЛЕНИЕ ОТЛОЖЕННЫМИ ЗАКАЗАМИ",
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#2c3e50"),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            stack.Children.Add(titleBlock);

            TextBlock listTitle = new TextBlock
            {
                Text = "Список отложенных заказов:",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#3498db"),
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(listTitle);

            DataGrid dgLayaway = new DataGrid
            {
                AutoGenerateColumns = false,
                Height = 220,
                IsReadOnly = true,
                FontSize = 13,
                RowHeight = 40,
                Margin = new Thickness(0, 0, 0, 20)
            };
            dgLayaway.Columns.Add(new DataGridTextColumn { Header = "Номер заказа", Binding = new Binding("ordernumber"), Width = 200 });
            dgLayaway.Columns.Add(new DataGridTextColumn { Header = "Дата", Binding = new Binding("orderdate"), Width = 160 });
            dgLayaway.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new Binding("totalamount") { StringFormat = "{0:F2} ₽" }, Width = 140 });
            dgLayaway.Columns.Add(new DataGridTextColumn { Header = "Способ оплаты", Binding = new Binding("paymentmethod"), Width = 130 });
            dgLayaway.ItemsSource = layawayOrders.DefaultView;
            stack.Children.Add(dgLayaway);

            stack.Children.Add(new Border { BorderBrush = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#bdc3c7"), BorderThickness = new Thickness(0, 1, 0, 0), Margin = new Thickness(0, 5, 0, 15) });

            TextBlock detailsTitle = new TextBlock
            {
                Text = "Состав выбранного заказа:",
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Foreground = (System.Windows.Media.Brush)new System.Windows.Media.BrushConverter().ConvertFrom("#27ae60"),
                Margin = new Thickness(0, 0, 0, 10)
            };
            stack.Children.Add(detailsTitle);

            DataGrid dgOrderItems = new DataGrid
            {
                AutoGenerateColumns = false,
                Height = 220,
                IsReadOnly = true,
                FontSize = 13,
                RowHeight = 40,
                Margin = new Thickness(0, 0, 0, 20)
            };
            dgOrderItems.Columns.Add(new DataGridTextColumn { Header = "Товар", Binding = new Binding("productname"), Width = 380 });
            dgOrderItems.Columns.Add(new DataGridTextColumn { Header = "Кол-во", Binding = new Binding("quantity"), Width = 100 });
            dgOrderItems.Columns.Add(new DataGridTextColumn { Header = "Цена", Binding = new Binding("unitprice") { StringFormat = "{0:F2} ₽" }, Width = 130 });
            dgOrderItems.Columns.Add(new DataGridTextColumn { Header = "Сумма", Binding = new Binding("totalprice") { StringFormat = "{0:F2} ₽" }, Width = 140 });
            stack.Children.Add(dgOrderItems);

            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
            Button btnSelect = new Button { Content = "Восстановить", Width = 130, Height = 40, Margin = new Thickness(0, 0, 15, 0), Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, FontSize = 14, FontWeight = FontWeights.Bold, Cursor = System.Windows.Input.Cursors.Hand };
            Button btnClose = new Button { Content = "Закрыть", Width = 100, Height = 40, Background = System.Windows.Media.Brushes.Gray, Foreground = System.Windows.Media.Brushes.White, FontSize = 14, Cursor = System.Windows.Input.Cursors.Hand };
            btnPanel.Children.Add(btnSelect);
            btnPanel.Children.Add(btnClose);
            stack.Children.Add(btnPanel);

            mainBorder.Child = new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            dialog.Content = mainBorder;

            dgLayaway.SelectionChanged += (s, ev) =>
            {
                if (dgLayaway.SelectedItem is DataRowView row)
                {
                    int orderId = Convert.ToInt32(row["orderid"]);
                    DataTable details = DatabaseHelper.GetLayawayOrderDetails(orderId);
                    dgOrderItems.ItemsSource = details.DefaultView;
                }
            };

            btnSelect.Click += (s, ev) =>
            {
                if (dgLayaway.SelectedItem is DataRowView row)
                {
                    int orderId = Convert.ToInt32(row["orderid"]);
                    string orderNumber = row["ordernumber"].ToString();
                    DataTable layawayDetails = DatabaseHelper.GetLayawayOrderDetails(orderId);

                    cart.Clear();
                    foreach (DataRow detail in layawayDetails.Rows)
                    {
                        int productId = Convert.ToInt32(detail["productid"]);
                        int quantity = Convert.ToInt32(detail["quantity"]);
                        int stock = DatabaseHelper.GetProductStock(productId);

                        if (stock < quantity)
                        {
                            MessageBox.Show($"Недостаточно товара \"{detail["productname"]}\" на складе! Доступно: {stock} шт.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    foreach (DataRow detail in layawayDetails.Rows)
                    {
                        cart.Rows.Add(detail["productid"], detail["productname"], detail["unitprice"], detail["quantity"], detail["totalprice"]);
                    }
                    UpdateTotal();
                    DatabaseHelper.DeleteLayawayOrder(orderId);
                    MessageBox.Show($"Заказ {orderNumber} восстановлен в корзину!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    dialog.Close();
                }
                else
                {
                    MessageBox.Show("Выберите заказ для восстановления!");
                }
            };
            btnClose.Click += (s, ev) => dialog.Close();
            dialog.ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Выберите действие при выходе: Закрыть смену?",
                "Выход",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                DatabaseHelper.EndShift(App.CurrentShiftId);
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }
            else if (result == MessageBoxResult.No)
            {
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
        }
    }
}