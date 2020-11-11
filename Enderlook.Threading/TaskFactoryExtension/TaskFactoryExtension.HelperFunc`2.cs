using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Threading
{
    public static partial class TaskFactoryExtension
    {
        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> action, TState state)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Pack.Create(action, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> action, TState state, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Pack.Create(action, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> action, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Pack.Create(action, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> action, TState state, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Pack.Create(action, state), creationOptions);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> action, TState state)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Pack.Create(action, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> action, TState state, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Pack.Create(action, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> action, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Pack.Create(action, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> action, TState state, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Pack.Create(action, state), creationOptions);

        private static class HelperFunc<TState, TResult>
        {
            public static readonly Func<object, TResult> Basic = BasicMethod;

            private static TResult BasicMethod(object obj) => ((Pack)obj).Run();

            public sealed class Pack
            {
                private static ConcurrentBag<Pack> packs = new ConcurrentBag<Pack>();

                private Func<TState, TResult> action;
                private TState state;

                private Pack(Func<TState, TResult> action, TState state)
                {
                    this.action = action;
                    this.state = state;
                }

                public TResult Run()
                {
                    TResult result = action(state);
                    action = null;
                    state = default;
                    packs.Add(this);
                    return result;
                }

                public static Pack Create(Func<TState, TResult> action, TState state)
                {
                    if (packs.TryTake(out Pack pack))
                    {
                        pack.Set(action, state);
                        return pack;
                    }
                    return new Pack(action, state);
                }

                private void Set(Func<TState, TResult> action, TState state)
                {
                    this.action = action;
                    this.state = state;
                }
            }
        }
    }
}