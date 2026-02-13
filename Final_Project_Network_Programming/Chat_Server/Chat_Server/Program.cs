using Db_Controller.Entities;
using DbController;
using System.Net.Sockets;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Components;

internal class Program
{
    public static List<TcpClient> tcpClients = new List<TcpClient>();
    public static Dictionary<TcpClient, User> clientToUser = new Dictionary<TcpClient, User>();
    public static Db_functional context = new Db_functional();

    private static async Task Start(TcpClient client)
    {
        Console.WriteLine("Connected!");

        using NetworkStream ns = client.GetStream();
        using StreamWriter sw = new StreamWriter(ns) { AutoFlush = true };
        using StreamReader sr = new StreamReader(ns);

        if (tcpClients.Contains(client))
        {
            tcpClients.Remove(client);
        }
        tcpClients.Add(client);


        try
        {
            while (true)
            {
                string? fullMessage = await sr.ReadLineAsync();
                if (fullMessage == null)
                    break;

                if (fullMessage.Contains(":Hello server:"))
                {
                    var parts1 = fullMessage.Split(':', 2);
                    if (parts1.Length == 2)
                    {
                        string userName1 = parts1[0];
                        User user1 = context.GetUser(userName1);
                        if (user1 != null)
                        {
                            clientToUser[client] = user1;
                            Console.WriteLine($"{user1.Username} приєднався до чату.");

                            await sw.WriteLineAsync($"Сервер: Вітаємо, {user1.Username}! Ви приєдналися до чату.");
                        }
                    }
                    continue;
                }
                if (fullMessage.Contains(":FILE:"))
                {
                    await ReceiveAndBroadcastFile(client, fullMessage);
                    continue;
                }
                if(fullMessage.Contains(":Invite:"))
                {
                    var parts2 = fullMessage.Split(':',5);
                    if (parts2.Length < 5) 
                        continue;

                    string senderName = parts2[0];
                    string command = parts2[1];
                    string UserName = parts2[2];
                    string ChatName = parts2[3];
                    int ChatId = int.Parse(parts2[4]);

                    await InviteToGroup(senderName, command, UserName, ChatName, ChatId);
                    continue;
                }

                var parts = fullMessage.Split(':', 2);
                if (parts.Length != 2)
                    continue;

                string userName = parts[0];
                string message = parts[1];

                context.SaveChanges();

                User user = context.GetUser(userName);
                if (user == null)
                {
                    await sw.WriteLineAsync("Сервер: Користувач не знайдений в базі.");
                    continue;
                }
                
                clientToUser[client] = user;

                Console.WriteLine($"Група користувача {userName}: {user.Group.Name}");
                Console.WriteLine($"{DateTime.Now:T} {userName} :: {message} from -- {client.Client.RemoteEndPoint}");

                if (message == "$<close>")
                {
                    tcpClients.Remove(client);
                    clientToUser.Remove(client);
                    client.Close();
                    break;
                }

                foreach (var tcpClient in tcpClients.ToList())
                {
                    try
                    {
                        if (!tcpClient.Connected)
                        {
                            tcpClients.Remove(tcpClient);
                            continue;
                        }

                        if (clientToUser.TryGetValue(tcpClient, out User joinedUser) &&
                            joinedUser.GroupId == user.GroupId)
                        {
                            NetworkStream clientStream = tcpClient.GetStream();
                            using StreamWriter sw_ = new StreamWriter(clientStream, leaveOpen: true) { AutoFlush = true };
                            await sw_.WriteLineAsync($"{userName}:{message}");
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Помилка при надсиланні до клієнта: {e.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка з клієнтом: {ex.Message}");
        }
        finally
        {
            tcpClients.Remove(client);
            clientToUser.Remove(client);
            client.Close();
        }
    }
    public static async Task ReceiveAndBroadcastFile(TcpClient senderClient, string header)
    {
        try
        {
            var parts = header.Split(':');
            if (parts.Length < 4) return;

            string senderName = parts[0];
            string fileName = parts[2];
            int length = int.Parse(parts[3]);

            var user = context.GetUser(senderName);
            if (user == null) return;

            NetworkStream senderStream = senderClient.GetStream();
            byte[] buffer = new byte[length];
            int bytesRead = 0;

            while (bytesRead < length)
            {
                int read = await senderStream.ReadAsync(buffer, bytesRead, length - bytesRead);
                if (read == 0) break;
                bytesRead += read;
            }

            foreach (var tcpClient in tcpClients.ToList())
            {
                try
                {
                    if (!tcpClient.Connected)
                    {
                        tcpClients.Remove(tcpClient);
                        continue;
                    }

                    if (clientToUser.TryGetValue(tcpClient, out User joinedUser) &&
                        joinedUser.GroupId == user.GroupId)
                    {
                        NetworkStream clientStream = tcpClient.GetStream();
                        using StreamWriter writer = new StreamWriter(clientStream, leaveOpen: true) { AutoFlush = true };
                        await writer.WriteLineAsync($"{senderName}:FILE:{fileName}:{length}");
                        await clientStream.WriteAsync(buffer, 0, length);
                        await clientStream.FlushAsync();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Send file error: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ReceiveAndBroadcastFile error: {ex.Message}");
        }
    }
    public static async Task InviteToGroup(string senderName, string command, string UserName, string GroupName, int GroupId)
    {
        var inviterClient = tcpClients.FirstOrDefault(c => clientToUser[c].Username == senderName);
        if (inviterClient == null)
        {
            Console.WriteLine($"Не вдалося знайти клієнта для користувача {senderName}");
            return;
        }


        var user_ = clientToUser[inviterClient];
        if (user_ == null)
        {
            Console.WriteLine("Помилка: Користувач не знайдений.");
            return;
        }

        var invitedUser = context.GetUser(UserName);
        if (invitedUser == null)
        {
            Console.WriteLine($"Користувача {UserName} не знайдено.");
            return;
        }

        string Message = $"{senderName}:Invite:{invitedUser.Username}:{GroupName}:{GroupId}";
        
        Console.WriteLine(Message);

        foreach (var tcpClient in tcpClients)
        {
            if (clientToUser.TryGetValue(tcpClient, out var clientUser) && clientUser.Username == invitedUser.Username)
            {
                var invitedStream = tcpClient.GetStream();
                using var invitedWriter = new StreamWriter(invitedStream, leaveOpen: true) { AutoFlush = true };
                await invitedWriter.WriteLineAsync(Message);

                Console.WriteLine($"Запрошення надіслано користувачу {invitedUser.Username} в групу {GroupName} (ID: {GroupId})");
                break;
            }
        }

        var inviterStream = inviterClient.GetStream();
        using var inviterWriter = new StreamWriter(inviterStream, leaveOpen: true) { AutoFlush = true };
        await inviterWriter.WriteLineAsync($"Ви {senderName} запросили корнистувача {invitedUser.Username} в групу {GroupName} (ID: {GroupId})");
    }

    private static void Main(string[] args)
    {
        IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 4040);
        TcpListener server = new TcpListener(ipPoint);

        try
        {
            context = new Db_functional();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Помилка ініціалізації бази даних: {ex.Message}");
            return;
        }

        try
        {
            server.Start();
            Console.WriteLine("Server started! Waiting for connections...");

            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("Новий клієнт підключився");

                if (tcpClients.Count < 5)
                {
                    Task.Run(() => Start(client));
                }
                else
                {
                    NetworkStream ns = client.GetStream();
                    StreamWriter sw = new StreamWriter(ns) { AutoFlush = true };
                    sw.WriteLine("Сервер: Сервер переповнений!");
                    client.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server error: {ex.Message}");
        }
    }

}