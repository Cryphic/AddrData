using System.Net;
using System.Net.Sockets;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Data;
using System;
using System.Runtime;
using System.IO;
using System.Net.NetworkInformation;

namespace AddrData
{
   
    class DatabaseSettings
    {
        public string connectionString { get; set; } = null!;
        public string database { get; set; } = null!;
        public string collection { get; set; } = null!;

 

        public void ReadSettings(string filePath)
        {
            try
            {
                Dictionary<string, string> settings = new Dictionary<string, string>();

                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (line.TrimStart().StartsWith("[Settings]"))
                        {
                            continue;
                        }

                        int equalIndex = line.IndexOf('=');
                        if (equalIndex != -1)
                        {
                            string key = line.Substring(0, equalIndex).Trim();
                            string value = line.Substring(equalIndex + 1).Trim(' ', '"');

                            settings[key] = value;
                        }
                    }
                }

                connectionString = settings["ConnectionString"];
                database = settings["Database"];
                collection = settings["Collection"];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading settings from {filePath}: {ex.Message}");
                throw;
            }
        }
    }

    public class PacketData
    {
        public ObjectId Id { get; set; } 
        public string IP { get; set; } = null!;
        public string Sender { get; set; } = null!;
        public int Count { get; set; } 
    }


    internal class Program
    {
       
        //implementoitu vanhasta projustani
        static async Task serverlistener(CancellationToken cancellationToken)
        {
            TcpListener server = null;
            try
            {
                // Avaa uusi TCPKuuntelija portille 8787 ja hyväksy kaikki IP-Osoitteet
                server = new TcpListener(IPAddress.Any, 8787);
                // Aloita kuuntelu
                server.Start();

                // Aloita loop
                while (true)
                {
                    Thread.Sleep(1000);
                    Console.Write("Listening...");
                    // Hyväksy kaikki yhteydet
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Connected");

                    // Stream objekti lukemiselle
                    NetworkStream stream = client.GetStream();
                    while (true)
                    {
                        if (!stream.DataAvailable)
                        {
                            if (server.Pending())
                            {
                                break;
                            }
                        }

                        if (stream.DataAvailable)
                        {
                            //uusi puskuri ja puskurinkoko pyydetään clientiltä
                            byte[] buffer = new byte[client.ReceiveBufferSize];
                            int bytesRead = await stream.ReadAsync(buffer, 0, client.ReceiveBufferSize);
                            //Dekoodataan Bytes jotta saadaan merkkijono.
                            string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            //tulostetaan merkkijono
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Received: {0}", dataReceived);
                            Console.ResetColor();
                            //add block rule in windows firewall
                            string ipAddressToBlock = dataReceived;

                            Console.WriteLine("Starting block process");
                            Process process = new Process();
                            process.StartInfo.FileName = "netsh";
                            
                            
                            process.StartInfo.Arguments = $"advfirewall firewall add rule name=\"ADDRCLIENT\" dir=in action=block remoteip={ipAddressToBlock} enable=yes"; ;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.Start();
                            string output = process.StandardOutput.ReadToEnd();
                            process.WaitForExit();
                            Console.WriteLine("IN block: " + output);

                            process.StartInfo.Arguments = $"advfirewall firewall add rule name=\"ADDRCLIENT\" dir=out action=block remoteip={ipAddressToBlock} enable=yes"; ;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.Start();
                            output = process.StandardOutput.ReadToEnd();
                            process.WaitForExit();
                            Console.WriteLine("OUT block: " + output);


                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("IP: {0} blocked", ipAddressToBlock);
                            Console.ResetColor();



                        }

                        if (server.Pending())
                        {
                            break;
                        }
                    }

                    Console.WriteLine("Client close");
                    // Sulje yhteys
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                Console.WriteLine("Closing socket listen.");
                server.Stop();
            }
        }


 

        static void Main(string[] args)
        {

           //Init settings class
            var MongoSettings = new DatabaseSettings();
            MongoSettings.ReadSettings("Settings.ini");
            //print variables
            Console.WriteLine("MongoDB Settings read as:");
            Console.WriteLine(MongoSettings.connectionString);
            Console.WriteLine(MongoSettings.database);
            Console.WriteLine(MongoSettings.collection);


            var client = new MongoClient(MongoSettings.connectionString);        
            while (true) { 
            try {                     
            bool isMongoLive = client.GetDatabase(MongoSettings.database).RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait(1000);
                    if (isMongoLive)
                    {
                        Console.WriteLine("MongoDB Connection established.");
                        break;
                    }
                } catch (Exception ex)
                {
                    Console.WriteLine("Error connecting to MongoDB:" + ex.Message);
                    Console.WriteLine("Retrying in 5 seconds...");
                    Thread.Sleep(5000);
                }
            }

            var database = client.GetDatabase(MongoSettings.database);
            var collection = database.GetCollection<PacketData>(MongoSettings.collection);

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) => {
                eventArgs.Cancel = true;
                cts.Cancel();
            };


            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
            socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.1.102"), 0));
            socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
            socket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), BitConverter.GetBytes(1));

            var listenerTask = serverlistener(cts.Token);

            while (!cts.Token.IsCancellationRequested)
            {
                




                // Packet receive
                byte[] buffer = new byte[4096];
                try
                {
                    int bytesReceived = socket.Receive(buffer);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex.Message);
                    continue;
                }

                // IP header Source/Dest
                byte[] sourceBytes = new byte[4];
                Array.Copy(buffer, 12, sourceBytes, 0, 4);
                IPAddress sourceAddress = new IPAddress(sourceBytes);
                
            




                byte[] destinationBytes = new byte[4];
                Array.Copy(buffer, 16, destinationBytes, 0, 4);
                IPAddress destinationAddress = new IPAddress(destinationBytes);

                // LAN address check bitwise operatio
                uint address = BitConverter.ToUInt32(sourceAddress.GetAddressBytes(), 0);
                uint subnet192168 = BitConverter.ToUInt32(IPAddress.Parse("255.255.0.0").GetAddressBytes(), 0);
                uint subnet10 = BitConverter.ToUInt32(IPAddress.Parse("255.0.0.0").GetAddressBytes(), 0);
                uint subnet17216 = BitConverter.ToUInt32(IPAddress.Parse("255.240.0.0").GetAddressBytes(), 0);

                if ((address & subnet192168) == BitConverter.ToUInt32(IPAddress.Parse("192.168.0.0").GetAddressBytes(), 0) ||
                    (address & subnet10) == BitConverter.ToUInt32(IPAddress.Parse("10.0.0.0").GetAddressBytes(), 0) ||
                    (address & subnet17216) == BitConverter.ToUInt32(IPAddress.Parse("172.16.0.0").GetAddressBytes(), 0))
                {
                    // skip iteratio        
                    continue;
                }



                var packetData = new PacketData
                {
                    IP = sourceAddress.ToString(),
                    Sender = Environment.MachineName
                };

                var filter = Builders<PacketData>.Filter.Where(x => x.IP == packetData.IP && x.Sender == packetData.Sender);
                var update = Builders<PacketData>.Update.Inc("Count", 1);

                var options = new FindOneAndUpdateOptions<PacketData>
                {
                    IsUpsert = true,
                    ReturnDocument = ReturnDocument.After
                };

                var result = collection.FindOneAndUpdate(filter, update, options);

                // Info printti
                Console.WriteLine("From " + sourceAddress.ToString() + " to " + destinationAddress.ToString());
                Array.Clear(buffer, 0, buffer.Length);
            }

            //testing purposes clear rules
            Console.WriteLine("clear firewall rules");           
            Process process = new Process();
            process.StartInfo.FileName = "netsh";
            process.StartInfo.Arguments = "advfirewall reset";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            Console.WriteLine(output);
            Console.WriteLine("Closing");

            
        }
    }
}

         