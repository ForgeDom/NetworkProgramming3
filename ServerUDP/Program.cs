using System;
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
    
    const int maxClients = 5;
    static readonly TimeSpan timeWindow = TimeSpan.FromHours(1);
    static readonly TimeSpan inactivityTimeout = TimeSpan.FromMinutes(1);
    static Dictionary<string, DateTime> activeClients = new Dictionary<string, DateTime>();
    static Dictionary<string, (int requestCount, DateTime lastRequestTime)> clientRequests = new Dictionary<string, (int, DateTime)>();
    const int requestLimit = 10;

    static void Main()
    {
        UdpClient udpServer = new UdpClient(150);
        Console.WriteLine("Сервер запущено на порту 150...");

        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 150);
        
        Thread cleanupThread = new Thread(CleanupInactiveClients);
        cleanupThread.Start();

        while (true)
        {
            byte[] data = udpServer.Receive(ref remoteEP);
            string clientIP = remoteEP.Address.ToString();
            string message = Encoding.UTF8.GetString(data);
            Console.WriteLine($"Отримано запит від {remoteEP}: {message}");

            if (!activeClients.ContainsKey(clientIP))
            {
                if (activeClients.Count >= maxClients)
                {
                    string limitMessage = "Перевищено ліміт одночасних підключень. Спробуйте пізніше.";
                    byte[] limitData = Encoding.UTF8.GetBytes(limitMessage);
                    udpServer.Send(limitData, limitData.Length, remoteEP);
                    continue;
                }

                // Додавання нового клієнта
                activeClients[clientIP] = DateTime.Now;
            }
            else
            {
                // Оновлення часу останньої активності клієнта
                activeClients[clientIP] = DateTime.Now;
            }
            
            if (IsClientRateLimited(clientIP))
            {
                string rateLimitMessage = "Перевищено ліміт запитів. Спробуйте пізніше.";
                byte[] rateLimitData = Encoding.UTF8.GetBytes(rateLimitMessage);
                udpServer.Send(rateLimitData, rateLimitData.Length, remoteEP);
                continue;
            }

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

    static bool IsClientRateLimited(string clientIP)
    {
        if (clientRequests.TryGetValue(clientIP, out var clientData))
        {
            if (DateTime.Now - clientData.lastRequestTime <= timeWindow)
            {
                if (clientData.requestCount >= requestLimit)
                {
                    return true; 
                }
                else
                {
                    clientRequests[clientIP] = (clientData.requestCount + 1, DateTime.Now);
                }
            }
            else
            {
                clientRequests[clientIP] = (1, DateTime.Now);
            }
        }
        else
        {
            clientRequests[clientIP] = (1, DateTime.Now);
        }

        return false;
    }
    
    static void CleanupInactiveClients()
    {
        while (true)
        {
            Thread.Sleep(60000); // Перевірка кожну хвилину
            DateTime now = DateTime.Now;

            foreach (var client in new List<string>(activeClients.Keys))
            {
                if (now - activeClients[client] > inactivityTimeout)
                {
                    Console.WriteLine($"Клієнт {client} відключений через неактивність.");
                    activeClients.Remove(client);
                }
            }
        }
    }
}