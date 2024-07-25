using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.Models;

namespace CourseLibrary.API.ValidationAttributes;

public class DifferentTitleAndDescriptionRequired: ValidationAttribute
{
    public DifferentTitleAndDescriptionRequired()
    {}

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (validationContext.ObjectInstance is not CourseManipulationDto course)
        {
            throw new Exception($"Attribute {nameof(DifferentTitleAndDescriptionRequired)} must be applied to a {nameof(CourseManipulationDto)} or its derived types.");
        }
        if (course.Title == course.Description)
        {
            return new ValidationResult("The description of the course should not be the same as title of the course."
            , new[] {"Course"});
        }
        return ValidationResult.Success;
    }

}
