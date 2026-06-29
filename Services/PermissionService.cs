using ShopManagementAPI.Data;
using ShopManagementAPI.Exceptions;
using ShopManagementAPI.Models;
using ShopManagementAPI.Repositories;
using Microsoft.EntityFrameworkCore;
using ShopManagementAPI.DTOs.Common;
using ShopManagementAPI.DTOs.request.Permission;
using ShopManagementAPI.DTOs.response.Permission;

namespace ShopManagementAPI.Services
{
    public class PermissionService
    {
        private readonly PermissionRepository _repo;

        public PermissionService(PermissionRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResponseDTO<PermissionResponseDTO>> GetAllAsync(
        PermissionQueryDTO request)
        {
            var query = _repo.Query();

            // SEARCH : Tìm kiếm theo Name hoặc Description
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = query.Where(x =>
                    x.Name.Contains(request.Keyword) ||
                    (x.Description != null &&
                     x.Description.Contains(request.Keyword)));
            }

            // FILTER : Lọc theo Module
            if (!string.IsNullOrWhiteSpace(request.Module))
            {
                query = query.Where(x =>
                    x.Module == request.Module);
            }

            // Lọc theo ApiPath
            if (!string.IsNullOrWhiteSpace(request.ApiPath))
            {
                query = query.Where(x =>
                    x.ApiPath.Contains(request.ApiPath));
            }

            // Lọc theo HttpMethod
            if (request.Method.HasValue)
            {
                query = query.Where(x =>
                    x.Method == request.Method.Value);
            }

            // SORT
            query = request.SortBy.ToLower() switch
            {
                "name" => request.SortDirection == "desc"
                    ? query.OrderByDescending(x => x.Name)
                    : query.OrderBy(x => x.Name),

                "module" => request.SortDirection == "desc"
                    ? query.OrderByDescending(x => x.Module)
                    : query.OrderBy(x => x.Module),

                "apipath" => request.SortDirection == "desc"
                    ? query.OrderByDescending(x => x.ApiPath)
                    : query.OrderBy(x => x.ApiPath),

                "createdat" => request.SortDirection == "desc"
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt),

                _ => request.SortDirection == "desc"
                    ? query.OrderByDescending(x => x.Id)
                    : query.OrderBy(x => x.Id)
            };

            var totalCount = await query.CountAsync();

            // PAGINATION
            var permissions = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new PagedResponseDTO<PermissionResponseDTO>
            {
                Items = permissions
                    .Select(MapToDTO)
                    .ToList(),
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(
                    totalCount / (double)request.PageSize)
            };
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
