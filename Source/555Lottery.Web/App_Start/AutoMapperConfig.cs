using _555Lottery.DataModel;
using _555Lottery.Web.Mappers;
using _555Lottery.Web.ViewModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web
{
	public static class AutoMapperConfig
	{
		public static void Configure()
		{
			Mapper.Initialize(x =>
			{
				x.AddProfile<DomainToViewModelMappingProfile>();
				x.AddProfile<ViewModelToDomainMappingProfile>();
			});
		}
	}
}