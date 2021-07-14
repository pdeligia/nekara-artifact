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
    /// HorizontalPodAutoscaler is the configuration for a horizontal pod
    /// autoscaler, which automatically manages the replica count of any
    /// resource implementing the scale subresource based on the metrics
    /// specified.
    /// </summary>
    public partial class V2beta1HorizontalPodAutoscaler
    {
        /// <summary>
        /// Initializes a new instance of the V2beta1HorizontalPodAutoscaler
        /// class.
        /// </summary>
        public V2beta1HorizontalPodAutoscaler()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V2beta1HorizontalPodAutoscaler
        /// class.
        /// </summary>
        /// <param name="apiVersion">APIVersion defines the versioned schema of
        /// this representation of an object. Servers should convert recognized
        /// schemas to the latest internal value, and may reject unrecognized
        /// values. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#resources</param>
        /// <param name="kind">Kind is a string value representing the REST
        /// resource this object represents. Servers may infer this from the
        /// endpoint the client submits requests to. Cannot be updated. In
        /// CamelCase. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds</param>
        /// <param name="metadata">metadata is the standard object metadata.
        /// More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#metadata</param>
        /// <param name="spec">spec is the specification for the behaviour of
        /// the autoscaler. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#spec-and-status.</param>
        /// <param name="status">status is the current information about the
        /// autoscaler.</param>
        public V2beta1HorizontalPodAutoscaler(string apiVersion = default(string), string kind = default(string), V1ObjectMeta metadata = default(V1ObjectMeta), V2beta1HorizontalPodAutoscalerSpec spec = default(V2beta1HorizontalPodAutoscalerSpec), V2beta1HorizontalPodAutoscalerStatus status = default(V2beta1HorizontalPodAutoscalerStatus))
        {
            ApiVersion = apiVersion;
            Kind = kind;
            Metadata = metadata;
            Spec = spec;
            Status = status;
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
        /// Gets or sets kind is a string value representing the REST resource
        /// this object represents. Servers may infer this from the endpoint
        /// the client submits requests to. Cannot be updated. In CamelCase.
        /// More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds
        /// </summary>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets metadata is the standard object metadata. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#metadata
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

        /// <summary>
        /// Gets or sets spec is the specification for the behaviour of the
        /// autoscaler. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#spec-and-status.
        /// </summary>
        [JsonProperty(PropertyName = "spec")]
        public V2beta1HorizontalPodAutoscalerSpec Spec { get; set; }

        /// <summary>
        /// Gets or sets status is the current information about the
        /// autoscaler.
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public V2beta1HorizontalPodAutoscalerStatus Status { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Spec != null)
            {
                Spec.Validate();
            }
            if (Status != null)
            {
                Status.Validate();
            }
        }
    }
}
