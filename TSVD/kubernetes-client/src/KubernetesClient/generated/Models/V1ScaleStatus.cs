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
    /// ScaleStatus represents the current status of a scale subresource.
    /// </summary>
    public partial class V1ScaleStatus
    {
        /// <summary>
        /// Initializes a new instance of the V1ScaleStatus class.
        /// </summary>
        public V1ScaleStatus()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1ScaleStatus class.
        /// </summary>
        /// <param name="replicas">actual number of observed instances of the
        /// scaled object.</param>
        /// <param name="selector">label query over pods that should match the
        /// replicas count. This is same as the label selector but in the
        /// string format to avoid introspection by clients. The string will be
        /// in the same format as the query-param syntax. More info about label
        /// selectors:
        /// http://kubernetes.io/docs/user-guide/labels#label-selectors</param>
        public V1ScaleStatus(int replicas, string selector = default(string))
        {
            Replicas = replicas;
            Selector = selector;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets actual number of observed instances of the scaled
        /// object.
        /// </summary>
        [JsonProperty(PropertyName = "replicas")]
        public int Replicas { get; set; }

        /// <summary>
        /// Gets or sets label query over pods that should match the replicas
        /// count. This is same as the label selector but in the string format
        /// to avoid introspection by clients. The string will be in the same
        /// format as the query-param syntax. More info about label selectors:
        /// http://kubernetes.io/docs/user-guide/labels#label-selectors
        /// </summary>
        [JsonProperty(PropertyName = "selector")]
        public string Selector { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            //Nothing to validate
        }
    }
}
