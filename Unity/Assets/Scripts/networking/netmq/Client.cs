using System.Collections.Concurrent;
using UnityEngine;
using vrcode.networking.netmq;

public class Client : MonoBehaviour
{
    private NetMQListener _netMqListener;

    private void HandleMessage(string message)
    {
        // TODO
    }

    private void Start()
    {
        _netMqListener = new NetMQListener(HandleMessage);
        _netMqListener.Start();
    }

    private void Update()
    {
        _netMqListener.Update();
    }

    private void OnDestroy()
    {
        if (_netMqListener != null)
            _netMqListener.Stop();
    }
}