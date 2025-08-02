namespace BlockiFinAid.Data.DTOs;

public class UserAccessControlDto
{
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public IEnumerable<string> Permissions { get; set; } =  new List<string>();
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UAMResponse : UserAccessControlDto
{
    public Guid UserId { get; set; }
  
}