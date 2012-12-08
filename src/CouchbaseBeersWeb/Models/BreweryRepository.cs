using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using inflector_extension;
using Couchbase;
using Enyim.Caching.Memcached.Results;

namespace CouchbaseBeersWeb.Models
{
	public class BreweryRepository : RepositoryBase<Brewery>
	{
		public IEnumerable<Brewery> GetAllByName(string startKey = null, string endKey = null, int limit = 0, bool allowStale = false)
		{
			var view = GetView("by_name");
			if (limit > 0) view.Limit(limit);
			if (! allowStale) view.Stale(StaleMode.False);
			if (! string.IsNullOrEmpty(startKey)) view.StartKey(startKey);
			if (! string.IsNullOrEmpty(endKey)) view.StartKey(endKey);
			return view;
		}

		public Tuple<Brewery, bool, string> GetWithBeers(string id)
		{
			var rows = GetViewRaw("all_with_beers")
				.StartKey(new object[] { id, 0 })
				.EndKey(new object[] { id, "\uefff", 1 })
				.ToArray();

			var result = Get(rows[0].ItemId);
			result.Item1.Beers = rows.Skip(1)
				.Select(r => new Beer { Id = r.ItemId, Name = r.ViewKey[1].ToString() })
				.ToList();
			
			return result;
		}

		protected override string BuildKey(Brewery model)
		{
			return model.Name.InflectTo().Underscored;
		}
	}
}