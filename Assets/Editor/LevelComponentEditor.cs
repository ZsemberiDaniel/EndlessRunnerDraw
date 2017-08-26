using Bezier;
using EndlessRunner;
using UnityEditor;
using UnityEngine;

namespace EndlessRunnerEditor {
    [CustomEditor(typeof(LevelComponent))]
    public class LevelComponentEditor : Editor {

        private static readonly GUIContent defaultLaneLabel = new GUIContent("Middle lane");
        private static readonly GUIContent arcQualityLabel = new GUIContent("Lanes arc quality", "To how many segments the lane curves should be divided to in calcualtions.");
        private static readonly GUIStyle invisBackgroundButtonStyle = new GUIStyle();

        private LevelComponent levelComponent;

        private SerializedProperty laneCountProperty;
        private SerializedProperty expandMiddleLaneProperty;
        private SerializedProperty defaultLaneProperty;
        private SerializedProperty laneWidthProperty;
        /// <summary>
        /// Array of lanes
        /// </summary>
        private SerializedProperty lanesProperty;

        private bool[] laneFoldouts;

        private void OnEnable() {
            levelComponent = (LevelComponent) target;

            laneCountProperty = serializedObject.FindProperty("laneCount");
            expandMiddleLaneProperty = serializedObject.FindProperty("expandMiddleLane");
            defaultLaneProperty = serializedObject.FindProperty("defaultLane");
            laneWidthProperty = serializedObject.FindProperty("laneWidth");
            lanesProperty = serializedObject.FindProperty("lanes");

            laneFoldouts = new bool[lanesProperty.arraySize];
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

                // arc length qualities
                {
                    EditorGUI.BeginChangeCheck();
                    int _newQuality = EditorGUILayout.IntField(arcQualityLabel, levelComponent.GetLaneAt(0).ArcLengthQuality);

                    // changed -> assign it to every lane
                    if (EditorGUI.EndChangeCheck()) {
                        for (int i = 0; i < levelComponent.LaneCountReal; i++)
                            levelComponent.GetLaneAt(i).ArcLengthQuality = _newQuality;
                    }
                }
            } else { // we want to do all lanes ourselves
                EditorGUILayout.LabelField("Lanes", EditorStyles.boldLabel);
                for (int i = lanesProperty.arraySize - 1; i >= 0; i--) { // go backwards in case of removal
                    BezierSpline lane = (BezierSpline) lanesProperty.GetArrayElementAtIndex(i).objectReferenceValue;

                    // the header of the lane which can be folded out or not
                    EditorGUILayout.BeginHorizontal();
                    {
                        // "custom" foldout button
                        if (GUILayout.Button(laneFoldouts[i] ? "▼" : "►", invisBackgroundButtonStyle, GUILayout.MaxWidth(20f)))
                            laneFoldouts[i] = !laneFoldouts[i];

                        EditorGUILayout.PropertyField(lanesProperty.GetArrayElementAtIndex(i), new GUIContent(GetLaneName(i)));

                        // delete
                        if (GUILayout.Button("X", GUILayout.MaxWidth(20f))) {
                            lanesProperty.DeleteArrayElementAtIndex(i);
                            ArrayUtility.RemoveAt(ref laneFoldouts, i);

                            EditorGUILayout.EndHorizontal(); // end horizontal so it doesn't freak out
                            continue;
                        }

                    }
                    EditorGUILayout.EndHorizontal();

                    // folded out
                    if (laneFoldouts[i]) {
                        // indent the array attributes a bit in
                        EditorGUI.indentLevel = 1;
                        {
                            lane.ArcLengthQuality = EditorGUILayout.IntField(arcQualityLabel, lane.ArcLengthQuality);
                        }
                        EditorGUI.indentLevel = 0;
                    }
                }

                if (GUILayout.Button("Add lane")) {
                    int _whereToInsert;

                    if (lanesProperty.arraySize % 2 == 0 && lanesProperty.arraySize != 0) _whereToInsert = lanesProperty.arraySize - 1;
                    else _whereToInsert = 0;

                    lanesProperty.InsertArrayElementAtIndex(_whereToInsert);
                    ArrayUtility.Insert(ref laneFoldouts, _whereToInsert, false);
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
}