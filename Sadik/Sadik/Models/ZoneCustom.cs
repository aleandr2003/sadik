using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public partial class Zone
    {
        public const string globalZoneDisplayName = "ГЛАВНАЯ";

        public int GetDepth()
        {
            if (ChildZones.Count() == 0) return 1;
            else
            {
                return ChildZones.ToList().Select(z => z.GetDepth()).Max() + 1;
            }
        }
        public ICollection<Inventory> ActiveInventory
        {
            get
            {
                return Inventories.Where(i => !i.IsDeleted).ToList();
            }
        }

        private List<int> allChildInventories = null;
        public List<int> GetAllChildItemsIds()
        {
            if (allChildInventories == null)
            {
                allChildInventories = new List<int>();
                allChildInventories.AddRange(Inventories.Select(i => i.Id).ToList());
                foreach (var zone in ChildZones)
                {
                    allChildInventories.AddRange(zone.GetAllChildItemsIds());
                }
            }
            return allChildInventories;
        }

        private string _zonePathStr = null;
        public string ZonePathStr
        {
            get
            {
                if (_zonePathStr == null)
                {
                    _zonePathStr = GetZonePathStr();
                }
                return _zonePathStr;
            }
        }

        private List<Zone> _zonePath = null;
        public List<Zone> ZonePath
        {
            get
            {
                if (_zonePath == null)
                {
                    _zonePath = GetZonePath();
                }
                return _zonePath;
            }
        }

        private string GetZonePathStr()
        {
            return String.Join("|", ZonePath.Select(z => z.Name).ToArray());
        }

        private List<Zone> GetZonePath()
        {
            var currentZone = this;
            var zoneList = new List<Zone>();
            while (currentZone.ParentZoneId != null)
            {
                zoneList.Add(currentZone);
                currentZone = currentZone.ParentZone;
            }
            zoneList.Reverse();
            return zoneList;
        }
    }
}