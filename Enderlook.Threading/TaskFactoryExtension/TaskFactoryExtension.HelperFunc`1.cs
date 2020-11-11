using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Threading
{
    public static partial class TaskFactoryExtension
    {
        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult})"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory source, Func<TResult> action)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Pack.Create(action));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory source, Func<TResult> action, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Pack.Create(action), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory source, Func<TResult> action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Pack.Create(action), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, TaskCreationOptions)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory source, Func<TResult> action, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Pack.Create(action), creationOptions);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult})"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory<TResult> source, Func<TResult> action)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Pack.Create(action));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory<TResult> source, Func<TResult> action, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Pack.Create(action), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory<TResult> source, Func<TResult> action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Pack.Create(action), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, TaskCreationOptions)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory<TResult> source, Func<TResult> action, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Pack.Create(action), creationOptions);

        private static class HelperFunc<TResult>
        {
            public static readonly Func<object, TResult> Basic = BasicMethod;

            private static TResult BasicMethod(object obj) => ((Pack)obj).Run();

            public sealed class Pack
            {
                private static ConcurrentBag<Pack> packs = new ConcurrentBag<Pack>();

                private Func<TResult> action;

                private Pack(Func<TResult> action) => this.action = action;

                public TResult Run()
                {
                    TResult result = action();
                    action = null;
                    packs.Add(this);
                    return result;
                }

                public static Pack Create(Func<TResult> action)
                {
                    if (packs.TryTake(out Pack pack))
                    {
                        pack.Set(action);
                        return pack;
                    }
                    return new Pack(action);
                }

                private void Set(Func<TResult> action) => this.action = action;
            }
        }
    }
}