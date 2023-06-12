using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
	[Route("api/[Controller]")]
	[ApiController]
	public class VillaAPIController : ControllerBase
	{
		private readonly ApplicationDbContext _db;
		private readonly ILogging _logger;

        public VillaAPIController(ApplicationDbContext db, ILogging logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDto>>> GetVillas()
		{
			_logger.Log("Getting all villas","");

			return Ok(await _db.Villas.ToListAsync());
		}


		[HttpGet("{id:int}",Name = "GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<VillaDto>> GetVillaAsync(int id)
		{
			if(id == 0) 
			{
                _logger.Log("Get Villa Error with id: " + id, "error");
                return BadRequest();
			}

			var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);

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
		public async Task<ActionResult<VillaDto>> CreateVillaAsync([FromBody] VillaCreateDto villaDto)
		{
			//if the villa name exist in database
			if (await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower()) != null)
			{
				ModelState.AddModelError("CustomError", "Villa already Exists!");

				return BadRequest(ModelState);
			}

			if (villaDto == null)
			{
				return BadRequest(villaDto);
			}

			//if (villaDto.Id > 0)
			//{
			//	return StatusCode(StatusCodes.Status500InternalServerError);
			//}
			//villa.Id = VillaStore.villaList.OrderByDescending(u=>u.Id).FirstOrDefault().Id+1;

			Villa model = new()
			{
				Amenity = villaDto.Amenity,
				Details = villaDto.Details,
				//Id = villaDto.Id,
				ImageUrl = villaDto.ImageUrl,
				Name = villaDto.Name,
				Occupancy = villaDto.Occupancy,
				Rate = villaDto.Rate,
				Sqft = villaDto.Sqft

			};

			await _db.Villas.AddAsync(model);
			await _db.SaveChangesAsync();

            //VillaStore.villaList.Add(villa);

            return CreatedAtRoute("GetVilla", new { id = model.Id }, model);

		}


		[HttpDelete("{id:int}", Name = "DeleteVilla")]
			[ProducesResponseType(StatusCodes.Status204NoContent)]
			[ProducesResponseType(StatusCodes.Status404NotFound)]
			[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteVila(int id)
		{
			if (id == 0)
			{
				return BadRequest();
			}

			var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
			if (villa == null)
			{
				return NotFound();
			}

           _db.Villas.Remove(villa);
           await _db.SaveChangesAsync();

            return NoContent();
		}


		[HttpPut("{id:int}", Name = "PutVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult> UpdateVillaAsync(int id, [FromBody]VillaUpdateDto villaDto)
		{
			if (villaDto == null || id != villaDto.Id)
			{
				return BadRequest();
			}

            //var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            //villa.Name = villaDto.Name;
            //villa.Sqft = villaDto.Sqft;
            //villa.Occupancy = villaDto.Occupancy;

            Villa model = new()
            {
                Amenity = villaDto.Amenity,
                Details = villaDto.Details,
                Id = villaDto.Id,
                ImageUrl = villaDto.ImageUrl,
                Name = villaDto.Name,
                Occupancy = villaDto.Occupancy,
                Rate = villaDto.Rate,
                Sqft = villaDto.Sqft

            };

            _db.Villas.Update(model);
			await _db.SaveChangesAsync();


            return NoContent();
		}


		[HttpPatch("{id:int}", Name = "PatchVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult> UpdatePartialVillaAsync(int id, JsonPatchDocument<VillaUpdateDto> patchDto) 
		{
			if(patchDto == null || id == 0) 
			{ 
				return BadRequest(); 
			}

            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

            VillaUpdateDto villaDto = new()
            {
                Amenity = villa.Amenity,
                Details = villa.Details,
                Id = villa.Id,
                ImageUrl = villa.ImageUrl,
                Name = villa.Name,
                Occupancy = villa.Occupancy,
                Rate = villa.Rate,
                Sqft = villa.Sqft
            };

            if (villa == null)
			{
				return BadRequest();
			}

            patchDto.ApplyTo(villaDto, ModelState);

            Villa model = new()
            {
                Amenity = villaDto.Amenity,
                Details = villaDto.Details,
                Id = villaDto.Id,
                ImageUrl = villaDto.ImageUrl,
                Name = villaDto.Name,
                Occupancy = villaDto.Occupancy,
                Rate = villaDto.Rate,
                Sqft = villaDto.Sqft

            };

			_db.Villas.Update(model);
			await  _db.SaveChangesAsync();

            if (!ModelState.IsValid) 
			{ 
				return BadRequest();
			}

			return NoContent();	
		}
	}
}
