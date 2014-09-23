﻿using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment.Extensions;
using Orchard.Environment.Features;
using OShop.Models;
using OShop.Services;
using OShop.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OShop.Drivers {
    [OrchardFeature("OShop.Shipping")]
    public class ShippingProviderPartDriver : ContentPartDriver<ShippingProviderPart> {
        private readonly ICurrencyProvider _currencyProvider;

        private const string TemplateName = "Parts/ShippingProvider";

        public ShippingProviderPartDriver(ICurrencyProvider currencyProvider) {
            _currencyProvider = currencyProvider;
        }

        protected override string Prefix { get { return "ShippingProvider"; } }

        // GET
        protected override DriverResult Editor(ShippingProviderPart part, dynamic shapeHelper) {
            var model = new ShippingProviderPartEditViewModel() {
                NumberFormat = _currencyProvider.NumberFormat,
                Id = part.Id,
                Options = part.Options
            };

            return ContentShape("Parts_ShippingProvider_Edit",
                () => shapeHelper.EditorTemplate(
                    TemplateName: TemplateName,
                    Model: model,
                    Prefix: Prefix));
        }

        // POST
        protected override DriverResult Editor(ShippingProviderPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}