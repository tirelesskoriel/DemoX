namespace DemoX.Framework.Core
{
    public static class SpaceTools
    {
        public static float ConvertToSignAngle(float angle)
        {
            angle %= 360;
            if (angle > 180)
            {
                angle -= 360;
            }
            return angle;
        }
    }
}