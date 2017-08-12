using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelComponent))]
public class LevelComponentEditor : Editor {

    private LevelComponent levelComponent;

    private SerializedProperty laneCountProperty;
    private SerializedProperty expandMiddleLaneProperty;
    private SerializedProperty defaultLaneProperty;
    private SerializedProperty laneWidthProperty;
    private SerializedProperty lanesProperty;

    private GUIContent defaultLaneLabel;

    private void OnEnable() {
        levelComponent = (LevelComponent) target;

        laneCountProperty = serializedObject.FindProperty("laneCount");
        expandMiddleLaneProperty = serializedObject.FindProperty("expandMiddleLane");
        defaultLaneProperty = serializedObject.FindProperty("defaultLane");
        laneWidthProperty = serializedObject.FindProperty("laneWidth");
        lanesProperty = serializedObject.FindProperty("lanes");

        defaultLaneLabel = new GUIContent("Middle lane");
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        EditorGUILayout.PropertyField(expandMiddleLaneProperty);
        EditorGUILayout.Space();

        // we want the program to calculate other lanes for us
        if (expandMiddleLaneProperty.boolValue) {
            EditorGUILayout.PropertyField(laneCountProperty);
            EditorGUILayout.PropertyField(laneWidthProperty);

            EditorGUILayout.PropertyField(defaultLaneProperty, defaultLaneLabel);

            if (GUILayout.Button("Expand middle lane")) {
                levelComponent.CalculateLanesBasedOnDefaultLane();
            }
        } else { // we want to do all lanes ourselves
            EditorGUILayout.LabelField("Lanes", EditorStyles.boldLabel);
            for (int i = lanesProperty.arraySize - 1; i >= 0; i--) {
                GUILayout.BeginHorizontal();
                { 
                    EditorGUILayout.PropertyField(lanesProperty.GetArrayElementAtIndex(i), new GUIContent(GetLaneName(i)));

                    // delete
                    if (GUILayout.Button("X", GUILayout.MaxWidth(20f))) {
                        lanesProperty.DeleteArrayElementAtIndex(i);
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Add lane")) {
                int _whereToInsert;

                if (lanesProperty.arraySize % 2 == 0 && lanesProperty.arraySize != 0) _whereToInsert = lanesProperty.arraySize - 1;
                else _whereToInsert = 0;

                lanesProperty.InsertArrayElementAtIndex(_whereToInsert);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private string GetLaneName(int index) {
        string _name = "Lane ";

        int _midIndex = lanesProperty.arraySize / 2;
        if (index < _midIndex) _name += "left " + (_midIndex - index);
        else if (index > _midIndex) _name += "right " + (index - _midIndex);
        else _name += "middle";

        return _name;
    }

}
