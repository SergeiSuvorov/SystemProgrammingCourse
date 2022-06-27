#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlanetsSpawnSettings))]
public class PlanetsSpawnSettingsEditor : Editor
{
    private PlanetsSpawnSettings _dataBase;
    void Awake()
    {
        _dataBase = (PlanetsSpawnSettings)target;
    }

    private void OnEnable()
    {
        _dataBase = (PlanetsSpawnSettings)target;
    }

    public override void OnInspectorGUI()
    {

        if (_dataBase == null)
        {
            _dataBase = (PlanetsSpawnSettings)target;
            Debug.Log("wrong");
            return;
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("New Record"))
            _dataBase.CreateRecord();
        if (GUILayout.Button("Remove"))
            _dataBase.RemoveRecord();
        if (GUILayout.Button("<="))
            _dataBase.PrevRecord();
        if (GUILayout.Button("=>"))
            _dataBase.NextRecord();

        GUILayout.EndHorizontal();

        base.OnInspectorGUI();

    }
}
#endif
