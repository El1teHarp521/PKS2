using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using NetworkAnalyzer.Data;
using NetworkAnalyzer.Models;

namespace NetworkAnalyzer
{
    public partial class MainWindow : Window
    {
        private NetworkContext _db = new NetworkContext();

        public MainWindow()
        {
            InitializeComponent();
            _db.Database.EnsureCreated(); // Создает базу данных при первом запуске
            LoadInterfaces();
            LoadHistory();
        }

        // Загрузка списка сетевых карт
        private void LoadInterfaces()
        {
            lstInterfaces.ItemsSource = NetworkInterface.GetAllNetworkInterfaces()
                .Select(i => i.Name).ToList();
        }

        // Клик по списку интерфейсов
        private void InterfaceSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var ni = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(i => i.Name == lstInterfaces.SelectedItem?.ToString());
            
            if (ni == null) return;

            var ipProp = ni.GetIPProperties();
            var ipv4 = ipProp.UnicastAddresses.FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Тип: {ni.NetworkInterfaceType}");
            sb.AppendLine($"Статус: {ni.OperationalStatus}");
            sb.AppendLine($"MAC: {ni.GetPhysicalAddress()}");
            if (ipv4 != null) {
                sb.AppendLine($"IP: {ipv4.Address}");
                sb.AppendLine($"Mask: {ipv4.IPv4Mask}");
                sb.AppendLine($"Тип IP: {(IPAddress.IsLoopback(ipv4.Address) ? "Loopback" : "Внешний/Локальный")}");
            }
            txtInterfaceInfo.Text = sb.ToString();
        }

        // Парсинг URL
        private void AnalyzeUrl_Click(object sender, RoutedEventArgs e)
        {
            try {
                Uri uri = new Uri(txtUrlInput.Text);
                txtResults.Text = $"Протокол: {uri.Scheme}\nХост: {uri.Host}\nПорт: {uri.Port}\nПуть: {uri.AbsolutePath}\nПараметры: {uri.Query}";
                SaveHistory(txtUrlInput.Text, "Анализ");
            } catch { MessageBox.Show("Неверный формат URL!"); }
        }

        // Пинг
        private async void Ping_Click(object sender, RoutedEventArgs e)
        {
            try {
                Uri uri = new Uri(txtUrlInput.Text);
                Ping p = new Ping();
                var reply = await p.SendPingAsync(uri.Host);
                txtResults.Text = $"Пинг {uri.Host}: {reply.Status} ({reply.RoundtripTime}ms)";
                SaveHistory(txtUrlInput.Text, "Ping");
            } catch { MessageBox.Show("Ошибка пинга!"); }
        }

        // DNS
        private async void Dns_Click(object sender, RoutedEventArgs e)
        {
            try {
                Uri uri = new Uri(txtUrlInput.Text);
                var entry = await Dns.GetHostEntryAsync(uri.Host);
                txtResults.Text = $"DNS для {uri.Host}:\n" + string.Join("\n", entry.AddressList.Select(a => a.ToString()));
                SaveHistory(txtUrlInput.Text, "DNS");
            } catch { MessageBox.Show("Ошибка DNS!"); }
        }

        private void SaveHistory(string url, string res) {
            _db.Histories.Add(new UrlHistory { Url = url, CheckedAt = DateTime.Now, Result = res });
            _db.SaveChanges();
            LoadHistory();
        }

        private void LoadHistory() => lstHistory.ItemsSource = _db.Histories.ToList();
    }
}