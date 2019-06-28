﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;

/// <summary>
/// サーバーに接続してデータのやり取りをする
/// 使うときは継承してコールバックをoverrideする
/// </summary>

public class ClientNetwork : MonoBehaviour
{
    private TcpClient connection;
    private string ipAddress;
    private int port;

    public void Connect(string address, int port)
    {
        Task.Run(() =>
        {
            connection = new TcpClient(address, port);
            var stream = connection.GetStream();
            var reader = new StreamReader(stream, Encoding.UTF8);

        });

        
    }

    public void OnMessage(string str)
    {

    }

}