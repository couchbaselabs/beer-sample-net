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
using System.Web.Mvc;
using CouchbaseBeersWeb.Models;

namespace CouchbaseBeersWeb.Controllers
{
	public class BeersController : Controller
	{
		public BeerRepository BeerRepository { get; set; }

		public BeersController()
		{
			BeerRepository = new BeerRepository();
		}

		//
		// GET: /beers/

		public ActionResult Index(string startKey, int pageSize = 25)
		{
			var beers = BeerRepository.GetAllByName(startKey: startKey, limit: pageSize + 1);
			ViewBag.StartKey = beers.ElementAt(0).Name;
			ViewBag.NextStartKey = beers.ElementAt(beers.Count() - 1).Name;
			return View(beers.Take(pageSize));
		}

		//
		// GET: /beers/Details/5

		public ActionResult Details(string id)
		{
			var Beer = BeerRepository.Get(id);
			return View(Beer);
		}

		//
		// GET: /beers/Create

		public ActionResult Create()
		{
			return View();
		}

		//
		// POST: /beers/Create

		[HttpPost]
		public ActionResult Create(Beer Beer)
		{
			BeerRepository.Create(Beer);
			return RedirectToAction("Index");
		}

		//
		// GET: /beers/Edit/5

		public ActionResult Edit(string id)
		{
			var Beer = BeerRepository.Get(id);
			return View(Beer);
		}

		//
		// POST: /beers/Edit/5

		[HttpPost]
		public ActionResult Edit(string id, Beer Beer)
		{
			BeerRepository.Update(Beer);
			return RedirectToAction("Index");
		}

		//
		// GET: /beers/Delete/5

		public ActionResult Delete(string id)
		{
			var Beer = BeerRepository.Get(id);
			return View(Beer);
		}

		//
		// POST: /beers/Delete/5

		[HttpPost]
		public ActionResult Delete(string id, Beer Beer)
		{
			BeerRepository.Delete(id);
			return RedirectToAction("Index");
		}
	}
}
