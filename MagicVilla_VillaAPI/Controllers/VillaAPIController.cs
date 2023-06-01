using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
	[Route("api/[Controller]")]
	[ApiController]
	public class VillaAPIController : ControllerBase
	{
		[HttpGet]
		public ActionResult<IEnumerable<VillaDto>> GetVillas()
		{
			return Ok(VillaStore.villaList);
		}


		[HttpGet("{id:int}",Name = "GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult<VillaDto> GetVilla(int id)
		{
			if(id == 0) 
			{
				return BadRequest();
			}

			var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);

			if(villa == null)
			{
				return NotFound();
			}

			return Ok(villa);
		}


		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public ActionResult<VillaDto> CreateVilla([FromBody]VillaDto villa)
		{
			//if the villa name exist in database
			if(VillaStore.villaList.FirstOrDefault(u=>u.Name.ToLower()==villa.Name.ToLower()) != null) 
			{
				ModelState.AddModelError("CustomError", "Villa already Exists!");

				return BadRequest(ModelState);
			}

			if(villa == null)
			{
				return BadRequest(villa);
			}

			if (villa.Id > 0)
			{ 
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
			villa.Id = VillaStore.villaList.OrderByDescending(u=>u.Id).FirstOrDefault().Id+1;

			VillaStore.villaList.Add(villa);

			return CreatedAtRoute("GetVilla", new { id = villa.Id }, villa);

		}


		[HttpDelete("{id:int}", Name = "DeleteVilla")]
			[ProducesResponseType(StatusCodes.Status204NoContent)]
			[ProducesResponseType(StatusCodes.Status404NotFound)]
			[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult DeleteVila(int id)
		{
			if (id == 0)
			{
				return BadRequest();
			}

			var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
			if (villa == null)
			{
				return NotFound();
			}

			VillaStore.villaList.Remove(villa);
			return NoContent();
		}


		[HttpPut("{id:int}", Name = "PutVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult UpdateVilla(int id, [FromBody]VillaDto villaDto)
		{
			if (villaDto == null || id != villaDto.Id)
			{
				return BadRequest();
			}
			 
			var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
			villa.Name = villaDto.Name;
			villa.Sqft = villaDto.Sqft;
			villa.Occupancy = villaDto.Occupancy;

			return NoContent();
		}


		[HttpPatch("{id:int}", Name = "PatchVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public ActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDto> patchDto) 
		{
			if(patchDto == null || id == 0) 
			{ 
				return BadRequest(); 
			}

			var villa = VillaStore.villaList.FirstOrDefault(u =>u.Id == id);

			if (villa == null)
			{
				return BadRequest();
			}

            patchDto.ApplyTo(villa, ModelState);
 
			if(!ModelState.IsValid) 
			{ 
				return BadRequest();
			}

			return NoContent();
		}
	}
}
