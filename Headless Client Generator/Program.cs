using System;
using DarkRift.Client;
using DarkRift;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;


class HeadlessClientGenerator
{
    public static List<DarkRiftClient> Clients = new List<DarkRiftClient>();

    private const int NumberOfClientToConnect = 50;

    private const string ServerIP = "41.46.133.246";

    private const int CooldownInMilliseconds = 10;

    static void Main(string[] args)
    {

        for (var i = 0; i < NumberOfClientToConnect; i++)
        {
            Clients.Add(new DarkRiftClient());
            Clients[i].Connect(IPAddress.Parse(ServerIP), 4296, false);
            Thread.Sleep(CooldownInMilliseconds);
        }

    }
}
