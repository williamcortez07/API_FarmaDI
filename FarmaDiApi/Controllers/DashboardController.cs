using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class DashboardController : BaseApiController
    {
        private readonly IKPIDashboardService _dashboardService;

        public DashboardController(IKPIDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("kpis")]
        public async Task<IActionResult> GetKPIs()
        {
            var response = await _dashboardService.GetDashboardKPIsAsync();

            if (response.Data is null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new UnsuccessfulResponseDto
                    {
                        Code = StatusCodes.Status500InternalServerError.ToString(),
                        Message = "Ocurrió un error al obtener los indicadores.",
                        Details = new { info = response.Message ?? "Error interno en la aplicación" }
                    });
            }

            return Ok(response.Data);
        }
    }
}
