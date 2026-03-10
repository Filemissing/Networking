using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TMPro;
using UnityEngine;

public class TextClient : MonoBehaviour
{
	[SerializeField] TextMeshProUGUI textField;
	[SerializeField] TMP_InputField inputField;

	[Header("Connection")]
	[SerializeField] string serverIP = "127.0.0.1";
	[SerializeField] int serverPort = 50001;
    [SerializeField] int localPort = 0;

	const int maxLinesDisplay = 22;
	List<string> lines = new List<string>();

	TcpClient client;
	byte[] buffer = new byte[1024];
	NetworkStream stream;

	// This method is called from an input field event:
	public void OnMessageEntered() {
        // TODO: Instead of displaying the message directly, send it to the server here:
        string str = inputField.text;
        DisplayMessage(str);

		if (stream != null)
		{
            byte[] data = Encoding.ASCII.GetBytes(str);
			stream.Write(data);
        }

        // Clear the input field, and activate it again for the next user input:
        inputField.text = "";
		inputField.ActivateInputField();
		inputField.Select();
	}

	/// <summary>
	/// Adds new text to the text display, while ensuring the total number of lines 
	/// doesn't exceed the maximum.
	/// </summary>
	void DisplayMessage(string text) {
		string[] newLines = text.Split('\n');
		lines.AddRange(newLines);
		if (lines.Count > maxLinesDisplay) {
			lines.RemoveRange(0, lines.Count - maxLinesDisplay);
		}
		textField.text = "";
		foreach (string line in lines) {
			textField.text += line + '\n';
		}
	}

	void Start()
    {
		// TODO: create a Udp or Tcp client to communicate with a server
        if (localPort < 49125 | localPort > 65535 && localPort != 0)
        {
            Debug.LogError("Client port must be either 0 or in range 49125-65355");
			return;
        }

		try
		{
            client = localPort > 0 ? new TcpClient(new IPEndPoint(IPAddress.Any, localPort)) : new TcpClient();

			client.Connect(serverIP, serverPort);
			Debug.Log($"Started client on {client.Client.LocalEndPoint}, connected to {client.Client.RemoteEndPoint}");

			stream = client.GetStream();
        }
		catch (SocketException exception)
		{
			DisplayMessage($"target machine refused connection, are you sure there is a sevrver running on port {serverPort}?");
		}
		catch (Exception exception)
		{
			DisplayMessage($"Error: {exception.Message}");
			Debug.LogError(exception);
		}
    }

    void Update()
    {
		// TODO: check the Udp or Tcp client for available incoming messages.
		// If there are any, decode and display them.
		int bytesRead = 0;
		if (client.Available > 0)
			bytesRead = stream.Read(buffer, 0, buffer.Length);

		if (bytesRead > 0) 
		{
            string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
			DisplayMessage(message);
        }
    }
}
