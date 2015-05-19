using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public class StatReport
    {
        public int kidId;
        public Kid kid;
        public DateTime? startPeriod;
        public DateTime? endPeriod;
        private Dictionary<int, ItemActivityStat> itemStats;
        private List<Zone> zones;
        private Zone globalZone;
        public int GetZoneDepth
        {
            get
            {
                return globalZone.GetDepth() - 1;
            }
        }

        public StatReport(Kid _kid, DateTime? _startPeriod, DateTime? _endPeriod)
        {
            kid = _kid;
            kidId = kid.Id;
            startPeriod = _startPeriod;
            endPeriod = _endPeriod;
            RetrieveFullStatistics();
            RetrieveZones();
        }

        private void RetrieveZones()
        {
            using (var context = new SadikEntities())
            {
                var rootZone = context.Zones.FirstOrDefault(z => z.ParentZoneId == null && z.KindergartenId == kid.KindergartenId);
                if (rootZone == null)
                    throw new ArgumentException("Ошибка данных. Корневая зона не найдена");

                zones = context.Zones.Include("ChildZones").Include("ParentZone").Include("Inventories")
                    .Where(z => z.KindergartenId == kid.KindergartenId).ToList();
                globalZone = zones.First(z => z.Id == rootZone.Id);
            }
            
        }

        private void RetrieveFullStatistics()
        {
            using (var context = new SadikEntities())
            {
                var statistics = context.CalculateKidActivityStats(kidId, startPeriod, endPeriod).ToList();
                var extraDataList = (from item in context.Inventory.Where(i => !i.IsDeleted && i.KindergartenId == kid.KindergartenId)
                                     join p in context.Presentations.Where(p => p.KidId == kidId)
                                         on item.Id equals p.ItemId into leftJoin1
                                     from lj1 in leftJoin1.DefaultIfEmpty()
                                     join s in context.Skills.Where(p => p.KidId == kidId)
                                         on item.Id equals s.ItemId into leftJoin2
                                     from lj2 in leftJoin2.DefaultIfEmpty()
                                     select new
                                     {
                                         ItemName = item.Name,
                                         ItemId = item.Id,
                                         PresentationDate = (DateTime?)lj1.DatePerformed,
                                         skillDegree = (SkillDegree?)lj2.Degree
                                     }
                                  ).ToList();
                var statisticsList = (from data in extraDataList
                                      join a in statistics
                                          on data.ItemId equals a.ItemId into leftJoin3
                                      from stat in leftJoin3.DefaultIfEmpty()
                                      select new ItemActivityStat
                                      {
                                          ItemName = data.ItemName,
                                          ItemId = data.ItemId,
                                          PresentationDate = data.PresentationDate,
                                          skillDegree = data.skillDegree,
                                          statBrief = stat.Brief,
                                          statShort = stat.Short,
                                          statLong = stat.Long,
                                          statPolarization = stat.Polarization,
                                          statChoseHimSelf = stat.ChoseHimSelf
                                      }).ToList();
                itemStats = statisticsList.ToDictionary(s => s.ItemId, s => s);
            }
        }

        private void FillPercentage(List<ZoneActivityStat> zoneStats)
        {
            var sum = zoneStats.Select(z => z.Total).Sum();
            foreach (var zoneStat in zoneStats)
            {
                zoneStat.percentage = sum > 0 ? (double) zoneStat.Total / sum * 100 : 0;
            }
        }

        public List<StatReportLine> ComputeZoneStats()
        {
            var report = new List<StatReportLine>();
            foreach (var zone in zones)
            {
                foreach (var item in zone.ActiveInventory)
                {
                    report.Add(new ItemStatReportLine()
                    {
                        ZonePath = zone.ZonePath,
                        ZonePathStr = zone.ZonePathStr,
                        stat = itemStats[item.Id]
                    });
                }
                var zoneItemsIds = zone.GetAllChildItemsIds();
                var zoneItemsStats = itemStats.Where(i => zoneItemsIds.Contains(i.Key)).Select(i => i.Value);
                var zoneStat = new ZoneActivityStat()
                {
                    ZoneId = zone.Id,
                    statBrief = zoneItemsStats.Sum(i => i.statBrief),
                    statShort = zoneItemsStats.Sum(i => i.statShort),
                    statLong = zoneItemsStats.Sum(i => i.statLong),
                    statPolarization = zoneItemsStats.Sum(i => i.statPolarization),
                    statChoseHimSelf = zoneItemsStats.Sum(i => i.statChoseHimSelf)
                };
                report.Add(new ZoneStatReportLine()
                {
                    ZonePath = zone.ZonePath,
                    ZonePathStr = zone.ZonePathStr,
                    stat = zoneStat
                });
            }
            FillPercentage(report.Where(l => l is ZoneStatReportLine && l.ZonePath.Count == 1)
                .Select(l => (l as ZoneStatReportLine).stat).ToList());
            return report.OrderBy(r => r.ZonePathStr).ThenByDescending(r => r.LineType).ToList();
        }
    }

    public class ItemActivityStat
    {
        public int ItemId;
        public string ItemName;
        public DateTime? PresentationDate;
        public SkillDegree? skillDegree;
        public int? statBrief;
        public int? statShort;
        public int? statLong;
        public int? statPolarization;
        public int? statChoseHimSelf;
        public int Total
        {
            get
            {
                return (statLong ?? 0) + (statBrief ?? 0) + (statShort ?? 0); 
            }
        }
    }

    public class ZoneActivityStat
    {
        public int ZoneId;
        public int? statBrief;
        public int? statShort;
        public int? statLong;
        public int? statPolarization;
        public int? statChoseHimSelf;
        public double? percentage;
        public int Total
        {
            get
            {
                return (statLong ?? 0) + (statBrief ?? 0) + (statShort ?? 0); 
            }
        }
    }

    public class StatReportLine
    {
        public List<Zone> ZonePath;
        public string ZonePathStr;
        public string LineType = "";
    }

    public class ItemStatReportLine : StatReportLine
    {
        public ItemActivityStat stat;
        public ItemStatReportLine()
        {
            LineType = "ItemStatReportLine"; //need this to sort lines in the report appropriately
        }
    }

    public class ZoneStatReportLine : StatReportLine
    {
        public ZoneActivityStat stat;
        public ZoneStatReportLine()
        {
            LineType = "ZoneStatReportLine"; //need this to sort lines in the report appropriately
        }
    }
}