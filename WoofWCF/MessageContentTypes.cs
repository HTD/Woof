using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;

namespace Woof.ServiceModel {

    public class MessageContentTypes {

        public const string Json = "application/json";
        public const string Xml = "application/xml";
        public const string Text = "text/plain";
        public const string Html = "text/html";
        public const string Form = "application/x-www-form-urlencoded";
        public const string Charset = "; charset=UTF-8";

        private string Fallback;

        public string Accepts { get; set; }

        public MessageContentTypes(Message request, string fallback = null) {
            var httpRequest = request.Properties[HttpRequestMessageProperty.Name] as HttpRequestMessageProperty;
            if (fallback != null) {
                var p = fallback.IndexOf(';');
                Fallback = p > 0 ? fallback.Substring(0, p) : fallback;
            }
            Accepts = httpRequest.Headers["Accept"];
            RequestContentType = httpRequest.Headers["Content-Type"];
            AcceptsAnything = String.IsNullOrEmpty(Accepts) || Accepts.StartsWith("*");
            if (AcceptsAnything && RequestContentType == null) { // IE LT 10 case - we know nothing about the request
                if (RequestContentType == null) {
                    if (fallback != null) {
                        if (fallback.StartsWith(Json)) { AcceptsJson = true; AcceptsAnything = false; AcceptsText = false; } else if (fallback.StartsWith(Xml)) { AcceptsXml = true; AcceptsAnything = false; AcceptsText = false; }
                    }
                }
            } else
                if (!AcceptsAnything) {
                    if (Accepts.StartsWith(Json)) AcceptsJson = true;
                    else if (Accepts.StartsWith(Xml)) AcceptsXml = true;
                    else if (Accepts.StartsWith(Html)) AcceptsHtml = true;
                    else if (Accepts.StartsWith(Text)) AcceptsText = true;
                    else if (RequestContentType != null && RequestContentType.StartsWith(Form)) RequestIsForm = true;
                }
        }

        public bool AcceptsAnything { get; private set; }

        public bool AcceptsText { get; private set; }

        public bool AcceptsHtml { get; private set; }

        public bool AcceptsJson { get; private set; }

        public bool AcceptsXml { get; private set; }

        public bool RequestIsForm { get; private set; }

        public string RequestContentType { get; private set; }

        public string ResponseContentType {
            get {
                if (RequestIsForm) return Html + Charset;
                if (AcceptsJson || (AcceptsHtml && Fallback.StartsWith(Json))) return Json + Charset;
                if (AcceptsXml || (AcceptsHtml && Fallback.StartsWith(Xml))) return Xml + Charset;
                if (Fallback != null) return Fallback + Charset;
                return Text + Charset;
            }
        }

        public WebMessageFormat? Format {
            get {
                if (AcceptsJson || (AcceptsHtml && Fallback.StartsWith(Json))) return WebMessageFormat.Json;
                if (AcceptsXml || (AcceptsHtml && Fallback.StartsWith(Xml))) return WebMessageFormat.Xml;
                return null;
            }
        }


    }

}