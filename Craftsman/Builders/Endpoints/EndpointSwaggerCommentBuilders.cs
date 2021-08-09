namespace Craftsman.Builders.Endpoints
{
    using System;
    using System.Collections.Generic;
    using Helpers;
    using Models;

    public class EndpointSwaggerCommentBuilders
    {
        public static string GetSwaggerComments_GetList(Entity entity, bool buildComments, string listResponse, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);

            if (buildComments)
                return $@"
        /// <summary>
        /// Gets a list of all {entity.Plural}.
        /// </summary>
        /// <response code=""200"">{entity.Name} list returned successfully.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        /// <remarks>
        /// Requests can be narrowed down with a variety of query string values:
        /// ## Query String Parameters
        /// - **PageNumber**: An integer value that designates the page of records that should be returned.
        /// - **PageSize**: An integer value that designates the number of records returned on the given page that you would like to return. This value is capped by the internal MaxPageSize.
        /// - **SortOrder**: A comma delimited ordered list of property names to sort by. Adding a `-` before the name switches to sorting descendingly.
        /// - **Filters**: A comma delimited list of fields to filter by formatted as `{{Name}}{{Operator}}{{Value}}` where
        ///     - {{Name}} is the name of a filterable property. You can also have multiple names (for OR logic) by enclosing them in brackets and using a pipe delimiter, eg. `(LikeCount|CommentCount)>10` asks if LikeCount or CommentCount is >10
        ///     - {{Operator}} is one of the Operators below
        ///     - {{Value}} is the value to use for filtering. You can also have multiple values (for OR logic) by using a pipe delimiter, eg.`Title@= new|hot` will return posts with titles that contain the text ""new"" or ""hot""
        ///
        ///    | Operator | Meaning                       | Operator  | Meaning                                      |
        ///    | -------- | ----------------------------- | --------- | -------------------------------------------- |
        ///    | `==`     | Equals                        |  `!@=`    | Does not Contains                            |
        ///    | `!=`     | Not equals                    |  `!_=`    | Does not Starts with                         |
        ///    | `>`      | Greater than                  |  `@=*`    | Case-insensitive string Contains             |
        ///    | `&lt;`   | Less than                     |  `_=*`    | Case-insensitive string Starts with          |
        ///    | `>=`     | Greater than or equal to      |  `==*`    | Case-insensitive string Equals               |
        ///    | `&lt;=`  | Less than or equal to         |  `!=*`    | Case-insensitive string Not equals           |
        ///    | `@=`     | Contains                      |  `!@=*`   | Case-insensitive string does not Contains    |
        ///    | `_=`     | Starts with                   |  `!_=*`   | Case-insensitive string does not Starts with |
        /// </remarks>
        [ProducesResponseType(typeof({listResponse}), 200)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        public static string GetSwaggerComments_GetRecord(Entity entity, bool buildComments, string singleResponse, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);

            if (buildComments)
                return $@"
        /// <summary>
        /// Gets a single {entity.Name} by ID.
        /// </summary>
        /// <response code=""200"">{entity.Name} record returned successfully.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(typeof({singleResponse}), 200)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        public static string GetSwaggerComments_CreateRecord(Entity entity, bool buildComments, string singleResponse, bool hasAuthentications, bool hasGuidPk)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
            var conflictResponses = GetConflictResponses(hasGuidPk);
            return buildComments ? $@"
        /// <summary>
        /// Creates a new {entity.Name} record.
        /// </summary>
        /// <response code=""201"">{entity.Name} created.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(typeof({singleResponse}), 201)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}{conflictResponses}
        [ProducesResponseType(500)]" : "";
        }

        public static string GetSwaggerComments_DeleteRecord(Entity entity, bool buildComments, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
            if (buildComments)
                return $@"
        /// <summary>
        /// Deletes an existing {entity.Name} record.
        /// </summary>
        /// <response code=""204"">{entity.Name} deleted.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        public static string GetSwaggerComments_PatchRecord(Entity entity, bool buildComments, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
            if (buildComments)
                return $@"
        /// <summary>
        /// Updates specific properties on an existing {entity.Name}.
        /// </summary>
        /// <response code=""204"">{entity.Name} updated.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        public static string GetSwaggerComments_PutRecord(Entity entity, bool buildComments, bool hasAuthentications)
        {
            var authResponses = GetAuthResponses(hasAuthentications);
            var authCommentResponses = GetAuthCommentResponses(hasAuthentications);
            if (buildComments)
                return $@"
        /// <summary>
        /// Updates an entire existing {entity.Name}.
        /// </summary>
        /// <response code=""204"">{entity.Name} updated.</response>
        /// <response code=""400"">{entity.Name} has missing/invalid values.</response>{authCommentResponses}
        /// <response code=""500"">There was an error on the server while creating the {entity.Name}.</response>
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(Response<>), 400)]{authResponses}
        [ProducesResponseType(500)]";

            return "";
        }

        public static string BuildAuthorizations(List<Policy> policies, Endpoint endpoint, string entityName)
        {
            var results = Utilities.GetEndpointPolicies(policies, endpoint, entityName);

            var authorizations = "";
            foreach (var result in results)
            {
                if (result.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Scope))
                //|| result.PolicyType == Enum.GetName(typeof(PolicyType), PolicyType.Claim))
                {
                    authorizations += $@"{Environment.NewLine}        [Authorize(Policy = ""{result.Name}"")]";
                }
                else
                {
                    authorizations += $@"{Environment.NewLine}        [Authorize(Roles = ""{result.Name}"")]";
                }
            }

            return authorizations;
        }

        public static string GetAuthResponses(bool hasAuthentications)
        {
            var authResponses = "";
            if (hasAuthentications)
            {
                authResponses = $@"
        [ProducesResponseType(typeof(Response<>), 401)]
        [ProducesResponseType(typeof(Response<>), 403)]";
            }

            return authResponses;
        }

        public static string GetConflictResponses(bool hasConflictResponse)
        {
            var conflictResponses = "";
            if (hasConflictResponse)
            {
                conflictResponses = $@"
        [ProducesResponseType(typeof(Response<>), 409)]";
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

        public static string GetConflictCommentResponses(bool hasConflictResponse)
        {
            var responseComments = "";
            if (hasConflictResponse)
            {
                responseComments = $@"
        /// <response code=""409"">A record already exists with this primary key.</response>";
            }

            return responseComments;
        }
    }
}