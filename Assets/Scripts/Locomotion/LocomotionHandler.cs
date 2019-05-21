using System;

namespace Locomotion
{
    public enum LocomotionBehaviour
    {
        hover,
        tracker,
        ghost,
        behaviourCount
    }

    public static class LocomotionHandler
    {
        static ILocomotionBehaviour s_locomotionBehaviour = new LocomotionHover();

        public static void moveForward()
        {
            s_locomotionBehaviour.moveForward();
        }

        public static void stopMoving()
        {
            s_locomotionBehaviour.stopMoving();
        }

        public static void changeLocomotionBehaviour(ILocomotionBehaviour newLocomotionBehaviour)
        {
            s_locomotionBehaviour.reset();
            s_locomotionBehaviour = newLocomotionBehaviour;
        }
    } 
}
