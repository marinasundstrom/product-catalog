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

        [HttpPost]
        public async Task<ActionResult<ApiProduct>> CreateProduct(ApiCreateProduct data)
        {
            return Ok(await api.CreateProduct(data));
        }

        [HttpGet("{productId}/Options/Groups")]
        public async Task<ActionResult<IEnumerable<ApiOptionGroup>>> GetOptionGroups(string productId)
        {
            return Ok(await api.GetOptionGroups(productId));
        }

        [HttpPost("{productId}/Options/Groups")]
        public async Task<ActionResult<ApiOptionGroup>> CreateOptionGroup(string productId, ApiCreateProductOptionGroup data)
        {
            return Ok(await api.CreateOptionGroup(productId, data));
        }

        [HttpGet("{productId}/Options")]
        public async Task<ActionResult<IEnumerable<ApiOption>>> GetProductOptions(string productId)
        {
            return Ok(await api.GetProductOptions(productId));
        }


        [HttpPost("{productId}/Options")]
        public async Task<ActionResult<ApiOption>> CreateProductOption(string productId, ApiCreateProductOption data)
        {
            return Ok(await api.CreateProductOption(productId, data));
        }

        [HttpPut("{productId}/Options/{optionId}")]
        public async Task<ActionResult<ApiOption>> UpdateProductOption(string productId, string optionId, ApiUpdateProductOption data)
        {
            return Ok(await api.UpdateProductOption(productId, optionId, data));
        }

        [HttpDelete("{productId}/Options/{optionId}")]
        public async Task<ActionResult> DeleteProductOption(string productId, string optionId)
        {
            await api.DeleteProductOption(productId, optionId);
            return Ok();
        }

        [HttpPost("{productId}/Options/{optionId}/Values")]
        public async Task<ActionResult<ApiOptionValue>> CreateProductOptionValue(string productId, string optionId, ApiCreateProductOptionValue data)
        {
            
            return Ok(await api.CreateProductOptionValue(productId, optionId, data));
        }

        [HttpPost("{productId}/Options/{optionId}/Values/{valueId}")]
        public async Task<ActionResult> DeleteProductOptionValue(string productId, string optionId, string valueId)
        {
            await api.DeleteProductOptionValue(productId, optionId, valueId);
            return Ok();
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

        [HttpDelete("{productId}/Variants/{variantId}")]
        public async Task<ActionResult> DeleteVariant(string productId, string variantId)
        {
            await api.DeleteVariant(productId, variantId);
            return Ok();
        }

        [HttpGet("{productId}/Variants/{variantId}")]
        public async Task<ActionResult<ApiProductVariant>> GetVariant(string productId, string variantId)
        {
            return Ok(await api.GetProductVariant(productId, variantId));
        }

        [HttpPost("{productId}/Variants/Find")]
        public async Task<ActionResult<ApiProductVariant>> FindVariantByOptionValues(string productId, Dictionary<string, string?> selectedOptions)
        {
            return Ok(await api.FindProductVariant(productId, selectedOptions));
        }

        [HttpGet("{productId}/Variants/{variantId}/Options")]
        public async Task<ActionResult<ApiProductVariant>> GetVariantOptions(string productId, string variantId)
        {
            return Ok(await api.GetProductVariantOptions(productId, variantId));
        }

        [HttpPost("{productId}/Variants")]
        public async Task<ActionResult<ApiProductVariant>> CreateVariant(string productId, ApiCreateProductVariant variant)
        {
            return Ok(await api.CreateVariant(productId, variant));
        }

        [HttpPut("{productId}/Variants/{variantId}")]
        public async Task<ActionResult<ApiProductVariant>> UpdateVariant(string productId, string variantId, ApiUpdateProductVariant data)
        {
            return Ok(await api.UpdateVariant(productId, variantId, data));
        }
    }
}