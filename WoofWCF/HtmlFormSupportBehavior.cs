using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace Woof.ServiceModel {

    /// <summary>
    /// HTML Form handler operation behavior
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HtmlFormHandler : Attribute, IDispatchMessageInspector, IOperationBehavior {

        public const string HtmlFormDataPropertyKey = "FormData";
        private const string HttpOperationName = "HttpOperationName";
        private const string ContentTypeHeader = "Content-Type";

        /// <summary>
        /// Contains operation to which the behavior is bound to
        /// </summary>
        public DispatchOperation Operation { get; private set; }

        /// <summary>
        /// Tests raw text data for being non-URL-encoded, returns true if the string seems legit URL-encoded
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static bool SeemsUrlEncoded(string data) {
            return !(String.IsNullOrEmpty(data) || data.StartsWith("<") || data.StartsWith("{")) && data.Contains('=');
        }

        /// <summary>
        /// Request IMessageInspector implementation, decodes message raw data as URL-encoded, passes decoded data to FormData message property
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <param name="instanceContext"></param>
        /// <returns></returns>
        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, System.ServiceModel.IClientChannel channel, System.ServiceModel.InstanceContext instanceContext) {
            if (request.IsEmpty || request.Properties[HttpOperationName].ToString() != Operation.Name) return null;
            var contentType = ((HttpRequestMessageProperty)request.Properties[HttpRequestMessageProperty.Name]).Headers[ContentTypeHeader];
            if (contentType == null) throw new WebFaultException(System.Net.HttpStatusCode.NotAcceptable);
            if (contentType != null && contentType.StartsWith(MessageContentTypes.Form)) {
                byte[] rawContent = null;
                using (var contentStream = request.GetReaderAtBodyContents()) rawContent = contentStream.ReadContentAsBase64();
                string rawTextContent = rawContent != null ? Encoding.UTF8.GetString(rawContent) : null;
                if (SeemsUrlEncoded(rawTextContent)) {
                    var formData = HttpUtility.ParseQueryString(rawTextContent);
                    request.Properties.Add(HtmlFormDataPropertyKey, formData);
                    return formData;
                }
            }
            return null;
        }

        /// <summary>
        /// Reply IMessageInspector implementation, sets text/html content type for HTML forms to emulate browser behavior
        /// </summary>
        /// <param name="reply"></param>
        /// <param name="correlationState"></param>
        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState) {
            if (correlationState is NameValueCollection) WebOperationContext.Current.OutgoingResponse.ContentType = MessageContentTypes.Html + MessageContentTypes.Charset;
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters) {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation) {
        }

        /// <summary>
        /// Binds dispatchOperation to this handler and adds IMessageInspector implementation
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <param name="dispatchOperation"></param>
        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation) {
            Operation = dispatchOperation;
            dispatchOperation.Parent.MessageInspectors.Add(this);
        }

        public void Validate(OperationDescription operationDescription) {
        }

    }

    /// <summary>
    /// Class for easy access to decoded HTML form data
    /// </summary>
    public static class HtmlFormData {

        /// <summary>
        /// Gets current HTML form data
        /// </summary>
        public static NameValueCollection Current {
            get {
                var p = OperationContext.Current.RequestContext.RequestMessage.Properties;
                var k = HtmlFormHandler.HtmlFormDataPropertyKey;
                return p.ContainsKey(k) ? (NameValueCollection)p[k] : null;
            }
        }

    }

    /// <summary>
    /// Class for creating UTF-8 encoded stream from string
    /// </summary>
    public static class RawTextOutput {

        /// <summary>
        /// Returns UTF-8 encoded byte stream from string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Stream PlainTextAsStream(string s) {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }

    }

}