namespace Pondrop.Service.Store.Application.Models;

public class SubmissionViewTopicConfiguration
{
    public const string Key = nameof(SubmissionViewTopicConfiguration);
    
    public string Endpoint { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
}
