using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Npgsql;

namespace MMarketApp.Database
{
    public static class DatabaseHelper
    {
        private static readonly string? connectionString = ConfigurationManager.ConnectionStrings["MMarketDB"]?.ConnectionString;

        static DatabaseHelper()
        {
            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Connection string 'MMarketDB' not found in configuration");
        }

        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(connectionString);
        }

        public static DataTable ExecuteQuery(string query, NpgsqlParameter[]? parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                using (var da = new NpgsqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
         

        public static int ExecuteNonQuery(string query, NpgsqlParameter[]? parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public static object? ExecuteScalar(string query, NpgsqlParameter[]? parameters = null)
        {
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(query, conn))
            {
                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);
                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

        public static bool Login(string login, string password, out string role, out string fullName, out int userId)
        {
            role = "";
            fullName = "";
            userId = 0;
            string query = "SELECT userid, fullname, role FROM users WHERE login = @Login AND password = @Password AND status = 'Active'";
            var parameters = new[]
            {
                new NpgsqlParameter("@Login", login),
                new NpgsqlParameter("@Password", password)
            };
            var dt = ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                userId = Convert.ToInt32(dt.Rows[0]["userid"]);
                fullName = dt.Rows[0]["fullname"].ToString() ?? "";
                role = dt.Rows[0]["role"].ToString() ?? "";
                return true;
            }
            return false;
        }

        public static int StartShift(int userId)
        {
            string query = "INSERT INTO shifts (userid, starttime, status) VALUES (@UserID, CURRENT_TIMESTAMP, 'Active') RETURNING shiftid;";
            var parameters = new[] { new NpgsqlParameter("@UserID", userId) };
            return Convert.ToInt32(ExecuteScalar(query, parameters));
        }

        public static bool EndShift(int shiftId)
        {
            string query = "UPDATE shifts SET endtime = CURRENT_TIMESTAMP, status = 'Closed' WHERE shiftid = @ShiftID AND status = 'Active'";
            var parameters = new[] { new NpgsqlParameter("@ShiftID", shiftId) };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static DataTable GetAllProducts()
        {
            string query = @"SELECT p.productid, p.article, p.name, p.categoryid, c.name AS CategoryName, p.brand, p.price, p.stock, p.unit, p.expirydate
                             FROM products p LEFT JOIN categories c ON p.categoryid = c.categoryid ORDER BY p.name";
            return ExecuteQuery(query);
        }

        public static DataTable SearchProducts(string searchTerm)
        {
            string query = @"SELECT p.productid, p.article, p.name, p.categoryid, c.name AS CategoryName, p.brand, p.price, p.stock, p.unit, p.expirydate
                             FROM products p LEFT JOIN categories c ON p.categoryid = c.categoryid
                             WHERE p.name ILIKE @Search OR p.article ILIKE @Search OR p.brand ILIKE @Search ORDER BY p.name";
            var parameters = new[] { new NpgsqlParameter("@Search", "%" + searchTerm + "%") };
            return ExecuteQuery(query, parameters);
        }

        public static DataTable GetProductsByCategory(int categoryId)
        {
            string query = @"SELECT p.productid, p.article, p.name, p.categoryid, c.name AS CategoryName, p.brand, p.price, p.stock, p.unit, p.expirydate
                             FROM products p LEFT JOIN categories c ON p.categoryid = c.categoryid WHERE p.categoryid = @CategoryID";
            var parameters = new[] { new NpgsqlParameter("@CategoryID", categoryId) };
            return ExecuteQuery(query, parameters);
        }

        public static DataTable GetAllCategories()
        {
            string query = "SELECT categoryid, name, description FROM categories ORDER BY name";
            return ExecuteQuery(query);
        }

        public static DataTable GetOrders(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            string query = @"SELECT o.orderid, o.ordernumber, o.orderdate, o.customername, o.totalamount, o.paymentmethod, o.status, u.fullname AS SellerName
                             FROM orders o JOIN users u ON o.userid = u.userid WHERE 1=1";
            var parameters = new List<NpgsqlParameter>();
            if (dateFrom.HasValue)
            {
                query += " AND o.orderdate::DATE >= @DateFrom";
                parameters.Add(new NpgsqlParameter("@DateFrom", dateFrom.Value));
            }
            if (dateTo.HasValue)
            {
                query += " AND o.orderdate::DATE <= @DateTo";
                parameters.Add(new NpgsqlParameter("@DateTo", dateTo.Value));
            }
            query += " ORDER BY o.orderdate DESC";
            return ExecuteQuery(query, parameters.ToArray());
        }

        public static DataTable GetOrderDetails(int orderId)
        {
            string query = @"SELECT od.productid, p.name AS ProductName, od.quantity, od.unitprice, od.totalprice
                             FROM orderdetails od JOIN products p ON od.productid = p.productid WHERE od.orderid = @OrderID";
            var parameters = new[] { new NpgsqlParameter("@OrderID", orderId) };
            return ExecuteQuery(query, parameters);
        }

        public static bool CreateOrder(string orderNumber, int userId, string customerName, string paymentMethod, DataTable cartItems)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        decimal totalAmount = 0;
                        foreach (DataRow row in cartItems.Rows)
                            totalAmount += Convert.ToDecimal(row["Total"]);

                        string orderQuery = @"INSERT INTO orders (ordernumber, userid, customername, paymentmethod, totalamount, status)
                                              VALUES (@OrderNumber, @UserID, @CustomerName, @PaymentMethod, @TotalAmount, 'Completed')
                                              RETURNING orderid;";
                        int orderId;
                        using (var orderCmd = new NpgsqlCommand(orderQuery, conn, transaction))
                        {
                            orderCmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                            orderCmd.Parameters.AddWithValue("@UserID", userId);
                            orderCmd.Parameters.AddWithValue("@CustomerName", string.IsNullOrEmpty(customerName) ? "Гость" : customerName);
                            orderCmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                            orderCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            orderId = Convert.ToInt32(orderCmd.ExecuteScalar());
                        }

                        foreach (DataRow row in cartItems.Rows)
                        {
                            string detailQuery = @"INSERT INTO orderdetails (orderid, productid, quantity, unitprice, totalprice)
                                                   VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice, @TotalPrice)";
                            using (var detailCmd = new NpgsqlCommand(detailQuery, conn, transaction))
                            {
                                detailCmd.Parameters.AddWithValue("@OrderID", orderId);
                                detailCmd.Parameters.AddWithValue("@ProductID", Convert.ToInt32(row["ProductID"]));
                                detailCmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(row["Quantity"]));
                                detailCmd.Parameters.AddWithValue("@UnitPrice", Convert.ToDecimal(row["Price"]));
                                detailCmd.Parameters.AddWithValue("@TotalPrice", Convert.ToDecimal(row["Total"]));
                                detailCmd.ExecuteNonQuery();
                            }

                            string updateStockQuery = "UPDATE products SET stock = stock - @Quantity WHERE productid = @ProductID";
                            using (var stockCmd = new NpgsqlCommand(updateStockQuery, conn, transaction))
                            {
                                stockCmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(row["Quantity"]));
                                stockCmd.Parameters.AddWithValue("@ProductID", Convert.ToInt32(row["ProductID"]));
                                stockCmd.ExecuteNonQuery();
                            }
                        }

