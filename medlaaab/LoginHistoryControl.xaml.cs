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
    /// Логика взаимодействия для LoginHistoryControl.xaml
    /// </summary>
    public partial class LoginHistoryControl : Window
    {
        public LoginHistoryControl()
        {
            InitializeComponent();
            LoadLoginHistory();
        }

        private void LoadLoginHistory()
        {
            using (var db = new МедицинскаяЛабораторияEntities())
            {
                var history = db.ИсторияВходов
                    .Include(h => h.Пользователи)
                    .OrderByDescending(h => h.время_попытки)
                    .ToList();

                LoginHistoryDataGrid.ItemsSource = history;
            }
        }

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            var filterText = FilterTextBox.Text;

            using (var db = new МедицинскаяЛабораторияEntities())
            {
                var query = db.ИсторияВходов
                    .Include(h => h.Пользователи)
                    .AsQueryable();

                if (!string.IsNullOrEmpty(filterText))
                {
                    query = query.Where(h => h.Пользователи.логин.Contains(filterText));
                }

                LoginHistoryDataGrid.ItemsSource = query
                    .OrderByDescending(h => h.время_попытки)
                    .ToList();
            }
        }
    }
}
