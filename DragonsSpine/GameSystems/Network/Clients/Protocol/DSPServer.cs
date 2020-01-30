#region 
/*
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;

namespace DSPNet
{
    public delegate void ServerDataHandler(Client client, Packet packet);
    /// <summary>
    /// 
    /// </summary>
    public class Server
    {
        private List<Client> clients = new List<Client>();
        private TcpListener listener;
        private Thread server_thread;
        private int port = 4000;
        private int num_clients = 20;
        public bool Running = false;
        public event ServerDataHandler OnReceive;
        public void StartListening(int num_clients, int port)
        {
            this.port = port;
            this.num_clients = num_clients;

            server_thread = new Thread(new ThreadStart(Run));
            server_thread.Start();

        }
        public Client GetClient(string name)
        {
            foreach (Client client in clients)
            {
                if (client.Name == name)
                {
                    return client;
                }
            }
            return null;
        }
        public void SendToAll(Packet packet)
        {
            foreach (Client client in clients)
            {
                client.Send(packet);
            }
        }
        public void SendToName(string name, Packet packet)
        {
            foreach (Client client in clients)
            {
                if (client.Name == name)
                {
                    client.Send(packet);
                }
            }
        }

        void Run()
        {
            Running = true;
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start(num_clients);
            while (Running)
            {
                if (listener.Pending())
                {
                    Client client_socket = new Client(listener.AcceptTcpClient());
                    client_socket.OnReceive += new ClientDataHandler(client_socket_OnReceive);
                    clients.Add(client_socket);
                }
                List<Client> clientList = new List<Client>();
                foreach (Client cli in clients)
                {
                    clientList.Add(cli);
                }
                foreach (Client client in clientList)
                {
                    if (client.client_socket == null)
                    {
                        clients.Remove(client);
                    }
                }
                Thread.Sleep(100);
            }
            TShutdown();
        }

        void client_socket_OnReceive(Client client, Packet packet)
        {
            if (packet.messageType == 0)
            {
                client.Name = (string)packet.message;
            }
            else
            {
                lock (this)
                {
                    OnReceive(client, packet);
                }
            }
        }
        void TShutdown()
        {
            foreach (Client client in clients)
            {
                client.Shutdown();
            }
            listener.Stop();
            listener = null;
            Thread.CurrentThread.Abort();
        }
        public void Shutdown()
        {
            Running = false;
        }

    }
}
