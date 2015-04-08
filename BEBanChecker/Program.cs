using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace BEBanChecker
{
    public class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Main());
        }

        public static string CheckKey(string key)
        {
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sock.ReceiveTimeout = 500;
            sock.SendTimeout = 500;

            //All domains lead to the same server, so it should work for Arma3/DayZ as well
            //Arma 2: arma2oa1.battleye.com
            //Arma 3: arma31.battleye.com
            //DayZ SA: dayz1.battleye.com

            IPEndPoint endPoint = new IPEndPoint(Dns.GetHostAddresses("arma2oa1.battleye.com")[0], 2324);
            byte[] send_buffer = Encoding.ASCII.GetBytes(CreateRequestString(key));
            try
            {
                sock.SendTo(send_buffer, endPoint);
                byte[] receive_buffer = new byte[1024];
                EndPoint endpnt = (EndPoint)endPoint;
                int recv = sock.ReceiveFrom(receive_buffer, ref endpnt);
                string response = Encoding.ASCII.GetString(receive_buffer, 0, recv).Remove(0, 4);
                return String.IsNullOrEmpty(response) ? "Clean" : response;
            }
            catch (Exception) 
            {
                return "Unknown Error";
            }
        }

        private static string CreateRequestString(string key)
        {
            if (key.Length == 17)
            {
                long steamID;
                bool status = long.TryParse(key, out steamID);
                if (status)
                {
                    return "..6.." + CreateBEGuid(steamID);
                }
            }

            //Old way to check if key is banned
            return ".gr." + (GetMD5Hash("BE" + GetMD5Hash(key))); ;
        }

        public static string CreateBEGuid(long SteamId)
        {
            byte[] parts = { 0x42, 0x45, 0, 0, 0, 0, 0, 0, 0, 0 };
            byte counter = 2;

            do
            {
                parts[counter++] = (byte)(SteamId & 0xFF);
            } while ((SteamId >>= 8) > 0);

            return GetMD5Hash(parts);
        }

        private static string GetMD5Hash(string value)
        {
            return GetMD5Hash(Encoding.UTF8.GetBytes(value));
        }

        private static string GetMD5Hash(byte[] value)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(value);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
}
