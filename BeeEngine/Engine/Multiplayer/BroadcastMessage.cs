using System.Net.Sockets;

namespace BeeEngine.Multiplayer
{
    internal sealed class BroadcastMessage
    {
        public bool isDone { get; private set; }
        public byte[] message { get; private set; }
        List<TcpClient> needToGet;
        public BroadcastMessage(byte[] Message, List<TcpClient> WhoNeedsToGet)
        {
            message = Message;
            needToGet = WhoNeedsToGet;
        }
        public void IRecieved(TcpClient client)
        {
            lock(needToGet)
            {
                needToGet.Remove(client);
                if (needToGet.Count == 0)
                {
                    isDone = true;
                }
            }
        }
        public bool SendTo(TcpClient tcpClient)
        {
            return needToGet.Contains(tcpClient);
        }
    }

    public readonly struct Message
    {
        public string Text { get;}
        public TcpClient Client { get;}

        internal Message(string text, TcpClient client)
        {
            Text = text;
            Client = client;
        }
    }
}