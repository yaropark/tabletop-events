namespace TabletopEvents.Api.Models.Enums;

/// <summary>What the database did with a capacity-checked registration insert.</summary>
public enum RegistrationInsertStatus
{
    Inserted,
    /// <summary>No row written: event missing, full, or already started.</summary>
    NotInserted,
    /// <summary>A unique index rejected the row: this player is already registered.</summary>
    Duplicate,
}
