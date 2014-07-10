using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libairvidproto
{
    public interface IServer
    {
        string Name
        {
            get;
        }
        string Address
        {
            get;
        }

        ushort Port
        {
            get;
        }

        /// <summary>
        /// indicates if it's a manually added server.
        /// </summary>
        bool IsManual
        {
            get;
        }

        string Password
        { get; }
    }
}
