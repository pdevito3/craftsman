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
    [Authorize]
    [HttpPut(""{{userId:guid}}/addRole"", Name = ""AddRole"")]
    public async Task<IActionResult> AddRole([FromRoute] Guid userId, [FromBody] string role)
    {{
        var command = new AddUserRole.Command(userId, role);
        await mediator.Send(command);
        return NoContent();
    }}";
    }
    public static string RemoveRoleEndpoint()
    {
        return @$"    /// <summary>
    /// Removes a role from a User
    /// </summary>
    [Authorize]
    [HttpPut(""{{userId:guid}}/removeRole"", Name = ""RemoveRole"")]
    public async Task<ActionResult> RemoveRole([FromRoute] Guid userId, [FromBody] string role)
    {{
        var command = new RemoveUserRole.Command(userId, role);
        await mediator.Send(command);
        return NoContent();
    }}";
    }
}
