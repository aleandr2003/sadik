using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.Models
{
    public partial class Inventory
    {
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
            var currentZone = this.Zone;
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