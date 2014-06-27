// =====================================================================
// Copyright 2011 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEditor;
using UnityEngine;

[MBParameterHandler(typeof(MBParticleCollider))]
public class MBEditorParticleColliderHandler : MBEditorParameterHandler
{
    public MBEditorParticleColliderHandler()
    {
        HideBirthGUI = true;
    }

    public override void OnLifetimeGUI()
    {
        base.OnLifetimeGUI();
        MBParticleCollider P = Target as MBParticleCollider;
        EditorGUILayout.BeginHorizontal();
            P.Bounce = MBGUI.DoFloatSlider("Bounce", "Bounciness", P.Bounce, 0, 1);
            P.BounceCombine = (PhysicMaterialCombine)MBGUI.DoEnumField("Bounce Combine", "How Bounciness of particles and colliders are combined", P.BounceCombine);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        P.RestBelowVelocity = MBGUI.DoFloatField("Rest below Velocity", "Set Velocity to zero under this value", P.RestBelowVelocity);
        EditorGUILayout.EndHorizontal();
        P.ParticleCollidesSM = MBGUI.DoEditorEvent("OnParticleCollides", "ParticleCollides Event Target", P.ParticleCollidesSM);
    }

}