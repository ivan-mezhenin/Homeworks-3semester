// <copyright file="Client.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Test3;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

/// <summary>
/// Класс, реализующий клиентскую часть чата.
/// </summary>
public static class Client
{
    /// <summary>
    /// Запускает клиент и подключается к серверу.
    /// </summary>
    /// <param name="ipAddress">IP-адрес сервера.</param>
    /// <param name="port">Порт сервера.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task StartAsync(IPAddress ipAddress, int port)
    {
        try
        {
            Console.WriteLine($"Подключение к {ipAddress}:{port}...");

            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(ipAddress, port);
            var timeoutTask = Task.Delay(5000);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Console.WriteLine("Таймаут подключения. Сервер недоступен.");
                return;
            }

            if (!client.Connected)
            {
                Console.WriteLine("Не удалось подключиться к серверу.");
                return;
            }

            Console.WriteLine("Подключение установлено!");

            await ConnectionHandler.HandleConnectionAsync(client, "Клиент", "Сервер");
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Сетевая ошибка: {ex.SocketErrorCode} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка клиента: {ex.Message}");
        }
    }
}