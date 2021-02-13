using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class Client : MonoBehaviour
{

    private Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public delegate void OnRecievedMessage(string message);
    public OnRecievedMessage onRecievedMessage;
    public delegate void OnRecievedUserID(string userID);
    public OnRecievedUserID onRecievedUserID;

    private string ip;
    private int port;

    public void Connect(string ip = "50.88.215.23", int port = 25565)
    {
        if(clientSocket == null)
        {
            return;
        }
        if(IsConnected())
        {
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();
        }

        this.ip = ip;
        this.port = port;
        StartCoroutine(ConnectCoroutine());
    }

    IEnumerator ConnectCoroutine()
    {
        bool failed = false;
        int attempts = 0;
        while(IsConnected() == false)
        {
            attempts++;
            if (attempts >= 5)
                break;

            try
            {
                Debug.Log("Attempting to connect");
                failed = false;
                clientSocket.Connect(IPAddress.Parse(ip), port);
            }
            catch(Exception e)
            {
                failed = true;
                Debug.Log("Connection failed, trying again");
            }

            if (failed)
            {
                yield return new WaitForSeconds(3);
            }

            yield return null;
        }

        if(failed)
        {
            Debug.Log("Could not connect to server");
            yield break;
        }

        Debug.Log("Client Connected");
        clientSocket.ReceiveTimeout = 2000;
        clientSocket.SendTimeout = 2000;
        StartCoroutine(RecieveData());
    }

    public void SendToServer(int header, string message)
    {
        if(IsConnected() == false)
        {
            Debug.Log("Not connected!");
            return;
        }
        
        byte[] tempArray = Encoding.UTF8.GetBytes(message);

        byte[] dataArray = new byte[tempArray.Length + 1];
        dataArray[0] = Convert.ToByte(header);
        Array.Copy(tempArray, 0, dataArray, 1, tempArray.Length);

        Debug.Log("Sending to server: " + message);

        clientSocket.BeginSend(dataArray, 0, dataArray.Length, SocketFlags.None, SendCallback, clientSocket);
    }

    public void SendCallback(IAsyncResult Result)
    {
        Socket socket = (Socket)Result.AsyncState;
        socket.EndSend(Result);
    }

    public IEnumerator RecieveData()
    {
        byte[] buffer = new byte[1024];
        int recieved = 0;

        while(IsConnected())
        {
            if(clientSocket.Available > 0)
            {
                recieved = clientSocket.Receive(buffer);
                byte[] dataRecieved = new byte[recieved];
                Array.Copy(buffer, dataRecieved, recieved);

                byte header = dataRecieved[0];
                byte[] messageData = new byte[dataRecieved.Length - 1];
                Array.Copy(dataRecieved, 1, messageData, 0, messageData.Length);

                string messageRecieved = Encoding.UTF8.GetString(messageData);

                if (header == 0)
                {
                    onRecievedUserID.Invoke(messageRecieved);
                }
                else if (header == 9)
                {
                    onRecievedMessage.Invoke(messageRecieved);
                }

                Debug.Log("Recieved from server: (" + header + ") - " + messageRecieved);
            }

            yield return null;
        }
    }

    public bool IsConnected()
    {
        return clientSocket.Connected;
    }
}
