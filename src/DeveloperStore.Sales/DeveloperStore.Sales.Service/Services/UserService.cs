using AutoMapper;
using DeveloperStore.Sales.Domain.Dtos.Response;
using DeveloperStore.Sales.Domain.Dtos.User;
using DeveloperStore.Sales.Domain.Models;
using DeveloperStore.Sales.Service.Extensions;
using DeveloperStore.Sales.Service.Interfaces;
using DeveloperStore.Sales.Storage.Interfaces;
using FluentValidation;

namespace DeveloperStore.Sales.Service.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<RequestUserDto> _validator;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IMapper mapper, IValidator<RequestUserDto> validator, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(userRepository));
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

        var exists = await _userRepository.ExistsByEmailAsync(dto.Email);
        if (exists)
            return ApiResponseDto<UserDto>.AsBadRequest("Já existe um usuário com este e-mail.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = _mapper.Map<User>(dto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password); 

            await _userRepository.AddAsync(user);
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

        var existingUser = await _userRepository.GetByIdAsync(id);
        if (existingUser == null)
            return ApiResponseDto<UserDto>.AsNotFound($"Usuário com o ID {id} não encontrado.");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            if (!string.IsNullOrEmpty(dto.Password))
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            _mapper.Map(dto, existingUser);

            await _userRepository.UpdateAsync(existingUser);
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
}
