using Unity.Extensions;
using UnityEngine;

namespace Oversight.Core
{
    public enum HandMenuState
    {
        SignOut,
        General,
        PoseFineTurning,
        Clipping
    }

    public class HandMenuStateVisualizer : EnumStateVisualizer<HandMenuState>
    {
        public void GoToSignOut() => GoToView(HandMenuState.SignOut);
        public void GoToGeneral() => GoToView(HandMenuState.General);
        public void GoToPoseFineTurning() => GoToView(HandMenuState.PoseFineTurning);
        public void GoToClippingFineTurning() => GoToView(HandMenuState.Clipping);

        private void GoToView(HandMenuState state) => base.SetEnumValue(state);
    }
}
