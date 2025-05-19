using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
using System.Xml.Linq;
using ZXing;
using ZXing.Common;

namespace medlaaab
{
    /// <summary>
    /// Логика взаимодействия для OrderCreationWindow.xaml
    /// </summary>
    public partial class OrderCreationWindow : Window
    {
        private Пользователи currentUser;
        private List<Услуги> selectedServices = new List<Услуги>();
        private Пациенты currentPatient;
        private string generatedBarcode;
        private BarcodeScannerService _barcodeScanner;

        public OrderCreationWindow(Пользователи user)
        {
            InitializeComponent();
            currentUser = user;
            InitializeBarcodeField();
            LoadInsuranceCompanies();

            // Инициализация сканера
            _barcodeScanner = new BarcodeScannerService();
            _barcodeScanner.StartListening(this, barcode =>
            {
                Dispatcher.Invoke(() =>
                {
                    BarcodeTextBox.Text = barcode;
                    ValidateAndGenerateBarcode();
                });
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            _barcodeScanner.StopListening();
            base.OnClosed(e);
        }

        private void InitializeBarcodeField()
        {
            using (var db = new МедицинскаяЛабораторияEntities())
            {
                var lastOrder = db.Заказы.OrderByDescending(o => o.id).FirstOrDefault();
                int nextId = lastOrder != null ? lastOrder.id + 1 : 1;
                BarcodeTextBox.Text = nextId.ToString();
                BarcodeTextBox.ToolTip = $"Последний номер заказа: {lastOrder?.id ?? 0}. Предлагаемый: {nextId}";
            }
        }

        private void LoadInsuranceCompanies()
        {
            using (var db = new МедицинскаяЛабораторияEntities())
            {
                InsuranceCompanyComboBox.ItemsSource = db.СтраховыеКомпании.ToList();
            }
        }

        private void BarcodeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidateAndGenerateBarcode();
            }
        }

