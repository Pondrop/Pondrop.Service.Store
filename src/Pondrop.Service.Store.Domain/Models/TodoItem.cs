namespace Pondrop.Service.Store.Domain.Models;

public record TodoItem
{
    public TodoItem()
    {
        Id = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
        Completed = false;
        Category = string.Empty;
    }

    public string Id { get; init; }

    public string Name { get; init; }

    public string Description { get; init; }

    public bool Completed { get; init; }

    public string Category { get; init; }
}