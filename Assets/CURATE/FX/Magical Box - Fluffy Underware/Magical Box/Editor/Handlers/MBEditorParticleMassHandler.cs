// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleMass))]
public class MBEditorParticleMassHandler : MBEditorParameterHandler
{
    public override void  OnBirthGUI()
    {
 	     base.OnBirthGUI();
         MBParticleMass P = Target as MBParticleMass;
         EditorGUILayout.BeginHorizontal();
         P.Base = MBGUI.DoFloatField("Base", "", P.Base);
         P.RandomPercent = MBGUI.DoFloatSlider("Random %", "Random deviation from Base", P.RandomPercent, 0, 1);
         EditorGUILayout.EndHorizontal();

         if (P.AnimatedBirth) {
             EditorGUILayout.BeginHorizontal();
                P.CurveBirth = MBGUI.DoCurve("Mass/Birth", "", P.CurveBirth, P.Base, P.Base);
             EditorGUILayout.EndHorizontal();
         }
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleMass P = Target as MBParticleMass;

        EditorGUILayout.BeginHorizontal();
        P.CurveLife = MBGUI.DoCurve("Growth/Life", "", P.CurveLife, P.Base, P.Base);
        EditorGUILayout.EndHorizontal();
    }
    
}