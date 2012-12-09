using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using inflector_extension;
using Couchbase;
using Enyim.Caching.Memcached.Results;

namespace CouchbaseBeersWeb.Models
{
	public enum BreweryGroupLevels { Country = 1, Province, City, PostalCode }

	public class BreweryRepository : RepositoryBase<Brewery>
	{
		public IEnumerable<Brewery> GetAllByName(string startKey = null, string endKey = null, int limit = 0, bool allowStale = false)
		{
			var view = GetView("by_name");
			if (limit > 0) view.Limit(limit);
			if (!allowStale) view.Stale(StaleMode.False);
			if (!string.IsNullOrEmpty(startKey)) view.StartKey(startKey);
			if (!string.IsNullOrEmpty(endKey)) view.StartKey(endKey);
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

		public IEnumerable<KeyValuePair<string, int>> GetGroupedByLocation(BreweryGroupLevels groupLevel, string[] keys = null)
		{
			var view = GetViewRaw("by_country")
						.Group(true)
						.GroupAt((int)groupLevel);

			if (keys != null)
			{
				view.StartKey(keys);
				view.EndKey(keys.Concat(new string[] { "\uefff" }));
			}

			foreach (var item in view)
			{
				var key = item.ViewKey[(int)groupLevel - 1].ToString();
				var value = Convert.ToInt32(item.Info["value"]);
				yield return new KeyValuePair<string, int>(key, value);
			}
		}

		public IEnumerable<Brewery> GetByLocation(string country, string province, string city, string code)
		{
			return GetView("by_country").Key(new string[] { country, province, city, code }).Reduce(false);
		}

		public IEnumerable<Brewery> GetByPoints(string boundingBox)
		{
			var points = boundingBox.Split(',').Select(s => float.Parse(s)).ToArray();
			return GetSpatialView("points").BoundingBox(points[0], points[1], points[2], points[3]);
		}

		protected override string BuildKey(Brewery model)
		{
			return model.Name.ToLower().InflectTo().Pluralized.InflectTo().Underscored;
		}
	}
}