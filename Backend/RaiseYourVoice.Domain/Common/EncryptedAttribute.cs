using System;

namespace RaiseYourVoice.Domain.Common
{
    /// <summary>
    /// Attribute to mark properties that should be encrypted at rest
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class EncryptedAttribute : Attribute
    {
    }
}