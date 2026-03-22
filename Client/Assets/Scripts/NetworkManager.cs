using Rug.Osc;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    [SerializeField] bool localServer;
    [SerializeField] string serverIP;
    [SerializeField] int serverPort;
    [SerializeField] int localPort = 0;

    [HideInInspector] public bool isInLobby = false;

    TcpClient client;

    void OnServerMessage(OscMessage msg)
    {
        string[] addressParts = msg.Address.Trim('/').Split('/');
        switch(addressParts[0])
        {
            case "echo":
                ChatWindow.instance.RecieveMessage(msg, addressParts[1]);
                break;

            case "lobby":
                LobbyWindow.instance.HandleLobbyEvent(msg, addressParts[1]);
                break;

            case "game":
                BoardManager.instance.HandleGameEvent(msg, addressParts[1]);
                break;

            case "player":
                BoardManager.instance.HandlePlayerEvent(msg, addressParts);
                break;
        }
    }

    void Start()
    {
        if (localServer)
        {
            serverIP = "127.0.0.1";
            serverPort = 50001;
        }

        // enforce standard port range
        if (localPort < 49125 | localPort > 65535 && localPort != 0)
        {
            Debug.LogError("Client port must be either 0 (for random) or in range 49125-65355");
            return;
        }

        try
        {
            client = localPort > 0 ? new TcpClient(new IPEndPoint(IPAddress.Any, localPort)) : new TcpClient();

            client.Connect(serverIP, serverPort);
            Debug.Log($"Started client on {client.Client.LocalEndPoint}, connected to {client.Client.RemoteEndPoint}");
        }
        catch (SocketException)
        {
            ChatWindow.instance.DisplayMessage($"target machine refused connection, are you sure there is a sevrver running on port {serverPort}?");
        }
        catch (Exception exception)
        {
            ChatWindow.instance.DisplayMessage($"Error: {exception.Message}");
            Debug.LogError(exception);
        }
    }
    void Update()
    {
        if (client.Available > 0)
        {
            NetworkStream stream = client.GetStream();

            // message length is denoted by a 32 bit integer -> 4 bytes
            byte[] lengthBytes = new byte[4];
            stream.Read(lengthBytes, 0, 4);
            int packetLength = BitConverter.ToInt32(lengthBytes, 0);

            // read the actual packet content
            byte[] bytes = new byte[packetLength];
            stream.Read(bytes, 0, packetLength);

            OscMessage message = OscMessage.Read(bytes, packetLength);

            OnServerMessage(message);
        }
    }
    private void OnDestroy()
    {
        Debug.Log("Disconnecting");
        if (client != null)
        {
            client.Close();
            client.Dispose();
        }
    }

    // helper methods
    public void SendOscMessage(OscMessage message)
    {
        byte[] oscData = message.ToByteArray();
        byte[] lengthBytes = BitConverter.GetBytes(oscData.Length);

        NetworkStream stream = client.GetStream();
        stream.Write(lengthBytes, 0, 4);
        stream.Write(oscData, 0, oscData.Length);
    }
}
