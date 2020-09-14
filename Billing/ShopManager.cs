﻿using Billing.Dto.Shop;
using Billing.DTO;
using Core;
using Core.Model;
using Core.Primitives;
using InternalServices;
using IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Billing
{
    public interface IShopManager : IBaseRepository
    {
        bool HasAccessToShop(int character, int shop);
        List<ShopDto> GetShops(Expression<Func<ShopWallet, bool>> predicate);
        List<CorporationDto> GetCorporations(Expression<Func<CorporationWallet, bool>> predicate);
        List<QRDto> GetAvailableQR(int shop);
        OrganisationViewModel GetAvailableOrganisations(int modelId);
        string GetCharacterName(int modelId);
        List<TransferDto> GetTransfers(int shop);
        Transfer MakeTransferLegSIN(int legFrom, int sinTo, decimal amount, string comment);
        Transfer MakeTransferLegLeg(int legFrom, int legTo, decimal amount, string comment);
        List<RentaDto> GetRentas(int shop);
        void WriteRenta(int rentaId, string qrEncoded);
    }

    public class ShopManager : BaseBillingRepository, IShopManager
    {
        public void WriteRenta(int rentaId, string qrEncoded)
        {
            var qr = EreminQrService.GetPayload(qrEncoded);
            var renta = Get<Renta>(p => p.Id == rentaId && p.HasQRWrite && string.IsNullOrEmpty(p.QRRecorded), r => r.Sku.Nomenklatura);
            if (renta == null)
                throw new ShopException($"offer {rentaId} записать на qr невозможно");
            var code = renta.Sku.Nomenklatura.Code;
            var name = renta.Sku.Name;
            var description = renta.Sku.Nomenklatura.Description;
            //TODO
            var count = 1;
            if (!EreminService.WriteQR(qr, code, name, description, count, renta.BasePrice, BillingHelper.GetFinalPrice(renta.BasePrice, renta.Discount, renta.CurrentScoring), renta.Secret, rentaId, (Lifestyles)renta.LifeStyle))
            {
                throw new ShopException("запись на qr не получилось");
            }
            renta.QRRecorded = qr;
            Add(renta);
            Context.SaveChanges();
        }

        public List<RentaDto> GetRentas(int shop)
        {
            var list = GetList<Renta>(r => r.ShopId == shop, r => r.Sku.Nomenklatura.ProductType, r => r.Sku.Corporation, r => r.Shop, r => r.Sin);
            return list.OrderByDescending(r => r.DateCreated)
                    .Select(r =>
                    new RentaDto
                    {
                        FinalPrice = BillingHelper.GetFinalPrice(r.BasePrice, r.Discount, r.CurrentScoring),
                        ProductType = r.Sku.Nomenklatura.ProductType.Name,
                        Shop = r.Shop.Name,
                        NomenklaturaName = r.Sku.Nomenklatura.Name,
                        SkuName = r.Sku.Name,
                        Corporation = r.Sku.Corporation.Name,
                        HasQRWrite = r.HasQRWrite,
                        QRRecorded = r.QRRecorded,
                        PriceId = r.PriceId,
                        RentaId = r.Id,
                        DateCreated = r.DateCreated,
                        CharacterName = r.Sin.PersonName
                    }).ToList();
        }

        [BillingBlock]
        public Transfer MakeTransferLegLeg(int shopFrom, int shopTo, decimal amount, string comment)
        {
            var shopWalletFrom = Get<ShopWallet>(s => s.Id == shopFrom, s => s.Wallet);
            var shopWalletTo = Get<ShopWallet>(s => s.Id == shopTo, s => s.Wallet);
            var transfer = MakeNewTransfer(shopWalletFrom.Wallet, shopWalletTo.Wallet, amount, comment);
            return transfer;
        }

        [BillingBlock]
        public Transfer MakeTransferLegSIN(int shop, int character, decimal amount, string comment)
        {
            var sin = GetSINByModelId(character, s => s.Wallet);
            var anon = false;
            try
            {
                anon = EreminService.GetAnonimous(character);
            }
            catch (Exception e)
            {

            }
            var shopWallet = Get<ShopWallet>(s => s.Id == shop, s => s.Wallet);
            var transfer = MakeNewTransfer(shopWallet.Wallet, sin.Wallet, amount, comment, anon);
            return transfer;
        }

        public List<TransferDto> GetTransfers(int shop)
        {
            var shopWallet = Get<ShopWallet>(s => s.Id == shop, s => s.Wallet);
            var listFrom = GetList<Transfer>(t => t.WalletFromId == shopWallet.WalletId, t => t.WalletFrom, t => t.WalletTo);

            var allList = new List<TransferDto>();
            var owner = GetWalletName(shopWallet.Wallet);
            if (listFrom != null)
                allList.AddRange(listFrom
                    .Select(s => CreateTransferDto(s, TransferType.Outcoming, owner))
                    .ToList());
            var listTo = GetList<Transfer>(t => t.WalletToId == shopWallet.WalletId, t => t.WalletFrom, t => t.WalletTo);
            if (listTo != null)
                allList.AddRange(listTo
                    .Select(s => CreateTransferDto(s, TransferType.Incoming, owner))
                    .ToList());
            return allList.OrderByDescending(t => t.OperationTime).ToList();
        }

        public OrganisationViewModel GetAvailableOrganisations(int modelId)
        {
            var isAdmin = BillingHelper.IsAdmin(modelId);
            var model = new OrganisationViewModel
            {
                CurrentModelId = modelId,
                CurrentCharacterName = GetCharacterName(modelId)
            };
            model.Shops = GetShops(s => s.Owner == modelId || isAdmin);
            model.Corporations = GetCorporations(s => s.Owner == modelId || isAdmin);
            return model;
        }

        public string GetCharacterName(int modelId)
        {
            var character = Get<SIN>(s => s.Character.Model == modelId);
            return character?.PersonName;
        }

        public bool HasAccessToShop(int character, int shopId)
        {
            if (character == 0)
            {
                throw new BillingAuthException("Character not authorized");
            }
            var shop = Get<ShopWallet>(s => s.Id == shopId);
            if (shop == null)
            {
                throw new BillingException("shop not found");
            }
            return shop.Owner == character;
        }

        public List<ShopDto> GetShops(Expression<Func<ShopWallet, bool>> predicate)
        {
            return GetList(predicate, new string[] { "Wallet", "Specialisations", "Specialisations.ProductType" }).Select(s =>
                     new ShopDto()
                     {
                         Id = s.Id,
                         Name = s.Name,
                         OwnerId = s.Owner,
                         Comission = s.Commission,
                         Lifestyle = ((Lifestyles)s.LifeStyle).ToString(),
                         Balance = BillingHelper.RoundDown(s.Wallet.Balance),
                         Specialisations = CreateSpecialisationDto(s)
                     }).ToList();
        }

        public List<CorporationDto> GetCorporations(Expression<Func<CorporationWallet, bool>> predicate)
        {
            return GetList(predicate, c => c.Wallet).Select(c =>
                  new CorporationDto
                  {
                      Id = c.Id,
                      Name = c.Name,
                      OwnerId = c.Owner,
                      CorporationUrl = c.CorporationLogoUrl
                  }).ToList();
        }

        public List<QRDto> GetAvailableQR(int shop)
        {
            var skus = GetSkusForShop(shop);
            var qrs = new List<QRDto>();
            foreach (var sku in skus)
            {
                var qr = CreateQRDto(shop, sku);
                qrs.Add(qr);
            }
            return qrs;
        }

        #region private

        private QRDto CreateQRDto(int shop, SkuDto sku)
        {
            var qr = new QRDto();
            qr.Shop = shop;
            qr.Sku = sku;
            qr.QRID = QRHelper.Concatenate(sku.SkuId, shop);
            var cache = Get<CacheQRContent>(q => q.QRID == qr.QRID);
            if (cache == null)
            {
                cache = new CacheQRContent
                {
                    QRID = qr.QRID,
                    Encoded = EreminQrService.GetQRUrl(qr.QRID)
                };
                Add(cache);
                Context.SaveChanges();
            }
            qr.QR = cache.Encoded;
            return qr;
        }

        private List<SpecialisationDto> CreateSpecialisationDto(ShopWallet shop)
        {
            var list = new List<SpecialisationDto>();
            if (shop.Specialisations == null)
                return list;
            list.AddRange(shop.Specialisations.Select(s => new SpecialisationDto
            {
                ProductTypeId = s.ProductTypeId,
                ProductTypeName = s.ProductType?.Name,
                ShopId = s.ShopId,
                ShopName = shop.Name
            }));
            return list;
        }
        private List<SkuDto> GetSkusForShop(int shop)
        {
            return GetSkuList(shop).Select(s => new SkuDto(s)).ToList();
        }
        #endregion
    }
}
