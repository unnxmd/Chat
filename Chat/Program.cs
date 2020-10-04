using System;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Chat
{
    class Program
    {
        private static readonly object Lock = new object();
        static void Main(string[] args)
        {
            string ip;
            Console.Write("Введите IP адрес: ");
            IPAddress ipAddress;
            ip = Console.ReadLine();
            if (!IPAddress.TryParse(ip, out ipAddress)) {
                do
                {
                    Console.Write("Некорректный IP! Введите IP еще раз: ");
                    ip = Console.ReadLine();
                } while (IPAddress.TryParse(ip, out ipAddress));
            }
            int port = 15000;
            TcpClient client;
            NetworkStream stream;
            byte[] byte_msg = new byte[512];
            string string_msg = "";

            try
            {
                client = new TcpClient(ip, port);
                stream = client.GetStream();
            }
            catch
            {
                Console.WriteLine("Второй клиент не запущен. Ждем...");
                try
                {
                    TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                    listener.Start();
                    client = listener.AcceptTcpClient();
                    stream = client.GetStream();
                    listener.Stop();
                }
                catch (Exception e1)
                {
                    Console.WriteLine(e1.Message);
                    throw;
                }
            }
            Console.WriteLine("Подключение установлено!");
            Thread th = new Thread(o => GetMessage((TcpClient)o, stream));
            th.Start(client);
            try
            {
                while (true)
                {
                    byte_msg = new byte[512];
                    string_msg = Console.ReadLine();
                    string_msg = " <<< " + string_msg;
                    byte_msg = Encoding.UTF8.GetBytes(string_msg);
                    stream.Write(byte_msg, 0, byte_msg.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.ReadLine();
        }

        public static void GetMessage(TcpClient client, NetworkStream stream)
        {
            byte[] bytes = new byte[512];
            string message;
            try
            {
                while (true)
                {
                    stream = client.GetStream();
                    if (stream.Read(bytes, 0, bytes.Length) != 0)
                    {
                        message = Encoding.UTF8.GetString(bytes);
                        message = message.Replace("\0", "");
                        lock (Lock) Console.WriteLine(message);
                        bytes = new byte[512];
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Thread.CurrentThread.Abort();
            }
        }
    }
}
