﻿using Billing;
using Billing.Dto;
using Core;
using Core.Model;
using Core.Primitives;
using Dapper;
using IoC;
using Microsoft.EntityFrameworkCore;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Scoringspace
{
    public interface IScoringManager
    {
        Task OnLifeStyleChanged(Scoring scoring, Lifestyles from, Lifestyles to);
        Task OnPillConsumed(int model, string pillLifestyle);
        Task OnWounded(int model);
        Task OnClinicalDeath(int model);
        Task OnDumpshock(int model);
        Task OnFoodConsume(int model, string foodLifeStyle);
        Task OnOtherBuy(SIN sin, int lifestyle);
        Task OnMatrixBuy(SIN sin, int lifestyle);
        Task OnDroneBuy(SIN sin, int lifestyle);
        Task OnPillBuy(SIN sin, int lifestyle);
        Task OnWeaponBuy(SIN sin, int lifestyle);
        Task OnMagicBuy(SIN sin, int lifestyle);
        Task OnInsuranceBuy(SIN sin, int lifestyle);
        Task OnCharityBuy(SIN sin, int lifestyle);
        Task OnFoodBuy(SIN sin, int lifestyle);
        Task OnImplantBuy(SIN sin, int lifestyle);
        Task OnImplantInstalled(int model, string implantlifestyle, string autodoclifestyle);
        Task OnMetatypeChanged(SIN sin);
        ScoringDto GetFullScoring(int character);
    }

    public class ScoringManager : BaseEntityRepository, IScoringManager
    {
        #region implementation

        public async Task OnInsuranceBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.insurance);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }
        public async Task OnMatrixBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.buy_matrix);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }
        public async Task OnDroneBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.buy_drone);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }
        public async Task OnCharityBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.buy_charity);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }

        public async Task OnMetatypeChanged(SIN sin)
        {
            var factorId = GetFactorId(ScoringFactorEnum.metatype);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == sin.Passport.MetatypeId);
                return value?.Value ?? 1;
            });
        }

        public async Task OnImplantInstalled(int model, string implantlifestyle, string autodoclifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.implant_install);
            var scoring = GetScoringByModelId(model);
            if (!BillingHelper.LifestyleIsDefined(implantlifestyle) || !BillingHelper.LifestyleIsDefined(autodoclifestyle))
            {
                return;
            }
            var lifestyle = BillingHelper.GetLifestyle(implantlifestyle);
            await RaiseScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == (int)lifestyle);
                return value?.Value ?? 1;
            });
            factorId = GetFactorId(ScoringFactorEnum.where_implant_install);
            lifestyle = BillingHelper.GetLifestyle(autodoclifestyle);
            await RaiseScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == (int)lifestyle);
                return value?.Value ?? 1;
            });

        }

        public async Task OnImplantBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.buy_implant);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }

        public async Task OnOtherBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.buy_other);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }

        public async Task OnPillBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.buy_pill);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }

        public async Task OnWeaponBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.buy_weapon);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }

        public async Task OnMagicBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.buy_magic);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }

        public async Task OnFoodBuy(SIN sin, int lifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.buy_food);
            await RaiseScoringEvent(sin.ScoringId ?? 0, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == lifestyle);
                return value?.Value ?? 1;
            });
        }

        public async Task OnFoodConsume(int model, string foodLifeStyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.food_consume);
            if (!BillingHelper.LifestyleIsDefined(foodLifeStyle))
            {
                return;
            }
            var lifestyle = BillingHelper.GetLifestyle(foodLifeStyle);
            var scoring = GetScoringByModelId(model);
            await RaiseScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == (int)lifestyle);
                return value?.Value ?? 1;
            });
        }

        public async Task OnPillConsumed(int model, string pillLifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.pill_use);
            if (!BillingHelper.LifestyleIsDefined(pillLifestyle))
            {
                return;
            }
            var lifestyle = BillingHelper.GetLifestyle(pillLifestyle);
            var scoring = GetScoringByModelId(model);
            await RaiseScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == (int)lifestyle);
                return value?.Value ?? 1;
            });
        }

        public async Task OnWounded(int model)
        {
            var factorId = GetFactorId(ScoringFactorEnum.worse);
            var scoring = GetScoringByModelId(model);
            await RaiseScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == 1);
                return value?.Value ?? 1;
            });
        }

        public async Task OnClinicalDeath(int model)
        {
            var factorId = GetFactorId(ScoringFactorEnum.clinical_death);
            var scoring = GetScoringByModelId(model);
            await RaiseScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == 1);
                return value?.Value ?? 1;
            });
        }

        public async Task OnDumpshock(int model)
        {
            var factorId = GetFactorId(ScoringFactorEnum.dumpshock);
            var scoring = GetScoringByModelId(model);
            await RaiseScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == 1);
                return value?.Value ?? 1;
            });
        }

        public async Task OnLifeStyleChanged(Scoring scoring, Lifestyles from, Lifestyles to)
        {
            var factorId = GetFactorId(ScoringFactorEnum.ls_change);
            await RaiseScoringEvent(scoring.Id, factorId, (context) =>
             {
                 var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == ScoringHelper.GetEventNumberLifestyle(from, to));
                 return value?.Value ?? 1;
             });
        }
        #endregion

        public ScoringDto GetFullScoring(int character)
        {
            var sin = Get<SIN>(s => s.Character.Model == character, s => s.Scoring);
            var fixenum = (int)ScoringCategoryType.Fix;
            var relativenum = (int)ScoringCategoryType.Relative;

            var fixFactors = GetList<CurrentFactor>(f => f.CurrentCategory.Category.CategoryType == fixenum && f.CurrentCategory.ScoringId == sin.ScoringId, f => f.ScoringFactor, f => f.CurrentCategory.Category);
            var relativFactors = GetList<CurrentFactor>(f => f.CurrentCategory.Category.CategoryType == relativenum && f.CurrentCategory.ScoringId == sin.ScoringId, f => f.ScoringFactor, f => f.CurrentCategory.Category);
            var fixCategories = fixFactors
                .GroupBy(f => f.ScoringFactor.Category)
                .Select(g => new ScoringCategoryDto
                {
                    Name = g.Key.Name,
                    Value = Math.Round(g.FirstOrDefault()?.CurrentCategory?.Value ?? 0, 2),
                    Weight = g.Key.Weight,
                    Factors = g.Select(f => new ScoringFactorDto
                    {
                        Value = Math.Round(f.Value, 2),
                        Name = f.ScoringFactor.Name
                    }).ToList()
                }).ToList();
            var relativCategories = relativFactors
                .GroupBy(f => f.ScoringFactor.Category)
                .Select(g => new ScoringCategoryDto
                {
                    Name = g.Key.Name,
                    Value = Math.Round(g.FirstOrDefault()?.CurrentCategory?.Value ?? 0, 2),
                    Weight = g.Key.Weight,
                    Factors = g.Select(f => new ScoringFactorDto
                    {
                        Value = Math.Round(f.Value, 2),
                        Name = f.ScoringFactor.Name,
                    }).ToList()
                }).ToList();

            return new ScoringDto
            {
                Character = character,
                CurrentFix = Math.Round(sin.Scoring.CurrentFix, 2),
                CurrentRelative = Math.Round(sin.Scoring.CurerentRelative, 2),
                FixCategories = fixCategories,
                RelativeCategories = relativCategories
            };
        }

        #region mathematic

        private Task RaiseScoringEvent(int scoringId, int factorId, Func<BillingContext, decimal> action)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var context = new BillingContext())
                    {
                        using (var dbContextTransaction = context.Database.BeginTransaction())
                        {
                            var connection = context.Database.GetDbConnection();
                            var id = connection.QueryFirstOrDefault<int>($"SELECT id FROM scoring  WHERE id = {scoringId} FOR UPDATE;");//block scoring for updates
                            var start = DateTime.Now;
                            var lifestyle = action(context);
                            var factor = context.Set<ScoringFactor>().AsNoTracking().FirstOrDefault(f => f.Id == factorId);
                            var category = context.Set<ScoringCategory>().AsNoTracking().FirstOrDefault(f => f.Id == factor.CategoryId);
                            var scoring = context.Set<Scoring>().AsTracking().FirstOrDefault(s => s.Id == scoringId);
                            var systemsettings = IocContainer.Get<ISettingsManager>();
                            var oldScoring = scoring.CurerentRelative + scoring.CurrentFix;
                            var curCategory = context.Set<CurrentCategory>().AsNoTracking().FirstOrDefault(c => c.ScoringId == scoringId && c.CategoryId == factor.CategoryId);
                            if (curCategory == null)
                            {
                                curCategory = new CurrentCategory
                                {
                                    ScoringId = scoringId,
                                    CategoryId = factor.CategoryId
                                };
                                Add(curCategory, context);
                            }
                            var curFactor = context.Set<CurrentFactor>().AsNoTracking().FirstOrDefault(s => s.CurrentCategoryId == curCategory.Id && s.ScoringFactorId == factorId);
                            if (curFactor == null)
                            {
                                curFactor = new CurrentFactor
                                {
                                    ScoringFactorId = factorId,
                                    CurrentCategoryId = curCategory.Id,
                                    Value = scoring.StartFactor ?? 1
                                };
                                Add(curFactor, context);
                            }
                            var oldFactorValue = curFactor.Value;
                            var newValue = CalculateFactor((double)lifestyle, (double)curFactor.Value);
                            curFactor.Value = newValue;
                            Add(curFactor, context);
                            var curFactors = context.Set<CurrentFactor>().AsNoTracking().Include(f => f.ScoringFactor).Where(f => f.CurrentCategoryId == curCategory.Id).ToList();
                            var factorsCount = curFactors.Count;
                            if (factorsCount == 0)
                            {
                                factorsCount = 1;
                            }
                            var oldCatValue = curCategory.Value;
                            curCategory.Value = curFactors.Sum(f => f.Value) / factorsCount;
                            var newCatValue = curCategory.Value;
                            Add(curCategory, context);
                            var curCategories = context.Set<CurrentCategory>().AsNoTracking().Include(f => f.Category).Where(c => c.Category.CategoryType == category.CategoryType && c.ScoringId == scoringId);
                            var curCatCount = curCategories.ToList().Count;
                            var k = (decimal)Math.Pow((curCatCount > 0 ? curCatCount : 2) * 2, -1);
                            if (category.CategoryType == (int)ScoringCategoryType.Fix)
                            {
                                scoring.CurrentFix = (decimal)curCategories.Sum(c => Math.Pow((double)c.Value, (double)(c.Category.Weight > 1 || c.Category.Weight < 0 ? 0 : c.Category.Weight))) * k;
                            }
                            else if (category.CategoryType == (int)ScoringCategoryType.Relative)
                            {
                                scoring.CurerentRelative = (decimal)curCategories.Sum(c => Math.Pow((double)c.Value, (double)(c.Category.Weight > 1 || c.Category.Weight < 0 ? 0 : c.Category.Weight))) * k;
                            }
                            Add(scoring, context);
                            var end = DateTime.Now;
                            var scoringEvent = new ScoringEvent
                            {
                                FinishTime = end,
                                StartTime = start,
                                CurrentFactor = curFactor,
                                OldFactorValue = oldFactorValue,
                                NewFactorValue = newValue,
                                OldCategoryValue = oldCatValue,
                                NewCategoryValue = newCatValue,
                                OldScoring = oldScoring,
                                NewScoring = scoring.CurerentRelative + scoring.CurrentFix
                            };
                            Add(scoringEvent, context);
                            dbContextTransaction.Commit();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            });
        }

        private decimal CalculateFactor(double lifestyle, double current)
        {
            var result = (1 + Math.Sqrt(Math.Abs(lifestyle))) * (lifestyle + Math.Abs(lifestyle)) * (current + Math.Sqrt(1 / (10 * current))) / (4 * lifestyle)
                + Math.Sqrt(1 / Math.Abs(lifestyle)) * ((lifestyle - Math.Abs(lifestyle)) * (Math.Sqrt(current + 0.25) - 0.5) / (2 * lifestyle));
            if (result > 3)
                result = 3;
            if (result < 0.3)
                result = 0.3;
            return (decimal)result;
        }

        #endregion

        #region private
        private int GetFactorId(ScoringFactorEnum factorName)
        {
            using (var context = new BillingContext())
            {
                var factor = context.Set<ScoringFactor>().AsNoTracking().FirstOrDefault(f => f.Code == factorName.ToString());
                return factor?.Id ?? 0;
            }
        }
        private void Add<T>(T entity, BillingContext context) where T : BaseEntity
        {
            if (entity.Id > 0)
                context.Entry(entity).State = EntityState.Modified;
            else
                context.Entry(entity).State = EntityState.Added;
            context.SaveChanges();
        }

        private Scoring GetScoringByModelId(string model)
        {
            int modelId;
            if (!int.TryParse(model, out modelId))
                throw new Exception("model must be int");
            return GetScoringByModelId(modelId);
        }

        private Scoring GetScoringByModelId(int modelId)
        {
            var sin = Get<SIN>(s => s.Character.Model == modelId, s => s.Scoring);
            if (sin.Scoring == null)
                throw new Exception("scoring not found");
            return sin.Scoring;
        }

        #endregion
    }
}
