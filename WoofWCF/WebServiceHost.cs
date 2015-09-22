using System;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace Woof.ServiceModel {
    
    /// <summary>
    /// Special stand-alone, zero-configuration RESTful WebService host
    /// </summary>
    public class WebServiceHost : ServiceHost {

        public WebServiceHost(Type serviceType, Uri endpointUri) : base(serviceType, new[] { endpointUri }) { }

        /// <summary>
        /// Configures service endpoint and adds special behaviors to it (WebHttp and CorsSupport)
        /// </summary>
        protected override void OnOpening() {
            var securityMode = BaseAddresses.FirstOrDefault().Scheme == "https" ? WebHttpSecurityMode.Transport : WebHttpSecurityMode.None;
            var receiveTimeout = new TimeSpan(1, 0, 0);
            var defaultWebHttpBehavior = new WebHttpBehavior() {
                AutomaticFormatSelectionEnabled = true,
                DefaultBodyStyle = WebMessageBodyStyle.Wrapped,
                DefaultOutgoingRequestFormat = WebMessageFormat.Json,
                HelpEnabled = false
            };
            var endPoint = new ServiceEndpoint(
                ContractDescription.GetContract(Description.ServiceType),
                new WebHttpBinding(securityMode) {
                    UseDefaultWebProxy = false,
                    ReceiveTimeout = receiveTimeout,
                    MaxBufferSize = Config.MaxBufferSize,
                    MaxReceivedMessageSize = Config.MaxReceivedMessageSize
                },
                new EndpointAddress(BaseAddresses.FirstOrDefault())
            );
            endPoint.EndpointBehaviors.Add(defaultWebHttpBehavior);
            endPoint.EndpointBehaviors.Add(new CorsSupportBehavior());
            endPoint.EndpointBehaviors.Add(new BrowserCompatibilityBehavior());
            AddServiceEndpoint(endPoint);
            base.OnOpening();
        }

    }

}