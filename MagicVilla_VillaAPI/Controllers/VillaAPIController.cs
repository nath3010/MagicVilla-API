using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Logging;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
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
		private readonly IVillaRepository _dbVilla;
		private readonly ILogging _logger;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, ILogging logger, IMapper mapper)
        {
			_dbVilla = dbVilla;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDto>>> GetVillasAsync()
		{
			_logger.Log("Getting all villas","");

			IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();

			return Ok(_mapper.Map<List<VillaDto>>(villaList));
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

			var villa = await _dbVilla.GetAsync(u => u.Id == id);

			if(villa == null)
			{
				return NotFound();
			}

			return Ok(_mapper.Map<VillaDto>(villa));
		}


		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<VillaDto>> CreateVillaAsync([FromBody] VillaCreateDto createDto)
		{
			//if the villa name exist in database
			if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDto.Name.ToLower()) != null)
			{
				ModelState.AddModelError("CustomError", "Villa already Exists!");

				return BadRequest(ModelState);
			}

			if (createDto == null)
			{
				return BadRequest(createDto);
			}

			Villa model = _mapper.Map<Villa>(createDto);

			await _dbVilla.CreateAsync(model);

            return CreatedAtRoute("GetVilla", new { id = model.Id }, model);

		}


		[HttpDelete("{id:int}", Name = "DeleteVilla")]
			[ProducesResponseType(StatusCodes.Status204NoContent)]
			[ProducesResponseType(StatusCodes.Status404NotFound)]
			[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteVilaAsync(int id)
		{
			if (id == 0)
			{
				return BadRequest();
			}

			var villa = await _dbVilla.GetAsync(u => u.Id == id);
			if (villa == null)
			{
				return NotFound();
			}

            await _dbVilla.RemoveAsync(villa);

            return NoContent();
		}


		[HttpPut("{id:int}", Name = "PutVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult> UpdateVillaAsync(int id, [FromBody]VillaUpdateDto updateDto)
		{
			if (updateDto == null || id != updateDto.Id)
			{
				return BadRequest();
			}

            Villa model = _mapper.Map<Villa>(updateDto);

            await _dbVilla.UpdateAsync(model);

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

            var villa = await _dbVilla.GetAsync(u => u.Id == id, tracked:false);

            VillaUpdateDto villaDto = _mapper.Map<VillaUpdateDto>(villa);

            if (villa == null)
			{
				return BadRequest();
			}

            patchDto.ApplyTo(villaDto, ModelState);

            Villa model = _mapper.Map<Villa>(villaDto);

			await _dbVilla.UpdateAsync(model);

            if (!ModelState.IsValid) 
			{ 
				return BadRequest();
			}

			return NoContent();	
		}
	}
}
