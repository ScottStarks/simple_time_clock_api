using Microsoft.EntityFrameworkCore;
using WorkingShiftActivity.Context;
using WorkingShiftActivity.Context.Models;
using WorkingShiftActivity.Repository;

namespace WorkingShiftActivity.DataManager
{
    public class ActivityDataManager : IActivityDataRepository<Activity, ActivityDto>
    {
        private WorkingShiftContext _workingShiftDbContext { get; set; }
        public ActivityDataManager(WorkingShiftContext context)
        {
            _workingShiftDbContext = context;
        }
        public void Add(Activity entity)
        {
            _workingShiftDbContext.Activities.Add(entity);
            _workingShiftDbContext.SaveChanges();
        }

        public Activity Get(int id)
        {
            return _workingShiftDbContext.Activities.FirstOrDefault(act => act.Id == id);
        }

        public IList<Activity> GetAll(string? employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
            {
                return _workingShiftDbContext.Activities.Include(acv => acv.Employee).ToList();
            }
            return _workingShiftDbContext.Activities.Where(act => act.EmployeeId == employeeId).ToList();
        }

        public void Update(Activity entity)
        {
            var entityToUpdate = _workingShiftDbContext.Activities.Single(b => b.Id == entity.Id);
            entityToUpdate.WorkShiftEndTime = entity.WorkShiftEndTime;
            entityToUpdate.LunchStartTime = entity.LunchStartTime;
            entityToUpdate.LunchEndTime = entity.LunchEndTime;
            entityToUpdate.BreakStartTime = entity.BreakStartTime;
            entityToUpdate.BreakEndTime = entity.BreakEndTime;
            entityToUpdate.IsLunchActive = entity.IsLunchActive;
            entityToUpdate.IsShiftActive = entity.IsShiftActive;
            entityToUpdate.IsBreakActive = entityToUpdate.IsBreakActive;

            _workingShiftDbContext.SaveChanges();
        }
    }

    public class ActivityDto
    {
        public ActivityDto()
        {
        }
        public int Id { get; set; }
        public DateTime WorkShiftStartTime { get; set; }
        public DateTime? WorkShiftEndTime { get; set; }
        public DateTime? BreakStartTime { get; set; }
        public DateTime? BreakEndTime { get; set; }
        public DateTime? LunchStartTime { get; set; }
        public DateTime? LunchEndTime { get; set; }
        public bool IsShiftActive { get; set; }
        public bool IsBreakActive { get; set; }
        public bool IsLunchActive { get; set; }
        public Employee Employee { get; set; }
    }
}
