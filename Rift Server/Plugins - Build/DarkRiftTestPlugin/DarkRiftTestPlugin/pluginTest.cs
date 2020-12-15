using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DarkRift;
using DarkRift.Server.Plugins;
using DarkRift.Server;
using DarkRift.Dispatching;
using System.Threading;
using System.Collections.Concurrent;

namespace DarkRiftTestPlugin
{

    public class PluginTest : Plugin
    {
        public override bool ThreadSafe => false;

        public override Version Version => new Version(1, 0, 0);

        public ConcurrentDictionary<ushort, Player> idsToPlayers = new ConcurrentDictionary<ushort, Player>();

        private byte updateRate = 100;

        private bool isUpdatingWorld = true;
        
        public PluginTest(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            ClientManager.ClientConnected += OnClientConnected;
            ClientManager.ClientDisconnected += OnClientDisconnected;
            
            var worldUpdate = new Thread(this.SendWorldUpdate);
            worldUpdate.Start();
        }

        private void SendWorldUpdate()
        {
            while (this.isUpdatingWorld)
            {
                if (idsToPlayers.Count == 0)
                {
                    continue;
                }

                using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
                {
                    foreach (var player in this.idsToPlayers)
                    {
                        newPlayerWriter.Write(player.Value);
                    }

                    using (Message newPlayerMessage = Message.Create(MessageTag.UpdatePlayerData, newPlayerWriter))
                    {                       
                        foreach (IClient client in ClientManager.GetAllClients())
                        {
                            if(client == null)
                            {
                                continue;
                            }

                            client.SendMessage(newPlayerMessage, SendMode.Unreliable);
                        }
                    }
                }

                Thread.Sleep(updateRate);
            }
        }

        private void OnClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            var removedPlayer = this.idsToPlayers[e.Client.ID];
            this.idsToPlayers.TryRemove(e.Client.ID, out _);
            this.DestroyPlayer(removedPlayer);
            // Remove Player from all
        }

        private void DestroyPlayer(Player removedPlayer)
        {
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(removedPlayer);

                using (Message newPlayerMessage = Message.Create(MessageTag.DestroyPlayer, newPlayerWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients().Where(p => p.ID != removedPlayer.ID))
                    {
                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                    }
                }
            }
        }

        private void OnClientConnected(object sender, ClientConnectedEventArgs e)
        {
            this.SpawnPlayer(e.Client);
            this.SpawnBufferedPlayers(e.Client);
            e.Client.MessageReceived += OnMessageReceived;
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage() as Message)
            {
                if (message.Tag == MessageTag.UpdatePlayerData)
                {
                    using (DarkRiftReader reader = message.GetReader())
                    {
                        var player = reader.ReadSerializable<Player>();

                        this.idsToPlayers[player.ID].InheritValues(player);
                    }
                }
            }
        }

        private void SpawnBufferedPlayers(IClient client)
        {
            using (DarkRiftWriter playerWriter = DarkRiftWriter.Create())
            {
                foreach (Player player in idsToPlayers.Values)
                {
                    // Skip newly connected player
                    if (player.ID == client.ID)
                    {
                        continue;
                    }

                    playerWriter.Write(player.ID);
                    playerWriter.Write(player.X);
                    playerWriter.Write(player.Y);
                    playerWriter.Write(player.Z);
                }

                using (Message playerMessage = Message.Create(MessageTag.SpawnPlayer, playerWriter))
                {
                    client.SendMessage(playerMessage, SendMode.Reliable);
                }
            }
        }

        private void SpawnPlayer(IClient connectedClient)
        {

            var player = new Player() { ID = connectedClient.ID, X = 0, Y = 0, Z = 0 };

            idsToPlayers.TryAdd(player.ID, player);

            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                newPlayerWriter.Write(player);

                using (Message newPlayerMessage = Message.Create(MessageTag.SpawnPlayer, newPlayerWriter))
                {
                    foreach (IClient client in ClientManager.GetAllClients())
                    {
                        // ClientManager.GetAllClients() returns null clients that are still init?
                        if (client == null)
                        {
                            continue;
                        }

                        client.SendMessage(newPlayerMessage, SendMode.Reliable);
                    }

                }
            }

        }
    }
}
