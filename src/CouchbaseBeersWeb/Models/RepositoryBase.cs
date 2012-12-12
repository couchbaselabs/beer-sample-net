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
using Couchbase.Operations;

namespace CouchbaseBeersWeb.Models
{
	/// <summary>
	/// Abstract base class encapsulating most common data access
	/// methods for Couchbase apps.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class RepositoryBase<T> where T : ModelBase
	{
		protected static CouchbaseClient _Client { get; set; }

		private readonly string _designDoc;

		public RepositoryBase()
		{
			/// design documents are conventionally expected to be pluaralized name of typeof(T)
			_designDoc = typeof(T).Name.ToLower().InflectTo().Pluralized;
		}

		static RepositoryBase()
		{
			//ensure a single CouchbaseClient per app domain
			_Client = new CouchbaseClient(); //requires "couchbase" section to be defined for config
		}

		/// <summary>
		/// Query the "all" view.  View is expected to emit null keys/value pairs.
		/// <code>
		/// //_design/users/_views/all
		/// function(doc, meta) {
		///		if (doc.type == "user") {
		///			emit(null, null);
		///		}
		/// }
		/// </code>
		/// </summary>
		/// <param name="limit">Number of records to return</param>
		/// <param name="updateIndex">When true, index is updated before returning results.</param>
		/// <returns></returns>
		public virtual IEnumerable<T> GetAll(int limit = 0, bool updateIndex = false)
		{
			var view = _Client.GetView<T>(_designDoc, "all", true);
			if (limit > 0) view.Limit(limit);
			if (updateIndex) view.Stale(StaleMode.False);
			return view;
		}

		/// <summary>
		/// Retrieve a document from a bucket
		/// </summary>
		/// <param name="key">Key of document</param>
		/// <returns>When key is found, returns document, else null.</returns>
		public virtual T Get(string key)
		{
			var result = _Client.ExecuteGet<string>(key);
			if (result.Exception != null) throw result.Exception;

			if (result.Value == null)
			{
				return null;
			}

			var model = JsonConvert.DeserializeObject<T>(result.Value);
			model.Id = key; //Id is not serialized into the JSON document on store, so need to set it before returning
			return model;
		}

		/// <summary>
		/// Create a document.  Will fail if key exists.
		/// </summary>
		/// <param name="value">New document</param>
		/// <returns>Status code for Add operation.  Successful updates return 0.</returns>
		public virtual int Create(T value, PersistTo persistTo = PersistTo.Zero)
		{
			var result = _Client.ExecuteStore(StoreMode.Add, BuildKey(value), serializeAndIgnoreId(value), persistTo);
			if (result.Exception != null) throw result.Exception;
			return result.StatusCode.Value;
		}

		/// <summary>
		/// Update a document.  Will fail if key (Id property) doesn't exit.
		/// </summary>
		/// <param name="value">New document</param>
		/// /// <param name="persistTo">Optional durability requirement</param>
		/// <returns>Status code for Update operation.  Successful updates return 0.</returns>
		public virtual int Update(T value, PersistTo persistTo = PersistTo.Zero)
		{
			var result = _Client.ExecuteStore(StoreMode.Replace, value.Id, serializeAndIgnoreId(value), persistTo);
			if (result.Exception != null) throw result.Exception;
			return result.StatusCode.Value;
		}

		/// <summary>
		/// Create a document if key doesn't exist, otherwise update document.
		/// </summary>
		/// <param name="value">New document</param>
		/// <param name="persistTo">Optional durability requirement</param>
		/// <returns>Status code for Set operation.  Successful updates return 0.</returns>
		public virtual int Save(T value, PersistTo persistTo = PersistTo.Zero)
		{
			var key = string.IsNullOrEmpty(value.Id) ? BuildKey(value) : value.Id;
			var result = _Client.ExecuteStore(StoreMode.Set, key, serializeAndIgnoreId(value), persistTo);
			if (result.Exception != null) throw result.Exception;
			return result.StatusCode.Value;
		}

		/// <summary>
		/// Delete a key.
		/// </summary>
		/// <param name="key">Key of document to delete</param>
		/// <param name="persistTo">Durability requirement</param>
		/// <returns>Status code of Remove operation</returns>
		public virtual int Delete(string key, PersistTo persistTo = PersistTo.Zero)
		{
			var result = _Client.ExecuteRemove(key, persistTo);
			if (result.Exception != null) throw result.Exception;
			return result.StatusCode.HasValue ? result.StatusCode.Value : 0;
		}

		/// <summary>
		/// Return named view for T's design document.
		/// </summary>
		/// <param name="name">Name of view</param>
		/// <param name="isProjection">Whether value is projection</param>
		/// <returns>Typed IView instance</returns>
		protected IView<T> GetView(string name, bool isProjection = false)
		{
			return _Client.GetView<T>(_designDoc, name, ! isProjection);
		}

		/// <summary>
		/// Return named view for T's design document.
		/// </summary>
		/// <param name="name">Name of view</param>
		/// <returns>IView of IViewRow instance</returns>
		protected IView<IViewRow> GetViewRaw(string name)
		{
			return _Client.GetView(_designDoc, name);
		}
		
		/// <summary>
		/// Get typed spatial view
		/// </summary>
		/// <param name="name">Name of spatial view</param>
		/// <param name="isProjection">Whether value is projection</param>
		/// <returns>Typed spatial view</returns>
		protected virtual ISpatialView<T> GetSpatialView(string name, bool isProjection = false)
		{
			return _Client.GetSpatialView<T>(_designDoc, name, !isProjection);
		}

		/// <summary>
		/// Get spatial view
		/// </summary>
		/// <param name="name">Name of spatial view</param>		
		/// <returns>Typed ISpatialView of ISpatialViewRow</returns>
		protected virtual ISpatialView<ISpatialViewRow> GetSpatialViewRaw(string name)
		{
			return _Client.GetSpatialView(_designDoc, name);
		}

		/// <summary>
		/// Default key generation strategy.  If no key is provided
		/// then GUID is created.  Subclasses should override
		/// when Id property shouldn't be used.
		/// </summary>
		/// <param name="model"></param>
		/// <returns></returns>
		protected virtual string BuildKey(T model)
		{
			if (string.IsNullOrEmpty(model.Id))
			{
				return Guid.NewGuid().ToString();
			}
			return model.Id.ToLower().InflectTo().Underscored;
		}

		/// <summary>
		/// Ignore Id property when serializing objects
		/// </summary>
		/// <param name="obj">T instance to serialize</param>
		/// <returns>JSON string to save</returns>
		private string serializeAndIgnoreId(T obj)
		{
			var json = JsonConvert.SerializeObject(obj,
				new JsonSerializerSettings()
				{
					ContractResolver = new DocumentIdContractResolver(),
				});

			return json;
		}

		/// <summary>
		/// ContractResolver implementation used to instruct Json.NET not to serialize
		/// the Id property on models, since it is already part of the meta data available
		/// to views.  Extends CamelCasePropertyNamesContractResolver to provide standard
		/// JSON property naming conventions.
		/// </summary>
		private class DocumentIdContractResolver : CamelCasePropertyNamesContractResolver
		{
			protected override List<MemberInfo> GetSerializableMembers(Type objectType)
			{
				return base.GetSerializableMembers(objectType).Where(o => o.Name != "Id").ToList();
			}
		}
	}
}