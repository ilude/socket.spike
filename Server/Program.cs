using System;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        private static Socket socket;
        private static IOLoop loop;
        private static event EventHandler<ConnectionAcceptedEventArgs> ConnectionAccepted;

        static void Main(string[] args)
        {
            loop = new IOLoop();

            socket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                ConnectionAccepted += ProgramConnectionAccepted;
                socket.Bind(new IPEndPoint(IPAddress.Any, 3677));
                socket.Listen(5);

                Console.WriteLine("Listening...");

                socket.BeginAccept(AcceptCallback, null);
                loop.Start();
            }
            catch(Exception ex)
            {
                OnError(ex);
            }
        }

        static void ProgramConnectionAccepted(object sender, ConnectionAcceptedEventArgs e)
        {
            Console.WriteLine("Connection Accepted!");
            e.SocketStream.Socket.Dispose();
        }

        

        private static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                var sock = socket.EndAccept(ar);

                if (ConnectionAccepted != null)
                {
                    loop.NonBlockInvoke( () => ConnectionAccepted(null, new ConnectionAcceptedEventArgs(new SocketStream(loop, sock))) );
                }
                else
                {
                    sock.Dispose();
                }
                socket.BeginAccept(AcceptCallback, null);
            }
            catch(Exception ex)
            {
                OnError(ex);
            }
        }

        private static void OnError(Exception exception)
        {
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
        }
    }

    internal class ConnectionAcceptedEventArgs : EventArgs
    {
        public SocketStream SocketStream { get; set; }

        public ConnectionAcceptedEventArgs(SocketStream socketStream)
        {
            SocketStream = socketStream;
        }
    }

    internal class SocketStream
    {
        public IOLoop Loop { get; set; }
        public Socket Socket { get; set; }

        public SocketStream(IOLoop loop, Socket socket)
        {
            Loop = loop;
            Socket = socket;
        }
    }
}
