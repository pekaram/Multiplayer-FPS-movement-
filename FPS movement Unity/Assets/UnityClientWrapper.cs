using DarkRift.Dispatching;
using System;
using System.Net;
using System.Net.Sockets;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;
using DarkRift;

public class UnityClientWrapper : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private UnityClient unityClient;

    private const float PlayerUpdateRate = 0.1f;

    /// <summary>
    /// 	The actual client connecting to the server.
    /// </summary>
    /// <value>The client.</value>
    public DarkRiftClient Client { get { return this.unityClient.Client; } }

    private ClientMessageHandler messageHandler;

    private void Awake()
    {
        this.messageHandler = new ClientMessageHandler(this.unityClient, this.playerPrefab);
        this.messageHandler.OnLocalPlayerCreated += OnLocalPlayerCreated;
    }

    private void OnLocalPlayerCreated()
    {
        this.StartSendingLocalPlayerUpdates();
    }

    public void StartSendingLocalPlayerUpdates()
    {
        this.InvokeRepeating("SendUpdate", PlayerUpdateRate, PlayerUpdateRate);
    }

    private void SendUpdate()
    {
        using (DarkRiftWriter writer = DarkRiftWriter.Create())
        {
            writer.Write(this.messageHandler.LocalPlayer);

            using (Message message = Message.Create(MessageTag.UpdatePlayerData, writer))
            {
                this.unityClient.SendMessage(message, SendMode.Reliable);
            }
        }
    }

}
