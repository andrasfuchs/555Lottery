using _555Lottery.DataModel;
using _555Lottery.Web.ViewModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Mappers
{
	public class DomainToViewModelMappingProfile : Profile
	{
		public override string ProfileName
		{
			get { return "DomainToViewModelMappings"; }
		}

		protected override void Configure()
		{
			Mapper.CreateMap<Draw, DrawViewModel>();
			Mapper.CreateMap<TicketLot, TicketLotViewModel>();
			Mapper.CreateMap<Ticket, TicketViewModel>();
		}
	}
}