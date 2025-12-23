// <copyright file="Program.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using System.Net;
using Test3;

await (args switch
{
    [var portArg] => HandleServerModeAsync(portArg),
    [var ipArg, var portArg] => HandleClientModeAsync(ipArg, portArg),
    _ => ShowHelpAsync(),
});

return;

static async Task HandleServerModeAsync(string portArgument)
{
    if (int.TryParse(portArgument, out var port) && port is > 0 and <= 65535)
    {
        await Server.StartAsync(port);
        return;
    }

    Console.WriteLine("Ошибка: порт должен быть числом в диапазоне 1-65535");
}

static async Task HandleClientModeAsync(string ipArgument, string portArgument)
{
    if (IPAddress.TryParse(ipArgument, out var ipAddress) &&
        int.TryParse(portArgument, out var port) && port is > 0 and <= 65535)
    {
        await Client.StartAsync(ipAddress, port);
        return;
    }

    Console.WriteLine("Ошибка: неверный формат IP-адреса или порта");
}

static async Task ShowHelpAsync()
{
    Console.WriteLine("Использование:");
    Console.WriteLine("  Запуск сервера: dotnet run -- <порт>");
    Console.WriteLine("  Запуск клиента: dotnet run --  <IP-адрес> <порт>");
    await Task.CompletedTask;
}