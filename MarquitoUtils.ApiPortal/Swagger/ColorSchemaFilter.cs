using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Drawing;

namespace MarquitoUtils.ApiPortal.Swagger
{
    /// <summary>
    /// Modifies the OpenAPI schema for properties of type <see cref="Color"/> to represent them as hexadecimal color
    /// strings in generated API documentation.
    /// </summary>
    /// <remarks>This filter is intended for use with OpenAPI/Swagger schema generation tools to ensure that
    /// properties of type <see cref="Color"/> are documented as strings in hexadecimal color format (e.g., "#FF0000").
    /// This improves the accuracy and clarity of the API documentation for consumers who need to supply or interpret
    /// color values.</remarks>
    public sealed class ColorSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(Color))
            {
                schema.Type = "string";
                schema.Format = "hex-color";
                schema.Example = new Microsoft.OpenApi.Any.OpenApiString("#FF0000");
            }
        }
    }
}
