using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using MiniJSON;

/// <summary>
/// サーバーに接続してデータのやり取りをする
/// 使うときはコールバックをoverrideする
/// </summary>


enum GameState
{
    Matching,
    Playing,
    Finish,
    Reset
}

public class ClientNetwork : MonoBehaviour
{
    private TcpClient connection;
    private GameState currentState;

    public int port = 30000;
    public string severIP = "192.168.0.200";


    // Start is called before the first frame update
    void Start()
    {
        currentState = GameState.Reset;

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected void OnApplicationQuit()
    {
        connection.Close();
    }


// ネットワーク

    public void Connect(string address, int port)
    {
        Task.Run(() =>
        {
            connection = new TcpClient(address, port);
            var stream = connection.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);

            while (connection.Connected)
            {
                while (!reader.EndOfStream)
                {
                    string str = reader.ReadLine();
                    OnMessage(str);
                }


                if (connection.Client.Poll(1000, SelectMode.SelectRead) && (connection.Client.Available == 0))
                {
                    Debug.Log("Disconnect: " + connection.Client.RemoteEndPoint);
                    connection.Close();
                    break;
                }
            }

        });

        
    }

    private void OnMessage(string str)
    {
        Dictionary<string, object> msgJson = stringToDict(str);
        string msgName = (string)msgJson["name"];
        string msgValue = (string)msgJson["value"];

        if (msgName == "joined")
        {

        }
        else if (msgName == "start" && currentState == GameState.Reset)
        {

        }

    }

    protected void SendMessageToServer(string msg)
    {
        var body = Encoding.UTF8.GetBytes(msg);
        connection.GetStream().Write(body, 0, body.Length);

    }

    private Dictionary<string, object> stringToDict(string str)
    {
        return Json.Deserialize(str) as Dictionary<string, object>;
    }


// 状態遷移メソッド

    private void GameMatching()
    {
        currentState = GameState.Matching;
        Connect(severIP, port);
    }

    private void GameStart()
    {
        currentState = GameState.Playing;

    }

    private void GameFinish()
    {

    }

    private void GameReset()
    {

    }

    
// コールバック

    protected virtual void GameStartReady()
    {

        SendMessageToServer("{\"name\":\"start\"}");
    }
}
