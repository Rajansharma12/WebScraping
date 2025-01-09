using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyFirstPROJECT.Models;

namespace MyFirstPROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuchanController : ControllerBase
    {
        private readonly ILogger<AuchanController> _logger;

        public AuchanController(ILogger<AuchanController> logger)
        {
            _logger = logger;
        }

        [HttpGet]

        public async Task<IActionResult> ScrapeProduct([FromQuery] string url = "https://www.auchan.fr/apple-iphone-16-pro-max-256go-titane-noir/pr-C1818737")
        {
            var product = new ProductProperties
            {
                Name = string.Empty,
                MRP = string.Empty,
                Images = Array.Empty<string>(),
                Description = Array.Empty<string>(),
                Category = Array.Empty<string>(),
                TableData = new Dictionary<string, string>(),
            };

            try
            {

                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(url);
                    HtmlDocument document = new HtmlDocument();
                    document.LoadHtml(response);

                    _logger.LogInformation(document.DocumentNode.OuterHtml);

                    var Categories = document.DocumentNode.SelectNodes("//div[contains(@class,\'site-breadcrumb__wrapper\')]//a");
                    if (Categories != null)
                    {
                        product.Category = Categories
                            .Select(row => $"{row.InnerText.Trim()}")
                            .ToArray();
                    }


                    var name = document.DocumentNode.SelectSingleNode("//div[contains(@class,\'offer-selector__name--large\')]//h1");
                    if (name != null)
                    {
                        product.Name = name.InnerText.Trim();
                    }

                    var productBrand = document.DocumentNode.SelectSingleNode("//div[contains(@class,\"offer-selector__name--large\")]/a/bold");
                    if (productBrand != null)
                    {
                        product.Brand = productBrand.InnerText.Trim();
                    }
               
                   var mrp = document.DocumentNode.SelectSingleNode("//div[contains(@itemprop,'offers')]//div[contains(@class,'product-price product-price--large')]");
                    if (mrp != null)
                    {
                        product.MRP = mrp.InnerText.Trim();
                    }

                    var offerprice = document.DocumentNode.SelectSingleNode("//div[contains(@class,\'product-price--large\')]");
                    if (offerprice != null)
                    {
                        product.OfferPrice = offerprice.InnerText.Trim();
                    }


                    var imageUrl = document.DocumentNode.SelectNodes("//nav[contains(@class,'product-nav-items navGalleryScroller')]//img");
                    if (imageUrl != null)
                    {
                        product.Images = imageUrl
                            .Select(node => node.GetAttributeValue("src", string.Empty))
                            .Where(src => !string.IsNullOrEmpty(src))
                            .ToArray();
                    }



                    var productData = document.DocumentNode.SelectNodes("//div[@id='product-features']//div[contains(@class, \'product-description__feature-group-wrapper\')]");


                    if (productData != null)
                    {
                        foreach (var row in productData)
                        {
                            var keyNode = row.SelectSingleNode(".//span[contains(@class, \'product-description__feature-label\')]");
                            var valueNode = row.SelectSingleNode(".//span[contains(@class, \'product-description__feature-value\')]");

                            if (keyNode != null && valueNode != null)
                            {
                                string key = keyNode.InnerText.Trim();
                                string value = valueNode.InnerText.Trim();
                                product.TableData[key] = value;
                            }
                        }
                    }



                    var description = document.DocumentNode.SelectNodes("//div[contains(@class,\'product-description__content-wrapper\')]");
                    if (description != null)
                    {
                        product.Description = description
                            .Select(row =>
                            {
                                string cleanText = Regex.Replace(row.InnerText, @"\s+", " ").Trim();
                                return cleanText;
                            })
                            .Where(text => !string.IsNullOrWhiteSpace(text))
                            .ToArray();
                    }

                    var rating = document.DocumentNode.SelectSingleNode("//span[contains(@class,\'rating-value__value rating-value__value--bolder\')]"); 
                    if (rating != null)
                    {
                        product.ProductRating = rating.InnerText.Trim();
                    }


                }
            }
            catch (HttpRequestException e)
            {
                return StatusCode(500, $"Error fetching product details: {e.Message}");
            }

            return Ok(product);
        }
    }
}
