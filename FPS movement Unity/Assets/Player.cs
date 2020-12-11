using DarkRift.Dispatching;
using System;
using System.Net;
using System.Net.Sockets;
using DarkRift.Client;
using DarkRift.Client.Unity;
using UnityEngine;
using DarkRift;

public class Player : IDarkRiftSerializable
{
    public ushort ID { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public Player()
    {

    }

    public void Deserialize(DeserializeEvent e)
    {
        this.ID = e.Reader.ReadUInt16();
        this.X = e.Reader.ReadSingle();
        this.Y = e.Reader.ReadSingle();
        this.Z = e.Reader.ReadSingle();
    }

    public void Serialize(SerializeEvent e)
    {
        e.Writer.Write(this.ID);
        e.Writer.Write(this.X);
        e.Writer.Write(this.Y);
        e.Writer.Write(this.Z);
    }

    public void InheritValues(Player player)
    {
        this.ID = player.ID;
        this.X = player.X;
        this.Y = player.Y;
        this.Z = player.Z;
    }
}