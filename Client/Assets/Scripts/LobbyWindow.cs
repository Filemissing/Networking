using Rug.Osc;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class LobbyWindow : MonoBehaviour
{
    public static LobbyWindow instance;
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    [Header("Lobby List")]
    [SerializeField] CanvasGroup listGroup;
    [SerializeField] Transform listParent;
    [SerializeField] LobbyButton lobbyButtonPrefab;

    [SerializeField] TMP_InputField lobbyNameField;

    [Header("Lobby info")]
    [SerializeField] CanvasGroup infoGroup;
    [SerializeField] TMP_Text infoText;

    
    public void HandleLobbyEvent(OscMessage msg, string subAddress)
    {
        switch (subAddress)
        {
            case "list":
                UpdateList(msg.Cast<string>().ToArray());
                break;

            case "join":
                JoinLobby(msg[0] as string, (int)msg[1]);
                break;
        }
    }

    private void Start() => StartCoroutine(LobbyRequestLoop());
    private void OnDestroy() => StopAllCoroutines();


    public void JoinLobby(string name, int playerCount)
    {
        NetworkManager.instance.isInLobby = true;

        listGroup.alpha = 0;
        listGroup.interactable = false;
        listGroup.blocksRaycasts = false;

        infoGroup.alpha = 1;
        infoGroup.interactable = true;
        infoGroup.blocksRaycasts = true;

        infoText.text = $"<b>{name}</b>\nPlayers: {playerCount}/2";
    }
    public void LeaveLobby()
    {
        NetworkManager.instance.SendOscMessage(new OscMessage("/lobby/leave"));
        NetworkManager.instance.isInLobby = false;

        infoGroup.alpha = 0;
        infoGroup.interactable = false;
        infoGroup.blocksRaycasts = false;

        listGroup.alpha = 1;
        listGroup.interactable = true;
        listGroup.blocksRaycasts = true;
    }

    const int waitTime = 1;
    IEnumerator LobbyRequestLoop()
    {
        while (true)
        {
            if (NetworkManager.instance.isInLobby == true)
                yield return new WaitUntil(() => NetworkManager.instance.isInLobby == false);

            NetworkManager.instance.SendOscMessage(new OscMessage("/lobby/list"));
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void UpdateList(string[] newList)
    {
        ClearList();

        foreach (string entry in newList)
        {
            LobbyButton button = Instantiate(lobbyButtonPrefab, listParent);
            button.Init(entry);
        }
    }
    void ClearList()
    {
        for (int i = 0; i < listParent.childCount; i++)
            Destroy(listParent.GetChild(i).gameObject);
    }

    public void CreateLobby()
    {
        string name = lobbyNameField.text;

        if (string.IsNullOrEmpty(name))
            return;

        NetworkManager.instance.SendOscMessage(new Rug.Osc.OscMessage("/lobby/create", name));

        lobbyNameField.ActivateInputField();
    }
}
