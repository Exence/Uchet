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
using Uchet.Classes;

namespace Uchet.Views
{
    /// <summary>
    /// Логика взаимодействия для ChooseUserWindow.xaml
    /// </summary>
    public partial class ChooseUserWindow : Window
    {
        public ChooseUserWindow(string surname, string name, string middleName, List<int> numbers)
        {
            InitializeComponent();
            comboBoxNum.ItemsSource = numbers;
            comboBoxNum.SelectedIndex = 0;
            labelCount.Content = numbers.Count().ToString();
            labelName.Content = surname + " " + name + " " + middleName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void comboBoxNum_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Flags.selectedIndex = comboBoxNum.SelectedIndex;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Flags.selectedIndex = comboBoxNum.SelectedIndex;
            Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Flags.selectedIndex = comboBoxNum.SelectedIndex;
        }
    }
}
