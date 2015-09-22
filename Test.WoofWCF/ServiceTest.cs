using System.ServiceModel;

namespace Test.WoofWCF {

    [ServiceContract]
    class ServiceTest {

        [OperationContract]
        public string Echo(string message) {
            return message;
        }
    }

}
