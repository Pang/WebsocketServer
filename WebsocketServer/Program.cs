using System.Collections.Concurrent;
using System.IO.Ports;
using System.Net;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketServer
{
    class Program
    {
        const string listenerAddress = "http://localhost:5000/ws/";
        const string portName = "COM6";
        const int baudRate = 9600;

        private static ConcurrentBag<WebSocket> _sockets = new ConcurrentBag<WebSocket>();
        private static SerialPort serialPort;

        static async Task Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(listenerAddress);
            listener.Start();
            Console.WriteLine($"WebSocket server started at {listenerAddress}");

            ConnectToArduino();

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    WebSocket webSocket = wsContext.WebSocket;
                    _sockets.Add(webSocket);
                    Console.WriteLine("New connection made: " + wsContext.SecWebSocketKey);

                    _ = Task.Run(async () =>
                    {
                        byte[] buffer = new byte[1024];
                        while (webSocket.State == WebSocketState.Open)
                        {
                            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            Console.WriteLine("Received: " + message);
                            
                            SendSerialMessageToArduino(message);

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                _sockets.TryTake(out webSocket);
                                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                            }
                            else
                            {
                                await BroadcastMessage("Message received: " + message);
                            }
                        }
                    });
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private static async Task BroadcastMessage(string message)
        {
            byte[] response = Encoding.UTF8.GetBytes(message);
            foreach (var socket in _sockets)
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(new ArraySegment<byte>(response), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        static void SendSerialMessageToArduino(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            serialPort.WriteLine(message);
            Console.WriteLine($"'{message}' sent.");
        }

        static void ConnectToArduino()
        {
            serialPort = new SerialPort(portName, baudRate);
            try
            {
                serialPort.Open();

                if (serialPort.IsOpen)
                {
                    Console.WriteLine("Serial port to arduino opened successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to open serial port.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    Console.WriteLine("Serial port closed.");
                }
            }
        }
    }
}