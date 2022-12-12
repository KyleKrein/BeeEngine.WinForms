using System.Net.Sockets;

namespace BeeEngine.Multiplayer
{
    public sealed class Client : IDisposable
    {
        public bool Logging = false;
        public const string DefaultReply = "CLIENT_ALIVE";
        private LinkedList<byte[]> messageQueue = new LinkedList<byte[]>();
        private LinkedList<byte[]> recievedMessagesB = new LinkedList<byte[]>();
        private LinkedList<string> recievedMessages = new LinkedList<string>();
        private readonly int _asyncDelay = 40;
        private readonly GameEngine _gameInstance;
        private readonly ILogger _logger;

        public Client()
        {
            _gameInstance = GameApplication.Instance;
            if (_gameInstance is null)
                throw new InvalidOperationException("Can't start client if Game is not started");
        }

        public Client(TimeSpan asyncDelay): this()
        {
            _asyncDelay = asyncDelay.Milliseconds;
        }


        public void SendMessage(string message, bool CleanMessage = true)
        {
            lock (messageQueue)
            {
                messageQueue.AddLast(EncodeString(message, CleanMessage));
            }
        }

        public string ReceiveMessage()
        {
            while (recievedMessages.Count == 0)
            {
                Thread.Sleep(5);
            }
            lock (recievedMessages)
            {
                string m = recievedMessages.First();
                recievedMessages.RemoveFirst();
                return m;
            }
        }
        public string? TryReceiveMessage()
        {
            if (recievedMessages.Count == 0)
                return null;
            lock (recievedMessages)
            {
                string m = recievedMessages.First();
                recievedMessages.RemoveFirst();
                return m;
            }
        }

        public async Task<string> ReceiveMessageAsync()
        {
            while (recievedMessages.Count == 0)
            {
                await Task.Delay(_asyncDelay);
            }

            string m;
            lock (recievedMessages)
            {
                m = recievedMessages.First();
                recievedMessages.RemoveFirst();
            }
            return m;
        }

        public void SendMessage(byte[] message)
        {
            byte[] c = new byte[1] { 2 };
            var m = c.Concat(message).ToArray();
            lock (messageQueue)
            {
                messageQueue.AddLast(m);
            }
        }

        public byte[] ReceiveMessageByte()
        {
            while (recievedMessagesB.Count == 0)
            {
                Thread.Sleep(5);
            }
            lock (recievedMessagesB)
            {
                var m = recievedMessagesB.First();
                recievedMessagesB.RemoveFirst();
                return m;
            }
        }

        private TcpClient client;
        private const int bytesize = 1024 * 1024;

        public void Init(string ip, int port)
        {
            IP = ip;
            PORT = port;
            client = new TcpClient(ip, port); // Create a new connection
            
            _gameInstance.Window.MessageReceiver = new BackgroundWorker();
            _gameInstance.Window.MessageReceiver.DoWork += ClientReceiverDoWork;
            _gameInstance.Window.MessageReceiver.RunWorkerAsync();
        }

        private string IP;
        private int PORT;
        private bool disposedValue;

        public Client(ILogger logger)
        {
            _logger = logger;
        }

        private void ClientReceiverDoWork(object s, DoWorkEventArgs e)
        {
            byte[] buffer;
            string message;
            byte[] bytes;
            while (true)
            {
                try
                {
                    /*if (!client.Connected)
                    {
                        client = new TcpClient(IP, PORT);
                    }*/
                    message = null;
                    buffer = new byte[bytesize];

                    if (messageQueue.Count != 0)
                    {
                        bytes = messageQueue.First();
                        messageQueue.RemoveFirst();
                    }
                    else
                    {
                        bytes = EncodeString(DefaultReply);
                    }
                    sendMessage(bytes, client);

                    client.GetStream().Read(buffer, 0, bytesize);

                    // Read the message and perform different actions
                    message = cleanMessage(buffer);
                    if (message != Server.DefaultReply && message != "NOT_A_STRING")
                    {
                        recievedMessages.AddLast(message);
                        _logger.Log(message, LogLevel.Debug);
                    }
                    if (Logging)
                        _logger.Log(message, LogLevel.Debug);
                    //client.Close();
                }
                catch (Exception ex)
                {
                    _logger.Log(ex.ToString(), LogLevel.Error);
                }
            }
        }

        private string cleanMessage(byte[] bytes)
        {
            // Get the string of the message from bytes
            string message = Encoding.Unicode.GetString(bytes[1..]);
            if (bytes[0] == 1)
            {
                string messageToPrint = null;
                // Loop through each character in that message
                for (int index = 0; index < message.Length; index++)
                {
                    char nullChar = message[index];
                    // Only store the characters, that are not null character
                    if (nullChar != '\0' && nullChar != 65533)
                    {
                        messageToPrint += nullChar;
                    }
                }
                
                /*
                // Return the message without null characters.
                Log.Error($"{messageToPrint}: {messageToPrint.Length}: {(int)messageToPrint[messageToPrint.Length-1]}");*/
                return messageToPrint;
            }
            if (bytes[0] == 2)
            {
                recievedMessagesB.AddLast(bytes.Skip(1).ToArray());
                return "NOT_A_STRING";
            }
            return message;
        }

        private static void sendMessage(byte[] bytes, TcpClient client)
        {
            // Client must be connected to
            client.GetStream() // Get the stream and write the bytes to it
                  .Write(bytes, 0,
                  bytes.Length); // Send the stream
        }

        private byte[] EncodeString(string String, bool CleanMessage = true)
        {
            byte[] c = new byte[1] { CleanMessage ? (byte)1 : (byte)0 };
            var r = c.Concat(Encoding.Unicode.GetBytes(String)).ToArray();
            return r;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    recievedMessages.Clear();
                    messageQueue.Clear();
                    recievedMessagesB.Clear();
                    if (_gameInstance.Window.MessageReceiver != null)
                    {
                        _gameInstance.Window.MessageReceiver.WorkerSupportsCancellation = true;
                        _gameInstance.Window.MessageReceiver.CancelAsync();
                        _gameInstance.Window.MessageReceiver.Dispose();
                    }
                    // TODO: освободить управляемое состояние (управляемые объекты)
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                if (client != null)
                {
                    if (client.Client.IsConnected())
                        client.Close();
                }
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        ~Client()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки в методе "Dispose(bool disposing)".
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}