﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Billing;
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
        private readonly Lazy<IBillingManager> _manager = new Lazy<IBillingManager>(IocContainer.Get<IBillingManager>);

        #region admin
        [HttpGet("createphysicalwallet")]
        public DataResult<SINDetails> CreatePhysicalWallet(string sin, decimal balance)
        {
            var manager = _manager.Value;
            var result = RunAction(() => manager.CreatePhysicalWallet(sin, balance));
            return result;
        }
        #endregion

        #region transfer
        [HttpGet("transfer/maketransfersinsin")]
        public DataResult<Transfer> MakeTransferSINSIN(int sin1, int sin2, decimal amount, string comment)
        {
            var manager = _manager.Value;
            var result = RunAction(() => manager.MakeTransferSINSIN(sin1, sin2, amount, comment));
            return result;
        }
        [HttpGet("transfer/maketransfersinleg")]
        public DataResult<Transfer> MakeTransferSINLeg(int sin, int leg, decimal amount, string comment)
        {
            var manager = _manager.Value;
            var result = RunAction(() => manager.MakeTransferSINLeg(sin, leg, amount, comment));
            return result;
        }
        [HttpGet("transfer/maketransferlegsin")]
        public DataResult<Transfer> MakeTransferLegSIN(int leg, int sin, decimal amount, string comment)
        {
            var manager = _manager.Value;
            var result = RunAction(() => manager.MakeTransferSINLeg(leg, sin, amount, comment));
            return result;
        }
        [HttpGet("transfer/maketransferlegleg")]
        public DataResult<Transfer> MakeTransferLegLeg(int leg1, int leg2, decimal amount, string comment)
        {
            var manager = _manager.Value;
            var result = RunAction(() => manager.MakeTransferSINLeg(leg1, leg2, amount, comment));
            return result;
        }
        #endregion
        #region renta
        [HttpGet("renta/createprice")]
        public Result CreatePrice()
        {
            throw new NotImplementedException();
        }
        [HttpGet("renta/createpricebyparams ")]
        public Result CreatePriceByParams()
        {
            throw new NotImplementedException();
        }
        [HttpGet("renta/confirmrenta ")]
        public Result ConfirmRenta()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region info
        [HttpGet("info/base")]
        public Result Base(string id)
        {
            throw new NotImplementedException();
        }
        [HttpGet("info/advanced")]
        public Result Advanced(string id)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}