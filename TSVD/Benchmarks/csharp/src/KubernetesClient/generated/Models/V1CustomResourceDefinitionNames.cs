// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace k8s.Models
{
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// CustomResourceDefinitionNames indicates the names to serve this
    /// CustomResourceDefinition
    /// </summary>
    public partial class V1CustomResourceDefinitionNames
    {
        /// <summary>
        /// Initializes a new instance of the V1CustomResourceDefinitionNames
        /// class.
        /// </summary>
        public V1CustomResourceDefinitionNames()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1CustomResourceDefinitionNames
        /// class.
        /// </summary>
        /// <param name="kind">kind is the serialized kind of the resource. It
        /// is normally CamelCase and singular. Custom resource instances will
        /// use this value as the `kind` attribute in API calls.</param>
        /// <param name="plural">plural is the plural name of the resource to
        /// serve. The custom resources are served under
        /// `/apis/&lt;group&gt;/&lt;version&gt;/.../&lt;plural&gt;`. Must
        /// match the name of the CustomResourceDefinition (in the form
        /// `&lt;names.plural&gt;.&lt;group&gt;`). Must be all
        /// lowercase.</param>
        /// <param name="categories">categories is a list of grouped resources
        /// this custom resource belongs to (e.g. 'all'). This is published in
        /// API discovery documents, and used by clients to support invocations
        /// like `kubectl get all`.</param>
        /// <param name="listKind">listKind is the serialized kind of the list
        /// for this resource. Defaults to "`kind`List".</param>
        /// <param name="shortNames">shortNames are short names for the
        /// resource, exposed in API discovery documents, and used by clients
        /// to support invocations like `kubectl get &lt;shortname&gt;`. It
        /// must be all lowercase.</param>
        /// <param name="singular">singular is the singular name of the
        /// resource. It must be all lowercase. Defaults to lowercased
        /// `kind`.</param>
        public V1CustomResourceDefinitionNames(string kind, string plural, IList<string> categories = default(IList<string>), string listKind = default(string), IList<string> shortNames = default(IList<string>), string singular = default(string))
        {
            Categories = categories;
            Kind = kind;
            ListKind = listKind;
            Plural = plural;
            ShortNames = shortNames;
            Singular = singular;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets categories is a list of grouped resources this custom
        /// resource belongs to (e.g. 'all'). This is published in API
        /// discovery documents, and used by clients to support invocations
        /// like `kubectl get all`.
        /// </summary>
        [JsonProperty(PropertyName = "categories")]
        public IList<string> Categories { get; set; }

        /// <summary>
        /// Gets or sets kind is the serialized kind of the resource. It is
        /// normally CamelCase and singular. Custom resource instances will use
        /// this value as the `kind` attribute in API calls.
        /// </summary>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        /// <summary>
        /// Gets or sets listKind is the serialized kind of the list for this
        /// resource. Defaults to "`kind`List".
        /// </summary>
        [JsonProperty(PropertyName = "listKind")]
        public string ListKind { get; set; }

        /// <summary>
        /// Gets or sets plural is the plural name of the resource to serve.
        /// The custom resources are served under
        /// `/apis/&amp;lt;group&amp;gt;/&amp;lt;version&amp;gt;/.../&amp;lt;plural&amp;gt;`.
        /// Must match the name of the CustomResourceDefinition (in the form
        /// `&amp;lt;names.plural&amp;gt;.&amp;lt;group&amp;gt;`). Must be all
        /// lowercase.
        /// </summary>
        [JsonProperty(PropertyName = "plural")]
        public string Plural { get; set; }

        /// <summary>
        /// Gets or sets shortNames are short names for the resource, exposed
        /// in API discovery documents, and used by clients to support
        /// invocations like `kubectl get &amp;lt;shortname&amp;gt;`. It must
        /// be all lowercase.
        /// </summary>
        [JsonProperty(PropertyName = "shortNames")]
        public IList<string> ShortNames { get; set; }

        /// <summary>
        /// Gets or sets singular is the singular name of the resource. It must
        /// be all lowercase. Defaults to lowercased `kind`.
        /// </summary>
        [JsonProperty(PropertyName = "singular")]
        public string Singular { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Kind == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Kind");
            }
            if (Plural == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Plural");
            }
        }
    }
}
