using System.ComponentModel.DataAnnotations;


namespace Catalog.Api.Dtos
{
  public record CreateItemDto
  {
    [Required]
    [RegularExpression(@"^[a-zA-Z0-9]{1,40}$", 
         ErrorMessage = "Name can only contain characters and numbers")]
    public string Name { get; init; }
    [Required]
    [Range(1, 1000)]
    public decimal Price { get; init; }
  }
}