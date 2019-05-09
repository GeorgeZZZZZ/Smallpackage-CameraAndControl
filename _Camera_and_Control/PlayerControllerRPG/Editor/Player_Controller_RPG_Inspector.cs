using UnityEngine;
using UnityEditor;

/*  vr:
 *  - 0.2.0
 *  change script name
 *  2019.02.08 split inspecte script to two part, one for rpg one for rts
 *      change onenable to virtual
 */
namespace GeorgeScript
{
    [CustomEditor(typeof(Player_Controller_RPG))]
    public class Player_Controller_RPG_Inspector : Editor
    {
        Player_Controller_RPG CPC;

        SerializedObject serOBJ;
        SerializedProperty PMB, PTB, camOBJ;

        public virtual void OnEnable()
        {
            CPC = (Player_Controller_RPG)target;

            serOBJ = new SerializedObject(target);
            PMB = serOBJ.FindProperty("PlayerMoveBehivior");    //  find serializable enum
            PTB = serOBJ.FindProperty("PlayerTurnBehivior");
            //camOBJ = serOBJ.FindProperty("Cam_Center_Point"); // 19.02.15 auto search for cam obj, not really need to show this any more
        }

        public override void OnInspectorGUI()
        {
            serOBJ.Update();    //  update serializeable objs

            //  General Settings---------------------------
            GUILayout.Space(10);
            EditorGUILayout.LabelField("General Settings", EditorStyles.boldLabel);
            //EditorGUILayout.PropertyField(camOBJ);

            CPC.Edge_Boundary = EditorGUILayout.IntField("Edge_Boundary", CPC.Edge_Boundary);
            CPC.Player_Normal_Speed = EditorGUILayout.FloatField("Player_Normal_Speed", CPC.Player_Normal_Speed);
            CPC.Player_Run_Speed = EditorGUILayout.FloatField("Player_Run_Speed", CPC.Player_Run_Speed);
            CPC.Player_Turnning_Speed = EditorGUILayout.FloatField("Player_Turnning_Speed", CPC.Player_Turnning_Speed);
            CPC.Jump_Speed = EditorGUILayout.FloatField("Jump_Speed", CPC.Jump_Speed);

            //  Character move type Settings---------------------------
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Settings Player Move Type: ", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(PMB);
            switch (PMB.enumValueIndex)
            {
                case 0:
                    CPC.Move_Or_Turn_Player_According_To_Camera = true;
                    CPC.Move_Player_towards_Character_Facing = false;
                    CPC.Move_Player_Along_World_Axis = false;
                    break;
                case 1:
                    CPC.Move_Or_Turn_Player_According_To_Camera = false;
                    CPC.Move_Player_towards_Character_Facing = true;
                    CPC.Move_Player_Along_World_Axis = false;
                    break;
                case 2:
                    CPC.Move_Or_Turn_Player_According_To_Camera = false;
                    CPC.Move_Player_towards_Character_Facing = false;
                    CPC.Move_Player_Along_World_Axis = true;
                    break;
            }

            if (!CPC.Move_Or_Turn_Player_According_To_Camera)
            {
                EditorGUILayout.PropertyField(PTB);
                switch (PTB.enumValueIndex)
                {
                    case 0:
                        CPC.Turn_Player_by_Keyboard = true;
                        CPC.Turn_Player_by_Mouse_Point = false;
                        break;
                    case 1:
                        CPC.Turn_Player_by_Keyboard = false;
                        CPC.Turn_Player_by_Mouse_Point = true;
                        break;
                }
            }
            else
            {
                CPC.Turn_Player_by_Keyboard = false;
                CPC.Turn_Player_by_Mouse_Point = false;
            }

            serOBJ.ApplyModifiedProperties();   //  apply serializable objs change
        }

    }
}