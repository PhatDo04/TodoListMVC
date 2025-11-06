using System.ComponentModel.DataAnnotations;

namespace TodoListMVC.DTOs
{
    public class TaskCreateDto
    {
        [Required]
        [MinLength(3)]
        public string Title { get; set; }
    }
}