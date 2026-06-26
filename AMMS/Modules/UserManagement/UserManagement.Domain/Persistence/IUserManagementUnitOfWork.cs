using AMMS.Core.Interfaces;
using UserManagement.Domain.Repositories;

namespace UserManagement.Domain.Persistence;

public interface IUserManagementUnitOfWork : IUnitOfWork
{
    IUserManagementRepository Users { get; }

    IRoleRepository Roles { get; }

    IRoleGroupRepository RoleGroups { get; }
}
