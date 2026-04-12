using Demo_Course_Management.Data;
using Demo_Course_Management.DTOs.request;
using Demo_Course_Management.DTOs.response;
using Demo_Course_Management.Middleware;
using Demo_Course_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Demo_Course_Management.Services
{
    public class PermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<PermissionResponseDTO>> GetAllAsync()
        {
            return await _context.Permissions
                .Select(p => new PermissionResponseDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ApiPath = p.ApiPath,
                    Method = p.Method,
                    Module = p.Module,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<PermissionResponseDTO> GetByIdAsync(int id)
        {
            var permission = await _context.Permissions
                .Where(p => p.Id == id)
                .Select(p => new PermissionResponseDTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ApiPath = p.ApiPath,
                    Method = p.Method,
                    Module = p.Module,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (permission == null)
            {
                throw new NotFoundException("Permission not found");
            }

            return permission;
        }

    }
}
