﻿using Core.Primitives;
using InternalServices.EreminModel;
using Serialization;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace InternalServices
{
    public class EreminService
    {
        const string URL = "http://models-manager.evarun.ru";

        public static CharacterModel GetCharacter(int characterId)
        {
            var client = new HttpClient();
            var url = $"{URL}/character/model/{characterId}";
            var response = client.GetAsync(url).Result;
            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var model = Serializer.Deserialize<CharacterModel>(response.Content.ReadAsStringAsync().Result);
                return model; 
            }
            throw new Exception(response.Content.ReadAsStringAsync().Result);
        }

        public static bool GetAnonimous(int characterId)
        {
            var model = GetCharacter(characterId);
            if (model != null)
            {
                return model.workModel.billing.anonymous;
            }
            return false;
        }

        public static decimal GetDiscount(int characterId, DiscountType discountType)
        {
            var model = GetCharacter(characterId);
            if (model != null)
            {
                var gesheft = ParseToDecimal(model.workModel.discounts.all);
                decimal samurai = 0;
                if(discountType == DiscountType.Samurai)
                    samurai = ParseToDecimal(model.workModel.discounts.samurai);
                return gesheft > samurai ? gesheft : samurai;
            }
            return 0;
        }

        private static decimal ParseToDecimal(string value)
        {
            decimal dec = 0;
            decimal.TryParse(value, out dec);
            return dec;
        }


    }
}
