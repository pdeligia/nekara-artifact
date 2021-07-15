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
    /// HorizontalPodAutoscalerCondition describes the state of a
    /// HorizontalPodAutoscaler at a certain point.
    /// </summary>
    public partial class V2beta1HorizontalPodAutoscalerCondition
    {
        /// <summary>
        /// Initializes a new instance of the
        /// V2beta1HorizontalPodAutoscalerCondition class.
        /// </summary>
        public V2beta1HorizontalPodAutoscalerCondition()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// V2beta1HorizontalPodAutoscalerCondition class.
        /// </summary>
        /// <param name="status">status is the status of the condition (True,
        /// False, Unknown)</param>
        /// <param name="type">type describes the current condition</param>
        /// <param name="lastTransitionTime">lastTransitionTime is the last
        /// time the condition transitioned from one status to another</param>
        /// <param name="message">message is a human-readable explanation
        /// containing details about the transition</param>
        /// <param name="reason">reason is the reason for the condition's last
        /// transition.</param>
        public V2beta1HorizontalPodAutoscalerCondition(string status, string type, System.DateTime? lastTransitionTime = default(System.DateTime?), string message = default(string), string reason = default(string))
        {
            LastTransitionTime = lastTransitionTime;
            Message = message;
            Reason = reason;
            Status = status;
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets lastTransitionTime is the last time the condition
        /// transitioned from one status to another
        /// </summary>
        [JsonProperty(PropertyName = "lastTransitionTime")]
        public System.DateTime? LastTransitionTime { get; set; }

        /// <summary>
        /// Gets or sets message is a human-readable explanation containing
        /// details about the transition
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets reason is the reason for the condition's last
        /// transition.
        /// </summary>
        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets status is the status of the condition (True, False,
        /// Unknown)
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets type describes the current condition
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