                        string updateShiftQuery = @"UPDATE shifts SET totalorders = totalorders + 1, totalamount = totalamount + @TotalAmount 
                                                   WHERE userid = @UserID AND status = 'Active'";
                        using (var shiftCmd = new NpgsqlCommand(updateShiftQuery, conn, transaction))
                        {
                            shiftCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            shiftCmd.Parameters.AddWithValue("@UserID", userId);
                            shiftCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public static DataTable GetUserShifts(int userId)
        {
            string query = "SELECT shiftid, starttime, endtime, totalorders, totalamount, status FROM shifts WHERE userid = @UserID ORDER BY shiftid DESC";
            var parameters = new[] { new NpgsqlParameter("@UserID", userId) };
            return ExecuteQuery(query, parameters);
        }

        public static DataTable GetCurrentShiftStats(int shiftId)
        {
            string query = @"SELECT starttime, totalorders, totalamount, CASE WHEN totalorders > 0 THEN totalamount / totalorders ELSE 0 END AS AvgOrder
                             FROM shifts WHERE shiftid = @ShiftID";
            var parameters = new[] { new NpgsqlParameter("@ShiftID", shiftId) };
            return ExecuteQuery(query, parameters);
        }

        public static DataTable GetSalesStats(DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            string query = @"SELECT COALESCE(SUM(od.quantity), 0) AS TotalItems, COALESCE(SUM(od.totalprice), 0) AS TotalRevenue,
                             COUNT(DISTINCT o.orderid) AS TotalOrders, COALESCE(AVG(o.totalamount), 0) AS AvgOrderValue
                             FROM orders o LEFT JOIN orderdetails od ON o.orderid = od.orderid WHERE o.status = 'Completed'";
            var parameters = new List<NpgsqlParameter>();
            if (dateFrom.HasValue)
            {
                query += " AND o.orderdate::DATE >= @DateFrom";
                parameters.Add(new NpgsqlParameter("@DateFrom", dateFrom.Value));
            }
            if (dateTo.HasValue)
            {
                query += " AND o.orderdate::DATE <= @DateTo";
                parameters.Add(new NpgsqlParameter("@DateTo", dateTo.Value));
            }
            return ExecuteQuery(query, parameters.ToArray());
        }

        public static DataTable GetAllUsers()
        {
            string query = "SELECT userid, login, fullname, email, role, status, createdat FROM users ORDER BY userid";
            return ExecuteQuery(query);
        }

        public static bool AddUser(string login, string password, string fullName, string email, string role)
        {
            string query = @"INSERT INTO users (login, password, fullname, email, role, status) VALUES (@Login, @Password, @FullName, @Email, @Role, 'Active')";
            var parameters = new[]
            {
                new NpgsqlParameter("@Login", login),
                new NpgsqlParameter("@Password", password),
                new NpgsqlParameter("@FullName", fullName),
                new NpgsqlParameter("@Email", email),
                new NpgsqlParameter("@Role", role)
            };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool UpdateUser(int userId, string fullName, string email, string role, string status)
        {
            string query = "UPDATE users SET fullname = @FullName, email = @Email, role = @Role, status = @Status WHERE userid = @UserID";
            var parameters = new[]
            {
                new NpgsqlParameter("@UserID", userId),
                new NpgsqlParameter("@FullName", fullName),
                new NpgsqlParameter("@Email", email),
                new NpgsqlParameter("@Role", role),
                new NpgsqlParameter("@Status", status)
            };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool DeleteUser(int userId)
        {
            string query = "DELETE FROM users WHERE userid = @UserID AND role != 'Admin'";
            var parameters = new[] { new NpgsqlParameter("@UserID", userId) };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool AddProduct(string name, int categoryId, string brand, decimal price, int stock, string unit, DateTime? expiryDate = null)
        {
            string query = @"INSERT INTO products (name, categoryid, brand, price, stock, unit, expirydate) VALUES (@Name, @CategoryID, @Brand, @Price, @Stock, @Unit, @ExpiryDate)";
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@CategoryID", categoryId > 0 ? categoryId : DBNull.Value),
                new NpgsqlParameter("@Brand", string.IsNullOrEmpty(brand) ? DBNull.Value : brand),
                new NpgsqlParameter("@Price", price),
                new NpgsqlParameter("@Stock", stock),
                new NpgsqlParameter("@Unit", string.IsNullOrEmpty(unit) ? "шт." : unit),
                new NpgsqlParameter("@ExpiryDate", expiryDate.HasValue ? expiryDate.Value : DBNull.Value)
            };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool UpdateProduct(int productId, string name, int categoryId, string brand, decimal price, int stock, string unit, DateTime? expiryDate = null)
        {
            string query = @"UPDATE products SET name = @Name, categoryid = @CategoryID, brand = @Brand, price = @Price, stock = @Stock, unit = @Unit, expirydate = @ExpiryDate WHERE productid = @ProductID";
            var parameters = new[]
            {
                new NpgsqlParameter("@ProductID", productId),
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@CategoryID", categoryId > 0 ? categoryId : DBNull.Value),
                new NpgsqlParameter("@Brand", string.IsNullOrEmpty(brand) ? DBNull.Value : brand),
                new NpgsqlParameter("@Price", price),
                new NpgsqlParameter("@Stock", stock),
                new NpgsqlParameter("@Unit", string.IsNullOrEmpty(unit) ? "шт." : unit),
                new NpgsqlParameter("@ExpiryDate", expiryDate.HasValue ? expiryDate.Value : DBNull.Value)
            };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool DeleteProduct(int productId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Сначала удаляем связанные записи из orderdetails
                        string deleteDetailsQuery = "DELETE FROM orderdetails WHERE productid = @ProductID";
                        using (var detailCmd = new NpgsqlCommand(deleteDetailsQuery, conn, transaction))
                        {
                            detailCmd.Parameters.AddWithValue("@ProductID", productId);
                            detailCmd.ExecuteNonQuery();
                        }

                        // Затем удаляем сам товар
                        string deleteProductQuery = "DELETE FROM products WHERE productid = @ProductID";
                        using (var productCmd = new NpgsqlCommand(deleteProductQuery, conn, transaction))
                        {
                            productCmd.Parameters.AddWithValue("@ProductID", productId);
                            productCmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public static DataTable GetLowStockProducts(int threshold = 10)
        {
            string query = @"SELECT p.productid, p.name, c.name AS CategoryName, p.stock, p.unit,
                                    CASE WHEN p.stock <= @Threshold THEN 'Критический' WHEN p.stock <= @Threshold * 2 THEN 'Низкий' ELSE 'Нормальный' END AS StockStatus
                             FROM products p LEFT JOIN categories c ON p.categoryid = c.categoryid
                             WHERE p.stock <= @Threshold * 2 ORDER BY p.stock";
            var parameters = new[] { new NpgsqlParameter("@Threshold", threshold) };
            return ExecuteQuery(query, parameters);
        }

        public static DataTable GetAllSuppliers()
        {
            string query = "SELECT supplierid, name, contactperson, phone, email, address, createdat FROM suppliers ORDER BY name";
            return ExecuteQuery(query);
        }

        public static bool AddSupplier(string name, string contactPerson, string phone, string email, string address)
        {
            string query = @"INSERT INTO suppliers (name, contactperson, phone, email, address) VALUES (@Name, @ContactPerson, @Phone, @Email, @Address)";
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@ContactPerson", string.IsNullOrEmpty(contactPerson) ? DBNull.Value : contactPerson),
                new NpgsqlParameter("@Phone", string.IsNullOrEmpty(phone) ? DBNull.Value : phone),
                new NpgsqlParameter("@Email", string.IsNullOrEmpty(email) ? DBNull.Value : email),
                new NpgsqlParameter("@Address", string.IsNullOrEmpty(address) ? DBNull.Value : address)
            };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool UpdateSupplier(int id, string name, string contactPerson, string phone, string email, string address)
        {
            string query = @"UPDATE suppliers SET name = @Name, contactperson = @ContactPerson, phone = @Phone, email = @Email, address = @Address WHERE supplierid = @Id";
            var parameters = new[]
            {
                new NpgsqlParameter("@Id", id),
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@ContactPerson", string.IsNullOrEmpty(contactPerson) ? DBNull.Value : contactPerson),
                new NpgsqlParameter("@Phone", string.IsNullOrEmpty(phone) ? DBNull.Value : phone),
                new NpgsqlParameter("@Email", string.IsNullOrEmpty(email) ? DBNull.Value : email),
                new NpgsqlParameter("@Address", string.IsNullOrEmpty(address) ? DBNull.Value : address)
            };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool DeleteSupplier(int id)
        {
            string checkQuery = "SELECT COUNT(*) FROM supply WHERE supplierid = @Id";
            var checkParams = new[] { new NpgsqlParameter("@Id", id) };
            var count = Convert.ToInt64(ExecuteScalar(checkQuery, checkParams));
            if (count > 0)
                throw new Exception("Невозможно удалить поставщика, так как есть связанные поставки");
            string query = "DELETE FROM suppliers WHERE supplierid = @Id";
            var parameters = new[] { new NpgsqlParameter("@Id", id) };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static DataTable GetSupplyHistory()
        {
            string query = @"SELECT s.supplyid, s.supplynumber, s.supplydate, sup.name AS Supplier, u.fullname AS UserName, s.totalamount, s.status
                             FROM supply s LEFT JOIN suppliers sup ON s.supplierid = sup.supplierid LEFT JOIN users u ON s.userid = u.userid ORDER BY s.supplydate DESC";
            return ExecuteQuery(query);
        }

        public static DataTable GetSupplyDetails(int supplyId)
        {
            string query = @"SELECT sd.productid, p.name AS ProductName, sd.quantity, sd.price, sd.totalprice
                             FROM supplydetails sd JOIN products p ON sd.productid = p.productid WHERE sd.supplyid = @SupplyID";
            var parameters = new[] { new NpgsqlParameter("@SupplyID", supplyId) };
            return ExecuteQuery(query, parameters);
        }

        public static bool CreateSupply(string supplyNumber, int supplierId, int userId, DataTable supplyItems)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        decimal totalAmount = 0;
                        foreach (DataRow row in supplyItems.Rows)
                            totalAmount += Convert.ToDecimal(row["TotalPrice"]);

                        string supplyQuery = @"INSERT INTO supply (supplynumber, supplierid, userid, totalamount, status)
                                               VALUES (@SupplyNumber, @SupplierID, @UserID, @TotalAmount, 'Completed')
                                               RETURNING supplyid;";
                        int supplyId;
                        using (var supplyCmd = new NpgsqlCommand(supplyQuery, conn, transaction))
                        {
                            supplyCmd.Parameters.AddWithValue("@SupplyNumber", supplyNumber);
                            supplyCmd.Parameters.AddWithValue("@SupplierID", supplierId > 0 ? supplierId : DBNull.Value);
                            supplyCmd.Parameters.AddWithValue("@UserID", userId);
                            supplyCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            supplyId = Convert.ToInt32(supplyCmd.ExecuteScalar());
                        }

                        foreach (DataRow row in supplyItems.Rows)
                        {
                            string detailQuery = @"INSERT INTO supplydetails (supplyid, productid, quantity, price, totalprice)
                                                   VALUES (@SupplyID, @ProductID, @Quantity, @Price, @TotalPrice)";
                            using (var detailCmd = new NpgsqlCommand(detailQuery, conn, transaction))
                            {
                                detailCmd.Parameters.AddWithValue("@SupplyID", supplyId);
                                detailCmd.Parameters.AddWithValue("@ProductID", Convert.ToInt32(row["ProductID"]));
                                detailCmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(row["Quantity"]));
                                detailCmd.Parameters.AddWithValue("@Price", Convert.ToDecimal(row["Price"]));
                                detailCmd.Parameters.AddWithValue("@TotalPrice", Convert.ToDecimal(row["TotalPrice"]));
                                detailCmd.ExecuteNonQuery();
                            }

                            string updateStockQuery = "UPDATE products SET stock = stock + @Quantity WHERE productid = @ProductID";
                            using (var stockCmd = new NpgsqlCommand(updateStockQuery, conn, transaction))
                            {
                                stockCmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(row["Quantity"]));
                                stockCmd.Parameters.AddWithValue("@ProductID", Convert.ToInt32(row["ProductID"]));
                                stockCmd.ExecuteNonQuery();
                            }
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public static DataTable SearchSuppliers(string searchTerm)
        {
            string query = @"SELECT supplierid, name, contactperson, phone, email FROM suppliers WHERE name ILIKE @Search OR contactperson ILIKE @Search OR phone ILIKE @Search ORDER BY name";
            var parameters = new[] { new NpgsqlParameter("@Search", "%" + searchTerm + "%") };
            return ExecuteQuery(query, parameters);
        }

        public static bool AddCategory(string name, string description)
        {
            string query = @"INSERT INTO categories (name, description) VALUES (@Name, @Description)";
            var parameters = new[]
            {
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@Description", string.IsNullOrEmpty(description) ? DBNull.Value : description)
            };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool UpdateCategory(int id, string name, string description)
        {
            string query = @"UPDATE categories SET name = @Name, description = @Description WHERE categoryid = @Id";
            var parameters = new[]
            {
                new NpgsqlParameter("@Id", id),
                new NpgsqlParameter("@Name", name),
                new NpgsqlParameter("@Description", string.IsNullOrEmpty(description) ? DBNull.Value : description)
            };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool DeleteCategory(int id)
        {
            string query = "DELETE FROM categories WHERE categoryid = @Id";
            var parameters = new[] { new NpgsqlParameter("@Id", id) };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public class ShiftInfo
        {
            public int ShiftId { get; set; }
            public DateTime StartTime { get; set; }
            public string Status { get; set; }
        }

        public static bool AddOrder(string orderNumber, DateTime orderDate, decimal totalAmount, string paymentMethod, string status, int userId)
        {
            string query = @"INSERT INTO orders (ordernumber, orderdate, totalamount, paymentmethod, status, userid) 
                     VALUES (@OrderNumber, @OrderDate, @TotalAmount, @PaymentMethod, @Status, @UserID)";
            var parameters = new[]
            {
        new NpgsqlParameter("@OrderNumber", orderNumber),
        new NpgsqlParameter("@OrderDate", orderDate),
        new NpgsqlParameter("@TotalAmount", totalAmount),
        new NpgsqlParameter("@PaymentMethod", paymentMethod),
        new NpgsqlParameter("@Status", status),
        new NpgsqlParameter("@UserID", userId)
    };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool UpdateOrder(int orderId, DateTime orderDate, string paymentMethod, string status, int userId)
        {
            string query = @"UPDATE orders SET orderdate = @OrderDate, paymentmethod = @PaymentMethod, 
                     status = @Status, userid = @UserID WHERE orderid = @OrderID";
            var parameters = new[]
            {
        new NpgsqlParameter("@OrderID", orderId),
        new NpgsqlParameter("@OrderDate", orderDate),
        new NpgsqlParameter("@PaymentMethod", paymentMethod),
        new NpgsqlParameter("@Status", status),
        new NpgsqlParameter("@UserID", userId)
    };
            return ExecuteNonQuery(query, parameters) > 0;
        }

        public static bool DeleteOrder(int orderId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var deleteDetailsQuery = "DELETE FROM orderdetails WHERE orderid = @OrderID";
                        using (var cmd = new NpgsqlCommand(deleteDetailsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.ExecuteNonQuery();
                        }
                        var deleteOrderQuery = "DELETE FROM orders WHERE orderid = @OrderID";
                        using (var cmd = new NpgsqlCommand(deleteOrderQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
        public static ShiftInfo GetActiveShift(int userId)
        {
            string query = "SELECT shiftid, starttime, status FROM shifts WHERE userid = @UserID AND status = 'Active' ORDER BY shiftid DESC LIMIT 1";
            var parameters = new[] { new NpgsqlParameter("@UserID", userId) };
            var dt = ExecuteQuery(query, parameters);

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                return new ShiftInfo
                {
                    ShiftId = Convert.ToInt32(row["shiftid"]),
                    StartTime = Convert.ToDateTime(row["starttime"]),
                    Status = row["status"].ToString()
                };
            }
            return null;
        }

        public static bool ReturnOrder(int orderId, int userId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var details = GetOrderDetails(orderId);
                        foreach (DataRow row in details.Rows)
                        {
                            int productId = Convert.ToInt32(row["productid"]);
                            int quantity = Convert.ToInt32(row["quantity"]);
                            string updateStockQuery = "UPDATE products SET stock = stock + @Quantity WHERE productid = @ProductID";
                            using (var stockCmd = new NpgsqlCommand(updateStockQuery, conn, transaction))
                            {
                                stockCmd.Parameters.AddWithValue("@Quantity", quantity);
                                stockCmd.Parameters.AddWithValue("@ProductID", productId);
                                stockCmd.ExecuteNonQuery();
                            }
                        }
                        string updateOrderQuery = "UPDATE orders SET status = 'Cancelled' WHERE orderid = @OrderID";
                        using (var orderCmd = new NpgsqlCommand(updateOrderQuery, conn, transaction))
                        {
                            orderCmd.Parameters.AddWithValue("@OrderID", orderId);
                            orderCmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public static DataTable GetExpiredProducts()
        {
            string query = @"SELECT p.productid, p.name, c.name AS CategoryName, 
                            p.stock, p.unit, p.expirydate,
                            CASE 
                                WHEN p.expirydate IS NOT NULL AND p.createdat IS NOT NULL 
                                     AND (p.createdat + (p.expirydate || ' days')::interval) < CURRENT_DATE THEN 'Просрочен'
                                WHEN p.expirydate IS NOT NULL AND p.createdat IS NOT NULL 
                                     AND (p.createdat + (p.expirydate || ' days')::interval) <= CURRENT_DATE + INTERVAL '3 days' THEN 'Скоро истекает'
                                ELSE 'Нормальный'
                            END AS Status
                     FROM products p
                     LEFT JOIN categories c ON p.categoryid = c.categoryid
                     WHERE p.expirydate IS NOT NULL 
                       AND p.createdat IS NOT NULL
                       AND (p.createdat + (p.expirydate || ' days')::interval) <= CURRENT_DATE + INTERVAL '3 days'
                     ORDER BY p.expirydate";
            return ExecuteQuery(query);
        }

        // Получение остатка товара
        public static int GetProductStock(int productId)
        {
            string query = "SELECT stock FROM products WHERE productid = @ProductID";
            NpgsqlParameter[] parameters = { new NpgsqlParameter("@ProductID", productId) };
            var result = ExecuteScalar(query, parameters);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }

        public static bool CreateLayawayOrder(string orderNumber, int userId, string paymentMethod, DataTable cartItems)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        decimal totalAmount = 0;
                        foreach (DataRow row in cartItems.Rows)
                            totalAmount += Convert.ToDecimal(row["Total"]);

                        string orderQuery = @"INSERT INTO layawayorders (ordernumber, userid, paymentmethod, totalamount, status)
                                              VALUES (@OrderNumber, @UserID, @PaymentMethod, @TotalAmount, 'Layaway')
                                              RETURNING orderid;";
                        int orderId;
                        using (var orderCmd = new NpgsqlCommand(orderQuery, conn, transaction))
                        {
                            orderCmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                            orderCmd.Parameters.AddWithValue("@UserID", userId);
                            orderCmd.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                            orderCmd.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            orderId = Convert.ToInt32(orderCmd.ExecuteScalar());
                        }

                        foreach (DataRow row in cartItems.Rows)
                        {
                            string detailQuery = @"INSERT INTO layawayorderdetails (orderid, productid, productname, quantity, unitprice, totalprice)
                                                   VALUES (@OrderID, @ProductID, @ProductName, @Quantity, @UnitPrice, @TotalPrice)";
                            using (var detailCmd = new NpgsqlCommand(detailQuery, conn, transaction))
                            {
                                detailCmd.Parameters.AddWithValue("@OrderID", orderId);
                                detailCmd.Parameters.AddWithValue("@ProductID", Convert.ToInt32(row["ProductID"]));
                                detailCmd.Parameters.AddWithValue("@ProductName", row["Name"].ToString());
                                detailCmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(row["Quantity"]));
                                detailCmd.Parameters.AddWithValue("@UnitPrice", Convert.ToDecimal(row["Price"]));
                                detailCmd.Parameters.AddWithValue("@TotalPrice", Convert.ToDecimal(row["Total"]));
                                detailCmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public static DataTable GetLayawayOrders(int userId)
        {
            string query = @"SELECT orderid, ordernumber, orderdate, totalamount, paymentmethod, status
                             FROM layawayorders WHERE userid = @UserID AND status = 'Layaway'
                             ORDER BY orderdate DESC";
            var parameters = new[] { new NpgsqlParameter("@UserID", userId) };
            return ExecuteQuery(query, parameters);
        }

        public static DataTable GetLayawayOrderDetails(int orderId)
        {
            string query = @"SELECT od.productid, od.productname, od.quantity, od.unitprice, od.totalprice
                             FROM layawayorderdetails od WHERE od.orderid = @OrderID";
            var parameters = new[] { new NpgsqlParameter("@OrderID", orderId) };
            return ExecuteQuery(query, parameters);
        }

        public static bool DeleteLayawayOrder(int orderId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        var deleteDetailsQuery = "DELETE FROM layawayorderdetails WHERE orderid = @OrderID";
                        using (var cmd = new NpgsqlCommand(deleteDetailsQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.ExecuteNonQuery();
                        }
                        var deleteOrderQuery = "DELETE FROM layawayorders WHERE orderid = @OrderID";
                        using (var cmd = new NpgsqlCommand(deleteOrderQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}