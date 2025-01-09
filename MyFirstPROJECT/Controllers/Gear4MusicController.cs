using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyFirstPROJECT.Models;

namespace MyFirstPROJECT.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Gear4MusicController : ControllerBase
    {
        private readonly ILogger<Gear4MusicController> _logger;

        public Gear4MusicController(ILogger<Gear4MusicController> logger)
        {
            _logger = logger;
        }

        [HttpGet]

        public async Task<IActionResult> ScrapeProduct([FromQuery] string url = "https://www.gear4music.com/Keyboards-and-Pianos/Yamaha-PSR-E473-Portable-Keyboard/4M4B")
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

                    //_logger.LogInformation(document.DocumentNode.OuterHtml);

                    var Categories = document.DocumentNode.SelectNodes("//div[contains(@class,\'pdp-breadcrumb\')]//li//span");
                    if (Categories != null)
                    {
                        product.Category = Categories
                            .Select(row => $"{row.InnerText.Trim()}")
                            .ToArray();
                    }


                    var name = document.DocumentNode.SelectSingleNode("//h1[contains(@itemprop,\'name\')]");
                    if (name != null)
                    {
                        product.Name = name.InnerText.Trim();
                    }

                    

                    var brand = document.DocumentNode.SelectSingleNode("//div[contains(@class,'manufacturer-logo')]//meta");
                    if (brand != null)
                    {
                        product.Brand = brand.InnerText.Trim();
                    }

                    var mrp = document.DocumentNode.SelectSingleNode("//span[contains(@class,\'info-row-price\')]");
                    if (mrp != null)
                    {
                        product.MRP = mrp.InnerText.Trim();
                    }

                    var offerprice = document.DocumentNode.SelectSingleNode("//div[contains(@class,\'product-price--large\')]");
                    if (offerprice != null)
                    {
                        product.OfferPrice = offerprice.InnerText.Trim();
                    }


                    var imageUrl = document.DocumentNode.SelectNodes("//div[contains(@class,\"switcher hide-mobile images\")]//ul//li//img");
                    if (imageUrl != null)
                    {
                        product.Images = imageUrl
                            .Select(node => node.GetAttributeValue("src", string.Empty))
                            .Where(src => !string.IsNullOrEmpty(src))
                            .ToArray();
                    }



                    var specificationNodes = document.DocumentNode.SelectNodes("//div[contains(@class,\'col-xs-12\')]//div[contains(@class,\'row row-top-border full-description\')]");

                    if (specificationNodes != null)
                    {
                        foreach (var detailNode in specificationNodes)
                        {

                            var keyNode = detailNode.SelectSingleNode(".//div[contains(@class,\'slide\')]//ul/li");
                            var valueNode = keyNode?.NextSibling?.InnerText?.Trim();

                            if (keyNode != null && !string.IsNullOrEmpty(valueNode))
                            {
                                string specName = keyNode.InnerText.Trim(':').Trim();
                                string specValue = valueNode;

                                product.TableData[specName] = specValue;
                            }
                        }
                    }



                    var description = document.DocumentNode.SelectNodes("//div[contains(@class,\"col-xs-12\")]//div[contains(@class,\"row row-top-border full-description\")]//ul[7]/li");
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

                    var rating = document.DocumentNode.SelectSingleNode("//span[contains(@class,\'rating\')]//span"); 
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
