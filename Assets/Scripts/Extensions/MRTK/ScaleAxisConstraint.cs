using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace MRTK.Extensions
{
    /// <summary>
    /// A constraint that restricts scaling to selected axes (X, Y, Z).
    /// </summary>
    public class ScaleAxisConstraint : TransformConstraint
    {
        /// <summary>
        /// Enum for selecting axis constraints.
        /// </summary>
        [System.Flags]
        public enum AxisFlags
        {
            None = 0,
            X = 1 << 0,
            Y = 1 << 1,
            Z = 1 << 2,
        }

        [Tooltip("Specifies which axes are allowed to scale.")]
        [SerializeField]
        private AxisFlags allowedAxes = AxisFlags.X;

        /// <summary>
        /// The axes that are allowed to scale.
        /// </summary>
        public AxisFlags AllowedAxes
        {
            get => allowedAxes;
            set => allowedAxes = value;
        }

        public override TransformFlags ConstraintType => TransformFlags.Scale;

        public override void OnManipulationStarted(MixedRealityTransform worldPose)
        {
            base.OnManipulationStarted(worldPose);
            WorldPoseOnManipulationStart = worldPose;
        }

        public override void ApplyConstraint(ref MixedRealityTransform transform)
        {
            Vector3 currentScale = transform.Scale;
            Vector3 lockedScale = WorldPoseOnManipulationStart.Scale;

            // Apply the allowed axes based on enum
            Vector3 constrainedScale = new Vector3(
                allowedAxes.HasFlag(AxisFlags.X) ? currentScale.x : lockedScale.x,
                allowedAxes.HasFlag(AxisFlags.Y) ? currentScale.y : lockedScale.y,
                allowedAxes.HasFlag(AxisFlags.Z) ? currentScale.z : lockedScale.z
            );

            transform.Scale = constrainedScale;
        }
    }
}
