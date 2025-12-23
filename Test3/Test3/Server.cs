// <copyright file="Server.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Test3;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

/// <summary>
/// Класс, реализующий серверную часть чата.
/// </summary>
public static class Server
{
    /// <summary>
    /// Запускает сервер на указанном порту.
    /// </summary>
    /// <param name="port">Порт для прослушивания подключений.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task StartAsync(int port)
    {
        try
        {
            Console.WriteLine($"Запуск сервера на порту {port}...");

            var listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            Console.WriteLine("Сервер запущен. Ожидание подключения клиента...");
            Console.WriteLine($"IP-адрес сервера: {GetLocalIpAddress()}");
            Console.WriteLine($"Порт: {port}");

            using (var client = await listener.AcceptTcpClientAsync())
            {
                Console.WriteLine("Клиент подключен!");

                await ConnectionHandler.HandleConnectionAsync(client, "Сервер", "Клиент");
            }

            listener.Stop();
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Сетевая ошибка: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка сервера: {ex.Message}");
        }
    }

    /// <summary>
    /// Получает локальный IP-адрес компьютера.
    /// </summary>
    /// <returns>Строка с локальным IP-адресом.</returns>
    private static string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        return "127.0.0.1";
    }
}