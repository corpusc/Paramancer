// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;

/// <summary>
/// Base emitte type class
/// </summary>
/// <remarks>
/// \sa \ref emtyperef
/// \sa \ref codeextendemtype
/// </remarks>
public class MBEmitterType : MBObject
{
    #region ### Inspector Fields ###
    /// <summary>
    /// Color used by EmitterType visualization
    /// </summary>
    public Color GizmoColor1 = new Color(1, 0, 0);
    /// <summary>
    /// Color used by EmitterType visualization;
    /// </summary>
    public Color GizmoColor2 = new Color(1, 0.5f, 0);
    /// <summary>
    /// Color used by EmitterType visualization;
    /// </summary>
    public Color GizmoColor3 = new Color(0, 0.75f, 0);
    /// <summary>
    /// The initial heading of emitted particles
    /// </summary>
    public MBEmitterTypeHeading Heading = MBEmitterTypeHeading.Center;
    /// <summary>
    /// Heading vector used when Heading is set to MBEmitterTypeHeading.Fixed
    /// </summary>
    public Vector3 FixedHeading;
    /// <summary>
    /// Whether Fixed takes emitter rotation into account or not
    /// </summary>
    public bool FixedHeadingIsGlobal = true;
    /// <summary>
    /// Whether the initial heading will be inversed
    /// </summary>
    public bool InverseHeading;
    /// <summary>
    /// Heading arc used for several heading modes
    /// </summary>
    public float HeadingArc = Mathf.PI;
    /// <summary>
    /// Whether the emitter type scales the x-axis to fit into viewport on play
    /// </summary>
    public bool FitScreenWidth;
    /// <summary>
    /// Whether the emitter type scales the y-axis to fit into viewport on play
    /// </summary>
    public bool FitScreenHeight;
    #endregion

    #region #### Public Fields & Properties ###

    /// <summary>
    /// Gets the Emitter this EmitterType belongs to
    /// </summary>
    public MBEmitter Emitter
    {
        get { return Parent as MBEmitter; }
    }

    /// <summary>
    /// Gets the associated EmitterTypeInfo attribute
    /// </summary>
    public MBEmitterTypeInfo EmitterTypeInfo
    {
        get
        {
            return (MBEmitterTypeInfo)System.Attribute.GetCustomAttribute(GetType(), typeof(MBEmitterTypeInfo));
        }
    }

    public override MBObject Parent
    {
        get
        {
            if (!mParent)
                mParent = GetComponent<MBEmitter>();
            return mParent;
        }
    }
    #endregion

    #region ### Unity Callbacks ###

    protected override void OnDrawGizmos()
    {
        if (Emitter && Emitter.ShouldDrawGizmos)
            DoGizmos();
    }

    protected override void OnDrawGizmosSelected()
    {
        if (Emitter && Emitter.ShouldDrawGizmosSelected)
            DoGizmos();
    }


    void OnDestroy()
    {
        if (Emitter)
            Emitter.SetEmitterType(null);
    }

    #endregion

    #region ### Public Methods and Properties ###

    /// <summary>
    /// Scales the emitter to fit the viewport dimensions using ParticleSystem's Camera
    /// </summary>
    /// <remarks>This is useful for 2D emitters like MBRectEmitter</remarks>
    public void ScaleToFitScreen()
    {
        ScaleToFitScreen(true, true);
    }
    /// <summary>
    /// Scales the emitter to fit the viewport dimensions using ParticleSystem's Camera
    /// </summary>
    /// <param name="scaleX">True to scale the x axis</param>
    /// <param name="scaleY">True to scale the y axis</param>
    public void ScaleToFitScreen(bool scaleX, bool scaleY)
    {
        Camera cam = ParticleSystem.Camera;
        if (cam && Application.isPlaying && (scaleX || scaleY)) {
            Vector3 bl = cam.ViewportToWorldPoint(new Vector3(0, 0, Transform.position.z - cam.transform.position.z));
            Vector3 tr = cam.ViewportToWorldPoint(new Vector3(1, 1, Transform.position.z - cam.transform.position.z));
            if (scaleX)
                Scale.x = Mathf.Abs(bl.x) + Mathf.Abs(tr.x);
            if (scaleY)
                Scale.y = Mathf.Abs(bl.y) + Mathf.Abs(tr.y);
        }
    }

