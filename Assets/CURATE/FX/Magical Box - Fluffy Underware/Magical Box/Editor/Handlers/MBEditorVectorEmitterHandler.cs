// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;
using UnityEngine;

[MBEmitterTypeHandler(typeof(MBLineEmitter))]
public class MBEditorLineEmitterHandler : MBEditorEmitterTypeHandler
{
    bool pointsFolderOpen = true;
    protected bool isPolygon = false;

    public override void OnGUI()
    {
        base.OnGUI();
        MBLineEmitter E = Target as MBLineEmitter;
        E.Points = MBGUI.DoVector3Array("Points", E.Points, isPolygon, ref pointsFolderOpen);
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = !E.Emitter.IsTrail;
        E.EvenlySpread = MBGUI.DoToggle("Evenly Spread", "", E.EvenlySpread);
        if (E.EvenlySpread)
            E.DistributionPoints = MBGUI.DoIntField("Distribution", "Distribution Points", E.DistributionPoints);
        E.DistributeOverTotal = MBGUI.DoToggle("OverTotal", "Spread points over total length", E.DistributeOverTotal);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
        
    }

}

[MBEmitterTypeHandler(typeof(MBPolygonEmitter))]
public class MBEditorPolygonEmitterHandler : MBEditorLineEmitterHandler
{
    public MBEditorPolygonEmitterHandler()
    {
        isPolygon = true;
    }
}

