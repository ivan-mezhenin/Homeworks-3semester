// <copyright file="Program.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using System.Text;
using MyFtp;

var server = new Server(8888);
_ = Task.Run(() => server.StartAsync());

await Task.Delay(1000);

using var client = new Client("127.0.0.1", 8888);

var getResult = await client.GetRequestAsync("./TestFiles/Hello.txt");

Console.WriteLine((getResult.Error ?? getResult.Error) ?? string.Empty, getResult.Size);

var text = Encoding.UTF8.GetString(getResult.Content);
Console.WriteLine(text);

server.Stop();
Console.ReadKey();