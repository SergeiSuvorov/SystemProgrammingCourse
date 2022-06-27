using UnityEngine;

[CreateAssetMenu(fileName = "newPSpawnSettings", menuName = "Settings/SpawnSettings")]
public class SpawnSettings : ScriptableObject
{
    [Header("StarConfiguration")]
    public int StarCount;
    public int StarSpawnZoneMin;
    public int StarSpawnZoneMax;
    public CristalController StarPrefab;
}