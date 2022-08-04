using Newtonsoft.Json;

namespace Pondrop.Service.Store.Infrastructure.Models;

public class TodoItemEntity
{
    public TodoItemEntity()
    {
        Id = Guid.NewGuid().ToString();
        Name = string.Empty;
        Description = string.Empty;
        Completed = false;
        Category = string.Empty;
    }

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; set; }

    [JsonProperty(PropertyName = "description")]
    public string Description { get; set; }

    [JsonProperty(PropertyName = "isComplete")]
    public bool Completed { get; set; }

    [JsonProperty(PropertyName = "category")]
    public string Category { get; set; }

    public override string ToString() => JsonConvert.SerializeObject(this);
}