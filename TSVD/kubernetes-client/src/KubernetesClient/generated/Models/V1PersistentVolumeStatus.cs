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
    /// PersistentVolumeStatus is the current status of a persistent volume.
    /// </summary>
    public partial class V1PersistentVolumeStatus
    {
        /// <summary>
        /// Initializes a new instance of the V1PersistentVolumeStatus class.
        /// </summary>
        public V1PersistentVolumeStatus()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1PersistentVolumeStatus class.
        /// </summary>
        /// <param name="message">A human-readable message indicating details
        /// about why the volume is in this state.</param>
        /// <param name="phase">Phase indicates if a volume is available, bound
        /// to a claim, or released by a claim. More info:
        /// https://kubernetes.io/docs/concepts/storage/persistent-volumes#phase</param>
        /// <param name="reason">Reason is a brief CamelCase string that
        /// describes any failure and is meant for machine parsing and tidy
        /// display in the CLI.</param>
        public V1PersistentVolumeStatus(string message = default(string), string phase = default(string), string reason = default(string))
        {
            Message = message;
            Phase = phase;
            Reason = reason;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets a human-readable message indicating details about why
        /// the volume is in this state.
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets phase indicates if a volume is available, bound to a
        /// claim, or released by a claim. More info:
        /// https://kubernetes.io/docs/concepts/storage/persistent-volumes#phase
        /// </summary>
        [JsonProperty(PropertyName = "phase")]
        public string Phase { get; set; }

        /// <summary>
        /// Gets or sets reason is a brief CamelCase string that describes any
        /// failure and is meant for machine parsing and tidy display in the
        /// CLI.
        /// </summary>
        [JsonProperty(PropertyName = "reason")]
        public string Reason { get; set; }

    }
}
