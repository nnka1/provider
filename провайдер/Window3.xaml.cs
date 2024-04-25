using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace провайдер
{
    public class TariffViewModel
    {
        public int id_тарифа { get; set; }
        public string Тариф { get; set; }
        // Добавьте другие свойства тарифа, если необходимо
    }

    public partial class Window3 : Window
    {

        private DataTable table;
        private string connectionString = "Server=localhost;Port=5432;Database=provider;User ID=postgres;Password=123";

        public List<TariffViewModel> TariffList { get; set; }

        public Window3()
        {
            InitializeComponent();
            table = new DataTable();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM \"Пользователи\"";

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sqlQuery, connection))
                {
                    adapter.Fill(table);
                }
            }
            dataGrid.ItemsSource = table.DefaultView;
            dataGrid.Loaded += DataGrid_Loaded;
            TariffList = GetTariffListFromDatabase();
            DataContext = this; // Set the data context for data binding in XAML
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustColumnWidths();
        }

        private void AdjustColumnWidths()
        {
            int columnIndex = 0;
            foreach (DataGridColumn column in dataGrid.Columns)
            {
                if (columnIndex == 0)
                {
                    // Устанавливаем фиксированную ширину для первого столбца
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }
                else if (columnIndex == dataGrid.Columns.Count - 1)
                {
                    // Устанавливаем оставшееся пространство для последнего столбца
                    column.Width = new DataGridLength(2, DataGridLengthUnitType.Star);
                }
                else
                {
                    // Распределяем оставшееся пространство между остальными столбцами
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                }

                columnIndex++;
            }
        }

        private List<TariffViewModel> GetTariffListFromDatabase()
        {
            List<TariffViewModel> tariffList = new List<TariffViewModel>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT id_тарифа, Тариф FROM Тарифы";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            TariffViewModel tariff = new TariffViewModel();
                            tariff.id_тарифа = reader.GetInt32(0);
                            tariff.Тариф = reader.GetString(1);
                            // Add other tariff properties if necessary
                            tariffList.Add(tariff);
                        }
                    }
                }
            }

            return tariffList;
        }
    
    

    private void Поиск_GotFocus(object sender, RoutedEventArgs e)
        {

            TextBox textBox = sender as TextBox;

            if (textBox.Text == "Найти")
            {
                textBox.Text = "";
            }

        }

        private void назад_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window1 window1 = new Window1();
            window1.Show();
        }

        private void Добавить_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;

            if (selectedRow != null)
            {
                int id_тарифа = Convert.ToInt32(selectedRow["id_тарифа"]);
                string ФИО = selectedRow["ФИО"].ToString();
                string Контакты = selectedRow["Контакты"].ToString();
                string Адрес = selectedRow["Адрес"].ToString();
                DateTime Дата_рождения = DateTime.Parse(selectedRow["Дата_рождения"].ToString()).Date;

                decimal salePrice = GetTariffPrice(id_тарифа);
                if (salePrice == 0)
                {
                    MessageBox.Show("Не удалось получить цену тарифа.");
                    return;
                }

                string insertQuery = "INSERT INTO \"Пользователи\" (ФИО, Контакты, Адрес, Дата_рождения, id_тарифа) VALUES (@ФИО, @Контакты, @Адрес, @Дата_рождения, @id_тарифа) RETURNING *";
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ФИО", ФИО);
                        command.Parameters.AddWithValue("@Контакты", Контакты);
                        command.Parameters.AddWithValue("@Адрес", Адрес);
                        command.Parameters.AddWithValue("@id_тарифа", id_тарифа);
                        command.Parameters.AddWithValue("@Дата_рождения", NpgsqlTypes.NpgsqlDbType.Date).Value = Дата_рождения;

                        command.ExecuteNonQuery();
                    }
                }
            }
        
    }

        private decimal GetTariffPrice(int tariffId)
        {
            string query = "SELECT Стоимость FROM Тарифы WHERE id_тарифа = @id_тарифа";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id_тарифа", tariffId);
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

        private void Удалить_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;

            if (selectedRow != null)
            {
                int id = Convert.ToInt32(selectedRow["id_пользователя"]);

                string deleteQuery = "DELETE FROM \"Пользователи\" WHERE id_пользователя = @id";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        command.ExecuteNonQuery();
                    }
                }


                table.Clear();
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = "SELECT * FROM \"Пользователи\"";

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sqlQuery, connection))
                    {
                        adapter.Fill(table);
                    }
                }



            }
            dataGrid.ItemsSource = table.DefaultView;
            dataGrid.Loaded += DataGrid_Loaded;
        }

        private void Изменить_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;

            if (selectedRow != null)
            {
                int id_пользователя = Convert.ToInt32(selectedRow["id_пользователя"]);
                string ФИО = selectedRow["ФИО"].ToString();
                string Контакты = selectedRow["Контакты"].ToString();
                string Адрес = selectedRow["Адрес"].ToString();
                DateTime Дата_рождения = DateTime.Parse(selectedRow["Дата_рождения"].ToString()).Date;
                int id_тарифа = Convert.ToInt32(selectedRow["id_тарифа"]); // Добавляем получение id_тарифа из выбранной строки

                string updateQuery = "UPDATE \"Пользователи\" SET ФИО = @ФИО, Контакты = @Контакты, Дата_рождения = @Дата_рождения, Адрес = @Адрес, id_тарифа = @id_тарифа WHERE id_пользователя = @id_пользователя";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ФИО", ФИО);
                        command.Parameters.AddWithValue("@Контакты", Контакты);
                        command.Parameters.AddWithValue("@Адрес", Адрес);
                        command.Parameters.AddWithValue("@id_пользователя", id_пользователя);
                        command.Parameters.AddWithValue("@Дата_рождения", NpgsqlTypes.NpgsqlDbType.Date).Value = Дата_рождения;
                        command.Parameters.AddWithValue("@id_тарифа", id_тарифа); // Добавляем параметр id_тарифа

                        command.ExecuteNonQuery();
                    }
                }

                table.Clear();
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = "SELECT * FROM \"Пользователи\"";

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sqlQuery, connection))
                    {
                        adapter.Fill(table);
                    }
                }
            }
            dataGrid.ItemsSource = table.DefaultView;
            dataGrid.Loaded += DataGrid_Loaded;
        }

        private void Поиск1_Click(object sender, RoutedEventArgs e)
        {
            string searchText = Поиск.Text;

            string connectionString = "Server=localhost;Port=5432;Database=provider;User ID=postgres;Password=123";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string searchQuery = "SELECT * FROM \"Пользователи\" WHERE CAST(id_пользователя AS TEXT) LIKE @searchText OR \"ФИО\" LIKE @searchText OR CAST(\"Контакты\" AS TEXT) LIKE @searchText OR CAST(\"Адрес\" AS TEXT) LIKE @searchText OR CAST(Дата_рождения AS TEXT) LIKE @searchText OR CAST(id_тарифа AS TEXT) LIKE @searchText";

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(searchQuery, connection))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@searchText", "%" + searchText + "%");

                    DataTable searchResult = new DataTable();
                    adapter.Fill(searchResult);

                    dataGrid.ItemsSource = searchResult.DefaultView;
                    AdjustColumnWidths();
                }
            }
        }

        private void Назад_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window1 window1 = new Window1();
            window1.Show();
        }
    }
}
