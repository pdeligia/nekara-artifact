// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace k8s.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// ScaleSpec describes the attributes of a scale subresource.
    /// </summary>
    public partial class V1ScaleSpec
    {
        /// <summary>
        /// Initializes a new instance of the V1ScaleSpec class.
        /// </summary>
        public V1ScaleSpec()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1ScaleSpec class.
        /// </summary>
        /// <param name="replicas">desired number of instances for the scaled
        /// object.</param>
        public V1ScaleSpec(int? replicas = default(int?))
        {
            Replicas = replicas;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets desired number of instances for the scaled object.
        /// </summary>
        [JsonProperty(PropertyName = "replicas")]
        public int? Replicas { get; set; }

    }
}
