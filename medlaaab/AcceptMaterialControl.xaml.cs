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
    /// Логика взаимодействия для AcceptMaterialControl.xaml
    /// </summary>
    public partial class AcceptMaterialControl : Window
    {
        private Пользователи currentUser;

        public AcceptMaterialControl(Пользователи user)
        {
            InitializeComponent();
            currentUser = user;
            LoadPatients();
            LoadServices();
        }

        private void LoadPatients()
        {
            using (var db = new МедЛабDbContext())
            {
                PatientsComboBox.ItemsSource = db.Пациенты.ToList();
                PatientsComboBox.DisplayMemberPath = "ФИО";
            }
        }

        private void LoadServices()
        {
            using (var db = new МедЛабDbContext())
            {
                ServicesListBox.ItemsSource = db.Услуги.ToList();
            }
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPatient = PatientsComboBox.SelectedItem as Пациенты;
            var selectedServices = ServicesListBox.SelectedItems.Cast<Услуги>().ToList();

            if (selectedPatient == null || !selectedServices.Any())
            {
                MessageBox.Show("Выберите пациента и хотя бы одну услугу");
                return;
            }

            using (var db = new МедЛабDbContext())
            {
                var order = new Заказы
                {
                    id_пациента = selectedPatient.id,
                    id_врача = currentUser.id,
                    дата_создания = DateTime.Now,
                    статус = "Новый",
                    в_архиве = false,
                    штрих_код = GenerateBarcode()
                };

                db.Заказы.Add(order);
                db.SaveChanges();

                foreach (var service in selectedServices)
                {
                    db.УслугиВЗаказе.Add(new УслугиВЗаказе
                    {
                        id_заказа = order.id,
                        код_услуги = service.код,
                        статус = "Назначена",
                        дата_назначения = DateTime.Now
                    });
                }

                db.SaveChanges();
                MessageBox.Show($"Заказ #{order.id} создан. Штрих-код: {order.штрих_код}");
            }
        }

        private string GenerateBarcode()
        {
            var random = new Random();
            return random.Next(1000000, 9999999).ToString();
        }
    }
}
