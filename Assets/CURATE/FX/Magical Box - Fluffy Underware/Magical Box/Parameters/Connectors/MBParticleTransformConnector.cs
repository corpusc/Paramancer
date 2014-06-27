// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Mesh parameter
/// </summary>
/// <remarks>
/// See also: \ref paramtransform "Transform parameter reference"
/// </remarks>
[MBParameterInfo(Menu = "Connector/Transform",
                 CanAnimateBirth = MBParameterAnimationMode.None,
                 CanAnimateLife = MBParameterAnimationMode.Mandatory)]
public class MBParticleTransformConnector : MBParameter
{
    public Transform Source;
    public bool SyncPosition;
    public MBParticleTransformConnectorRotationMode SyncRotation;
    public bool SyncSize;

    int mGameObjectSlotID;

    /// <summary>
    /// Override this to use your favorite pooling solution
    /// </summary>
    /// <returns>a cloned Transform</returns>
    public virtual Transform SpawnObject()
    {
            return (Source) ? GameObject.Instantiate(Source) as Transform : null;
    }

    /// <summary>
    /// Override this to use your favorite pooling solution
    /// </summary>
    /// <param name="obj"></param>
    public virtual void DespawnObject(Transform obj)
    {
        DestroyImmediate(obj.gameObject);
    }

    public override void OnPlay()
    {
        base.OnPlay();
        mGameObjectSlotID = ParentEmitter.RegisterParticleUserData("Transform"+GetInstanceID());
        ParentEmitter.ParticleDeath -= ParentEmitter_ParticleDeath;
        ParentEmitter.ParticleDeath += new MBEventHandler(ParentEmitter_ParticleDeath);
    }

    void ParentEmitter_ParticleDeath(MBEvent e)
    {
        if (e.Particle.HasUserData(mGameObjectSlotID)) {
            Transform ptobj = (Transform)e.Particle.UserData[mGameObjectSlotID];
            if (ptobj)
                DespawnObject(ptobj);
        }
    }

    public override void OnBirth(MBParticle PT)
    {
        if (PT.HasUserData(mGameObjectSlotID)) {
            if (Source) {
                Transform ptobj = SpawnObject();
                ptobj.parent = Source.parent;
#if UNITY_4_0
                ptobj.gameObject.SetActive(true);
#else
                ptobj.gameObject.active = true;
#endif
                PT.UserData[mGameObjectSlotID] = ptobj;
            }
        }
    }

    public override bool OnLifetime(MBParticle PT)
    {
        if (PT.HasUserDataValue(mGameObjectSlotID)) {
            Transform ptobj = PT.UserData[mGameObjectSlotID] as Transform;
            if (ptobj) {
                if (SyncPosition)
                    ptobj.position = ParticleSystem.Transform.TransformPoint(PT.Position);
                if (SyncSize)
                    ptobj.localScale = PT.Scale;

                switch (SyncRotation) {
                    case MBParticleTransformConnectorRotationMode.None: break;
                    case MBParticleTransformConnectorRotationMode.ByVelocity:
                        if (PT.Velocity != Vector3.zero)
                            ptobj.rotation = Quaternion.LookRotation(PT.Velocity);
                        break;
                    case MBParticleTransformConnectorRotationMode.ByHeading:
                        if (PT.Heading != Vector3.zero)
                            ptobj.rotation = Quaternion.LookRotation(PT.Heading);
                        break;
                }


            }
        }
        return true;
    }

    public override void Reset()
    {
        base.Reset();
        Order = 140;
        SyncPosition = true;
        SyncSize = true;
        Source = null;
        SyncRotation = MBParticleTransformConnectorRotationMode.ByVelocity;
    }

    public override void Purge()
    {
    }

}
/// <summary>
/// Defines rotation synchronization mode for connector parameters
/// </summary>
public enum MBParticleTransformConnectorRotationMode
{
    /// <summary>
    /// Don't synchronize rotation
    /// </summary>
    None = 0,
    /// <summary>
    /// Synchronize rotation with particle's velocity
    /// </summary>
    ByVelocity = 1,
    /// <summary>
    /// Synchronize rotation with particle's heading
    /// </summary>
    ByHeading = 2
}
