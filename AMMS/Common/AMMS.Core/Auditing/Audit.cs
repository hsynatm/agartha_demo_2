namespace AMMS.Core.Auditing
{
    public static class Audit
    {
        public interface IAuditableEntity;
        public enum OperationType
        {
            Create = 1,
            Update = 2,
            Delete = 3
        }

        [AttributeUsage(AttributeTargets.Property)]
        public sealed class IgnoreAttribute : Attribute;
    }

}
