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
    /// Логика взаимодействия для AddPatientWindow.xaml
    /// </summary>
    public partial class AddPatientWindow : Window
    {
        public Пациенты NewPatient { get; private set; }

        public AddPatientWindow()
        {
            InitializeComponent();
            BirthDatePicker.SelectedDate = DateTime.Now.AddYears(-30);
            LoadInsuranceTypes();
        }

        private void LoadInsuranceTypes()
        {
            InsuranceTypeComboBox.Items.Add("ОМС");
            InsuranceTypeComboBox.Items.Add("ДМС");
            InsuranceTypeComboBox.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LastNameTextBox.Text) ||
                string.IsNullOrEmpty(FirstNameTextBox.Text) ||
                BirthDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Заполните обязательные поля (Фамилия, Имя, Дата рождения)");
                return;
            }

            NewPatient = new Пациенты
            {
                фамилия = LastNameTextBox.Text,
                имя = FirstNameTextBox.Text,
                отчество = MiddleNameTextBox.Text,
                дата_рождения = BirthDatePicker.SelectedDate.Value,
                серия_паспорта = PassportSeriesTextBox.Text,
                номер_паспорта = PassportNumberTextBox.Text,
                телефон = PhoneTextBox.Text,
                email = EmailTextBox.Text,
                номер_полиса = InsuranceNumberTextBox.Text,
                тип_полиса = InsuranceTypeComboBox.SelectedItem.ToString(),
                id_страховой_компании = (InsuranceCompanyComboBox.SelectedItem as СтраховыеКомпании)?.id,
                дата_регистрации = DateTime.Now
            };

            using (var db = new МедЛабDbContext())
            {
                db.Пациенты.Add(NewPatient);
                db.SaveChanges();
            }

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
