using DomoTroller2.ESFramework.Common.Base;

namespace DomoTroller2.ESFramework.Common.Interfaces
{
    public interface IRepository<T> where T : IAggregate, new()
    {
        T GetById(CompositeAggregateId aggregateId);
    }
}
