using UnityEngine;

[CreateAssetMenu(fileName = "newPSpawnSettings", menuName = "Settings/SpawnSettings")]
public class SpawnSettings : ScriptableObject
{
    [Header("StarConfiguration")]
    public int StarCount;
    public int StarSpawnZoneMin;
    public int StarSpawnZoneMax;
    public CristalController StarPrefab;


    [Header("AsteroidConfiguration")]
    [Space(30)]
    public AsteroidOrbit AsteroidOrbit;
    [SerializeField] public int Depth;
    [SerializeField, Range(0, 4)] public float AsteroidSpeed;
    [SerializeField] public int AsteroidRadius;
    [SerializeField, Range(0, 360)] public int AsteroidRadiusOffset = 4;
    [SerializeField] public int MaxScale;
}