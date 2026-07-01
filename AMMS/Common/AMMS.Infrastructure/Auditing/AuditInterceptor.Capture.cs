using AMMS.Shared.Auditing;
using AMMS.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.Json;

namespace AMMS.Infrastructure.Auditing
{

	public sealed partial class AuditInterceptor
	{
		private static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			WriteIndented = false
		};

		private static readonly string[] SoftDeleteAuditPropertyNames =
		[
			nameof(BaseEntity.IsDeleted),
			nameof(BaseEntity.UpdatedAt),
			nameof(BaseEntity.UpdatedBy)
		];

		private List<AuditEntry> CaptureEntries(DbContext context)
		{
			EnsureModuleNameConfigured();

			context.ChangeTracker.DetectChanges();

			var entries = new List<AuditEntry>();
			var contextMetadata = BuildContextMetadata();

			foreach (var entry in context.ChangeTracker.Entries())
			{
				if (entry.Entity is AuditLogEntry or not Audit.IAuditableEntity)
				{
					continue;
				}

				var auditEntry = BuildAuditEntry(entry, contextMetadata);
				if (auditEntry is not null)
				{
					entries.Add(auditEntry);
				}
			}

			return entries;
		}

		private void EnsureModuleNameConfigured()
		{
			if (string.IsNullOrWhiteSpace(_moduleName))
			{
				throw new InvalidOperationException("Audit module name is required.");
			}
		}

		private AuditEntry? BuildAuditEntry(EntityEntry entry, ContextMetadata metadata)
		{
			var operationType = ResolveOperationType(entry);
			if (operationType is null)
			{
				return null;
			}

			var (oldValues, newValues, changedColumns) = BuildValueSnapshots(entry, operationType.Value);

			if (operationType == Audit.OperationType.Update && changedColumns is null)
			{
				return null;
			}

			if (operationType == Audit.OperationType.Create && string.IsNullOrWhiteSpace(newValues))
			{
				return null;
			}

			if (operationType == Audit.OperationType.Delete
				&& entry.State != EntityState.Modified
				&& string.IsNullOrWhiteSpace(oldValues))
			{
				return null;
			}

			if (operationType == Audit.OperationType.Delete
				&& entry.State == EntityState.Modified
				&& string.IsNullOrWhiteSpace(newValues))
			{
				return null;
			}

			return new AuditEntry(
				ModuleName: _moduleName,
				EntityName: ResolveEntityName(entry),
				EntityId: ResolveEntityId(entry),
				OperationType: operationType.Value,
				OldValues: oldValues,
				NewValues: newValues,
				ChangedColumns: changedColumns,
				OrganizationId: metadata.OrganizationId,
				UserId: metadata.UserId,
				IpAddress: metadata.IpAddress,
				TraceId: metadata.TraceId);
		}

		private static Audit.OperationType? ResolveOperationType(EntityEntry entry)
		{
			return entry.State switch
			{
				EntityState.Added => Audit.OperationType.Create,
				EntityState.Deleted => Audit.OperationType.Delete,
				EntityState.Modified when IsSoftDelete(entry) => Audit.OperationType.Delete,
				EntityState.Modified => Audit.OperationType.Update,
				_ => null
			};
		}

