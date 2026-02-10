namespace Lattice.Models;

public class Server
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int Port { get; set; }
    public string MaxMemory { get; set; } = "2G";
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
