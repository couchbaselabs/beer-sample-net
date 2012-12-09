using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Couchbase;
using Couchbase.Configuration;
using inflector_extension;
using Enyim.Caching.Memcached.Results;
using Enyim.Caching.Memcached;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace CouchbaseBeersWeb.Models
{
	public abstract class RepositoryBase<T> where T : ModelBase
	{
		protected static CouchbaseClient _Client { get; set; }

		private readonly string _designDoc;

		public RepositoryBase()
		{
			_designDoc = typeof(T).Name.ToLower().InflectTo().Pluralized;
		}

		static RepositoryBase()
		{
			_Client = new CouchbaseClient();
		}

		public virtual IEnumerable<T> GetAll(int limit = 0)
		{
			var view = _Client.GetView<T>(_designDoc, "all", true);
			if (limit > 0) view.Limit(limit);
			return view;
		}

		public virtual Tuple<T, bool, string> Get(string key)
		{
			var result = _Client.ExecuteGet<string>(key);
			if (result.Value == null)
			{
				return Tuple.Create(default(T), result.Success, result.Message);
			}

			var model = JsonConvert.DeserializeObject<T>(result.Value);
			model.Id = key;
			return Tuple.Create(model, result.Success, result.Message);
		}

		public virtual Tuple<bool, string> Create(T value)
		{
			var result = _Client.ExecuteStore(StoreMode.Add, BuildKey(value), serializeAndIgnoreId(value));
			return Tuple.Create(result.Success, result.Message);
		}

		public virtual Tuple<bool, string> Update(T value)
		{
			var result = _Client.ExecuteStore(StoreMode.Replace, value.Id, serializeAndIgnoreId(value));
			return Tuple.Create(result.Success, result.Message);
		}

		public virtual Tuple<bool, string> Save(T value)
		{
			var key = string.IsNullOrEmpty(value.Id) ? BuildKey(value) : value.Id;
			var result = _Client.ExecuteStore(StoreMode.Set, key, serializeAndIgnoreId(value));
			return Tuple.Create(result.Success, result.Message);
		}

		public virtual Tuple<bool, string> Delete(string key)
		{
			var result = _Client.ExecuteRemove(key);
			return Tuple.Create(result.Success, result.Message);
		}

		protected IView<T> GetView(string name, bool isProjection = false)
		{
			return _Client.GetView<T>(_designDoc, name, ! isProjection);
		}

		protected IView<IViewRow> GetViewRaw(string name)
		{
			return _Client.GetView(_designDoc, name);
		}

		protected virtual ISpatialView<T> GetSpatialView(string name, bool isProjection = false)
		{
			return _Client.GetSpatialView<T>(_designDoc, name, !isProjection);
		}

		protected virtual ISpatialView<ISpatialViewRow> GetSpatialViewRaw(string name)
		{
			return _Client.GetSpatialView(_designDoc, name);
		}

		protected virtual string BuildKey(T model)
		{
			if (string.IsNullOrEmpty(model.Id))
			{
				return Guid.NewGuid().ToString();
			}
			return model.Id.ToLower().InflectTo().Underscored;
		}

		private string serializeAndIgnoreId(T obj)
		{
			var json = JsonConvert.SerializeObject(obj,
				new JsonSerializerSettings()
				{
					ContractResolver = new DocumentIdContractResolver(),
				});

			return json;
		}

		private class DocumentIdContractResolver : CamelCasePropertyNamesContractResolver
		{
			protected override List<MemberInfo> GetSerializableMembers(Type objectType)
			{
				return base.GetSerializableMembers(objectType).Where(o => o.Name != "Id").ToList();
			}
		}
	}
}