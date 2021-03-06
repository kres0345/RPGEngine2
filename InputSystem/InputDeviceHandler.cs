﻿using System.Collections.Generic;
using System.Linq;

namespace RPGEngine2.InputSystem
{
    public static class InputDeviceHandler
    {
        internal static Mouse InternalMouseDevice;
        private static readonly List<IInputDevice> inputDevices = new List<IInputDevice>();

        public static void RefreshDevices()
        {
            foreach (var device in inputDevices)
            {
                device.Update();
            }

            InputAxis.Update();
        }

        public static void ActivateDevice(IInputDevice device)
        {
            if (device is null)
            {
                return;
            }

            device.Initialize();
            inputDevices.Add(device);

            if (device is Mouse mouse)
            {
                InternalMouseDevice = mouse;
            }
        }

        public static T GetDevice<T>() => inputDevices.OfType<T>().First();

        internal static IEnumerable<IAxisInput> GetAxisDevices()
        {
            foreach (IInputDevice device in inputDevices)
            {
                if (device is IAxisInput axisDevice)
                {
                    yield return axisDevice;
                }
            }
        }
    }
}
