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
    /// HorizontalPodAutoscalerBehavior configures the scaling behavior of the
    /// target in both Up and Down directions (scaleUp and scaleDown fields
    /// respectively).
    /// </summary>
    public partial class V2beta2HorizontalPodAutoscalerBehavior
    {
        /// <summary>
        /// Initializes a new instance of the
        /// V2beta2HorizontalPodAutoscalerBehavior class.
        /// </summary>
        public V2beta2HorizontalPodAutoscalerBehavior()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// V2beta2HorizontalPodAutoscalerBehavior class.
        /// </summary>
        /// <param name="scaleDown">scaleDown is scaling policy for scaling
        /// Down. If not set, the default value is to allow to scale down to
        /// minReplicas pods, with a 300 second stabilization window (i.e., the
        /// highest recommendation for the last 300sec is used).</param>
        /// <param name="scaleUp">scaleUp is scaling policy for scaling Up. If
        /// not set, the default value is the higher of:
        /// * increase no more than 4 pods per 60 seconds
        /// * double the number of pods per 60 seconds
        /// No stabilization is used.</param>
        public V2beta2HorizontalPodAutoscalerBehavior(V2beta2HPAScalingRules scaleDown = default(V2beta2HPAScalingRules), V2beta2HPAScalingRules scaleUp = default(V2beta2HPAScalingRules))
        {
            ScaleDown = scaleDown;
            ScaleUp = scaleUp;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets scaleDown is scaling policy for scaling Down. If not
        /// set, the default value is to allow to scale down to minReplicas
        /// pods, with a 300 second stabilization window (i.e., the highest
        /// recommendation for the last 300sec is used).
        /// </summary>
        [JsonProperty(PropertyName = "scaleDown")]
        public V2beta2HPAScalingRules ScaleDown { get; set; }

        /// <summary>
        /// Gets or sets scaleUp is scaling policy for scaling Up. If not set,
        /// the default value is the higher of:
        /// * increase no more than 4 pods per 60 seconds
        /// * double the number of pods per 60 seconds
        /// No stabilization is used.
        /// </summary>
        [JsonProperty(PropertyName = "scaleUp")]
        public V2beta2HPAScalingRules ScaleUp { get; set; }

    }
}
