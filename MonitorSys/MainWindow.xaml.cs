using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;
using MonitorSys.Models;

namespace MonitorSys
{
    public partial class MainWindow : Window
    {
        private HttpListener? _listener;
        private readonly HttpClient _httpClient = new HttpClient();
        private bool _isRunning = false;
        private DateTime _startTime;
        
        private int _getRequests = 0;
        private int _postRequests = 0;
        private readonly List<long> _processingTimes = new List<long>();

        public MainWindow()
        {
            InitializeComponent();
        }

        // --- ЗАПУСК СЕРВЕРА ---
        private async void StartServer_Click(object sender, RoutedEventArgs e)
        {
            if (_isRunning) return;

            if (!int.TryParse(txtServerPort.Text, out int port)) {
                MessageBox.Show("Неверный порт!"); return;
            }

            try {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{port}/");
                _listener.Start();
                
                _isRunning = true;
                _startTime = DateTime.Now;
                
                // Управление кнопками
                btnStartServer.IsEnabled = false;
                btnStopServer.IsEnabled = true;
                txtServerPort.IsEnabled = false;

                LogToServerUI("Сервер запущен", "System", 200);

                _ = Task.Run(() => ListenLoop());
                
                _ = Task.Run(async () => {
                    while (_isRunning) {
                        Dispatcher.Invoke(() => lblUptime.Text = $"Работает: {(int)(DateTime.Now - _startTime).TotalSeconds} сек");
                        await Task.Delay(1000);
                    }
                });
            } catch (Exception ex) {
                MessageBox.Show("Ошибка запуска: " + ex.Message);
            }
        }

        // --- ОСТАНОВКА СЕРВЕРА ---
        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRunning || _listener == null) return;

            try {
                _isRunning = false;
                _listener.Stop();
                _listener.Close();
                _listener = null;

                // Управление кнопками
                btnStartServer.IsEnabled = true;
                btnStopServer.IsEnabled = false;
                txtServerPort.IsEnabled = true;

                LogToServerUI("Сервер остановлен", "System", 0);
            } catch (Exception ex) {
                MessageBox.Show("Ошибка при остановке: " + ex.Message);
            }
        }

        private async Task ListenLoop()
        {
            while (_isRunning && _listener != null)
            {
                try {
                    var context = await _listener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequest(context)); 
                } catch { 
                    break; 
                }
            }
        }

        private async Task ProcessRequest(HttpListenerContext context)
        {
            Stopwatch sw = Stopwatch.StartNew();
            var request = context.Request;
            var response = context.Response;
            string responseString = "";

            try {
                if (request.HttpMethod == "GET") {
                    _getRequests++;
                    responseString = JsonConvert.SerializeObject(new { 
                        status = "Online", 
                        gets = _getRequests,
                        posts = _postRequests
                    });
                } 
                else if (request.HttpMethod == "POST") {
                    _postRequests++;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding)) {
                        string body = await reader.ReadToEndAsync();
                        Dispatcher.Invoke(() => lstMessages.Items.Insert(0, body));
                        responseString = JsonConvert.SerializeObject(new { id = Guid.NewGuid().ToString(), status = "Received" });
                    }
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.ContentType = "application/json";
                await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            } catch {
                // Обработка ошибок записи в поток
            }
            finally {
                sw.Stop();
                _processingTimes.Add(sw.ElapsedMilliseconds);
                LogToServerUI(request.HttpMethod, request.Url?.ToString() ?? "Unknown", 200);
                UpdateStats();
                try { response.Close(); } catch { }
            }
        }

        private void LogToServerUI(string method, string url, int status)
        {
            var log = new LogEntry { Method = method, Url = url, StatusCode = status, Type = "Входящий" };
            Dispatcher.Invoke(() => {
                lstServerLogs.Items.Insert(0, log.ToString());
            });
            File.AppendAllText("logs.txt", log.ToString() + Environment.NewLine);
        }

        private void UpdateStats()
        {
            Dispatcher.Invoke(() => {
                lblGetCount.Text = $"GET запросов: {_getRequests}";
                lblPostCount.Text = $"POST запросов: {_postRequests}";
                if (_processingTimes.Count > 0)
                    lblAvgTime.Text = $"Ср. время: {(int)_processingTimes.Average()} мс";
            });
        }

        private async void SendRequest_Click(object sender, RoutedEventArgs e)
        {
            string url = txtClientUrl.Text;
            var selectedItem = cbMethod.SelectedItem as ComboBoxItem;
            string method = selectedItem?.Content?.ToString() ?? "GET";
            
            try {
                HttpResponseMessage response;
                if (method == "GET") {
                    response = await _httpClient.GetAsync(url);
                } else {
                    var content = new StringContent(txtClientBody.Text, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync(url, content);
                }

                string result = await response.Content.ReadAsStringAsync();
                txtClientResponse.Text = $"Статус: {response.StatusCode}\nТело:\n{result}";
            }
            catch (Exception ex) {
                txtClientResponse.Text = "ОШИБКА: " + ex.Message;
            }
        }
    }
}