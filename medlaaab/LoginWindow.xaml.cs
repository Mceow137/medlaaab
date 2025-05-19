using ClientDependency.Core;
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
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private int failedAttempts = 0;
        private bool captchaRequired = false;
        private string currentCaptcha = "";

        public LoginWindow()
        {
            InitializeComponent();
            UpdateUI();
        }

        private void UpdateUI()
        {
            CaptchaPanel.Visibility = captchaRequired ? Visibility.Visible : Visibility.Collapsed;
            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            if (!captchaRequired) return;

            var random = new Random();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789";
            currentCaptcha = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Здесь должна быть реализация отрисовки CAPTCHA с шумом
            // Для простоты выведем текст
            CaptchaText.Text = currentCaptcha;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new МедицинскаяЛабораторияEntities())
            {
                try
                {
                    var username = UsernameTextBox.Text;
                    var password = PasswordBox.Password;
                    var captcha = CaptchaTextBox.Text;

                    // Проверка блокировки
                    var ip = NetworkHelper.GetLocalIPAddress();
                    var isBlocked = db.Блокировки.Any(b => b.ip_адрес == ip && b.дата_окончания > DateTime.Now);

                    if (isBlocked)
                    {
                        MessageBox.Show("Ваш IP временно заблокирован. Попробуйте позже.");
                        return;
                    }

                    // Проверка CAPTCHA если требуется
                    if (captchaRequired && captcha != currentCaptcha)
                    {
                        MessageBox.Show("Неверная CAPTCHA");
                        failedAttempts++;
                        if (failedAttempts >= 3)
                        {
                            BlockUser(ip);
                        }
                        GenerateCaptcha();
                        return;
                    }

                    // Поиск пользователя
                    var user = db.Пользователи
                        .Include(u => u.Роли)
                        .FirstOrDefault(u => u.логин == username && u.пароль == password && u.активен == true);

                    if (user != null)
                    {
                        // Успешный вход
                        db.ИсторияВходов.Add(new ИсторияВходов
                        {
                            id_пользователя = user.id,
                            время_попытки = DateTime.Now,
                            успешна = true,
                            ip_адрес = ip,
                            captcha_использована = captchaRequired,
                            userAgent = "WPF Application"
                        });
                        db.SaveChanges();

                        // Обновляем время последнего входа
                        user.последний_вход = DateTime.Now;
                        db.SaveChanges();

                        // Открываем главное окно
                        OpenMainWindow(user);
                    }
                    else
                    {
                        // Неудачный вход
                        db.ИсторияВходов.Add(new ИсторияВходов
                        {
                            время_попытки = DateTime.Now,
                            успешна = false,
                            ip_адрес = ip,
                            captcha_использована = captchaRequired,
                            userAgent = "WPF Application"
                        });
                        db.SaveChanges();

                        failedAttempts++;
                        if (failedAttempts >= 2)
                        {
                            captchaRequired = true;
                            UpdateUI();
                        }

                        MessageBox.Show("Неверный логин или пароль");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при входе: {ex.Message}");
                }
            }
        }

        private void BlockUser(string ip)
        {
            using (var db = new МедицинскаяЛабораторияEntities())
            {
                db.Блокировки.Add(new Блокировки
                {
                    ip_адрес = ip,
                    время_начала = DateTime.Now,
                    длительность_минуты = 1, // Для теста 1 минута
                    причина = "Много неудачных попыток входа"
                });
                db.SaveChanges();
            }

            MessageBox.Show("Слишком много неудачных попыток входа. Ваш IP заблокирован на 1 минуту.");
            captchaRequired = false;
            failedAttempts = 0;
            UpdateUI();
        }

        private void OpenMainWindow(Пользователи user)
        {
            var mainWindow = new MainWindow(user);
            mainWindow.Show();
            this.Close();
        }

        private void ShowPasswordCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordBox.Visibility = Visibility.Collapsed;
            PasswordTextBox.Visibility = Visibility.Visible;
        }

        private void ShowPasswordCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
        }

        private void RegenerateCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateCaptcha();
        }
    }
}
