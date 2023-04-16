namespace SearchAThing.Sci;

public static partial class Ext
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NVector4 Transform(this NVector4 v, Matrix4x4 m) =>
        NVector4.Transform(v, m);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NVector4 Normalized(this NVector4 v) => NVector4.Normalize(v);

    /// <summary>
    /// convert given Vector4 to Vector3D ( discarding w )
    /// </summary>
    /// <param name="v">Vector4</param>
    /// <returns>Vector3D</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D ToVector3D(this NVector4 v) => new Vector3D(v.X, v.Y, v.Z);

    /// <summary>
    /// create vector3 from vector4 discarding w
    /// </summary>
    /// <param name="v">vector4 input</param>
    /// <returns>vector3</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NVector3 ToVector3(this NVector4 v) => new NVector3(v.X, v.Y, v.Z);

    /// <summary>
    /// create vector2 from vector4 discarding z, w
    /// </summary>
    /// <param name="v">vector4 input</param>
    /// <returns>vector2</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static NVector2 ToVector2(this NVector4 v) => new NVector2(v.X, v.Y);

    /// <summary>
    /// debug to console with optional prefix
    /// </summary>
    /// <param name="v">vector</param>
    /// <param name="prefix">optional prefix</param>
    /// <returns>vector</returns>
    public static NVector4 Debug(this NVector4 v, string prefix = "")
    {
        System.Diagnostics.Debug.WriteLine($"{(prefix.Length > 0 ? ($"{prefix}:") : "")}{v}");
        return v;
    }

}