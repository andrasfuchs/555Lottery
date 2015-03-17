using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using System.Collections.Generic;
using System.Web;
using System.Linq;
using _555Lottery.Web.Controllers;
using _555Lottery.Web.ViewModels;
using _555Lottery.Web.Mappers;
using _555Lottery.Service;
using _555Lottery.DataModel;


namespace _555Lottery.Web.Tests
{
	[TestClass]
	public class GenerateDraws
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

		string[] validBitcoinAddresses = {
			"1H4jpRkFAsN4LZa259t2Y16bmCfQFzhec2",
			"1BczBapmt4BUTAqAKgNkWiAqThGCfwu1Sj",
			"1BxqffQGiEf7G6s4Uj2Ha3drWH1NyDXaFj",
			"1JoSxHQownRUqkaH35C6kmuLcVjNkDfDcT",
			"1MQQzHod4Z7hVVdSnYeUcbs2DxyCxwEUJR",
			"14fqXvw2GXyjLqkuVdw4qLGC8BgEKUcArX",
			"1MW9funVuY4E4bn7eH9xcWyGgtcPztWS8G",
			"154PzJjoocKidKFHRGLdvkSGvF8p1YAdwa",
			"17GxR5ARGZx5qqYmnodK76fg7TSw8B3yZ6",
			"1EuubVHfD5GGA2QZgduwcuP2L2M5xSRLvx",
			"1DWvf1RndERP7vsjc54xoxhihrKBk9xQTa",
			"15D7uABCtM9NppCfcJwZ3RFrg3gxo99UCT",
			"1ALzhbjCi3HRj1m23NLpYNgMqj5GHmRuqy",
			"1968y13WLsJM8ZnsabZeZ44Z62Vg6tQTcj",
			"1PYXM1X5d9N6r34NtgpSMPhsvUd1K2qkaB",
			"1HpTFaEGHSSJKisa4DinYP1mRBxRMhgMHt",
			"1E7cPoeRWHKjz8N8H6JYsCXvXQhpwuXsHA",
			"1GNfgJARpVUHAVoDRS8R2njxoqFUgLcLuL",
			"19kFNbmA5nJ9MsDjYEAX5f1miME9oH4mjj",
			"1Ern3LKFz4k8xzzrR8mVorbXRj4VTGWtJ7",
			"1ehgRArkf7fHQkRWXJbg7WjwQaU337cyt",
			"1GRniumjXxrdM58D2bZk79CBwBd9QRJiso",
			"1JFD8Af78MJNQsGju3ikYLmpN88PUAa5gg",
			"14yLkuCpFiEveLjwDnSdd5YdCia2LNJR22",
			"1C8PsoLPf1NuqwQL1NAyVsaGv59VA52qPQ",
			"1PqVwT8EnjPqnom6usmiEXaaqnH5eULX57",
			"16tApsS6wG37ybpaSN3C3sXr5DRU7GSQxe",
			"188YkoPJqSaNeRsJg6bZnWvA2jM4rXUvd8",
			"1PwKdUUYtLwmLoenvPfzFoQJK4US1L5UzE",
			"16NFez4jpThKVeDFSC4EBdHznuSbsyTTgo",
			"14haM3r8RxtXuN4yAKEq97jCH6X2xjZFi3",
			"174juFecus1327pyDRz1KeBTYb9zmLy3Sw",
			"1C5E5ZuYwnJLcYzAK9MwaVFDMxXkh67Y3d",
			"1C8WH9J2Ehwsvm5wBDMWpx4LMRUL6z8s2r",
			"1LSfpWe1ax8x7tKHAG5DmKVXmqc6Dv2Wzy",
			"1FssiiNoppuoTik92KSRzCt6Da3EeRV4q2",
			"1BECyE4vjUBZvoewZ5Y9yCurYWFpe1UqTJ",
			"18Df5G714VKWgv1GDZBGk5pp4DLsDpFPmm",
			"17r67Zou9ZzzWvDa1ZMpr9dmFQjfcW3myt",
			"1GTbcxwCtYZLnPoqmdrak1p6H4rwNPirJV",
			"13xQoPdwsSRHuHdADQqcdm3X5NgyPUFKt5",
			"1BRMdMNetD1Uhf9RgdiQt9ByBotvBEJsgG",
			"13TLGDb1CHUEjmLS8xw4x52v1J2ronqk8D",
			"1D77sR9Mi8tug7qVzxJqM2aFth98FCZkYv",
			"14i5jPuxLUzQmq6aDokWaFtDaJd9fnX9LR",
			"1Cp9bWPKW8dYJWXNKPZDeCCZcvbaM5SxBu",
			"1HSr1vgxZqFMAPN1ayA7Tchq2chq3eTZ3X",
			"1JrUmBNkHqozCDtRjvWNrHddRJocSgKR2K",
			"1Lmciuers3e3vrwCNDeeJHww669QogY71F",
			"121po3uUn6WUrd4urMxrxDoFTTf8vYTyPV",
			"1FVtMzEjkvbGkJUdWeezNf4ftQ1jGSWt86",
			"1L7Y9keu5e5ZXhzSJFdB8ButoMaTiGhkaL",
			"1E3cQBopqGY9K3QvW8VpAkVjTSVBJHQBSP",
			"1M2rA8noEoK8zNibSg499b78rrfJzCDgF",
			"1JrBxYuKkeG8LKFDf1XfSyJktNqn6gaBMb",
			"18ytKY1xANYek8P6Jots4RUgH5aV1Edfpn",
			"1LRHduWzxB7JSXSKyrqtd1sYBxMjWAVWDp",
			"13Yt5y3m7wVBrVsHpnuhkbpvAKmNQLCaDp",
			"1Q4anXLGrVePY6577EifG6y9gioaJkgXxF",
			"12bWMSy3PR7Q3go2AjUQ4frDb7qca3PcoG",
			"15ZdqPdkW9qThzbJKyMYypzRdrcGyYEb8L",
			"1N2GsVeYMWnEUcwuRFJaCCmDeA2iTiYAjr",
			"13vjKgeEDxW3BYLJpEAWFCByvyceJC7PFi",
			"1B6oBdkPxrKjpr4j9f1MeTUays9tbeehap",
			"19LpDJ2ygFcdbX4reBnbborJJs4BKWNteA",
			"1ETyC7PWSVF9yFEGoeijPtzbVdwnwvqA5w",
			"18HzbfknKDEnmdNUwBgmBiLLNN2PvsqMzn",
			"1gFBauv18LdifVpVnm6MS4X764dsL6AdV",
			"1JAzeYoX9UTQTcd3643XiAFsyZ4wDP7GQN",
			"1Jyx2JanRAYZLM4rDDPh2jA8Dnk1edohbc",
			"18f6prFhvw6QXgaX9FHWvxVubhtpAjfits",
			"1HqbrqRd8LA3QKpwqeN2275qFnaAKYbv9S",
			"13Vakryd2v5E4h1dgFJ25zvJBuPSU5Z41t",
			"16jiCFozPtA6fWkYwdtHpZFYBvRHnh9uT5",
			"1K8U9SkxT7Ae8vPDvKfYqeHjSFjo2eiR5r",
			"13SvFRVsqCgZ6XZwpKGpip91BjZekh7uCj",
			"165UzqiJstU7Eb1NmDwU6NDqD7yWsMw3r4",
			"1AoUpAzLEmRr7QAVx1LyNVP7R2QSrJWPpR",
			"1Add7hjCRNMevYPebGPqx8iHUzamr3N9FZ",
			"13eTik1MQLyobqioWrost5HmvdZCsrQK4z",
			"18eAvgxTGVvJkty2buuAmpNzP3shedycWn",
			"1F8R3489foGYXeekMqqdaGCnBHf8euPpdf",
			"15x8K5sRaK5J5cDNWwKrXqFrZL3kAsQSqi",
			"1Dpt1TfeMQpcSDXx88SXHnhCBEgkS5cPmi",
			"13kGjJiBMtJAeEF4WMG1A7NjA6QZ727BRY",
			"1C8UA1QE9iyGcpfkfjQaWBZ2d3yMJxkYby",
			"1Gy15hSPuPyNCqRqQaKrmiDY8mcND5RPKU",
			"1KSe66xrgkJGB4mGvTTzhbKXL2BoTPn2fd",
			"18Mq2cWsv7YMwyerw3WVHkBbkEX9ZYnKxU",
			"1Pv8DJqp3JPeW2DG9caHRzAB6jcj73YHAb",
			"1CYpXGLQGUFwzkBpBSz6AT4u7Kieqtv6LE",
			"14ngf4Q2u2pdi9SbTszJRbXfcYL3ZpnWct",
			"1EpaAfMLSbMaa52hBfsE6GLc7M7DGrPqnY",
			"1PnzzsDA6zxfg3cByFeQA7VmsejuYJ3JUy",
			"1JdovAEizxrNDwDunPNCDsNsrxEkcLSdvT",
			"1CJ8mSV6aWN5xkGzSzFYiXtsTCHLssNqzS",
			"1HG8autAfwC3XtUwQ25f19hKMYi2E5MiTX",
			"15iXL8GWi4LiYDgWtmXi6QEv1cYHmenN5o",
			"194oAfFZAfr57ym8jczbmiCSKg6Mi2nV56",
			"1KP8u384oxhTWgTpuBGVhbJ36tHjwie6FR",
			"18qNWyPybj62bjzM1KNbTBhF4tw5LSWwZq",
			"1H6fPeBSBGUysfckCFVCaMWnAe9ieQje7h",
			"1JnQSPRKgog4n9M1ZwqvLxWYGzX3FVYFQN",
			"1Fzce9cNrD7pm33BTxmntViyRScKoaYMX9",
			"1121GPRgr2tPuNxYu45c9YKiJ6yJBnjRam",
			"1241ba9cPEbmGstQEvsBozgBM59iN2XvD6",
			"181NPYuc7ny3M6LCh1etBtqjGNNCwzyJna",
			"1Bcx2i71nz7aCW6ALaUegUyi7WEExSmKW8",
			"15EaMNa6DvRkmEGjP4bGLDULyM13rzFBi1",
			"1NVmYqyWU4WJogNoQbNQNQMsBhToWDv15R",
			"16uHR4LkiDhbJSoMUkEMs4SSEh7Wsqs4wC",
			"1Dpz7vt1mcqp2RW2T86Kc5QVDNu2QK74BT",
			"1GyVVCvJruvXTBt3p6hTSuNHDjno6bXWit" };


