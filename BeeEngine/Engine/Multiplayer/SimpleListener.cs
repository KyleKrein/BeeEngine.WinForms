using System.Net;
using System.Net.Sockets;

namespace BeeEngine.Multiplayer
{
    internal sealed class SimpleListener : IDisposable
    {
        public int DisconnectionDelay = 1000;

        private readonly TcpListener _tcpListen;

        //declare delegate to handle new connections
        public delegate void NewClient(TcpClient tcpClient);

        //declare a clients container (or something like...). OPTION 1
        private readonly List<TcpClient> _connectedClients = new List<TcpClient>();
        private bool disposedValue;

        //declare an event and event handler (the same for _new_client) for new connections OPTION 2
        public event NewClient NewTcpClient;


        //public (The list of connected clients).
        public List<TcpClient> ConnectedClients
        {
            get
            {
                lock (_connectedClients)
                {
                    return _connectedClients;
                }
            }
        }


        public SimpleListener(string ip, int listenport)
        {
            IPAddress ipAd = IPAddress.Parse(ip);
            TcpListener tcpListener = new TcpListener(new IPEndPoint(ipAd, listenport));
            _tcpListen = tcpListener;
            NewTcpClient += SimpleListener_new_tcp_client;
            OnDisconnect += SimpleListener_OnDisconnect;
        }

        private void SimpleListener_OnDisconnect(TcpClient p)
        {
            lock (_connectedClients)
            {
                _connectedClients.Remove(p);
            }
        }

        private async void SimpleListener_new_tcp_client(TcpClient tcpclient)
        {
            while (tcpclient.Client.IsConnected())
            {
                await Task.Delay(DisconnectionDelay);
            }

            OnDisconnect(tcpclient);
        }

        //Fire this method to start listening...
        public void Listen()
        {
            _tcpListen.Start();
            SetListen();
        }

        //... and this method to stop listener and release resources on listener
        public void Stop()
        {
            _tcpListen.Stop();
        }

        //This method set the socket on listening mode... 
        private void SetListen()
        {
            //Let's do it asynchronously - Set the method callback, with the same definition as delegate _new_client
            _tcpListen.BeginAcceptTcpClient(OnNewClient, null);
        }

        /// <summary>
        /// Event is triggered when the peer is disconnecting
        /// </summary>
        public event DisconnectHandler OnDisconnect;

        public delegate void DisconnectHandler(TcpClient p);

        //This is the callback for new clients
        private void OnNewClient(IAsyncResult asyncClient)
        {
            try
            {
                //Lets get the new client...
                TcpClient _tcp_cl = _tcpListen.EndAcceptTcpClient(asyncClient);
                //Push the new client to the list
                lock (_connectedClients)
                {
                    _connectedClients.Add(_tcp_cl);
                }

                //OPTION 2 : Fire new_tcp_client Event - Suscribers will do some stuff...
                NewTcpClient?.Invoke(_tcp_cl);
                //Set socket on listening mode again... (and wait for new connections, while we can manage the new client connection)
                SetListen();
            }
            catch (Exception ex)
            {
                //Do something...or not
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты)
                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить метод завершения
                // TODO: установить значение NULL для больших полей
                Stop();
                DisconnectionDelay = 1;
                lock (_connectedClients)
                {
                    foreach (var client in _connectedClients)
                    {
                        client.Close();
                    }
                }

                disposedValue = true;
            }
        }

        // // TODO: переопределить метод завершения, только если "Dispose(bool disposing)" содержит код для освобождения неуправляемых ресурсов
        ~SimpleListener()
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