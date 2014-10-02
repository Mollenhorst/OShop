﻿using Orchard.Data;
using Orchard.Environment.Extensions;
using OShop.Models;
using OShop.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OShop.Services {
    [OrchardFeature("OShop.Shipping")]
    public class ShippingService : IShippingService {
        private readonly IRepository<ShippingZoneRecord> _zoneRepository;
        private readonly IRepository<LocationsCountryRecord> _countryRepository;
        private readonly IRepository<LocationsStateRecord> _stateRepository;
        private readonly IRepository<ShippingOptionRecord> _optionRepository;

        public ShippingService(
            IRepository<ShippingZoneRecord> zoneRepository,
            IRepository<LocationsCountryRecord> countryRepository,
            IRepository<LocationsStateRecord> stateRepository,
            IRepository<ShippingOptionRecord> optionRepository) {
            _zoneRepository = zoneRepository;
            _countryRepository = countryRepository;
            _stateRepository = stateRepository;
            _optionRepository = optionRepository;
        }

        #region Shipping zones
        public void CreateZone(ShippingZoneRecord record) {
            _zoneRepository.Create(record);
        }

        public void UpdateZone(ShippingZoneRecord record) {
            _zoneRepository.Update(record);
        }

        public void DeleteZone(int ZoneId) {
            DeleteZone(GetZone(ZoneId));
        }

        public void DeleteZone(ShippingZoneRecord record) {
            if (record != null) {
                foreach (var country in _countryRepository.Fetch(c => c.ShippingZoneRecord != null && c.ShippingZoneRecord.Id == record.Id)) {
                    country.ShippingZoneRecord = null;
                };
                foreach (var state in _stateRepository.Fetch(s => s.ShippingZoneRecord != null && s.ShippingZoneRecord.Id == record.Id)) {
                    state.ShippingZoneRecord = null;
                }
                foreach (var option in _optionRepository.Fetch(o => o.ShippingZoneRecord != null && o.ShippingZoneRecord.Id == record.Id)) {
                    option.ShippingZoneRecord = null;
                }

                _zoneRepository.Delete(record);
            }
        }

        public ShippingZoneRecord GetZone(int Id) {
            return _zoneRepository.Get(Id);
        }

        public IEnumerable<ShippingZoneRecord> GetZones() {
            return _zoneRepository.Table.OrderBy(z => z.Name);
        }

        public IEnumerable<ShippingZoneRecord> GetEnabledZones() {
            return _zoneRepository.Fetch(z => z.Enabled).OrderBy(z => z.Name);
        }
        
        #endregion

        #region Shipping options
        public void CreateOption(ShippingOptionRecord record) {
            _optionRepository.Create(record);
        }

        public void UpdateOption(ShippingOptionRecord record) {
            _optionRepository.Update(record);
        }

        public void DeleteOption(int OptionId) {
            DeleteOption(GetOption(OptionId));
        }

        public void DeleteOption(ShippingOptionRecord record) {
            _optionRepository.Delete(record);
        }

        public ShippingOptionRecord GetOption(int Id) {
            return _optionRepository.Get(Id);
        }

        public IEnumerable<ShippingOptionRecord> GetOptions(ShippingProviderPart part) {
            return _optionRepository.Fetch(o => o.ShippingProviderId == part.Id);
        }

        public ShippingOptionRecord GetSuitableOption(int ShippingProviderId, ShippingZoneRecord zone, ShoppingCart cart) {
            if (!zone.Enabled) {
                return null;
            }

            return _optionRepository
                .Fetch(o => o.ShippingProviderId == ShippingProviderId && o.ShippingZoneRecord == zone && o.Enabled)
                .OrderByDescending(o => o.Priority)
                .Where(o => MeetsContraints(o, cart))
                .FirstOrDefault();
        }

        #endregion

        private bool MeetsContraints(ShippingOptionRecord option, ShoppingCart cart) {
            foreach (var contraint in option.Contraints) {
                double propertyValue = EvalProperty(contraint.Property, cart);
                switch (contraint.Operator) {
                    case ShippingContraintOperator.LessThan:
                        if (contraint.Value <= propertyValue)
                            return false;
                        break;
                    case ShippingContraintOperator.LessThanOrEqual:
                        if (contraint.Value < propertyValue)
                            return false;
                        break;
                    case ShippingContraintOperator.Equal:
                        if (contraint.Value != propertyValue)
                            return false;
                        break;
                    case ShippingContraintOperator.GreaterThan:
                        if (contraint.Value >= propertyValue)
                            return false;
                        break;
                    case ShippingContraintOperator.GreaterThanOrEqual:
                        if (contraint.Value > propertyValue)
                            return false;
                        break;
                    case ShippingContraintOperator.NotEqual:
                        if (contraint.Value == propertyValue)
                            return false;
                        break;
                }
            }

            return true;
        }

        private double EvalProperty(ShippingContraintProperty property, ShoppingCart cart) {
            var shippingInfos = cart.Items.Where(i => i.ShippingInfo != null && i.ShippingInfo.RequiresShipping);
            switch (property) {
                case ShippingContraintProperty.TotalPrice:
                    return Convert.ToDouble(cart.Items.Total());
                case ShippingContraintProperty.TotalWeight:
                    return shippingInfos.Sum(i => i.Quantity * i.ShippingInfo.Weight);
                case ShippingContraintProperty.TotalVolume:
                    return shippingInfos.Sum(i => i.Quantity * i.ShippingInfo.Length * i.ShippingInfo.Width * i.ShippingInfo.Height);
                case ShippingContraintProperty.ItemLongestDimension:
                    return shippingInfos.Max(i => new double[] { i.ShippingInfo.Length, i.ShippingInfo.Width, i.ShippingInfo.Height }.Max());
                case ShippingContraintProperty.MaxItemLength:
                    return shippingInfos.Max(i => i.ShippingInfo.Length);
                case ShippingContraintProperty.MaxItemWidth:
                    return shippingInfos.Max(i => i.ShippingInfo.Width);
                case ShippingContraintProperty.MaxItemHeight:
                    return shippingInfos.Max(i => i.ShippingInfo.Height);
                default:
                    return 0;
            }
        }

    }
}