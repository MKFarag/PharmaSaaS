namespace PharmaSaaS.Presentation.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class NoTenantRequiredAttribute : Attribute;
