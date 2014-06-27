// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;
using UnityEngine;

[MBEmitterTypeHandler(typeof(MBSphereEmitter))]
public class MBEditorSphereEmitterHandler : MBEditorEmitterTypeHandler
{
    public override void OnGUI()
    {
        base.OnGUI();
        MBSphereEmitter E = Target as MBSphereEmitter;
        E.Scale = MBGUI.DoVector3Field("Scale", E.Scale);
        EditorGUILayout.BeginHorizontal();
        E.Hollow = MBGUI.DoFloatSlider("Hollow", "Cut out in percent", E.Hollow, 0, 1);
        E.Arc = MBGUI.DoFloatSlider("Arc", "Latitude cutout",E.Arc, 0, 1);
        EditorGUILayout.EndHorizontal();
    }
    
}


[MBEmitterTypeHandler(typeof(MBBoxEmitter))]
public class MBEditorBoxEmitterHandler : MBEditorEmitterTypeHandler
{
    public override void OnGUI()
    {
        base.OnGUI();
        MBBoxEmitter E = Target as MBBoxEmitter;
        E.Scale = MBGUI.DoVector3Field("Scale", E.Scale);
        //E.Hollow = MBGUI.DoFloatSlider("Hollow", "Cut out in percent", E.Hollow, 0, 1);
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = !E.Emitter.IsTrail;
        E.EvenlySpread = MBGUI.DoToggle("Evenly Spread", "", E.EvenlySpread);
        if (E.EvenlySpread) {
            E.DistributionPointsX = MBGUI.DoIntField("Distribution X", "", E.DistributionPointsX);
            E.DistributionPointsY = MBGUI.DoIntField("Distribution Y", "", E.DistributionPointsY);
            E.DistributionPointsZ = MBGUI.DoIntField("Distribution Z", "", E.DistributionPointsZ);
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        
    }
}

[MBEmitterTypeHandler(typeof(MBHollowBoxEmitter))]
public class MBEditorHollowBoxEmitterHandler : MBEditorEmitterTypeHandler
{
    public override void OnGUI()
    {
        base.OnGUI();
        MBHollowBoxEmitter E = Target as MBHollowBoxEmitter;
        E.Scale = MBGUI.DoVector3Field("Scale", E.Scale);
        EditorGUILayout.BeginHorizontal();
        E.Hollow.x = MBGUI.DoFloatSlider("Hollow X", "Cut out in percent", E.Hollow.x, 0, 1);
        E.Hollow.y = MBGUI.DoFloatSlider("Hollow Y", "Cut out in percent", E.Hollow.y, 0, 1);
        E.Hollow.z = MBGUI.DoFloatSlider("Hollow Z", "Cut out in percent", E.Hollow.z, 0, 1);
        EditorGUILayout.EndHorizontal();
    }

}

