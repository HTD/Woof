using Woof.ServiceModel;

namespace Test.WoofWCF {

    class Program {

        static int Main(string[] args) {
            return new WoofService(typeof(Program), typeof(ServiceTest), args).ReturnValue;
        }

    }

}