using Microsoft.AspNetCore.Authorization;

namespace ShopManagementAPI.Authorization
{
    public class Permissions
    {
        // CATEGORY
        public const string CreateCategory = "CREATE_CATEGORY";
        public const string UpdateCategory = "UPDATE_CATEGORY";
        public const string DeleteCategory = "DELETE_CATEGORY";
        public const string RestoreCategory = "RESTORE_CATEGORY";

        // PRODUCT
        public const string CreateProduct = "CREATE_PRODUCT";
        public const string UpdateProduct = "UPDATE_PRODUCT";
        public const string DeleteProduct = "DELETE_PRODUCT";
        public const string UpdateProductStock = "UPDATE_PRODUCT_STOCK";
        public const string RestoreProduct = "RESTORE_PRODUCT";

        // ORDER
        public const string CreateOrder = "CREATE_ORDER";
        public const string GetOrders = "GET_ORDERS";
        public const string GetOrderDetail = "GET_ORDER_DETAIL";
        public const string UpdateOrderStatus = "UPDATE_ORDER_STATUS";

        // MY ORDER
        public const string GetMyOrders = "GET_MY_ORDERS";
        public const string GetMyOrderDetail = "GET_MY_ORDER_DETAIL";
        public const string CancelOrder = "CANCEL_ORDER";

        // USER
        public const string CreateUser = "CREATE_USER";
        public const string GetUsers = "GET_USERS";
        public const string GetUserDetail = "GET_USER_DETAIL";
        public const string UpdateUserProfile = "UPDATE_USER_PROFILE";
        public const string AddUserRoles = "ADD_USER_ROLES";
        public const string RemoveUserRoles = "REMOVE_USER_ROLES";
        public const string LockUser = "LOCK_USER";
        public const string UnlockUser = "UNLOCK_USER";

        // ROLE
        public const string GetRoles = "GET_ROLES";
        public const string GetRoleDetail = "GET_ROLE_DETAIL";
        public const string AddRolePermissions = "ADD_ROLE_PERMISSIONS";
        public const string RemoveRolePermissions = "REMOVE_ROLE_PERMISSIONS";

        // PERMISSION
        public const string GetPermissions = "GET_PERMISSIONS";
        public const string GetPermissionDetail = "GET_PERMISSION_DETAIL";

        // DASHBOARD
        public const string ViewDashboardOverview = "VIEW_DASHBOARD_OVERVIEW";
        public const string ViewRevenue = "VIEW_REVENUE";



    }
}
