using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using _555Lottery.Web.Controllers;
using _555Lottery.Web.ViewModels;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web;
using _555Lottery.Web.Mappers;
using _555Lottery.Service;
using _555Lottery.DataModel;

namespace _555Lottery.Web.Tests
{
	[TestClass]
	public class CreateNewLotsTest
	{
		[TestInitialize]
		public void Initialize()
		{
			HttpContext.Current = MockHelper.FakeHttpContext();

			AutoMapper.Mapper.Initialize(x =>
			{
				x.AddProfile<DomainToViewModelMappingProfile>();
				x.AddProfile<ViewModelToDomainMappingProfile>();
			});
		}

		[TestMethod]
		public void GenerateTicketLots()
		{
			HomeController homeController = new HomeController();

			for (int i = 0; i < 500; i++)
			{
				homeController.Index();
				homeController.AcceptTicket("R1", ",,,,|", 0);
				((TicketLotViewModel)HttpContext.Current.Session["Tickets"]).DrawNumber = 1;
				JsonResult tl = homeController.LetsPlay();
				TicketLotViewModel tlvm = tl.Data as TicketLotViewModel;
				if (tlvm != null)
				{
					homeController.Check(tlvm.Code);
				}
			}
		}

		[TestMethod]
		public void DoADrawWithAllTicketLots()
		{
			Random rnd = new Random();

			LotteryService.Instance.Initialize(null, false);

			Draw d = LotteryService.Instance.CloneDraw("DRW2013-010", "DRWTEST-" + rnd.Next(1000).ToString("000"), DateTime.UtcNow);

			LotteryService.Instance.MoveAndConfirmAllTicketLotsForTesting(d, "lotteryunittestsessionid");

			DateTime start = DateTime.Now;
			LotteryService.Instance.DrawDraw(d);
			System.Diagnostics.Debug.WriteLine("Running a draw with {0} games took {1} seconds.", d.ValidGameCount, (DateTime.Now - start).TotalSeconds.ToString("0.00"));
		}
	}
}
