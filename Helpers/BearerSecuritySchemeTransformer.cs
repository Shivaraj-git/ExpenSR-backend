using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace ExpenSR.Helpers
{
    /// <summary>
    /// Registers a "Bearer" security scheme on the generated OpenAPI document and
    /// requires it on every operation, so Scalar's UI shows a token input and
    /// sends "Authorization: Bearer {token}" on requests you try from the browser.
    /// </summary>
    internal sealed class BearerSecuritySchemeTransformer(
        IAuthenticationSchemeProvider authenticationSchemeProvider) : IOpenApiDocumentTransformer
    {
        public async Task TransformAsync(
            OpenApiDocument document,
            OpenApiDocumentTransformerContext context,
            CancellationToken cancellationToken)
        {
            var schemes = await authenticationSchemeProvider.GetAllSchemesAsync();
            if (!schemes.Any(s => s.Name == "Bearer"))
                return;

            var bearerScheme = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token (without the word 'Bearer')"
            };

            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
            {
                ["Bearer"] = bearerScheme
            };

            // Require it on every operation so Scalar prompts for a token everywhere
            foreach (var pathItem in document.Paths.Values)
            {
                foreach (var operation in pathItem.Operations.Values)
                {
                    operation.Security ??= [];
                    operation.Security.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                    });
                }
            }
        }
    }
}