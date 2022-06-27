using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetsSpawner : NetworkBehaviour
{
    [SerializeField] public List<PlanetCoordinate> planetCoordinates;

    public override void OnStartServer()
    {
        base.OnStartServer();

        for (int i = 0; i < planetCoordinates.Count; i++)
        {
            GameObject planetInstance = Instantiate(planetCoordinates[i].planetOrbit.gameObject);

            NetworkServer.Spawn(planetInstance, NetworkServer.localConnection);
            planetInstance.transform.position = planetCoordinates[i].coordinate;
        }
        Destroy(gameObject);
    }

    [Serializable]
    public struct PlanetCoordinate
    {
        public PlanetOrbit planetOrbit;
        public Vector3 coordinate;
    }
   
}
