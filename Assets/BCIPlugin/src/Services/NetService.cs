using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using UnityEngine.UI;

public class AppClient
{
    // TCP client connection and data buffer
    readonly internal TcpClient client = new TcpClient();
    readonly internal byte[] buffer = new byte[5000];

    // Connection options
    readonly internal IPAddress address;
    readonly internal int port;

    public delegate void MessageHandler(string msg);
    public MessageHandler Distributer;

    // Convenience getter
    internal NetworkStream Stream
    {
        get
        {
            return client.GetStream();
        }
    }

    public AppClient(string address, int port)
    {
        this.address = IPAddress.Parse(address);
        this.port = port;
    }

    public void ConnectToServer()
    {
        client.NoDelay = true;
        client.BeginConnect(address, port, HandleConnect, null);

        Debug.Log("AppClient Connecting to BCIServer......");
    }

    // Send a message to the server.
    internal void SendMessage(string message)
    {
        byte[] b = System.Text.Encoding.UTF8.GetBytes(message);
        Stream.Write(b, 0, b.Length);
    }

    // Client did finish connecting asynchronously.
    void HandleConnect(IAsyncResult a)
    {
        Stream.BeginRead(buffer, 0, buffer.Length, OnRead, null);
    }

    // Client is reading data from server
    internal void OnRead(IAsyncResult a)
    {
        int length = Stream.EndRead(a);
        if (length == 0)
        {
            Debug.Log("No length!");
            return;
        }

        string msg = System.Text.Encoding.UTF8.GetString(buffer, 0, length);

        // Split messages
        string[] messages = msg.Split(';');

        // Handle each server message
        foreach (string message in messages)
        {
            Distributer(message);
            //触发事件，监听的对象是Service,在Service的内部分发
        }

        Stream.BeginRead(buffer, 0, buffer.Length, OnRead, null);
    }

}

public class NetService
{
    public string IP = "127.0.0.1";
    public int clientPort = 50000;
    public AppClient appClient;


    static NetService instance;

    public static NetService Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new NetService();
            }
            return instance;
        }
    }

    public NetService()
    {
        appClient = new AppClient(IP, clientPort);
        appClient.Distributer += DistributeMessage;
    }

    public void SendMessage(string msg)
    {
        appClient.SendMessage(msg);
    }

    public void ConnectToServer()
    {
        appClient.ConnectToServer();
    }

    public void AddDistributer(AppClient.MessageHandler handler)
    {
        appClient.Distributer += handler;
    }

    public void DistributeMessage(string msg)
    {
        // Debug.Log("cmd:" + msg);
        // Distribute Messages
        string[] messages = msg.Split('_');
        if(messages[0]== "SessionControl")
        {
            ExpService.Instance.SessionControlHandler(msg);
        }

        if (messages[0] == "Value")
        {
            ValueService.Instance.ValueHandler(msg);
        }

    }

}
