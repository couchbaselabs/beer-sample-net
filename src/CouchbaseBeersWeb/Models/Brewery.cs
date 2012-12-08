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

		public string City { get; set; }

		public string State { get; set; }

		public string Code { get; set; }

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

		public override string Type
		{
			get { return "brewery"; }
		}
	}
}