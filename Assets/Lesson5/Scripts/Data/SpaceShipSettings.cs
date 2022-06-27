using UnityEngine;


    [CreateAssetMenu(fileName = "SpaceShipSettings", menuName = "Geekbrains/Settings/Space Ship Settings")]
    public class SpaceShipSettings : ScriptableObject
    {
        [Range(.01f, 0.1f)] public float acceleration;
        [Range(1f, 2000f)] public float shipSpeed;
        [Range(1f, 5f)] public int faster;
        [Range(.01f, 179)] public float normalFov = 60;
        [Range(.01f, 179)] public float fasterFov = 30;
        [Range(.1f, 5f)] public float changeFovSpeed = .5f;
    }

