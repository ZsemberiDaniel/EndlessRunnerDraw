using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using OneDollar;

namespace OneDollarEditor {
    [CustomEditor(typeof(TemplateGroup))]
    class TemplateGroupEditor : Editor {

        private List<bool> foldOut = new List<bool>();
        private TemplateGroup group;

        private GUIStyle largeLabel;
        private GUIStyle shapeLabel;

        private void OnEnable() {
            group = target as TemplateGroup;

            // Styles
            InitStyles();
        }

        private void InitStyles() {
            largeLabel = new GUIStyle(EditorStyles.boldLabel);
            largeLabel.fontSize = 16;
            largeLabel.fixedHeight = 160;

            shapeLabel = new GUIStyle(EditorStyles.boldLabel);
            shapeLabel.fontSize = 14;
            shapeLabel.fixedHeight = 140;
        }

        public override void OnInspectorGUI() {
            if (largeLabel == null) InitStyles();

            // if we have more in the foldout list then take out until we have the same as template count
            if (foldOut.Count > group.ModifiableTemplates.Count) {
                foldOut = foldOut.TakeWhile((value, index) => index < group.ModifiableTemplates.Count).ToList();
                // if we have less int the foldout list add untul we have the same as template count
            } else if (foldOut.Count < group.ModifiableTemplates.Count) {
                int foldOutStartCount = foldOut.Count;

                for (int i = 0; i < group.ModifiableTemplates.Count - foldOutStartCount; i++)
                    foldOut.Add(false);
            }

            EditorGUILayout.LabelField("Templates", largeLabel);
            GUILayout.Space(10f);

            // draw templates
            for (int i = group.ModifiableTemplates.Count - 1; i >= 0; i--) {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(group.ModifiableTemplates[i].Name, shapeLabel);

                    if (GUILayout.Button("X", EditorStyles.miniButtonRight, GUILayout.MaxWidth(40f))) {
                        OnTemplateDeleted(i);
                        continue;
                    }
                }
                EditorGUILayout.EndHorizontal();

                foldOut[i] = EditorGUILayout.Foldout(foldOut[i], "Shape");

                if (foldOut[i]) {
                    // shape size
                    float shapeSize = EditorGUIUtility.currentViewWidth * 0.5f;
                    // where to draw the shape
                    Rect _rect = GUILayoutUtility.GetRect(shapeSize, shapeSize * 1.5f);

                    // Draw shape
                    GUI.DrawTexture(_rect, EditorGUIUtility.whiteTexture);
                    Handles.BeginGUI();
                    {
                        Handles.color = Color.red;
                        Handles.DrawAAPolyLine(5f, group.ModifiableTemplates[i].Points
                            .Select(point => new Vector3(point.x, point.y) * shapeSize + new Vector3(_rect.center.x, _rect.center.y))
                            .ToArray());
                    }
                    Handles.EndGUI();
                }

                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Draw new template")) {
                var window = EditorWindow.GetWindow<LearnTemplateEditorWindow>();
                window.SetTemplateGroup(group);

                window.OnTemplateAdded += OnTemplateAdded;
            }
        }

        private void OnTemplateDeleted(int index) {
            if (EditorUtility.DisplayDialog("Really?", "Are you sure you want to delete " + group.ModifiableTemplates[index].Name + "?", "Delete", "Cancel")) {
                group.ModifiableTemplates.RemoveAt(index);
                foldOut.RemoveAt(index);
            }
        }

        private void OnTemplateAdded(string templateName, List<Vector2> points) {
            group.Learn(templateName, points.ToArray());
            foldOut.Add(false);
        }
    }
}