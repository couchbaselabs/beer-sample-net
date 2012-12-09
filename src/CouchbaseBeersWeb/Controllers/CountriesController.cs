using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CouchbaseBeersWeb.Models;

namespace CouchbaseBeersWeb.Controllers
{
	public class CountriesController : Controller
	{
		public BreweryRepository BreweryRepository { get; set; }

		public CountriesController()
		{
			BreweryRepository = new BreweryRepository();
		}

		public ActionResult Index()
		{
			var grouped = BreweryRepository.GetGroupedByLocation(BreweryGroupLevels.Country);
			return View(grouped);
		}

		public ActionResult Provinces(string country)
		{
			var grouped = BreweryRepository.GetGroupedByLocation(
						BreweryGroupLevels.Province, new string[] { country });
			return View(grouped);
		}

		public ActionResult Cities(string country, string province)
		{
			var grouped = BreweryRepository.GetGroupedByLocation(
						BreweryGroupLevels.City, new string[] { country, province });
			return View(grouped);
		}

		public ActionResult Codes(string country, string province, string city)
		{
			var grouped = BreweryRepository.GetGroupedByLocation(
						BreweryGroupLevels.PostalCode, new string[] { country, province, city });
			return View(grouped);
		}

		public ActionResult Details(string country, string province, string city, string code)
		{
			var breweries = BreweryRepository.GetByLocation(country, province, city, code);
			return View(breweries);
		}
	}
}
