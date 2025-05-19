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
    /// Логика взаимодействия для PatientSelectionWindow.xaml
    /// </summary>
    public partial class PatientSelectionWindow : Window
    {
        public Пациенты SelectedPatient { get; private set; }

        public PatientSelectionWindow(List<Пациенты> patients)
        {
            InitializeComponent();
            PatientsListBox.ItemsSource = patients;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (PatientsListBox.SelectedItem != null)
            {
                SelectedPatient = (Пациенты)PatientsListBox.SelectedItem;
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
