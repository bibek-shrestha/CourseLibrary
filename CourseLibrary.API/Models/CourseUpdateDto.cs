using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public class CourseUpdateDto: CourseManipulationDto
{
    [Required(ErrorMessage = "Description is required for the course.")]
    public override string Description { get => base.Description; set => base.Description = value; }
}
