using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.ValidationAttributes;

namespace CourseLibrary.API.Models;

[DifferentTitleAndDescriptionRequired]
public abstract class CourseManipulationDto
{
    [Required(ErrorMessage = "Title is required for the course.")]
    [MaxLength(100, ErrorMessage = "Title should not have more than 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1500, ErrorMessage = "Description should not exceed more than 1500 characters")]
    public virtual string Description { get; set; } = string.Empty;

}
