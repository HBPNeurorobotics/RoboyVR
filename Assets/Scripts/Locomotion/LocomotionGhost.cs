namespace Locomotion
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class LocomotionGhost : LocomotionHover
    {
        protected override void initializeHover()
        {
        }

        ~LocomotionGhost() { }

        public override void moveForward()
        {
            translateForwardController();
        }

        public override void stopMoving()
        {
            
        }

        public override void reset()
        {
        }
    } 
}
