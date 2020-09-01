﻿using Billing.Dto.Shop;
using Core.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Billing.DTO
{
    public class ShopDto : OrganisationBase
    {
        public decimal Comission { get; set; }
        public string Lifestyle { get; set; }
        public decimal Balance { get; set; }
        public List<SpecialisationDto> Specialisations { get; set; }
    }
}
