using System;

namespace Fenrir.ECS.Operations
{
    internal struct AddArchetypeOperation
    {
        public Type[] ComponentTypes;

        public AddArchetypeOperation(Type[] componentTypes)
        {
            ComponentTypes = componentTypes;
        }
    }
}
