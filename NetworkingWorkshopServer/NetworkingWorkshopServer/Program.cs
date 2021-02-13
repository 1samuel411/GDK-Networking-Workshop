using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetworkingWorkshopServer
{
    class Program
    {

        public static Server server;

        static void Main(string[] args)
        {
            Console.WriteLine("What IP should we use: ");
            string ip = Console.ReadLine();

            Console.WriteLine("What Port: ");
            string port = Console.ReadLine();

            server = new Server();
            server.Setup(ip, port);

            ListenForCommands();
        }

        static void ListenForCommands()
        {
            while (true)
            {
                string command = Console.ReadLine();

                if(command == "exit")
                {
                    break;
                }
            }
        }
    }

    public class Server
    {
        private Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private byte[] buffer = new byte[1024];
        private Dictionary<Socket, int> connectedUsers = new Dictionary<Socket, int>();

        public void Setup(string ip, string port)
        {
            Console.WriteLine("Creating a server on IP: " + ip + ":" + port);

            IPAddress iPAddress = IPAddress.Parse(ip);
            serverSocket.Bind(new IPEndPoint(iPAddress, int.Parse(port)));
            serverSocket.Listen(100);

            BeginAccepting();
        }

        private void BeginAccepting()
        {
            serverSocket.BeginAccept(AcceptedConnection, null);
        }

        void AcceptedConnection(IAsyncResult Result)
        {
            if (serverSocket == null)
                return;

            Socket socket = serverSocket.EndAccept(Result);

            int uniqueUserId = new Random().Next(3, 500);
            while(connectedUsers.Values.Any(x => x == uniqueUserId))
            {
                uniqueUserId++;
            }

            connectedUsers.Add(socket, uniqueUserId);
            Console.WriteLine("Recieved Connection: (" + uniqueUserId + ") " + socket.AddressFamily);

            SendToClient(socket, 0, uniqueUserId.ToString());

            BeginAccepting();
            BeginReceiving(socket);
        }

        private void BeginReceiving(Socket socket)
        {
            try 
            {
                socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, RecieveCallback, socket);
            }
            catch (Exception)
            {
                Console.WriteLine("Client " + connectedUsers[socket] + " disconnected");
                connectedUsers.Remove(socket);
            }
        }

        private void RecieveCallback(IAsyncResult Result)
        {
            Socket clientSocket = (Socket)Result.AsyncState;
            if(clientSocket.Connected == false)
            {
                Console.WriteLine("Attempting to recieve from disconnected client");
                return;
            }

            int recieved = clientSocket.EndReceive(Result);

            if (recieved <= 0)
            {
                BeginReceiving(clientSocket);
                return;
            }

            byte[] dataRecieved = new byte[recieved];
            
            Array.Copy(buffer, dataRecieved, recieved);

            int clientSender = connectedUsers[clientSocket];

            byte header = dataRecieved[0];
            byte[] messageData = new byte[dataRecieved.Length - 1];
            Array.Copy(dataRecieved, 1, messageData, 0, messageData.Length);

            string messageRecieved = Encoding.UTF8.GetString(messageData);

            Console.WriteLine("Recieved from (" + clientSender + "): " + header + " - " + messageRecieved);

            if(header == 0)
            {

            }
            else if(header == 10)
            {
                Console.WriteLine("Relaying chat message to all clients");
                foreach(KeyValuePair<Socket, int> user in connectedUsers)
                {
                    SendToClient(user.Key, 9, messageRecieved);
                }
            }

            BeginReceiving(clientSocket);
        }

        private void SendToClient(Socket socket, int header, string message)
        {
            if (socket.Connected == false)
            {
                Console.WriteLine("Not connected!");
                return;
            }

            byte[] tempArray = Encoding.UTF8.GetBytes(message);

            byte[] dataArray = new byte[tempArray.Length + 1];
            dataArray[0] = Convert.ToByte(header);
            Array.Copy(tempArray, 0, dataArray, 1, tempArray.Length);

            Console.WriteLine("Sending data");
            socket.BeginSend(dataArray, 0, dataArray.Length, SocketFlags.None, SendCallback, socket);
        }

        private void SendCallback(IAsyncResult Result)
        {
            Socket socket = (Socket)Result.AsyncState;
            socket.EndSend(Result);
        }
    }
}