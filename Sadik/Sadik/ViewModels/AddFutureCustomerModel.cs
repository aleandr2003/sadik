using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Sadik.ViewModels
{
    public class AddFutureCustomerModel
    {
        public string PlaceId { get; set; }
	    public string Name  { get; set; }
	    public string Country  { get; set; }
	    public string State  { get; set; }
	    public string City  { get; set; }
	    public string Street  { get; set; }
	    public string Building { get; set; }
	    public string Phone  { get; set; }
	    public string GoogleMapsUrl  { get; set; }
	    public string Website  { get; set; }
	    public string Email  { get; set; }
	    public string Reference  { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}