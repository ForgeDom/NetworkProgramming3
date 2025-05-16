using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UdpPriceClient
{
    static void Main()
    {
        UdpClient client = new UdpClient();
        IPEndPoint serverEP = new IPEndPoint(IPAddress.Loopback, 150);

        Console.WriteLine("UDP Клієнт запущений. Введіть назву комплектуючої або 'вихід':");

        while (true)
        {
            Console.Write("-> ");
            string input = Console.ReadLine();

            if (input.ToLower() == "вихід")
                break;

            byte[] data = Encoding.UTF8.GetBytes(input);
            client.Send(data, data.Length, serverEP);

            var responseData = client.Receive(ref serverEP);
            string response = Encoding.UTF8.GetString(responseData);

            Console.WriteLine($"Відповідь сервера: {response}");
        }

        client.Close();
    }
}