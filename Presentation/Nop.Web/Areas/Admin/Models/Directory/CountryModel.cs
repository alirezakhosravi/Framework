﻿using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Web.Areas.Admin.Models.Directory
{
    /// <summary>
    /// Represents a country model
    /// </summary>
    public partial class CountryModel : BaseNopEntityModel, ILocalizedModel<CountryLocalizedModel>
    {
        #region Ctor

        public CountryModel()
        {
            Locales = new List<CountryLocalizedModel>();
            StateProvinceSearchModel = new StateProvinceSearchModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.TwoLetterIsoCode")]
        public string TwoLetterIsoCode { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.ThreeLetterIsoCode")]
        public string ThreeLetterIsoCode { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.NumericIsoCode")]
        public int NumericIsoCode { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.SubjectToVat")]
        public bool SubjectToVat { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.NumberOfStates")]
        public int NumberOfStates { get; set; }

        public IList<CountryLocalizedModel> Locales { get; set; }

        public StateProvinceSearchModel StateProvinceSearchModel { get; set; }

        #endregion
    }

    public partial class CountryLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        public string Name { get; set; }
    }
}