		private static bool IsSoftDelete(EntityEntry entry)
		{
			var isDeletedProperty = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(BaseEntity.IsDeleted));
			return isDeletedProperty is { IsModified: true, CurrentValue: true }
				&& !ValuesEqual(isDeletedProperty.OriginalValue, isDeletedProperty.CurrentValue);
		}

		private static (string? OldValues, string? NewValues, string? ChangedColumns) BuildValueSnapshots(EntityEntry entry,Audit.OperationType operationType)
		{
			if (operationType == Audit.OperationType.Delete && entry.State == EntityState.Modified)
			{
				return BuildSoftDeleteSnapshots(entry);
			}

			var auditableProperties = GetIncludedProperties(entry).ToList();

			return operationType switch
			{
				Audit.OperationType.Create => (null, SerializeProperties(auditableProperties, useOriginal: false), null),
				Audit.OperationType.Delete => (SerializeProperties(auditableProperties, useOriginal: true), null, null),
				Audit.OperationType.Update => BuildUpdateSnapshots(auditableProperties),
				_ => (null, null, null)
			};
		}

		private static (string? OldValues, string? NewValues, string? ChangedColumns) BuildSoftDeleteSnapshots(EntityEntry entry)
		{
			var changedProperties = entry.Properties
				.Where(property => SoftDeleteAuditPropertyNames.Contains(property.Metadata.Name))
				.Where(property => property.IsModified && !ValuesEqual(property.OriginalValue, property.CurrentValue))
				.ToList();

			if (changedProperties.Count == 0)
			{
				return (null, null, null);
			}

			var oldSnapshot = new Dictionary<string, object?>(StringComparer.Ordinal);
			var newSnapshot = new Dictionary<string, object?>(StringComparer.Ordinal);
			var changedColumnNames = new List<string>();

			foreach (var property in changedProperties)
			{
				var propertyName = property.Metadata.Name;
				oldSnapshot[propertyName] = NormalizeValue(property.OriginalValue);
				newSnapshot[propertyName] = NormalizeValue(property.CurrentValue);
				changedColumnNames.Add(propertyName);
			}

			return (
				JsonSerializer.Serialize(oldSnapshot, JsonOptions),
				JsonSerializer.Serialize(newSnapshot, JsonOptions),
				JsonSerializer.Serialize(changedColumnNames, JsonOptions));
		}

		private static (string? OldValues, string? NewValues, string? ChangedColumns) BuildUpdateSnapshots(IReadOnlyList<PropertyEntry> auditableProperties)
		{
			var changedProperties = auditableProperties
				.Where(p => p.IsModified && !ValuesEqual(p.OriginalValue, p.CurrentValue))
				.ToList();

			if (changedProperties.Count == 0)
			{
				return (null, null, null);
			}

			var oldSnapshot = new Dictionary<string, object?>(StringComparer.Ordinal);
			var newSnapshot = new Dictionary<string, object?>(StringComparer.Ordinal);
			var changedColumnNames = new List<string>();

			foreach (var property in changedProperties)
			{
				var propertyName = property.Metadata.Name;
				oldSnapshot[propertyName] = NormalizeValue(property.OriginalValue);
				newSnapshot[propertyName] = NormalizeValue(property.CurrentValue);
				changedColumnNames.Add(propertyName);
			}

			return (
				JsonSerializer.Serialize(oldSnapshot, JsonOptions),
				JsonSerializer.Serialize(newSnapshot, JsonOptions),
				JsonSerializer.Serialize(changedColumnNames, JsonOptions));
		}

		private static string? SerializeProperties(IEnumerable<PropertyEntry> properties, bool useOriginal)
		{
			var snapshot = new Dictionary<string, object?>(StringComparer.Ordinal);

			foreach (var property in properties)
			{
				snapshot[property.Metadata.Name] = NormalizeValue(useOriginal ? property.OriginalValue : property.CurrentValue);
			}

			return snapshot.Count == 0 ? null : JsonSerializer.Serialize(snapshot, JsonOptions);
		}

		private static bool ValuesEqual(object? original, object? current)
		{
			if (original is null && current is null)
			{
				return true;
			}

			if (original is null || current is null)
			{
				return false;
			}

			return Equals(NormalizeValue(original), NormalizeValue(current));
		}

		private static object? NormalizeValue(object? value)
		{
			return value switch
			{
				null => null,
				Enum enumValue => enumValue.ToString(),
				DateTime dateTime => dateTime,
				DateTimeOffset dateTimeOffset => dateTimeOffset,
				_ => value
			};
		}

		private static string ResolveEntityName(EntityEntry entry)
		{
			var tableAttribute = entry.Entity.GetType().GetCustomAttribute<TableAttribute>();
			return tableAttribute?.Name ?? entry.Entity.GetType().Name;
		}

		private static string ResolveEntityId(EntityEntry entry)
		{
			var idProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())
				?? entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(BaseEntity.Id));

			var idValue = idProperty?.CurrentValue ?? idProperty?.OriginalValue;
			return idValue?.ToString() ?? string.Empty;
		}

		private ContextMetadata BuildContextMetadata()
		{
			var httpContext = _httpContextAccessor.HttpContext;
			var currentUser = _currentUserService.CurrentUser;

			// TODO: Auth altyapısı tamamlandığında gerçek OrganizationId buradan alınacak.
			var organizationId = _currentOrganizationService.CurrentOrganization?.OrganizationId;

			return new ContextMetadata(
				OrganizationId: organizationId,
				UserId: currentUser?.UserId,
				IpAddress: httpContext?.Connection.RemoteIpAddress?.ToString(),
				TraceId: httpContext?.TraceIdentifier);
		}

		private static IEnumerable<PropertyEntry> GetIncludedProperties(EntityEntry entry)
		{
			return entry.Properties.Where(property => ShouldInclude(entry, property));
		}

		private static bool ShouldInclude(EntityEntry entry, PropertyEntry property)
		{
			if (property.Metadata.IsPrimaryKey())
			{
				return false;
			}

			if (AlwaysIgnoredPropertyNames.Contains(property.Metadata.Name))
			{
				return false;
			}

			if (HasAuditIgnoreAttribute(entry, property.Metadata.Name))
			{
				return false;
			}

			return true;
		}

		private static bool HasAuditIgnoreAttribute(EntityEntry entry, string propertyName)
		{
			for (var type = entry.Entity.GetType(); type is not null; type = type.BaseType)
			{
				var propertyInfo = type.GetProperty(
					propertyName,
					BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				if (propertyInfo is not null && Attribute.IsDefined(propertyInfo, typeof(Audit.IgnoreAttribute)))
				{
					return true;
				}
			}

			return false;
		}


		private static readonly HashSet<string> AlwaysIgnoredPropertyNames =
			new(StringComparer.Ordinal)
			{
			"RowVersion",
			"ConcurrencyStamp"
			};

		private sealed record ContextMetadata(
			string? OrganizationId,
			string? UserId,
			string? IpAddress,
			string? TraceId);
	}









}
