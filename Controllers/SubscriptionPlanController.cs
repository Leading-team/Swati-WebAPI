using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SWWebAPI.Data;
using SWWebAPI.Models;
using SWWebAPI.Models.Entities;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SWWebAPI.Controllers
{
    [Route("api/subscriptionplan")]
    [ApiController]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IConfiguration _configuration;
        public SubscriptionPlanController(ApplicationDbContext dbContext, IConfiguration configuration)
        {
            this.dbContext = dbContext;
            _configuration = configuration;
        }
        
        [HttpGet("GetAllSubscriptionPlans")]
        public async Task<IActionResult> GetAllSubscriptionPlans()
        {
            int CountryId = 1;
            int LanguageId = 23;
            try
            {
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
                string containerName = "swati-images";
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


                var json = new StringBuilder();
                var sql = @"
    SELECT
    m.Title,
    m.image_path as Image,
    JSON_QUERY((
        SELECT
           pp.planpriceid PlanPriceId,
           p.Plan_name PlanName,
            pp.Price,
            pp.courier_price Courier_Price
        FROM Subscription_Plan p
        JOIN PlanPrice pp
            ON pp.Plan_Id = p.Plan_Id
        WHERE p.Magazine_Id = m.Magazine_Id
          AND pp.Country_Id = @CountryId
          AND p.Active = 1
          AND pp.EffectiveFrom <= CAST(GETDATE() AS DATE)
          AND (
              pp.EffectiveTo IS NULL
              OR pp.EffectiveTo >= CAST(GETDATE() AS DATE)
          )
        FOR JSON PATH
    )) AS Plans
FROM Magazine m
WHERE m.Active = 1 and language_id = @LanguageId
ORDER BY m.Magazine_Id
FOR JSON PATH;";
                var connection = dbContext.Database.GetDbConnection();
                var shouldClose = connection.State != System.Data.ConnectionState.Open;
                string jsonResult = "[]";
                try
                {
                    if (shouldClose)
                        await connection.OpenAsync();

                    await using var command = connection.CreateCommand();
                    command.CommandText = sql;

                    var countryParam = command.CreateParameter();
                    countryParam.ParameterName = "@CountryId";
                    countryParam.Value = CountryId;
                    command.Parameters.Add(countryParam);

                    var languageParam = command.CreateParameter();
                    languageParam.ParameterName = "@LanguageId";
                    languageParam.Value = LanguageId;
                    command.Parameters.Add(languageParam);

                    var jsonBuilder = new StringBuilder();

                    await using var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        jsonBuilder.Append(reader.GetString(0));
                    }
                    var magazines = JsonNode.Parse(jsonBuilder.ToString()) as JsonArray
                    ?? new JsonArray();

                    foreach (var magazine in magazines)
                    {
                        if (magazine is not JsonObject magazineObject)
                            continue;

                        string imagePath = magazineObject["Image"]?.GetValue<string>();

                        magazineObject["Image"] = ToUrl(imagePath);
                    }

                    jsonResult = magazines.ToJsonString(
                        new JsonSerializerOptions
                        {
                            WriteIndented = false
                        });
                    jsonResult = jsonResult.Length > 0 ? jsonResult.ToString() : "[]";
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    if (shouldClose)
                        await connection.CloseAsync();
                }
                return Content(jsonResult, "application/json");

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
                    message = "Unable to retrieve subscription images from storage."
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
                    message = "An unexpected error occurred while retrieving subscription plans."
                });
            }
            /* var conn = dbContext.Database.GetDbConnection();
             var wasClosed = conn.State == ConnectionState.Closed;
             if (wasClosed) await conn.OpenAsync();

             try
             {
                 await using var cmd = conn.CreateCommand();
                 cmd.CommandText = "dbo.GetAllSubscriptionPlans";
                 cmd.CommandType = CommandType.StoredProcedure;

                 //var pCountry = cmd.CreateParameter();
                 //pCountry.ParameterName = "@CountryId";
                 //pCountry.Value = countryId;
                 //cmd.Parameters.Add(pCountry);

                 //var pLang = cmd.CreateParameter();
                 //pLang.ParameterName = "@LanguageId";
                 //pLang.Value = (object?)languageId ?? DBNull.Value;
                 //cmd.Parameters.Add(pLang);

                 await using var reader = await cmd.ExecuteReaderAsync();
                 while (await reader.ReadAsync())
                 {
                     json.Append(reader.GetString(0));   // FOR JSON comes back in chunks of ~2 KB
                 }

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
             finally
             {
                 if (wasClosed) await conn.CloseAsync();
             }

             // No rows come back when nothing matches, so return an empty array
             var result = json.Length == 0 ? "[]" : json.ToString();
             return Content(result, "application/json");*/
        }

        [HttpPost("AddSubscriptionPlan")]
        public IActionResult AddSubscriptionPlan(AddSubscriptionPlanDto addSubscriptionPlanDto)
        {
            var subscriptionEntity = new subscription_plan()
            {
                plan_name = addSubscriptionPlanDto.plan_name,
                created_by = addSubscriptionPlanDto.created_by,

            };
            //dbContext.subscription_plan.Add(subscriptionEntity);
            //dbContext.SaveChanges();
            return Ok(subscriptionEntity);
        }


    }
}
