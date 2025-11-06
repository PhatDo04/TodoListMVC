using AutoMapper;
using TodoListMVC.DTOs;
using TodoListMVC.Models;

namespace TodoListMVC.App_Start
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Dạy AutoMapper cách ánh xạ giữa các lớp nguồn và đích
            //Từ TaskModel sang TaskDto (Gửi đi)
            CreateMap<TaskModel, TaskDto>();
            //Từ TaskCreateDto và TaskUpdateDto sang TaskModel (Nhận vào)
            CreateMap<TaskCreateDto, TaskModel>();
            //Từ TaskUpdateDto sang TaskModel (Cập nhật)
            CreateMap<TaskUpdateDto, TaskModel>();
        }
    }
}