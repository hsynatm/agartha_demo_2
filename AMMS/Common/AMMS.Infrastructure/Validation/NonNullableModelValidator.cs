using System.Collections;
using System.Reflection;
using AMMS.Core.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AMMS.Infrastructure.Validation;

/// <summary>
/// Validates non-nullable model members without DataAnnotations.
/// Empty JSON values are bound by converters; this pass rejects missing/empty required values.
/// </summary>
public static class NonNullableModelValidator
{
    private static readonly NullabilityInfoContext NullabilityContext = new();

    public static void Validate(object model, ModelStateDictionary modelState, string prefix = "")
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(modelState);

        foreach (var property in model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var value = property.GetValue(model);
            var key = BuildKey(prefix, property.Name);
            var propertyType = property.PropertyType;

            if (IsNonNullableString(property))
            {
                if (value is not string text || string.IsNullOrWhiteSpace(text))
                {
                    AddRequiredError(modelState, key);
                }

                continue;
            }

            if (propertyType == typeof(Guid))
            {
                if (value is Guid guid && guid == Guid.Empty)
                {
                    AddRequiredError(modelState, key);
                }

                continue;
            }

            if (propertyType == typeof(DateTime))
            {
                if (value is DateTime dateTime && dateTime == default)
                {
                    AddRequiredError(modelState, key);
                }

                continue;
            }

            if (TryGetCollectionElementType(propertyType, out var elementType))
            {
                if (value is not IEnumerable items)
                {
                    continue;
                }

                var index = 0;
                foreach (var item in items)
                {
                    if (item is not null && ShouldValidateComplexType(elementType))
                    {
                        Validate(item, modelState, $"{key}[{index}]");
                    }

                    index++;
                }

                continue;
            }

            if (ShouldValidateComplexType(propertyType) && value is not null)
            {
                Validate(value, modelState, key);
            }
        }
    }

    private static bool IsNonNullableString(PropertyInfo property) =>
        property.PropertyType == typeof(string)
        && NullabilityContext.Create(property).ReadState == NullabilityState.NotNull;

    private static bool TryGetCollectionElementType(Type type, out Type elementType)
    {
        elementType = typeof(object);

        if (type == typeof(string))
        {
            return false;
        }

        if (type.IsArray)
        {
            elementType = type.GetElementType()!;
            return true;
        }

        if (type.IsGenericType)
        {
            var definition = type.GetGenericTypeDefinition();
            if (definition == typeof(IEnumerable<>) || definition == typeof(ICollection<>) || definition == typeof(IList<>))
            {
                elementType = type.GetGenericArguments()[0];
                return true;
            }
        }

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        if (enumerableInterface is null)
        {
            return false;
        }

        elementType = enumerableInterface.GetGenericArguments()[0];
        return true;
    }

    private static bool ShouldValidateComplexType(Type type) =>
        type.IsClass && type != typeof(string);

    private static void AddRequiredError(ModelStateDictionary modelState, string key) =>
        modelState.AddModelError(key, LocalizationKeys.Shared.Required);

    private static string BuildKey(string prefix, string propertyName) =>
        string.IsNullOrEmpty(prefix) ? propertyName : $"{prefix}.{propertyName}";
}
