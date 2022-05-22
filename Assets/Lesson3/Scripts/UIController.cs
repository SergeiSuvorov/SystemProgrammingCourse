
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Button buttonStartServer;
    [SerializeField]
    private Button buttonShutDownServer;
    [SerializeField]
    private Button buttonConnectClient;
    [SerializeField]
    private Button buttonDisconnectClient;
    [SerializeField]
    private Button buttonSendMessage;
    [SerializeField]
    private Button buttonChangeName;

    [SerializeField]
    private TMP_InputField inputField;


    [SerializeField]
    private TextField textField;

    [SerializeField]
    private Server server;
    [SerializeField]
    private Client client;

    private void Start()
    {
        buttonStartServer.onClick.AddListener(() => StartServer());
        buttonShutDownServer.onClick.AddListener(() => ShutDownServer());
        buttonConnectClient.onClick.AddListener(() => Connect());
        buttonDisconnectClient.onClick.AddListener(() => Disconnect());
        buttonSendMessage.onClick.AddListener(() => SendMessage(0));
        buttonChangeName.onClick.AddListener(() => SendMessage(1));
        //inputField.onEndEdit.AddListener((text) =>SendMessage());
        client.onMessageReceive += ReceiveMessage;
    }

    private void StartServer() =>    
        server.StartServer();
    
    private void ShutDownServer() =>    
        server.ShutDownServer();
    
    private void Connect() =>    
        client.Connect();    

    private void Disconnect() =>    
        client.Disconnect();    

    private void SendMessage(short messageType)
    {
        client.SendMessage(messageType,inputField.text);
        inputField.text = "";
    }

    public void ReceiveMessage(object message) =>    
        textField.ReceiveMessage(message);
    
}
