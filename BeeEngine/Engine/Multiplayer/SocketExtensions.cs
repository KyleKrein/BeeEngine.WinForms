using System.Net;
using System.Net.Sockets;

namespace BeeEngine.Multiplayer
{
    public static class SocketExtensions
    {
        /// <summary>
        /// Extension method to tell if the Socket REALLY is closed
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool IsConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }
        public static IPAddress GetIPAddress(this TcpClient client)
        {
            return ((IPEndPoint)client.Client.RemoteEndPoint).Address;
        }
    }
}