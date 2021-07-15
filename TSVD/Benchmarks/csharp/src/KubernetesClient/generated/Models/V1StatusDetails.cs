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
    /// StatusDetails is a set of additional properties that MAY be set by the
    /// server to provide additional information about a response. The Reason
    /// field of a Status object defines what attributes will be set. Clients
    /// must ignore fields that do not match the defined type of each
    /// attribute, and should assume that any attribute may be empty, invalid,
    /// or under defined.
    /// </summary>
    public partial class V1StatusDetails
    {
        /// <summary>
        /// Initializes a new instance of the V1StatusDetails class.
        /// </summary>
        public V1StatusDetails()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1StatusDetails class.
        /// </summary>
        /// <param name="causes">The Causes array includes more details
        /// associated with the StatusReason failure. Not all StatusReasons may
        /// provide detailed causes.</param>
        /// <param name="group">The group attribute of the resource associated
        /// with the status StatusReason.</param>
        /// <param name="kind">The kind attribute of the resource associated
        /// with the status StatusReason. On some operations may differ from
        /// the requested resource Kind. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds</param>
        /// <param name="name">The name attribute of the resource associated
        /// with the status StatusReason (when there is a single name which can
        /// be described).</param>
        /// <param name="retryAfterSeconds">If specified, the time in seconds
        /// before the operation should be retried. Some errors may indicate
        /// the client must take an alternate action - for those errors this
        /// field may indicate how long to wait before taking the alternate
        /// action.</param>
        /// <param name="uid">UID of the resource. (when there is a single
        /// resource which can be described). More info:
        /// http://kubernetes.io/docs/user-guide/identifiers#uids</param>
        public V1StatusDetails(IList<V1StatusCause> causes = default(IList<V1StatusCause>), string group = default(string), string kind = default(string), string name = default(string), int? retryAfterSeconds = default(int?), string uid = default(string))
        {
            Causes = causes;
            Group = group;
            Kind = kind;
            Name = name;
            RetryAfterSeconds = retryAfterSeconds;
            Uid = uid;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets the Causes array includes more details associated with
        /// the StatusReason failure. Not all StatusReasons may provide
        /// detailed causes.
        /// </summary>
        [JsonProperty(PropertyName = "causes")]
        public IList<V1StatusCause> Causes { get; set; }

        /// <summary>
        /// Gets or sets the group attribute of the resource associated with
        /// the status StatusReason.
        /// </summary>
        [JsonProperty(PropertyName = "group")]
        public string Group { get; set; }

        /// <summary>
        /// Gets or sets the kind attribute of the resource associated with the
        /// status StatusReason. On some operations may differ from the
        /// requested resource Kind. More info:
        /// https://git.k8s.io/community/contributors/devel/sig-architecture/api-conventions.md#types-kinds
        /// </summary>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets the name attribute of the resource associated with the
        /// status StatusReason (when there is a single name which can be
        /// described).
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets if specified, the time in seconds before the operation
        /// should be retried. Some errors may indicate the client must take an
        /// alternate action - for those errors this field may indicate how
        /// long to wait before taking the alternate action.
        /// </summary>
        [JsonProperty(PropertyName = "retryAfterSeconds")]
        public int? RetryAfterSeconds { get; set; }

        /// <summary>
        /// Gets or sets UID of the resource. (when there is a single resource
        /// which can be described). More info:
        /// http://kubernetes.io/docs/user-guide/identifiers#uids
        /// </summary>
        [JsonProperty(PropertyName = "uid")]
        public string Uid { get; set; }

    }
}
