using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

class UdpPriceServer
{
    static Dictionary<string, string> partsPrices = new Dictionary<string, string>()
    {
        { "процесор", "12000 грн" },
        { "відеокарта", "25000 грн" },
        { "оперативна пам'ять", "4000 грн" },
        { "жорсткий диск", "3000 грн" },
        { "материнська плата", "5000 грн" }
    };

    static void Main()
    {
        
        UdpClient udpServer = new UdpClient(150); 
        Console.WriteLine("Сервер запущено на порту 150...");

        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 150);
        
        
        while (true)
        {
            byte[] data = udpServer.Receive(ref remoteEP);
            string message = Encoding.UTF8.GetString(data);
            Console.WriteLine($"Отримано запит від {remoteEP}: {message}");

            string response;
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
        }
    }
}