using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using CatalogTest;

using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Catalog.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : Controller
    {
        private readonly Api api;

        public ProductsController(Api api)
        {
            this.api = api;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ApiProduct>>> GetProducts()
        {
            return Ok(await api.GetProducts());
        }

        [HttpGet("{productId}")]
        public async Task<ActionResult<ApiProduct>> GetProduct(string productId)
        {
            return Ok(await api.GetProduct(productId));
        }

        [HttpGet("{productId}/Options")]
        public async Task<ActionResult<IEnumerable<ApiOption>>> GetProductOptions(string productId)
        {
            return Ok(await api.GetProductOptions(productId));
        }

        [HttpPost("{productId}/Options/{optionId}/GetAvailableValues")]
        public async Task<ActionResult<IEnumerable<ApiOptionValue>>> GetAvailableOptionValues(string productId, string optionId, Dictionary<string, string?> selectedOptions)
        {
            return Ok(await api.GetAvailableOptionValues(productId, optionId, selectedOptions));
        }

        [HttpGet("{productId}/Options/{optionId}/Values")]
        public async Task<ActionResult<IEnumerable<ApiOptionValue>>> GetProductOptionValues(string productId, string optionId)
        {
            return Ok(await api.GetOptionValues(optionId));
        }

        [HttpGet("{productId}/Variants")]
        public async Task<ActionResult<IEnumerable<ApiProductVariant>>> GetVariants(string productId)
        {
            return Ok(await api.GetProductVariants(productId));
        }

        [HttpPost("{productId}/Variants/Find")]
        public async Task<ActionResult<ApiProductVariant>> FindVariantByOptionValues(string productId, Dictionary<string, string?> selectedOptions)
        {
            return Ok(await api.GetProductVariant(productId, selectedOptions));
        }

        [HttpGet("{productId}/Variants/{variantId}")]
        public async Task<ActionResult<IEnumerable<ApiProductVariant>>> GetVariants(string productId, string variantId)
        {
            return Ok(await api.GetProductVariantOptions(productId, variantId));
        }
    }
}