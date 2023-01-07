namespace Craftsman.Builders.Endpoints;

using System;
using Domain;

public class EndpointSwaggerCommentBuilders
{
    public static string GetSwaggerComments_GetList(Entity entity, bool buildComments, string listResponse, bool hasAuthentications)
    {
        if (buildComments)
            return $@"
    /// <summary>
    /// Gets a list of all {entity.Plural}.
    /// </summary>";

        return "";
    }

    public static string GetSwaggerComments_GetRecord(Entity entity, bool buildComments, string singleResponse, bool hasAuthentications)
    {
        return buildComments ? $@"
    /// <summary>
    /// Gets a single {entity.Name} by ID.
    /// </summary>" : "";
    }

    public static string GetSwaggerComments_CreateRecord(Entity entity, bool buildComments, string singleResponse, bool hasAuthentications)
    {
        return buildComments ? $@"
    /// <summary>
    /// Creates a new {entity.Name} record.
    /// </summary>" : "";
    }

    public static string GetSwaggerComments_CreateList(Entity entity, bool buildComments, string singleResponse, bool hasAuthentications)
    {
        return buildComments ? $@"
    /// <summary>
    /// Creates one or more {entity.Name} records.
    /// </summary>" : "";
    }

    public static string GetSwaggerComments_DeleteRecord(Entity entity, bool buildComments, bool hasAuthentications)
    {
        return buildComments ? $@"
    /// <summary>
    /// Deletes an existing {entity.Name} record.
    /// </summary>" : "";
    }

    public static string GetSwaggerComments_PatchRecord(Entity entity, bool buildComments, bool hasAuthentications)
    {
        var authResponses = GetAuthResponses(hasAuthentications);
        var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
        return buildComments ? $@"
    /// <summary>
    /// Updates specific properties on an existing {entity.Name}.
    /// </summary>" : "";
    }

    public static string GetSwaggerComments_PutRecord(Entity entity, bool buildComments, bool hasAuthentications)
    {
        var authResponses = GetAuthResponses(hasAuthentications);
        var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
        return buildComments ? $@"
    /// <summary>
    /// Updates an entire existing {entity.Name}.
    /// </summary>" : "";
    }

    public static string BuildAuthorizations()
    {
        return $@"{Environment.NewLine}    [Authorize]";
    }

    public static string GetAuthResponses(bool hasAuthentications)
    {
        var authResponses = "";
        if (hasAuthentications)
        {
            authResponses = $@"
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]";
        }

        return authResponses;
    }

    public static string GetConflictResponses(bool hasConflictResponse)
    {
        var conflictResponses = "";
        if (hasConflictResponse)
        {
            conflictResponses = $@"
    [ProducesResponseType(409)]";
        }

        return conflictResponses;
    }

    public static string GetAuthCommentResponses(bool hasAuthentications)
    {
        var authResponseComments = "";
        if (hasAuthentications)
        {
            authResponseComments = $@"
    /// <response code=""401"">This request was not able to be authenticated.</response>
    /// <response code=""403"">The required permissions to access this resource were not present in the given request.</response>";
        }

        return authResponseComments;
    }
}
