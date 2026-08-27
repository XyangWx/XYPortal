using System;

namespace XYPortal.EvGRPC.SourceCategories;

/// <summary>
/// Aggregate: a single source_category record on evGRpc. The
/// upstream schema treats the id as a string (UUID); the name
/// column is varchar(36) in the live PostgreSQL fixture.
///
/// Invariants:
///   - id               non-blank
///   - name             non-blank; 36 char cap (evGRpc DB limit)
/// </summary>
public class SourceCategory
{
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private SourceCategory() { }

    public SourceCategory(string id, string name)
    {
        Id = ValidateNonBlank(id, nameof(id));
        Name = ValidateName(name);
    }

    /// <summary>
    /// Factory for a not-yet-persisted category. Server assigns id.
    /// Skips the id invariant on the public ctor so we can hand
    /// the wire request through without a server-issued id.
    /// </summary>
    public static SourceCategory Create(string name)
    {
        var validatedName = ValidateName(name);
        var stub = new SourceCategory();
        stub.Name = validatedName;
        // Id stays string.Empty until the server assigns one and
        // the caller wires it back via the protobuf response.
        return stub;
    }

    /// <summary>
    /// Rename the category. Same name constraints as the ctor.
    /// </summary>
    public void SetName(string name) => Name = ValidateName(name);

    private static string ValidateNonBlank(string value, string paramName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{paramName} must be non-blank.", paramName)
            : value;

    private static string ValidateName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("name must be non-blank.", nameof(value));
        if (value.Length > 36)
            throw new ArgumentException(
                $"name must be at most 36 characters (got {value.Length}).",
                nameof(value));
        return value;
    }
}
