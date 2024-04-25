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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace провайдер
{

    public partial class Window1 : Window
    {

        public Window1()
        {
            InitializeComponent();

        }

        private void тарифные_планы_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window2 window2 = new Window2();
            window2.Show(); 
        }

        private void пользователи_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window3 window3 = new Window3();
            window3.Show();
        }

        private void Выход_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void Скидки_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Оборудование_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window4 window4 = new Window4();
            window4.Show();
        }



        private void Отчеты_Click_1(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Window6 Window6 = new Window6();
            Window6.Show();
        }
    }

}
