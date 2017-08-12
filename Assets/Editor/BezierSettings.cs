using UnityEngine;

public class BezierSettings : ScriptableObject {
    
    public int StepsPerCurve = 10;
    public float DirectionScale = 1f;
    public float HandleSize = 0.1f;
    public float PickSize = 0.01f;
    public bool ShowSteps = true;
    public bool ShowPivots = false;
    public KeyCode HotkeyNewCurve = KeyCode.F2;

}
