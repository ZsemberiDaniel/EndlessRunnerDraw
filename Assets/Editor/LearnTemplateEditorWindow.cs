using OneDollar;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OneDollarEditor {
    public class LearnTemplateEditorWindow : EditorWindow {

        private Color backgroundColor = Color.white;
        private Texture2D backgroundTex;

        private TemplateGroup templateGroup;

        private string shapeName = "";
        private List<Vector3> points;

        internal delegate void TemplateAdd(string templateName, List<Vector2> points);
        internal event TemplateAdd OnTemplateAdded;

        private void OnEnable() {
            points = new List<Vector3>();

            backgroundTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            backgroundTex.SetPixel(1, 1, backgroundColor);
            backgroundTex.Apply();
        }

        private void OnGUI() {
            Event _currEvent = Event.current;

            // Draw background
            GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), backgroundTex);

            shapeName = EditorGUILayout.TextField("Template name ", shapeName);

            // Save button
            if (GUILayout.Button("Save")) {
                // Error handling
                if (string.IsNullOrEmpty(shapeName)) {
                    EditorUtility.DisplayDialog("Cannot save", "Couldn't save the template because name is empty!", "Ok");

                    return;
                } else if (points.Count == 0) {
                    EditorUtility.DisplayDialog("Cannot save", "Couldn't save the template because there is no shape!", "Ok");

                    return;
                } else if (templateGroup != null && templateGroup.ContainsName(shapeName)) {
                    EditorUtility.DisplayDialog("Cannot save", "Couldn't save the template because the name is already taken in the template group", "Ok");

                    return;
                }

                // we can safely save the template                                             here we need to subtract from height to mirror y axis
                OnTemplateAdded?.Invoke(shapeName, points.Select(point => new Vector2(point.x, position.height - point.y)).ToList());
                EditorUtility.DisplayDialog("Saved", "Template saved!", "Ok");

                _currEvent.Use();
            }

            // Mouse pressed
            switch (_currEvent.type) {
                case EventType.MouseDown:
                    points.Clear();
                    points.Add(_currEvent.mousePosition);

                    _currEvent.Use();
                    break;
                case EventType.MouseDrag:
                    points.Add(_currEvent.mousePosition);

                    _currEvent.Use();
                    break;
            }

            // Draw the line
            Handles.BeginGUI();
            {
                Handles.color = Color.red;
                Handles.DrawAAPolyLine(4f, points.ToArray());
            }
            Handles.EndGUI();
            HandleUtility.Repaint();
        }

        public void SetTemplateGroup(TemplateGroup templateGroup) {
            this.templateGroup = templateGroup;
        }
    }
}