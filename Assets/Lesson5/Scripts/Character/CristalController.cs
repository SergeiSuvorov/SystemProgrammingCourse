using System;
using Main;
using Mirror;
using Network;
using UnityEngine;

public class CristalController : NetworkMovableObject
{
    protected override float speed => throw new NotImplementedException();

    public override void OnStartAuthority()
    {
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        networkManager.AddCristalToList(this);

        base.OnStartAuthority();
    }

    protected override void FromOwnerUpdate()
    {
        return;
    }

    protected override void HasAuthorityMovement()
    {
        return;
    }

    protected override void SendToClients()
    {
        serverPosition = transform.position;
        serverEulers = transform.eulerAngles;
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

