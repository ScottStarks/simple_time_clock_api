using WorkingShiftActivity.Models.RequestModels;
using WorkingShiftActivity.Models.ResponseModels;

namespace WorkingShiftActivity.Repository
{
    public interface IAuthDataRepository<TEntity, TDto>
    {
        TEntity Get(string id);
        int Add(TEntity entity);
        Tokens Authenticate(TEntity entity);
    }
}
