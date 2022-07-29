namespace Siberia.FrameworkWebAPI.Controllers
{
    public class PipelinesController : GenericODataController<Pipeline> { public PipelinesController() { Context = new NordStreamDbEntities(); } }
    public class SocietiesController : GenericODataController<Society> { public SocietiesController() { Context = new NordStreamDbEntities(); } }
}