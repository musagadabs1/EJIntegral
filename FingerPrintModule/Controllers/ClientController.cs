using System.Linq;
using System.Net.NetworkInformation;
using System.Web.Http;

namespace FingerPrintModule.Controllers
{
    public class ClientController : ApiController
    {

        [HttpGet]
        public string Get()
        {
            var macAddr =
                (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.NetworkInterfaceType.ToString() == "Ethernet"
                select nic.GetPhysicalAddress().ToString()).FirstOrDefault();
            return macAddr;
        }
    }
}
