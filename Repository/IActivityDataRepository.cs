namespace WorkingShiftActivity.Repository
{
    public interface IActivityDataRepository<TEntity, TDto>
    {
        IEnumerable<TEntity> GetAll(string id);
        TEntity Get(int id);
        void Add(TEntity entity);
        void Update(TEntity entity);
    }
}
