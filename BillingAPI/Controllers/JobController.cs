﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Billing;
using BillingAPI.Filters;
using BillingAPI.Model;
using Core;
using Core.Model;
using Core.Primitives;
using Hangfire;
using IoC;
using Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BillingAPI.Controllers
{
    [Route("[controller]")]
    public class JobController : EvarunApiController
    {
        private readonly IJobManager Manager = IocContainer.Get<IJobManager>();


        [HttpGet("cycle")]
        //[CheckSecret]
        public Result ProcessCycle()
        {
            var result = RunAction(() => 
            {
                var life = new JobLifeService();
                life.ToggleCycle();
            }, $"period");
            return result;
        }

        [HttpGet("beatcharacters")]
        //[CheckSecret]
        public DataResult<string> ProcessBeat()
        {
            var result = RunAction(() =>
            {
                var life = new JobLifeService();
                return life.DoBeat("test", BeatTypes.Characters);
            }, $"period");
            return result;
        }


    }
}