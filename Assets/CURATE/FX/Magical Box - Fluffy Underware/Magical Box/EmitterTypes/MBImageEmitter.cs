// =====================================================================
// Copyright 2011-2013 FluffyUnderware
// All rights reserved
// =====================================================================
using UnityEngine;


/// <summary>
/// Provides an image map based emitter type
/// </summary>
[MBEmitterTypeInfo(Menu = "2D/Image")]
public class MBImageEmitter : MBEmitterType
{
    #region ### Inspector Fields ###
    /// <summary>
    /// The image used as mask
    /// </summary>
    public Texture2D Image;
    /// <summary>
    /// Pixels need at least this alpha value to qualify as spawn point
    /// </summary>
    public float MinAlpha = 0.1f;
    #endregion

    public override Vector3 GetPosition(MBParticle PT)
    {
        if (Image) {
            int x, y;
            int trials = 100;

            while (trials-- > 0) {
                x=Random.Range(0, Image.width - 1);
                y=Random.Range(0, Image.height - 1);
                Color col = Image.GetPixel(x,y);
                if (col.a > MinAlpha) {
                    PT.Color = col;
                    x -= Image.width / 2;
                    y -= Image.height / 2;
                    return new Vector3(x*(Scale.x/Image.width),y*(Scale.y/Image.height),0);
                }
            }
        }
        return Vector3.zero;
    }

    protected override void DoGizmos()
    {
        base.DoGizmos();
        if (Image) {
            Gizmos.color = GizmoColor1;
            Vector3 scale = MBUtility.Scale(Scale, Transform.lossyScale);
            Gizmos.matrix = Matrix4x4.TRS(Transform.position, Transform.rotation, scale);
            MBUtility.DrawGizmoRect(new Rect(-0.5f,-0.5f,1,1));
        }
    }


}