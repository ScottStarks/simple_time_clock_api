using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WorkingShiftActivity.Context.Models;
using WorkingShiftActivity.DataManager;
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
        private readonly IMapper _mapper;
        public ActivityController(IActivityDataRepository<Activity, 
            ActivityDto> dataRepository,
            IMapper mapper)
        {
            _dataRepository = dataRepository;
            _mapper = mapper;
        }

        [Route("GetShiftData")]
        [HttpGet]
        public IActionResult Activity([FromQuery] string employeeId)
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
                var employeeActivities = _dataRepository.GetAll(employeeId).ToList();
                bool isAnotherShiftActive = employeeActivities.Find(x => x.IsShiftActive) != null;
                if (!isAnotherShiftActive)
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
                        Message = "Activity created successfully",
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
                };
                return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse> { Message = "Can't create new shift as another active!" });
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

                    entity.WorkShiftEndTime = DateTime.Now;
                    entity.IsShiftActive = false;

                    _dataRepository.Update(entity);
                    return StatusCode((int)HttpStatusCode.OK, new Response<ActivityResponse>
                    {
                        Message = "Shift ended successfully!"
                    });

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
                    if (entity.IsShiftActive)
                    {
                        var isBreakAlreadyEnded = entity.BreakEndTime != null;
                        var isLunchTimeAlreadyEnded = entity.LunchEndTime != null;
                        if (entity.IsBreakActive || isBreakAlreadyEnded)
                        {
                            return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                            {
                                Message = "Can't start break time as already active or ended!"
                            });
                        }
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

                    return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                    {
                        Message = "Can't start break time in an inactive shift!"
                    });
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
                    if (entity.IsShiftActive)
                    {
                        if (entity.IsBreakActive)
                        {
                            entity.BreakEndTime = DateTime.Now;
                            entity.IsBreakActive = false;

                            _dataRepository.Update(entity);
                            return StatusCode((int)HttpStatusCode.OK, new Response<ActivityResponse>
                            {
                                Message = "Break time end!",
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
                    if (entity.IsShiftActive)
                    {
                        var isLunchTimeAlreadyEnded = entity.LunchEndTime != null;
                        if (entity.IsLunchActive || isLunchTimeAlreadyEnded)
                        {
                            return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                            {
                                Message = "Can't start lunch time as already active or ended!"
                            });
                        }
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

                    return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                    {
                        Message = "Can't start lunch time in an inactive shift!"
                    });
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
                    if (entity.IsShiftActive)
                    {
                        if (entity.IsLunchActive)
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
                        return StatusCode((int)HttpStatusCode.Conflict, new Response<ActivityResponse>
                        {
                            Message = "there is no active lunch!"
                        });
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

    }
}
