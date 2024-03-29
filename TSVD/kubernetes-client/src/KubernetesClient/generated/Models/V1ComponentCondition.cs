// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace k8s.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// Information about the condition of a component.
    /// </summary>
    public partial class V1ComponentCondition
    {
        /// <summary>
        /// Initializes a new instance of the V1ComponentCondition class.
        /// </summary>
        public V1ComponentCondition()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1ComponentCondition class.
        /// </summary>
        /// <param name="status">Status of the condition for a component. Valid
        /// values for "Healthy": "True", "False", or "Unknown".</param>
        /// <param name="type">Type of condition for a component. Valid value:
        /// "Healthy"</param>
        /// <param name="error">Condition error code for a component. For
        /// example, a health check error code.</param>
        /// <param name="message">Message about the condition for a component.
        /// For example, information about a health check.</param>
        public V1ComponentCondition(string status, string type, string error = default(string), string message = default(string))
        {
            Error = error;
            Message = message;
            Status = status;
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets condition error code for a component. For example, a
        /// health check error code.
        /// </summary>
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        /// <summary>
        /// Gets or sets message about the condition for a component. For
        /// example, information about a health check.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets status of the condition for a component. Valid values
        /// for "Healthy": "True", "False", or "Unknown".
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets type of condition for a component. Valid value:
        /// "Healthy"
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Status == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Status");
            }
            if (Type == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Type");
            }
        }
    }
}
