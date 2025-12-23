// <copyright file="ConnectionHandler.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

namespace Test3;

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Класс для обработки соединения между клиентом и сервером.
/// </summary>
public static class ConnectionHandler
{
    /// <summary>
    /// Обрабатывает соединение между клиентом и сервером.
    /// </summary>
    /// <param name="client">TCP-клиент.</param>
    /// <param name="localName">Локальное имя (Сервер или Клиент).</param>
    /// <param name="remoteName">Имя удаленной стороны.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public static async Task HandleConnectionAsync(TcpClient client, string localName, string remoteName)
    {
        try
        {
            var stream = client.GetStream();

            var receiveTask = ReceiveMessagesAsync(stream, remoteName);
            var sendTask = SendMessagesAsync(stream);

            await Task.WhenAny(receiveTask, sendTask);

            client.Close();
            Console.WriteLine("Соединение закрыто.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
        }
    }

    /// <summary>
    /// Асинхронно получает сообщения из сетевого потока.
    /// </summary>
    /// <param name="stream">Сетевой поток.</param>
    /// <param name="senderName">Имя отправителя сообщений.</param>
    private static async Task ReceiveMessagesAsync(NetworkStream stream, string senderName)
    {
        var buffer = new byte[1024];

        try
        {
            while (true)
            {
                var bytesRead = await stream.ReadAsync(buffer);

                if (bytesRead == 0)
                {
                    Console.WriteLine($"{senderName} отключился.");
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                DisplayMessage(senderName, message);

                if (IsExitCommand(message))
                {
                    Console.WriteLine($"{senderName} завершил чат.");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении сообщения: {ex.Message}");
        }
    }

    /// <summary>
    /// Асинхронно отправляет сообщения в сетевой поток.
    /// </summary>
    /// <param name="stream">Сетевой поток.</param>
    private static async Task SendMessagesAsync(NetworkStream stream)
    {
        try
        {
            while (true)
            {
                var message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                {
                    continue;
                }

                if (IsExitCommand(message))
                {
                    var exitMessage = Encoding.UTF8.GetBytes("exit");
                    await stream.WriteAsync(exitMessage);
                    Console.WriteLine("Завершение чата...");
                    break;
                }

                var buffer = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(buffer);

                DisplayMessage("Вы", message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при отправке сообщения: {ex.Message}");
        }
    }

    /// <summary>
    /// Проверяет, является ли сообщение командой выхода.
    /// </summary>
    /// <param name="message">Сообщение для проверки.</param>
    /// <returns>True, если сообщение является командой выхода.</returns>
    private static bool IsExitCommand(string message)
    {
        return message.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Отображает сообщение в консоли с форматированием.
    /// </summary>
    /// <param name="sender">Отправитель сообщения.</param>
    /// <param name="message">Текст сообщения.</param>
    private static void DisplayMessage(string sender, string message)
    {
        var originalColor = Console.ForegroundColor;

        try
        {
            if (sender == "Вы")
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{sender}: {message}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{sender}: {message}");
            }
        }
        finally
        {
            Console.ForegroundColor = originalColor;
        }
    }
}