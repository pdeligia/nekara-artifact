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
    /// Eviction evicts a pod from its node subject to certain policies and
    /// safety constraints. This is a subresource of Pod.  A request to cause
    /// such an eviction is created by POSTing to .../pods/&lt;pod
    /// name&gt;/evictions.
    /// </summary>
    public partial class V1beta1Eviction
    {
        /// <summary>
        /// Initializes a new instance of the V1beta1Eviction class.
        /// </summary>
        public V1beta1Eviction()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1beta1Eviction class.
        /// </summary>
        /// <param name="apiVersion">APIVersion defines the versioned schema of
        /// this representation of an object. Servers should convert recognized
        /// schemas to the latest internal value, and may reject unrecognized
        /// values. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources</param>
        /// <param name="deleteOptions">DeleteOptions may be provided</param>
        /// <param name="kind">Kind is a string value representing the REST
        /// resource this object represents. Servers may infer this from the
        /// endpoint the client submits requests to. Cannot be updated. In
        /// CamelCase. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds</param>
        /// <param name="metadata">ObjectMeta describes the pod that is being
        /// evicted.</param>
        public V1beta1Eviction(string apiVersion = default(string), V1DeleteOptions deleteOptions = default(V1DeleteOptions), string kind = default(string), V1ObjectMeta metadata = default(V1ObjectMeta))
        {
            ApiVersion = apiVersion;
            DeleteOptions = deleteOptions;
            Kind = kind;
            Metadata = metadata;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets aPIVersion defines the versioned schema of this
        /// representation of an object. Servers should convert recognized
        /// schemas to the latest internal value, and may reject unrecognized
        /// values. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources
        /// </summary>
        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or sets deleteOptions may be provided
        /// </summary>
        [JsonProperty(PropertyName = "deleteOptions")]
        public V1DeleteOptions DeleteOptions { get; set; }

        /// <summary>
        /// Gets or sets kind is a string value representing the REST resource
        /// this object represents. Servers may infer this from the endpoint
        /// the client submits requests to. Cannot be updated. In CamelCase.
        /// More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds
        /// </summary>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets objectMeta describes the pod that is being evicted.
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

    }
}
