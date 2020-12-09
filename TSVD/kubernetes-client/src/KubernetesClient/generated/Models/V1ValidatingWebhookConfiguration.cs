// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace k8s.Models
{
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// ValidatingWebhookConfiguration describes the configuration of and
    /// admission webhook that accept or reject and object without changing it.
    /// </summary>
    public partial class V1ValidatingWebhookConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the V1ValidatingWebhookConfiguration
        /// class.
        /// </summary>
        public V1ValidatingWebhookConfiguration()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1ValidatingWebhookConfiguration
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
        /// <param name="metadata">Standard object metadata; More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#metadata.</param>
        /// <param name="webhooks">Webhooks is a list of webhooks and the
        /// affected resources and operations.</param>
        public V1ValidatingWebhookConfiguration(string apiVersion = default(string), string kind = default(string), V1ObjectMeta metadata = default(V1ObjectMeta), IList<V1ValidatingWebhook> webhooks = default(IList<V1ValidatingWebhook>))
        {
            ApiVersion = apiVersion;
            Kind = kind;
            Metadata = metadata;
            Webhooks = webhooks;
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
        /// Gets or sets standard object metadata; More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#metadata.
        /// </summary>
        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }

        /// <summary>
        /// Gets or sets webhooks is a list of webhooks and the affected
        /// resources and operations.
        /// </summary>
        [JsonProperty(PropertyName = "webhooks")]
        public IList<V1ValidatingWebhook> Webhooks { get; set; }

    }
}
