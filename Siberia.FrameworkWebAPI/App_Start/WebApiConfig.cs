using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using System.Web.Http;

namespace Siberia.FrameworkWebAPI
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var odataBatchHandler = new DefaultODataBatchHandler(GlobalConfiguration.DefaultServer);
            odataBatchHandler.MessageQuotas.MaxNestingDepth = 2;
            odataBatchHandler.MessageQuotas.MaxOperationsPerChangeset = 10;
            odataBatchHandler.MessageQuotas.MaxReceivedMessageSize = 100;

            // NordStreamDb
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Pipeline>("Pipelines");
            builder.EntitySet<Society>("Societies");
            config.MapODataServiceRoute(
                routeName: "NordStreamDb",
                routePrefix: "NordStreamDb",
                model: builder.GetEdmModel(),
                batchHandler: odataBatchHandler);
            config.EnableCors();
        }
    }
}
