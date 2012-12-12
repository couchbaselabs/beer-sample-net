using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CouchbaseBeersWeb.Models
{
	/// <summary>
	/// Abstract base class for all model classes to
	/// be serialized into Couchbase Server.
	/// </summary>
	public abstract class ModelBase
	{
		/// <summary>
		/// Document key used to store and retrieve documents.
		/// It's marked virtual for classes that need to override
		/// attributes such as JsonIgnore or CouchbaseViewKey
		/// </summary>
		public virtual string Id { get; set; }

		/// <summary>
		/// All model classes must implement this "type" property
		/// to provide documents with a taxonomy.  Map functions
		/// will then be used as follows (for a User class with type "user"):
		/// <code>
		/// function(doc, meta) {
		///		if (doc.type == "user" && doc.lastname) { 
		///			emit(doc.lastname, null); 
		///		}
		/// }
		/// </code>
		/// </summary>
		public abstract string Type { get; }
	}
}