using System.ComponentModel.DataAnnotations;

namespace TaskNest.Helper
{
    public static class ValidationHelper
    {
        public static (bool IsValid, IEnumerable<string> Errors) ValidateObject<T>(T obj)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(obj);

            bool isValid = Validator.TryValidateObject(obj, context, results, true);

            var errors = results.Select(r => r.ErrorMessage ?? "Validation error occurred.");
            return (isValid, errors);
        }
    }
}
