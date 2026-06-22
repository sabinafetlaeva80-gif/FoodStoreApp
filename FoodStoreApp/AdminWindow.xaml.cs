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
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            lblUser.Text = App.CurrentUserName;
            ConfigureAllGrids();
            LoadUsers();
            LoadProducts();
            LoadCategories();
            LoadOrders();
            LoadReports();
            LoadSuppliers();
            LoadSupplyHistory();
            LoadLowStock();
            AddButtonsToPanels();
        }

        private void ConfigureAllGrids()
        {
            ConfigureUsersGrid();
            ConfigureProductsGrid();
            ConfigureCategoriesGrid();
            ConfigureOrdersGrid();
            ConfigureSuppliersGrid();
            ConfigureSupplyHistoryGrid();
            ConfigureLowStockGrid();
            ConfigureExpiredGrid();
        }

        private void ConfigureUsersGrid()
        {
            if (dgUsers != null)
            {
                dgUsers.AutoGenerateColumns = false;
                dgUsers.IsReadOnly = true;
                dgUsers.Columns.Clear();
                dgUsers.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("userid"), Width = 50 });
                dgUsers.Columns.Add(new DataGridTextColumn { Header = "Логин", Binding = new Binding("login"), Width = 120 });
                dgUsers.Columns.Add(new DataGridTextColumn { Header = "ФИО", Binding = new Binding("fullname"), Width = 150 });
                dgUsers.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new Binding("email"), Width = 150 });
                dgUsers.Columns.Add(new DataGridTextColumn { Header = "Роль", Binding = new Binding("role"), Width = 100 });
                dgUsers.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new Binding("status"), Width = 80 });
                var createdAtColumn = new DataGridTextColumn { Header = "Дата создания", Binding = new Binding("createdat"), Width = 120 };
                createdAtColumn.Binding.StringFormat = "dd.MM.yyyy";
                dgUsers.Columns.Add(createdAtColumn);
                var templateColumn = new DataGridTemplateColumn { Header = "Действия", Width = 100 };
                var factory = new FrameworkElementFactory(typeof(StackPanel));
                factory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                factory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                var editBtn = new FrameworkElementFactory(typeof(Button));
                editBtn.SetValue(Button.ContentProperty, "✏️");
                editBtn.SetValue(Button.WidthProperty, 30.0);
                editBtn.SetValue(Button.HeightProperty, 30.0);
                editBtn.SetValue(Button.MarginProperty, new Thickness(2));
                editBtn.SetValue(Button.TagProperty, new Binding("userid"));
                editBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnEditUser_Click));
                var deleteBtn = new FrameworkElementFactory(typeof(Button));
                deleteBtn.SetValue(Button.ContentProperty, "🗑️");
                deleteBtn.SetValue(Button.WidthProperty, 30.0);
                deleteBtn.SetValue(Button.HeightProperty, 30.0);
                deleteBtn.SetValue(Button.MarginProperty, new Thickness(2));
                deleteBtn.SetValue(Button.TagProperty, new Binding("userid"));
                deleteBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnDeleteUser_Click));
                factory.AppendChild(editBtn);
                factory.AppendChild(deleteBtn);
                templateColumn.CellTemplate = new DataTemplate { VisualTree = factory };
                dgUsers.Columns.Add(templateColumn);
            }
        }

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        private void ConfigureLowStockGrid()
        {
            if (dgLowStock != null)
            {
                dgLowStock.AutoGenerateColumns = false;
                dgLowStock.IsReadOnly = true;
                dgLowStock.Columns.Clear();
                dgLowStock.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding("name"), Width = 200 });
                dgLowStock.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new Binding("CategoryName"), Width = 130 });
                dgLowStock.Columns.Add(new DataGridTextColumn { Header = "Остаток", Binding = new Binding("stock"), Width = 80 });
                dgLowStock.Columns.Add(new DataGridTextColumn { Header = "Ед.", Binding = new Binding("unit"), Width = 50 });
                dgLowStock.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new Binding("StockStatus"), Width = 100 });
            }
        }
        private void BtnHelp_Click(object sender, RoutedEventArgs e)
        {
            HelpAdminWindow help = new HelpAdminWindow();
            help.Owner = this;
            help.ShowDialog();
        }

        private void ConfigureProductsGrid()
        {
            if (dgProducts != null)
            {
                dgProducts.AutoGenerateColumns = false;
                dgProducts.IsReadOnly = true;
                dgProducts.Columns.Clear();
                dgProducts.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding("name"), Width = 200 });
                dgProducts.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new Binding("categoryname"), Width = 130 });
                dgProducts.Columns.Add(new DataGridTextColumn { Header = "Бренд", Binding = new Binding("brand"), Width = 100 });
                var priceColumn = new DataGridTextColumn { Header = "Цена", Binding = new Binding("price"), Width = 80 };
                priceColumn.Binding.StringFormat = "{0:F2} ₽";
                dgProducts.Columns.Add(priceColumn);
                dgProducts.Columns.Add(new DataGridTextColumn { Header = "Остаток", Binding = new Binding("stock"), Width = 70 });
                dgProducts.Columns.Add(new DataGridTextColumn { Header = "Ед.", Binding = new Binding("unit"), Width = 50 });
                dgProducts.Columns.Add(new DataGridTextColumn { Header = "Срок годности", Binding = new Binding("expirydate"), Width = 100 });
                var productTemplateColumn = new DataGridTemplateColumn { Header = "Действия", Width = 100 };
                var productFactory = new FrameworkElementFactory(typeof(StackPanel));
                productFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                productFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                var editProductBtn = new FrameworkElementFactory(typeof(Button));
                editProductBtn.SetValue(Button.ContentProperty, "✏️");
                editProductBtn.SetValue(Button.WidthProperty, 30.0);
                editProductBtn.SetValue(Button.HeightProperty, 30.0);
                editProductBtn.SetValue(Button.MarginProperty, new Thickness(2));
                editProductBtn.SetValue(Button.TagProperty, new Binding("productid"));
                editProductBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnEditProduct_Click));
                var deleteProductBtn = new FrameworkElementFactory(typeof(Button));
                deleteProductBtn.SetValue(Button.ContentProperty, "🗑️");
                deleteProductBtn.SetValue(Button.WidthProperty, 30.0);
                deleteProductBtn.SetValue(Button.HeightProperty, 30.0);
                deleteProductBtn.SetValue(Button.MarginProperty, new Thickness(2));
                deleteProductBtn.SetValue(Button.TagProperty, new Binding("productid"));
                deleteProductBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnDeleteProduct_Click));
                productFactory.AppendChild(editProductBtn);
                productFactory.AppendChild(deleteProductBtn);
                productTemplateColumn.CellTemplate = new DataTemplate { VisualTree = productFactory };
                dgProducts.Columns.Add(productTemplateColumn);
            }
        }

        private void ConfigureCategoriesGrid()
        {
            if (dgCategories != null)
            {
                dgCategories.AutoGenerateColumns = false;
                dgCategories.IsReadOnly = true;
                dgCategories.Columns.Clear();
                dgCategories.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("categoryid"), Width = 50 });
                dgCategories.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding("name"), Width = 200 });
                dgCategories.Columns.Add(new DataGridTextColumn { Header = "Описание", Binding = new Binding("description"), Width = 300 });
                var categoryTemplateColumn = new DataGridTemplateColumn { Header = "Действия", Width = 100 };
                var categoryFactory = new FrameworkElementFactory(typeof(StackPanel));
                categoryFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                categoryFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                var editCategoryBtn = new FrameworkElementFactory(typeof(Button));
                editCategoryBtn.SetValue(Button.ContentProperty, "✏️");
                editCategoryBtn.SetValue(Button.WidthProperty, 30.0);
                editCategoryBtn.SetValue(Button.HeightProperty, 30.0);
                editCategoryBtn.SetValue(Button.MarginProperty, new Thickness(2));
                editCategoryBtn.SetValue(Button.TagProperty, new Binding("categoryid"));
                editCategoryBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnEditCategory_Click));
                var deleteCategoryBtn = new FrameworkElementFactory(typeof(Button));
                deleteCategoryBtn.SetValue(Button.ContentProperty, "🗑️");
                deleteCategoryBtn.SetValue(Button.WidthProperty, 30.0);
                deleteCategoryBtn.SetValue(Button.HeightProperty, 30.0);
                deleteCategoryBtn.SetValue(Button.MarginProperty, new Thickness(2));
                deleteCategoryBtn.SetValue(Button.TagProperty, new Binding("categoryid"));
                deleteCategoryBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnDeleteCategory_Click));
                categoryFactory.AppendChild(editCategoryBtn);
                categoryFactory.AppendChild(deleteCategoryBtn);
                categoryTemplateColumn.CellTemplate = new DataTemplate { VisualTree = categoryFactory };
                dgCategories.Columns.Add(categoryTemplateColumn);
            }
        }

        private void ConfigureOrdersGrid()
        {
            if (dgOrders != null)
            {
                dgOrders.AutoGenerateColumns = false;
                dgOrders.IsReadOnly = true;
                dgOrders.Columns.Clear();
                dgOrders.Columns.Add(new DataGridTextColumn { Header = "№ чека", Binding = new Binding("ordernumber"), Width = 120 });
                dgOrders.Columns.Add(new DataGridTextColumn { Header = "Дата", Binding = new Binding("orderdate"), Width = 120 });
                var amountColumn = new DataGridTextColumn { Header = "Сумма", Binding = new Binding("totalamount"), Width = 100 };
                amountColumn.Binding.StringFormat = "{0:F2} ₽";
                dgOrders.Columns.Add(amountColumn);
                dgOrders.Columns.Add(new DataGridTextColumn { Header = "Оплата", Binding = new Binding("paymentmethod"), Width = 80 });
                dgOrders.Columns.Add(new DataGridTextColumn { Header = "Продавец", Binding = new Binding("sellername"), Width = 150 });
                var orderTemplateColumn = new DataGridTemplateColumn { Header = "Действия", Width = 100 };
                var orderFactory = new FrameworkElementFactory(typeof(StackPanel));
                orderFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                orderFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                var editOrderBtn = new FrameworkElementFactory(typeof(Button));
                editOrderBtn.SetValue(Button.ContentProperty, "✏️");
                editOrderBtn.SetValue(Button.WidthProperty, 30.0);
                editOrderBtn.SetValue(Button.HeightProperty, 30.0);
                editOrderBtn.SetValue(Button.MarginProperty, new Thickness(2));
                editOrderBtn.SetValue(Button.TagProperty, new Binding("orderid"));
                editOrderBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnEditOrder_Click));
                var deleteOrderBtn = new FrameworkElementFactory(typeof(Button));
                deleteOrderBtn.SetValue(Button.ContentProperty, "🗑️");
                deleteOrderBtn.SetValue(Button.WidthProperty, 30.0);
                deleteOrderBtn.SetValue(Button.HeightProperty, 30.0);
                deleteOrderBtn.SetValue(Button.MarginProperty, new Thickness(2));
                deleteOrderBtn.SetValue(Button.TagProperty, new Binding("orderid"));
                deleteOrderBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnDeleteOrder_Click));
                orderFactory.AppendChild(editOrderBtn);
                orderFactory.AppendChild(deleteOrderBtn);
                orderTemplateColumn.CellTemplate = new DataTemplate { VisualTree = orderFactory };
                dgOrders.Columns.Add(orderTemplateColumn);
            }
        }


        private void ConfigureSuppliersGrid()
        {
            if (dgSuppliers != null)
            {
                dgSuppliers.AutoGenerateColumns = false;
                dgSuppliers.IsReadOnly = true;
                dgSuppliers.Columns.Clear();
                dgSuppliers.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("supplierid"), Width = 50 });
                dgSuppliers.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding("name"), Width = 150 });
                dgSuppliers.Columns.Add(new DataGridTextColumn { Header = "Контактное лицо", Binding = new Binding("contactperson"), Width = 120 });
                dgSuppliers.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new Binding("phone"), Width = 100 });
                dgSuppliers.Columns.Add(new DataGridTextColumn { Header = "Email", Binding = new Binding("email"), Width = 150 });
                dgSuppliers.Columns.Add(new DataGridTextColumn { Header = "Адрес", Binding = new Binding("address"), Width = 200 });
                var supplierTemplateColumn = new DataGridTemplateColumn { Header = "Действия", Width = 100 };
                var supplierFactory = new FrameworkElementFactory(typeof(StackPanel));
                supplierFactory.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
                supplierFactory.SetValue(StackPanel.HorizontalAlignmentProperty, HorizontalAlignment.Center);
                var editSupplierBtn = new FrameworkElementFactory(typeof(Button));
                editSupplierBtn.SetValue(Button.ContentProperty, "✏️");
                editSupplierBtn.SetValue(Button.WidthProperty, 30.0);
                editSupplierBtn.SetValue(Button.HeightProperty, 30.0);
                editSupplierBtn.SetValue(Button.MarginProperty, new Thickness(2));
                editSupplierBtn.SetValue(Button.TagProperty, new Binding("supplierid"));
                editSupplierBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnEditSupplier_Click));
                var deleteSupplierBtn = new FrameworkElementFactory(typeof(Button));
                deleteSupplierBtn.SetValue(Button.ContentProperty, "🗑️");
                deleteSupplierBtn.SetValue(Button.WidthProperty, 30.0);
                deleteSupplierBtn.SetValue(Button.HeightProperty, 30.0);
                deleteSupplierBtn.SetValue(Button.MarginProperty, new Thickness(2));
                deleteSupplierBtn.SetValue(Button.TagProperty, new Binding("supplierid"));
                deleteSupplierBtn.AddHandler(Button.ClickEvent, new RoutedEventHandler(BtnDeleteSupplier_Click));
                supplierFactory.AppendChild(editSupplierBtn);
                supplierFactory.AppendChild(deleteSupplierBtn);
                supplierTemplateColumn.CellTemplate = new DataTemplate { VisualTree = supplierFactory };
                dgSuppliers.Columns.Add(supplierTemplateColumn);
            }
        }

        private void ConfigureSupplyHistoryGrid()
        {
            if (dgSupplyHistory != null)
            {
                dgSupplyHistory.AutoGenerateColumns = false;
                dgSupplyHistory.IsReadOnly = true;
                dgSupplyHistory.Columns.Clear();
                dgSupplyHistory.Columns.Add(new DataGridTextColumn { Header = "№ поставки", Binding = new Binding("supplynumber"), Width = 120 });
                dgSupplyHistory.Columns.Add(new DataGridTextColumn { Header = "Дата", Binding = new Binding("supplydate"), Width = 120 });
                dgSupplyHistory.Columns.Add(new DataGridTextColumn { Header = "Поставщик", Binding = new Binding("supplier"), Width = 150 });
                var amountColumn = new DataGridTextColumn { Header = "Сумма", Binding = new Binding("totalamount"), Width = 100 };
                amountColumn.Binding.StringFormat = "{0:F2} ₽";
                dgSupplyHistory.Columns.Add(amountColumn);
                dgSupplyHistory.Columns.Add(new DataGridTextColumn { Header = "Принял", Binding = new Binding("username"), Width = 150 });
                dgSupplyHistory.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new Binding("status"), Width = 100 });
            }
        }

        private void ConfigureExpiredGrid()
        {
            if (dgExpired != null)
            {
                dgExpired.AutoGenerateColumns = false;
                dgExpired.IsReadOnly = true;
                dgExpired.Columns.Clear();
                dgExpired.Columns.Add(new DataGridTextColumn { Header = "Название", Binding = new Binding("name"), Width = 200 });
                dgExpired.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new Binding("categoryname"), Width = 130 });
                dgExpired.Columns.Add(new DataGridTextColumn { Header = "Остаток", Binding = new Binding("stock"), Width = 80 });
                dgExpired.Columns.Add(new DataGridTextColumn { Header = "Ед.", Binding = new Binding("unit"), Width = 50 });
                dgExpired.Columns.Add(new DataGridTextColumn { Header = "Срок годности", Binding = new Binding("expirydate"), Width = 120 });
            }
        }

        private void AddButtonsToPanels()
        {
            if (panelProducts != null && !panelProducts.Children.OfType<StackPanel>().Any(p => p.Name == "ProductButtons"))
            {
                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15), Name = "ProductButtons" };
                var btnAdd = new Button { Content = "➕ Добавить товар", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 150, Height = 35, Margin = new Thickness(0, 0, 10, 0), Cursor = System.Windows.Input.Cursors.Hand };
                btnAdd.Click += BtnAddProduct_Click;
                btnPanel.Children.Add(btnAdd);
                var existingText = panelProducts.Children.OfType<TextBlock>().FirstOrDefault();
                if (existingText != null)
                {
                    int index = panelProducts.Children.IndexOf(existingText) + 1;
                    panelProducts.Children.Insert(index, btnPanel);
                }
            }

            if (panelCategories != null && !panelCategories.Children.OfType<StackPanel>().Any(p => p.Name == "CategoryButtons"))
            {
                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15), Name = "CategoryButtons" };
                var btnAdd = new Button { Content = "➕ Добавить категорию", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 150, Height = 35, Cursor = System.Windows.Input.Cursors.Hand };
                btnAdd.Click += BtnAddCategory_Click;
                btnPanel.Children.Add(btnAdd);
                var existingText = panelCategories.Children.OfType<TextBlock>().FirstOrDefault();
                if (existingText != null)
                {
                    int index = panelCategories.Children.IndexOf(existingText) + 1;
                    panelCategories.Children.Insert(index, btnPanel);
                }
            }

            if (panelOrders != null && !panelOrders.Children.OfType<StackPanel>().Any(p => p.Name == "OrderButtons"))
            {
                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15), Name = "OrderButtons" };
                var btnRefresh = new Button { Content = "🔄 Обновить", Background = System.Windows.Media.Brushes.Blue, Foreground = System.Windows.Media.Brushes.White, Width = 150, Height = 35, Cursor = System.Windows.Input.Cursors.Hand };
                btnRefresh.Click += (s, e) => LoadOrders();
                btnPanel.Children.Add(btnRefresh);
                var existingText = panelOrders.Children.OfType<TextBlock>().FirstOrDefault();
                if (existingText != null)
                {
                    int index = panelOrders.Children.IndexOf(existingText) + 1;
                    panelOrders.Children.Insert(index, btnPanel);
                }
            }

            if (panelSuppliers != null && !panelSuppliers.Children.OfType<StackPanel>().Any(p => p.Name == "SupplierButtons"))
            {
                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15), Name = "SupplierButtons" };
                var btnAdd = new Button { Content = "➕ Добавить поставщика", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 150, Height = 35, Cursor = System.Windows.Input.Cursors.Hand };
                btnAdd.Click += BtnAddSupplier_Click;
                btnPanel.Children.Add(btnAdd);
                var existingText = panelSuppliers.Children.OfType<TextBlock>().FirstOrDefault();
                if (existingText != null)
                {
                    int index = panelSuppliers.Children.IndexOf(existingText) + 1;
                    panelSuppliers.Children.Insert(index, btnPanel);
                }
            }

            if (panelSupply != null && !panelSupply.Children.OfType<StackPanel>().Any(p => p.Name == "SupplyButtons"))
            {
                var btnPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15), Name = "SupplyButtons" };
                var btnAdd = new Button { Content = "➕ Новое поступление", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 150, Height = 35, Cursor = System.Windows.Input.Cursors.Hand };
                btnAdd.Click += BtnNewSupply_Click;
                btnPanel.Children.Add(btnAdd);
                var existingText = panelSupply.Children.OfType<TextBlock>().FirstOrDefault();
                if (existingText != null)
                {
                    int index = panelSupply.Children.IndexOf(existingText) + 1;
                    panelSupply.Children.Insert(index, btnPanel);
                }
            }
        }

        private void LoadUsers()
        {
            dgUsers.ItemsSource = DatabaseHelper.GetAllUsers().DefaultView;
        }

        private void LoadProducts()
        {
            dgProducts.ItemsSource = DatabaseHelper.GetAllProducts().DefaultView;
        }

        private void LoadCategories()
        {
            dgCategories.ItemsSource = DatabaseHelper.GetAllCategories().DefaultView;
        }

        private void LoadOrders()
        {
            dgOrders.ItemsSource = DatabaseHelper.GetOrders().DefaultView;
        }

        private void LoadReports(DateTime? fromDate = null, DateTime? toDate = null)
        {
            DataTable stats = DatabaseHelper.GetSalesStats(fromDate, toDate);
            if (stats.Rows.Count > 0)
            {
                var row = stats.Rows[0];
                decimal totalRevenue = row["TotalRevenue"] != DBNull.Value ? Convert.ToDecimal(row["TotalRevenue"]) : 0;
                lblTotalRevenue.Text = $"{totalRevenue:F2} ₽";
                lblTotalOrders.Text = row["TotalOrders"] != DBNull.Value ? row["TotalOrders"].ToString() : "0";
                lblTotalItems.Text = row["TotalItems"] != DBNull.Value ? row["TotalItems"].ToString() : "0";
                decimal avgOrder = row["AvgOrderValue"] != DBNull.Value ? Convert.ToDecimal(row["AvgOrderValue"]) : 0;
                lblAvgOrder.Text = $"{avgOrder:F2} ₽";
            }
        }

        private void LoadSuppliers()
        {
            dgSuppliers.ItemsSource = DatabaseHelper.GetAllSuppliers().DefaultView;
        }

        private void LoadSupplyHistory()
        {
            dgSupplyHistory.ItemsSource = DatabaseHelper.GetSupplyHistory().DefaultView;
        }

        private void LoadLowStock()
        {
            dgLowStock.ItemsSource = DatabaseHelper.GetLowStockProducts(10).DefaultView;
        }

        private void LoadExpiredProducts()
        {
            dgExpired.ItemsSource = DatabaseHelper.GetExpiredProducts().DefaultView;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtSearch.Text))
                LoadProducts();
            else
                dgProducts.ItemsSource = DatabaseHelper.SearchProducts(txtSearch.Text).DefaultView;
        }

        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Users");
            LoadUsers();
        }

        private void BtnProducts_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Products");
            LoadProducts();
        }

        private void BtnCategories_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Categories");
            LoadCategories();
        }

        private void BtnOrders_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Orders");
            LoadOrders();
        }

        private void BtnReports_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Reports");
            LoadReports();
        }

        private void BtnSuppliers_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Suppliers");
            LoadSuppliers();
        }

        private void BtnSupply_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Supply");
            LoadSupplyHistory();
        }

        private void BtnLowStock_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("LowStock");
            LoadLowStock();
        }

        private void BtnExpiredProducts_Click(object sender, RoutedEventArgs e)
        {
            ShowPanel("Expired");
            LoadExpiredProducts();
        }

        private void BtnShowReport_Click(object sender, RoutedEventArgs e)
        {
            LoadReports(dpReportFrom.SelectedDate, dpReportTo.SelectedDate);
        }

        private void ShowPanel(string panel)
        {
            if (panelUsers != null) panelUsers.Visibility = Visibility.Collapsed;
            if (panelProducts != null) panelProducts.Visibility = Visibility.Collapsed;
            if (panelCategories != null) panelCategories.Visibility = Visibility.Collapsed;
            if (panelOrders != null) panelOrders.Visibility = Visibility.Collapsed;
            if (panelReports != null) panelReports.Visibility = Visibility.Collapsed;
            if (panelSuppliers != null) panelSuppliers.Visibility = Visibility.Collapsed;
            if (panelSupply != null) panelSupply.Visibility = Visibility.Collapsed;
            if (panelLowStock != null) panelLowStock.Visibility = Visibility.Collapsed;
            if (panelExpired != null) panelExpired.Visibility = Visibility.Collapsed;

            switch (panel)
            {
                case "Users": if (panelUsers != null) panelUsers.Visibility = Visibility.Visible; break;
                case "Products": if (panelProducts != null) panelProducts.Visibility = Visibility.Visible; break;
                case "Categories": if (panelCategories != null) panelCategories.Visibility = Visibility.Visible; break;
                case "Orders": if (panelOrders != null) panelOrders.Visibility = Visibility.Visible; break;
                case "Reports": if (panelReports != null) panelReports.Visibility = Visibility.Visible; break;
                case "Suppliers": if (panelSuppliers != null) panelSuppliers.Visibility = Visibility.Visible; break;
                case "Supply": if (panelSupply != null) panelSupply.Visibility = Visibility.Visible; break;
                case "LowStock": if (panelLowStock != null) panelLowStock.Visibility = Visibility.Visible; break;
                case "Expired": if (panelExpired != null) panelExpired.Visibility = Visibility.Visible; break;
            }
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            AddEditUserDialog();
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            AddEditUserDialog(id);
        }

        private void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            if (MessageBox.Show("Удалить пользователя?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.DeleteUser(id);
                LoadUsers();
            }
        }

        private void AddEditUserDialog(int id = 0)
        {
            Window dialog = new Window();
            dialog.Title = id == 0 ? "Новый пользователь" : "Редактирование пользователя";
            dialog.Width = 400;
            dialog.Height = 500;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            StackPanel stack = new StackPanel { Margin = new Thickness(20) };
            TextBox txtLogin = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            PasswordBox txtPassword = new PasswordBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtFullName = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtEmail = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            ComboBox cmbRole = new ComboBox { Margin = new Thickness(0, 5, 0, 20), Height = 35 };
            cmbRole.Items.Add(new ComboBoxItem { Content = "Продавец", Tag = "Seller" });
            cmbRole.Items.Add(new ComboBoxItem { Content = "Администратор", Tag = "Admin" });
            cmbRole.SelectedIndex = 0;

            if (id > 0)
            {
                DataTable users = DatabaseHelper.GetAllUsers();
                var user = users.AsEnumerable().FirstOrDefault(r => r.Field<int>("userid") == id);
                if (user != null)
                {
                    txtLogin.Text = user.Field<string>("login") ?? "";
                    txtLogin.IsEnabled = false;
                    txtFullName.Text = user.Field<string>("fullname") ?? "";
                    txtEmail.Text = user.Field<string>("email") ?? "";
                    string role = user.Field<string>("role") ?? "Seller";
                    cmbRole.SelectedIndex = role == "Admin" ? 1 : 0;
                }
            }

            stack.Children.Add(new TextBlock { Text = "Логин", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtLogin);
            stack.Children.Add(new TextBlock { Text = "Пароль", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtPassword);
            stack.Children.Add(new TextBlock { Text = "ФИО", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtFullName);
            stack.Children.Add(new TextBlock { Text = "Email", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtEmail);
            stack.Children.Add(new TextBlock { Text = "Роль", FontWeight = FontWeights.Bold });
            stack.Children.Add(cmbRole);
            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            Button btnSave = new Button { Content = "Сохранить", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 100, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Отмена", Width = 100, Height = 35 };
            btnPanel.Children.Add(btnSave);
            btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);
            dialog.Content = new Border { Child = stack, Background = System.Windows.Media.Brushes.White };
            btnSave.Click += (s, ev) =>
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password;
                string fullName = txtFullName.Text.Trim();
                string email = txtEmail.Text.Trim();
                string role = ((ComboBoxItem)cmbRole.SelectedItem)?.Tag?.ToString() ?? "Seller";
                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email))
                {
                    MessageBox.Show("Заполните все поля!");
                    return;
                }
                if (id == 0 && string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Введите пароль!");
                    return;
                }
                bool result;
                if (id == 0)
                    result = DatabaseHelper.AddUser(login, password, fullName, email, role);
                else
                    result = DatabaseHelper.UpdateUser(id, fullName, email, role, "Active");
                if (result)
                {
                    LoadUsers();
                    dialog.Close();
                }
                else
                    MessageBox.Show("Ошибка!");
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            dialog.ShowDialog();
        }

        private void BtnAddCategory_Click(object sender, RoutedEventArgs e)
        {
            AddEditCategoryDialog();
        }

        private void BtnEditCategory_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            AddEditCategoryDialog(id);
        }

        private void BtnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            if (MessageBox.Show("Удалить категорию?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.DeleteCategory(id);
                LoadCategories();
            }
        }

        private void AddEditCategoryDialog(int id = 0)
        {
            Window dialog = new Window();
            dialog.Title = id == 0 ? "Новая категория" : "Редактирование категории";
            dialog.Width = 450;
            dialog.Height = 350;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            StackPanel stack = new StackPanel { Margin = new Thickness(20) };
            TextBox txtName = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtDescription = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 80, TextWrapping = TextWrapping.Wrap };
            if (id > 0)
            {
                DataTable categories = DatabaseHelper.GetAllCategories();
                var category = categories.AsEnumerable().FirstOrDefault(r => r.Field<int>("categoryid") == id);
                if (category != null)
                {
                    txtName.Text = category.Field<string>("name") ?? "";
                    txtDescription.Text = category.Field<string>("description") ?? "";
                }
            }
            stack.Children.Add(new TextBlock { Text = "Название*", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtName);
            stack.Children.Add(new TextBlock { Text = "Описание", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtDescription);
            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
            Button btnSave = new Button { Content = "Сохранить", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 100, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Отмена", Width = 100, Height = 35 };
            btnPanel.Children.Add(btnSave);
            btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);
            dialog.Content = new Border { Child = stack, Background = System.Windows.Media.Brushes.White };
            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    MessageBox.Show("Введите название категории!");
                    return;
                }
                bool result;
                if (id == 0)
                    result = DatabaseHelper.AddCategory(txtName.Text, txtDescription.Text);
                else
                    result = DatabaseHelper.UpdateCategory(id, txtName.Text, txtDescription.Text);
                if (result)
                {
                    LoadCategories();
                    dialog.Close();
                }
                else
                    MessageBox.Show("Ошибка!");
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            dialog.ShowDialog();
        }

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            AddEditProductDialog();
        }

        private void BtnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            AddEditProductDialog(id);
        }

        private void BtnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            if (MessageBox.Show("Удалить товар?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.DeleteProduct(id);
                LoadProducts();
            }
        }

        private void AddEditProductDialog(int id = 0)
        {
            Window dialog = new Window();
            dialog.Title = id == 0 ? "Новый товар" : "Редактирование товара";
            dialog.Width = 500;
            dialog.Height = 650;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            StackPanel stack = new StackPanel { Margin = new Thickness(20) };
            TextBox txtName = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            ComboBox cmbCategory = new ComboBox { Margin = new Thickness(0, 5, 0, 15), Height = 35, DisplayMemberPath = "name", SelectedValuePath = "categoryid" };
            TextBox txtBrand = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtPrice = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtStock = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtUnit = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35, Text = "шт." };
            DatePicker dpExpiryDate = new DatePicker { Margin = new Thickness(0, 5, 0, 20), Height = 35 };
            DataTable categories = DatabaseHelper.GetAllCategories();
            cmbCategory.ItemsSource = categories.DefaultView;
            if (id > 0)
            {
                DataTable products = DatabaseHelper.GetAllProducts();
                var product = products.AsEnumerable().FirstOrDefault(r => r.Field<int>("productid") == id);
                if (product != null)
                {
                    txtName.Text = product.Field<string>("name") ?? "";
                    cmbCategory.SelectedValue = product.Field<int?>("categoryid");
                    txtBrand.Text = product.Field<string>("brand") ?? "";
                    txtPrice.Text = product.Field<decimal>("price").ToString();
                    txtStock.Text = product.Field<int>("stock").ToString();
                    txtUnit.Text = product.Field<string>("unit") ?? "шт.";
                    object expiryValue = product["expirydate"];
                    if (expiryValue != null && expiryValue != DBNull.Value)
                    {
                        DateTime expiryDate;
                        if (DateTime.TryParse(expiryValue.ToString(), out expiryDate))
                        {
                            dpExpiryDate.SelectedDate = expiryDate;
                        }
                    }
                }
            }
            stack.Children.Add(new TextBlock { Text = "Название*", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtName);
            stack.Children.Add(new TextBlock { Text = "Категория", FontWeight = FontWeights.Bold });
            stack.Children.Add(cmbCategory);
            stack.Children.Add(new TextBlock { Text = "Бренд", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtBrand);
            stack.Children.Add(new TextBlock { Text = "Цена*", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtPrice);
            stack.Children.Add(new TextBlock { Text = "Остаток*", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtStock);
            stack.Children.Add(new TextBlock { Text = "Единица измерения", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtUnit);
            stack.Children.Add(new TextBlock { Text = "Срок годности", FontWeight = FontWeights.Bold });
            stack.Children.Add(dpExpiryDate);
            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
            Button btnSave = new Button { Content = "Сохранить", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 100, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Отмена", Width = 100, Height = 35 };
            btnPanel.Children.Add(btnSave);
            btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);
            dialog.Content = new Border { Child = stack, Background = System.Windows.Media.Brushes.White };
            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    MessageBox.Show("Заполните обязательные поля!");
                    return;
                }
                if (!decimal.TryParse(txtPrice.Text, out decimal price))
                {
                    MessageBox.Show("Некорректная цена!");
                    return;
                }
                if (!int.TryParse(txtStock.Text, out int stock))
                {
                    MessageBox.Show("Некорректный остаток!");
                    return;
                }
                int? categoryId = cmbCategory.SelectedValue != null ? Convert.ToInt32(cmbCategory.SelectedValue) : (int?)null;
                DateTime? expiryDate = dpExpiryDate.SelectedDate;
                bool result;
                if (id == 0)
                    result = DatabaseHelper.AddProduct(txtName.Text, categoryId ?? 0, txtBrand.Text, price, stock, txtUnit.Text, expiryDate);
                else
                    result = DatabaseHelper.UpdateProduct(id, txtName.Text, categoryId ?? 0, txtBrand.Text, price, stock, txtUnit.Text, expiryDate);
                if (result)
                {
                    LoadProducts();
                    dialog.Close();
                }
                else
                    MessageBox.Show("Ошибка!");
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            dialog.ShowDialog();
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            AddEditOrderDialog();
        }

        private void BtnEditOrder_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            AddEditOrderDialog(id);
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            if (MessageBox.Show("Удалить заказ? Это действие нельзя отменить!", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                DatabaseHelper.DeleteOrder(id);
                LoadOrders();
            }
        }

        private void AddEditOrderDialog(int id = 0)
        {
            Window dialog = new Window();
            dialog.Title = id == 0 ? "Новый заказ" : "Редактирование заказа";
            dialog.Width = 450;
            dialog.Height = 500;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            StackPanel stack = new StackPanel { Margin = new Thickness(20) };

            TextBox txtOrderNumber = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            DatePicker dpOrderDate = new DatePicker { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtTotalAmount = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35, IsReadOnly = true, Background = System.Windows.Media.Brushes.LightGray };
            ComboBox cmbPaymentMethod = new ComboBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            cmbPaymentMethod.Items.Add(new ComboBoxItem { Content = "Наличные", Tag = "Cash" });
            cmbPaymentMethod.Items.Add(new ComboBoxItem { Content = "Карта", Tag = "Card" });
            cmbPaymentMethod.Items.Add(new ComboBoxItem { Content = "QR-код", Tag = "QR" });
            cmbPaymentMethod.SelectedIndex = 0;
            ComboBox cmbStatus = new ComboBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            cmbStatus.Items.Add(new ComboBoxItem { Content = "Pending", Tag = "Pending" });
            cmbStatus.Items.Add(new ComboBoxItem { Content = "Processing", Tag = "Processing" });
            cmbStatus.Items.Add(new ComboBoxItem { Content = "Completed", Tag = "Completed" });
            cmbStatus.Items.Add(new ComboBoxItem { Content = "Cancelled", Tag = "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            ComboBox cmbSeller = new ComboBox { Margin = new Thickness(0, 5, 0, 15), Height = 35, DisplayMemberPath = "fullname", SelectedValuePath = "userid" };
            cmbSeller.ItemsSource = DatabaseHelper.GetAllUsers().DefaultView;

            if (id > 0)
            {
                DataTable orders = DatabaseHelper.GetOrders();
                var order = orders.AsEnumerable().FirstOrDefault(r => r.Field<int>("orderid") == id);
                if (order != null)
                {
                    txtOrderNumber.Text = order.Field<string>("ordernumber") ?? "";
                    txtOrderNumber.IsEnabled = false;
                    dpOrderDate.SelectedDate = order.Field<DateTime>("orderdate");
                    txtTotalAmount.Text = order.Field<decimal>("totalamount").ToString("F2");
                    string payment = order.Field<string>("paymentmethod") ?? "Cash";
                    string status = order.Field<string>("status") ?? "Pending";
                    for (int i = 0; i < cmbPaymentMethod.Items.Count; i++)
                    {
                        if (((ComboBoxItem)cmbPaymentMethod.Items[i]).Tag.ToString() == payment)
                        { cmbPaymentMethod.SelectedIndex = i; break; }
                    }
                    for (int i = 0; i < cmbStatus.Items.Count; i++)
                    {
                        if (((ComboBoxItem)cmbStatus.Items[i]).Tag.ToString() == status)
                        { cmbStatus.SelectedIndex = i; break; }
                    }
                    int sellerId = order.Field<int?>("userid") ?? 0;
                    cmbSeller.SelectedValue = sellerId;
                }
            }

            stack.Children.Add(new TextBlock { Text = "Номер чека*", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtOrderNumber);
            stack.Children.Add(new TextBlock { Text = "Дата заказа", FontWeight = FontWeights.Bold });
            stack.Children.Add(dpOrderDate);
            stack.Children.Add(new TextBlock { Text = "Сумма", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtTotalAmount);
            stack.Children.Add(new TextBlock { Text = "Способ оплаты", FontWeight = FontWeights.Bold });
            stack.Children.Add(cmbPaymentMethod);
            stack.Children.Add(new TextBlock { Text = "Статус", FontWeight = FontWeights.Bold });
            stack.Children.Add(cmbStatus);
            stack.Children.Add(new TextBlock { Text = "Продавец", FontWeight = FontWeights.Bold });
            stack.Children.Add(cmbSeller);

            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
            Button btnSave = new Button { Content = "Сохранить", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 100, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Отмена", Width = 100, Height = 35 };
            btnPanel.Children.Add(btnSave);
            btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);
            dialog.Content = new Border { Child = stack, Background = System.Windows.Media.Brushes.White };

            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrEmpty(txtOrderNumber.Text))
                {
                    MessageBox.Show("Введите номер чека!");
                    return;
                }
                bool result;
                if (id == 0)
                {
                    result = DatabaseHelper.AddOrder(txtOrderNumber.Text, dpOrderDate.SelectedDate ?? DateTime.Now,
                        decimal.Parse(txtTotalAmount.Text), ((ComboBoxItem)cmbPaymentMethod.SelectedItem)?.Tag?.ToString() ?? "Cash",
                        ((ComboBoxItem)cmbStatus.SelectedItem)?.Tag?.ToString() ?? "Pending",
                        cmbSeller.SelectedValue != null ? Convert.ToInt32(cmbSeller.SelectedValue) : 0);
                }
                else
                {
                    result = DatabaseHelper.UpdateOrder(id, dpOrderDate.SelectedDate ?? DateTime.Now,
                        ((ComboBoxItem)cmbPaymentMethod.SelectedItem)?.Tag?.ToString() ?? "Cash",
                        ((ComboBoxItem)cmbStatus.SelectedItem)?.Tag?.ToString() ?? "Pending",
                        cmbSeller.SelectedValue != null ? Convert.ToInt32(cmbSeller.SelectedValue) : 0);
                }
                if (result)
                {
                    LoadOrders();
                    dialog.Close();
                }
                else
                    MessageBox.Show("Ошибка!");
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            dialog.ShowDialog();
        }

        private void BtnAddSupplier_Click(object sender, RoutedEventArgs e)
        {
            AddEditSupplierDialog();
        }

        private void BtnEditSupplier_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            AddEditSupplierDialog(id);
        }

        private void BtnDeleteSupplier_Click(object sender, RoutedEventArgs e)
        {
            int id = Convert.ToInt32(((Button)sender).Tag);
            if (MessageBox.Show("Удалить поставщика?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    DatabaseHelper.DeleteSupplier(id);
                    LoadSuppliers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddEditSupplierDialog(int id = 0)
        {
            Window dialog = new Window();
            dialog.Title = id == 0 ? "Новый поставщик" : "Редактирование поставщика";
            dialog.Width = 450;
            dialog.Height = 550;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            StackPanel stack = new StackPanel { Margin = new Thickness(20) };
            TextBox txtName = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtContact = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtPhone = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtEmail = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtAddress = new TextBox { Margin = new Thickness(0, 5, 0, 20), Height = 35 };
            if (id > 0)
            {
                DataTable suppliers = DatabaseHelper.GetAllSuppliers();
                var supplier = suppliers.AsEnumerable().FirstOrDefault(r => r.Field<int>("supplierid") == id);
                if (supplier != null)
                {
                    txtName.Text = supplier.Field<string>("name") ?? "";
                    txtContact.Text = supplier.Field<string>("contactperson") ?? "";
                    txtPhone.Text = supplier.Field<string>("phone") ?? "";
                    txtEmail.Text = supplier.Field<string>("email") ?? "";
                    txtAddress.Text = supplier.Field<string>("address") ?? "";
                }
            }
            stack.Children.Add(new TextBlock { Text = "Название*", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtName);
            stack.Children.Add(new TextBlock { Text = "Контактное лицо", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtContact);
            stack.Children.Add(new TextBlock { Text = "Телефон", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtPhone);
            stack.Children.Add(new TextBlock { Text = "Email", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtEmail);
            stack.Children.Add(new TextBlock { Text = "Адрес", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtAddress);
            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 20, 0, 0) };
            Button btnSave = new Button { Content = "Сохранить", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 100, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Отмена", Width = 100, Height = 35 };
            btnPanel.Children.Add(btnSave);
            btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);
            dialog.Content = new Border { Child = stack, Background = System.Windows.Media.Brushes.White };
            btnSave.Click += (s, ev) =>
            {
                if (string.IsNullOrEmpty(txtName.Text))
                {
                    MessageBox.Show("Введите название поставщика!");
                    return;
                }
                bool result;
                if (id == 0)
                    result = DatabaseHelper.AddSupplier(txtName.Text, txtContact.Text, txtPhone.Text, txtEmail.Text, txtAddress.Text);
                else
                    result = DatabaseHelper.UpdateSupplier(id, txtName.Text, txtContact.Text, txtPhone.Text, txtEmail.Text, txtAddress.Text);
                if (result)
                {
                    LoadSuppliers();
                    dialog.Close();
                }
                else
                    MessageBox.Show("Ошибка!");
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            dialog.ShowDialog();
        }

        private void BtnNewSupply_Click(object sender, RoutedEventArgs e)
        {
            AddSupplyDialog();
        }

        private void AddSupplyDialog()
        {
            Window dialog = new Window();
            dialog.Title = "Новая поставка";
            dialog.Width = 650;
            dialog.Height = 700;
            dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            dialog.Owner = this;
            dialog.ResizeMode = ResizeMode.NoResize;
            ScrollViewer scrollViewer = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            StackPanel stack = new StackPanel { Margin = new Thickness(20) };
            ComboBox cmbSupplier = new ComboBox { Margin = new Thickness(0, 5, 0, 15), Height = 35, DisplayMemberPath = "name", SelectedValuePath = "supplierid" };
            cmbSupplier.ItemsSource = DatabaseHelper.GetAllSuppliers().DefaultView;
            DataGrid dgSupplyItems = new DataGrid { Margin = new Thickness(0, 5, 0, 15), Height = 200, AutoGenerateColumns = true };
            DataTable supplyCart = new DataTable();
            supplyCart.Columns.Add("ProductID", typeof(int));
            supplyCart.Columns.Add("ProductName", typeof(string));
            supplyCart.Columns.Add("Quantity", typeof(int));
            supplyCart.Columns.Add("Price", typeof(decimal));
            supplyCart.Columns.Add("TotalPrice", typeof(decimal));
            dgSupplyItems.ItemsSource = supplyCart.DefaultView;
            ComboBox cmbProduct = new ComboBox { Margin = new Thickness(0, 5, 0, 15), Height = 35, DisplayMemberPath = "name", SelectedValuePath = "productid" };
            cmbProduct.ItemsSource = DatabaseHelper.GetAllProducts().DefaultView;
            TextBox txtQuantity = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            TextBox txtPrice = new TextBox { Margin = new Thickness(0, 5, 0, 15), Height = 35 };
            StackPanel addPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            Button btnAddItem = new Button { Content = "Добавить товар", Width = 120, Height = 35, Margin = new Thickness(5) };
            addPanel.Children.Add(btnAddItem);
            stack.Children.Add(new TextBlock { Text = "Поставщик", FontWeight = FontWeights.Bold });
            stack.Children.Add(cmbSupplier);
            stack.Children.Add(new TextBlock { Text = "Товар", FontWeight = FontWeights.Bold });
            stack.Children.Add(cmbProduct);
            stack.Children.Add(new TextBlock { Text = "Количество", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtQuantity);
            stack.Children.Add(new TextBlock { Text = "Цена закупки", FontWeight = FontWeights.Bold });
            stack.Children.Add(txtPrice);
            stack.Children.Add(addPanel);
            stack.Children.Add(new TextBlock { Text = "Товары в поставке", FontWeight = FontWeights.Bold });
            stack.Children.Add(dgSupplyItems);
            StackPanel btnPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 10, 0, 0) };
            Button btnSave = new Button { Content = "Оформить поставку", Background = System.Windows.Media.Brushes.Green, Foreground = System.Windows.Media.Brushes.White, Width = 120, Height = 35, Margin = new Thickness(0, 0, 10, 0) };
            Button btnCancel = new Button { Content = "Отмена", Width = 100, Height = 35 };
            btnPanel.Children.Add(btnSave);
            btnPanel.Children.Add(btnCancel);
            stack.Children.Add(btnPanel);
            scrollViewer.Content = stack;
            dialog.Content = new Border { Child = scrollViewer, Background = System.Windows.Media.Brushes.White };
            btnAddItem.Click += (s, ev) =>
            {
                if (cmbProduct.SelectedValue == null)
                {
                    MessageBox.Show("Выберите товар!");
                    return;
                }
                if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
                {
                    MessageBox.Show("Введите корректное количество!");
                    return;
                }
                if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
                {
                    MessageBox.Show("Введите корректную цену!");
                    return;
                }
                int productId = Convert.ToInt32(cmbProduct.SelectedValue);
                string productName = cmbProduct.Text;
                decimal total = qty * price;
                supplyCart.Rows.Add(productId, productName, qty, price, total);
                txtQuantity.Clear();
                txtPrice.Clear();
            };
            btnSave.Click += (s, ev) =>
            {
                if (supplyCart.Rows.Count == 0)
                {
                    MessageBox.Show("Добавьте хотя бы один товар!");
                    return;
                }
                string supplyNumber = $"ПОСТ-{DateTime.Now:yyyyMMdd-HHmmss}";
                int supplierId = cmbSupplier.SelectedValue != null ? Convert.ToInt32(cmbSupplier.SelectedValue) : 0;
                try
                {
                    DatabaseHelper.CreateSupply(supplyNumber, supplierId, App.CurrentUserId, supplyCart);
                    MessageBox.Show("Поставка оформлена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadSupplyHistory();
                    LoadProducts();
                    LoadLowStock();
                    dialog.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            btnCancel.Click += (s, ev) => dialog.Close();
            dialog.ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Выйти из системы?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
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