using _555Lottery.DataModel;
using _555Lottery.Web.ViewModels;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace _555Lottery.Web.Mappers
{
	public class ViewModelToDomainMappingProfile : Profile
	{
		public override string ProfileName
		{
			get { return "ViewModelToDomainMappings"; }
		}

		protected override void Configure()
		{
			Mapper.CreateMap<DrawViewModel, Draw>();
			Mapper.CreateMap<TicketLotViewModel, TicketLot>();
			Mapper.CreateMap<TicketViewModel, Ticket>();
		}
	}
}