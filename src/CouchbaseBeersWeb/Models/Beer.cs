using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CouchbaseBeersWeb.Models
{
	public class Beer : ModelBase
	{
		public string Name { get; set; }

		public override string Type
		{
			get { return "beer"; }
		}
	}
}