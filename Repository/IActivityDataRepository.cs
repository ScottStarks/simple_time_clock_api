namespace WorkingShiftActivity.Repository
{
    public interface IActivityDataRepository<TEntity, TDto>
    {
        IList<TEntity> GetAll(string? id);
        TEntity Get(int id);
        void Add(TEntity entity);
        void Update(TEntity entity);
    }
}
