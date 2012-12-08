using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CouchbaseBeersWeb.Models
{
	public abstract class ModelBase
	{
		public virtual string Id { get; set; }

		public abstract string Type { get; }
	}
}