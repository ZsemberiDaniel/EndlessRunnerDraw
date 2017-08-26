using UnityEngine;
using UnityEditor;
using System.IO;


namespace BezierEditor {
    [InitializeOnLoad]
    public class BezierSplineSettingsWindow : EditorWindow {

        private const string SettingsFilePath = "Assets/Bezier/Settings.json";
        public static BezierSettings Settings;

        // initialization
        static BezierSplineSettingsWindow() {
            Settings = CreateInstance<BezierSettings>();

            try {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(SettingsFilePath), Settings);
            } catch (DirectoryNotFoundException e) {
                CreateNewSettingsFile();
            }
        }

        [MenuItem("Tools/Bezier spline settings")]
        private static void OpenWindow() {
            GetWindow<BezierSplineSettingsWindow>();

            // load settings from file
            try {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(SettingsFilePath), Settings);
            } catch (FileNotFoundException e) {
                CreateNewSettingsFile();
            }
        }

        private void OnDisable() {
            // save settings to file
            File.WriteAllText(SettingsFilePath, JsonUtility.ToJson(Settings));
        }

        private static void CreateNewSettingsFile() {
            // create directory if doesn't exist
            FileInfo info = new FileInfo(SettingsFilePath);
            info.Directory.Create();

            // write to file
            File.WriteAllText(SettingsFilePath, JsonUtility.ToJson(Settings));
        }

        public void OnGUI() {
            EditorGUILayout.LabelField("Hotkeys", EditorStyles.boldLabel);
            Settings.HotkeyNewCurve = (KeyCode) EditorGUILayout.EnumPopup("Add new curve to spline", Settings.HotkeyNewCurve);

            EditorGUILayout.LabelField("Spline", EditorStyles.boldLabel);
            Settings.ShowSteps = EditorGUILayout.Toggle("Show steps", Settings.ShowSteps);
            Settings.ShowPivots = EditorGUILayout.Toggle("Show pivots", Settings.ShowPivots);

            Settings.StepsPerCurve = EditorGUILayout.IntField("Steps per curve", Settings.StepsPerCurve);
            Settings.DirectionScale = EditorGUILayout.Slider("Step scale", Settings.DirectionScale, 0.1f, 2f);
            Settings.HandleSize = EditorGUILayout.Slider("Handle size", Settings.HandleSize, 0.1f, 2f);
            Settings.PickSize = EditorGUILayout.Slider("Pick size", Settings.PickSize, 0.1f, 2f);
        }

    }
}