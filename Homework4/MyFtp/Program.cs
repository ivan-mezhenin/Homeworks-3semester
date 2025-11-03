// <copyright file="Program.cs" company="ivan-mezhenin">
// Copyright (c) ivan-mezhenin. All rights reserved.
// </copyright>

using MyFtp;

var server = new Server(8888);

_ = server.StartAsync();