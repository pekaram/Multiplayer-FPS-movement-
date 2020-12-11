using DarkRift.Dispatching;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using DarkRift.Client;
using DarkRift.Client.Unity;
using DarkRift;
using System.Collections.Generic;

public class ClientMessageHandler 
{
    public UnityClient unityClient;

    public Dictionary<ushort, PlayerPositionUpdater> idsToTransforms = new Dictionary<ushort, PlayerPositionUpdater>();

    public Dictionary<ushort, Player> idsToPlayers = new Dictionary<ushort, Player>();

    public Player LocalPlayer { get; private set; }

    private GameObject playerPrefab;

    public event Action OnLocalPlayerCreated;

    public ClientMessageHandler(UnityClient unityClient, GameObject characterPrefab)
    {
        this.unityClient = unityClient;
        this.unityClient.MessageReceived += this.OnMessageReceived;
        this.playerPrefab = characterPrefab;
    }

    public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        using (DarkRiftReader reader = message.GetReader())
        {
            List<Player> updatedPlayers = new List<Player>();
            while (reader.Position < reader.Length)
            {
                updatedPlayers.Add(reader.ReadSerializable<Player>());
            }

            switch (e.Tag)
            {
                case MessageTag.UpdatePlayerData:
                    foreach (var player in updatedPlayers)
                    {
                        if (player.ID == this.unityClient.ID)
                        {
                            continue;
                        }

                        this.OnPlayerUpdated(player);
                    }
                    break;

                case MessageTag.SpawnPlayer:
                    foreach (var player in updatedPlayers)
                    {
                        this.OnPlayerAdded(player);
                    }
                    break;

                case MessageTag.DestroyPlayer:
                    foreach(var player in updatedPlayers)
                    {
                        if(!idsToTransforms.ContainsKey(player.ID))
                        {
                            //Client already doesn't have this.
                        }
                        UnityEngine.Object.Destroy(idsToTransforms[player.ID].gameObject);
                    }
                    break;
            }
        }
    }


    public void OnPlayerUpdated(Player updatedInfo)
    {
        this.idsToPlayers[updatedInfo.ID].InheritValues(updatedInfo);
    }
    
    public void OnPlayerAdded(Player player)
    {
        var playerCharacter = UnityEngine.Object.Instantiate(this.playerPrefab, new Vector3(player.X, player.Y, player.Z), Quaternion.identity);
        var positionUpdater = playerCharacter.AddComponent<PlayerPositionUpdater>();
        positionUpdater.player = player;

        this.idsToTransforms.Add(player.ID, positionUpdater);
        this.idsToPlayers.Add(player.ID, player);

        if(player.ID == this.unityClient.ID)
        {
            var playerInput = playerCharacter.AddComponent<PlayerInput>();
            playerInput.player = player;

            this.LocalPlayer = player;
            this.OnLocalPlayerCreated?.Invoke();
        }
    }
}
