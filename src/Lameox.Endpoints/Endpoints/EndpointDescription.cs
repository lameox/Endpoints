namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal Type EndpointType { get; }
        internal bool ImplementsHandleAsync { get; }
        internal bool ImplementsGetResponseAsync { get; }

        /// <summary>
        /// Returns true if either both <see cref="ImplementsHandleAsync"/> and <see cref="ImplementsGetResponseAsync"/> are 
        /// <see langword="true"/> or both are <see langword="false"/>, since the user should override exacly one of the functions for
        /// a given endpoint.
        /// </summary>
        internal bool HasFaultyOverrides => ImplementsHandleAsync ^ ImplementsGetResponseAsync;

        public string Pattern { get; }
        public HttpVerb Verbs { get; }


        private EndpointDescription(
            Type endpointType,
            string pattern,
            HttpVerb verbs,
            bool implementsHandleAsync,
            bool implementsGetResponseAsync)
        {
            EndpointType = endpointType;
            Pattern = pattern;
            Verbs = verbs;
            ImplementsHandleAsync = implementsHandleAsync;
            ImplementsGetResponseAsync = implementsGetResponseAsync;
        }
    }
}