﻿using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Users
{
    /// <summary>
    /// Represents a user role model
    /// </summary>
    public partial class UserRoleModel : BaseNopEntityModel
    {
        #region Ctor

        public UserRoleModel()
        {
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.IsSystemRole")]
        public bool IsSystemRole { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.SystemName")]
        public string SystemName { get; set; }

        [NopResourceDisplayName("Admin.Users.UserRoles.Fields.EnablePasswordLifetime")]
        public bool EnablePasswordLifetime { get; set; }

        #endregion
    }
}