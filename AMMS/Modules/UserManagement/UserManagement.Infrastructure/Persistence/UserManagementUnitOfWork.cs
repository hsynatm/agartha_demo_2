using AMMS.Infrastructure.Persistence;
using UserManagement.Domain.Persistence;
using UserManagement.Domain.Repositories;

namespace UserManagement.Infrastructure.Persistence;

public sealed class UserManagementUnitOfWork : EfUnitOfWork<UserManagementDbContext>, IUserManagementUnitOfWork
{
    public UserManagementUnitOfWork(UserManagementDbContext context,IUserManagementRepository users,IRoleRepository roles,IRoleGroupRepository roleGroups): base(context)
    {
        Users = users;
        Roles = roles;
        RoleGroups = roleGroups;
    }

    public IUserManagementRepository Users { get; }

    public IRoleRepository Roles { get; }

    public IRoleGroupRepository RoleGroups { get; }
}
