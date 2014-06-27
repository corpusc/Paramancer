// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleAcceleration))]
public class MBEditorParticleAccelerationHandler : MBEditorParameterHandler
{
    public override void OnBirthGUI()
    {
        base.OnBirthGUI();
        MBParticleAcceleration P = Target as MBParticleAcceleration;

        EditorGUILayout.BeginHorizontal();
        P.Base = MBGUI.DoFloatField("Base", "", P.Base);
        P.RandomPercent = MBGUI.DoFloatSlider("Random %", "Deviation from Base", P.RandomPercent, 0, 1);
        EditorGUILayout.EndHorizontal();
        if (P.AnimatedBirth) {
            EditorGUILayout.BeginHorizontal();
            P.CurveBase = MBGUI.DoCurve("Accel/Birth", "", P.CurveBase, P.Base, P.Base);
            EditorGUILayout.EndHorizontal();
        }
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleAcceleration P = Target as MBParticleAcceleration;
        EditorGUILayout.BeginHorizontal();
        P.CurveLife = MBGUI.DoCurve("Accel/Life", "", P.CurveLife, 0, 0);
        EditorGUILayout.EndHorizontal();
    }
}
