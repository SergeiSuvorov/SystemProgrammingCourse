using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "newPlanetsSpawnSettings", menuName = "Settings/PlanetsSpawnSettings")]
public class PlanetsSpawnSettings : ScriptableObject
{

   
    public List<PlanetData> Planets => planets;

    //[Header("StarConfiguration")]
    //public int StarCount;
    //public int StarSpawnZoneMin;
    //public int StarSpawnZoneMax;
    //public CristalController StarPrefab;


   
    [Header("PlanetData(Open for show all planet)")]
    [SerializeField] private List<PlanetData> planets;
    [Header("PlanetConfiguration")]
    [SerializeField] private PlanetData _currentBaseRecords;
    private int _currentIndex;
    public  void CreateRecord()
    {
        if (planets == null)
            planets = new List<PlanetData>();
        PlanetData record = new PlanetData(planets.Count);
        planets.Add(record);
        _currentIndex = planets.Count-1;
    }
    public  void RemoveRecord()
    {
        if (planets == null || _currentBaseRecords == null)
            return;
        planets.Remove(_currentBaseRecords);
        if (planets.Count > 0)
            _currentBaseRecords = planets[0];
        else CreateRecord();
        _currentIndex = 0;
    }
    public  void NextRecord()
    {
        if (_currentIndex + 1 < planets.Count)
        {
            _currentIndex++;
            _currentBaseRecords = planets[_currentIndex];
        }
    }
    public  void PrevRecord()
    {
        if (_currentIndex > 0)
        {
            _currentIndex--;
            _currentBaseRecords = planets[_currentIndex];
        }
    }
}


[Serializable]
public class PlanetData
{
    [SerializeField] int _id;

    [SerializeField] public string Name;

    [Header("PlanetOrbitConfiguration")]
    [SerializeField] public PlanetOrbit planetOrbit;
    [SerializeField] public float radius;

    [Header("CustomConfiguration")]
    public float seed;
    public PlanetColorGroup planetColorGroup;

    
    [Header("AtmosphereConfiguration")]
    [Space(40)]
    public bool hasAtmosphere;
    public Color AtmosphereColor;

    [Header("PlanetRingConfiguration")]
    public bool hasPlanetRing;
    public Color PlanetRingColor;

    public PlanetData(int id)
    {
        _id = id;
    }

}

[Serializable]
public class PlanetColorGroup
{
    public Color GroundColor;
    public Color SeaColor;
    public Color MountainColor;
}
