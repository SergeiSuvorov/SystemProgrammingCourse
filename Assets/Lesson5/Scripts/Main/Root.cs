using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Root : MonoBehaviour
{
    public PlanetsSpawnSettings planetsSpawnSettings;
    public SpaceShipSettings spaceShipSettings;
    public SpawnSettings spawnSettings;
    public SolarSystemNetworkManager networkManagerPrefab;
    public UiManager uiManager;
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
        PlanetSpawner.Spawn(Instantiate(planetsSpawnSettings));
        CristalSpawner.Spawn(Instantiate(spawnSettings));
    }
}

