using ShopManagementAPI.Models;
using ShopManagementAPI.Models.Enum;
using static ShopManagementAPI.Authorization.UserDataScopeService;

namespace ShopManagementAPI.Authorization
{
    //nơi phân quyền user được xem những gì 
    public class UserDataScopeService
    {
        private static bool HasRole(
        User user,
        RoleType role)
        {
            return user.UserRoles.Any(ur =>
                ur.Role.Name == role);
        }

        //get all user 
        public IQueryable<User> FilterUsers(
        IQueryable<User> users,
        List<RoleType> currentUserRoles)
        {
            if (!currentUserRoles.Any())
                return Enumerable.Empty<User>().AsQueryable();

            if (currentUserRoles.Contains(RoleType.ADMIN))
                return users;

            if (currentUserRoles.Contains(RoleType.MANAGER))
            {
                return users.Where(u =>
                    u.UserRoles.Any(ur =>
                        ur.Role.Name == RoleType.STAFF ||
                        ur.Role.Name == RoleType.CUSTOMER));
            }

            if (currentUserRoles.Contains(RoleType.STAFF))
            {
                return users.Where(u =>
                    u.UserRoles.Any(ur =>
                        ur.Role.Name == RoleType.CUSTOMER));
            }

            return Enumerable.Empty<User>().AsQueryable();
        }

        //get user by id 
        public bool CanViewUser(
        IEnumerable<RoleType> currentRoles,
        User targetUser)
        {
            return currentRoles.Any(role =>
                role switch
                {
                    RoleType.ADMIN => true,

                    RoleType.MANAGER =>
                        HasRole(targetUser, RoleType.STAFF) ||
                        HasRole(targetUser, RoleType.CUSTOMER),

                    RoleType.STAFF =>
                        HasRole(targetUser, RoleType.CUSTOMER),

                    _ => false
                });
        }

        //lock/unlock chỉ admin
        public bool CanManageUser(
        IEnumerable<RoleType> currentRoles,
        User targetUser)
        {
            return currentRoles.Contains(RoleType.ADMIN);
        }

        //add remove permissions chỉ admin
        public bool CanManageRole(
        IEnumerable<RoleType> currentRoles,
        Role targetRole)
        {
            return currentRoles.Contains(RoleType.ADMIN);
        }
    }
}
