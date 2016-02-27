using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;

namespace TgcViewer.Utils.Networking
{
    public class TgcLanBrowser
    {
        /// <summary>
        ///     Devuelve una lista de Nombres de dominio de las PCs de la red local
        ///     que sean del tipo SV_TYPE_WORKSTATION y SV_TYPE_SERVER
        ///     Uses the DllImport : NetServerEnum with all its required parameters
        ///     (see http://msdn.microsoft.com/library/default.asp?url=/library/en-us/netmgmt/netmgmt/netserverenum.asp
        ///     for full details or method signature) to retrieve a list of domain SV_TYPE_WORKSTATION and SV_TYPE_SERVER PC's
        /// </summary>
        /// <returns>Lista de Domains encontrados</returns>
        public List<string> getNetworkComputers()
        {
            //local fields
            var networkComputers = new List<string>();
            const int MAX_PREFERRED_LENGTH = -1;
            var SV_TYPE_WORKSTATION = 1;
            var SV_TYPE_SERVER = 2;
            var buffer = IntPtr.Zero;
            var tmpBuffer = IntPtr.Zero;
            var entriesRead = 0;
            var totalEntries = 0;
            var resHandle = 0;
            var sizeofINFO = Marshal.SizeOf(typeof (SERVER_INFO_100));

            try
            {
                //call the DllImport : NetServerEnum  with all its required parameters
                //see http://msdn.microsoft.com/library/default.asp?url=/library/en-us/netmgmt/netmgmt/netserverenum.asp
                //for full details of method signature
                var ret = NetServerEnum(null, 100, ref buffer, MAX_PREFERRED_LENGTH, out entriesRead,
                    out totalEntries, SV_TYPE_WORKSTATION | SV_TYPE_SERVER, null, out resHandle);

                //if the returned with a NERR_Success
                if (ret == 0)
                {
                    //loop through all SV_TYPE_WORKSTATION and SV_TYPE_SERVER PC's
                    for (var i = 0; i < totalEntries; i++)
                    {
                        //get pointer to, Pointer to the buffer that received the data from the call to NetServerEnum.
                        //Must ensure to use correct size of STRUCTURE to ensure correct location in memory is pointed to
                        tmpBuffer = new IntPtr((int) buffer + i*sizeofINFO);

                        //Have now got a pointer to the list of SV_TYPE_WORKSTATION and SV_TYPE_SERVER PC's, which is unmanaged memory.
                        //Needs to Marshal data from an unmanaged block of memory to a managed object, again using
                        //STRUCTURE to ensure the correct data is marshalled
                        var svrInfo = (SERVER_INFO_100) Marshal.PtrToStructure(tmpBuffer, typeof (SERVER_INFO_100));

                        //add the PC names to the list
                        networkComputers.Add(svrInfo.sv100_name);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Problem with acessing network computers in NetworkBrowser.", ex);
            }
            finally
            {
                //The NetApiBufferFree function frees
                //the memory that the
                //NetApiBufferAllocate function allocates
                NetApiBufferFree(buffer);
            }

            //return entries found
            return networkComputers;
        }

        #region Dll Imports

        //declare the Netapi32 : NetServerEnum method import
        [DllImport("Netapi32", CharSet = CharSet.Auto, SetLastError = true), SuppressUnmanagedCodeSecurity]
        /// <summary>
        /// Netapi32.dll : The NetServerEnum function lists all servers
        /// of the specified type that are
        /// visible in a domain. For example, an
        /// application can call NetServerEnum
        /// to list all domain controllers only
        /// or all SQL servers only.
        /// You can combine bit masks to list
        /// several types. For example, a value
        /// of 0x00000003  combines the bit
        /// masks for SV_TYPE_WORKSTATION
        /// (0x00000001) and SV_TYPE_SERVER (0x00000002)
        /// </summary>
        public static extern int NetServerEnum(
            string ServerNane, // must be null
            int dwLevel,
            ref IntPtr pBuf,
            int dwPrefMaxLen,
            out int dwEntriesRead,
            out int dwTotalEntries,
            int dwServerType,
            string domain, // null for login domain
            out int dwResumeHandle
            );

        //declare the Netapi32 : NetApiBufferFree method import
        [DllImport("Netapi32", SetLastError = true), SuppressUnmanagedCodeSecurity]
        /// <summary>
        /// Netapi32.dll : The NetApiBufferFree function frees
        /// the memory that the NetApiBufferAllocate function allocates.
        /// Call NetApiBufferFree to free
        /// the memory that other network
        /// management functions return.
        /// </summary>
        public static extern int NetApiBufferFree(IntPtr pBuf);

        //create a _SERVER_INFO_100 STRUCTURE
        [StructLayout(LayoutKind.Sequential)]
        private struct SERVER_INFO_100
        {
            internal readonly int sv100_platform_id;

            [MarshalAs(UnmanagedType.LPWStr)] internal readonly string sv100_name;
        }

        #endregion Dll Imports
    }
}