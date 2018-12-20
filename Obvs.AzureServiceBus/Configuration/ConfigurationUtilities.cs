using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Azure.ServiceBus;

namespace Obvs.AzureServiceBus.Configuration {
    internal static class ReceiveModeTranslator {
        public static ReceiveMode TranslateReceiveModeConfigurationValueToAzureServiceBusValue(ReceiveMode messageReceiveMode) {
            ReceiveMode result;

            switch (messageReceiveMode) {
                case ReceiveMode.PeekLock:
                    result = ReceiveMode.PeekLock;

                    break;

                case ReceiveMode.ReceiveAndDelete:
                    result = ReceiveMode.ReceiveAndDelete;

                    break;

                default:
                    throw new ArgumentOutOfRangeException("configurationReceiveMode", "Unexpected ReceiveMode value specified: " + messageReceiveMode.ToString());
            }

            return result;
        }

        public static ReceiveMode TranslateAzureServiceBusReceiveModeValueToConfigurationValue(ReceiveMode azureServiceBusReceiveMode) {
            ReceiveMode result;

            switch (azureServiceBusReceiveMode) {
                case ReceiveMode.PeekLock:
                    result = ReceiveMode.PeekLock;

                    break;

                case ReceiveMode.ReceiveAndDelete:
                    result = ReceiveMode.ReceiveAndDelete;

                    break;

                default:
                    throw new ArgumentOutOfRangeException("azureServiceBusReceiveMode", "Unexpected ReceiveMode value specified: " + azureServiceBusReceiveMode.ToString());
            }

            return result;
        }
    }
}
