using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class ConcentrationController : BaseApiController
    {
        private readonly IConcentrationServices _concentrationsService;

        public ConcentrationController(IConcentrationServices concentrationsService)
        {
            _concentrationsService = concentrationsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _concentrationsService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var concentrationsDtoCollection = serviceResponse.Data.Select(b => new ConcentrationDto
                {
                    ConcentrationId = b.ConcentrationId,
                    Volume = b.Volume,
                    porcentage = b.porcentage,
                    IsActive = b.IsActive
                });

                return Ok(BuildCollectionResponse(concentrationsDtoCollection, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _concentrationsService.GetConcentrationsPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Items != null)
            {
                var (items, totalCount) = serviceResponse.Data;
                var dtos = items.Select(b => new ConcentrationDto
                {
                    ConcentrationId = b.ConcentrationId,
                    Volume = b.Volume,
                    porcentage = b.porcentage,
                    IsActive = b.IsActive
                });

                return Ok(BuildPagedResponse(dtos, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }
    }
}
