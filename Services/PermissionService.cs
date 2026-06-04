using ShopManagementAPI.Data;
using ShopManagementAPI.DTOs.request;
using ShopManagementAPI.DTOs.response;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.Models;
using ShopManagementAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ShopManagementAPI.Services
{
    public class PermissionService
    {
        private readonly PermissionRepository _repo;

        public PermissionService(PermissionRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<PermissionResponseDTO>> GetAllAsync()
        {
            var permissions = await _repo.GetAllAsync();

            return permissions.Select(MapToDTO).ToList();
        }

        public async Task<PermissionResponseDTO> GetByIdAsync(int id)
        {
            var permission = await _repo.GetByIdAsync(id)
                ?? throw new NotFoundException("Không tìm thấy quyền");

            return MapToDTO(permission);
        }

        private static PermissionResponseDTO MapToDTO(Permission p)
        {
            return new PermissionResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ApiPath = p.ApiPath,
                Method = p.Method,
                Module = p.Module,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            };
        }
    }

}
