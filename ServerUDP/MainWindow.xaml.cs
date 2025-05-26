
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ServerUDP;

public partial class MainWindow : Window
{
    private UdpClient udpServer;
        private IPEndPoint remoteEP;
        private CancellationTokenSource cts;

        private Dictionary<string, string> partsPrices = new Dictionary<string, string>()
        {
            { "процесор", "12000 грн" },
            { "відеокарта", "25000 грн" },
            { "оперативна пам'ять", "4000 грн" },
            { "жорсткий диск", "3000 грн" },
            { "материнська плата", "5000 грн" }
        };

        public MainWindow()
        {
            InitializeComponent();
            UpdateStatus("Сервер не запущений.");
        }

        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            cts = new CancellationTokenSource();
            Task.Run(() => RunServer(cts.Token));
            UpdateStatus("Сервер запущено.");
            AddLog("Очікування запитів...");
        }

        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            cts?.Cancel();
            udpServer?.Close();
            UpdateStatus("Сервер зупинено.");
            AddLog("Сервер зупинено.");
        }
        
        private HashSet<string> connectedClients = new HashSet<string>();
        private void RunServer(CancellationToken token)
        {
            udpServer = new UdpClient(150);
            remoteEP = new IPEndPoint(IPAddress.Any, 150);
        
            try
            {
                while (!token.IsCancellationRequested)
                {
                    byte[] data = udpServer.Receive(ref remoteEP);
                    string message = Encoding.UTF8.GetString(data);
                    string response;
        
                    if (connectedClients.Add(remoteEP.Address.ToString()))
                    {
                        string clientConnectedMessage = $"Новий клієнт підключився: {remoteEP.Address}";
                        Dispatcher.Invoke(() => AddLog(clientConnectedMessage));
                        Logger.Log(clientConnectedMessage);
                    }
        
                    string clientRequestMessage = $"[{remoteEP}] Запит: {message}";
                    Dispatcher.Invoke(() => AddLog(clientRequestMessage));
                    Logger.Log(clientRequestMessage);
        
                    if (partsPrices.TryGetValue(message.ToLower(), out response))
                    {
                        response = $"Ціна на {message}: {response}";
                    }
                    else
                    {
                        response = $"Невідомий компонент: {message}";
                    }
        
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    udpServer.Send(responseData, responseData.Length, remoteEP);
        
                    string serverResponseMessage = $"→ Відповідь для [{remoteEP}]: {response}";
                    Dispatcher.Invoke(() => AddLog(serverResponseMessage));
                    Logger.Log(serverResponseMessage);
                }
            }
            catch (SocketException)
            {
                string stopMessage = "Сервер було зупинено.";
                Dispatcher.Invoke(() => AddLog(stopMessage));
                Logger.Log(stopMessage);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Помилка: {ex.Message}";
                Dispatcher.Invoke(() => AddLog(errorMessage));
                Logger.Log(errorMessage);
            }
        }

        private void AddLog(string text)
        {
            LogListBox.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {text}");
        }

        private void UpdateStatus(string status)
        {
            this.DataContext = new { ServerStatus = status };
        }
}

public static class Logger
{
    private static readonly string logFilePath = "server_log.txt";

    public static void Log(string message)
    {
        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
        File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
    }
}