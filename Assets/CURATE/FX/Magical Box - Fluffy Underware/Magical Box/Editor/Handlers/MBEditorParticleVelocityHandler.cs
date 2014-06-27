// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleVelocity))]
public class MBEditorParticleVelocityHandler : MBEditorParameterHandler
{
    public override void  OnBirthGUI()
    {
 	     base.OnBirthGUI();
         MBParticleVelocity P = Target as MBParticleVelocity;

         EditorGUILayout.BeginHorizontal();
         P.Base = MBGUI.DoFloatField("Base","", P.Base);
         P.RandomPercent = MBGUI.DoFloatSlider("Random %","Deviation from Base", P.RandomPercent, 0, 1);
         EditorGUILayout.EndHorizontal();
         if (P.AnimatedBirth) {
             EditorGUILayout.BeginHorizontal();
                P.CurveBase = MBGUI.DoCurve("Velocity/Birth", "", P.CurveBase, P.Base, P.Base);
             EditorGUILayout.EndHorizontal();
         }
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleVelocity P = Target as MBParticleVelocity;
        P.Relative = MBGUI.DoToggle("Relative", "Control increase or total Velocity?", P.Relative);
        EditorGUILayout.BeginHorizontal();
        if (P.Relative)
            P.CurveLife = MBGUI.DoCurve("Increase/Life", "", P.CurveLife, 0, 0);
        else
            P.CurveLife = MBGUI.DoCurve("Velocity/Life", "", P.CurveLife, P.Base, P.Base);
        EditorGUILayout.EndHorizontal();
    }
    
}

