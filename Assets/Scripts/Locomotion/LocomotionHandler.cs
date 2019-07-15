namespace Locomotion
{
    public enum LocomotionBehaviour
    {
        Hover,
        Tracker
    }

    public static class LocomotionHandler
    {
        private static ILocomotionBehaviour s_locomotionBehaviour = new LocomotionTracker();

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