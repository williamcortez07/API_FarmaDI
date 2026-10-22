using FarmaDiBusiness.DTOs;
using FarmaDiBusiness.DTOs.ProductDto;
using FarmaDiBusiness.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FarmaDiApi.Controllers
{
    [Route("api/[controller]")]
    public class ProductsController : BaseApiController
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddProductDto addProductDto)
        {
            if (addProductDto == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _productService.AddAsync(addProductDto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var productDto = new ProductDto
                {
                    ProductId = serviceResponse.Data.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.CategoryId,
                    PresentationId = serviceResponse.Data.PresentationId,
                    ConcentrationId = serviceResponse.Data.ConcentrationId,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue,
                    SupplierId = serviceResponse.Data.SupplierId,
                    BrandId = serviceResponse.Data.BrandId,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = productDto.ProductId },
                    productDto);
            }

            return MapServiceError(
                serviceResponse,
                customConflictMessage: "El nombre del producto ya existe");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var serviceResponse = await _productService.GetAllAsync();

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var productsDtoCollection = serviceResponse.Data.Select(p => new GetAllProductsDto
                {
                    ProductId = p.ProductId,
                    GenericName = p.GenericName,
                    TradeName = p.TradeName,
                    CategoryId = p.oCategory.CategoryId,
                    CategoryName = p.oCategory.CategoryName,
                    PresentationId = p.oPresentation.Id,
                    PresentationName = p.oPresentation.Description,
                    ConcentrationId = p.oconcentration.ConcentrationId,
                    Porcentage = p.oconcentration.Volume,
                    ConcentrationValue = p.ConcentrationValue,
                    SupplierId = p.oSupplier.SupplierId,
                    SupplierName = p.oSupplier.SupplierName,
                    BrandId = p.obrand.BrandId,
                    BrandName = p.obrand.BrandName,
                    IsActive = p.IsActive
                });

                return Ok(BuildCollectionResponse(productsDtoCollection, serviceResponse.Message));
            }

            return MapServiceError(serviceResponse);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _productService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var productDto = new GetAllProductsDto
                {
                    ProductId = serviceResponse.Data.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.oCategory.CategoryId,
                    CategoryName = serviceResponse.Data.oCategory.CategoryName,
                    PresentationId = serviceResponse.Data.oPresentation.Id,
                    PresentationName = serviceResponse.Data.oPresentation.Description,
                    ConcentrationId = serviceResponse.Data.oconcentration.ConcentrationId,
                    Porcentage = serviceResponse.Data.oconcentration.Volume,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue,
                    SupplierId = serviceResponse.Data.oSupplier.SupplierId,
                    SupplierName = serviceResponse.Data.oSupplier.SupplierName,
                    BrandId = serviceResponse.Data.obrand.BrandId,
                    BrandName = serviceResponse.Data.obrand.BrandName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(productDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un producto asociado al Id proporcionado");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dataProduct)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            if (dataProduct == null)
            {
                return BadRequestError("El cuerpo de la solicitud no puede estar vacío.");
            }

            var serviceResponse = await _productService.UpdateAsync(id, dataProduct);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var updatedProduct = new ProductDto
                {
                    ProductId = serviceResponse.Data.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.CategoryId,
                    PresentationId = serviceResponse.Data.PresentationId, // CORREGIDO: Mapeaba erróneamente ConcentrationId
                    ConcentrationId = serviceResponse.Data.ConcentrationId,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue,
                    SupplierId = serviceResponse.Data.SupplierId,
                    BrandId = serviceResponse.Data.BrandId,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(updatedProduct);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un producto con el Id proporcionado");
        }

        [HttpGet("byname/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequestError(
                    "El dato proporcionado no es válido",
                    new { Error = "El nombre no puede ser nulo o vacío" });
            }

            var serviceResponse = await _productService.GetByNameAsync(name);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var productDto = new GetAllProductsDto
                {
                    ProductId = serviceResponse.Data.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.oCategory.CategoryId,
                    CategoryName = serviceResponse.Data.oCategory.CategoryName,
                    PresentationId = serviceResponse.Data.oPresentation.Id,
                    PresentationName = serviceResponse.Data.oPresentation.Description,
                    ConcentrationId = serviceResponse.Data.oconcentration.ConcentrationId,
                    Porcentage = serviceResponse.Data.oconcentration.Volume,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue,
                    SupplierId = serviceResponse.Data.oSupplier.SupplierId,
                    SupplierName = serviceResponse.Data.oSupplier.SupplierName,
                    BrandId = serviceResponse.Data.obrand.BrandId,
                    BrandName = serviceResponse.Data.obrand.BrandName,
                    IsActive = serviceResponse.Data.IsActive,
                };

                return Ok(productDto);
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "Producto no encontrado");
        }

        [HttpPatch("{id}/state")]
        public async Task<IActionResult> SetState(int id, [FromQuery] bool state)
        {
            if (!TryValidatePositiveId(id, "Id", out var validationError))
            {
                return validationError!;
            }

            var serviceResponse = await _productService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(new
                {
                    ProductId = serviceResponse.Data.ProductId,
                    GenericName = serviceResponse.Data.GenericName,
                    TradeName = serviceResponse.Data.TradeName,
                    CategoryId = serviceResponse.Data.CategoryId,
                    PresentationId = serviceResponse.Data.PresentationId,
                    ConcentrationId = serviceResponse.Data.ConcentrationId,
                    ConcentrationValue = serviceResponse.Data.ConcentrationValue,
                    SupplierId = serviceResponse.Data.SupplierId,
                    BrandId = serviceResponse.Data.BrandId,
                    IsActive = serviceResponse.Data.IsActive,
                    Message = serviceResponse.Message
                });
            }

            return MapServiceError(
                serviceResponse,
                customNotFoundMessage: "No se encontró un producto con el Id proporcionado");
        }

        [HttpGet("paged")]
        public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            NormalizePagination(ref page, ref limit);

            var serviceResponse = await _productService.GetProductsPagedAsync(page, limit);

            if (serviceResponse.IsSuccess && serviceResponse.Data.Item1 != null)
            {
                var (products, totalCount) = serviceResponse.Data;
                var productsDtoCollection = products.Select(p => new GetAllProductsDto
                {
                    ProductId = p.ProductId,
                    GenericName = p.GenericName,
                    TradeName = p.TradeName,
                    CategoryId = p.oCategory.CategoryId,
                    CategoryName = p.oCategory.CategoryName,
                    PresentationId = p.oPresentation.Id,
                    PresentationName = p.oPresentation.Description,
                    ConcentrationId = p.oconcentration.ConcentrationId,
                    Porcentage = p.oconcentration.Volume,
                    ConcentrationValue = p.ConcentrationValue,
                    SupplierId = p.oSupplier.SupplierId,
                    SupplierName = p.oSupplier.SupplierName,
                    BrandId = p.obrand.BrandId,
                    BrandName = p.obrand.BrandName,
                    IsActive = p.IsActive
                });

                return Ok(BuildPagedResponse(productsDtoCollection, totalCount, page, limit));
            }

            return MapServiceError(serviceResponse);
        }
    }
}
