using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using System.Globalization;

namespace AMMS.Infrastructure.Logging
{
    public sealed class GraylogJsonFormatter : ITextFormatter
    {
        private static readonly HashSet<string> ReservedPropertyNames = new(StringComparer.OrdinalIgnoreCase)
        {
            "Timestamp",
            "Level",
            "Message",
            "Exception"
        };

        private readonly JsonValueFormatter _valueFormatter = new(typeTagName: "$type");

        public void Format(LogEvent logEvent, TextWriter output)
        {
            ArgumentNullException.ThrowIfNull(logEvent);
            ArgumentNullException.ThrowIfNull(output);

            output.Write("{\"Timestamp\":\"");
            output.Write(logEvent.Timestamp.UtcDateTime.ToString("O"));
            output.Write("\",\"Level\":\"");
            output.Write(logEvent.Level);
            output.Write("\",\"Message\":");
            JsonValueFormatter.WriteQuotedJsonString(
                logEvent.RenderMessage(CultureInfo.InvariantCulture),
                output);

            if (logEvent.Exception != null)
            {
                output.Write(",\"Exception\":");
                JsonValueFormatter.WriteQuotedJsonString(logEvent.Exception.ToString(), output);
            }

            foreach (var property in logEvent.Properties)
            {
                if (ReservedPropertyNames.Contains(property.Key))
                {
                    continue;
                }

                output.Write(',');
                JsonValueFormatter.WriteQuotedJsonString(property.Key, output);
                output.Write(':');
                _valueFormatter.Format(property.Value, output);
            }

            output.Write('}');
            output.WriteLine();
        }
    }

}
