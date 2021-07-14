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
    /// VolumeAttachmentSource represents a volume that should be attached.
    /// Right now only PersistenVolumes can be attached via external attacher,
    /// in future we may allow also inline volumes in pods. Exactly one member
    /// can be set.
    /// </summary>
    public partial class V1alpha1VolumeAttachmentSource
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1VolumeAttachmentSource
        /// class.
        /// </summary>
        public V1alpha1VolumeAttachmentSource()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1VolumeAttachmentSource
        /// class.
        /// </summary>
        /// <param name="inlineVolumeSpec">inlineVolumeSpec contains all the
        /// information necessary to attach a persistent volume defined by a
        /// pod's inline VolumeSource. This field is populated only for the
        /// CSIMigration feature. It contains translated fields from a pod's
        /// inline VolumeSource to a PersistentVolumeSpec. This field is
        /// alpha-level and is only honored by servers that enabled the
        /// CSIMigration feature.</param>
        /// <param name="persistentVolumeName">Name of the persistent volume to
        /// attach.</param>
        public V1alpha1VolumeAttachmentSource(V1PersistentVolumeSpec inlineVolumeSpec = default(V1PersistentVolumeSpec), string persistentVolumeName = default(string))
        {
            InlineVolumeSpec = inlineVolumeSpec;
            PersistentVolumeName = persistentVolumeName;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets inlineVolumeSpec contains all the information
        /// necessary to attach a persistent volume defined by a pod's inline
        /// VolumeSource. This field is populated only for the CSIMigration
        /// feature. It contains translated fields from a pod's inline
        /// VolumeSource to a PersistentVolumeSpec. This field is alpha-level
        /// and is only honored by servers that enabled the CSIMigration
        /// feature.
        /// </summary>
        [JsonProperty(PropertyName = "inlineVolumeSpec")]
        public V1PersistentVolumeSpec InlineVolumeSpec { get; set; }

        /// <summary>
        /// Gets or sets name of the persistent volume to attach.
        /// </summary>
        [JsonProperty(PropertyName = "persistentVolumeName")]
        public string PersistentVolumeName { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="Microsoft.Rest.ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (InlineVolumeSpec != null)
            {
                InlineVolumeSpec.Validate();
            }
        }
    }
}
