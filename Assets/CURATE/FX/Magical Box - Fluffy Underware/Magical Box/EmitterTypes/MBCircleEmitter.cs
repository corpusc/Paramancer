// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Provides a circular emitter type
/// </summary>
[MBEmitterTypeInfo(Menu = "2D/Circle")]
public class MBCircleEmitter : MBEmitterType
{
    #region ### Inspector Fields ###
    /// <summary>
    /// Cut out area percentage
    /// </summary>
    public float Hollow;
    /// <summary>
    /// Limit angle
    /// </summary>
    public float Arc = Mathf.PI;
    /// <summary>
    /// True to spawn particles evenly spread along the arc/cirlce
    /// </summary>
    public bool EvenlySpread;
    /// <summary>
    /// Number of spawnpoints the arc/circle will be divided into when evenly spread
    /// </summary>
    public int DistributionPoints;

    #endregion

    int mDistribution;
    float mDistributionStep;

    public override Vector3 GetPosition(MBParticle PT)
    {
        float theta=0;//=PT.Parent.Transform.rotation.eulerAngles.z*Mathf.Deg2Rad;
        if (!EvenlySpread)
            theta = Random.Range(-Arc, Arc);
        else if (DistributionPoints > 0) {
            theta =  -Arc + mDistributionStep * mDistribution++;
            if (mDistribution >= DistributionPoints)
                mDistribution = 0;
        }

        float d = Random.Range(Hollow, 1);

        Vector3 v = new Vector3(d * -Mathf.Sin(theta),
                                d * Mathf.Cos(theta),0);
        
        v.Scale(Scale);
        return v;
    }

    public override void Validate()
    {
        base.Validate();
        Hollow = Mathf.Clamp01(Hollow);
        
        if (EvenlySpread && DistributionPoints>0)
            mDistributionStep = ((Arc * 2) / DistributionPoints);
        if (Emitter.IsTrail) {
            EvenlySpread = false;
            DistributionPoints = 0;
        }

    }

    public override void OnPlay()
    {
        base.OnPlay();
        //mDistribution = 0;
        if (EvenlySpread && DistributionPoints > 0)
            mDistributionStep = ((Arc * 2) / DistributionPoints);
    }

    public override void Reset()
    {
        base.Reset();
        Arc = Mathf.PI;
        Hollow = 0;
        EvenlySpread = false;
        DistributionPoints = 0;
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();
        Gizmos.color = GizmoColor1;
        Vector3 scale = MBUtility.Scale(Scale, Transform.lossyScale);
        Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, scale);
        MBUtility.DrawGizmoArc(-Arc, Arc);
        if (Hollow > 0) {
            Gizmos.color = GizmoColor2;
            Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, scale * Hollow);
            MBUtility.DrawGizmoArc(-Arc, Arc);
        }
    }

   
}