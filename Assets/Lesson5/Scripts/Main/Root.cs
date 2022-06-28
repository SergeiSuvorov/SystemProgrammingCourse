using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour
{
    public PlanetsSpawnSettings planetsSpawnSettings;
    public SpaceShipSettings spaceShipSettings;
    public SpawnSettings _spawnSettings;
    public SolarSystemNetworkManager networkManagerPrefab;
    public UiManager uiManager;

    [Space(20)]
    public List<Transform> spaceShipSpawnPoints;
    

    void Start()
    {
        var networkManager = Instantiate(networkManagerPrefab);
        foreach (var spawnPoint in spaceShipSpawnPoints)
            NetworkManager.RegisterStartPosition(spawnPoint);

        networkManager.Init(Instantiate(spaceShipSettings));
        networkManager.OnServerStart += SpawnPlanet;

    }
    private void SpawnPlanet()
    {
        var spawnSettings = Instantiate(_spawnSettings);
        PlanetSpawner.Spawn(Instantiate(planetsSpawnSettings));
        CristalSpawner.Spawn(spawnSettings);


        var asteroidOrbit = AsteroidOrbit.Instantiate(spawnSettings.AsteroidOrbit);

        asteroidOrbit.Init(spawnSettings.AsteroidRadius, spawnSettings.AsteroidRadiusOffset, spawnSettings.AsteroidSpeed , spawnSettings.MaxScale, spawnSettings.Depth);
        NetworkServer.Spawn(asteroidOrbit.gameObject);
    }
}

