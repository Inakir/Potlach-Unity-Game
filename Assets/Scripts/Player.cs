using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class Player : NetworkBehaviour {

    public GameObject[] go_grids;
    public Grid[] grids;

    [SyncVar]
    public int localVar = 0;
    [SyncVar]
    public int remoteVar = 0;
    
    void Start () {

        if (!isLocalPlayer)
            return;

            if (isServer)
            {
                print("i am the server");
                //grids[0] = go_grids[1].GetComponent<Grid>();
                //grids[1] = go_grids[0].GetComponent<Grid>();
            }
            if (!isServer) {
                print("i am the client");
                //grids[0] = go_grids[0].GetComponent<Grid>();
                //grids[1] = go_grids[1].GetComponent<Grid>();
            }

            //grids[0] = go_grids[1].GetComponent<Grid>();

    }

    [Command]
    void CmdTestCommand()
    {
        //if host, send row to client
        GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Grid>().insertRow();
    }

    [ClientRpc]
    void RpcTestCommand()
    {
        //if client, sends row to host
        GameObject.FindGameObjectsWithTag("Player")[1].GetComponent<Grid>().insertRow();

    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Input.GetKeyDown("h"))
        {
            if (!isServer)
            {
                //if host, send row to client
                CmdTestCommand();
            }
            else
            {
                //if client, sends row to host
                RpcTestCommand();
            }
        }
    }
}
