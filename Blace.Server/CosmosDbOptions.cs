using System.ComponentModel.DataAnnotations;

namespace Blace.Server;

public class CosmosDbOptions
{
    [Required] public string ConnectionString { get; set; } = null!;
}