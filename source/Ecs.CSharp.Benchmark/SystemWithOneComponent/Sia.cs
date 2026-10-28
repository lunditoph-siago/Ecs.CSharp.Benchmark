using BenchmarkDotNet.Attributes;
using Ecs.CSharp.Benchmark.Contexts;
using Sia;

namespace Ecs.CSharp.Benchmark
{
    public partial class SystemWithOneComponent
    {
        private sealed class SiaContext : SiaBaseContext
        {
            public SystemStage MonoThreadScheduler;
            public SystemStage MultiThreadScheduler;

            public override void Dispose() { MonoThreadScheduler.Dispose(); MultiThreadScheduler.Dispose(); base.Dispose(); }

            public sealed class MonoThreadUpdateSystem()
                : SystemBase(matcher: Matchers.Of<Component1>())
            {
                public override void Execute(WorldContext context, IEntityQuery query)
                {
                    query.ForSlice((ref Component1 component) => {
                        ++component.Value;
                    });
                }
            }

            public sealed class MultiThreadUpdateSystem()
                : SystemBase(matcher: Matchers.Of<Component1>())
            {
                public override void Execute(WorldContext context, IEntityQuery query)
                {
                    query.ForSliceOnParallel((ref Component1 component) => {
                        ++component.Value;
                    });
                }
            }

            public SiaContext(int entityCount, int entityPadding) : base()
            {
                for (int i = 0; i < entityCount; ++i)
                {
                    for (int j = 0; j < entityPadding; ++j)
                    {
                        World.Create(HList.From(0));
                    }

                    World.Create(HList.From(new Component1()));
                }

                MonoThreadScheduler = SystemChain.Empty.Add<MonoThreadUpdateSystem>().CreateStage(World);

                MultiThreadScheduler = SystemChain.Empty.Add<MultiThreadUpdateSystem>().CreateStage(World);
            }
        }

        [Context]
        private readonly SiaContext _sia;

        [BenchmarkCategory(Categories.Sia)]
        [Benchmark]
        public void Sia_MonoThread() => _sia.MonoThreadScheduler.Tick();

        [BenchmarkCategory(Categories.Sia)]
        [Benchmark]
        public void Sia_MultiThread() => _sia.MultiThreadScheduler.Tick();
    }
}
