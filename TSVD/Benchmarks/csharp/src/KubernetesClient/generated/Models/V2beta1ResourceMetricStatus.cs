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
    /// ResourceMetricStatus indicates the current value of a resource metric
    /// known to Kubernetes, as specified in requests and limits, describing
    /// each pod in the current scale target (e.g. CPU or memory).  Such
    /// metrics are built in to Kubernetes, and have special scaling options on
    /// top of those available to normal per-pod metrics using the "pods"
    /// source.
    /// </summary>
    public partial class V2beta1ResourceMetricStatus
    {
        /// <summary>
        /// Initializes a new instance of the V2beta1ResourceMetricStatus
        /// class.
        /// </summary>
        public V2beta1ResourceMetricStatus()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V2beta1ResourceMetricStatus
        /// class.
        /// </summary>
        /// <param name="currentAverageValue">currentAverageValue is the
        /// current value of the average of the resource metric across all
        /// relevant pods, as a raw value (instead of as a percentage of the
        /// request), similar to the "pods" metric source type. It will always
        /// be set, regardless of the corresponding metric
        /// specification.</param>
        /// <param name="name">name is the name of the resource in
        /// question.</param>
        /// <param name="currentAverageUtilization">currentAverageUtilization
        /// is the current value of the average of the resource metric across
        /// all relevant pods, represented as a percentage of the requested
        /// value of the resource for the pods.  It will only be present if
        /// `targetAverageValue` was set in the corresponding metric
        /// specification.</param>
        public V2beta1ResourceMetricStatus(ResourceQuantity currentAverageValue, string name, int? currentAverageUtilization = default(int?))
        {
            CurrentAverageUtilization = currentAverageUtilization;
            CurrentAverageValue = currentAverageValue;
            Name = name;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets currentAverageUtilization is the current value of the
        /// average of the resource metric across all relevant pods,
        /// represented as a percentage of the requested value of the resource
        /// for the pods.  It will only be present if `targetAverageValue` was
        /// set in the corresponding metric specification.
        /// </summary>
        [JsonProperty(PropertyName = "currentAverageUtilization")]
        public int? CurrentAverageUtilization { get; set; }

        /// <summary>
        /// Gets or sets currentAverageValue is the current value of the
        /// average of the resource metric across all relevant pods, as a raw
        /// value (instead of as a percentage of the request), similar to the
        /// "pods" metric source type. It will always be set, regardless of the
        /// corresponding metric specification.
        /// </summary>
        [JsonProperty(PropertyName = "currentAverageValue")]
        public ResourceQuantity CurrentAverageValue { get; set; }

        /// <summary>
        /// Gets or sets name is the name of the resource in question.
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
            if (CurrentAverageValue == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "CurrentAverageValue");
            }
            if (Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Name");
            }
        }
    }
}
