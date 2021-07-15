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
    /// ObjectFieldSelector selects an APIVersioned field of an object.
    /// </summary>
    public partial class V1ObjectFieldSelector
    {
        /// <summary>
        /// Initializes a new instance of the V1ObjectFieldSelector class.
        /// </summary>
        public V1ObjectFieldSelector()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1ObjectFieldSelector class.
        /// </summary>
        /// <param name="fieldPath">Path of the field to select in the
        /// specified API version.</param>
        /// <param name="apiVersion">Version of the schema the FieldPath is
        /// written in terms of, defaults to "v1".</param>
        public V1ObjectFieldSelector(string fieldPath, string apiVersion = default(string))
        {
            ApiVersion = apiVersion;
            FieldPath = fieldPath;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets version of the schema the FieldPath is written in
        /// terms of, defaults to "v1".
        /// </summary>
        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets path of the field to select in the specified API
        /// version.
        /// </summary>
        [JsonProperty(PropertyName = "fieldPath")]
        public string FieldPath { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (FieldPath == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "FieldPath");
            }
        }
    }
}
