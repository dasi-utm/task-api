using TaskManager.Application.Common;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Queries.User;

public record GetUsersQuery(int Page = 1, int PageSize = 20);

public class GetUsersQueryHandler
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserDto>> Handle(GetUsersQuery query)
    {
        var users = await _userRepository.GetAllAsync();
        var total = users.Count;

        var paged = users
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(u => new UserDto(u.Id, u.Email, u.FirstName, u.LastName, u.Role.ToString()))
            .ToList();

        return new PagedResult<UserDto>(paged, total, query.Page, query.PageSize);
    }
}