    /// <summary>
    /// Resets to default values
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        Scale = Vector3.one;
    }

    #endregion

    #region ### Virtual Methods all EmitterTypes should override ###

    /// <summary>
    /// Gizmo drawing method, called by OnDrawGizmos and OnDrawGizmosSelected. Place your gizmo code here!
    /// </summary>
    /// <remarks>Gizmo-Visibility will be set by the user in the editor</remarks>
    protected virtual void DoGizmos()
    {
        // Use color3 to visualize heading
        Gizmos.color = GizmoColor3;
        Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, Scale);

        switch (Heading) {
            case MBEmitterTypeHeading.Random2D:
                if (InverseHeading)
                    MBUtility.DrawGizmoArc(Mathf.PI - HeadingArc, Mathf.PI + HeadingArc, 1.1f);
                else
                    MBUtility.DrawGizmoArc(-HeadingArc, HeadingArc, 1.1f);
                break;
        }
    }

    /// <summary>
    /// Gets the initial heading of the new particle
    /// </summary>
    /// <param name="PT">The new Particle with its position already set</param>
    /// <returns>the initial heading</returns>
    public virtual Vector3 GetHeading(MBParticle PT)
    {
        int dir = InverseHeading ? -1 : 1;
        Vector3 hdg;
        switch (Heading) {
            case MBEmitterTypeHeading.Center:
                hdg = (PT.Position - PT.Parent.Position).normalized * dir;
                PT.Orientation = -90 - Mathf.Atan2(-hdg.y, -hdg.x) * Mathf.Rad2Deg;
                return hdg;
            case MBEmitterTypeHeading.Random2D:
                float theta = Random.Range(-HeadingArc + Transform.eulerAngles.z * Mathf.Deg2Rad, HeadingArc + Transform.eulerAngles.z * Mathf.Deg2Rad);
                hdg = new Vector3(-Mathf.Sin(theta), Mathf.Cos(theta), 0) * dir;
                PT.Orientation = -90 - Mathf.Atan2(-hdg.y, -hdg.x) * Mathf.Rad2Deg;
                return hdg;
            //return Random.insideUnitCircle;
            case MBEmitterTypeHeading.Random3D:
                return Random.insideUnitSphere;
            case MBEmitterTypeHeading.User:
            case MBEmitterTypeHeading.MeshNormal:
                return Vector3.zero;
            case MBEmitterTypeHeading.TrailOrientation:
            case MBEmitterTypeHeading.TrailVelocity:
                return FixedHeading; // set by Trail emitter
            default:
                return (FixedHeadingIsGlobal) ? ParticleSystem.Transform.InverseTransformDirection(FixedHeading).normalized * dir :
                    ParticleSystem.Transform.InverseTransformDirection(Transform.TransformDirection(FixedHeading)).normalized * dir;
        }

    }

    /// <summary>
    /// Gets a point inside the shape of this Emittertype
    /// </summary>
    /// <param name="PT">The new particle. Usually this isn't needed</param>
    /// <returns>Particles spawnpoint in emitter space</returns>
    /// <remarks>This is called by the emitter to get a spawn point for a new particle</remarks>
    public virtual Vector3 GetPosition(MBParticle PT)
    {
        return Vector3.zero;
    }

    /// <summary>
    /// Called when the emitter starts playing
    /// </summary>
    /// <remarks>Override this to initialize an EmitterType</remarks>
    public virtual void OnPlay()
    {
        ScaleToFitScreen(FitScreenWidth, FitScreenHeight);
    }

    /// <summary>
    /// Validate/Check/Limit fields and properties here
    /// </summary>
    /// <remarks>This is called by the editor after values were changed by the user.</remarks>
    public virtual void Validate()
    {
    }

    #endregion

    #region ### Internal Publics. Don't use them unless you know what you're doing ###
    /*! @name Internal Public
     * Don't use them unless you know what you're doing
     */
    //@{
    public override void mbReloadHierarchy()
    {
    }
    #endregion
}

/// <summary>
/// Determines the way a particle's initial heading will be calculated
/// </summary>
public enum MBEmitterTypeHeading
{
    /// <summary>
    /// Set heading by direction from center to spawn-point
    /// </summary>
    Center = 0,
    /// <summary>
    /// Fixed heading
    /// </summary>
    Fixed = 1,
    /// <summary>
    /// Set heading by Particle Velocity (Trails only)
    /// </summary>
    TrailVelocity = 2,
    /// <summary>
    /// Set heading by Particle Orientation (Trails only)
    /// </summary>
    TrailOrientation = 3,
    /// <summary>
    /// Set heading to a random x/y direction
    /// </summary>
    Random2D = 4,
    /// <summary>
    /// Set heading to a random x/y/z direction
    /// </summary>
    Random3D = 5,
    /// <summary>
    /// Set heading by mesh normal
    /// </summary>
    /// <remarks>
    /// Parameter only used by mesh emitters</remarks>
    MeshNormal = 6,
    /// <summary>
    /// Heading needs to be set by the emitter
    /// </summary>
    User = 99
}

/// <summary>
/// Attribute class that defines editor-parameters for EmitterType's
/// </summary>
public class MBEmitterTypeInfo : System.Attribute
{
    /// <summary>
    /// Name of the EmitterType in the "Change EmitterType" menu
    /// </summary>
    public string Menu;

    public MBEmitterTypeInfo() : this("Add a MBEmitterTypeInfo attribute to this class!") { }
    public MBEmitterTypeInfo(string menu)
    {
        Menu = menu;
    }

}

