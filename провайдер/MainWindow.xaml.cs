using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;

namespace провайдер
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void логин_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox.Text == "Логин")
            {
                textBox.Text = "";
            }
        }

        private void Пароль_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox.Text == "Пароль")
            {
                textBox.Text = "";
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = "Server=localhost;Port=5432;Database=provider;User ID=postgres;Password=123";
            NpgsqlConnection connection = new NpgsqlConnection(connectionString);

            string логин = Логин.Text;
            string пароль = Пароль.Text;

            string query = "SELECT * FROM admin WHERE log = @логин AND pass = @пароль";

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@логин", логин);
                cmd.Parameters.AddWithValue("@пароль", пароль);

                try
                {
                    connection.Open();
                    NpgsqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();
                        string fullname = reader["log"].ToString();

                        this.Hide();
                        Window1 Window1 = new Window1();
                        Window1.Show();
                    }
                    else
                    {
                        MessageBox.Show("Неправильный логин или пароль");
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при выполнении запроса: " + ex.Message);
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}
