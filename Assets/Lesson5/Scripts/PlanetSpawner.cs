using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlanetSpawner : NetworkBehaviour
{
    public static void Spawn(PlanetsSpawnSettings planetSpawnSettins)
    {
        var planets = planetSpawnSettins.Planets;
        foreach (var planetData in planets)
        {
            var planetOrbit = PlanetOrbit.Instantiate(planetData.planetOrbit);
            
            planetOrbit.Init(planetData);
            NetworkServer.Spawn(planetOrbit.gameObject);

        }
    }

}
