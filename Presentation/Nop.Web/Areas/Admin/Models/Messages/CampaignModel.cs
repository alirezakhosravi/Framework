﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Messages
{
    /// <summary>
    /// Represents a campaign model
    /// </summary>
    public partial class CampaignModel : BaseNopEntityModel
    {
        #region Ctor

        public CampaignModel()
        {
            this.AvailableSites = new List<SelectListItem>();
            this.AvailableUserRoles = new List<SelectListItem>();
            this.AvailableEmailAccounts = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Subject")]
        public string Subject { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Body")]
        public string Body { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.Site")]
        public int SiteId { get; set; }
        public IList<SelectListItem> AvailableSites { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.UserRole")]
        public int UserRoleId { get; set; }
        public IList<SelectListItem> AvailableUserRoles { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.DontSendBeforeDate")]
        [UIHint("DateTimeNullable")]
        public DateTime? DontSendBeforeDate { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.AllowedTokens")]
        public string AllowedTokens { get; set; }

        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.EmailAccount")]
        public int EmailAccountId { get; set; }
        public IList<SelectListItem> AvailableEmailAccounts { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Promotions.Campaigns.Fields.TestEmail")]
        public string TestEmail { get; set; }

        #endregion
    }
}