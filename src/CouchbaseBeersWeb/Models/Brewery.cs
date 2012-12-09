using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CouchbaseModelViews.Framework.Attributes;
using Newtonsoft.Json;

namespace CouchbaseBeersWeb.Models
{
	[CouchbaseDesignDoc("breweries")]
	[CouchbaseAllView]
	public class Brewery : ModelBase
	{
		[CouchbaseCollatedViewKey("all_with_beers", "beer", "name", "brewery_id")]
		public override string Id { get; set; }

		[CouchbaseViewKey("by_name", "name")]
		public string Name { get; set; }

		[CouchbaseViewKeyCount("by_country", "city", 2)]
		public string City { get; set; }

		[CouchbaseViewKeyCount("by_country", "state", 1)]
		public string State { get; set; }

		[CouchbaseViewKeyCount("by_country", "code", 3, "null")]
		public string Code { get; set; }

		[CouchbaseViewKeyCount("by_country", "country", 0)]
		public string Country { get; set; }

		public string Phone { get; set; }

		public string Website { get; set; }

		public DateTime Updated { get; set; }

		public string Description { get; set; }

		public IList<string> Addresses { get; set; }

		public IDictionary<string, object> Geo { get; set; }

		private IList<Beer> _beers = new List<Beer>();

		[JsonIgnore]
		public IList<Beer> Beers
		{
			get { return _beers; }
			set { _beers = value; }
		}

		[JsonIgnore]
		public string GeoAccuracy
		{
			get
			{
				return Geo != null && Geo.ContainsKey("accuracy") ? Geo["accuracy"] as string : "";
			}
		}

		[CouchbaseSpatialViewKey("points", "geo.lng", 0)]
		[JsonIgnore]
		public float Longitude
		{
			get
			{
				return Geo != null && Geo.ContainsKey("lng") ? Convert.ToSingle(Geo["lng"]) : 0f;
			}
		}

		[CouchbaseSpatialViewKey("points", "geo.lat", 1)]
		[JsonIgnore]
		public float Latitude
		{
			get
			{
				return Geo != null && Geo.ContainsKey("lat") ? Convert.ToSingle(Geo["lat"]) : 0f;
			}
		}

		public override string Type
		{
			get { return "brewery"; }
		}
	}
}