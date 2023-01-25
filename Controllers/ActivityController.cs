using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WorkingShiftActivity.Context.Models;
using WorkingShiftActivity.DataManager;
using WorkingShiftActivity.Enums;
using WorkingShiftActivity.Models.RequestModels;
using WorkingShiftActivity.Models.ResponseModels;
using WorkingShiftActivity.Repository;

namespace WorkingShiftActivity.Controllers
{
    [Authorize]
    [Route("api/activity")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityDataRepository<Activity, ActivityDto> _dataRepository;
        private readonly IAuthDataRepository<Employee, EmployeeDto> _authRepository;
        private readonly IMapper _mapper;
        public ActivityController(IActivityDataRepository<Activity,
            ActivityDto> dataRepository,
            IMapper mapper,
            IAuthDataRepository<Employee, EmployeeDto> authRepository)
        {
            _dataRepository = dataRepository;
            _mapper = mapper;
            _authRepository = authRepository;
        }

        [Route("getshiftdata")]
        [HttpGet]
        public IActionResult Activity([FromQuery] string? employeeId)
        {
            try
            {
                var data = _dataRepository.GetAll(employeeId);
                return StatusCode((int)HttpStatusCode.OK, new Response<List<ActivityResponse>> { Message = "Success!", Data = _mapper.Map<List<ActivityResponse>>(data) });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.InnerException?.Message);
            }
        }

