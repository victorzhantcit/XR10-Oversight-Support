using System;
using Unity.Extensions;

namespace Oversight.Core
{
    [Serializable]
    public enum BuildingSystem
    {
        ALL,
        AR,
        AC,
        EE,
        WE,
        PS,
        FS,
        SHELL // will not select by user
    }

    public class FloorViewer : EnumStateVisualizer<BuildingSystem>
    {
        private new void Start()
        {
            // skipping base.Start()
        }

    }

}
