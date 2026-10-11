using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CURSOVA.Infrastructure.Persistence;

// SQLite has no native date type, so values come back as DateTimeKind.Unspecified.
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v.ToUniversalTime(),
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
