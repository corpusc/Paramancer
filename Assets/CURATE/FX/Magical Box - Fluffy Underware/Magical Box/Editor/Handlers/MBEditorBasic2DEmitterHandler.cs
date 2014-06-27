// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;

[MBEmitterTypeHandler(typeof(MBCircleEmitter))]
public class MBEditorCircleEmitterHandler : MBEditorEmitterTypeHandler
{
    public override void OnGUI()
    {
        base.OnGUI();
        MBCircleEmitter E=Target as MBCircleEmitter;
        EditorGUILayout.BeginHorizontal();
        E.Scale = MBGUI.DoVector2Field("Scale", E.Scale);
        E.Scale.z = 1;
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
            E.Hollow = MBGUI.DoFloatSlider("Hollow", "Cut out in percent",E.Hollow,0,1);
            E.Arc = Mathf.Deg2Rad*MBGUI.DoFloatSlider("Arc", "Limit circle", E.Arc*Mathf.Rad2Deg, 0, 180);
            
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = !E.Emitter.IsTrail;
        E.EvenlySpread = MBGUI.DoToggle("Evenly Spread", "", E.EvenlySpread);
        if (E.EvenlySpread)
            E.DistributionPoints = MBGUI.DoIntField("Distribution", "Distribution Points", E.DistributionPoints);
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

}

[MBEmitterTypeHandler(typeof(MBRectEmitter))]
public class MBEditorRectEmitterHandler : MBEditorEmitterTypeHandler
{
    public override void OnGUI()
    {
        base.OnGUI();
        MBRectEmitter E = Target as MBRectEmitter;
        EditorGUILayout.BeginHorizontal();
        E.Scale = MBGUI.DoVector2Field("Scale", E.Scale);
        E.Scale.z = 1;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
            GUI.enabled = !E.Emitter.IsTrail;
            E.EvenlySpread = MBGUI.DoToggle("Evenly Spread", "", E.EvenlySpread);
            if (E.EvenlySpread) {
                E.DistributionPointsX = MBGUI.DoIntField("Distribution X", "", E.DistributionPointsX);
                E.DistributionPointsY = MBGUI.DoIntField("Distribution Y", "", E.DistributionPointsY);
            }
            GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }
}

[MBEmitterTypeHandler(typeof(MBHollowRectEmitter))]
public class MBEditorHollowRectEmitterHandler : MBEditorEmitterTypeHandler
{
    public override void OnGUI()
    {
        base.OnGUI();
        MBHollowRectEmitter E = Target as MBHollowRectEmitter;
        EditorGUILayout.BeginHorizontal();
        E.Scale = MBGUI.DoVector2Field("Scale", E.Scale);
        E.Scale.z = 1;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        E.Hollow.x = MBGUI.DoFloatSlider("Hollow X", "Cut out in percent", E.Hollow.x, 0, 1);
        E.Hollow.y = MBGUI.DoFloatSlider("Hollow Y", "Cut out in percent", E.Hollow.y, 0, 1);
        EditorGUILayout.EndHorizontal();
    }

}

[MBEmitterTypeHandler(typeof(MBImageEmitter))]
public class MBEditorImageEmitterHandler : MBEditorEmitterTypeHandler
{
    bool KeepAspect=true;
    float aspect = 1;

    public override void OnGUI()
    {
        base.OnGUI();
        
        MBImageEmitter E = Target as MBImageEmitter;
        EditorGUILayout.BeginHorizontal();
            Vector2 v = new Vector2(E.Scale.x,E.Scale.y);
            E.Scale = MBGUI.DoVector2Field("Scale", E.Scale);
            if (KeepAspect)
                if (v.x != E.Scale.x)
                    E.Scale.y = E.Scale.x / aspect;
                else if (v.y != E.Scale.y)
                    E.Scale.x = E.Scale.y * aspect;
                    E.Scale.z = 1;
            Texture2D t = E.Image;

            E.Image = (Texture2D)EditorGUILayout.ObjectField("Image", E.Image, typeof(Texture2D),true);

            if (t != E.Image)
                MBEditorUtility.MakeTexturesReadable(new Texture2D[] { E.Image });
            if (E.Image)
                aspect=E.Image.width/(float)E.Image.height;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            E.MinAlpha = MBGUI.DoFloatSlider("Min Alpha", "Mimimum alpha level to allow spawning", E.MinAlpha,0,1);
            bool changed;
            KeepAspect = MBGUI.DoToggle("Keep Aspect", "Keep Aspect Ratio while scaling", KeepAspect,out changed);
            if (changed && KeepAspect)
                E.Scale.y=E.Scale.x/aspect;

        EditorGUILayout.EndHorizontal();
        
    }
}