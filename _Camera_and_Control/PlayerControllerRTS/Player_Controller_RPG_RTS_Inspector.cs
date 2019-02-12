using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GeorgeScript
{

    [CustomEditor(typeof(Player_Controller_RPG_RTS))]
    public class Player_Controller_RPG_RTS_Inspector : Player_Controller_RPG_Inspector
    {
        Player_Controller_RPG_RTS PlayerRR;
        SerializedObject serOBJ;
        SerializedProperty selectOBJ;
        public override void OnEnable()
        {
            base.OnEnable();
            PlayerRR = (Player_Controller_RPG_RTS)target;
            serOBJ = new SerializedObject(target);
            selectOBJ = serOBJ.FindProperty("Select_Circle_Prefab");

        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // RTS Setting
            GUILayout.Space(10);
            EditorGUILayout.LabelField("RTS Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(selectOBJ);

            serOBJ.ApplyModifiedProperties();   //  apply serializable objs change
        }
    }
}
