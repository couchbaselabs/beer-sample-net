using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CouchbaseModelViews.Framework.Attributes;

namespace CouchbaseBeersWeb.Models
{
	[CouchbaseDesignDoc("beers")]
	[CouchbaseAllView]
	public class Beer : ModelBase
	{
		[CouchbaseViewKey("by_name", "name")]
		public string Name { get; set; }

		public float ABV { get; set; }

		public float IBU { get; set; }

		public float SRM { get; set; }

		public float UPC { get; set; }

		public string BreweryId { get; set; }

		public DateTime Updated { get; set; }

		public string Description { get; set; }

		public string Style { get; set; }

		public string Category { get; set; }

		public override string Type { get { return "beer"; } }
	}
}