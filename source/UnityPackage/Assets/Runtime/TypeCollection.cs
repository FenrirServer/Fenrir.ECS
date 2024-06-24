using System;
using System.Linq;

namespace Fenrir.ECS
{
    public class TypeCollection
    {
        public Type[] Types { get; private set; }

        public TypeCollection(params Type[] types)
        {
            Types = types;
            Array.Sort(Types, (t1, t2) => string.Compare(t1.FullName, t2.FullName)); // TODO Find faster method
        }

        public static TypeCollection Create<T1>() => new TypeCollection(new[] { typeof(T1) } );
        public static TypeCollection Create<T1, T2>() => new TypeCollection(new[] { typeof(T1), typeof(T2) });
        public static TypeCollection Create<T1, T2, T3>() => new TypeCollection(new[] { typeof(T1), typeof(T2), typeof(T3) });
        public static TypeCollection Create<T1, T2, T3, T4>() => new TypeCollection(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4) });
        public static TypeCollection Create<T1, T2, T3, T4, T5>() => new TypeCollection(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5) });
        public static TypeCollection Create<T1, T2, T3, T4, T5, T6>() => new TypeCollection(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6) });
        public static TypeCollection Create<T1, T2, T3, T4, T5, T6, T7>() => new TypeCollection(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7) });
        public static TypeCollection Create<T1, T2, T3, T4, T5, T6, T7, T8>() => new TypeCollection(new[] { typeof(T1), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8) });

        public override int GetHashCode()
        {
            int hashCode = 0;
            for(int i=0; i < Types.Length; i++)
            {
                hashCode ^= Types[i].GetHashCode();
            }
            return hashCode;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as TypeCollection);
        }

        public bool Equals(TypeCollection obj)
        {
            if (obj == null)
                return false;

            if (object.ReferenceEquals(obj, this))
                return true;

            if(obj.Types.Length != Types.Length) 
                return false;

            for(int i=0; i<Types.Length; i++)
            {
                if (Types[i] != obj.Types[i])
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Join(',', Types.Select(type => type.Name));
        }
    }
}
