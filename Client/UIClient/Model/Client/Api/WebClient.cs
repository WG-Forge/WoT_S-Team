using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UIClient.Model.Client.Api
{
    public class WebClientDll
    {
        const string _PathWebDll = "WebClient.dll";

        [DllImport(_PathWebDll, CallingConvention = CallingConvention.Cdecl)]
        public  extern static IntPtr create_wc();

        [DllImport(_PathWebDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result destroy_wc(IntPtr web);

        [DllImport(_PathWebDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result connect_(IntPtr web, uint addr, ushort port);

        [DllImport(_PathWebDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result detach(IntPtr web);

        [DllImport(_PathWebDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result send_packet(IntPtr web, Model.WebAction action, int size, IntPtr data, IntPtr out_size, IntPtr out_data);
    }
}
