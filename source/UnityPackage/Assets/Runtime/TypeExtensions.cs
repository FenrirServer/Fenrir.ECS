using System;
using System.Linq;
using System.Reflection;

namespace Fenrir.ECS
{
    static class TypeExtensions
    {
        internal static bool IsUnmanagedStruct(this Type t)
        {
            return t.IsStruct() && t.IsUnmanaged();
        }

        internal static bool IsStruct(this Type t)
        {
            return t.IsValueType && !t.IsPrimitive;
        }

        internal static bool IsUnmanaged(this Type t)
        {
            if (t.IsPrimitive || t.IsPointer || t.IsEnum)
            {
                return true;
            }
            else if (t.IsValueType && t.IsGenericType)
            {
                var areGenericTypesAllBlittable = t.GenericTypeArguments.All(x => IsUnmanaged(x));

                if (areGenericTypesAllBlittable)
                {
                    return t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                              .All(x => IsUnmanaged(x.FieldType));
                }
                else
                {
                    return false;
                }
            }
            else if (t.IsValueType)
            {
                return t.GetFields(BindingFlags.Public |
                                     BindingFlags.NonPublic | BindingFlags.Instance)
                          .All(x => IsUnmanaged(x.FieldType));
            }
            else
            {
                return false;
            }
        }
    }
}
