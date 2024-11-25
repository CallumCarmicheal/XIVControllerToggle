using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XIVControllerToggle {
    internal unsafe class XIVFunctions {

        internal static AtkValue? HideChatLog() {
            var agent = FFXIVClientStructs.FFXIV.Client.UI.Agent.AgentChatLog.Instance();
            if (agent != null) {
                var returnValue = stackalloc AtkValue[1];
                agent->ReceiveEvent(returnValue, null, 0, 8);
                return *returnValue;
            }
            return null;
        }
    }
}
