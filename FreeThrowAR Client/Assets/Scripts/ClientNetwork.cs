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
    GameState currentState;

    public int port = 30000;
    public string serverIP = "192.168.0.200";


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

    private void Connect(string address, int port)
    {
        Task.Run(() =>
        {
            connection = new TcpClient(address, port);
            var stream = connection.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            Debug.Log("Connect:" + connection.Client.RemoteEndPoint);

            while (true)
            {   
                while (!reader.EndOfStream)
                {
                    string str = reader.ReadLine();
                    Debug.Log("MessageReceived: " + str);
                    OnMessage(str);
                }


                if (connection.Client.Poll(1000, SelectMode.SelectRead) && (connection.Client.Available == 0))
                {
                    Debug.Log("Disconnect: " + connection.Client.RemoteEndPoint);
                    connection.Close();
                    GameReset();
                    break;
                }
            }

        });

        
    }


    // パケットを受け取った時
    private void OnMessage(string str)
    {
        IDictionary msgJson = null;
        var msgName = "";

        msgJson = (IDictionary)Json.Deserialize(str);
        msgName = (string)msgJson["name"];

        if (msgName == "joined")
        {
            var msgValue = (int)msgJson["value"];
            Debug.Log("Joined" + msgValue);
            OnPlayerJoined(msgValue);
        }
        else if (msgName == "start" && currentState == GameState.Matching)
        {
            GameStart();
            OnGameStart();
        }
        else if(msgName == "ball" && currentState == GameState.Playing)
        {
            float posx = (float)msgJson["posx"];
            float posy = (float)msgJson["posy"];
            float posz = (float)msgJson["posz"];
            float wayx = (float)msgJson["wayx"];
            float wayy = (float)msgJson["wayy"];
            float wayz = (float)msgJson["wayz"];

            Vector3 pos = new Vector3(posx, posy, posz);
            Vector3 way = new Vector3(wayx, wayy, wayz);

            OnReceiveBallData(pos, way);
        }

    }

    protected void SendMessageToServer(string msg)
    {
        msg += "\n";
        var body = Encoding.UTF8.GetBytes(msg);
        connection.GetStream().Write(body, 0, body.Length);

    }
    




// 状態遷移メソッド

    private void GameMatching()
    {
        Debug.Log("State: Matching");
        currentState = GameState.Matching;
    }

    private void GameStart()
    {
        Debug.Log("State: Playing");
        currentState = GameState.Playing;

    }

    private void GameFinish()
    {
        Debug.Log("State: Finish");
        currentState = GameState.Finish;

    }

    private void GameReset()
    {
        Debug.Log("State: Reset");
        currentState = GameState.Reset;
    }





 // パブリックメソッドとコールバック
    public void ConnectToSerer()
    {
        currentState = GameState.Matching;
        Connect(serverIP, port);
    }

    public void GameStartReady()
    {
        SendMessageToServer("{\"name\":\"start\"}");
    }

    protected virtual void OnPlayerJoined(int playerCount)
    {

    }

    protected virtual void OnGameStart()
    {

    }

    public void SendBallData(Vector3 pos, Vector3 way)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("name", "ball");
        dict.Add("posx", pos.x);
        dict.Add("posy", pos.y);
        dict.Add("posz", pos.z);
        dict.Add("wayx", way.x);
        dict.Add("wayy", way.y);
        dict.Add("wayz", way.z);
        string json = Json.Serialize(dict);
        SendMessageToServer(json);
        

    }

    protected virtual void OnReceiveBallData(Vector3 pos, Vector3 way)
    {

    }

    public void sendPointData(int point)
    {
        point = 10;
        GameFinish();
        SendMessageToServer("{\"name\":\"finish\",\"value\":" + point + "}");
    }

    protected virtual void OnReceiveRankingData()
    {

    }

    public void sendGameReset()
    {
        GameReset();
        SendMessageToServer("{\"name\":\"reset\"}");
    }
    
}
