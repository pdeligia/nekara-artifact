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
    /// ServiceReference holds a reference to Service.legacy.k8s.io
    /// </summary>
    public partial class Apiextensionsv1ServiceReference
    {
        /// <summary>
        /// Initializes a new instance of the Apiextensionsv1ServiceReference
        /// class.
        /// </summary>
        public Apiextensionsv1ServiceReference()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Apiextensionsv1ServiceReference
        /// class.
        /// </summary>
        /// <param name="name">name is the name of the service.
        /// Required</param>
        /// <param name="namespaceProperty">namespace is the namespace of the
        /// service. Required</param>
        /// <param name="path">path is an optional URL path at which the
        /// webhook will be contacted.</param>
        /// <param name="port">port is an optional service port at which the
        /// webhook will be contacted. `port` should be a valid port number
        /// (1-65535, inclusive). Defaults to 443 for backward
        /// compatibility.</param>
        public Apiextensionsv1ServiceReference(string name, string namespaceProperty, string path = default(string), int? port = default(int?))
        {
            Name = name;
            NamespaceProperty = namespaceProperty;
            Path = path;
            Port = port;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets name is the name of the service. Required
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets namespace is the namespace of the service. Required
        /// </summary>
        [JsonProperty(PropertyName = "namespace")]
        public string NamespaceProperty { get; set; }

        /// <summary>
        /// Gets or sets path is an optional URL path at which the webhook will
        /// be contacted.
        /// </summary>
        [JsonProperty(PropertyName = "path")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets port is an optional service port at which the webhook
        /// will be contacted. `port` should be a valid port number (1-65535,
        /// inclusive). Defaults to 443 for backward compatibility.
        /// </summary>
        [JsonProperty(PropertyName = "port")]
        public int? Port { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Name == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Name");
            }
            if (NamespaceProperty == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "NamespaceProperty");
            }
        }
    }
}
