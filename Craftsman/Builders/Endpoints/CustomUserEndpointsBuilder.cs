namespace Craftsman.Builders.Endpoints;

using Domain;
using Domain.Enums;
using Helpers;
using Services;

public class CustomUserEndpointsBuilder
{
    public static string AddRoleEndpoint()
    {
        return @$"    /// <summary>
    /// Adds a new role to a user.
    /// </summary>
    /// <response code=""204"">Role added.</response>
    /// <response code=""400"">Request has missing/invalid values.</response>
    /// <response code=""401"">This request was not able to be authenticated.</response>
    /// <response code=""403"">The required permissions to access this resource were not present in the given request.</response>
    /// <response code=""500"">There was an error on the server while adding the role.</response>
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [Consumes(""application/json"")]
    [Authorize]
    [HttpPut(""{{userId:guid}}/addRole"", Name = ""AddRole"")]
    public async Task<IActionResult> AddRole([FromRoute] Guid userId, [FromBody] string role)
    {{
        var command = new AddUserRole.Command(userId, role);
        await _mediator.Send(command);
        return NoContent();
    }}";
    }
    public static string RemoveRoleEndpoint()
    {
        return @$"    /// <summary>
    /// Removes a role from a User
    /// </summary>
    /// <response code=""204"">Role removed.</response>
    /// <response code=""400"">Request has missing/invalid values.</response>
    /// <response code=""401"">This request was not able to be authenticated.</response>
    /// <response code=""403"">The required permissions to access this resource were not present in the given request.</response>
    /// <response code=""500"">There was an error on the server while removing the UserRole.</response>
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    [Consumes(""application/json"")]
    [Authorize]
    [HttpDelete(""{{userId:guid}}/removeRole"", Name = ""RemoveRole"")]
    public async Task<ActionResult> RemoveRole([FromRoute] Guid userId, [FromBody] string role)
    {{
        var command = new RemoveUserRole.Command(userId, role);
        await _mediator.Send(command);
        return NoContent();
    }}";
    }
}
