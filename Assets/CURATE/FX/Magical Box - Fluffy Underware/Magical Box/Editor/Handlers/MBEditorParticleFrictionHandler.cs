// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleFriction))]
public class MBEditorParticleFrictionHandler : MBEditorParameterHandler
{
    public override void OnBirthGUI()
    {
        base.OnBirthGUI();
        MBParticleFriction P = Target as MBParticleFriction;

        EditorGUILayout.BeginHorizontal();
        P.Base = MBGUI.DoFloatSlider("Base", "Friction", P.Base,0,1);
        P.RandomPercent = MBGUI.DoFloatSlider("Random %", "Deviation from Base", P.RandomPercent, 0, 1);
        EditorGUILayout.EndHorizontal();
        if (P.AnimatedBirth) {
            EditorGUILayout.BeginHorizontal();
            P.CurveBase = MBGUI.DoCurve("Friction/Birth", "", P.CurveBase, P.Base, P.Base);
            EditorGUILayout.EndHorizontal();
        }
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleFriction P = Target as MBParticleFriction;
        EditorGUILayout.BeginHorizontal();
        P.CurveLife = MBGUI.DoCurve("Friction/Life", "", P.CurveLife, P.Base, P.Base);
        EditorGUILayout.EndHorizontal();
    }
}
