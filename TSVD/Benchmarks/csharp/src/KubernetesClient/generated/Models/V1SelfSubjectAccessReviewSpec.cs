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
    /// SelfSubjectAccessReviewSpec is a description of the access request.
    /// Exactly one of ResourceAuthorizationAttributes and
    /// NonResourceAuthorizationAttributes must be set
    /// </summary>
    public partial class V1SelfSubjectAccessReviewSpec
    {
        /// <summary>
        /// Initializes a new instance of the V1SelfSubjectAccessReviewSpec
        /// class.
        /// </summary>
        public V1SelfSubjectAccessReviewSpec()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1SelfSubjectAccessReviewSpec
        /// class.
        /// </summary>
        /// <param name="nonResourceAttributes">NonResourceAttributes describes
        /// information for a non-resource access request</param>
        /// <param name="resourceAttributes">ResourceAuthorizationAttributes
        /// describes information for a resource access request</param>
        public V1SelfSubjectAccessReviewSpec(V1NonResourceAttributes nonResourceAttributes = default(V1NonResourceAttributes), V1ResourceAttributes resourceAttributes = default(V1ResourceAttributes))
        {
            NonResourceAttributes = nonResourceAttributes;
            ResourceAttributes = resourceAttributes;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets nonResourceAttributes describes information for a
        /// non-resource access request
        /// </summary>
        [JsonProperty(PropertyName = "nonResourceAttributes")]
        public V1NonResourceAttributes NonResourceAttributes { get; set; }

        /// <summary>
        /// Gets or sets resourceAuthorizationAttributes describes information
        /// for a resource access request
        /// </summary>
        [JsonProperty(PropertyName = "resourceAttributes")]
        public V1ResourceAttributes ResourceAttributes { get; set; }

    }
}
