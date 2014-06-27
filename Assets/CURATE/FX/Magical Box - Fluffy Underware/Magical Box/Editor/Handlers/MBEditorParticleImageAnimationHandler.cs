// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;

[MBParameterHandler(typeof(MBParticleImageAnimation))]
public class MBEditorParticleImageAnimationHandler : MBEditorParameterHandler
{
    SerializedObject mImageObject;
    SerializedProperty mImageField;

    public override void  OnBirthGUI()
    {
        
 	     base.OnBirthGUI();
         MBParticleImageAnimation P = Target as MBParticleImageAnimation;
         EditorGUILayout.BeginHorizontal();
         P.StartFrame = MBGUI.DoIntField("Start Frame", "", P.StartFrame);
         EditorGUILayout.EndHorizontal();
         
         if (P.AnimatedBirth) {
             EditorGUILayout.BeginHorizontal();
             P.BirthAnimMode = (MBImageAnimationMode)MBGUI.DoEnumField("Mode", "Animation Mode", P.BirthAnimMode);
             P.BirthAnimTiming = (MBImageAnimationTimingMode)MBGUI.DoEnumField("Timing", "Timing mode", P.BirthAnimTiming);
             EditorGUILayout.EndHorizontal();
             EditorGUILayout.BeginHorizontal();
             P.BirthAnimSpeed = MBGUI.DoFloatField("Speed", "Animation speed", P.BirthAnimSpeed);
             P.BirthAnimDirection = (MBImageAnimationDirection)MBGUI.DoEnumField("Direction", "Animation direction", P.BirthAnimDirection);
             P.BirthAnimRepeat = (MBImageAnimationRepeat)MBGUI.DoEnumField("Repeat", "Animation repeat mode", P.BirthAnimRepeat);
             EditorGUILayout.EndHorizontal();
         }
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleImageAnimation P = Target as MBParticleImageAnimation;

        if (P.AnimatedLife) {
            EditorGUILayout.BeginHorizontal();
            P.LifetimeAnimMode = (MBImageAnimationMode)MBGUI.DoEnumField("Mode", "Animation Mode", P.LifetimeAnimMode);
            P.LifetimeAnimTiming = (MBImageAnimationTimingMode)MBGUI.DoEnumField("Timing", "Timing mode", P.LifetimeAnimTiming);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            P.LifetimeAnimSpeed = MBGUI.DoFloatField("Speed", "Animation speed", P.LifetimeAnimSpeed);
            P.LifetimeAnimDirection = (MBImageAnimationDirection)MBGUI.DoEnumField("Direction", "Animation direction", P.LifetimeAnimDirection);
            P.LifetimeAnimRepeat = (MBImageAnimationRepeat)MBGUI.DoEnumField("Repeat", "Animation repeat mode", P.LifetimeAnimRepeat);
            EditorGUILayout.EndHorizontal();
        }
    }
    
}