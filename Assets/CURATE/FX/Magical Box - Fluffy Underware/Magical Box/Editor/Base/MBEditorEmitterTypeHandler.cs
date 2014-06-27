// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;


/// <summary>
/// Base MBEmitterType's GUI handler class
/// </summary>
public class MBEditorEmitterTypeHandler
{
    public MBEmitterType Target { get; set; }
    public MBEmitterTypeInfo EmitterTypeInfo { get; set; }

    /// <summary>
    /// If true, the Magical Box window will be repainted
    /// </summary>
    public bool NeedRepaint;

    public virtual void OnGUI()
    {
        BaseOnGUI();  
    }

    public void BaseOnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        Target.Heading = (MBEmitterTypeHeading)MBGUI.DoEnumField("Heading", "Initial Heading", Target.Heading);

        if (!Target.Emitter.IsTrail && Target.Heading == MBEmitterTypeHeading.TrailVelocity) {
            Target.Heading = MBEmitterTypeHeading.Center;
            EditorApplication.Beep();
            Debug.Log("Magical Box: Trail Velocity heading is only accessible to trail emitters!");
        }
        switch (Target.Heading) {
            case MBEmitterTypeHeading.Fixed:
                Target.FixedHeading = MBGUI.DoVector3Field("Fixed", Target.FixedHeading);
                Target.FixedHeadingIsGlobal = MBGUI.DoToggle("Global", "Turn off to use emitter's rotation", Target.FixedHeadingIsGlobal);
                break;
            case MBEmitterTypeHeading.Random2D:
                Target.HeadingArc = Mathf.Deg2Rad * MBGUI.DoFloatSlider("Arc", "Limit circle", Target.HeadingArc * Mathf.Rad2Deg, 0, 180);
                break;
        }
        Target.InverseHeading = MBGUI.DoToggle("Inverse", "Inverse heading?", Target.InverseHeading);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        Target.FitScreenWidth = MBGUI.DoToggle("Fit Width", "Scale width to fit screen", Target.FitScreenWidth);
        Target.FitScreenHeight = MBGUI.DoToggle("Fit Height", "Scale height to fit screen", Target.FitScreenHeight);
        EditorGUILayout.EndHorizontal();
    }

}


/// <summary>
/// Attribute class to identify a MBEmitterType's GUI handling class 
/// </summary>
public class MBEmitterTypeHandler : System.Attribute
{
    public System.Type EmitterType;

    public MBEmitterTypeHandler(System.Type emitterType)
    {
        EmitterType = emitterType;
    }
}
