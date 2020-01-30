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
using System.Linq;
using System.Text;

namespace DSPNet
{
    /// <summary>
    /// The master enumeration of what type of server is to be used in TVNet
    /// </summary>
    public enum ServerType
    {
        XML,
        BINARY,
        CUSTOM
    }

    /// <summary>
    /// Master singleton class for TVNet
    /// </summary>
    public class Instances
    {
        protected static int nClients = 20;
        protected static Server server_instance;

        /// <summary>
        /// Creates a server instance in TVNet
        /// </summary>
        /// <param name="number_of_clients">The number of clients the server will support</param>
        /// <param name="port">The port number to listen to</param>
        /// <param name="multithreaded">Whether the server is multi-threaded or single-threaded</param>
        /// <param name="serverType">The server master enumeration type</param>
        public static Server CreateServer(int number_of_clients, int port)
        {
            server_instance = new Server();
            server_instance.StartListening(number_of_clients, port);
            return server_instance;
        }

        public static Client CreateClient(string host, int port)
        {
            Client client = new Client();
            client.Connect(host, port);
            return client;
        }
        public static Client GetClient(string name)
        {
            return server_instance.GetClient(name);
        }

        public static void ShutdownServer()
        {
            server_instance.Running = false;
        }
    }
}
