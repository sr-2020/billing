﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Billing;
using Billing.Dto;
using Billing.Dto.Shop;
using Billing.DTO;
using BillingAPI.Filters;
using BillingAPI.Model;
using Core.Model;
using IoC;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BillingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BillingController : EvarunApiController
    {
        #region refactored
        /// <summary>
        /// Get base info for current character
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        [HttpGet("info/getbalance")]
        public DataResult<BalanceDto> GetBalance(int characterId)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.GetBalance(characterId), $"GetBalance {characterId}");
            return result;
        }

        [HttpGet("getshop")]
        [AdminAuthorization]
        public DataResult<ShopDto> GetShop(int shopId)
        {
            var manager = IocContainer.Get<IShopManager>();
            var result = RunAction(() => manager.GetShops(s => s.Id == shopId).FirstOrDefault());
            return result;
        }


        [HttpGet("getnomenklaturas")]
        [AdminAuthorization]
        public DataResult<List<NomenklaturaDto>> GetNomenklaturas()
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.GetNomenklaturas(0,0), $"getnomenklaturas");
            return result;
        }

        //[HttpPut("admin/createorupdateshopwallet")]
        //[AdminAuthorization]
        //public DataResult<ShopWallet> CreateOrUpdateShopWallet([FromBody] CreateShopModel request)
        //{
        //    var manager = IocContainer.Get<IBillingManager>();
        //    var result = RunAction(() => manager.CreateOrUpdateShopWallet(foreignId, amount, name, lifestyle, owner), $"createorupdateshopwallet {foreignId} {amount} {name} {lifestyle}");
        //    return result;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet("getcharacters")]
        [AdminAuthorization]
        public DataResult<List<CharacterDto>> GetCharacters()
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.GetCharactersInGame(), $"getcharacters ");
            return result;
        }

        #endregion




        #region admin

        /// <summary>
        /// Fill all tables related with wallet for current character
        /// </summary>
        /// <param name="character">ID from table Character</param>
        /// <param name="balance">initial wallet amount</param>
        /// <returns></returns>
        [HttpGet("admin/createphysicalwallet")]
        [Obsolete]
        public DataResult<SIN> CreatePhysicalWallet(int character, decimal balance)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.CreateOrUpdatePhysicalWallet(character, "", null, balance), $"createphysicalwallet {character} {balance}");
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelid"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("admin/initcharacter/{modelid}")]
        public DataResult<SIN> InitCharacter(int modelid, [FromBody] InitCharacterRequest request)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.InitCharacter(modelid, request.Name, request.Metarace), $"InitCharacter: {modelid};{request.Name};{request.Metarace};{request.Karma}");
            return result;
        }
        [HttpPost("createtransfermir")]
        [AdminAuthorization]
        public DataResult<Transfer> CreateTransferMIR([FromBody] CreateTransferSinSinRequest request)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.CreateTransferMIRSIN(request.CharacterTo, request.Amount), "createtransfermir");
            return result;
        }



        /// <summary>
        /// Create or update allowed product type
        /// </summary>
        /// <param name="id">0 for create new, specified for update</param>
        /// <param name="name">short description</param>
        /// <param name="discounttype">1 - for gesheftmaher only, 2 - for samurai too(weapons)</param>
        /// <returns></returns>
        [HttpPut("admin/createorupdateproduct")]
        public DataResult<ProductType> CreateOrUpdateProductType(int id, string name, int discounttype)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.CreateOrUpdateProductType(id, name, discounttype), $"createorupdateproduct {id}:{name}:{discounttype}");
            return result;
        }

        /// <summary>
        /// Create or update allowed nomenklatura
        /// </summary>
        /// <param name="id">0 for create new, specified for update</param>
        /// <param name="name">Header of product</param>
        /// <param name="code">it will be executed when user byu sku of this nomenklatura</param>
        /// <param name="specialisationId">id from getproducttypes</param>
        /// <param name="lifestyle">from 1 to 6</param>
        /// <param name="baseprice">decimal base price</param>
        /// <param name="description">description shown for user</param>
        /// <param name="pictureurl">url for picture</param>
        /// <returns></returns>
        [HttpPut("admin/createorupdatenomenklatura")]
        public DataResult<Nomenklatura> CreateOrUpdateNomenklatura(int id, string name, string code, int specialisationId, int lifestyle, decimal baseprice, string description, string pictureurl)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.CreateOrUpdateNomenklatura(id, name, code, specialisationId, lifestyle, baseprice, description, pictureurl), $"createorupdatenomenklatura {id}:{name}:{code}:{specialisationId}:{lifestyle}:{baseprice}:{description}:{pictureurl}");
            return result;
        }

        /// <summary>
        /// Create or update allowed sku
        /// </summary>
        /// <param name="id">0 for create new, specified for update</param>
        /// <param name="nomenklatura">id from getnomenklaturas</param>
        /// <param name="count">count of this item, minimum 1</param>
        /// <param name="corporation">id from getcorps</param>
        /// <param name="name">header</param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        [HttpPut("admin/createorupdatesku")]
        public DataResult<Sku> CreateOrUpdateSku(int id, int nomenklatura, int count, int corporation, string name, bool enabled)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.CreateOrUpdateSku(id, nomenklatura, count, corporation, name, enabled), $"CreateOrUpdateSku {id}:{name}:{nomenklatura}:{count}:{corporation}:{name}:{enabled}");
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="corpid">id for delete</param>
        /// <returns></returns>
        [HttpDelete("admin/deletecorporation")]
        public Result DeleteCorporation(int corpid)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.DeleteCorporation(corpid), $"deletecorporation {corpid}");
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shopid">id for delete</param>
        /// <returns></returns>
        [HttpDelete("admin/deleteshop")]
        public Result DeleteShop(int shopid)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.DeleteShop(shopid), $"deleteshop {shopid}");
            return result;
        }

        /// <summary>
        /// Get corporation wallet. If wallet not exists, then create it
        /// </summary>
        /// <param name="id">0 for create new, specified for update</param>
        /// <param name="amount">if negative then amount will not change</param>
        /// <param name="name">Some name</param>
        /// <param name="logoUrl">Url to picture</param>
        /// <returns></returns>
        [HttpPut("admin/createorupdatecorporationwallet")]
        public DataResult<CorporationWallet> CreateOrUpdateCorporationWallet(int id, decimal amount, string name, string logoUrl)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.CreateOrUpdateCorporationWallet(id, amount, name, logoUrl), $"createorupdatecorporationwallet {id}:{amount}:{name}:{logoUrl}");
            return result;
        }





        #endregion

        #region transfer

        /// <summary>
        /// Create transfer from Character1 to Character2 using sins
        /// </summary>
        /// <returns></returns>
        [HttpPost("transfer/createtransfersinsin")]
        public DataResult<Transfer> CreateTransferSINSIN(int character, [FromBody] CreateTransferSinSinRequest request)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.CreateTransferSINSIN(character.ToString() , request.CharacterTo, request.Amount, request.Comment), "transfer/createtransfersinsin");
            return result;
        }

        /// <summary>
        /// Create transfer from Character1 to Character2 using sins
        /// </summary>
        /// <param name="character1">ID from table Character</param>
        /// <param name="character2">ID from table Character</param>
        /// <param name="amount"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        [Obsolete]
        [HttpGet("transfer/maketransfersinsin")]
        public DataResult<Transfer> MakeTransferSINSIN(int character1, int character2, decimal amount, string comment)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.MakeTransferSINSIN(character1, character2, amount, comment), "transfer/maketransfersinsin");
            return result;
        }
        [HttpGet("transfer/maketransfersinleg")]
        public DataResult<Transfer> MakeTransferSINLeg(int sin, int leg, decimal amount, string comment)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.MakeTransferSINLeg(sin, leg, amount, comment), "transfer/maketransfersinleg");
            return result;
        }
        [HttpPost("transfer/maketransferlegsin")]
        public DataResult<Transfer> MakeTransferLegSIN(int leg, int sin, decimal amount, string comment)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.MakeTransferSINLeg(leg, sin, amount, comment), "transfer/maketransferlegsin");
            return result;
        }
        [HttpPost("transfer/maketransferlegleg")]
        public DataResult<Transfer> MakeTransferLegLeg(int leg1, int leg2, decimal amount, string comment)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.MakeTransferSINLeg(leg1, leg2, amount, comment), "transfer/maketransferlegleg");
            return result;
        }
        #endregion

        #region renta

        [HttpPost("renta/createcontract")]
        public DataResult<Contract> CreateContract(int corporation, int shop)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.CreateContract(corporation, shop), $"CreateContract {corporation} {shop}");
            return result;
        }

        [HttpDelete("renta/breakcontract")]
        public Result BreakContract(int corporation, int shop)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.BreakContract(corporation, shop), $"BreakContract {corporation} {shop}");
            return result;
        }
        #endregion

        #region info

        [HttpGet("info/getcontracts")]
        public DataResult<List<Contract>> GetContrats(int shopid, int corporationId)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.GetContrats(shopid, corporationId), $"GetContrats {shopid}:{corporationId}");
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="corporationId"></param>
        /// <param name="nomenklaturaId"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        [HttpGet("info/getskus")]
        public DataResult<List<SkuDto>> GetSkus(int corporationId, int nomenklaturaId, bool? enabled)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.GetSkus(corporationId, nomenklaturaId, enabled), $"getskus {corporationId}:{nomenklaturaId}:{enabled}");
            return result;
        }



        /// <summary>
        /// Get all rentas for current character
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        [HttpGet("info/getrentas")]
        public DataResult<List<RentaDto>> GetRentas(int character)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.GetRentas(character), $"getrentas {character}");
            return result;
        }



        /// <summary>
        /// Get all corporations
        /// </summary>
        /// <returns></returns>
        [HttpGet("info/getcorps")]
        public DataResult<List<CorporationDto>> GetCorps()
        {
            var manager = IocContainer.Get<IShopManager>();
            var result = RunAction(() => manager.GetCorporations(s => true), "getcorps");
            return result;
        }

        [HttpGet("info/getcharacteridbysin")]
        public DataResult<int> GetCharacterIdBySin(string sinString)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.GetModelIdBySinString(sinString), "info/getcharacteridbysin");
            return result;
        }

        [HttpGet("info/getsinbycharacterId")]
        public DataResult<string> GetSinByCharacter(int characterId)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.GetSinStringByCharacter(characterId), "info/getsinbycharacterId");
            return result;
        }

        /// <summary>
        /// Get all transfers(income and outcome) for current character
        /// </summary>
        /// <param name="characterId"></param>
        /// <returns></returns>
        [HttpGet("info/gettransfers")]
        public DataResult<List<TransferDto>> GetTransfers(int characterId)
        {
            var manager = IocContainer.Get<IBillingManager>();
            var result = RunAction(() => manager.GetTransfers(characterId), $"gettransfers {characterId}");
            return result;
        }

        #endregion

        #region test


        #endregion
    }
}