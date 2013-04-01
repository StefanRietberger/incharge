using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InCharge.Input
{
    public enum InputState
    {
        NoInput, // transient state
        Free,
            Terraform,
                Dig,
                Dump,
            Build,
                SetFloorPlan,
                DefineRooms,
    }
}
