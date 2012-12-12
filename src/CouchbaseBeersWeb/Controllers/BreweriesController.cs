using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CouchbaseBeersWeb.Models;

namespace CouchbaseBeersWeb.Controllers
{
	public class BreweriesController : Controller
	{
		public BreweryRepository BreweryRepository { get; set; }

		public BreweriesController()
		{
			BreweryRepository = new BreweryRepository();
		}

		//
		// GET: /Breweries/

		public ActionResult Index(string startKey, int pageSize = 25)
		{
			var breweries = BreweryRepository.GetAllByName(startKey: startKey, limit: pageSize + 1);
			ViewBag.StartKey = breweries.ElementAt(0).Name;
			ViewBag.NextStartKey = breweries.ElementAt(breweries.Count() - 1).Name;
			return View(breweries.Take(pageSize));
		}

		//
		// GET: /Breweries/Details/5

		public ActionResult Details(string id)
		{
			var brewery = BreweryRepository.GetWithBeers(id);
			return View(brewery);
		}

		//
		// GET: /Breweries/Create

		public ActionResult Create()
		{
			return View();
		}

		//
		// POST: /Breweries/Create

		[HttpPost]
		public ActionResult Create(Brewery brewery)
		{
			BreweryRepository.Create(brewery);
			return RedirectToAction("Index");
		}

		//
		// GET: /Breweries/Edit/5

		public ActionResult Edit(string id)
		{
			var brewery = BreweryRepository.Get(id);
			return View(brewery);
		}

		//
		// POST: /Breweries/Edit/5

		[HttpPost]
		public ActionResult Edit(string id, Brewery brewery)
		{
			BreweryRepository.Update(brewery);
			return RedirectToAction("Index");
		}

		//
		// GET: /Breweries/Delete/5

		public ActionResult Delete(string id)
		{
			var brewery = BreweryRepository.Get(id);
			return View(brewery);
		}

		//
		// POST: /Breweries/Delete/5

		[HttpPost]
		public ActionResult Delete(string id, Brewery brewery)
		{
			BreweryRepository.Delete(id);
			return RedirectToAction("Index");
		}
	}
}
