namespace Bezier {
    /// <summary>
    /// Describes in what mode the bezier curve's point is in.
    /// </summary>
    public enum BezierControlPointMode {
        /// <summary>
        /// Both points can be moved independently. Useful for sharp edges
        /// </summary>
	    Free,
        /// <summary>
        /// Both point depend on each other. They are always mirrored on the point their distance from it
        /// is not equal.
        /// </summary>
	    Aligned,
        /// <summary>
        /// The points depend on each other. They are always mirrored on the point (even distance)
        /// </summary>
	    Mirrored
    }
}