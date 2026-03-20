using TMPro;
using UnityEngine;

public class LobbyButton : MonoBehaviour
{
    public string name;
    public void Init(string name)
    {
        this.name = name;
        GetComponentInChildren<TMP_Text>().text = name;
    }
    public void JoinLobby()
    {
        NetworkManager.instance.SendOscMessage(new Rug.Osc.OscMessage("/lobby/join", name));
    }
}
