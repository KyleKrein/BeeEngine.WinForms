using System.Net.Sockets;

namespace BeeEngine.Multiplayer
{
    public sealed class Server : IDisposable
    {
        public bool Logging = false;
        public const string DefaultReply = "SERVER_ALIVE";
        private readonly int _asyncDelay = 40;
        private LinkedList<byte[]> messageQueue = new LinkedList<byte[]>();
        private LinkedList<BroadcastMessage> broadcastMessageQueue = new LinkedList<BroadcastMessage>();
        private LinkedList<byte[]> recievedMessagesB = new LinkedList<byte[]>();
        private LinkedList<Message> recievedMessages = new LinkedList<Message>();
        private readonly ILogger _logger;
        public event EventHandler<Message> MessageReceived;
        private WeakEvent<Message>? _weakMessageReceived;
        public WeakEvent<Message> WeakMessageReceived
        {
            get { return _weakMessageReceived ??= new WeakEvent<Message>(); }
            set
            {
                if (value.EventID != _weakMessageReceived!.EventID)
                    throw new InvalidOperationException("Can't set WeakEvent with different Event ID");
                _weakMessageReceived = value;
            }
        }

        private TaskCompletionSource<bool> _receivedMessage = new TaskCompletionSource<bool>();
        public Server()
        {
            MessageReceived += (sender, message) => { _receivedMessage.SetResult(true);};
        }

        public Server(TimeSpan asyncDelay)
        {
            _asyncDelay = asyncDelay.Milliseconds;
        }
        public void SendMessage(string message, bool CleanMessage = true, params TcpClient[] tcpClients)
        {
            lock (broadcastMessageQueue)
            {
                broadcastMessageQueue.AddLast(new BroadcastMessage(EncodeString(message, CleanMessage),
                    tcpClients.ToList()));
            }
        }

        public void SendMessageToAll(string message, bool cleanMessage = true, params TcpClient[] exceptTcpClients)
        {
            
            lock (Clients)
            {
                List<TcpClient> clients;
                clients = Clients.Except(exceptTcpClients).ToList();
                Log.Info($"Connected clients: {Clients.Count}, Clients to receive: {clients.Count}");
                if (clients.Count == 0)
                    return;
                lock (broadcastMessageQueue)
                {
                    broadcastMessageQueue.AddLast(new BroadcastMessage(EncodeString(message, cleanMessage),
                        clients));
                }
            }
        }

        public Message ReceiveMessage()
        {
            lock (recievedMessages)
            {
                Message m = recievedMessages.First();
                recievedMessages.RemoveFirst();
                return m;
            }
        }

        public Message? TryReceiveMessage()
        {
            if (recievedMessages.Count == 0)
                return null;
            lock (recievedMessages)
            {
                var message = recievedMessages.First();
                recievedMessages.RemoveFirst();
                return message;
            }
        }
        public async Task<Message> ReceiveMessageAsync()
        {
            await _receivedMessage.Task;
            Message m;
            lock (recievedMessages)
            {
                m = recievedMessages.First();
                recievedMessages.RemoveFirst();
            }
            return m;
        }
        //TODO: Сделать фильтрацию и отправку к выбранным клиентам
        public void SendMessage(byte[] message)
        {
            byte[] c = new byte[1] {2};
            var m = c.Concat(message).ToArray();
            lock (messageQueue)
            {
                messageQueue.AddLast(m);
            }
        }

