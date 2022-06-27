using UnityEngine;
using Mirror;

public class CristalSpawner : NetworkBehaviour
{

    public static void Spawn(SpawnSettings spawnSettins)
    {
        var cristalPrefab = spawnSettins.StarPrefab;
        var cristalCount = spawnSettins.StarCount;
        var cristalSpawnZoneMin = spawnSettins.StarSpawnZoneMin;
        var cristalSpawnZoneMax = spawnSettins.StarSpawnZoneMax;
        for (int i = 0; i < cristalCount; i++)
        {

            int xPos = Random.Range(-1, 2) * Random.Range(cristalSpawnZoneMin, cristalSpawnZoneMax);
            int yPos = Random.Range(-1, 2) * Random.Range(cristalSpawnZoneMin, cristalSpawnZoneMax);
            int zPos = Random.Range(-1, 2) * Random.Range(cristalSpawnZoneMin, cristalSpawnZoneMax);

            GameObject cristalInstance = Instantiate(cristalPrefab.gameObject);
            NetworkServer.Spawn(cristalInstance, NetworkServer.localConnection);
            cristalInstance.transform.position = new Vector3(xPos, yPos, zPos);
        }
    }
}
