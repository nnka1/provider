using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
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
    public partial class Window2 : Window
    {
        private DataTable table;
        private string connectionString = "Server=localhost;Port=5432;Database=provider;User ID=postgres;Password=123";

        public Window2()
        {
            InitializeComponent();

            table = new DataTable();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM \"Тарифы\"";

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sqlQuery, connection))
                {
                    adapter.Fill(table);
                }
            }
            dataGrid.ItemsSource = table.DefaultView;
            dataGrid.Loaded += DataGrid_Loaded;

        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustColumnWidths();
        }

        private void Добавить_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;

            if (selectedRow != null)
            {
                string tariff = selectedRow["Тариф"].ToString();
                decimal price = Convert.ToDecimal(selectedRow["Стоимость"]);
                int connectionSpeed = Convert.ToInt32(selectedRow["Скорость_соединения"]);

                string insertQuery = "INSERT INTO \"Тарифы\" (Тариф, Стоимость, \"Скорость_соединения\") VALUES (@tariff, @price, @connectionSpeed)";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@tariff", tariff);
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@connectionSpeed", connectionSpeed);

                        command.ExecuteNonQuery();
                    }
                }


                table.Clear();
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = "SELECT * FROM \"Тарифы\"";

                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sqlQuery, connection))
                    {
                        adapter.Fill(table);
                    }
                }


            }
            dataGrid.ItemsSource = table.DefaultView;
            dataGrid.Loaded += DataGrid_Loaded;
        }

        private void Удалить_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;

            if (selectedRow != null)
            {
                int id = Convert.ToInt32(selectedRow["id_тарифа"]);

                string deleteQuery = "DELETE FROM \"Тарифы\" WHERE id_тарифа = @id_тарифа";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@id_тарифа", id);

                        command.ExecuteNonQuery();
                    }
                }

                ОбновитьDataGrid(); // Заменить table.Clear(), adapter.Fill() и dataGrid.ItemsSource

            }
            dataGrid.Loaded += DataGrid_Loaded;
        }

        private void ОбновитьDataGrid()
        {
            table.Clear();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM \"Тарифы\"";

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sqlQuery, connection))
                {
                    adapter.Fill(table);
                }
            }

            dataGrid.ItemsSource = table.DefaultView;
        }

        private void Изменить_Click(object sender, RoutedEventArgs e)
        {
            DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;

            if (selectedRow != null)
            {
                int id = Convert.ToInt32(selectedRow["id_тарифа"]);
                string tariff = selectedRow["Тариф"].ToString();
                decimal price = Convert.ToDecimal(selectedRow["Стоимость"]);
                int connectionSpeed = Convert.ToInt32(selectedRow["Скорость_соединения"]);

                string updateQuery = "UPDATE \"Тарифы\" SET Тариф = @tariff, Стоимость = @price, \"Скорость_соединения\" = @connectionSpeed WHERE id_тарифа = @id_тарифа";

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    using (NpgsqlCommand command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@tariff", tariff);
                        command.Parameters.AddWithValue("@price", price);
                        command.Parameters.AddWithValue("@connectionSpeed", connectionSpeed);
                        command.Parameters.AddWithValue("@id_тарифа", id);

                        command.ExecuteNonQuery();
                    }
                }


                table.Clear();
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = "SELECT * FROM \"Тарифы\"";

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

                string searchQuery = "SELECT * FROM \"Тарифы\" WHERE CAST(id AS TEXT) LIKE @searchText OR \"Тариф\" LIKE @searchText OR CAST(\"Стоимость\" AS TEXT) LIKE @searchText OR CAST(\"Скорость_соединения\" AS TEXT) LIKE @searchText";

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

        private void Поиск_GotFocus(object sender, RoutedEventArgs e)
        {

            TextBox textBox = sender as TextBox;

            if (textBox.Text == "Найти")
            {
                textBox.Text = "";
            }

        }

        private void тарифные_планы_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window1 window1 = new Window1();
            window1.Show();
        }

        private void Назад_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window1 window1 = new Window1();    
            window1.Show();
        }
    }
}

