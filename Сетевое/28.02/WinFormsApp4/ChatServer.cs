using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp4
{
    /*
    Создайтесетевоеприложение,котороепозволяетпользователямобщаться 
    между собой с помощью сообщений. 
    При первом использовании приложения пользователь регистрируется, 
    при следующих запусках пользователь входит с помощью созданного логина 
    и пароля. Пользователь может послать приглашение для общения существу
    ющему пользователю, создать свой список контактов, добавлять, удалять, 
    переименовывать контакты.   Пользователи могут обмениваться текстовыми 
    сообщениями, картинками, файлами. Пользователь имеет возможность ор
    ганизовать групповой чат.
    */
    class ChatServer
    {
        private static Dictionary<TcpClient, string> _clients = new Dictionary<TcpClient, string>();

        static async Task Main()
        {
            TcpListener server = new TcpListener(IPAddress.Any, 5000);
            server.Start();
            Console.WriteLine("Server started at port 5000...");

            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                Console.WriteLine("Новое подключение");
                Task.Run(() => HandleClint(client));
            }
        }
        
        private static async Task HandleClint(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            string username = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            _clients.Add(client, username);
            Console.WriteLine($"[{username}] присоединился к чату.");
            
            while (true)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(message, username);
                if (message.StartsWith("/"));
                {
                    string command = message.Substring(1);
                    if (command.StartsWith("sendto"))
                    {
                        string[] args = command.Split(' ');
                        if (args.Length > 1)
                        {
                            string recipientUsername = args[1];
                            string messageToSend = string.Join(" ", args.Skip(2));
                            SendMessage(recipientUsername, $"{username}: {messageToSend}");
                        }
                    }
                    else if (command.StartsWith("sendall"))
                    {
                        string messageToSend = string.Join(" ", args.Skip(1));
                        SendMessageToAll(username, $"{username}: {messageToSend}");
                    }
                    else if (command.StartsWith("groupchat"))
                    {
                        string groupname = args[1];
                        SendGroupMessage(groupname, username, string.Join(" ", args.Skip(2)));
                    }
                    else if (command.StartsWith("listclients"))
                    {
                        SendListClients(username);
                    }
                    else if (command.StartsWith("creategroup"))
                    {
                        string groupname = args[1];
                        CreateGroup(groupname);
                    }
                    else if (command.StartsWith("joingroup"))
                    {
                        string groupname = args[1];
                        JoinGroup(username, groupname);
                    }
                    else if (command.StartsWith("leavegroup"))
                    {
                        string groupname = args[1];
                        LeaveGroup(username, groupname);
                    }
                    else if (command.StartsWith("listgroups"))
                    {
                        SendListGroups(username);
                    }
                    else {
                        Console.WriteLine($"[{username}] неизвестная команда: {command}");
                    }
                }
            }
        
    }
}
