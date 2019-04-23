namespace Locomotion
{
    public static class LocomotionHandler
    {
        static ILocomotionBehaviour _locomotionBehaviour = new LocomotionTracker();

        public static void moveForward()
        {
            _locomotionBehaviour.moveForward();
        }
    } 
}
