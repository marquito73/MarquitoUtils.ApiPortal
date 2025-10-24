using MarquitoUtils.Main.Class.Service.Sql;
using MarquitoUtils.Main.Class.Sql;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MarquitoUtils.WebPortalAPI.Class.Controllers
{
    public abstract class DefaultController<DBContext> : ControllerBase
        where DBContext : DefaultDbContext
    {
        protected ILogger<DefaultController<DBContext>> Logger { get; set; }
        /// <summary>
        /// The entity service
        /// </summary>
        protected IEntityService EntityService { get; set; }

        public DefaultController(ILogger<DefaultController<DBContext>> logger, IEntityService entityService)
        {
            this.Logger = logger;
            this.EntityService = entityService;
        }
    }
}