        [Route("startshift")]
        [HttpPost]
        public IActionResult StartWorkShift([FromHeader] string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId) || string.IsNullOrWhiteSpace(employeeId))
            {
                return BadRequest();
            }
            try
            {
                var employeeInfo = _authRepository.Get(employeeId);
                if (employeeInfo.Role == (int)RoleEnum.Admin)
                {
                    return CreateShift(employeeId);
                }
                else
                {
                    var employeeActivities = _dataRepository.GetAll(employeeId).ToList();
                    bool isAnotherShiftActive = employeeActivities.Find(x => x.IsShiftActive) != null;
                    if (!isAnotherShiftActive)
                    {
                        return CreateShift(employeeId);
                    };
                }
                return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse> { Message = "Can't start new shift as another one is active!" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.InnerException?.Message);
            }
        }

        [Route("endshift")]
        [HttpPost]
        public IActionResult EndWorkShift([FromBody] ActivityRequest request)
        {
            if (request is null)
            {
                return BadRequest("request contains null value.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var entity = _dataRepository.Get(request.ShiftId);

                if (entity != null)
                {
                    var employeeInfo = _authRepository.Get(entity.EmployeeId);
                    if (employeeInfo.Role == (int)RoleEnum.Admin)
                    {
                        return EndShift(entity);
                    }
                    else
                    {
                        if (entity.IsBreakActive)
                        {
                            return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                            {
                                Message = "Can't end shift in active break!"
                            });
                        }

                        if (entity.IsLunchActive)
                        {
                            return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                            {
                                Message = "Can't end shift in active lunch!"
                            });
                        }

                        return EndShift(entity);
                    }

                }
                return StatusCode((int)HttpStatusCode.NotFound, new Response<ActivityResponse> { Message = "Shift not found!" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.InnerException?.Message);
            }
        }

        [Route("startbreak")]
        [HttpPost]
        public IActionResult StartBreak([FromBody] ActivityRequest request)
        {
            if (request is null)
            {
                return BadRequest("request contains null value.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var entity = _dataRepository.Get(request.ShiftId);

                if (entity != null)
                {
                    var employeeInfo = _authRepository.Get(entity.EmployeeId);
                    if (employeeInfo.Role == (int)RoleEnum.Admin)
                    {
                        return StartBreak(entity);
                    }
                    else
                    {
                        if (entity.IsShiftActive)
                        {
                            var isBreakAlreadyEnded = entity.BreakEndTime != null;
                            var isLunchTimeAlreadyEnded = entity.LunchEndTime != null;
                            if (entity.IsBreakActive || isBreakAlreadyEnded)
                            {
                                return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                                {
                                    Message = "Can't start break time as another is already active or ended!"
                                });
                            }
                            return StartBreak(entity);
                        }

                        return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                        {
                            Message = "Can't start break time in an inactive shift!"
                        });
                    }
                }
                return StatusCode((int)HttpStatusCode.NotFound, new Response<ActivityResponse> { Message = "Shift not found!" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.InnerException?.Message);
            }
        }

        [Route("endbreak")]
        [HttpPost]
        public IActionResult EndBreak([FromBody] ActivityRequest request)
        {
            if (request is null)
            {
                return BadRequest("request contains null value.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var entity = _dataRepository.Get(request.ShiftId);

                if (entity != null)
                {
                    var employeeInfo = _authRepository.Get(entity.EmployeeId);
                    if (employeeInfo.Role == (int)RoleEnum.Admin)
                    {
                        return EndBreak(entity);
                    }
                    else
                    {
                        if (entity.IsShiftActive)
                        {
                            if (entity.IsBreakActive)
                            {
                                return EndBreak(entity);
                            }
                            return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                            {
                                Message = "There is no active break!"
                            });
                        }

                        return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                        {
                            Message = "Can't end break time in an inactive shift!"
                        });
                    }
                }
                return StatusCode((int)HttpStatusCode.NotFound, new Response<ActivityResponse> { Message = "Shift not found!" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.InnerException?.Message);
            }
        }

        [Route("startlunch")]
        [HttpPost]
        public IActionResult StartLunch([FromBody] ActivityRequest request)
        {
            if (request is null)
            {
                return BadRequest("request contains null value.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var entity = _dataRepository.Get(request.ShiftId);

                if (entity != null)
                {
                    var employeeInfo = _authRepository.Get(entity.EmployeeId);
                    if (employeeInfo.Role == (int)RoleEnum.Admin)
                    {
                        return StartLunch(entity);
                    }
                    else
                    {
                        if (entity.IsShiftActive)
                        {
                            var isLunchTimeAlreadyEnded = entity.LunchEndTime != null;
                            if (entity.IsLunchActive || isLunchTimeAlreadyEnded)
                            {
                                return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                                {
                                    Message = "Can't start lunch time as another one already active or ended!"
                                });
                            }
                            return StartLunch(entity);
                        }

                        return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                        {
                            Message = "Can't start lunch time in an inactive shift!"
                        });
                    }
                }
                return StatusCode((int)HttpStatusCode.NotFound, new Response<ActivityResponse> { Message = "Shift not found!" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.InnerException?.Message);
            }
        }

        [Route("endlunch")]
        [HttpPost]
        public IActionResult EndLunch([FromBody] ActivityRequest request)
        {
            if (request is null)
            {
                return BadRequest("request contains null value.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            try
            {
                var entity = _dataRepository.Get(request.ShiftId);

                if (entity != null)
                {
                    var employeeInfo = _authRepository.Get(entity.EmployeeId);
                    if (employeeInfo.Role == (int)RoleEnum.Admin)
                    {
                        return EndLunch(entity);
                    }
                    else
                    {
                        if (entity.IsShiftActive)
                        {
                            if (entity.IsLunchActive)
                            {
                                return EndLunch(entity);
                            }
                            return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                            {
                                Message = "there is no active lunch!"
                            });
                        }
                    }

                    return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                    {
                        Message = "Can't end lunch time in an inactive shift!"
                    });
                }
                return StatusCode((int)HttpStatusCode.NotFound, new Response<ActivityResponse> { Message = "Shift not found!" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.InnerException?.Message);
            }
        }

        #region methods
        private ObjectResult CreateShift(string employeeId)
        {
            Activity activity = new Activity
            {
                EmployeeId = employeeId,
                WorkShiftStartTime = DateTime.Now,
                WorkShiftEndTime = null,
                BreakStartTime = null,
                BreakEndTime = null,
                LunchStartTime = null,
                LunchEndTime = null,
                IsBreakActive = false,
                IsLunchActive = false,
                IsShiftActive = true

            };
            _dataRepository.Add(activity);
            return StatusCode((int)HttpStatusCode.Created, new Response<ActivityResponse>
            {
                Message = "Shift started successfully",
                Data = new ActivityResponse
                {
                    Id = activity.Id,
                    WorkShiftStartTime = activity.WorkShiftStartTime,
                    WorkShiftEndTime = activity.WorkShiftEndTime,
                    BreakStartTime = activity.BreakStartTime,
                    BreakEndTime = activity.BreakEndTime,
                    LunchStartTime = activity.LunchStartTime,
                    LunchEndTime = activity.LunchEndTime,
                    IsBreakActive = activity.IsBreakActive,
                    IsLunchActive = activity.IsLunchActive,
                    IsShiftActive = activity.IsShiftActive
                }
            });
        }

        private ObjectResult EndShift(Activity entity)
        {
            entity.WorkShiftEndTime = DateTime.Now;
            entity.IsShiftActive = false;

            _dataRepository.Update(entity);
            return StatusCode((int)HttpStatusCode.OK, new Response<ActivityResponse>
            {
                Message = "Shift ended successfully!",
                Data = new ActivityResponse
                {
                    Id = entity.Id,
                    WorkShiftStartTime = entity.WorkShiftStartTime,
                    WorkShiftEndTime = entity.WorkShiftEndTime,
                    BreakStartTime = entity.BreakStartTime,
                    BreakEndTime = entity.BreakEndTime,
                    LunchStartTime = entity.LunchStartTime,
                    LunchEndTime = entity.LunchEndTime,
                    IsBreakActive = entity.IsBreakActive,
                    IsLunchActive = entity.IsLunchActive,
                    IsShiftActive = entity.IsShiftActive
                }
            });
        }

        private ObjectResult StartBreak(Activity entity)
        {
            entity.BreakStartTime = DateTime.Now;
            entity.IsBreakActive = true;

            _dataRepository.Update(entity);
            return StatusCode((int)HttpStatusCode.OK, new Response<ActivityResponse>
            {
                Message = "Break time start!",
                Data = new ActivityResponse
                {
                    Id = entity.Id,
                    WorkShiftStartTime = entity.WorkShiftStartTime,
                    WorkShiftEndTime = entity.WorkShiftEndTime,
                    BreakStartTime = entity.BreakStartTime,
                    BreakEndTime = entity.BreakEndTime,
                    LunchStartTime = entity.LunchStartTime,
                    LunchEndTime = entity.LunchEndTime,
                    IsBreakActive = entity.IsBreakActive,
                    IsLunchActive = entity.IsLunchActive,
                    IsShiftActive = entity.IsShiftActive
                }
            });
        }

        private ObjectResult EndBreak(Activity entity)
        {
            entity.BreakStartTime = DateTime.Now;
            entity.IsBreakActive = true;

            _dataRepository.Update(entity);
            return StatusCode((int)HttpStatusCode.OK, new Response<ActivityResponse>
            {
                Message = "Break time start!",
                Data = new ActivityResponse
                {
                    Id = entity.Id,
                    WorkShiftStartTime = entity.WorkShiftStartTime,
                    WorkShiftEndTime = entity.WorkShiftEndTime,
                    BreakStartTime = entity.BreakStartTime,
                    BreakEndTime = entity.BreakEndTime,
                    LunchStartTime = entity.LunchStartTime,
                    LunchEndTime = entity.LunchEndTime,
                    IsBreakActive = entity.IsBreakActive,
                    IsLunchActive = entity.IsLunchActive,
                    IsShiftActive = entity.IsShiftActive
                }
            });
        }

        private ObjectResult StartLunch(Activity entity)
        {
            entity.LunchStartTime = DateTime.Now;
            entity.IsLunchActive = true;

            _dataRepository.Update(entity);
            return StatusCode((int)HttpStatusCode.OK, new Response<ActivityResponse>
            {
                Message = "Lunch time start!",
                Data = new ActivityResponse
                {
                    Id = entity.Id,
                    WorkShiftStartTime = entity.WorkShiftStartTime,
                    WorkShiftEndTime = entity.WorkShiftEndTime,
                    BreakStartTime = entity.BreakStartTime,
                    BreakEndTime = entity.BreakEndTime,
                    LunchStartTime = entity.LunchStartTime,
                    LunchEndTime = entity.LunchEndTime,
                    IsBreakActive = entity.IsBreakActive,
                    IsLunchActive = entity.IsLunchActive,
                    IsShiftActive = entity.IsShiftActive
                }
            });

        }

        private ObjectResult EndLunch(Activity entity)
        {
            entity.LunchEndTime = DateTime.Now;
            entity.IsLunchActive = false;

            _dataRepository.Update(entity);
            return StatusCode((int)HttpStatusCode.OK, new Response<ActivityResponse>
            {
                Message = "Lunch time end!",
                Data = new ActivityResponse
                {
                    Id = entity.Id,
                    WorkShiftStartTime = entity.WorkShiftStartTime,
                    WorkShiftEndTime = entity.WorkShiftEndTime,
                    BreakStartTime = entity.BreakStartTime,
                    BreakEndTime = entity.BreakEndTime,
                    LunchStartTime = entity.LunchStartTime,
                    LunchEndTime = entity.LunchEndTime,
                    IsBreakActive = entity.IsBreakActive,
                    IsLunchActive = entity.IsLunchActive,
                    IsShiftActive = entity.IsShiftActive
                }
            });
        }
        #endregion
    }
}