		TimeSpan generationInterval = new TimeSpan(168, 0, 0);

		[TestMethod]
		public void GenerateNewDraws()
		{
			Random rnd = new Random();

			LotteryService.Instance.Initialize(null, false);
			Draw lastDraw = LotteryService.Instance.Context.Draws.OrderByDescending(d => d.DeadlineUtc).First();

			DateTime currentDeadlineUtc = lastDraw.DeadlineUtc;
			int drawCodeIndex = Int32.Parse(lastDraw.DrawCode.Split('-')[1]);

			System.Diagnostics.Debug.WriteLine("Generating {0} new draws with the interval of {1} hours.", validBitcoinAddresses.Length, generationInterval.TotalHours.ToString("0.00"));
			for (int i = 0; i < validBitcoinAddresses.Length; i++)
			{
				if (currentDeadlineUtc.Year != (currentDeadlineUtc + generationInterval).Year)
				{
					drawCodeIndex = 0;
				}

				currentDeadlineUtc += generationInterval;
				drawCodeIndex++;

				if (drawCodeIndex >= 1000)
				{
					throw new Exception("It is not allowed to have more than 999 draws in a year!");
				}

				LotteryService.Instance.CloneDraw(lastDraw.DrawCode, "DRW" + currentDeadlineUtc.Year + "-" + drawCodeIndex.ToString("000"), currentDeadlineUtc, validBitcoinAddresses[i]);
			}
		}
	}
}
