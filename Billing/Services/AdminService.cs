﻿using Billing.Dto;
using Billing.Dto.Shop;
using Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Billing.Services
{
    public class AdminService : BaseService
    {
        public SessionDto GetSessionInfo(int character)
        {
            var result = new SessionDto();
            var ls = BillingHelper.GetLifeStyleDto();
            result.LifeStyle = new SessionDto.LifeStyleDto(ls);
            var cycle = Factory.Job.GetLastCycle();
            result.Cycle = new SessionDto.CycleDto(cycle);
            var beatCharacters = Factory.Job.GetLastBeatAsNoTracking(Core.Primitives.BeatTypes.Characters);
            var jsoncharacters = Factory.Settings.GetValue(Core.Primitives.SystemSettingsEnum.beat_characters_dto);
            var lsDto = Serialization.Serializer.Deserialize<JobLifeStyleDto>(jsoncharacters);
            result.BeatCharacters = new SessionDto.BeatCharactersDto(beatCharacters, lsDto);
            result.Deploy = Environment.GetEnvironmentVariable(SystemHelper.Billing);
            var sin = Factory.Billing.GetSINByModelId(character, s=>s.Passport);
            result.PersonName = BillingHelper.GetPassportName(sin.Passport);
            return result;
        }

    }
}
