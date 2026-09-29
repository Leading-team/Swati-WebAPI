using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWWebAPI.Data;
using SWWebAPI.Models;
using SWWebAPI.Models.Entities;

namespace SWWebAPI.Controllers
{
    [Route("api/menulist")]
    [ApiController]
    [Authorize]
    public class MenuListController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public MenuListController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet("GetAllMenus")]
        public IActionResult GetAllMenus(string language = "te")
        {
            return Ok(dbContext.menu_list.Where(m => m.language == language).ToList());

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
    }
}
