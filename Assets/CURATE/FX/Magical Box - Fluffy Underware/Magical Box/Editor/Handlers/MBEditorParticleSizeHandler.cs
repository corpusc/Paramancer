// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;
using UnityEditor;

[MBParameterHandler(typeof(MBParticleSize))]
public class MBEditorParticleSizeHandler : MBEditorParameterHandler
{
    public override void  OnBirthGUI()
    {
 	     base.OnBirthGUI();
         MBParticleSize P = Target as MBParticleSize;
         
         EditorGUILayout.BeginHorizontal();
         P.ConstrainAxis = MBGUI.DoToggle("Constrain Axis","Use quadrativ particles?", P.ConstrainAxis);
         P.DieOnZero = MBGUI.DoToggle("Die on Size","Die when size<=0 ?", P.DieOnZero);
         EditorGUILayout.EndHorizontal();
         EditorGUILayout.BeginHorizontal();
         if (P.ConstrainAxis) {
             float ini = MBGUI.DoFloatField("Base", "",P.Base.x);
             P.Base = new Vector2(ini, ini);
         }
         else
             P.Base = MBGUI.DoVector2Field("Base", P.Base);

         P.RandomPercent = MBGUI.DoFloatSlider("Random %","Random deviation from Base", P.RandomPercent, 0, 1);
         EditorGUILayout.EndHorizontal();

         if (P.AnimatedBirth) {
             // Curve X
             EditorGUILayout.BeginHorizontal();
                 P.CurveBirthX = MBGUI.DoCurve("Size/Birth", "X-Axis", P.CurveBirthX,P.Base.x,P.Base.x);
             EditorGUILayout.EndHorizontal();
             // Curve Y
             if (!P.ConstrainAxis) {
                 EditorGUILayout.BeginHorizontal();
                     P.CurveBirthY = MBGUI.DoCurve("Size/Birth", "Y-Axis", P.CurveBirthY,P.Base.y,P.Base.y);
                 EditorGUILayout.EndHorizontal();
             }
         }
         
        
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleSize P = Target as MBParticleSize;
        P.Relative = MBGUI.DoToggle("Relative", "Control Growth or total size?", P.Relative);
        // Curve X
        EditorGUILayout.BeginHorizontal();
        if (P.Relative)
            P.CurveLifeX = MBGUI.DoCurve("Growth/Life", "X-Axis", P.CurveLifeX,0,0);
        else
            P.CurveLifeX = MBGUI.DoCurve("Size/Life", "X-Axis", P.CurveLifeX, P.Base.x, P.Base.x);
        EditorGUILayout.EndHorizontal();
        // Curve Y
        if (!P.ConstrainAxis) {
            EditorGUILayout.BeginHorizontal();
            if (P.Relative)
                P.CurveLifeY = MBGUI.DoCurve("Growth/Life", "Y-Axis", P.CurveLifeY,0,0);
            else
                P.CurveLifeY = MBGUI.DoCurve("Size/Life", "Y-Axis", P.CurveLifeY, P.Base.y, P.Base.y);
            EditorGUILayout.EndHorizontal();
        }
        
    }
}