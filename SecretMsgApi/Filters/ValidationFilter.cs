using System.ComponentModel.DataAnnotations;

namespace SecretMsgApi.Filters
{
    public class ValidationFilter<T> : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {

            var obj = context.Arguments.OfType<T>().FirstOrDefault();
            if (obj == null)
                return Results.BadRequest("There are no derails in the request.");

            var validationContext = new ValidationContext(obj);
            var errors = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(obj, validationContext, errors, true);

            if (!isValid)
                return Results.BadRequest(errors.FirstOrDefault()?.ErrorMessage);

           return await next(context);
        }
    }
}
