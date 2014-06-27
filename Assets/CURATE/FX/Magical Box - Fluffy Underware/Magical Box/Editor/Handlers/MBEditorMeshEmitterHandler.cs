// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;
using UnityEngine;

[MBEmitterTypeHandler(typeof(MBMeshEdgeEmitter))]
public class MBEditorMeshEdgeEmitterHandler : MBEditorLineEmitterHandler
{
    public override void OnGUI()
    {
        BaseOnGUI();
        MBMeshEdgeEmitter E = Target as MBMeshEdgeEmitter;
        E.Mesh = MBGUI.DoObjectField("Mesh", "Mesh", E.Mesh, typeof(MeshFilter)) as MeshFilter;
        E.Scale = MBGUI.DoVector3Field("Scale", E.Scale);
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

[MBEmitterTypeHandler(typeof(MBMeshVertexEmitter))]
public class MBEditorMeshVertexEmitterHandler : MBEditorEmitterTypeHandler
{
    public override void OnGUI()
    {
        BaseOnGUI();
        MBMeshVertexEmitter E = Target as MBMeshVertexEmitter;
        E.Mesh = MBGUI.DoObjectField("Mesh", "Mesh", E.Mesh, typeof(MeshFilter)) as MeshFilter;
        E.Scale = MBGUI.DoVector3Field("Scale", E.Scale);
        
        GUI.enabled = !E.Emitter.IsTrail;
        E.Ordered = MBGUI.DoToggle("Ordered", "Use vertices in order", E.Ordered);
        GUI.enabled = true;
        
    }
}

[MBEmitterTypeHandler(typeof(MBMeshSurfaceEmitter))]
public class MBEditorMeshSurfaceEmitterHandler : MBEditorEmitterTypeHandler
{
    public override void OnGUI()
    {
        BaseOnGUI();
        MBMeshSurfaceEmitter E = Target as MBMeshSurfaceEmitter;
        E.Mesh = MBGUI.DoObjectField("Mesh", "Mesh", E.Mesh, typeof(MeshFilter)) as MeshFilter;
        E.Scale = MBGUI.DoVector3Field("Scale", E.Scale);
        E.UseCenter = MBGUI.DoToggle("UseCenter", "Use triangle center", E.UseCenter);
        GUI.enabled = !E.Emitter.IsTrail;
        E.Ordered = MBGUI.DoToggle("Ordered", "Use triangles in order", E.Ordered);
        GUI.enabled = true;
        
        
    }
}
