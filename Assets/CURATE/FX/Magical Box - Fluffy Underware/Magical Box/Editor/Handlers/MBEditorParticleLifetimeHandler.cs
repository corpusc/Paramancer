// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleLifetime))]
public class MBEditorParticleLifetimeHandler : MBEditorParameterHandler
{
    public override void OnBirthGUI()
    {
        base.OnBirthGUI();
        MBParticleLifetime P = Target as MBParticleLifetime;

        EditorGUILayout.BeginHorizontal();
            P.Base = MBGUI.DoFloatField("Base", "", P.Base);
            P.RandomPercent = MBGUI.DoFloatSlider("Random %", "Random deviation from Base", P.RandomPercent, 0, 1);
        EditorGUILayout.EndHorizontal();

        if (P.AnimatedBirth) {
            EditorGUILayout.BeginHorizontal();
                P.Curve = MBGUI.DoCurve("Lifetime/Birth", "", P.Curve,P.ParentEmitter.Duration,P.ParentEmitter.Duration);
            EditorGUILayout.EndHorizontal();
        }
    }

}