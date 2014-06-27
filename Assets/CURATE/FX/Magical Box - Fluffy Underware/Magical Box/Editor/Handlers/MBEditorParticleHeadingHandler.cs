// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleHeading))]
public class MBEditorParticleHeadingHandler : MBEditorParameterHandler
{
    public MBEditorParticleHeadingHandler()
    {
        HideBirthGUI = true;
    }

    public override void  OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleHeading P = Target as MBParticleHeading;

        
        P.Relative = MBGUI.DoToggle("Relative", "", P.Relative);
        EditorGUILayout.BeginHorizontal();
        P.CurveLifeX = MBGUI.DoCurve("X/Life", "", P.CurveLifeX, 0, 0);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        P.CurveLifeY = MBGUI.DoCurve("Y/Life", "", P.CurveLifeY, 0, 0);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        P.CurveLifeZ = MBGUI.DoCurve("Z/Life", "", P.CurveLifeZ, 0, 0);
        EditorGUILayout.EndHorizontal();
        
    }
    
}