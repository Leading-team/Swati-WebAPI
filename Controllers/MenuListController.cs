using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SWWebAPI.Data;
using SWWebAPI.Handler;
using SWWebAPI.Models;
using SWWebAPI.Models.Entities;

namespace SWWebAPI.Controllers
{
    [Route("api/menulist")]
    [ApiController]
    //[Authorize]
    public class MenuListController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration _configuration;
        public MenuListController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            _configuration = configuration;
        }
        [HttpGet("GetAllMenus")]
        public IActionResult GetAllMenus(string language = "te")
        {

            try
            {
                var languages = dbContext.menu_list
                                         .Where(m => m.language == language)
                                         .ToList();

                return Ok(languages);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // Log database connection or SQL errors
                return StatusCode(503, new
                {
                    success = false,
                    message = "Unable to connect to the database. Please try again later."
                });
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while retrieving languages."
                });
            }

            // return Ok(dbContext.menu_list.Where(m => m.language == language).ToList());
        }
        [HttpPost("AddMenu")]
        public IActionResult AddMenu(AddMenuDto addMenuDto)
        {
            var menuEntity = new menu_list()
            {
                menu_name = addMenuDto.menu_name,
                route_name = addMenuDto.route_name,
                language = addMenuDto.language,
            };
            dbContext.menu_list.Add(menuEntity);
            dbContext.SaveChanges();
            return Ok(menuEntity);
        }
        [HttpGet("GetImageListBySource")]
        public async Task<IActionResult> GetImageListBySource(string source = "")
        {

            string Azurestrgconnection_string = _configuration.GetConnectionString("AzureBlobStorage");
            string blobcotainerName = "swati-images";
            var containerClient = new BlobContainerClient(Azurestrgconnection_string, blobcotainerName);
            List<swati_images> images = new List<swati_images>();
            try
            {
                images = await dbContext.swati_images
         .Where(m => string.IsNullOrEmpty(source) || m.source == source)
   .ToListAsync();
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                return StatusCode(503, new
                {
                    success = false,
                    message = "Unable to connect to the database. Please try again later."
                });
            }
            var result = new List<object>();
            var files = new List<string>();
            try
            {
                foreach (var image in images)
                {
                    BlobClient blobClient = containerClient.GetBlobClient(image.filename);

                    if (!await blobClient.ExistsAsync())
                    {
                        //logger.LogWarning("Blob not found: {Uri}", blobClient.Uri);
                        continue;
                    }

                    var sasBuilder = new BlobSasBuilder
                    {
                        BlobContainerName = containerClient.Name,
                        BlobName = blobClient.Name,
                        Resource = "b",
                        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
                    };
                    sasBuilder.SetPermissions(BlobSasPermissions.Read);
                    result.Add(new
                    {
                        source = image.source,
                        fileUrl = blobClient.GenerateSasUri(sasBuilder).ToString()
                    });

                }
            }
            catch (Azure.RequestFailedException ex)
            {
                //_logger.LogError(ex, "Azure Blob Storage error while retrieving image URLs.");

                return StatusCode(503, new
                {
                    success = false,
                    message = "Unable to access image storage. Please try again later."
                });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Unexpected error while generating image URLs.");

                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while retrieving images."
                });

            }

            return Ok(result);
        }

    }
}