        public void SendMessageToAll(byte[] message)
        {
            byte[] c = new byte[1] {2};
            var m = c.Concat(message).ToArray();
            lock (messageQueue)
            {
                broadcastMessageQueue.AddLast(new BroadcastMessage(m, Listener.ConnectedClients));
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

        //IPAddress IP;
        //int PORT;
        private const int ByteSize = 1024 * 1024; // Constant, not going to change later

        /*internal void Init(IPAddress ip, int port)
        {
            IPEndPoint ep = new IPEndPoint(ip, port); // Address
            IP = ip;
            PORT = port;
            listener = new TcpListener(ep); // Instantiate the object
            listener.Start(); // Start listening...

            GameEngine.Window.MessageReceiver = new BackgroundWorker();
            GameEngine.Window.MessageReceiver.DoWork += ServerRecieverDoWork;
            GameEngine.Window.MessageReceiver.RunWorkerAsync();
            // Just a few variables
            //RunServerAsync();
            Console.WriteLine(@"
            ===================================================
                   Started listening requests at: {0}:{1}
            ===================================================",
            ep.Address, ep.Port);
        }*/
        private SimpleListener Listener;
        private bool disposedValue;

        public Server(ILogger logger)
        {
            _logger = logger;
        }

        public static int MessageSize
        {
            get { return ByteSize - 1; }
        }

        //declare delegate to handle new connections
        public delegate void ConnectHandler(TcpClient tcpclient);

        //declare an event and event handler (the same for _new_client) for new connections OPTION 2
        public event ConnectHandler OnConnect;

        public event DisconnectHandler OnDisconnect;

        public delegate void DisconnectHandler(TcpClient p);

        /// <summary>
        /// Start the server
        /// </summary>
        /// <param name="ServerIP"></param>
        /// <param name="ServerPort"></param>
        public void StartServer(string ServerIP, int ServerPort)
        {
            Listener = new SimpleListener(ServerIP, ServerPort);

            Listener.NewTcpClient += Listener_new_tcp_client;
            Listener.OnDisconnect += Listener_OnDisconnect;
            Listener.Listen();
            Console.WriteLine(@"
            ===================================================
                   Started listening requests at: {0}:{1}
            ===================================================",
                ServerIP, ServerPort);
        }

        private void Listener_OnDisconnect(TcpClient tcpclient)
        {
            _logger.Log($"Клиент {tcpclient.GetIPAddress()} отключился", LogLevel.Information);
            if (OnDisconnect != null)
            {
                OnDisconnect(tcpclient);
            }
        }

        public List<TcpClient> Clients = new List<TcpClient>();
        private async void Listener_new_tcp_client(TcpClient tcpclient)
        {
            _logger.Log($"Клиент {tcpclient.GetIPAddress()} подключился", LogLevel.Information);
            
            OnConnect?.Invoke(tcpclient);

            await Task.Run(() =>
            {
                byte[] buffer;
                string message;
                byte[] bytes = new byte[ByteSize];
                lock (Clients)
                {
                    Clients.Add(tcpclient);
                }
                while (tcpclient.Client.IsConnected())
                {
                    message = null;
                    buffer = new byte[ByteSize];

                    try
                    {
                        tcpclient.GetStream().Read(buffer, 0, ByteSize);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    // Read the message and perform different actions
                    message = cleanMessage(buffer);
                    if (message != Client.DefaultReply && message != "NOT_A_STRING")
                        recievedMessages.AddLast(new Message(message, tcpclient));
                    if (Logging)
                        Log.Debug(message);
                    // Save the data sent by the client;
                    //Person person = JsonConvert.DeserializeObject<Person>(message); // Deserialize
                    bool broadcast = false;
                    lock (broadcastMessageQueue)
                    {
                        while (broadcastMessageQueue.Count > 0 && broadcastMessageQueue.First().isDone)
                        {
                            broadcastMessageQueue.RemoveFirst();
                        }

                        if (broadcastMessageQueue.Count > 0)
                        {
                            var currentMessage = broadcastMessageQueue.First();
                            if (currentMessage.SendTo(tcpclient))
                            {
                                broadcast = true;
                                bytes = currentMessage.message;
                                currentMessage.IRecieved(tcpclient);
                            }
                        }
                    }

                    if (!broadcast)
                    {
                        if (messageQueue.Count != 0)
                        {
                            bytes = messageQueue.First();
                            messageQueue.RemoveFirst();
                        }
                        else
                        {
                            bytes = EncodeString(DefaultReply);
                        }
                    }

                    string t = Encoding.Unicode.GetString(bytes[1..]);
                    if (t != DefaultReply)
                        Log.Info(t);

                    sendMessage(bytes, tcpclient);
                }
                lock (Clients)
                {
                    Clients.Remove(tcpclient);
                }
            });
        }

        private byte[] EncodeString(string String, bool CleanMessage = true)
        {
            byte[] c = new byte[1] {CleanMessage ? (byte) 1 : (byte) 0};
            var r = c.Concat(Encoding.Unicode.GetBytes(String)).ToArray();
            return r;
        }

        // Pass byte array as parameter
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

        // Sends the message string using the bytes provided and TCP client connected
        private static void sendMessage(byte[] bytes, TcpClient client)
        {
            // Client must be connected to
            client.GetStream() // Get the stream and write the bytes to it
                .Write(bytes, 0,
                    bytes.Length); // Send the stream
        }

        public delegate void ServerStopHandler(Server sender, List<TcpClient> clients);

        public event ServerStopHandler OnServerStop;

        public void Stop()
        {
            if (OnServerStop != null)
            {
                OnServerStop(this, Listener.ConnectedClients);
            }

            Listener.Stop();
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                    recievedMessages.Clear();
                    recievedMessagesB.Clear();
                    lock (broadcastMessageQueue)
                    {
                        broadcastMessageQueue.Clear();
                    }
                    messageQueue.Clear();
                }

                Stop();
                Listener.Dispose();
                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        ~Server()
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