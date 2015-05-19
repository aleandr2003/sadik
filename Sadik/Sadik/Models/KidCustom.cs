using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public partial class Kid
    {
        public string GetFormattedAge()
        {
            return GetFormattedAge(null);
        }

        public string GetFormattedAge(DateTime? toDate)
        {
            DateTime ToDate = toDate ?? DateTime.Now;
            if (DateOfBirth > ToDate) return "Ещё не родился";
            var timespan = ToDate - DateOfBirth;
            var zeroTime = new DateTime(1, 1, 1);
            var years = (zeroTime + timespan).Year - 1;
            var months = (zeroTime + timespan).Month - 1;
            if (years > 0)
            {
                if (years > 7) return "Уже большой";
                else
                {
                    var yearStr = "";
                    if (years == 1)
                    {
                        yearStr = "1 год";
                    }
                    else if (years == 2 || years == 3 || years == 4)
                    {
                        yearStr = String.Format("{0} года", years);
                    }
                    else if (years < 8)
                    {
                        yearStr = String.Format("{0} лет", years);
                    }
                    var monthStr = GetFormattedMonths(months);
                    return String.Format("{0} {1}", yearStr, monthStr);
                }
            }
            else
            {
                months = (zeroTime + timespan).Month - 1;
                return GetFormattedMonths(months);
            }
        }

        public string GetFormattedMonths(int months)
        {
            if (months == 0)
            {
                return "";
            }
            else if (months == 1)
            {
                return "1 месяц";
            }
            else if (months == 2 || months == 3 || months == 4)
            {
                return String.Format("{0} месяца", months);
            }
            else
            {
                return String.Format("{0} месяцев", months);
            }
        }


        public SkillDegree GetSkill(int ItemId)
        {
            using (var context = new SadikEntities())
            {
                var skill = context.Skills.FirstOrDefault(s => s.KidId == Id && s.ItemId == ItemId);
                return skill != null ? skill.Degree : SkillDegree.None;
            }
        }

        public void UpdateSkill(int ItemId, int skillDegree)
        {
            this.UpdateSkill(ItemId, (SkillDegree)skillDegree);
        }

        public void UpdateSkill(int ItemId, SkillDegree skillDegree)
        {
            using (var context = new SadikEntities())
            {
                var skill = context.Skills.FirstOrDefault(s => s.KidId == Id && s.ItemId == ItemId);
                if (skill != null)
                {
                    skill.Degree = skillDegree;
                }
                else
                {
                    skill = new Skill
                    {
                        KidId = Id,
                        ItemId = ItemId,
                        Degree = skillDegree
                    };
                    context.Skills.Add(skill);
                }
                context.SaveChanges();
            }
        }

        public Presentation GetPresentation(int ItemId)
        {
            using (var context = new SadikEntities())
            {
                return context.Presentations.FirstOrDefault(s => s.KidId == Id && s.ItemId == ItemId);
            }
        }

        public void UpdatePresentation(int ItemId, bool presPerformed, DateTime? datePerformed)
        {
            using (var context = new SadikEntities())
            {
                var presentation = context.Presentations.FirstOrDefault(s => s.KidId == Id && s.ItemId == ItemId);
                if (presentation != null && presPerformed == false)
                {
                    context.Presentations.Remove(presentation);
                }
                else if (presentation == null && presPerformed == true)
                {
                    presentation = new Presentation
                    {
                        KidId = Id,
                        ItemId = ItemId,
                        DatePerformed = datePerformed ?? DateTime.Now
                    };
                    context.Presentations.Add(presentation);
                }
                else if (presentation != null && presPerformed == true && datePerformed.HasValue)
                {
                    presentation.DatePerformed = datePerformed.Value;
                }
                context.SaveChanges();
            }
        }

        public static Tuple<SkillDegree, Presentation> GetItemUsageDetails(int KidId, int ItemId)
        {
            using (var context = new SadikEntities())
            {
                var skill = context.Skills.FirstOrDefault(s => s.KidId == KidId && s.ItemId == ItemId);
                var degree = skill != null ? skill.Degree : SkillDegree.None;
                var pres = context.Presentations.FirstOrDefault(s => s.KidId == KidId && s.ItemId == ItemId);
                return new Tuple<SkillDegree, Presentation>(degree, pres);
            }
        }

    }
}