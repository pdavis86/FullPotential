using MLAPI;
using MLAPI.Messaging;
using System.Collections.Generic;
using UnityEngine;

//NOTE: Copied and adapted from the NetworkStats class from https://github.com/Unity-Technologies/com.unity.multiplayer.samples.coop/releases/tag/v0.2.1

[RequireComponent(typeof(NetworkObject))]
public class NetworkStats : NetworkBehaviour
{
    // Client sends a ping RPC to the server and starts it's timer.
    // The server receives the ping and sends a pong response to the client.
    // The client receives that pong response and stops its time.
    // The RPC value is using a moving average, so we don't have a value that moves too much, but is still reactive to RTT changes.

    [SerializeField]
    [Tooltip("The interval to send ping RPCs to calculate the RTT. The bigger the number, the less reactive the stat will be to RTT changes")]
    float _pingIntervalSeconds = 0.1f;

    const int _maxWindowSizeSeconds = 3;

    public float LastRtt { get; private set; }

    float _maxWindowSize => _maxWindowSizeSeconds / _pingIntervalSeconds;

    float _lastPingTime;
    int _currentPingId;
    Queue<float> _movingWindow = new Queue<float>();
    Dictionary<int, float> _pingHistoryStartTimes = new Dictionary<int, float>();
    ClientRpcParams _pongClientParams;

    public override void NetworkStart()
    {
        bool isClientOnly = IsClient && !IsServer;
        if (!IsOwner && isClientOnly)
        {
            Destroy(this);
            return;
        }

        _pongClientParams = new ClientRpcParams { Send = new ClientRpcSendParams() { TargetClientIds = new[] { OwnerClientId } } };
    }

    void FixedUpdate()
    {
        if (IsServer)
        {
            return;
        }

        if (Time.realtimeSinceStartup - _lastPingTime > _pingIntervalSeconds)
        {
            // We could have had a ping/pong where the ping sends the pong and the pong sends the ping. Issue with this
            // is the higher the latency, the lower the sampling would be. We need pings to be sent at a regular interval
            PingServerRPC(_currentPingId);
            _pingHistoryStartTimes[_currentPingId] = Time.realtimeSinceStartup;
            _currentPingId++;
            _lastPingTime = Time.realtimeSinceStartup;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PingServerRPC(int pingId, ServerRpcParams serverParams = default)
    {
        PongClientRPC(pingId, _pongClientParams);
    }

    [ClientRpc]
    public void PongClientRPC(int pingId, ClientRpcParams clientParams = default)
    {
        var startTime = _pingHistoryStartTimes[pingId];
        _pingHistoryStartTimes.Remove(pingId);
        _movingWindow.Enqueue(Time.realtimeSinceStartup - startTime);
        UpdateRTTSlidingWindowAverage();
    }

    void UpdateRTTSlidingWindowAverage()
    {
        if (_movingWindow.Count > _maxWindowSize)
        {
            _movingWindow.Dequeue();
        }

        float rttSum = 0;
        foreach (var singleRTT in _movingWindow)
        {
            rttSum += singleRTT;
        }

        LastRtt = rttSum / _maxWindowSize;
    }

}
