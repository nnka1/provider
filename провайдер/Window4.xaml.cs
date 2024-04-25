using Microsoft.Win32;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace провайдер
{

    public class Equipment
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private string название_оборудования;
        public string Название_оборудования
        {
            get { return название_оборудования; }
            set
            {
                название_оборудования = value;
                OnPropertyChanged();
            }
        }

        private string тип_оборудования;
        public string Тип_оборудования
        {
            get { return тип_оборудования; }
            set
            {
                тип_оборудования = value;
                OnPropertyChanged();
            }
        }
        public int id_оборудования { get; set; } 
        public string Стоимость_продажи { get; set; }
        public string Стоимость_аренды { get; set; }
        public string photo { get; set; }
        private BitmapImage _image;

        public BitmapImage Image
        {
            get { return _image; }
            set { _image = value; }
        }


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class User
    {
        public string id_пользователя { get; set; }
        public string ФИО { get; set; } // Свойство ФИО пользователя
        public string Контакты { get; set; } // Свойство контактов пользователя
    }

    public class AddEquipmentDialog : Window
    {
        public Equipment NewEquipment { get; private set; }

        
        public void SetNewEquipment(Equipment equipment)
        {
            NewEquipment = equipment;
        }

        
    }


    public partial class Window4 : Window
    {
        private string connectionString = "Server=localhost;Port=5432;Database=provider;User ID=postgres;Password=123";
        public ObservableCollection<Equipment> EquipmentList { get; set; }
        private ObservableCollection<User> userList { get; set; }

        private Equipment newEquipment;

        public Window4()
        {
            InitializeComponent();
            EquipmentList = new ObservableCollection<Equipment>();

            LoadEquipmentFromDatabase();
            DataContext = this;

            userList = new ObservableCollection<User>();
            Loaded += Window_Loaded; // Добавляем обработчик события Loaded

            // Устанавливаем DataContext окна на список пользователей
            DataContext = userList;

        }

        private void LoadEquipmentFromDatabase()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT id_оборудования, Название_оборудования, Тип_оборудования, Стоимость_продажи, Стоимость_аренды, photo FROM Оборудование";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Equipment equipment = new Equipment
                            {
                                id_оборудования = reader.GetInt32(reader.GetOrdinal("id_оборудования")),
                                Название_оборудования = reader["Название_оборудования"].ToString(),
                                Тип_оборудования = reader["Тип_оборудования"].ToString(),
                                Стоимость_продажи = reader["Стоимость_продажи"].ToString(),
                                Стоимость_аренды = reader["Стоимость_аренды"].ToString(),
                                photo = reader["photo"].ToString(),
                            };

                            if (File.Exists(equipment.photo))
                            {
                                equipment.Image = new BitmapImage(new Uri(equipment.photo));
                            }

                            Dispatcher.Invoke(() => EquipmentList.Add(equipment));
                        }
                    }
                }
            }
        }

        private Equipment GetSelectedEquipment(int index)
        {
            if (index >= 0 && index < EquipmentList.Count)
            {
                return EquipmentList[index];
            }

            return null;
        }
        private Equipment GetSelectedEquipment()
        {
            return equipmentListView.SelectedItem as Equipment;
        }
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string name = оборудование.Text;
            string type = тип.Text;
            decimal price = Decimal.Parse(продажа.Text); // Преобразуем введенное значение стоимости продажи в тип decimal
            decimal rentPrice = Decimal.Parse(аренда.Text);

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "INSERT INTO \"Оборудование\" (Название_оборудования, Тип_оборудования, Стоимость_продажи, Стоимость_аренды, photo) " +
                      "VALUES (@Name, @Type, @Price, @RentPrice, @Photo)";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Type", type);
                    cmd.Parameters.AddWithValue("@Price", price); // Добавляем значение стоимости продажи как параметр типа decimal
                    cmd.Parameters.AddWithValue("@RentPrice", rentPrice);
                    cmd.Parameters.AddWithValue("@Photo", newEquipment.photo);

                    cmd.ExecuteNonQuery();
                }
            }

            UpdateListView();
            equipmentListView.UpdateLayout();

        }


        private void UpdateListView()
        {

            ObservableCollection<Equipment> updatedEquipmentList = new ObservableCollection<Equipment>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM \"Оборудование\"";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Equipment equipment = new Equipment
                            {
                                Название_оборудования = reader["Название_оборудования"].ToString(),
                                Тип_оборудования = reader["Тип_оборудования"].ToString(),
                                Стоимость_продажи = reader["Стоимость_продажи"].ToString(),
                                Стоимость_аренды = reader["Стоимость_аренды"].ToString(),
                                photo = reader["photo"].ToString(),
                            };

                            if (File.Exists(equipment.photo))
                            {
                                equipment.Image = new BitmapImage(new Uri(equipment.photo));
                            }

                            updatedEquipmentList.Add(equipment);
                        }
                    }
                }
            }

            Dispatcher.Invoke(() => equipmentListView.ItemsSource = updatedEquipmentList);
        }
        private void UpdateEquipmentInDatabase(Equipment equipment)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "UPDATE \"Оборудование\" SET Название_оборудования = @Name, Тип_оборудования = @Type, " +
                                  "Стоимость_продажи = @Price, Стоимость_аренды = @RentPrice, photo = @Photo WHERE id_оборудования = @id_оборудования";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", equipment.Название_оборудования);
                    cmd.Parameters.AddWithValue("@Type", equipment.Тип_оборудования);
                    cmd.Parameters.AddWithValue("@Price", equipment.Стоимость_продажи);
                    cmd.Parameters.AddWithValue("@RentPrice", equipment.Стоимость_аренды);
                    cmd.Parameters.AddWithValue("@Photo", equipment.photo);
                    cmd.Parameters.AddWithValue("@id_оборудования", equipment.id_оборудования);

                    cmd.ExecuteNonQuery();
                }
            }
        }



        private void UpdateEquipmentList()
        {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Equipment selectedEquipment = GetSelectedEquipment();

            if (selectedEquipment != null)
            {
                try
                {
                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();

                        // Check if the equipment is currently rented
                        if (IsEquipmentRented(selectedEquipment.id_оборудования))
                        {
                            MessageBox.Show("Выбранное оборудование в данный момент арендовано и не может быть удалено.");
                            return;
                        }

                        // First, delete the corresponding records in the Продажи_оборудования table
                        string sqlQuery = "DELETE FROM \"Продажи_оборудования\" WHERE id_оборудования = @id";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", selectedEquipment.id_оборудования);
                            cmd.ExecuteNonQuery();
                        }

                        // Then, delete the equipment from the Оборудование table
                        sqlQuery = "DELETE FROM \"Оборудование\" WHERE Название_оборудования = @Name";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@Name", selectedEquipment.Название_оборудования);

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Запись успешно удалена.");
                                UpdateListView(); // Обновляем список
                            }
                            else
                            {
                                Console.WriteLine("Запись не была удалена. Убедитесь, что выбранное оборудование существует.");
                            }
                        }
                    }
                }
                catch (NpgsqlException ex)
                {
                    Console.WriteLine("Ошибка при удалении записи: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите оборудование для удаления.");
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            DataContext = this;
        }

        private void LoadUsers()
        {
            userList.Clear(); // Очищаем список пользователей перед загрузкой новых данных
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string sqlQuery = "SELECT * FROM Пользователи";
                using (NpgsqlCommand cmd = new NpgsqlCommand(sqlQuery, connection))
                {
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User user = new User
                            {
                                id_пользователя = reader["id_пользователя"].ToString(),
                                ФИО = reader["ФИО"].ToString(),
                                Контакты = reader["Контакты"].ToString(),
                            };
                            userList.Add(user); // Добавляем пользователя в список
                        }
                    }
                }
            }
            usersListView.ItemsSource = userList; // Устанавливаем список пользователей в качестве источника данных для ListView
        }





        private void UpdateImageDisplay()
        {
            if (File.Exists(newEquipment.photo))
            {
                newEquipment.Image = new BitmapImage(new Uri(newEquipment.photo));
            }

            // Here you would need the code to update the UI with the new image
        }

        private void SelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg;*.bmp)|*.png;*.jpeg;*.jpg;*.bmp|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                newEquipment = new Equipment();
                newEquipment.photo = openFileDialog.FileName;

                UpdateImageDisplay(); // Update the image display
            }
        }

        private void Назад_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window1 window1 = new Window1();
            window1.Show();
        }

        private void Оборудование_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox.Text == "Оборудование")
            {
                textBox.Text = "";
            }
        }
        private void Аренда_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox.Text == "Цена аренды")
            {
                textBox.Text = "";
            }
        }

        private void Продажа_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox.Text == "Стоимость продажи")
            {
                textBox.Text = "";
            }
        }

        private void Тип_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox.Text == "Тип")
            {
                textBox.Text = "";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateListView();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (usersListView.SelectedItem == null || equipmentListView.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя и оборудование для продажи.");
                return;
            }
            User selectedUser = (User)usersListView.SelectedItem;
            Equipment selectedEquipment = (Equipment)equipmentListView.SelectedItem;

            Console.WriteLine($"Выбранное оборудование: {selectedEquipment}");

            DateTime selectedDate = DateTime.Today;

            int equipmentId = selectedEquipment.id_оборудования;
            if (!IsEquipmentIdExist(equipmentId))
            {
                MessageBox.Show("Выбранное оборудование не существует.");
                return;
            }
            if (IsEquipmentRented(equipmentId))
            {
                MessageBox.Show("Выбранное оборудование в данный момент арендовано.");
                return;
            }
            if (IsEquipmentSold(equipmentId))
            {
                MessageBox.Show("Выбранное оборудование уже было продано.");
                return;
            }

            decimal salePrice = GetEquipmentSalePrice(equipmentId);
            if (salePrice == 0)
            {
                MessageBox.Show("Не удалось получить цену продажи оборудования.");
                return;
            }

            string query = "INSERT INTO Продажи_оборудования (id_пользователя, id_оборудования, Стоимость_продажи, Дата) VALUES (@id_пользователя, @id_оборудования, @Стоимость_продажи, @Дата)";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    int userId;
                    if (!int.TryParse(selectedUser.id_пользователя, out userId))
                    {
                        MessageBox.Show("Невозможно преобразовать id пользователя в целое число.");
                        return;
                    }

                    command.Parameters.AddWithValue("@id_пользователя", userId);
                    command.Parameters.AddWithValue("@id_оборудования", equipmentId);
                    command.Parameters.AddWithValue("@Стоимость_продажи", salePrice);
                    command.Parameters.AddWithValue("@Дата", selectedDate);
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Продажа успешно добавлена в базу данных.");
        }

        // Метод для получения цены продажи оборудования из таблицы "Оборудование"
        private decimal GetEquipmentSalePrice(int equipmentId)
        {
            string query = "SELECT Стоимость_продажи FROM Оборудование WHERE id_оборудования = @id_оборудования";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_оборудования", equipmentId);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDecimal(result);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }


        private bool IsEquipmentIdExist(int equipmentId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Оборудование WHERE id_оборудования = @id_оборудования";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_оборудования", equipmentId);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private bool IsEquipmentRented(int equipmentId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Аренда_оборудования WHERE id_оборудования = @id_оборудования AND Дата_окончания >= CURRENT_DATE";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_оборудования", equipmentId);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private bool IsEquipmentSold(int equipmentId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Продажи_оборудования WHERE id_оборудования = @id_оборудования";
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_оборудования", equipmentId);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }
        private decimal GetEquipmentRentalPrice(int equipmentId)
        {
            string query = "SELECT Стоимость_аренды FROM Оборудование WHERE id_оборудования = @id_оборудования";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_оборудования", equipmentId);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToDecimal(result);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }

        private void арендовать_Click(object sender, RoutedEventArgs e)
        {
            if (usersListView.SelectedItem == null || equipmentListView.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя и оборудование для аренды.");
                return;
            }

            // Создаем новое окно для выбора даты начала аренды
            DatePicker startDatePicker = new DatePicker();
            startDatePicker.Margin = new Thickness(10);
            startDatePicker.Width = 150;
            startDatePicker.SelectedDateChanged += (obj, args) =>
            {
                // При выборе даты начала, открываем окно для выбора даты окончания аренды
                DatePicker endDatePicker = new DatePicker();
                endDatePicker.Margin = new Thickness(10);
                endDatePicker.Width = 150;
                endDatePicker.SelectedDateChanged += (obj2, args2) =>
                {
                    // При выборе даты окончания аренды выполняем код добавления в базу данных
                    User selectedUser = (User)usersListView.SelectedItem;
                    Equipment selectedEquipment = (Equipment)equipmentListView.SelectedItem;
                    DateTime? startDate = startDatePicker.SelectedDate;
                    DateTime? endDate = endDatePicker.SelectedDate;

                    if (startDate == null || endDate == null)
                    {
                        MessageBox.Show("Пожалуйста, выберите дату начала и дату окончания аренды.");
                        return;
                    }


                    int equipmentId = selectedEquipment.id_оборудования;
                    if (!IsEquipmentIdExist(equipmentId))
                    {
                        MessageBox.Show("Выбранное оборудование не существует.");
                        return;
                    }
                    if (IsEquipmentRented(equipmentId))
                    {
                        MessageBox.Show("Выбранное оборудование в данный момент арендовано.");
                        return;
                    }
                    if (IsEquipmentSold(equipmentId))
                    {
                        MessageBox.Show("Выбранное оборудование уже было продано.");
                        return;
                    }
                    decimal rentalPrice = GetEquipmentRentalPrice(equipmentId);
                    if (rentalPrice == 0)
                    {
                        MessageBox.Show("Не удалось получить стоимость аренды оборудования.");
                        return;
                    }

                    // Вставка данных в таблицу Аренда_оборудования
                    string query = "INSERT INTO Аренда_оборудования (id_пользователя, id_оборудования, Стоимость_аренды, Дата_начала, Дата_окончания) VALUES (@id_пользователя, @id_оборудования, @Стоимость_аренды, @Дата_начала, @Дата_окончания)";

                    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                        {
                            int userId;
                            if (!int.TryParse(selectedUser.id_пользователя, out userId))
                            {
                                MessageBox.Show("Невозможно преобразовать id пользователя в целое число.");
                                return;
                            }

                            command.Parameters.AddWithValue("@id_пользователя", userId);
                            command.Parameters.AddWithValue("@id_оборудования", equipmentId);
                            command.Parameters.AddWithValue("@Стоимость_аренды", rentalPrice);
                            command.Parameters.AddWithValue("@Дата_начала", startDate);
                            command.Parameters.AddWithValue("@Дата_окончания", endDate);
                            command.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Аренда успешно добавлена в базу данных.");

                    // Закрываем окно выбора даты окончания аренды после добавления информации в базу данных
                    Window.GetWindow(endDatePicker).Close();
                };

                // Отображаем окно выбора даты окончания аренды
                var window = new Window
                {
                    Content = endDatePicker,
                    Width = 200,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    ResizeMode = ResizeMode.NoResize,
                    Title = "Выбор даты окончания аренды"
                };
                window.ShowDialog();
            };

            // Отображаем окно выбора даты начала аренды
            var windowStart = new Window
            {
                Content = startDatePicker,
                Width = 200,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Title = "Выбор даты начала аренды"
            };
            windowStart.ShowDialog();
        }
    }
}

