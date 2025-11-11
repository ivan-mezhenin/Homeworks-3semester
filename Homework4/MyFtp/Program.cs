// <copyright file="Program.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using MyFtp;

// === 1. Запускаем сервер ===
var server = new Server(8888);
_ = Task.Run(() => server.StartAsync());

await Task.Delay(1000); // ждём запуска

// === 2. Создаём клиента ===
using var client = new Client("127.0.0.1", 8888);

// === 3. Отправляем List ===
var result = await client.ListRequestAsync("./");

Console.WriteLine(result.Error!, result.Size);

foreach (var elem in result.Data)
{
    Console.WriteLine(elem);
}

server.Stop();
Console.ReadKey();