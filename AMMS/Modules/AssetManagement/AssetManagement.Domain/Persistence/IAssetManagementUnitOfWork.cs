using AMMS.Core.Interfaces;
using AssetManagement.Domain.Repositories;

namespace AssetManagement.Domain.Persistence;

public interface IAssetManagementUnitOfWork : IUnitOfWork
{
    IAssetManagementRepository Assets { get; }
}
