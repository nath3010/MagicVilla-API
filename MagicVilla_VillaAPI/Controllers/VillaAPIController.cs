using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
	[Route("api/[Controller]")]
	[ApiController]
	public class VillaAPIController : ControllerBase
	{
		[HttpGet]
		public IEnumerable<VillaDto> GetVillas()
		{
			return VillaStore.villaList;
		}

		[HttpGet("{id:int}")]
		public VillaDto GetVilla(int id)
		{
			return VillaStore.villaList.FirstOrDefault(u => u.Id == id);
		}

	}
}
