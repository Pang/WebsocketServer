using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            ClientWebSocket client = new ClientWebSocket();
            await client.ConnectAsync(new Uri("ws://localhost:5000/ws/"), CancellationToken.None);
            Console.WriteLine("Connected to WebSocket server");

            _ = Task.Run(async () =>
            {
                byte[] buffer = new byte[1024];
                while (client.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine("Received from server: " + message);
                }
            });

            while (client.State == WebSocketState.Open)
            {
                string message = Console.ReadLine();
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                await client.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
}