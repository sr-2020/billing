﻿using Billing;
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
        void OnLifeStyleChanged(Scoring scoring, Lifestyles from, Lifestyles to);
        void OnPillConsumed(string model, string pillLifestyle);
        void OnWounded(string model);
        void OnClinicalDeath(string model);
        void OnDumpshock(string model);
        void OnFoodConsume(string model, string foodLifeStyle);
    }

    public class ScoringManager : BaseEntityRepository, IScoringManager
    {
        public void OnFoodConsume(string model, string foodLifeStyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.food_consume);
            if (!BillingHelper.LifestyleIsDefined(foodLifeStyle))
            {
                return;
            }
            var lifestyle = BillingHelper.GetLifestyle(foodLifeStyle);
            var scoring = GetScoringByModelId(model);
            ScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == (int)lifestyle);
                return value?.Value ?? 1;
            });
        }

        public void OnPillConsumed(string model, string pillLifestyle)
        {
            var factorId = GetFactorId(ScoringFactorEnum.pill_use);
            if (!BillingHelper.LifestyleIsDefined(pillLifestyle))
            {
                return;
            }
            var lifestyle = BillingHelper.GetLifestyle(pillLifestyle);
            var scoring = GetScoringByModelId(model);
            ScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == (int)lifestyle);
                return value?.Value ?? 1;
            });
        }

        public void OnWounded(string model)
        {
            var factorId = GetFactorId(ScoringFactorEnum.worse);
            var scoring = GetScoringByModelId(model);
            ScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == 1);
                return value?.Value ?? 1;
            });
        }

        public void OnClinicalDeath(string model)
        {
            var factorId = GetFactorId(ScoringFactorEnum.clinical_death);
            var scoring = GetScoringByModelId(model);
            ScoringEvent(scoring.Id, factorId, (context) =>
            {
                var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == 1);
                return value?.Value ?? 1;
            });
        }

        public void OnDumpshock(string model)
        {
            var factorId = GetFactorId(ScoringFactorEnum.dumpshock);
            var scoring = GetScoringByModelId(model);
            ScoringEvent(scoring.Id, factorId, (context) =>
            {
                    var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == 1);
                    return value?.Value ?? 1;
            });
        }

        public void OnLifeStyleChanged(Scoring scoring, Lifestyles from, Lifestyles to)
        {
            var factorId = GetFactorId(ScoringFactorEnum.ls_change);
            ScoringEvent(scoring.Id, factorId, (context) =>
             {
                 var value = context.Set<ScoringEventLifestyle>().AsNoTracking().FirstOrDefault(s => s.ScoringFactorId == factorId && s.EventNumber == ScoringHelper.GetEventNumberLifestyle(from, to));
                 return value?.Value ?? 1;
             });
        }

        public void OnTest(int scoringId)
        {
            var factorId = GetFactorId(ScoringFactorEnum.test);
            ScoringEvent(scoringId, 1, (context) =>
            {
                Thread.Sleep(10000);
                return 0;
            });
        }

        private int GetFactorId(ScoringFactorEnum factorName)
        {
            using (var context = new BillingContext())
            {
                var factor = context.Set<ScoringFactor>().AsNoTracking().FirstOrDefault(f => f.Code == factorName.ToString());
                return factor.Id;
            }
        }

        private void ScoringEvent(int scoringId, int factorId, Func<BillingContext, decimal> action)
        {
            Task.Run(() =>
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

                            var curFactor = context.Set<CurrentFactor>().AsNoTracking().FirstOrDefault(s => s.ScoringId == scoringId && s.ScoringFactorId == factorId);
                            if (curFactor == null)
                            {
                                curFactor = new CurrentFactor
                                {
                                    ScoringFactorId = factorId,
                                    ScoringId = scoringId,
                                    Value = 1
                                };
                                Add(curFactor, context);
                            }
                            var newValue = CalculateFactor((double)lifestyle, (double)curFactor.Value);
                            curFactor.Value = newValue;
                            Add(curFactor, context);

                            var curCategory = context.Set<CurrentCategory>().AsNoTracking().FirstOrDefault(c => c.ScoringId == scoringId && c.CategoryId == factor.CategoryId);
                            if (curCategory == null)
                            {
                                curCategory = new CurrentCategory
                                {
                                    ScoringId = scoringId,
                                    CategoryId = factor.CategoryId
                                };
                            }
                            var curFactors = context.Set<CurrentFactor>().AsNoTracking().Include(f => f.ScoringFactor).Where(f => f.ScoringFactor.CategoryId == factor.CategoryId && f.ScoringId == scoringId);
                            curCategory.Value = curFactors.Sum(f => f.Value);
                            Add(curCategory, context);

                            var curCategories = context.Set<CurrentCategory>().AsNoTracking().Include(f => f.Category).Where(c => c.Category.CategoryType == category.CategoryType && c.ScoringId == scoringId);
                            var scoring = context.Set<Scoring>().AsTracking().FirstOrDefault(s => s.Id == scoringId);
                            var systemsettings = IocContainer.Get<ISettingsManager>();

                            if (category.CategoryType == (int)ScoringCategoryType.Fix)
                            {
                                var k = systemsettings.GetDecimalValue(SystemSettingsEnum.fixK);
                                scoring.CurrentFix = curCategories.Sum(c => c.Value * c.Category.Weight) * k;
                            }
                            else if (category.CategoryType == (int)ScoringCategoryType.Relative)
                            {
                                var k = systemsettings.GetDecimalValue(SystemSettingsEnum.relativeK);
                                scoring.CurerentRelative = curCategories.Sum(c => c.Value * c.Category.Weight) * k;
                            }
                            scoring.CurrentScoring = scoring.CurrentFix + scoring.CurerentRelative;
                            Add(scoring, context);
                            var end = DateTime.Now;
                            var scoringEvent = new ScoringEvent
                            {
                                FinishTime = end,
                                StartTime = start,
                                CurrentFactor = curFactor
                            };
                            Add(scoringEvent, context);
                            dbContextTransaction.Commit();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            });
        }

        private decimal CalculateFactor(double lifestyle, double current)
        {
            var result = Math.Sqrt(Math.Abs(lifestyle)) * (lifestyle + Math.Abs(lifestyle)) * (current + Math.Sqrt(1 / (3 * current))) / (2 * lifestyle) + Math.Sqrt(1 / Math.Abs(lifestyle)) * (Math.Sqrt(current + 1 / 4) - 1 / 2) / (2 * lifestyle);
            if (result > 3)
                result = 3;
            if (result < 0.3)
                result = 0.3;
            return (decimal)result;
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
            var sin = Get<SIN>(s => s.Character.Model == modelId, s => s.Scoring);
            if (sin.Scoring == null)
                throw new Exception("scoring not found");
            return sin.Scoring;
        }

        private Scoring GetScoringByModelId(int modelId)
        {
            var sin = Get<SIN>(s => s.Character.Model == modelId, s => s.Scoring);
            if (sin.Scoring == null)
                throw new Exception("scoring not found");
            return sin.Scoring;
        }

    }
}
