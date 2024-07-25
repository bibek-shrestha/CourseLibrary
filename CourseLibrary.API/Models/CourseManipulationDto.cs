using System.ComponentModel.DataAnnotations;

namespace CourseLibrary.API.Models;

public abstract class CourseManipulationDto: IValidatableObject
{
    [Required(ErrorMessage = "Title is required for the course.")]
    [MaxLength(100, ErrorMessage = "Title should not have more than 100 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1500, ErrorMessage = "Description should not exceed more than 1500 characters")]
    public virtual string Description { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Title == Description)
        {
            yield return new ValidationResult(
                "Title should not be the same as description for the course.",
                new [] { "Course" }
            );
        }
    }
}
