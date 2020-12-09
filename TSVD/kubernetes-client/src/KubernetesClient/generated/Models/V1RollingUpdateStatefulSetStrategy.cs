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
    /// RollingUpdateStatefulSetStrategy is used to communicate parameter for
    /// RollingUpdateStatefulSetStrategyType.
    /// </summary>
    public partial class V1RollingUpdateStatefulSetStrategy
    {
        /// <summary>
        /// Initializes a new instance of the
        /// V1RollingUpdateStatefulSetStrategy class.
        /// </summary>
        public V1RollingUpdateStatefulSetStrategy()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the
        /// V1RollingUpdateStatefulSetStrategy class.
        /// </summary>
        /// <param name="partition">Partition indicates the ordinal at which
        /// the StatefulSet should be partitioned. Default value is 0.</param>
        public V1RollingUpdateStatefulSetStrategy(int? partition = default(int?))
        {
            Partition = partition;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets partition indicates the ordinal at which the
        /// StatefulSet should be partitioned. Default value is 0.
        /// </summary>
        [JsonProperty(PropertyName = "partition")]
        public int? Partition { get; set; }

    }
}
