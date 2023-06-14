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
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
	[Route("api/[Controller]")]
	[ApiController]
	public class VillaAPIController : ControllerBase
	{
		protected APIResponse _response;
		private readonly IVillaRepository _dbVilla;
		private readonly ILogging _logger;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, ILogging logger, IMapper mapper)
        {
			_dbVilla = dbVilla;
            _logger = logger;
            _mapper = mapper;
			this._response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillasAsync()
		{
			try
			{
				_logger.Log("Getting all villas", "");

				IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();

				_response.Result = _mapper.Map<List<VillaDto>>(villaList);
				_response.StatusCode = HttpStatusCode.OK; //System.Net.HttpStatusCode.OK;

				return Ok(_response);
			}
			catch (Exception ex)
			{
				_response.IsSuccess = false;
				_response.ErrorsMessages = new List<string>() { ex.ToString()};
			}
			return _response;
		}


		[HttpGet("{id:int}",Name = "GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<APIResponse>> GetVillaAsync(int id)
		{
			try
			{
				if (id == 0)
				{
					_logger.Log("Get Villa Error with id: " + id, "error");
					
					_response.StatusCode=HttpStatusCode.BadRequest;
					return BadRequest(_response);
				}

				var villa = await _dbVilla.GetAsync(u => u.Id == id);

				if (villa == null)
				{
					_response.StatusCode = HttpStatusCode.NotFound;
						
                    return NotFound(_response);
				}
				_response.Result = _mapper.Map<VillaDto>(villa);
				_response.StatusCode = HttpStatusCode.OK;

				return Ok(_response);
			}
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorsMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }


		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<ActionResult<APIResponse>> CreateVillaAsync([FromBody] VillaCreateDto createDto)
		{
			try
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

				Villa villa = _mapper.Map<Villa>(createDto);

				await _dbVilla.CreateAsync(villa);

				_response.Result = _mapper.Map<VillaDto>(villa);
				_response.StatusCode = HttpStatusCode.Created;

				return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);

			}
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorsMessages = new List<string>() { ex.ToString() };
            }
            return _response;

        }


		[HttpDelete("{id:int}", Name = "DeleteVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> DeleteVilaAsync(int id)
		{
			try
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

				_response.StatusCode = HttpStatusCode.NoContent;
				_response.IsSuccess = true;

				return Ok(_response);
			}
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorsMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }


		[HttpPut("{id:int}", Name = "PutVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<APIResponse>> UpdateVillaAsync(int id, [FromBody]VillaUpdateDto updateDto)
		{
			try
			{
				if (updateDto == null || id != updateDto.Id)
				{
					return BadRequest();
				}

				Villa model = _mapper.Map<Villa>(updateDto);

				await _dbVilla.UpdateAsync(model);

				_response.StatusCode = HttpStatusCode.NoContent;
				_response.IsSuccess = true;

				return Ok(_response);
			}
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorsMessages = new List<string>() { ex.ToString() };
            }
            return _response;
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
