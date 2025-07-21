namespace BlockiFinAid.Data.Responses;

public class ServiceResponse<T>
{
    public T? Data { get; set; }
    public IEnumerable<string?> Error { get; set; } = new List<string?>();
    public DateTime Timestamp  { get; set; } = DateTime.UtcNow;
    public bool IsSuccess { get; set; } = false;
}