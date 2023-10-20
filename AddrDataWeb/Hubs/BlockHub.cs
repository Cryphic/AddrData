using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
public class BlockHub : Hub
{
    public async Task<bool> SendTCPBlock(string sender, string ip) {
        try {
            Console.WriteLine("Blocking : " + ip);
            Console.WriteLine(sender);
            var localip = "";
            IPAddress[] ipAddresses = Dns.GetHostAddresses(sender);
            foreach (IPAddress ipAddress in ipAddresses)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    localip = ipAddress.ToString();
                }
            }
            Console.WriteLine(localip);

            //send tcp message to block ip
            using (var client = new TcpClient())
            {
                await client.ConnectAsync(localip, 8787);
                var stream = client.GetStream();
                var data = System.Text.Encoding.ASCII.GetBytes(ip);
                await stream.WriteAsync(data, 0, data.Length);
                stream.Close();
            }
            Console.WriteLine("IP blocked successfully.");
            return true;
        }
        catch (Exception ex) {
            Console.WriteLine("Error blocking IP: " + ex.Message);
            return false;
        }
    }
    public async Task ExecuteBlockIP(string host, string ip)
    {
        Console.WriteLine("Blocking : " + ip);
        bool block = await SendTCPBlock(host, ip);
        if (block)
            await Clients.Caller.SendAsync("BlockIPResponse", "Sent Block Request on IP: " + ip + " to host: " + host);
        else
            await Clients.Caller.SendAsync("BlockIPResponse", "Failed to send Block Request on IP: " + ip + " to host: " + host);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", "You are now connected to the BlockHub.");
        await base.OnConnectedAsync();
    }
}