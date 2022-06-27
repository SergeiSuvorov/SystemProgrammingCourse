using System;
using Main;
using Mirror;
using Network;
using UnityEngine;

public class CristalController : NetworkBehaviour
{

    [SyncVar] protected Vector3 serverPosition;
    [SyncVar] protected Vector3 serverEulers;
    public override void OnStartServer()
    {
       
        base.OnStartServer();
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        networkManager.AddCristalToList(this);

    }


    protected  void SendToClients()
    {
        serverPosition = transform.position;
        serverEulers = transform.eulerAngles;
    }

    private void FixedUpdate()
    {
        Movement();
    }

    protected virtual void Movement()
    {
        if (!isServer)
        {
            FromSeverUpdate();
        }
        else
        {
            SendToClients();
        }
    }
    [Client]
    protected void FromSeverUpdate()
    {
        transform.position = serverPosition;
        transform.rotation = Quaternion.Euler(serverEulers);
    }


    private void OnTriggerEnter(Collider other)
    {
        var ship = other.gameObject.GetComponent<ShipController>();
        if (ship != null)
        {
            if (isServer)
                ServerTriggerEnter(this, ship);
            else
                CmdTriggerEnter(this, ship);
        }
    }


    private void ServerTriggerEnter(CristalController cristal, ShipController ship)
    {
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        networkManager.RemoveCristalFromList(cristal, ship);
    }

    [Command(requiresAuthority =false)]
    private void CmdTriggerEnter(CristalController cristal, ShipController ship)
    {
        ServerTriggerEnter(cristal, ship);
    }
}

