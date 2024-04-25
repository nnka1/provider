using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using iTextSharp;
using System.IO;
using iTextSharp.text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;


namespace провайдер
{

    /// <summary>
    /// Логика взаимодействия для Window6.xaml
    /// </summary>
    public partial class Window6 : Window
    {

        private string filePath;
        private DataTable table;
        private string connectionString = "Server=localhost;Port=5432;Database=provider;User ID=postgres;Password=123";

        public Window6()
        {
            InitializeComponent();
            table = new DataTable();



            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT * FROM \"Финансы\"";

                using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(sqlQuery, connection))
                {
                    adapter.Fill(table);
                }

            }

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT DISTINCT \"Тариф\" FROM \"Тарифы\"";
                using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbTariffs.Items.Add(reader.GetString(0));
                        }
                    }
                }
            }

            cmbTariffs.SelectionChanged += CmbTariffs_SelectionChanged;

            dataGrid.ItemsSource = table.DefaultView;
            dataGrid.Loaded += DataGrid_Loaded;
        }

        private void CmbTariffs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbTariffs.SelectedItem != null)
            {
                string selectedTariff = cmbTariffs.SelectedItem.ToString();
                int salesCount; // Объявление переменной "salesCount"
                decimal tariffPrice; // Объявление переменной "tariffPrice"

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // Retrieve and display the sales statistics for the selected tariff
                    string sqlQuery = $"SELECT COUNT(*) FROM \"Пользователи\" WHERE \"id_тарифа\" = (SELECT \"id_тарифа\" FROM \"Тарифы\" WHERE \"Тариф\" = '{selectedTariff}')";
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                    {
                        salesCount = Convert.ToInt32(command.ExecuteScalar());
                        lblSalesCount.Content = $"Количество продаж: {salesCount}";
                    }

                    // Retrieve the price of the selected tariff
                    sqlQuery = $"SELECT \"Стоимость\" FROM \"Тарифы\" WHERE \"Тариф\" = '{selectedTariff}'";
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                    {
                        tariffPrice = Convert.ToDecimal(command.ExecuteScalar());
                    }

                    // Calculate and display the total income from the selected tariff
                    decimal totalIncome = salesCount * tariffPrice;
                    lblTotalIncome.Content = $"Общий доход от тарифа '{selectedTariff}': {totalIncome} рублей";


                }

            }

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

        private void Назад_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window1 window1 = new Window1();
            window1.Show();
        }


        private void CalculateTotalIncome()
        {
            decimal totalIncome = 0;

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT \"Тариф\", \"Стоимость\" FROM \"Тарифы\"";
                using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                {
                    using (NpgsqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string tariff = reader.GetString(0);
                            decimal price = reader.GetDecimal(1);

                            int salesCount;
                            decimal tariffPrice;

                            using (NpgsqlConnection salesConnection = new NpgsqlConnection(connectionString))
                            {
                                salesConnection.Open();

                                string salesQuery = $"SELECT COUNT(*) FROM \"Пользователи\" WHERE \"id_тарифа\" = (SELECT \"id_тарифа\" FROM \"Тарифы\" WHERE \"Тариф\" = '{tariff}')";
                                using (NpgsqlCommand salesCommand = new NpgsqlCommand(salesQuery, salesConnection))
                                {
                                    salesCount = Convert.ToInt32(salesCommand.ExecuteScalar());
                                }

                                string priceQuery = $"SELECT \"Стоимость\" FROM \"Тарифы\" WHERE \"Тариф\" = '{tariff}'";
                                using (NpgsqlCommand priceCommand = new NpgsqlCommand(priceQuery, salesConnection))
                                {
                                    tariffPrice = Convert.ToDecimal(priceCommand.ExecuteScalar());
                                }

                                totalIncome += salesCount * tariffPrice;

                                salesConnection.Close();
                            }
                        }
                    }
                }
            }

            всевсе.Content = $"Общий доход от всех тарифов: {totalIncome} рублей";
        }

        private void CalculateTotalIncome_Click(object sender, RoutedEventArgs e)
        {
            CalculateTotalIncome();
        }

        private void BtnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Создать новый PDF-документ
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // Установить шрифт Arial для русского языка
                BaseFont baseFont = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Font font = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);

                // Добавить заголовок в документ
                Paragraph title = new Paragraph("Отчет по тарифам", font);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                // Добавить таблицу для отображения информации о тарифах
                PdfPTable table = new PdfPTable(4);
                table.AddCell(new Phrase("Тариф", font));
                table.AddCell(new Phrase("Стоимость", font));
                table.AddCell(new Phrase("Количество продаж", font));
                table.AddCell(new Phrase("Общий доход", font));
                

                // Пройтись по тарифам и добавить их в таблицу
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlQuery = "SELECT \"Тариф\", \"Стоимость\" FROM \"Тарифы\"";
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string tariff = reader.GetString(0);
                                decimal price = reader.GetDecimal(1);

                                // Рассчитать общий доход
                                int salesCount;
                                decimal totalIncome;

                                using (NpgsqlConnection salesConnection = new NpgsqlConnection(connectionString))
                                {
                                    salesConnection.Open();

                                    string salesQuery = $"SELECT COUNT(*) FROM \"Пользователи\" WHERE \"id_тарифа\" = (SELECT \"id_тарифа\" FROM \"Тарифы\" WHERE \"Тариф\" = '{tariff}')";
                                    using (NpgsqlCommand salesCommand = new NpgsqlCommand(salesQuery, salesConnection))
                                    {
                                        salesCount = Convert.ToInt32(salesCommand.ExecuteScalar());
                                    }

                                    totalIncome = salesCount * price;

                                    salesConnection.Close();
                                }

                                // Добавить информацию о тарифе в таблицу
                                table.AddCell(new Phrase(tariff, font));
                                table.AddCell(new Phrase(price.ToString(), font));
                                table.AddCell(new Phrase(salesCount.ToString(), font));
                                table.AddCell(new Phrase(totalIncome.ToString(), font));
                            }
                        }
                    }
                }

                document.Add(table);

                document.Close();

                // Автоматически открыть PDF-файл в браузере
                byte[] pdfBytes = ms.ToArray();
                string base64Pdf = Convert.ToBase64String(pdfBytes);
                string html = $"<html><body><object data=\"data:application/pdf;base64,{base64Pdf}\" type=\"application/pdf\" width=\"100%\" height=\"100%\"></object></body></html>";
                File.WriteAllText("report.html", html);
                Process.Start("report.html");
            }
        }

        private void BtnGenerateReport2_Click(object sender, RoutedEventArgs e)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();
                BaseFont baseFont = BaseFont.CreateFont("c:/windows/fonts/arial.ttf", BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                Font font = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);
                Paragraph title = new Paragraph("Отчет по проданному и арендованному оборудованию", font);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);
                PdfPTable table = new PdfPTable(6);
                table.AddCell(new Phrase("Тип операции", font));
                table.AddCell(new Phrase("ФИО", font));
                table.AddCell(new Phrase("Оборудование", font));
                table.AddCell(new Phrase("Дата начала", font));
                table.AddCell(new Phrase("Дата окончания", font));
                table.AddCell(new Phrase("Стоимость", font));
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = "SELECT Продажи_оборудования.id_пользователя, Продажи_оборудования.*, Пользователи.ФИО " +
                        "FROM Продажи_оборудования " +
                        "LEFT JOIN Пользователи ON Продажи_оборудования.id_пользователя = Пользователи.id_пользователя";
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                table.AddCell(new Phrase("Продажа", font));
                                table.AddCell(new Phrase(reader["ФИО"].ToString(), font));
                                table.AddCell(new Phrase(reader["id_оборудования"].ToString(), font));
                                table.AddCell(new Phrase(reader["Дата"].ToString(), font));
                                table.AddCell(new Phrase("", font));
                                table.AddCell(new Phrase(reader["Стоимость_продажи"].ToString(), font));
                            }
                        }
                    }
                    sqlQuery = "SELECT Продажи_оборудования.id_пользователя, Продажи_оборудования.*, Пользователи.ФИО, Аренда_оборудования.Дата_начала, Аренда_оборудования.Дата_окончания, Аренда_оборудования.Стоимость_аренды " +
                "FROM Продажи_оборудования " +
                "LEFT JOIN Пользователи ON Продажи_оборудования.id_пользователя = Пользователи.id_пользователя " +
                "LEFT JOIN Аренда_оборудования ON Продажи_оборудования.id_оборудования = Аренда_оборудования.id_оборудования";
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                    {
                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                table.AddCell(new Phrase("Аренда", font));
                                table.AddCell(new Phrase(reader["ФИО"].ToString(), font));
                                table.AddCell(new Phrase(reader["id_оборудования"].ToString(), font));
                                table.AddCell(new Phrase(reader["Дата_начала"].ToString(), font));
                                table.AddCell(new Phrase(reader["Дата_окончания"].ToString(), font));
                                table.AddCell(new Phrase(reader["Стоимость_аренды"].ToString(), font));
                            }
                        }
                    }
                }
                document.Add(table);
                document.Close();
                byte[] pdfBytes = ms.ToArray();
                string base64Pdf = Convert.ToBase64String(pdfBytes);
                string html = $"<html><body><object data=\"data:application/pdf;base64,{base64Pdf}\" type=\"application/pdf\" width=\"100%\" height=\"100%\"></object></body></html>";
                File.WriteAllText("report.html", html);
                Process.Start("report.html");
            }
        }
    }
}
        
