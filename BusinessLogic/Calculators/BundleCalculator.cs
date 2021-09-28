﻿using BusinessLogic.DTO;
using DataAccess.Entities;
using DataAccess.Enums;
using System.Collections.Generic;
using System.Linq;

namespace BusinessLogic.Calculators
{
    public static class BundleCalculator
    {
        public static CheckoutSummary Calculate(CheckoutSummary checkoutSummary, List<OrderItem> orderItems, Promotion promotion)
        {
            var bundleCount = orderItems.Count > 1 ? orderItems.Min(x => x.Quantity) : 0;
            var bundleModulusItems = new List<OrderItem>();
            var bundleItemFit = new List<OrderItem>();

            if (orderItems.Count > 1)
            {
                foreach (var item in orderItems)
                {
                    var bundleItemModulus = item.Quantity - bundleCount;
                    var bundleItemCount = item.Quantity - bundleItemModulus;

                    if (bundleItemModulus != 0)
                    {
                        bundleModulusItems.Add(new OrderItem { Price = item.Price, SKU = item.SKU, Quantity = bundleItemModulus });
                    }

                    bundleItemFit.Add(new OrderItem { Price = item.Price, SKU = item.SKU, Quantity = bundleItemCount });
                }
            }
            else
            {
                bundleModulusItems.Add(new OrderItem { Price = orderItems.FirstOrDefault().Price, SKU = orderItems.FirstOrDefault().SKU, Quantity = orderItems.FirstOrDefault().Quantity });
            }


            if (bundleCount > 0)
            {
                var priceAfterDiscount = 0.0;
                var priceBeforeDiscount = 0.0;
                if (bundleCount > 0)
                {
                    if (promotion.DiscountType == DiscountType.FixedPrice)
                    {
                        priceAfterDiscount = promotion.FixedPriceDiscount * bundleCount;
                    }
                    else if (promotion.DiscountType == DiscountType.Percentage)
                    {
                        priceBeforeDiscount = bundleItemFit.Sum(item => item.Price * item.Quantity);
                        priceAfterDiscount = priceBeforeDiscount - priceBeforeDiscount * promotion.PercentageDiscount / 100;
                    }
                }

                checkoutSummary.CombinationBundleItems.Add(new CombinationBundleItem
                {
                    BundleCount = orderItems.Sum(x => x.Quantity),
                    SKUs = orderItems.Select(x => x.SKU).ToList(),
                    PromotionDiscout = priceBeforeDiscount - priceAfterDiscount,
                    Amount = priceAfterDiscount
                });
            }

            if (bundleModulusItems.Count > 0)
            {
                foreach (var item in bundleModulusItems)
                {
                    checkoutSummary.SingleItems.Add(new SingleItem
                    {
                        ItemCount = item.Quantity,
                        PricePerItem = item.Price,
                        TotalPrice = item.Price * item.Quantity,
                        SKU = item.SKU
                    });
                }
            }

            return checkoutSummary;
        }
    }
}