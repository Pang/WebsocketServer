using System.IO.Ports;
using System.Net.WebSockets;
using System.Text;

class Program
{
    const string listenerAddress = "http://localhost:5000/ws/";
    const string portName = "COM6";
    const int baudRate = 9600;

    private static SerialPort serialPort;

    static async Task Main(string[] args)
    {
        ClientWebSocket client = new ClientWebSocket();
        await client.ConnectAsync(new Uri(listenerAddress), CancellationToken.None);
        Console.WriteLine("Connected to WebSocket server");

        _ = Task.Run(async () =>
        {
            byte[] buffer = new byte[1024];
            while (client.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine("Received from Websocket: " + message);
                SendSerialMessageToArduino(message);
            }
        });

        static void SendSerialMessageToArduino(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            serialPort.WriteLine(message);
            Console.WriteLine($"'{message}' sent to Arduino through Serial.");
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