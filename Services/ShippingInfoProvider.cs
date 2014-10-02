﻿using Orchard.Environment.Extensions;
using Orchard.ContentManagement;
using OShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OShop.Services {
    [OrchardFeature("OShop.Shipping")]
    public class ShippingInfoProvider : IShoppingCartResolver {

        public Int32 Priority {
            get { return 50; }
        }

        public void ResolveCart(ref ShoppingCart Cart) {
            foreach (var item in Cart.Items) {
                if (item.Item.Content != null) {
                    var shippingPart = item.Item.Content.As<ShippingPart>();
                    if (shippingPart != null) {
                        item.ShippingInfo = shippingPart;
                    }
                }
            }
        }
    }
}