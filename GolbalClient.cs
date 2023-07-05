using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Gui_client
{
    public static class GolbalClient
    {
        public static NetworkStream ClientStream { get; set; }
        public static string ClientName { get; set; }
        public static Dictionary<int, string> Rooms { get; set; }
        public static Dictionary<string, string> AdminRoom { get; set; }
        public static string adminRoomId { get; set; }
    }
}
