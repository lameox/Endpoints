using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    internal static partial class Binder<TRequest>
    {
        private ref struct FailureCollection
        {
            private static readonly ObjectPool<ImmutableArray<BindingFailure>.Builder> s_pool =
                new ObjectPool<ImmutableArray<BindingFailure>.Builder>(() => ImmutableArray.CreateBuilder<BindingFailure>());

            private ImmutableArray<BindingFailure>.Builder? _builder;

            public void Add(BindingFailure failure)
            {
                if (_builder is null)
                {
                    _builder = s_pool.Allocate();
                    _builder.Clear();
                }

                _builder.Add(failure);
            }

            public ImmutableArray<BindingFailure> ToImmutable()
            {
                if (_builder is null)
                {
                    return ImmutableArray<BindingFailure>.Empty;
                }

                var result = _builder.ToImmutable();

                _builder.Clear();
                s_pool.Free(_builder);

                return result;
            }
        }
    }
}
