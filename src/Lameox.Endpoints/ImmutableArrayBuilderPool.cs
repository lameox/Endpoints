using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    public interface IArrayBuilderOwner<T> : IDisposable
    {
        ImmutableArray<T>.Builder Builder { get; }
    }

    internal class ImmutableArrayBuilderPool<T>
    {
        private readonly struct Reservation : IArrayBuilderOwner<T>
        {
            public ImmutableArray<T>.Builder Builder { get; }

            public Reservation(ImmutableArray<T>.Builder builder)
            {
                Builder = builder;
            }

            public void Dispose()
            {
                ReturnInstance(Builder);
            }
        }

        private static readonly ObjectPool<ImmutableArray<T>.Builder> _pool =
            new ObjectPool<ImmutableArray<T>.Builder>(() => ImmutableArray.CreateBuilder<T>());

        public static ImmutableArray<T>.Builder GetInstance()
        {
            var result = _pool.Allocate();
            return result;
        }

        public static void ReturnInstance(ImmutableArray<T>.Builder instance)
        {
            if (instance.Capacity > 128)
            {
                return;
            }

            instance.Clear();
            _pool.Free(instance);
        }

        public static IArrayBuilderOwner<T> Get()
        {
            var instance = GetInstance();
            return new Reservation(instance);
        }
    }
}
