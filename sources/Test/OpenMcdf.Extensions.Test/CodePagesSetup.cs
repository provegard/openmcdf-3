using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenMcdf.Extensions.Test
{
    [TestClass]
    public sealed class CodePagesSetup
    {
        [AssemblyInitialize]
        public static void InstallCodePages(TestContext context)
        {
            // Allows the use of codepage 1252 on NET 5.
            var provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);
        }
    }
}