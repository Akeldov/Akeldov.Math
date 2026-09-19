namespace Akeldov.Math.Spatial3D
{
    /// <summary>
    /// Represents an object with a three-dimensional position.
    /// </summary>
    public interface IHasPosition3D
    {
        /// <summary>
        /// Gets the object's position.
        /// </summary>
        PointXYZ Position { get; }
    }
}