        private void ValidateAndGenerateBarcode()
        {
            string barcode = BarcodeTextBox.Text.Trim();

            if (string.IsNullOrEmpty(barcode))
            {
                MessageBox.Show("Введите код пробирки");
                return;
            }

            using (var db = new МедицинскаяЛабораторияEntities())
            {
                bool exists = db.Заказы.Any(o => o.штрих_код == barcode && o.в_архиве == false);
                if (exists)
                {
                    MessageBox.Show("Заказ с таким кодом уже существует");
                    return;
                }
            }

            // Генерация уникального кода
            var random = new Random();
            string uniqueCode = new string(Enumerable.Repeat("0123456789", 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            generatedBarcode = $"{barcode} {DateTime.Now:ddMMyyyy} {uniqueCode}";
            GenerateBarcodeImage(generatedBarcode);
        }

        private void GenerateBarcodeImage(string barcodeText)
        {
            var writer = new BarcodeWriter
            {
                Format = BarcodeFormat.CODE_128,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300,
                    Margin = 10
                }
            };

            var barcodeBitmap = writer.Write(barcodeText);
            BarcodeImage.Source = barcodeBitmap.ToBitmapImage();
            SaveBarcodeButton.IsEnabled = true;
        }

        private void SaveBarcodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(generatedBarcode))
            {
                MessageBox.Show("Сначала сгенерируйте штрих-код");
                return;
            }

            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF файлы (*.pdf)|*.pdf",
                FileName = $"Штрих-код_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                CreateBarcodePdf(generatedBarcode, saveDialog.FileName);
                MessageBox.Show("Штрих-код сохранен в PDF");
            }
        }

        private void CreateBarcodePdf(string barcodeText, string filePath)
        {
            Document document = new Document(new Rectangle(100f, 50f));
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            document.Open();

            // Создаем штрих-код для PDF
            Barcode128 barcode128 = new Barcode128
            {
                Code = barcodeText,
                BarHeight = 22.85f,
                X = 0.15f,
                Font = null,
                Size = 10f,
                Baseline = 10f,
                GuardBars = true
            };

            Image barcodeImage = barcode128.CreateImageWithBarcode(writer.DirectContent, null, null);
            document.Add(barcodeImage);

            // Добавляем текстовое представление
            document.Add(new Paragraph(barcodeText));

            document.Close();
        }

        private void FindPatientButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = PatientSearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Введите ФИО пациента для поиска");
                return;
            }

            using (var db = new МедицинскаяЛабораторияEntities())
            {
                // Нечеткий поиск по расстоянию Левенштейна
                var patients = db.Пациенты.ToList()
                    .Where(p => LevenshteinDistance.Compute(p.ФИО.ToLower(), searchText.ToLower()) <= 3)
                    .ToList();

                if (patients.Count == 0)
                {
                    var result = MessageBox.Show("Пациент не найден. Создать нового?", "Пациент не найден",
                        MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        var addPatientWindow = new AddPatientWindow();
                        if (addPatientWindow.ShowDialog() == true)
                        {
                            currentPatient = addPatientWindow.NewPatient;
                            DisplayPatientInfo();
                        }
                    }
                }
                else if (patients.Count == 1)
                {
                    currentPatient = patients.First();
                    DisplayPatientInfo();
                }
                else
                {
                    var selectionWindow = new PatientSelectionWindow(patients);
                    if (selectionWindow.ShowDialog() == true)
                    {
                        currentPatient = selectionWindow.SelectedPatient;
                        DisplayPatientInfo();
                    }
                }
            }
        }

        private void DisplayPatientInfo()
        {
            if (currentPatient != null)
            {
                PatientInfoTextBlock.Text = $"{currentPatient.фамилия} {currentPatient.имя} {currentPatient.отчество}\n" +
                                          $"Дата рождения: {currentPatient.дата_рождения:dd.MM.yyyy}\n" +
                                          $"Полис: {currentPatient.номер_полиса}";
            }
        }

        private void AddServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var serviceWindow = new ServiceSelectionWindow();
            if (serviceWindow.ShowDialog() == true)
            {
                selectedServices.AddRange(serviceWindow.SelectedServices);
                UpdateServicesList();
                CalculateTotal();
            }
        }

        private void UpdateServicesList()
        {
            ServicesListBox.ItemsSource = null;
            ServicesListBox.ItemsSource = selectedServices;
        }

        private void CalculateTotal()
        {
            decimal total = selectedServices.Sum(s => s.стоимость);
            TotalTextBlock.Text = $"Итого: {total:C}";
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentPatient == null)
            {
                MessageBox.Show("Выберите пациента");
                return;
            }

            if (selectedServices.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну услугу");
                return;
            }

            if (string.IsNullOrEmpty(generatedBarcode))
            {
                MessageBox.Show("Сгенерируйте штрих-код");
                return;
            }

            using (var db = new МедицинскаяЛабораторияEntities())
            {
                var order = new Заказы
                {
                    id_пациента = currentPatient.id,
                    id_врача = currentUser.id,
                    дата_создания = DateTime.Now,
                    статус = "Новый",
                    в_архиве = false,
                    штрих_код = generatedBarcode.Split(' ')[0] // Берем только ID
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

                // Генерация PDF с заказом
                GenerateOrderPdf(order);

                MessageBox.Show($"Заказ #{order.id} успешно создан");
                this.DialogResult = true;
                this.Close();
            }
        }

        private void GenerateOrderPdf(Заказы order)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF файлы (*.pdf)|*.pdf",
                FileName = $"Заказ_{order.id}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            };

            if (saveDialog.ShowDialog() == true)
            {
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(saveDialog.FileName, FileMode.Create));

                document.Open();

                // Заголовок
                document.Add(new Paragraph("Медицинская лаборатория №20", new Font(Font.FontFamily.HELVETICA, 18, Font.BOLD)));
                document.Add(new Paragraph($"Заказ №{order.id}", new Font(Font.FontFamily.HELVETICA, 14, Font.BOLD)));
                document.Add(new Paragraph($"Дата: {order.дата_создания:dd.MM.yyyy HH:mm}"));
                document.Add(new Paragraph(" "));

                // Информация о пациенте
                document.Add(new Paragraph("Пациент:", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)));
                document.Add(new Paragraph($"{currentPatient.фамилия} {currentPatient.имя} {currentPatient.отчество}"));
                document.Add(new Paragraph($"Дата рождения: {currentPatient.дата_рождения:dd.MM.yyyy}"));
                document.Add(new Paragraph($"Полис: {currentPatient.номер_полиса}"));
                document.Add(new Paragraph(" "));

                // Услуги
                document.Add(new Paragraph("Услуги:", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)));
                PdfPTable table = new PdfPTable(2);
                table.AddCell("Услуга");
                table.AddCell("Стоимость");

                foreach (var service in selectedServices)
                {
                    table.AddCell(service.наименование);
                    table.AddCell(service.стоимость.ToString("C"));
                }

                document.Add(table);
                document.Add(new Paragraph($"Итого: {selectedServices.Sum(s => s.стоимость):C}"));

                // Генерация ссылки
                string orderData = $"дата_заказа={order.дата_создания:yyyy-MM-ddTHH:mm:ss}&номер_заказа={order.id}";
                string base64Data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(orderData));
                string orderLink = $"https://wsrussia.ru/?data={base64Data}";

                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("Ссылка на заказ:", new Font(Font.FontFamily.HELVETICA, 12, Font.BOLD)));
                document.Add(new Paragraph(orderLink));

                // Сохраняем ссылку в текстовый файл
                string linkFilePath = Path.Combine(Path.GetDirectoryName(saveDialog.FileName),
                    $"Заказ_{order.id}_ссылка.txt");
                File.WriteAllText(linkFilePath, orderLink);

                document.Close();
            }
        }
    }

    public static class LevenshteinDistance
    {
        public static int Compute(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 0; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }

    public static class BitmapExtensions
    {
        public static System.Windows.Media.Imaging.BitmapImage ToBitmapImage(this System.Drawing.Bitmap bitmap)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
