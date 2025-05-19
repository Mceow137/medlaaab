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

namespace medlaaab
{
    /// <summary>
    /// Логика взаимодействия для ServiceSelectionWindow.xaml
    /// </summary>
    public partial class ServiceSelectionWindow : Window
    {
        public List<Услуги> SelectedServices { get; private set; } = new List<Услуги>();

        public ServiceSelectionWindow()
        {
            InitializeComponent();
            LoadServices();
        }

        private void LoadServices()
        {
            using (var db = new МедицинскаяЛабораторияEntities())
            {
                ServicesDataGrid.ItemsSource = db.Услуги.ToList();
            }
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchText))
            {
                LoadServices();
                return;
            }

            using (var db = new МедицинскаяЛабораторияEntities())
            {
                var services = db.Услуги.ToList()
                    .Where(s => s.наименование.ToLower().Contains(searchText) ||
                                LevenshteinDistance.Compute(s.наименование.ToLower(), searchText) <= 3)
                    .ToList();

                ServicesDataGrid.ItemsSource = services;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (ServicesDataGrid.SelectedItems.Count > 0)
            {
                SelectedServices.AddRange(ServicesDataGrid.SelectedItems.Cast<Услуги>());
                SelectedServicesListBox.ItemsSource = null;
                SelectedServicesListBox.ItemsSource = SelectedServices;
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServicesListBox.SelectedItems.Count > 0)
            {
                var itemsToRemove = SelectedServicesListBox.SelectedItems.Cast<Услуги>().ToList();
                foreach (var item in itemsToRemove)
                {
                    SelectedServices.Remove(item);
                }
                SelectedServicesListBox.ItemsSource = null;
                SelectedServicesListBox.ItemsSource = SelectedServices;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedServices.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну услугу");
                return;
            }

            this.DialogResult = true;
            this.Close();
        }
    }
}
