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
    /// PodsMetricSource indicates how to scale on a metric describing each pod
    /// in the current scale target (for example,
    /// transactions-processed-per-second). The values will be averaged
    /// together before being compared to the target value.
    /// </summary>
    public partial class V2beta1PodsMetricSource
    {
        /// <summary>
        /// Initializes a new instance of the V2beta1PodsMetricSource class.
        /// </summary>
        public V2beta1PodsMetricSource()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V2beta1PodsMetricSource class.
        /// </summary>
        /// <param name="metricName">metricName is the name of the metric in
        /// question</param>
        /// <param name="targetAverageValue">targetAverageValue is the target
        /// value of the average of the metric across all relevant pods (as a
        /// quantity)</param>
        /// <param name="selector">selector is the string-encoded form of a
        /// standard kubernetes label selector for the given metric When set,
        /// it is passed as an additional parameter to the metrics server for
        /// more specific metrics scoping When unset, just the metricName will
        /// be used to gather metrics.</param>
        public V2beta1PodsMetricSource(string metricName, ResourceQuantity targetAverageValue, V1LabelSelector selector = default(V1LabelSelector))
        {
            MetricName = metricName;
            Selector = selector;
            TargetAverageValue = targetAverageValue;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets metricName is the name of the metric in question
        /// </summary>
        [JsonProperty(PropertyName = "metricName")]
        public string MetricName { get; set; }

        /// <summary>
        /// Gets or sets selector is the string-encoded form of a standard
        /// kubernetes label selector for the given metric When set, it is
        /// passed as an additional parameter to the metrics server for more
        /// specific metrics scoping When unset, just the metricName will be
        /// used to gather metrics.
        /// </summary>
        [JsonProperty(PropertyName = "selector")]
        public V1LabelSelector Selector { get; set; }

        /// <summary>
        /// Gets or sets targetAverageValue is the target value of the average
        /// of the metric across all relevant pods (as a quantity)
        /// </summary>
        [JsonProperty(PropertyName = "targetAverageValue")]
        public ResourceQuantity TargetAverageValue { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (MetricName == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "MetricName");
            }
            if (TargetAverageValue == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "TargetAverageValue");
            }
        }
    }
}
