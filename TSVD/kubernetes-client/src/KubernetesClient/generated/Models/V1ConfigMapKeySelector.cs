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
    /// Selects a key from a ConfigMap.
    /// </summary>
    public partial class V1ConfigMapKeySelector
    {
        /// <summary>
        /// Initializes a new instance of the V1ConfigMapKeySelector class.
        /// </summary>
        public V1ConfigMapKeySelector()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1ConfigMapKeySelector class.
        /// </summary>
        /// <param name="key">The key to select.</param>
        /// <param name="name">Name of the referent. More info:
        /// https://kubernetes.io/docs/concepts/overview/working-with-objects/names/#names</param>
        /// <param name="optional">Specify whether the ConfigMap or its key
        /// must be defined</param>
        public V1ConfigMapKeySelector(string key, string name = default(string), bool? optional = default(bool?))
        {
            Key = key;
            Name = name;
            Optional = optional;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the key to select.
        /// </summary>
        [JsonProperty(PropertyName = "key")]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets name of the referent. More info:
        /// https://kubernetes.io/docs/concepts/overview/working-with-objects/names/#names
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets specify whether the ConfigMap or its key must be
        /// defined
        /// </summary>
        [JsonProperty(PropertyName = "optional")]
        public bool? Optional { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Key == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Key");
            }
        }
    }
}