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
    //[Authorize]
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
            var result = new List<SwatiMenuDto>();

            try
            {
                // Validate configuration
                string? storageConnectionString =
           _configuration.GetConnectionString("AzureBlobStorage");

                if (string.IsNullOrWhiteSpace(storageConnectionString))
                {
                    //_logger.LogError(
                    //    "AzureBlobStorage connection string is missing.");

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Storage configuration is unavailable."
                    });
                }
                string containerName = "swati-sections-test";
                var containerClient =
                    new BlobContainerClient(storageConnectionString, containerName);

                string? ToUrl(string? blobName)
                {
                    if (string.IsNullOrWhiteSpace(blobName))
                        return string.Empty;

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

                //        var data = await dbContext.MenuSubSections
                //.FromSqlRaw("EXEC dbo.GetMagazineMenuSections @MenuId",
                //            new SqlParameter("@MenuId", menuId))
                //.AsNoTracking()
                //.ToListAsync();
                const string sql = @"
    SELECT
        M.menu_name AS MenuName,
        S.te_section_name AS SectionTitle,
        S.en_section_name AS SectionName,
        ISNULL(S.navlink, '') AS NavLink,
        '' AS SectionDesc,
        '' AS CoverImage,
        ISNULL(SC.sub_caption, '') AS Title,
        ISNULL(SC.sub_caption_en, '') AS TitleEn,
        ISNULL(SC.content, '') AS Text,
        ISNULL(SC.content_en, '') AS TextEn,
        ISNULL(SC.filename, '') AS Image
    FROM menu_list M
    INNER JOIN Sections S
        ON M.menu_id = S.Menu_Id
    INNER JOIN section_configuration SC
        ON S.Section_Id = SC.section_id
    WHERE M.menu_id = @MenuId
    ORDER BY
        M.menu_id,
        S.Section_Id,
        SC.priority;";
                List<MenuSubSectionsDto> data;
                try
                {
                    data = await dbContext.MenuSubSections
              .FromSqlRaw(sql, new SqlParameter("@MenuId", menuId))
              .AsNoTracking()
              .ToListAsync();
                }
                catch (Microsoft.Data.SqlClient.SqlException ex)
                {
                    //_logger.LogError(
                    //    ex,
                    //    "Database error retrieving menu sections. MenuId: {MenuId}",
                    //    menuId);

                    return StatusCode(500, new
                    {
                        success = false,
                        message = "Unable to retrieve menu sections from the database."
                    });
                }
                result = data
          .GroupBy(x => x.MenuName)
          .Select(menu => new SwatiMenuDto
          {
              MenuName = menu.Key,

              Sections = menu
                  .GroupBy(x => new
                  {
                      x.SectionTitle,
                      x.SectionName,
                      x.NavLink,
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
                              TitleEn = x.TitleEn,
                              TextEn = x.TextEn,
                              Image = ToUrl(x.Image)
                          })
                          .ToList()
                  })
                  .ToList()
          })
          .ToList();
                return Ok(result);
            }
            catch (Azure.RequestFailedException ex)
            {
                //_logger.LogError(
                //    ex,
                //    "Azure Blob Storage error retrieving menu images. MenuId: {MenuId}",
                //    menuId);

                return StatusCode(502, new
                {
                    success = false,
                    message = "Unable to retrieve menu images from storage."
                });
            }
            catch (Exception ex)
            {
                //_logger.LogError(
                //    ex,
                //    "Unexpected error retrieving menu sections. MenuId: {MenuId}",
                //    menuId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while retrieving menu sections."
                });
            }
        }
    }
}
