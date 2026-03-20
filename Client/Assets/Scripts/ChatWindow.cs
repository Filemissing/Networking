using Rug.Osc;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatWindow : MonoBehaviour
{
    public static ChatWindow instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    [SerializeField] TextMeshProUGUI textField;
    [SerializeField] TMP_InputField inputField;

    bool showGlobalChat;
    public void SwitchChat()
    {
        showGlobalChat = !showGlobalChat;

        textField.text = "";
        if (showGlobalChat)
            foreach (string line in globalLines)
                textField.text += line + '\n';
        else
            foreach (string line in gameLines)
                textField.text += line + '\n';
    }

    const int maxLinesDisplay = 22;
    List<string> gameLines = new List<string>();
    List<string> globalLines = new List<string>();

    public void RecieveMessage(OscMessage msg, string subAddress)
    {
        DisplayMessage(msg[0] as string, subAddress == "global");
    }

    public void OnMessageEntered()
    {
        string str = inputField.text;
        if (string.IsNullOrEmpty(str))
            return;

        DisplayMessage("Me: " + str);

        if (showGlobalChat)
            NetworkManager.instance.SendOscMessage(new OscMessage("/echo/global", str));
        else
            NetworkManager.instance.SendOscMessage(new OscMessage("/echo/game", str));

        // Clear the input field, and activate it again for the next user input
        inputField.text = "";
        inputField.ActivateInputField();
        inputField.Select();
    }
    public void DisplayMessage(string text, bool globalRecieve = false)
    {
        string[] newLines = text.Split('\n');

        if (globalRecieve)
        {
            globalLines.AddRange(newLines);
            if (globalLines.Count > maxLinesDisplay)
            {
                globalLines.RemoveRange(0, globalLines.Count - maxLinesDisplay);
            }
        }
        else
        {
            gameLines.AddRange(newLines);
            if (gameLines.Count > maxLinesDisplay)
            {
                gameLines.RemoveRange(0, gameLines.Count - maxLinesDisplay);
            }
        }

        if (showGlobalChat)
        {
            textField.text = "";
            foreach (string line in globalLines)
            {
                textField.text += line + '\n';
            }
        }
        else
        {
            textField.text = "";
            foreach (string line in gameLines)
            {
                textField.text += line + '\n';
            }
        }
    }
}
