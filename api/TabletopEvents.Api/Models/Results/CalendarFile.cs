namespace TabletopEvents.Api.Models.Results;

/// <summary>A generated .ics file ready to download.</summary>
public sealed record CalendarFile(byte[] Content, string FileName);
