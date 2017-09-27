using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Net;
using UnityEngine.Networking.Match;
using UnityEngine.Networking.Types;

public class MultiplayerManager : NetworkManager {


    [AddComponentMenu("Network/MultiplayerManager")]
    public enum testy
    {
        Random,
        RoundRobin
    };

    public void sendRow()
    {
            int numPlayers = 0;
            foreach (var conn in NetworkServer.connections)
            {
                if (conn == null)
                    continue;

                foreach (var p in conn.playerControllers)
                {
                    if (p.IsValid)
                    {
                        numPlayers += 1;
                    }
                }
            }
            foreach (var conn in NetworkServer.localConnections)
            {
                if (conn == null)
                    continue;

                foreach (var p in conn.playerControllers)
                {
                    if (p.IsValid)
                    {
                        numPlayers += 1;
                    }
                }
            }

        print(numPlayers);

    }

}
