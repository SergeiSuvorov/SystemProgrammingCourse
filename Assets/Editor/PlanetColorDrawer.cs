using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[CustomPropertyDrawer(typeof(PlanetColorGroup))]
public class PlanetColorDrawer : PropertyDrawer
{
    private float labelWidth = 50f;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {

        GUIStyle style = new GUIStyle(EditorStyles.largeLabel);
        style.fontStyle = FontStyle.Bold;
        label = EditorGUI.BeginProperty(position, label, property);

        var contentPosition = EditorGUI.PrefixLabel(position, label, style);
        contentPosition.width *= 0.5f;
        contentPosition.y += 20;
        contentPosition.x = position.x + 20;
        EditorGUI.indentLevel = 0;
      

        EditorGUIUtility.labelWidth = labelWidth;
        contentPosition.y += 20;
        EditorGUI.PropertyField(contentPosition,
        property.FindPropertyRelative("GroundColor"), new GUIContent("Ground"));

        contentPosition.x += contentPosition.width;

        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUI.PropertyField(contentPosition,
        property.FindPropertyRelative("SeaColor"), new GUIContent("Sea"));

        contentPosition.x += contentPosition.width;

        EditorGUIUtility.labelWidth = labelWidth;
        EditorGUI.PropertyField(contentPosition,
        property.FindPropertyRelative("MountainColor"), new GUIContent("Mount"));

        EditorGUI.EndProperty();

    }

}
