using DeveloperStore.Sales.Domain.Dtos.Response;
using FluentValidation.Results;

namespace DeveloperStore.Sales.Service.Extensions;

public static class ValidatorExtensions
{
    public static ApiResponseDto<T> ToApiResponse<T>(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return ApiResponseDto<T>.AsSuccess();
        }

        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
        return ApiResponseDto<T>.AsBadRequest($"Erro(s) de validação: {errors}");
    }
}
