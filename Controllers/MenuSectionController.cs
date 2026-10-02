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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MenuSectionController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration _configuration;
        public MenuSectionController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            _configuration = configuration;
        }
        [HttpGet("GetMenuSectionsById")]
        public async Task<IActionResult> GetMenuSectionsById(int menuId)
        {
            //var data = await dbContext.MenuSubSections
            //    .FromSqlRaw("EXEC dbo.GetMagazineMenuSections @MenuId = {menuId}")
            //    .AsNoTracking()
            //    .ToListAsync();

            string Azurestrgconnection_string = _configuration.GetConnectionString("AzureBlobStorage");
            string blobcotainerName = "swati-sections-test";
            var containerClient = new BlobContainerClient(Azurestrgconnection_string, blobcotainerName);

            string? ToUrl(string? blobName)
            {
                if (string.IsNullOrWhiteSpace(blobName)) return "";

                var blob = containerClient.GetBlobClient(blobName.Trim());
                var sas = new BlobSasBuilder
                {
                    BlobContainerName = containerClient.Name,
                    BlobName = blob.Name,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(30)
                };
                sas.SetPermissions(BlobSasPermissions.Read);
                return blob.GenerateSasUri(sas).ToString();
            }

            var data = await dbContext.MenuSubSections
    .FromSqlRaw("EXEC dbo.GetMagazineMenuSections @MenuId",
                new SqlParameter("@MenuId", menuId))
    .AsNoTracking()
    .ToListAsync();
            var result = data
          .GroupBy(x => x.MenuName)
          .Select(menu => new SwatiMenuDto
          {
              MenuName = menu.Key,

              Sections = menu
                  .GroupBy(x => new
                  {
                      x.SectionTitle,
                      x.SectionName,
                      x.CoverImage
                  })
                  .Select(section => new SwatiSectionDto
                  {
                      SectionTitle = section.Key.SectionTitle,
                      SectionName = section.Key.SectionName,
                      CoverImage = ToUrl(section.Key.CoverImage),

                      Tiles = section
                          .Select(x => new SwatiTileDto
                          {
                              Title = x.Title,
                              Text = x.Text,
                              Image = ToUrl(x.Image)
                          })
                          .ToList()
                  })
                  .ToList()
          })
          .ToList();

            return Ok(result);
        }
    }
}
