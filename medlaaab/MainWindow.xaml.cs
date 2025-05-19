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
using System.Windows.Threading;

namespace medlaaab
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Пользователи currentUser;
        private DispatcherTimer sessionTimer;
        private TimeSpan sessionTimeLeft;
        private const int SessionDurationMinutes = 10; // 10 минут для теста
        private const int WarningMinutes = 5; // Предупреждение за 5 минут

        public MainWindow(Пользователи user)
        {
            InitializeComponent();
            currentUser = user;
            InitializeUserInterface();
            StartSessionTimer();
        }

        private void InitializeUserInterface()
        {
            // Установка информации о пользователе
            UserNameTextBlock.Text = $"{currentUser.фамилия} {currentUser.имя}";
            RoleTextBlock.Text = currentUser.Роли?.название ?? "Неизвестная роль";

            // Загрузка фото пользователя
            if (currentUser.фото != null && currentUser.фото.Length > 0)
            {
                using (var stream = new System.IO.MemoryStream(currentUser.фото))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    UserPhoto.Source = bitmap;
                }
            }

            // Настройка интерфейса в зависимости от роли
            switch (currentUser.Роли?.название)
            {
                case "Лаборант":
                    SetupLabAssistantUI();
                    break;
                case "Лаборант-исследователь":
                    SetupLabResearcherUI();
                    break;
                case "Бухгалтер":
                    SetupAccountantUI();
                    break;
                case "Администратор":
                    SetupAdminUI();
                    break;
                default:
                    MessageBox.Show("Неизвестная роль пользователя");
                    break;
            }
        }

        private void SetupLabAssistantUI()
        {
            // Настройка интерфейса для лаборанта
            MainTabControl.Items.Clear();

            var acceptTab = new TabItem { Header = "Прием биоматериала" };
            acceptTab.Content = new AcceptMaterialControl(currentUser);

            var reportsTab = new TabItem { Header = "Отчеты" };
            reportsTab.Content = new ReportsControl();

            MainTabControl.Items.Add(acceptTab);
            MainTabControl.Items.Add(reportsTab);
        }

        private void SetupLabResearcherUI()
        {
            // Настройка интерфейса для лаборанта-исследователя
            MainTabControl.Items.Clear();

            var analyzerTab = new TabItem { Header = "Работа с анализатором" };
            analyzerTab.Content = new AnalyzerControl(currentUser);

            MainTabControl.Items.Add(analyzerTab);
        }

        private void SetupLabAssistantUI()
        {
            MainTabControl.Items.Clear();

            var acceptTab = new TabItem { Header = "Прием биоматериала" };
            var acceptPanel = new StackPanel { Orientation = Orientation.Vertical };

            var newOrderButton = new Button
            {
                Content = "Сформировать новый заказ",
                Margin = new Thickness(10),
                Padding = new Thickness(10),
                FontSize = 14
            };
            newOrderButton.Click += (s, e) =>
            {
                var orderWindow = new OrderCreationWindow(currentUser);
                orderWindow.ShowDialog();
            };

            acceptPanel.Children.Add(newOrderButton);
            acceptTab.Content = acceptPanel;

            var reportsTab = new TabItem { Header = "Отчеты" };
            reportsTab.Content = new ReportsControl();

            MainTabControl.Items.Add(acceptTab);
            MainTabControl.Items.Add(reportsTab);
        }

        private void SetupAdminUI()
        {
            // Настройка интерфейса для администратора
            MainTabControl.Items.Clear();

            var reportsTab = new TabItem { Header = "Отчеты" };
            reportsTab.Content = new ReportsControl();

            var usersTab = new TabItem { Header = "Пользователи" };
            usersTab.Content = new UsersControl();

            var historyTab = new TabItem { Header = "История входов" };
            historyTab.Content = new LoginHistoryControl();

            var materialsTab = new TabItem { Header = "Расходные материалы" };
            materialsTab.Content = new MaterialsControl();

            MainTabControl.Items.Add(reportsTab);
            MainTabControl.Items.Add(usersTab);
            MainTabControl.Items.Add(historyTab);
            MainTabControl.Items.Add(materialsTab);
        }

        private void StartSessionTimer()
        {
            sessionTimeLeft = TimeSpan.FromMinutes(SessionDurationMinutes);
            UpdateTimerDisplay();

            sessionTimer = new DispatcherTimer();
            sessionTimer.Interval = TimeSpan.FromSeconds(1);
            sessionTimer.Tick += SessionTimer_Tick;
            sessionTimer.Start();
        }

        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            sessionTimeLeft = sessionTimeLeft.Subtract(TimeSpan.FromSeconds(1));
            UpdateTimerDisplay();

            // Проверка на предупреждение
            if (sessionTimeLeft.TotalMinutes <= WarningMinutes &&
                sessionTimeLeft.TotalMinutes > WarningMinutes - 1)
            {
                MessageBox.Show($"До окончания сеанса осталось {WarningMinutes} минут. " +
                               "Сохраните свои данные.", "Предупреждение");
            }

            // Проверка на окончание сеанса
            if (sessionTimeLeft.TotalSeconds <= 0)
            {
                EndSession();
            }
        }

        private void UpdateTimerDisplay()
        {
            TimerTextBlock.Text = $"{sessionTimeLeft:mm\\:ss}";
        }

        private void EndSession()
        {
            sessionTimer.Stop();

            MessageBox.Show("Ваш сеанс завершен. Требуется кварцевание помещений. " +
                           "Вход будет заблокирован на 1 минуту.", "Сеанс завершен");

            // Блокировка входа
            using (var db = new МедицинскаяЛабораторияEntities())
            {
                db.Блокировки.Add(new Блокировки
                {
                    id_пользователя = currentUser.id,
                    время_начала = DateTime.Now,
                    длительность_минуты = 1, // Для теста 1 минута
                    причина = "Кварцевание помещений"
                });
                db.SaveChanges();
            }

            // Возврат на окно входа
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            sessionTimer.Stop();
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
