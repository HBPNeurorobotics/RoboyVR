namespace Locomotion
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LocomotionGhost : LocomotionHover
    {
        public LocomotionGhost() { }

        ~LocomotionGhost() { }

        public override void moveForward()
        {
            translateForward();
        }

        public override void stopMoving()
        {
            
        }
    } 
}
