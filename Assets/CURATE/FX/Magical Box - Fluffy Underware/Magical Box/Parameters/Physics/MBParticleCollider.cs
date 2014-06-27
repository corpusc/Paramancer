// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// World Collider parameter
/// </summary>
/// <remarks>
/// See also: \ref paramcollider "World Collider parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Physics/Collider",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Mandatory)]
public class MBParticleCollider : MBParameter
{
    /// <summary>
    /// The layers to check collision against
    /// </summary>
    public LayerMask Layers;
    /// <summary>
    /// Bounciness of particles
    /// </summary>
    public float Bounce;
    /// <summary>
    /// How Bounciness of particles and colliders are combined
    /// </summary>
    public PhysicMaterialCombine BounceCombine = PhysicMaterialCombine.Average;
    public float RestBelowVelocity = 0.01f;

    /// <summary>
    /// This event is called when a particle collides
    /// </summary>
    public event MBEventHandler ParticleCollides;
    /// <summary>
    /// SendMessage like event linking
    /// </summary>
    /// <remarks>See \ref events</remarks>
    public MBSendMessageTarget ParticleCollidesSM;

    RaycastHit mHitInfo;

    public override bool OnLifetime(MBParticle PT)
    {
        float mag = PT.Velocity.magnitude;
        if (mag == 0) return true;

        float radius = PT.Scale.x * 0.5f;
        float d = Mathf.Max(mag * PT.DeltaTime, radius);

        if (Physics.Raycast(PT.WorldPosition, PT.Velocity, out mHitInfo, d, -1)) {
            Vector3 refl = Vector3.Reflect(PT.Velocity, mHitInfo.normal);

            float colbounce = mHitInfo.collider.material.bounciness;
            switch (BounceCombine) {
                case PhysicMaterialCombine.Average: colbounce = (colbounce + Bounce) / 2.0f; break;
                case PhysicMaterialCombine.Maximum: colbounce = Mathf.Max(colbounce, Bounce); break;
                case PhysicMaterialCombine.Minimum: colbounce = Mathf.Min(colbounce, Bounce); break;
                case PhysicMaterialCombine.Multiply: colbounce = colbounce * Bounce; break;
            }

            PT.Velocity = refl * colbounce;
                if (mag < d) {
                    if (mag >= RestBelowVelocity) 
                        PT.WorldPosition = mHitInfo.point + refl.normalized * d;
                    else
                        PT.Velocity = Vector3.zero;
                }
            
            OnParticleInsideZone(PT);
        }
        

        return true;
    }

    protected void OnParticleInsideZone(MBParticle PT)
    {
        if (ParticleCollides != null)
            ParticleCollides(new MBEvent(MBEventType.ParameterCallback, this, PT, this));
        else if (ParticleCollidesSM != null && ParticleCollidesSM.Target && !string.IsNullOrEmpty(ParticleCollidesSM.MethodName))
            ParticleCollidesSM.Invoke(new MBEvent(MBEventType.ParameterCallback, this, PT, this));
    }

    public override void Reset()
    {
        base.Reset();
        Layers = -1;
        Order = 200;
    }

  
    public override void Purge()
    {
        base.Purge();
    }

}
