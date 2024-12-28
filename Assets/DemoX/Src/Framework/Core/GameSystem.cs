using System;

namespace DemoX.Framework.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GameSystem : Attribute
    {
        public enum InstanceType
        {
            Create,
            Find
        }

        public InstanceType InsType { get; }

        public GameSystem(InstanceType instanceType = InstanceType.Create)
        {
            InsType = instanceType;
        }
    }
}