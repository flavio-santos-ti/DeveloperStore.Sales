using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Dtos.User;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Extensions;
using DeveloperStore.Sales.Service.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Extensions;
using DeveloperStore.Sales.Storage.SQL.PostgreSQL.Interfaces;

namespace DeveloperStore.Sales.Service.Services;

public class UserService : IUserService
{
    private readonly IMapper _mapper;
    private readonly IValidator<RequestUserDto> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IMapper mapper, IValidator<RequestUserDto> validator, IUnitOfWork unitOfWork)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(validator));
    }

    public async Task<ApiResponseDto<UserDto>> CreateAsync(RequestUserDto dto)
    {
        if (dto == null)
            return ApiResponseDto<UserDto>.AsBadRequest("Os dados do usuário não podem ser nulos.");

        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return validationResult.ToApiResponse<UserDto>();

        var exists = await _unitOfWork.UserRepository.ExistsByEmailAsync(dto.Email);
        if (exists)
            return ApiResponseDto<UserDto>.AsBadRequest("Já existe um usuário com este e-mail.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password); 

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.CommitAsync();

            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponseDto<UserDto>.AsCreated(userDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<UserDto>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<UserDto>> UpdateAsync(int id, RequestUserDto dto)
    {
        if (dto == null)
            return ApiResponseDto<UserDto>.AsBadRequest("Os dados do usuário não podem ser nulos.");

        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            return validationResult.ToApiResponse<UserDto>();

        var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(id);
        if (existingUser == null)
            return ApiResponseDto<UserDto>.AsNotFound($"Usuário com o ID {id} não encontrado.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (!string.IsNullOrEmpty(dto.Password))
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            _mapper.Map(dto, existingUser);

            await _unitOfWork.UserRepository.UpdateAsync(existingUser);
            await _unitOfWork.CommitAsync();

            var updatedUserDto = _mapper.Map<UserDto>(existingUser);
            return ApiResponseDto<UserDto>.AsSuccess(updatedUserDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<UserDto>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<UserDto>> DeleteAsync(int id)
    {
        var existingUser = await _unitOfWork.UserRepository.GetByIdAsync(id);
        if (existingUser == null)
            return ApiResponseDto<UserDto>.AsNotFound($"Usuário com o ID {id} não encontrado.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await _unitOfWork.UserRepository.DeleteAsync(existingUser);
            await _unitOfWork.CommitAsync();

            var userDto = _mapper.Map<UserDto>(existingUser);
            return ApiResponseDto<UserDto>.AsSuccess(userDto, $"Usuário com o ID {id} excluído com sucesso.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            return ApiResponseDto<UserDto>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<PagedResponseDto<UserDto>>> GetAllAsync(int page = 1, int size = 10, string? order = null)
    {
        try
        {
            var query = _unitOfWork.UserRepository.GetAllQueryable();

            if (!string.IsNullOrWhiteSpace(order))
            {
                var orderParams = order.Split(',');
                bool isFirstOrder = true;

                foreach (var param in orderParams)
                {
                    var isDescending = param.Trim().EndsWith(" desc", StringComparison.OrdinalIgnoreCase);
                    var propertyName = isDescending
                        ? param.Replace(" desc", "", StringComparison.OrdinalIgnoreCase).Trim()
                        : param.Replace(" asc", "", StringComparison.OrdinalIgnoreCase).Trim();

                    var propertyInfo = typeof(User).GetProperties()
                        .FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

                    if (propertyInfo == null)
                    {
                        return ApiResponseDto<PagedResponseDto<UserDto>>.AsBadRequest(
                            $"Propriedade '{propertyName}' não encontrada no modelo.");
                    }

                    query = isFirstOrder
                        ? query.OrderByDynamic(propertyInfo.Name, isDescending)
                        : query.ThenByDynamic(propertyInfo.Name, isDescending);

                    isFirstOrder = false;
                }
            }

            var totalItems = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            if (!users.Any())
                return ApiResponseDto<PagedResponseDto<UserDto>>.AsNotFound("Nenhum usuário encontrado.");

            var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);

            var pagedResponse = new PagedResponseDto<UserDto>(
                data: userDtos!,
                totalItems: totalItems,
                currentPage: page,
                totalPages: (int)Math.Ceiling((double)totalItems / size)
            );

            return ApiResponseDto<PagedResponseDto<UserDto>>.AsSuccess(pagedResponse);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<PagedResponseDto<UserDto>>.AsInternalServerError($"Erro interno: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<UserDto>> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(id);

        if (user == null)
            return ApiResponseDto<UserDto>.AsNotFound($"Usuário com o ID {id} não encontrado.");

        var userDto = _mapper.Map<UserDto>(user);

        return ApiResponseDto<UserDto>.AsSuccess(userDto);
    }
}
