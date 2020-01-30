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
using DragonsSpine;

namespace DSPNet
{
    public delegate void ClientDataHandler(Client client, Packet packet);
    public class Client
    {
        public string Name = "noname";
        public TcpClient client_socket;


        public string AccountName;
        public Account Account;
        public PC ActiveCharacter;
        public bool IsAuthenticated;
        public enum ClientState { None, MainMenu, Conference, CharacterSelect, Login, InGame, Chargen };
        public enum ClientPrivs { Player, AGM, GM, DEV, GHOD };


        private bool running = false;
        public event ClientDataHandler OnReceive;
        public Client()
        {
            client_socket = new TcpClient();
        }
        public Client(TcpClient client_socket)
        {
            this.client_socket = client_socket;
            
            new Thread(new ThreadStart(Run)).Start();
        }
        public void Connect(string host, int port)
        {
            client_socket.Connect(IPAddress.Parse(host), port);
            new Thread(new ThreadStart(Run)).Start();
        }
        void Run()
        {
            running = true;
            while (running)
            {
                try
                {
                    if (client_socket.GetStream().DataAvailable)
                    {
                        Receive();
                    }
                }
                catch
                {
                    running = false;
                }
                Thread.Sleep(100);
            }
            TShutdown();
        }
        public void Send(Packet packet)
        {
            try
            {
                if (client_socket != null)
                {
                    if (client_socket.GetStream().CanWrite)
                    {
                        MemoryStream stream = new MemoryStream();
                        BinaryFormatter formatter = new BinaryFormatter();
                        formatter.Serialize(stream, packet.message);
                        byte[] messagetype = BitConverter.GetBytes(packet.messageType);
                        byte[] message = stream.GetBuffer();
                        byte[] message_length = BitConverter.GetBytes(message.Length);
                        client_socket.GetStream().Write(messagetype, 0, 4);
                        client_socket.GetStream().Write(message_length, 0, 4);
                        client_socket.GetStream().Write(message, 0, message.Length);
                    }
                }
            }
            catch
            {
                Shutdown();
            }


        }
        public void UpdateName(string name)
        {
            this.Name = name;
            Packet packet = new Packet();
            packet.message = name;
            packet.messageType = 0;
            Send(packet);
        }
        public void Receive()
        {
            try
            {
                Packet packet = new Packet();
                byte[] messagetype = new byte[4];
                byte[] messagelength = new byte[4];
                client_socket.GetStream().Read(messagetype, 0, 4);
                client_socket.GetStream().Read(messagelength, 0, 4);
                int message_size = BitConverter.ToInt32(messagelength, 0);
                packet.messageType = BitConverter.ToInt32(messagetype, 0);
                byte[] message = new byte[message_size];
                client_socket.GetStream().Read(message, 0, message_size);
                MemoryStream stream = new MemoryStream(message);
                BinaryFormatter formatter = new BinaryFormatter();
                packet.message = formatter.Deserialize(stream);
                if (packet.messageType == 0)
                {
                    Name = (string)packet.message;
                }
                else
                {
                    OnReceive(this, packet);
                }
            }
            catch (SocketException)
            {
                TShutdown();
            }
            catch
            {
            }
        }
        void TShutdown()
        {
            if (client_socket != null)
            {
                client_socket.Close();
            }
            client_socket = null;
            Thread.CurrentThread.Abort();
        }
        public void Shutdown()
        {
            running = false;
        }
    }
}
