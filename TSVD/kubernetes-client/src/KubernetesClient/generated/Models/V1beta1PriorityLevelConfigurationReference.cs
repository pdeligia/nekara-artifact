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
    /// PriorityLevelConfigurationReference contains information that points to
    /// the "request-priority" being used.
    /// </summary>
    public partial class V1beta1PriorityLevelConfigurationReference
    {
        /// <summary>
        /// Initializes a new instance of the
        /// V1beta1PriorityLevelConfigurationReference class.
        /// </summary>
        public V1beta1PriorityLevelConfigurationReference()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// V1beta1PriorityLevelConfigurationReference class.
        /// </summary>
        /// <param name="name">`name` is the name of the priority level
        /// configuration being referenced Required.</param>
        public V1beta1PriorityLevelConfigurationReference(string name)
        {
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets `name` is the name of the priority level configuration
        /// being referenced Required.
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Name");
            }
        }
    }
}
