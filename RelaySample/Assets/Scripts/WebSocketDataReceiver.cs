using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;

public class WebSocketDataReceiver : MonoBehaviour
{
    public string address;
    private WebSocket _webSocket;

    private string _text;

    private Text _outputText;
    public GameObject outputText;

    void Start()
    {
        if (string.IsNullOrEmpty(address))
        {
            return;
        }

        if (outputText != null)
        {
            _outputText = outputText.GetComponent<Text>();
        }

        _webSocket = new WebSocket(address);
        _webSocket.OnOpen += WebSocket_OnOpen;
        _webSocket.OnClose += WebSocket_OnClose;
        _webSocket.OnError += WebSocket_OnError;
        _webSocket.OnMessage += WebSocket_OnMessage;
        _webSocket.ConnectAsync();
    }

    private void WebSocket_OnMessage(object sender, MessageEventArgs e)
    {
        _text = e.Data;
    }

    private void WebSocket_OnError(object sender, ErrorEventArgs e)
    {
        Debug.LogError(e.Exception);
        _text = e.Message;
    }

    private void WebSocket_OnClose(object sender, CloseEventArgs e)
    {
        Debug.Log(e.Reason);
        _text = "OnClose";
    }

    private void WebSocket_OnOpen(object sender, System.EventArgs e)
    {
        _text = "OnOpen";
    }

    private float delta = 0L;
    private void Update()
    {
        delta += Time.deltaTime;
        if (delta >= 10.0f)
        {
            _webSocket.Send($"Hello world {DateTime.Now}");
            delta = 0.0f;
        }

        if (_outputText != null)
        {
            _outputText.text = _text;
        }
    }
}
