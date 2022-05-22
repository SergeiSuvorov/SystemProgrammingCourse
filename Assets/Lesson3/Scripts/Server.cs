using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
public class Server : MonoBehaviour
{
    private const int MAX_CONNECTION = 10;
    private int port = 5805;
    private int hostID;
    private int reliableChannel;
    private bool isStarted = false;
    private byte error;
    private List<int> connectionIDs = new List<int>();
    private Dictionary<int, string> playerNamesDictionary = new Dictionary<int, string>();
    public void StartServer()
    {
        NetworkTransport.Init();
        ConnectionConfig cc = new ConnectionConfig();
        reliableChannel = cc.AddChannel(QosType.Reliable);
        HostTopology topology = new HostTopology(cc, MAX_CONNECTION);
        hostID = NetworkTransport.AddHost(topology, port);
        isStarted = true;
    }
    public void ShutDownServer()
    {
        if (!isStarted) return;
        NetworkTransport.RemoveHost(hostID);
        NetworkTransport.Shutdown();
        isStarted = false;
    }

    void Update()
    {
        if (!isStarted) return;
        int recHostId;
        int connectionId;
        int channelId;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        NetworkEventType recData = NetworkTransport.Receive(out recHostId, out connectionId, out
        channelId, recBuffer, bufferSize, out dataSize, out error);
        while (recData != NetworkEventType.Nothing)
        {
            switch (recData)
            {
                case NetworkEventType.Nothing:
                    break;
                case NetworkEventType.ConnectEvent:
                    connectionIDs.Add(connectionId);
                    playerNamesDictionary.Add(connectionId, $"Player {connectionId}");
                    SendMessageToAll($"Player {connectionId} has connected.");
                    Debug.Log($"Player {connectionId} has connected.");
                    break;
                case NetworkEventType.DataEvent:
                    short msgType = BitConverter.ToInt16(recBuffer, 0);
                    string message = Encoding.Unicode.GetString(recBuffer, 2, dataSize);
                    CheckMessage(connectionId, msgType, message);
                    break;
                case NetworkEventType.DisconnectEvent:
                    connectionIDs.Remove(connectionId);
                    SendMessageToAll($"Player {connectionId} has disconnected.");
                    Debug.Log($"Player {connectionId} has disconnected.");
                    break;
                case NetworkEventType.BroadcastEvent:
                    break;
            }
            recData = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, recBuffer,
            bufferSize, out dataSize, out error);
        }
    }
    private void CheckMessage(int connectionId, short msgType, string message)
    {
      
        switch (msgType)
        {
            case 0:
                SendMessageToAll($"Player { playerNamesDictionary[connectionId]} : {message}");
                Debug.Log($"{ playerNamesDictionary[connectionId]} : {message}");
                break;

            case 1:
                ChangeName(connectionId, message);
                break;
            default:
                break;
        }
    }

    private void ChangeName(int connectionId,  string newName)
    {
        newName = newName.Substring(0, newName.Length - 1);

        SendMessageToAll($"Player { playerNamesDictionary[connectionId]} new name: {newName}");
        playerNamesDictionary[connectionId] = newName;
        Debug.Log($"Player { playerNamesDictionary[connectionId]} new name: {newName}");
    }

    public void SendMessageToAll(string message)
    {
        for (int i = 0; i < connectionIDs.Count; i++)
        {
            SendMessage(message, connectionIDs[i]);
        }
    }
    public void SendMessage(string message, int connectionID)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(message);
        NetworkTransport.Send(hostID, connectionID, reliableChannel, buffer, message.Length *
        sizeof(char), out error);
        if ((NetworkError)error != NetworkError.Ok) Debug.Log((NetworkError)error);
    }
}