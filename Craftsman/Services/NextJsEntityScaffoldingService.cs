namespace Craftsman.Services;

using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Builders.NextJs.Domain;
using Builders.NextJs.Domain.Api;
using Domain;
using Domain.Enums;
using Helpers;
using MediatR;
using Craftsman.Builders.NextJs;
using Craftsman.Builders.NextJs.Domain.Features;

public class NextJsEntityScaffoldingService
{
    private readonly IFileSystem _fileSystem;
    private readonly ICraftsmanUtilities _utilities;
    private readonly IMediator _mediator;

    public NextJsEntityScaffoldingService(ICraftsmanUtilities utilities, IFileSystem fileSystem, IMediator mediator)
    {
        _fileSystem = fileSystem;
        _mediator = mediator;
        _utilities = utilities;
    }

    public void ScaffoldEntities(NextJsEntityTemplate template, string nextSrc)
    {
        var clientName = template.ClientName;
        foreach (var entity in template.Entities)
        {
            new NavigationComponentModifier(_fileSystem).AddFeatureListRouteToNav(nextSrc, entity.Plural);
            
            // apis
            new NextJsApiKeysBuilder(_utilities).CreateDynamicFeatureKeys(nextSrc, entity.Name, entity.Plural);
            new NextJsApiIndexBuilder(_utilities).CreateDynamicFeatureApiIndex(nextSrc, entity.Name, entity.Plural);
            
            new NextJsApiAddEntityBuilder(_utilities).CreateApiFile(nextSrc, 
                entity.Name,
                entity.Plural, 
                clientName);
            new NextJsApiGetListEntityBuilder(_utilities).CreateApiFile(nextSrc, 
                entity.Name,
                entity.Plural, 
                clientName);
            new NextJsApiDeleteEntityBuilder(_utilities).CreateApiFile(nextSrc, 
                entity.Name,
                entity.Plural, 
                clientName);
            new NextJsApiUpdateEntityBuilder(_utilities).CreateApiFile(nextSrc, 
                entity.Name,
                entity.Plural, 
                clientName);
            new NextJsApiGetEntityBuilder(_utilities).CreateApiFile(nextSrc, 
                entity.Name,
                entity.Plural, 
                clientName);
            
            // types
            new NextJsApiTypesBuilder(_utilities).CreateDynamicFeatureTypes(nextSrc, entity.Name, entity.Plural,
                entity.Properties);
            
            // features
            new NextJsEntityListTableBuilder(_utilities).CreateFile(nextSrc, entity.Name, entity.Plural, entity.Properties);
            
            
            
            
            
            
            new DynamicFeatureBuilder(_utilities).CreateDynamicFeatureIndex(nextSrc, entity.Plural);
            new DynamicFeatureRoutesBuilder(_utilities).CreateDynamicFeatureRoutes(nextSrc, entity.Name,
                entity.Plural);
            new NextJsApiTypesBuilder(_utilities).CreateDynamicFeatureTypes(nextSrc, entity.Name, entity.Plural,
                entity.Properties);
            new DynamicFeatureRoutesModifier(_fileSystem).AddRoute(nextSrc, entity.Name, entity.Plural);
            new NavigationComponentModifier(_fileSystem).AddFeatureListRouteToNav(nextSrc, entity.Plural);
            new DynamicFeatureRoutesModifier(_fileSystem).AddRoute(nextSrc, entity.Name, entity.Plural);

            
            
        }
    }
}
