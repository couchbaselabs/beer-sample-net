using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CouchbaseBeersWeb.Models;
using System.Web.Mvc;

namespace CouchbaseBeersWeb.Controllers
{
	public class LocationsController : Controller
	{
		public BreweryRepository BreweryRepository { get; set; }

		public LocationsController()
		{
			BreweryRepository = new BreweryRepository();
		}

		[HttpGet]
		public ActionResult Details()
		{
			return View();
		}

		[HttpPost]
		public ActionResult Details(string bbox)
		{
			var breweriesByPoints = BreweryRepository.GetByPoints(bbox)
										.Select(b => new
										{
											id = b.Id,
											name = b.Name,
											geo = new float[] { b.Longitude, b.Latitude }
										});
			return Json(breweriesByPoints);
		}
	}
}