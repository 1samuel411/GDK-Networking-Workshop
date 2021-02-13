using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    public ClientManager clientManager;

    public GameObject connectedPanel;
    public GameObject unConnectedPanel;

    private string message;

    public Text chatLogText;
    public Text userIDText;

    private void Start()
    {
        clientManager.client.onRecievedUserID += RecievedUserID;
        clientManager.client.onRecievedMessage += RecievedMessage;
        chatLogText.text = "";
    }

    void RecievedMessage(string message)
    {
        chatLogText.text += "\n" + message;
    }

    void RecievedUserID(string message)
    {
        userIDText.text = "User ID - " + message;
    }

    public void SetMessage(string message)
    {
        this.message = message;
    }

    public void Send()
    {
        clientManager.client.SendToServer(10, message);
    }

    private void Update()
    {
        if(clientManager.client.IsConnected())
        {
            connectedPanel.gameObject.SetActive(true);
            unConnectedPanel.gameObject.SetActive(false);
        }
        else
        {
            connectedPanel.gameObject.SetActive(false);
            unConnectedPanel.gameObject.SetActive(true);
        }
    }

}
