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

namespace ClientUDP;

public partial class MainWindow : Window
{
    private UdpClient client;
    private IPEndPoint serverEP;

    public MainWindow()
    {
        InitializeComponent();
        client = new UdpClient();
        serverEP = new IPEndPoint(IPAddress.Loopback, 150);
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        string input = InputTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            ResponseTextBlock.Text = "Введіть назву комплектуючої!";
            return;
        }

        try
        {
            byte[] data = Encoding.UTF8.GetBytes(input);
            await client.SendAsync(data, data.Length, serverEP);

            var result = await client.ReceiveAsync();
            string response = Encoding.UTF8.GetString(result.Buffer);

            ResponseTextBlock.Text = response;
        }
        catch (Exception ex)
        {
            ResponseTextBlock.Text = $"Помилка: {ex.Message}";
        }
    }
    
    private void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        string serverIp = ServerIpTextBox.Text.Trim();
        string serverPortText = ServerPortTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(serverIp) || string.IsNullOrWhiteSpace(serverPortText))
        {
            ResponseTextBlock.Text = "Введіть IP-адресу та порт сервера!";
            return;
        }

        if (!int.TryParse(serverPortText, out int serverPort) || serverPort <= 0 || serverPort > 65535)
        {
            ResponseTextBlock.Text = "Некоректний порт!";
            return;
        }

        try
        {
            serverEP = new IPEndPoint(IPAddress.Parse(serverIp), serverPort);
            ResponseTextBlock.Text = $"Підключено до сервера {serverIp}:{serverPort}";
        }
        catch (Exception ex)
        {
            ResponseTextBlock.Text = $"Помилка підключення: {ex.Message}";
        }
    }
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        client?.Close();
    }
}