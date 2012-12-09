#region [ License information          ]
/* ************************************************************
 * 
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2012 Couchbase, Inc.
 *    
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *        http://www.apache.org/licenses/LICENSE-2.0
 *    
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *    
 * ************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Couchbase;

namespace CouchbaseBeersWeb.Models
{
	public class BeerRepository : RepositoryBase<Beer>
	{
		public IEnumerable<Beer> GetAllByName(string startKey = null, string endKey = null, int limit = 0)
		{
			var view = GetView("by_name");

			if (!string.IsNullOrEmpty(startKey)) view.StartKey(startKey);
			if (!string.IsNullOrEmpty(endKey)) view.EndKey(startKey);
			if (limit > 0) view.Limit(limit);

			return view.Stale(StaleMode.False);
		}

		protected override string BuildKey(Beer beer)
		{
			beer.Id = beer.Name;
			return base.BuildKey(beer);
		}
	}
}