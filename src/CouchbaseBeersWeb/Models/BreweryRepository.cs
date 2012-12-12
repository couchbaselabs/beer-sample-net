using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using inflector_extension;
using Couchbase;
using Enyim.Caching.Memcached.Results;
using Couchbase.Operations;

namespace CouchbaseBeersWeb.Models
{
	public enum BreweryGroupLevels { Country = 1, Province, City, PostalCode }

	public class BreweryRepository : RepositoryBase<Brewery>
	{
		/// <summary>
		/// Create a Brewery, blocking until persisted to master node
		/// Views will not consider a new record for an index
		/// until persisted to disk.
		/// </summary>
		/// <param name="value">Brewery to create</param>
		/// <returns>Status code (0 on success)</returns>
		public int Create(Brewery brewery)
		{
			return base.Create(brewery, PersistTo.One);
		}

		/// <summary>
		/// Remove a Brewery, blocking until removed from disk
		/// Views will not remove a deleted record from an index
		/// until removed from disk.
		/// </summary>
		/// <param name="key">Key of brewery to delete</param>
		/// <returns>Status code (0 on success)</returns>
		public int Delete(string key)
		{
			return base.Delete(key, PersistTo.One);
		}

		public IEnumerable<Brewery> GetAllByName(string startKey = null, string endKey = null, int limit = 0, bool allowStale = false)
		{
			var view = GetView("by_name");
			if (limit > 0) view.Limit(limit);
			if (!allowStale) view.Stale(StaleMode.False);
			if (!string.IsNullOrEmpty(startKey)) view.StartKey(startKey);
			if (!string.IsNullOrEmpty(endKey)) view.StartKey(endKey);
			return view;
		}

		public Brewery GetWithBeers(string id)
		{
			var rows = GetViewRaw("all_with_beers")
				.StartKey(new object[] { id, 0 })
				.EndKey(new object[] { id, "\uefff", 1 })
				.ToArray();

			var brewery = Get(rows[0].ItemId);
			brewery.Beers = rows.Skip(1)
				.Select(r => new Beer { Id = r.ItemId, Name = r.ViewKey[1].ToString() })
				.ToList();

			return brewery;
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