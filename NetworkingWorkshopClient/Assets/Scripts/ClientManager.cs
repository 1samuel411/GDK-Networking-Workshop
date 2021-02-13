using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientManager : MonoBehaviour
{

    public Client client;

    public void ConnectToServer()
    {
        client.Connect();
    }
}
