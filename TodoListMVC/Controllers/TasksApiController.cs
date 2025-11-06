using AutoMapper;
using System;
using System.Collections.Generic;
using System.Web.Http;
using TodoListMVC.DTOs;
using TodoListMVC.Models;
using TodoListMVC.Repositories;

namespace TodoListMVC.Controllers
{
    public class TasksApiController : ApiController
    {
        //Giữ "khớp nối" (Interface)
        //private readonly ITaskRepository _repository;
        //Thay bằng sếp (UoW)
        private readonly IUnitOfWork _uow;

        private readonly IMapper _mapper;

        public TasksApiController(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        // GET api/<controller>
        public IHttpActionResult GetTasks()
        {
            try
            {
                //lấy full cột từ csdl
                var taskModel = _uow.Tasks.GetTasks();
                //dịch sang DTO (tự động lặp và dịch từng cái)
                var taskDtos = _mapper.Map<IEnumerable<TaskDto>>(taskModel);
                //Trả về JSON + mã 200 OK
                return Ok(taskDtos);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET api/<controller>/5
        public IHttpActionResult GetTask(int id)
        {
            try
            {
                var taskModel = _uow.Tasks.GetTask(id);
                if (taskModel == null)
                {
                    return NotFound();
                }
                var taskDto = _mapper.Map<TaskDto>(taskModel);
                // Trả về JSON (1 task) + mã 200 OK
                return Ok(taskDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // [FromBody] TaskModel task: Tự động "đổ" JSON gửi lên vào Model
        // POST api/<controller>
        [HttpPost]
        public IHttpActionResult PostTask([FromBody] TaskCreateDto taskDto)
        {
            if (!ModelState.IsValid) //Nếu model gửi lên  bị lỗi (vd: thiếu Title)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //dịch ngược từ DTO sang Model sau khi nhận vào từ client
                var taskModel = _mapper.Map<TaskModel>(taskDto);
                taskModel.IsCompleted = false; //Mặc định khi tạo mới là chưa hoàn thành
                taskModel.CreatedAt = DateTime.Now;
                taskModel.UpdatedAt = DateTime.Now;
                //Gửi model xuống repository để lưu vào CSDL để tạo task mới
                var createdTaskModel = _uow.Tasks.PostTask(taskModel);
                
                _uow.SaveChanges(); //Hoàn tất giao dịch (Commit)

                //Lấy task vừa tạo xong từ CSDL (để chắc chắn có đầy đủ dữ liệu)
                var fullyCreatedTask = _uow.Tasks.GetTask(createdTaskModel.Id);

                //Dịch ngược Model vừa tạo xong sang DTO để trả về cho client
                var createdTaskDto = _mapper.Map<TaskDto>(createdTaskModel);

                //Trả về mã 201 Created
                //Kèm  theo 1 link đến API "Get" (api/TasksApi/Id) và dữ liệu task mới
                return CreatedAtRoute("DefaultApi", new { id = createdTaskDto.Id }, createdTaskDto);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IHttpActionResult PutTask(int id, [FromBody] TaskUpdateDto taskDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Kiểm tra task có tồn tại không
            var taskModelFormDb = _uow.Tasks.GetTask(id);
            if (taskModelFormDb == null)
            {
                return NotFound();
            }

            //Dịch(đổ) dữ liệu từ DTO vào Model lấy từ DB
            _mapper.Map(taskDto, taskModelFormDb);

            _uow.Tasks.PutTask(id, taskModelFormDb);

            _uow.SaveChanges(); //Hoàn tất giao dịch (Commit)

            //Trả về 200 OK
            return Ok();
        }

        // DELETE api/<controller>/5
        [HttpDelete]
        public IHttpActionResult DeleteTask(int id)
        {
            try
            {
                var existingTask = _uow.Tasks.GetTask(id);
                if (existingTask == null)
                {
                    return NotFound();
                }

                _uow.Tasks.DeleteTask(id);

                _uow.SaveChanges(); //Hoàn tất giao dịch (Commit)

                return Ok();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        //Dọn dẹp UoW khi controller bị hủy
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _uow.Dispose(); //Tự động dọn dẹp kết nối/transaction
            }
            base.Dispose(disposing);
        }
    }
}