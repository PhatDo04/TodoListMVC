using System.ComponentModel.DataAnnotations;

namespace TodoListMVC.DTOs
{
    public class TaskUpdateDto
    {
        [Required]
        [MinLength(3)]
        public string Title { get; set; }

        [Required]
        public bool IsCompleted { get; set; }
    }
}